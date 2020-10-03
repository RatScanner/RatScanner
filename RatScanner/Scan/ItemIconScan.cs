using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Size = OpenCvSharp.Size;

namespace RatScanner.Scan
{
	/*
	 * Ideas to improve performance:
	 *
	 * - Detect item background color and against equals
	 * - Cache item masks
	 */

	internal class ItemIconScan : ItemScan
	{
		internal Vector2 MousePos;
		internal Rect IconRect;
		internal bool Rotated;

		private static readonly byte borderColor = 0xFF;
		private Mat _grid;

		private readonly object _matchLock = new object();

		internal static void Init()
		{
			IconManager.Init();
		}

		internal ItemIconScan(Bitmap capture, Vector2 mouseVector2)
		{
			Logger.ClearDebugMats();

			Capture = capture;
			MousePos = mouseVector2;

			var (iconPosition, iconSize) = LocateIcon(Capture);

			var padding = RatConfig.IconScan.ScanPadding;

			// Add padding
			var iconSlotSize = new Size();
			iconSlotSize.Width = IconManager.PixelsToSlots(iconSize.X);
			iconSlotSize.Height = IconManager.PixelsToSlots(iconSize.Y);

			// Rectify icon position for padding
			iconPosition -= new Vector2(padding / 2, padding / 2);
			iconSize.X += padding;
			iconSize.Y += padding;

			if (iconPosition.X < 0) iconPosition.X = 0;
			if (iconPosition.Y < 0) iconPosition.Y = 0;
			if (iconSize.X + iconPosition.X > Capture.Width) iconSize.X = Capture.Width - iconPosition.X;
			if (iconSize.Y + iconPosition.Y > Capture.Height) iconSize.Y = Capture.Height - iconPosition.Y;

			if (iconSlotSize.Width == 0 || iconSlotSize.Height == 0) return;

			Logger.LogDebug("pX: " + iconPosition.X + " | pY: " + iconPosition.Y);
			Logger.LogDebug("pW: " + iconSize.X + " | pH: " + iconSize.Y);
			Logger.LogDebug("sW: " + iconSlotSize.Width + " | sH: " + iconSlotSize.Height);

			// Crop capture to icon
			var sourceBitmap = new Bitmap(Capture).Clone(new Rectangle(iconPosition, iconSize), Capture.PixelFormat);
			var croppedIcon = sourceBitmap.ToMat();
			Logger.LogDebugMat(croppedIcon, "cropped_icon");

			// Rescale captured icon if resolution is not FHD
			if (RatConfig.ScreenResolution != RatConfig.Resolution.FHD)
			{
				var invSSF = RatConfig.GetInverseScreenScaleFactor();
				var croppedSize = new Size(croppedIcon.Width * invSSF, croppedIcon.Height * invSSF);
				croppedIcon = croppedIcon.Resize(croppedSize, 0, 0, InterpolationFlags.Area);
				Logger.LogDebugMat(croppedIcon, "resized_cropped_icon");
			}

			var iconWidthPixel = IconManager.SlotsToPixels(iconSlotSize.Width);
			var iconHeightPixel = IconManager.SlotsToPixels(iconSlotSize.Height);
			IconRect.Size = new Size(iconWidthPixel, iconHeightPixel);

			// Actually scan the source 
			var matchResult = ScanStaticAndDynamic(iconSlotSize, croppedIcon);

			if (RatConfig.IconScan.ScanRotatedIcons)
			{
				// Rotate source
				var croppedIconRotated = new Mat();
				Cv2.Rotate(croppedIcon, croppedIconRotated, RotateFlags.Rotate90Counterclockwise);
				var iconSlotSizeRotated = new Size(iconSlotSize.Height, iconSlotSize.Width);

				// Actually scan the source
				var matchResultRotated = ScanStaticAndDynamic(iconSlotSizeRotated, croppedIconRotated);

				if (matchResultRotated.conf > matchResult.conf)
				{
					matchResult = matchResultRotated;

					// Rotate match position when matched on rotated source
					var xPos = matchResult.pos.X;
					matchResult.pos.X = matchResult.pos.Y;
					matchResult.pos.Y = xPos;
					Rotated = true;
				}
			}


			Logger.LogDebug("Icon Key: " + matchResult.iconKey);
			Logger.LogDebug("Conf: " + matchResult.conf);

			IconRect.X = MousePos.X - (RatConfig.IconScan.ScanWidth / 2) + iconPosition.X + matchResult.pos.X;
			IconRect.Y = MousePos.Y - (RatConfig.IconScan.ScanHeight / 2) + iconPosition.Y + matchResult.pos.Y;

			Confidence = matchResult.conf;

			var itemInfos = IconManager.GetItemInfo(matchResult.iconKey);
			if (!(itemInfos?.Length > 0)) return;

			MatchedItems = Array.ConvertAll(itemInfos, itemInfo => itemInfo.GetMarketItem());
			if (MatchedItems?.Length > 0) MatchedItems = MatchedItems.Where(item => item != null).ToArray();
			if (MatchedItems?.Length > 0) ValidItem = true;
		}

		private (string iconKey, float conf, Vector2 pos) ScanStaticAndDynamic(Size iconSlotSize, Mat source)
		{
			var matchResultStatic = MatchIcon(iconSlotSize, source, true);
			var matchResultDynamic = MatchIcon(iconSlotSize, source, false);
			return matchResultStatic.conf > matchResultDynamic.conf ? matchResultStatic : matchResultDynamic;
		}

