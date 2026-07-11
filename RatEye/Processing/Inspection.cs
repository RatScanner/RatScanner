using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using RatStash;
using Tesseract;

namespace RatEye.Processing
{
	/// <summary>
	/// Represents an inspection view
	/// </summary>
	public class Inspection
	{
		private readonly Config _config;
		private readonly Bitmap _image;

		private Config.Path PathConfig => _config.PathConfig;
		private Config.Processing ProcessingConfig => _config.ProcessingConfig;
		private Config.Processing.Inspection InspectionConfig => ProcessingConfig.InspectionConfig;

		// Backing property fields
		private Vector2 _markerPosition;
		private float _markerConfidence;
		private string _title = "";

		/// <summary>
		/// Position of the marker in the given image
		/// </summary>
		/// <remarks><see langword="null"/> if no marker above threshold found</remarks>
		public Vector2 MarkerPosition
		{
			get
			{
				SatisfyState(State.SearchedMarker);
				return _markerPosition;
			}
			private set => _markerPosition = value;
		}

		/// <summary>
		/// The confidence that the image contains a marker
		/// </summary>
		public float MarkerConfidence
		{
			get
			{
				SatisfyState(State.SearchedMarker);
				return _markerConfidence;
			}
			private set => _markerConfidence = value;
		}

		/// <summary>
		/// <see langword="true"/> if the provided image contains a marker above the threshold
		/// </summary>
		public bool ContainsMarker => MarkerConfidence >= InspectionConfig.MarkerThreshold;

		/// <summary>
		/// Title of the inspection window
		/// <para>The title which is to the right of the marker</para>
		/// </summary>
		public string Title
		{
			get
			{
				SatisfyState(State.ScannedTitle);
				return _title;
			}
			private set => _title = value;
		}

		/// <summary>
		/// Detected item
		/// </summary>
		public Item Item
		{
			get
			{
				SatisfyState(State.ScannedTitle);
				return GetItem();
			}
		}

		/// <summary>
		/// The path to the icon of the detected item
		/// </summary>
		public string IconPath => _config.IconManager.GetIconPath(Item, new ItemExtraInfo());

		/// <summary>
		/// Constructor for inspection view processing object
		/// </summary>
		/// <param name="image">Image of the inspection view which will be processed</param>
		/// <param name="config">The config to use for this instance</param>
		/// <remarks>Provided image has to be in RGB</remarks>
		internal Inspection(Bitmap image, Config config)
		{
			_config = config;
			_image = image;
		}

		/// <summary>
		/// Constructor for inspection view processing object
		/// </summary>
		/// <param name="image">Image of the inspection view which will be processed</param>
		/// <param name="config">The config to use for this instance</param>
		/// <param name="markerPosition">Position of the marker in the given image</param>
		/// <param name="markerConfidence">Confidence of the marker in the given image</param>
		/// <remarks>Provided image has to be in RGB</remarks>
		internal Inspection(Bitmap image, Config config, Vector2 markerPosition, float markerConfidence)
		{
			_config = config;
			_image = image;
			_currentState = State.SearchedMarker;
			_markerPosition = markerPosition;
			_markerConfidence = markerConfidence;
		}

		#region Processing state handling

		private enum State
		{
			Default,
			SearchedMarker,
			ScannedTitle,
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
					case State.SearchedMarker:
						SearchMarker();
						break;
					case State.ScannedTitle:
						ScanTitle();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		#endregion

		/// <summary>
		/// Search for markers and pick the best matching one
		/// </summary>
		private void SearchMarker()
		{
			SatisfyState(State.Default);

			var marker = GetMarkerPosition(GetScaledMarker());
			MarkerConfidence = marker.confidence;
			MarkerPosition = marker.position;
		}

		/// <summary>
		/// Identify the give marker inside the source
		/// </summary>
		/// <param name="marker">The marker template to identify</param>
		/// <remarks>Provided marker has to be in RGB</remarks>
		/// <returns>Confidence and position of the best match</returns>
		private (float confidence, Vector2 position) GetMarkerPosition(Bitmap marker)
		{
			using var refMat = _image.ToMat();
			using var tplMat = marker.ToMat(); // tpl = template
			using var res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1);

			// Gray scale both reference and template image
			using var gref = refMat.CvtColor(ColorConversionCodes.RGB2GRAY);
			using var gtpl = tplMat.CvtColor(ColorConversionCodes.RGB2GRAY);

			Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
			//Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);
			Cv2.MinMaxLoc(res, out _, out var maxVal, out _, out var maxLoc);

			return ((float)maxVal, new Vector2(maxLoc));
		}

