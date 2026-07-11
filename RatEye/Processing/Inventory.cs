using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace RatEye.Processing
{
	/// <summary>
	/// Represents a grid style inventory which can contain multiple <see cref="RatEye.Processing.Icon"/>
	/// </summary>
	public class Inventory
	{
		private readonly Config _config;
		private readonly Mat _image;
		private Mat _grid;
		private Mat _vertGrid;
		private List<Rect> _boundingBoxes = new();
		private List<Icon> _icons = new();

		private Config.Processing ProcessingConfig => _config.ProcessingConfig;

		/// <summary>
		/// All icons, contained inside the inventory
		/// </summary>
		public IEnumerable<Icon> Icons
		{
			get
			{
				SatisfyState(State.GridParsed);
				return _icons;
			}
		}

		/// <summary>
		/// Constructor for inventory view processing object
		/// </summary>
		/// <param name="image">Image of the inventory which will be processed</param>
		/// <param name="config">The config to use for this instance></param>
		internal Inventory(System.Drawing.Bitmap image, Config config)
		{
			_config = config;
			_image = image.ToMat();
		}

		private enum State
		{
			Default,
			GridDetected,
			GridParsed,
		}

		private State _currentState = State.Default;

		private void SatisfyState(State targetState)
		{
			while (_currentState < targetState)
			{
				switch (_currentState + 1)
				{
					case State.Default:
						break;
					case State.GridDetected:
						DetectInventoryGrid();
						break;
					case State.GridParsed:
						ParseInventoryGrid();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		private void DetectInventoryGrid()
		{
			if (_config.ProcessingConfig.InventoryConfig.OptimizeHighlighted) DetectInventoryGridHighlighted();
			else DetectInventoryGridNormal();
		}

		private void DetectInventoryGridHighlighted()
		{
			var minBackgroundColor = _config.ProcessingConfig.InventoryConfig.MinHighlightingColor;
			var maxBackgroundColor = _config.ProcessingConfig.InventoryConfig.MaxHighlightingColor;
			var minBackgroundScalar = new Scalar(minBackgroundColor.hue, minBackgroundColor.saturation, minBackgroundColor.value);
			var maxBackgroundScalar = new Scalar(maxBackgroundColor.hue, maxBackgroundColor.saturation, maxBackgroundColor.value);
			using var hsv = _image.CvtColor(ColorConversionCodes.BGR2HSV_FULL);
			var colorFilter = hsv.InRange(minBackgroundScalar, maxBackgroundScalar);
			Logger.LogDebugMat(colorFilter, "inventory/colorFilter");

			var scale = _config.ProcessingConfig.Scale;

			using var hStructure = Mat.Ones(MatType.CV_8U, new[] { 1, (int)(2 * scale) }).ToMat();
			using var vStructure = Mat.Ones(MatType.CV_8U, new[] { (int)(2 * scale), 1 }).ToMat();

			Cv2.Dilate(colorFilter, colorFilter, hStructure, null, 1);
			Cv2.Dilate(colorFilter, colorFilter, vStructure, null, 1);
			Logger.LogDebugMat(colorFilter, "inventory/colorFilterPostDilate");

			_grid = colorFilter;
		}

		private void DetectInventoryGridNormal()
		{
			var minGridColor = _config.ProcessingConfig.InventoryConfig.MinGridColor;
			var maxGridColor = _config.ProcessingConfig.InventoryConfig.MaxGridColor;
			var minGridScalar = new Scalar(minGridColor.hue, minGridColor.saturation, minGridColor.value);
			var maxGridScalar = new Scalar(maxGridColor.hue, maxGridColor.saturation, maxGridColor.value);
			using var hsv = _image.CvtColor(ColorConversionCodes.BGR2HSV_FULL);
			using var colorFilter = hsv.InRange(minGridScalar, maxGridScalar);


			Logger.LogDebugMat(colorFilter, "inventory/colorFilter");

			// Extract vertical and horizontal lines
			var scaledSlotSize = (int)ProcessingConfig.ScaledSlotSize;
			var size = scaledSlotSize - (scaledSlotSize % 2) + 1;
			using var lineStructure = Mat.Ones(MatType.CV_8U, new[] { size, 1 });
			var verticalLines = colorFilter.Erode(lineStructure);
			Cv2.Dilate(verticalLines, verticalLines, lineStructure);
			using var horizontalLines = colorFilter.Erode(lineStructure.T());
			Cv2.Dilate(horizontalLines, horizontalLines, lineStructure.T());

			SmoothJaggedLines(verticalLines, false);
			SmoothJaggedLines(horizontalLines, true);

			var grid = CombineWithoutPeeks(verticalLines, horizontalLines);
			Cv2.Dilate(grid, grid, Mat.Ones(2, 2));
			Cv2.Erode(grid, grid, Mat.Ones(2, 2));

			_grid = grid;
			_vertGrid = verticalLines;
		}

		private void ParseInventoryGrid()
		{
			if (_config.ProcessingConfig.InventoryConfig.OptimizeHighlighted) ParseInventoryGridHighlighted();
			else ParseInventoryGridNormal();
		}

		private void ParseInventoryGridHighlighted()
		{
			var contours = _grid.FindContoursAsArray(RetrievalModes.External, ContourApproximationModes.ApproxSimple);

			using var debugMat = _image.Clone();
			if (Config.LogDebug) Cv2.DrawContours(debugMat, contours, -1, new(255, 0, 0));

			var boundingBoxes = contours.Select(Cv2.BoundingRect).ToList();
			if (Config.LogDebug) boundingBoxes.ForEach(b => debugMat.Rectangle(b, new(0, 255, 0)));

			// Merge overlapping bounding boxes
			for (var b = 0; b < boundingBoxes.Count; b++)
			{
				for (var i = b + 1; i < boundingBoxes.Count; i++)
				{
					if (!boundingBoxes[b].IntersectsWith(boundingBoxes[i])) continue;
					boundingBoxes[b] = boundingBoxes[b].Union(boundingBoxes[i]);
					boundingBoxes.RemoveAt(i);
					i = b;
				}
			}

			if (Config.LogDebug) boundingBoxes.ForEach(b => debugMat.Rectangle(b, new(0, 0, 255)));
			Logger.LogDebugMat(debugMat, "inventory/boundingBoxes");

			_boundingBoxes = boundingBoxes;
		}

		/// <summary>
		/// Scan along multiple spaced apart rows of the processed input image
		/// If the pixel under the scan position is white, check if there is
		/// a valid icon at that position and add it as done by the
		/// <see cref="TryAddIcon"/> method.
		/// At last, remove detected icons which are overlapping.
		/// </summary>
		private void ParseInventoryGridNormal()
		{
			var gridIndexer = _grid.GetGenericIndexer<byte>();
			var vertGrindIndexer = _vertGrid.GetGenericIndexer<byte>();

			var scaledSlotSize = (int)(_config.ProcessingConfig.ScaledSlotSize);

			var maxRows = _vertGrid.Rows;
			var maxCols = _vertGrid.Cols - scaledSlotSize / 2;

			for (var y = scaledSlotSize / 2; y < maxRows; y += scaledSlotSize)
			{
				for (var x = 0; x < maxCols; x++)
				{
					if (!vertGrindIndexer[y, x].Equals(0xFF)) continue;

					TryAddIcon(gridIndexer, x, y);
				}
			}

			// Quarter of the normal sized slot
			var overlapThreshold = scaledSlotSize / 2;

			for (var i = _icons.Count - 1; i >= 0; i--)
			{
				var iconA = _icons[i];
				var iconARect = new Rect(iconA.Position, iconA.Size);

				for (var j = _icons.Count - 1; j >= 0; j--)
				{
					// Skip if comparing to itself
					if (i == j) continue;

					var iconB = _icons[j];

					// Only remove icon A from the list if it is bigger then the compare to icon
					if (iconA.Size.Area < iconB.Size.Area) continue;

					var iconBRect = new Rect(iconB.Position, iconB.Size);

					var overlapRect = iconARect.Intersect(iconBRect);

					// Remove icon A from the icons and continue to the next one
					if (overlapRect.Width > overlapThreshold && overlapRect.Height > overlapThreshold)
					{
						_icons.RemoveAt(i);
						break;
					}
				}
			}
		}

		private void SmoothJaggedLines(Mat mat, bool horizontal)
		{
			using var extendedLines = new Mat();
			using var thickenedLines = new Mat();

			var size = (int)(ProcessingConfig.ScaledSlotSize * 2);
			size = size - (size % 2) + 1;
			var extendStructureSize = horizontal ? new[] { 1, size } : new[] { size, 1 };
			using var extendStructure = Mat.Ones(MatType.CV_8U, extendStructureSize).ToMat();

			var thickenStructureSize = horizontal ? new[] { 3, 1 } : new[] { 1, 3 };
			using var thickenStructure = Mat.Ones(MatType.CV_8U, thickenStructureSize).ToMat();

			Cv2.Dilate(mat, extendedLines, extendStructure, null, 10);
			Cv2.Dilate(mat, thickenedLines, thickenStructure);

			Cv2.BitwiseAnd(extendedLines, thickenedLines, mat);
		}

		private Mat CombineWithoutPeeks(Mat verticalLines, Mat horizontalLines)
		{
			using var holes = new Mat();
			using var vLinesWithHoles = new Mat();
			using var hLinesWithHoles = new Mat();

			Cv2.BitwiseAnd(verticalLines, horizontalLines, holes);
			Cv2.BitwiseXor(verticalLines, holes, vLinesWithHoles);
			Cv2.BitwiseXor(horizontalLines, holes, hLinesWithHoles);

			var scaledSlotSize = (int)(ProcessingConfig.ScaledSlotSize / 2);
			var size = scaledSlotSize - (scaledSlotSize % 2) + 1;

			using var vStructure = Mat.Ones(MatType.CV_8U, new[] { size, 1 }).ToMat();
			Cv2.Erode(vLinesWithHoles, vLinesWithHoles, vStructure);
			Cv2.Dilate(vLinesWithHoles, vLinesWithHoles, vStructure);

			using var hStructure = Mat.Ones(MatType.CV_8U, new[] { 1, size }).ToMat();
			Cv2.Erode(hLinesWithHoles, hLinesWithHoles, hStructure);
			Cv2.Dilate(hLinesWithHoles, hLinesWithHoles, hStructure);

			var grid = new Mat();
			Cv2.BitwiseOr(vLinesWithHoles, hLinesWithHoles, grid);
			Cv2.BitwiseOr(grid, holes, grid);
			return grid;
		}

		/// <summary>
		/// If the position at the indexer is part of a rectangle, it will be added to <see cref="_icons"/>
		/// </summary>
		/// <param name="indexer">The indexer of the mat</param>
		/// <param name="x">X position of the assumed icon</param>
		/// <param name="y">Y position of the assumed icon</param>
		/// <returns><see langword="true"/> if it is a valid icon, else <see langword="false"/></returns>
		private bool TryAddIcon(Mat.Indexer<byte> indexer, int x, int y)
		{
			/*
			 * The idea is, that we walk along the most inner
			 * edge of the assumed rectangle. If we do not find
			 * a left turn before reaching the end of our image,
			 * we know that the assumed rectangle was indeed not
			 * a rectangle and we return false.
			 *
			 * There are some optimizations to reduce the amount
			 * of loop iterations down to a minimum
			 */

			var rows = _grid.Rows;
			var cols = _grid.Cols;

			Vector2 bottomLeft = null;
			Vector2 bottomRight = null;
			Vector2 topRight = null;
			Vector2 topLeft = null;

			int a, b, c, d, e;

			// Go south
			for (a = y; a < rows; a++)
			{
				if (indexer[a, x].Equals(0x00)) return false;
				if (indexer[a, x + 1].Equals(0xFF))
				{
					bottomLeft = new Vector2(x, a);
					break;
				}

				if (!(a + 1 < rows)) return false;
			}

			// Go east
			for (b = x + 1; b < cols; b++)
			{
				if (indexer[a, b].Equals(0x00)) return false;
				if (indexer[a - 1, b].Equals(0xFF))
				{
					bottomRight = new Vector2(b, a);
					break;
				}

				if (!(b + 1 < cols)) return false;
			}

			// Go north
			for (c = a - 1; c > 0; c--)
			{
				if (indexer[c, b].Equals(0x00)) return false;
				if (indexer[c, b - 1].Equals(0xFF))
				{
					topRight = new Vector2(b, c);
					break;
				}

				if (!(c - 1 > 0)) return false;
			}

			// Go west
			for (d = b - 1; d >= x; d--)
			{
				if (indexer[c, d].Equals(0x00)) return false;
				if (indexer[c + 1, d].Equals(0xFF))
				{
					topLeft = new Vector2(d, c);
					break;
				}

				if (!(d - 1 >= x)) return false;
			}

			// Go south to origin
			for (e = c + 1; e <= y; e++)
			{
				if (indexer[e, d].Equals(0x00)) return false;
				if (!(e == y && d == x)) continue;

				var size = bottomRight - topLeft + Vector2.One;

				var altWidth = topRight.X - bottomLeft.X + 1;
				var altHeight = bottomLeft.Y - topRight.Y + 1;

				if (size != new Vector2(altWidth, altHeight)) return false;
				if (size == new Vector2(2, 2)) return false;
				if (_icons.Any(i => i.Position == topLeft)) return false;

				// Expand the rect by a small bit to make sure the icon is included
				var scaledSlotSize = (int)_config.ProcessingConfig.ScaledSlotSize;
				var scaledSlotSizeVec = new Vector2(scaledSlotSize, scaledSlotSize);
				topLeft -= scaledSlotSizeVec / 8;
				size += scaledSlotSizeVec / 4;

				var icon = _image.ToBitmap().Crop(topLeft.X, topLeft.Y, size.X, size.Y);
				_icons.Add(new Icon(icon, topLeft, size, _config));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Try to locate a icon at a given position
		/// </summary>
		/// <param name="position">Position at which to locate the icon. <see langword="null"/> = center</param>
		/// <returns><see langword="null"/> if no icon was found</returns>
		/// <remarks><see cref="Vector2.Zero"/> corresponds top left corner of the image</remarks>
		public Icon LocateIcon(Vector2 position = null)
		{
			SatisfyState(State.GridParsed);

			if (position == null) position = new Vector2(_grid.Size()) / 2;

			if (_config.ProcessingConfig.InventoryConfig.OptimizeHighlighted) return LocateIconHighlighted(position);

			if (Config.LogDebug)
			{
				Logger.LogDebugMat(_grid, "inventory/_grid");

				using var debugGrid = new Mat();
				Cv2.CvtColor(_grid.Clone(), debugGrid, ColorConversionCodes.GRAY2BGR);
				for (var i = 0; i < _icons.Count; i++)
				{
					var icon = _icons[i];
					var color = Scalar.RandomColor();
					for (var j = 0; j <= 20; j++)
					{
						var inc = Vector2.One * j;
						var rect = new Rect(icon.Position + inc, icon.Size - inc * 2);
						debugGrid.Rectangle(rect, color.Mul(new Scalar(j / 20f, j / 20f, j / 20f)));
					}
				}
				Logger.LogDebugMat(debugGrid, "inventory/iconRects");
			}

			foreach (var icon in _icons)
			{
				if (position.X <= icon.Position.X || position.Y <= icon.Position.Y) continue;

				var bottomRight = icon.Position + icon.Size;
				if (position.X < bottomRight.X && position.Y < bottomRight.Y) { return icon; }
			}

			return null;
		}

		private Icon LocateIconHighlighted(Vector2 position)
		{
			if (_boundingBoxes.Count == 0) return null;

			for (var i = 0; i < _boundingBoxes.Count; i++)
			{
				var bb = _boundingBoxes[i];
				if (!bb.Contains(position)) continue;

				// Compensate more in height as the top and bottom text often interfere
				var scaledSlotSize = (int)_config.ProcessingConfig.ScaledSlotSize;
				bb.X -= scaledSlotSize / 8;
				bb.Y -= scaledSlotSize / 4;
				bb.Width += scaledSlotSize / 4;
				bb.Height += scaledSlotSize / 2;

				if (Config.LogDebug)
				{
					using var debugMat = _image.Clone();
					debugMat.Rectangle(bb, new(255, 128, 0));
					Logger.LogDebugMat(debugMat, "inventory/icon");
				}

				var iconImage = _image.ToBitmap().Crop(bb.X, bb.Y, bb.Width, bb.Height);
				var icon = new Icon(iconImage, new(bb.X, bb.Y), new(bb.Width, bb.Height), _config);
				_icons = new List<Icon> { icon };
				return icon;
			}
			return null;
		}

		/// <summary>
		/// Inventory destructor
		/// </summary>
		~Inventory()
		{
			_image?.Dispose();
			_grid?.Dispose();
			_vertGrid?.Dispose();
		}
	}
}
