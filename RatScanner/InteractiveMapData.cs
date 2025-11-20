using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner
{
    public class InteractiveMapData
	{
		[JsonProperty("normalizedName")]
		public string NormalizedName { get; set; }

		[JsonProperty("primaryPath")]
		public string PrimaryPath { get; set; }

		[JsonProperty("maps")]
		public List<Map> Maps { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		public class Extent
		{
			[JsonProperty("height")]
			public List<double?> Height { get; set; }

			[JsonProperty("bounds")]
			public List<List<object>> Bounds { get; set; }
		}

		public class Label
		{
			[JsonProperty("position")]
			public List<double?> Position { get; set; }

			[JsonProperty("text")]
			public string Text { get; set; }

			[JsonProperty("rotation")]
			public object Rotation { get; set; }

			[JsonProperty("size")]
			public int? Size { get; set; }

			[JsonProperty("top")]
			public int? Top { get; set; }

			[JsonProperty("bottom")]
			public double? Bottom { get; set; }
		}

		public class Layer
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("svgLayer")]
			public string SvgLayer { get; set; }

			[JsonProperty("show")]
			public bool? Show { get; set; }

			[JsonProperty("extents")]
			public List<Extent> Extents { get; set; }

			[JsonProperty("tilePath")]
			public string TilePath { get; set; }
		}

		public class Map
		{
			[JsonProperty("key")]
			public string Key { get; set; }

			[JsonProperty("projection")]
			public string Projection { get; set; }

			[JsonProperty("minZoom")]
			public int? MinZoom { get; set; }

			[JsonProperty("maxZoom")]
			public int? MaxZoom { get; set; }

			[JsonProperty("transform")]
			public List<double?> Transform { get; set; }

			[JsonProperty("coordinateRotation")]
			public int? CoordinateRotation { get; set; }

			[JsonProperty("bounds")]
			public List<List<double?>> Bounds { get; set; }

			[JsonProperty("heightRange")]
			public List<double?> HeightRange { get; set; }

			[JsonProperty("author")]
			public string Author { get; set; }

			[JsonProperty("authorLink")]
			public string AuthorLink { get; set; }

			[JsonProperty("svgPath")]
			public string SvgPath { get; set; }

			[JsonProperty("svgLayer")]
			public string SvgLayer { get; set; }

			[JsonProperty("layers")]
			public List<Layer> Layers { get; set; }

			[JsonProperty("labels")]
			public List<Label> Labels { get; set; }

			[JsonProperty("specific")]
			public string Specific { get; set; }

			[JsonProperty("altMaps")]
			public List<string> AltMaps { get; set; }

			[JsonProperty("tileSize")]
			public int? TileSize { get; set; }

			[JsonProperty("tilePath")]
			public string TilePath { get; set; }

			[JsonProperty("_heightRange")]
			public List<int?> AlternateHeightRange { get; set; }

			[JsonProperty("orientation")]
			public string Orientation { get; set; }

			[JsonProperty("svgBounds")]
			public List<List<int?>> SvgBounds { get; set; }
		}
	}
}