		private void ScanTitle()
		{
			if (!ContainsMarker)
			{
				// No marker? Why even bother scanning...
				Logger.LogDebug("No marker found!");
				return;
			}

			// Compute title search box dimensions
			var position = MarkerPosition;
			position.X += GetHorizontalTitleSearchOffset();

			// Find end of the title bar
			var scaledMarker = GetScaledMarker();
			var closeBtnCenterHeight = MarkerPosition.Y + (scaledMarker.Height / 2);
			var lowC = InspectionConfig.CloseButtonColorLowerBound;
			var upC = InspectionConfig.CloseButtonColorUpperBound;
			var closeButtonPosition = _image.FindPixelInRange(closeBtnCenterHeight, lowC, upC, position.X);

			// Construct final search box dimensions
			var scaledTitleSearchHeight = (int)(InspectionConfig.BaseTitleSearchHeight * ProcessingConfig.Scale);
			var scaledTitleSearchWidth = (int)(InspectionConfig.BaseTitleSearchWidth * ProcessingConfig.Scale);
			var height = Math.Min(scaledTitleSearchHeight, _image.Height - position.Y);
			var width = Math.Min(scaledTitleSearchWidth, _image.Width - position.X);
			if (closeButtonPosition > 0)
			{
				// Calculate width of search box to the close button edge
				var tmpWidth = closeButtonPosition - position.X;
				// Shorten the width to account for extra buttons ( for example sort buttons )
				var titleSearchRightPadding =
					(int)(InspectionConfig.BaseTitleSearchRightPadding * ProcessingConfig.Scale);
				tmpWidth -= titleSearchRightPadding;
				// Apply new width if its the new minimum width
				width = Math.Min(width, tmpWidth);
			}

			// Crop image to title search box
			var searchBox = _image.Crop(position.X, position.Y, width, height);

			// Rescale title search box to 4k, adjusting the font size to the training data
			// We multiply the inverse scale with 2f to rescale to 4k instead of 1080p
			var rescaledSearchBox = searchBox.Rescale(ProcessingConfig.InverseScale * 2f);

			Title = OCR(rescaledSearchBox);
		}

		/// <summary>
		/// Perform optical character recognition on a image
		/// </summary>
		/// <param name="image">Image to perform OCR on</param>
		/// <returns>Detected characters in image</returns>
		private string OCR(Bitmap image)
		{
			// Setup tesseract
			using var mat = image.ToMat();

			// Gray scale image
			Logger.LogDebug("Gray scaling...");
			var cvu83 = mat.CvtColor(ColorConversionCodes.BGR2GRAY, 1);

			// Binarize image
			Logger.LogDebug("Binarizing...");
			cvu83 = cvu83.Threshold(120, 255, ThresholdTypes.BinaryInv);

			// Convert to Pix
			using var pix = PixConverter.ToPix(cvu83.ToBitmap());

			// OCR
			Logger.LogDebug("Applying OCR...");
			using var result = GetTesseractEngine().Process(pix);
			var text = result.GetText();

			Logger.LogDebug("Read: " + text);
			return text.CyrillicToLatin().Trim();
		}

		/// <summary>
		/// Creates an instance of the OCRTesseract class. Initializes Tesseract.
		/// </summary>
		/// <returns>Tesseract instance trained for the bender font</returns>
		private TesseractEngine GetTesseractEngine()
		{
			// Return if tesseract instance was already created
			var tesseractEngine = InspectionConfig.TesseractEngine;
			if (tesseractEngine != null) return tesseractEngine;

			// Check if trained data is present
			var langCode = _config.ProcessingConfig.Language.ToISO3Code();
			var traineddataPath = $"{PathConfig.TrainedData}\\{langCode}.traineddata";
			if (!File.Exists(traineddataPath))
			{
				var message = "Could not find traineddata at: " + traineddataPath;
				var ex = new ArgumentException(message, PathConfig.TrainedData);
				throw ex;
			}

			// Load additional language to expand the primary one
			var addLang = _config.ProcessingConfig.Language switch
			{
				//Language.Chinese => "eng",
				Language.Czech => "+eng",
				Language.Japanese => "+eng",
				Language.Korean => "+eng",
				Language.Russian => "+eng",
				_ => "",
			};

			var language = langCode + addLang;

			// Create a tesseract instance
			InspectionConfig.TesseractEngine = new TesseractEngine(PathConfig.TrainedData, language, EngineMode.LstmOnly);
			InspectionConfig.TesseractEngine.DefaultPageSegMode = PageSegMode.RawLine;

			return InspectionConfig.TesseractEngine;
		}

		/// <summary>
		/// Generate a marker bitmap
		/// </summary>
		/// <remarks><see cref="Config.Processing.Scale"/> is already accounted for.</remarks>
		/// <returns>A rescaled and alpha blended version of <see cref="Config.Processing.Inspection.Marker"/></returns>
		private Bitmap GetScaledMarker()
		{
			var output = InspectionConfig.Marker.Rescale(InspectionConfig.MarkerItemScale * ProcessingConfig.Scale);
			return output.TransparentToColor(InspectionConfig.MarkerBackgroundColor);
		}

		/// <summary>
		/// Scaled horizontal offset of the inspection window title search box
		/// <para>See <see cref="Config.Processing.Inspection.HorizontalTitleSearchOffsetFactor"/>.</para>
		/// </summary>
		/// <returns>The distance between the right edge of the marker and the beginning of the title search box</returns>
		private int GetHorizontalTitleSearchOffset()
		{
			var width = GetScaledMarker().Width;
			return (int)(width * InspectionConfig.HorizontalTitleSearchOffsetFactor);
		}

		/// <summary>
		/// Get the item, best matching the scanned title
		/// </summary>
		/// <returns>Item instance</returns>
		private Item GetItem()
		{
			var items = _config.RatStashDB.GetItems();
			return items.Aggregate((i1, i2) =>
			{
				var i1Dist = i1.Name.CyrillicToLatin().NormedLevenshteinDistance(Title);
				var i2Dist = i2.Name.CyrillicToLatin().NormedLevenshteinDistance(Title);
				return i1Dist > i2Dist ? i1 : i2;
			});
		}
	}
}
