using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = OpenCvSharp.Point;

namespace RatEye.Processing
{
	/// <summary>
	/// Represents multiple <see cref="RatEye.Processing.Inspection"/>
	/// </summary>
	public class MultiInspection
	{
		private readonly Config _config;
		private readonly Bitmap _image;

		private Config.Path PathConfig => _config.PathConfig;
		private Config.Processing ProcessingConfig => _config.ProcessingConfig;
		private Config.Processing.Inspection InspectionConfig => ProcessingConfig.InspectionConfig;

		// Backing property fields
		private List<Inspection> _inspections;

		/// <summary>
		/// List of all inspections found in the image
		/// </summary>
		public List<Inspection> Inspections
		{
			get
			{
				SatisfyState(State.SearchedMarkers);
				return _inspections;
			}
			private set => _inspections = value;
		}

		/// <summary>
		/// Constructor for MultiInspection view processing object
		/// </summary>
		/// <param name="image">Image of the multiInspection view which will be processed</param>
		/// <param name="config">The config to use for this instance></param>
		/// <remarks>Provided image has to be in RGB</remarks>
		internal MultiInspection(Bitmap image, Config config)
		{
			_config = config;
			_image = image;
		}

		#region Processing state handling

		private enum State
		{
			Default,
			SearchedMarkers,
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
					case State.SearchedMarkers:
						SearchMarker();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		#endregion

		/// <summary>
		/// Search for all different marker types and pick the best matching one
		/// </summary>
		private void SearchMarker()
		{
			SatisfyState(State.Default);

			var markers = GetMarkerPositions(GetScaledMarker());
			var threshold = InspectionConfig.MarkerThreshold;
			_inspections = markers.Select(marker => new Inspection(_image, _config, marker, threshold)).ToList();
		}

		/// <summary>
		/// Identify the give marker inside the source
		/// </summary>
		/// <param name="marker">The marker template to identify</param>
		/// <remarks>Provided marker has to be in RGB</remarks>
		/// <returns>List of markers which confidence is above <see cref="Config.Processing.Inspection.MarkerThreshold"/></returns>
		private List<Vector2> GetMarkerPositions(Bitmap marker)
		{
			using var refMat = _image.ToMat();
			using var tplMat = marker.ToMat(); // tpl = template
			using var res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1);

			// Gray scale both reference and template image
			using var gref = refMat.CvtColor(ColorConversionCodes.RGB2GRAY);
			using var gtpl = tplMat.CvtColor(ColorConversionCodes.RGB2GRAY);

			Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
			Cv2.Threshold(res, res, InspectionConfig.MarkerThreshold, 1, ThresholdTypes.Binary);
			var nonZeroes = res.FindNonZero();
			if (nonZeroes.Empty()) return new List<Vector2>();

			nonZeroes.GetArray(out Point[] points);
			return points.Select(point => new Vector2(point)).ToList();
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
	}
}