		private (string iconKey, float conf, Vector2 pos) MatchIcon(Size iconSlotSize, Mat source, bool useStaticIcons)
		{
			(string iconKey, float conf, Vector2 pos) matchResult = (null, 0f, Vector2.Zero());

			Dictionary<string, Mat> iconTemplates;
			if (useStaticIcons) iconTemplates = IconManager.GetStaticIcons(iconSlotSize);
			else iconTemplates = IconManager.GetDynamicIcons(iconSlotSize);
			if (iconTemplates == null) return matchResult;

			var firstIcon = iconTemplates.First().Value;
			if (source.Width < firstIcon.Width || source.Height < firstIcon.Height)
			{
				var infoText = "\nsW: " + source.Width + " | sH: " + source.Height;
				infoText += "\ntW: " + firstIcon.Width + " | tH: " + firstIcon.Height;
				Logger.LogWarning("Source dimensions smaller than template dimensions!" + infoText);
				return matchResult;
			}

			// hm = highest match
			var hmConf = 0f;
			var hmKey = "";
			var hmPos = Vector2.Zero();

			Parallel.ForEach(iconTemplates, icon =>
			{
				// TODO Prepare masks when loading icons
				var mask = icon.Value.InRange(new Scalar(0, 0, 0, 128), new Scalar(255, 255, 255, 255));
				mask = mask.CvtColor(ColorConversionCodes.GRAY2BGR, 3);
				var iconNoAlpha = icon.Value.CvtColor(ColorConversionCodes.BGRA2BGR, 3);

				var matches = source.MatchTemplate(iconNoAlpha, TemplateMatchModes.CCorrNormed, mask);
				matches.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);

				lock (_matchLock)
				{
					if (maxVal > hmConf)
					{
						hmConf = (float)maxVal;
						hmKey = icon.Key;
						hmPos = new Vector2(maxLoc);
					}
				}
			});

			return (hmKey, hmConf, hmPos);
		}

		/// <summary>
		/// Finds the position and size of the center most item
		/// </summary>
		/// <param name="image">The image which includes the to be located icon</param>
		/// <returns>A tuple containing position and size</returns>
		private (Vector2 position, Vector2 size) LocateIcon(Bitmap image)
		{
			var mat = image.ToMat();
			Logger.LogDebugMat(mat, "capture");
			_grid = mat.InRange(new Scalar(84, 81, 73), new Scalar(104, 101, 93));
			Logger.LogDebugMat(_grid, "in_range");
			var mask = mat.Canny(100, 100);
			Logger.LogDebugMat(mask, "canny_edge");
			mask = mask.Dilate(Mat.Ones(10, 3, MatType.CV_8UC1));
			Logger.LogDebugMat(mask, "canny_edge_dilate");
			Cv2.BitwiseAnd(_grid, mask, _grid);
			Logger.LogDebugMat(_grid, "in_range_and_canny_edge_dilate");

			var rightBorderDist = FindGridEdgeDistance(0, 1);
			var leftBorderDist = FindGridEdgeDistance(0, -1);
			var topBorderDist = FindGridEdgeDistance(-1, 0);
			var bottomBorderDist = FindGridEdgeDistance(1, 0);

			var positionX = (RatConfig.IconScan.ScanWidth / 2) - leftBorderDist;
			var positionY = (RatConfig.IconScan.ScanHeight / 2) - topBorderDist;
			var position = new Vector2(positionX, positionY);

			var size = new Vector2(rightBorderDist + leftBorderDist, topBorderDist + bottomBorderDist);
			return (position, size);
		}

		private int FindGridEdgeDistance(int up, int right)
		{
			var steps = 0;
			var localMousePos = new Vector2(_grid.Width / 2 - 1, _grid.Height / 2 - 1);

			switch (up, right)
			{
				case var d when d == (0, 1):
					steps = _grid.Width - localMousePos.X;
					break;
				case var d when d == (0, -1):
					steps = localMousePos.X;
					break;
				case var d when d == (1, 0):
					steps = _grid.Height - localMousePos.Y;
					break;
				case var d when d == (-1, 0):
					steps = localMousePos.Y;
					break;
			}

			var x = localMousePos.X;
			var y = localMousePos.Y;

			var indexer = _grid.GetGenericIndexer<byte>();
			for (var i = 0; i < steps; i++)
			{
				if (indexer[y, x].Equals(borderColor))
				{
					Logger.LogDebug("Potential Edge at: " + x + " | " + y);
					var horizontalEdge = up != 0;
					if (IsGridEdge(x, y, horizontalEdge))
					{
						Logger.LogDebug("Confirmed as Edge: " + i);
						return i;
					}
				}

				x += right;
				y += up;
			}

			Logger.LogDebug("Up: " + up + " | Right: " + right + " | Steps: " + steps);
			return 0;
		}

		private bool IsGridEdge(int x, int y, bool horizontal, int minSize = 60)
		{
			var size = 0;

			var indexer = _grid.GetGenericIndexer<byte>();

			var start = horizontal ? x : y;
			var end = horizontal ? _grid.Height : _grid.Width;

			// Use (start + 1) to avoid counting the origin pixel twice
			for (var i = start + 1; i < end; i++)
			{
				if (horizontal)
				{
					if (!indexer[y, i].Equals(borderColor)) break;
				}
				else
				{
					if (!indexer[i, x].Equals(borderColor)) break;
				}

				size += 1;
			}

			for (var i = start; i > 0; i--)
			{
				if (horizontal)
				{
					if (!indexer[y, i].Equals(borderColor)) break;
				}
				else
				{
					if (!indexer[i, x].Equals(borderColor)) break;
				}

				size += 1;
			}

			Logger.LogDebug("Size: " + size);
			return size > minSize;
		}

		internal override Vector2 GetToolTipPosition()
		{
			var pos = new Vector2(IconRect.X, IconRect.Y);
			return pos;
		}
	}
}
