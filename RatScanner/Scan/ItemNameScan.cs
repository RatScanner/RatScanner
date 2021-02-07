using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;
using RatScanner.Properties;
using RCNameScan = RatScanner.RatConfig.NameScan;

namespace RatScanner.Scan
{
	internal class ItemNameScan : ItemScan
	{

		internal Vector2 MarkerPos;

		internal Vector2 MousePos;

		internal Vector2 LocalMarkerPos;

		internal bool ContainsMarker => LocalMarkerPos != null;

		internal string ScannedText;

		internal bool IsShortName;

		private static OCRTesseract ocrTesseractInstance;

		internal static void Init()
		{
			Logger.LogInfo("Loading OCR data...");
			var traineddataDir = RatConfig.Paths.Data + "\\";
			var traineddataPath = traineddataDir + "bender.traineddata";
			if (!System.IO.File.Exists(traineddataPath))
			{
				Logger.LogError("Could not find traineddata at: " + traineddataPath);
				return;
			}
			ocrTesseractInstance = OCRTesseract.Create(traineddataDir, "bender", null, 3, 7);
		}

		internal ItemNameScan()
		{
			MatchedItems = new[] { RatScannerMain.Instance.MarketDB.GetItemByName("", false, out var _) };
		}

		internal ItemNameScan(Bitmap capture, Vector2 mouseVector2)
		{
			Capture = capture;
			MousePos = mouseVector2;

			// Get potential marker section of the screenshot
			var rectMarker = new Rectangle(0, 0, RCNameScan.MarkerScanSize, RCNameScan.MarkerScanSize);
			var markerSection = new Bitmap(Capture).Clone(rectMarker, Capture.PixelFormat);

			// Check if the section contains the marker
			LocalMarkerPos = GetMarkerPosition(markerSection, RCNameScan.Marker);

			// If the marker could not be found, check for short name marker
			if (!ContainsMarker)
			{
				IsShortName = true;
				LocalMarkerPos = GetMarkerPosition(markerSection, RCNameScan.MarkerShort);
			}

			// No marker matched so we can stop the scan
			// ContainsMarker => LocalMarkerPos != null;
			if (!ContainsMarker) return;
			Logger.LogDebug("Marker found! IsShortName: " + IsShortName);
			Logger.ClearDebugMats();

			// Calculate global marker position
			MarkerPos = MousePos;
			MarkerPos += LocalMarkerPos;
			MarkerPos -= new Vector2(RCNameScan.MarkerScanSize, RCNameScan.MarkerScanSize) / 2;

			// Get text section of the screenshot
			var textSectionX = LocalMarkerPos.X + RCNameScan.TextHorizontalOffset;
			var rectText = new Rectangle(textSectionX, LocalMarkerPos.Y, RCNameScan.TextWidth, RCNameScan.TextHeight);
			var textSection = new Bitmap(Capture).Clone(rectText, Capture.PixelFormat);

			// Use OCR to read the text
			ScannedText = OCR(textSection).Split('\n')[0];

			if (ScannedText.Length <= 0)
			{
				Logger.LogInfo("Couldn't find any text.");
				return;
			}

			// Cleanup text for short names since scan width is bigger than window size
			if (IsShortName)
			{
				var temp = ScannedText.ToLower();
				// 'x' is the red button, in the upper right corner, to close the window
				int? index = temp.IndexOf(" x", StringComparison.Ordinal);
				if (index > 0) ScannedText = ScannedText.Substring(0, index.Value);
			}

			ScannedText = ScannedText.Trim();

			// Get market item from detected text
			const string regexFilter = @"[^A-Za-z0-9]+"; // Filter out everything which is not alphanumerical
			MatchedItems = new[] { RatScannerMain.Instance.MarketDB.GetItemByName(ScannedText, IsShortName, out Confidence, regexFilter, false) };

			if (Confidence < RCNameScan.ConfWarnThreshold)
			{
				var tmp = "OCR:\"" + ScannedText + "\" Matched:\"" + MatchedItems[0].Name + "\"";
				Logger.LogWarning("Item below threshold. Perhaps a new item? " + tmp);
			}

			if (MatchedItems?.Length > 0) ValidItem = true;
		}

		// Identifies the marker and return the upper left corner of the match.
		// null is returned if no match is found.
		private Vector2 GetMarkerPosition(Bitmap src, Bitmap marker)
		{
			using (var refMat = src.ToMat())
			using (var tplMat = marker.ToMat())
			using (var res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
			{
				//Convert input images to gray
				var gref = refMat.Channels() == 3 ? refMat.CvtColor(ColorConversionCodes.BGR2GRAY) : refMat;
				var gtpl = tplMat.Channels() == 3 ? tplMat.CvtColor(ColorConversionCodes.BGR2GRAY) : tplMat;

				Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
				Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);

				Cv2.MinMaxLoc(res, out _, out var maxVal, out _, out var maxLoc);

				if (maxVal >= RCNameScan.MarkerThreshold) return new Vector2(maxLoc.X, maxLoc.Y);
			}

			return null;    // No match found
		}

		// Convert image to text
		private string OCR(Bitmap image)
		{
			Logger.LogDebugBitmap(image, "textSection");

			// Resize bitmap
			// We double the size to make the text fit the scale of our training data
			Logger.LogDebug("Scaling image...");
			var resizedImage = new Bitmap(image.Width * 2, image.Height * 2);

			using (var gr = Graphics.FromImage(resizedImage))
			{
				gr.SmoothingMode = SmoothingMode.HighQuality;
				gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var destRect = new Rectangle(0, 0, image.Width * 2, image.Height * 2);
				var srcRect = new Rectangle(0, 0, image.Width, image.Height);
				gr.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
			}
			Logger.LogDebugBitmap(resizedImage, "resizedTextSection");

			// Setup tesseract
			string text;

			using (var mat = resizedImage.ToMat())
			{
				// Gray scale image
				Logger.LogDebug("Gray scaling...");
				var cvu83 = mat.CvtColor(ColorConversionCodes.BGR2GRAY, 1);

				// Binarize image
				Logger.LogDebug("Binarizing...");
				cvu83 = cvu83.Threshold(120, 255, ThresholdTypes.BinaryInv);

				// OCR
				Logger.LogDebug("Applying OCR...");
				ocrTesseractInstance.Run(cvu83, out text, out _, out _, out _, ComponentLevels.TextLine);
			}

			Logger.LogDebug("Read: " + text);
			return text;
		}

		internal override Vector2 GetToolTipPosition()
		{
			var position = MarkerPos;
			position += new Vector2(0, Resources.markerFHD.Height);
			position += new Vector2(RatConfig.ToolTip.WidthOffset, RatConfig.ToolTip.HeightOffset);
			return position;
		}
	}
}
