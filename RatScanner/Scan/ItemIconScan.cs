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
		private Mat _highlight;
		private readonly List<(int, int)> _searchOrder = new List<(int, int)> { (-1, 0), (1, 0), (0, -1), (0, 1) };

		private const double GridSearchAvgScoreStop = 0.9;
		private const double GridSearchEarlyMinScore = 0.25;
		private const double GridDistanceScalingRate = 0.95;
		private const int GridSearchMaxIterations = 30;
		private const int GridSearchMinIterations = 6;
		private const int GridSearchEarlySearchIterations = 2;
		private const double GridSearchPerfectScore = 0.95;

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
			if (RatConfig.ScreenResolution != RatConfig.Resolution.R1920x1080)
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

			IconRect.X = MousePos.X - RatConfig.IconScan.ScanWidth / 2 + iconPosition.X + matchResult.pos.X;
			IconRect.Y = MousePos.Y - RatConfig.IconScan.ScanHeight / 2 + iconPosition.Y + matchResult.pos.Y;

			Confidence = matchResult.conf;

			var itemInfos = IconManager.GetItemInfo(matchResult.iconKey);
			if (!(itemInfos?.Length > 0)) return;

			MatchedItems = Array.ConvertAll(itemInfos, itemInfo => itemInfo.GetItem());
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
						hmConf = (float) maxVal;
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
			_highlight = _grid.Clone();
			Logger.LogDebugMat(_grid, "in_range");
			var mask = mat.Canny(100, 100);
			Logger.LogDebugMat(mask, "canny_edge");
			mask = mask.Dilate(Mat.Ones(10, 3, MatType.CV_8UC1));
			Logger.LogDebugMat(mask, "canny_edge_dilate");
			Cv2.BitwiseAnd(_grid, mask, _grid);
			Logger.LogDebugMat(_grid, "in_range_and_canny_edge_dilate");

			var (lBorderDist, rBorderDist, tBorderDist, bBorderDist, center) =
				FindGridEdgeDistances(new Vector2(_grid.Width / 2 - 1, _grid.Height / 2 - 1));

			var positionX = center.X - lBorderDist;
			var positionY = center.Y - bBorderDist;
			var position = new Vector2(positionX, positionY);

			var size = new Vector2(rBorderDist + lBorderDist, tBorderDist + bBorderDist);
			return (position, size);
		}

		private (int, int, int, int, Vector2) FindGridEdgeDistances(Vector2 center, int numIters = 0)
		{
			// Set up loop values
			var orderIdx = 0;
			var leftLimit = RatConfig.IconScan.ItemSlotSize / 2; // Initial edge searches will search 1 grid in each direction
			var rightLimit = leftLimit;
			var upLimit = leftLimit;
			var downLimit = leftLimit;
			var lastScores = new[] { 0.0, 0.0, 0.0, 0.0 };
			var foundNewEdge = new[] { false, false, false, false };
			var myIters = 0;
			var keepTrying = true;

			while (keepTrying)
			{
				// Get the next direction to search
				var direction = _searchOrder[orderIdx];

				// Find the edge limits to use on this search based on direction
				var plus = direction.Item1 == 0 ? upLimit : rightLimit;
				var minus = direction.Item1 == 0 ? downLimit : leftLimit;

				Logger.LogDebug($"Searching {direction.Item1}, {direction.Item2};");
				Logger.LogDebug($"Previous score: {lastScores[orderIdx]}; Current Limits: {plus}, {minus};");

				// Find the distance to edge and that edge's score
				// If we are in the first 2 loops, we are finding an initial bound so we lower the required score and we just search for the
				// first bound rather than the best bound
				var minScore = numIters < GridSearchMinIterations ? GridSearchEarlyMinScore : lastScores[orderIdx];
				var (step, score) = FindGridEdgeDistance(direction.Item1, direction.Item2, center.X, center.Y,
					GetSteps(direction.Item1, direction.Item2, center),
					plus, minus, myIters >= GridSearchEarlySearchIterations, minScore);

				// Update scores and limits based on results
				lastScores[orderIdx] = score;

				foundNewEdge[orderIdx] = false;
				if (score > 0.0)
				{
					switch (direction.Item1, direction.Item2)
					{
						case var d when d == (0, 1):
							foundNewEdge[orderIdx] = rightLimit != step;
							rightLimit = step;
							break;
						case var d when d == (0, -1):
							foundNewEdge[orderIdx] = leftLimit != step;
							leftLimit = step;
							break;
						case var d when d == (1, 0):
							foundNewEdge[orderIdx] = upLimit != step;
							upLimit = step;
							break;
						case var d when d == (-1, 0):
							foundNewEdge[orderIdx] = downLimit != step;
							downLimit = step;
							break;
					}
				}

				// Check if horizontal or vertical limits have squashed to 0
				// If this has happened, the center point is on an edge, so we need to search
				// to find the direction of the highlighted item, and reposition the center slightly
				// towards the center of the highlighted item then call ourselves to finish from thew new starting point
				if (leftLimit == 0 && rightLimit == 0)
				{
					var itemSlotSize = RatConfig.IconScan.ItemSlotSize;
					var newX = FindDirection(center, true) ? center.X + itemSlotSize / 2 : center.X - itemSlotSize / 2;

					var newCenter = new Vector2(newX, center.Y);
					Logger.LogDebug($"On Vertical Edge... Old Center: {center.X}, {center.Y}; New Center: {newCenter.X}, {newCenter.Y};");
					return FindGridEdgeDistances(newCenter, numIters);
				}

				if (upLimit == 0 && downLimit == 0)
				{
					var newY = FindDirection(center, false)
						? center.Y + RatConfig.IconScan.ItemSlotSize / 2
						: center.Y - RatConfig.IconScan.ItemSlotSize / 2;

					var newCenter = new Vector2(center.X, newY);
					Logger.LogDebug($"On Horizontal Edge... Old Center: {center.X}, {center.Y}; New Center: {newCenter.X}, {newCenter.Y};");
					return FindGridEdgeDistances(newCenter, numIters);
				}

				// Log the current bounds if logging debug
				if (RatConfig.LogDebug)
				{
					var positionX = center.X - leftLimit;
					var positionY = center.Y - downLimit;
					var position = new Vector2(positionX, positionY);
					var size = new Vector2(leftLimit + rightLimit, upLimit + downLimit);
					var rect = new Rect(position, size);
					var print = _grid.Clone(rect);
					Logger.LogDebugMat(print, $"search_{center.X}-{center.Y}_{numIters}_");
				}

				// Loop housekeeping
				numIters++;
				myIters++;
				orderIdx = (orderIdx + 1) % _searchOrder.Count;

				var avgScore = lastScores.Sum() / lastScores.Length;
				keepTrying = avgScore < GridSearchAvgScoreStop;
				if (numIters <= GridSearchMinIterations)
				{
					keepTrying = true;
				}
				// If we haven't found suitable limits in x iterations give up
				else if (numIters > GridSearchMaxIterations)
				{
					Logger.LogDebug("Hit maximum number of iterations...");
					keepTrying = false;
				}
				else if (foundNewEdge.All(s => !s))
				{
					Logger.LogDebug("Did a full loop without finding new edges...");
					keepTrying = false;
				}
			}

			return (leftLimit, rightLimit, upLimit, downLimit, center);
		}

		private (int, double) FindGridEdgeDistance(int up, int right, int x, int y, int steps, int plus, int minus, bool findBest = false, double minScore = 0.6)
		{
			var indexer = _grid.GetGenericIndexer<byte>();

			var bestScore = 0.0;
			var best = 0;
			for (var i = 0; i < steps; i++)
			{
				if (indexer[y, x].Equals(borderColor))
				{
					// Search along the edge inside the given plus, minus bounds and get what percentage of the pixels in that range were white
					var horizontalEdge = up != 0;
					var score = FindGridEdge(x, y, plus, minus, horizontalEdge);

					var numGrids = i / Math.Floor(RatConfig.IconScan.ItemSlotSize / 2.0);
					var scale = Math.Pow(GridDistanceScalingRate, numGrids);
					score *= scale;

					if (score > bestScore)
					{
						bestScore = score;
						best = i;
						Logger.LogDebug($"New Best Edge confirmed {i} with score {score};");
					}

					if (score > minScore && !findBest || score > GridSearchPerfectScore)
					{
						Logger.LogDebug($"Found Edge! Up: {up}; Right: {right}; Steps: {steps}; Plus: {plus}; Minus: {minus}; Dist: {i}; Score: {score}");
						return (i, score);
					}

					if (1 * scale < minScore)
					{
						Logger.LogDebug("Drifted too far to find new best edge.");
						break;
					}
				}

				x += right;
				y += up;
			}


			if (bestScore > minScore)
			{
				Logger.LogDebug($"Found Best Edge! Up: {up}; Right: {right}; Steps: {steps}; Plus: {plus}; Minus: {minus}; Dist: {best}; Score: {bestScore}");
				return (best, bestScore);
			}

			Logger.LogDebug($"Failed to find Edge! Up: {up}; Right: {right}; Steps: {steps}; Plus: {plus}; Minus: {minus}; Dist: {best}; Score: {bestScore}");

			return (0, -1.0);
		}

		/// <summary>
		/// Find the distance to the next grid edge
		/// </summary>
		/// <param name="x">Origin X</param>
		/// <param name="y">Origin Y</param>
		/// <param name="plus">Positive max offset from origin</param>
		/// <param name="minus">Negative max offset from origin</param>
		/// <param name="horizontal"><see langword="true"/> if searching for a grid edge horizontally</param>
		/// <returns>Distance to the next grid edge</returns>
		private double FindGridEdge(int x, int y, int plus, int minus, bool horizontal)
		{
			var indexer = _grid.GetGenericIndexer<byte>();

			var start = horizontal ? x : y;
			var plusEnd = start + plus;
			var minusEnd = start - minus;

			plusEnd = Math.Min(plusEnd, horizontal ? _grid.Height : _grid.Width);
			minusEnd = Math.Max(minusEnd, 0);

			var total = 0;
			var hits = 0;
			for (var i = start + 1; i < plusEnd; i++)
			{
				total++;
				var pixel = horizontal ? indexer[y, i] : indexer[i, x];
				if (pixel.Equals(borderColor)) hits++;
			}

			for (var i = start; i > minusEnd; i--)
			{
				total++;
				var pixel = horizontal ? indexer[y, i] : indexer[i, x];
				if (pixel.Equals(borderColor)) hits++;
			}

			if (total == 0) return -1.0;

			return hits / (double)total;
		}

		private bool FindDirection(Vector2 center, bool horizontal)
		{
			var upHits = 0;
			var downHits = 0;
			var indexer = _highlight.GetGenericIndexer<byte>();
			for (var i = 0; i < RatConfig.IconScan.ItemSlotSize * 2; i++)
			{
				if (horizontal)
				{
					upHits += indexer[center.Y, center.X + i].Equals(borderColor) ? 1 : 0;
					downHits += indexer[center.Y, center.X - i].Equals(borderColor) ? 1 : 0;
				}
				else
				{
					upHits += indexer[center.Y + i, center.X].Equals(borderColor) ? 1 : 0;
					downHits += indexer[center.Y - i, center.X].Equals(borderColor) ? 1 : 0;
				}
			}

			return upHits > downHits;
		}

		private int GetSteps(int up, int right, Vector2 center)
		{
			switch (up, right)
			{
				case var d when d == (0, 1):
					return _grid.Width - center.X;
				case var d when d == (0, -1):
					return center.X;
				case var d when d == (1, 0):
					return _grid.Height - center.Y;
				case var d when d == (-1, 0):
					return center.Y;
			}

			return 0;
		}

		internal override Vector2 GetToolTipPosition()
		{
			var pos = new Vector2(IconRect.X, IconRect.Y);
			return pos;
		}
	}
}
