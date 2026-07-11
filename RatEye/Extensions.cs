using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace RatEye
{
	/// <summary>
	/// Extension methods for OpenCvSharp
	/// </summary>
	public static class Extensions
	{
		#region Bitmap Extensions

		/// <summary>
		/// Rescales a bitmap
		/// </summary>
		/// <param name="image">The image to be rescaled</param>
		/// <param name="scale">Scale factor by which the dimensions will be multiplied</param>
		/// <returns>The rescaled input image</returns>
		/// <remarks>
		/// Uses <see cref="InterpolationFlags.Area"/> for downscaling and <see cref="InterpolationFlags.Cubic"/> for upscaling
		/// </remarks>
		public static Bitmap Rescale(this Bitmap image, float scale)
		{
			if (scale == 1f) return image;

			using var mat = image.ToMat();
			var rescaledSize = new Size((int)(mat.Width * scale), (int)(mat.Height * scale));
			var rescaleMode = scale < 1 ? InterpolationFlags.Area : InterpolationFlags.Cubic;
			Cv2.Resize(mat, mat, rescaledSize, 0, 0, rescaleMode);
			var rescaledImage = mat.ToBitmap();
			return rescaledImage;
		}


		/// <summary>
		/// Rotates a bitmap
		/// </summary>
		/// <param name="image">The image to be rotated</param>
		/// <returns>The rotated input image</returns>
		public static Bitmap Rotate(this Bitmap image)
		{
			using var mat = image.ToMat();
			Cv2.Rotate(mat, mat, RotateFlags.Rotate90Counterclockwise);
			return mat.ToBitmap();
		}

		/// <summary>
		/// Alpha blends the input image with a target color
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="target">The color which will replace the transparency</param>
		/// <returns>The input image with replaced alpha in 24bppRgb format</returns>
		public static Bitmap TransparentToColor(this Bitmap image, Color target)
		{
			var output = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
			var rect = new Rectangle(Point.Empty, image.Size);
			using var g = Graphics.FromImage(output);
			g.Clear(target);
			g.DrawImageUnscaledAndClipped(image, rect);
			return output;
		}

		/// <summary>
		/// Crops the input image
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="x">Horizontal position of the lower left corner</param>
		/// <param name="y">Vertical position of the lower left corner</param>
		/// <param name="width">Width of the output image</param>
		/// <param name="height">Height of the output image</param>
		/// <param name="ignoreBounds">Ignore if the given parameters go out of the image bounds and crop anyways</param>
		/// <returns>The cropped input image</returns>
		/// <exception cref="ArgumentException">x, y, width and height accept non negative values only</exception>
		/// <exception cref="ArgumentOutOfRangeException">The input image is not big enough for the desired crop</exception>
		public static Bitmap Crop(this Bitmap image, int x, int y, int width, int height, bool ignoreBounds = true)
		{
			if (ignoreBounds)
			{
				if (x < 0) width -= -x;
				if (y < 0) height -= -y;

				x = Math.Max(0, x);
				y = Math.Max(0, y);

				width = Math.Min(x + width, image.Width) - x;
				height = Math.Min(y + height, image.Height) - y;
			}
			else
			{
				if (x < 0 || y < 0 || width < 0 || height < 0)
				{
					const string message = "x, y, width and height accept non negative values only.";
					throw new ArgumentException(message);
				}

				if (x + width > image.Width || y + height > image.Height)
				{
					const string message = "The input image is not big enough for the desired crop";
					throw new ArgumentOutOfRangeException(nameof(image), message);
				}
			}

			var rect = new Rectangle(x, y, width, height);
			return new Bitmap(image).Clone(rect, image.PixelFormat);
		}

		/// <summary>
		/// Finds the horizontal position of the first pixel, on a given height and in a color range.
		/// Pixels are iterated from left to right.
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="searchHeight">The height on which to search for a matching pixel</param>
		/// <param name="lowerBoundColor">The lower bound matching color</param>
		/// <param name="upperBoundColor">The upper bound matching color</param>
		/// <param name="start">Start position for searching</param>
		/// <returns>The horizontal position of the first matching pixel. -1 if no match was found</returns>
		internal static int FindPixelInRange(
			this Bitmap image,
			int searchHeight,
			Color lowerBoundColor,
			Color upperBoundColor,
			int start = 0)
		{
			using var mat = image.ToMat();
			using var mat3 = new Mat<Vec3b>(mat);
			var indexer = mat3.GetIndexer();

			for (var x = start; x < mat.Width; x++)
			{
				var (blue, green, red) = indexer[searchHeight, x];

				if (blue < lowerBoundColor.B || blue > upperBoundColor.B) continue;
				if (green < lowerBoundColor.G || green > upperBoundColor.G) continue;
				if (red < lowerBoundColor.R || red > upperBoundColor.R) continue;

				return x;
			}

			return -1;
		}

		#endregion

		#region Mat Extensions

		/// <summary>
		/// Alpha blend two 8UC4 matrices
		/// </summary>
		/// <param name="a">Top matrix of type 8UC4</param>
		/// <param name="b">Bottom matrix of type 8UC4</param>
		/// <returns>A blended matrix of type 8UC4</returns>
		internal static Mat AlphaBlend(this Mat a, Mat b)
		{
			// Extract a alpha
			using var aAlpha = a.ExtractChannel(3);
			aAlpha.ConvertTo(aAlpha, MatType.CV_32FC1, 1 / 255f);

			// Extract b alpha
			using var bAlpha = b.ExtractChannel(3);
			bAlpha.ConvertTo(bAlpha, MatType.CV_32FC1, 1 / 255f);

			using var invBottomAlpha = Mat.Ones(aAlpha.Size(), MatType.CV_32FC1).Subtract(aAlpha).ToMat();

			using var aColor = a.RemoveChannel4();
			aColor.ConvertTo(aColor, MatType.CV_32FC3, 1 / 255f);

			using var bColor = b.RemoveChannel4();
			bColor.ConvertTo(bColor, MatType.CV_32FC3, 1 / 255f);

			using var tmp = invBottomAlpha.Mul(bAlpha).ToMat();
			Cv2.CvtColor(tmp, tmp, ColorConversionCodes.GRAY2BGR);
			Cv2.Multiply(tmp, bColor, tmp);

			using var alpha = aAlpha.Add(invBottomAlpha.Mul(bAlpha)).ToMat();
			using var alpha3C = alpha.CvtColor(ColorConversionCodes.GRAY2BGR);

			using var result = aAlpha.CvtColor(ColorConversionCodes.GRAY2BGR);
			Cv2.Multiply(result, aColor, result);
			Cv2.Add(result, tmp, result);
			Cv2.Divide(result, alpha3C, result);
			result.ConvertTo(result, MatType.CV_8UC3, 255);

			var output = new Mat(a.Size(), MatType.CV_8UC4);
			result.ExtractChannel(0).InsertChannel(output, 0);
			result.ExtractChannel(1).InsertChannel(output, 1);
			result.ExtractChannel(2).InsertChannel(output, 2);
			alpha.ConvertTo(alpha, MatType.CV_8UC1, 255);
			alpha.ExtractChannel(0).InsertChannel(output, 3);
			return output;
		}

		/// <summary>
		/// Remove the 4th channel of a matrix
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <returns>Matrix with truncated channels</returns>
		internal static Mat RemoveChannel4(this Mat src)
		{
			var type = src.Type() == MatType.CV_8UC4 ? MatType.CV_8UC3 : MatType.CV_32FC3;
			var output = Mat.Ones(src.Size(), type).ToMat();
			src.ExtractChannel(0).InsertChannel(output, 0);
			src.ExtractChannel(1).InsertChannel(output, 1);
			src.ExtractChannel(2).InsertChannel(output, 2);
			return output;
		}

		/// <summary>
		/// Remove all transparency from a 8UC4 matrix
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <returns>Matrix of type 8UC4</returns>
		internal static Mat RemoveTransparency(this Mat src)
		{
			var output = Mat.Ones(src.Size(), MatType.CV_8UC4).ToMat();
			using var clear = new Mat(src.Size(), MatType.CV_8UC4).SetTo(new Scalar(1, 1, 1, 0));
			using var full = new Mat(src.Size(), MatType.CV_8UC4).SetTo(new Scalar(0, 0, 0, 255));
			Cv2.Multiply(src, clear, output);
			Cv2.Add(output, full, output);
			return output;
		}

		/// <summary>
		/// Replicates the input matrix the specified number of times in the horizontal and/or vertical direction
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <param name="nx">How many times the src is repeated along the horizontal axis</param>
		/// <param name="ny">How many times the src is repeated along the vertical axis</param>
		/// <param name="dx">Horizontal left-padding</param>
		/// <param name="dy">Vertical top-padding</param>
		/// <returns>The repeated matrix</returns>
		internal static Mat Repeat(this Mat src, int nx, int ny, int dx, int dy)
		{
			var dst = new Mat((src.Rows + dy) * ny - dy, (src.Cols + dx) * nx - dx, src.Type());
			for (var iy = 0; iy < ny; ++iy)
			{
				for (var ix = 0; ix < nx; ++ix)
				{
					var roi = new Rect((src.Cols + dx) * ix, (src.Rows + dy) * iy, src.Cols, src.Rows);
					src.CopyTo(dst[roi]);
				}
			}

			return dst;
		}

		/// <summary>
		/// Add padding to a mat
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <param name="left">Amount of left padding</param>
		/// <param name="top">Amount of top padding</param>
		/// <param name="right">Amount of right padding</param>
		/// <param name="bottom">Amount of bottom padding</param>
		/// <returns>New matrix of same type with padding</returns>
		internal static Mat AddPadding(this Mat src, int left, int top, int right, int bottom)
		{
			var resultSize = new Size(src.Width + left + right, src.Height + top + bottom);
			var result = new Mat(resultSize, src.Type()).SetTo(new Scalar(0, 0, 0, 0));
			src.CopyTo(result[top, top + src.Rows, left, left + src.Cols]);
			return result;
		}

		/// <summary>
		/// Remove padding to a mat
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <param name="left">Amount of left padding</param>
		/// <param name="top">Amount of top padding</param>
		/// <param name="right">Amount of right padding</param>
		/// <param name="bottom">Amount of bottom padding</param>
		/// <returns>New matrix of same type with removed padding</returns>
		internal static Mat RemovePadding(this Mat src, int left, int top, int right, int bottom)
		{
			var resultSize = new Size(src.Width - left - right, src.Height - top - bottom);
			var result = new Mat(resultSize, src.Type()).SetTo(new Scalar(0, 0, 0, 0));
			src[top, src.Rows - bottom, left, src.Cols - right].CopyTo(result);
			return result;
		}

		#endregion

		#region String Extensions
		/// <summary>
		/// Calculates the normed Levenshtein distance between two strings
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="target">Target string</param>
		/// <returns>Levenshtein distance</returns>
		public static float NormedLevenshteinDistance(this string source, string target)
		{
			if (string.IsNullOrEmpty(source)) { return string.IsNullOrEmpty(target) ? 0 : target.Length; }

			if (string.IsNullOrEmpty(target)) return source.Length;

			if (source.Length > target.Length)
			{
				var temp = target;
				target = source;
				source = temp;
			}

			var m = target.Length;
			var n = source.Length;
			var distance = new int[2, m + 1];
			// Initialize the distance matrix
			for (var j = 1; j <= m; j++) distance[0, j] = j;

			var currentRow = 0;
			for (var i = 1; i <= n; ++i)
			{
				currentRow = i & 1;
				distance[currentRow, 0] = i;
				var previousRow = currentRow ^ 1;
				for (var j = 1; j <= m; j++)
				{
					var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
					distance[currentRow, j] = Math.Min(Math.Min(
							distance[previousRow, j] + 1,
							distance[currentRow, j - 1] + 1),
						distance[previousRow, j - 1] + cost);
				}
			}

			return (float)(target.Length - distance[currentRow, m]) / target.Length;
		}
		
		/// <summary>
		/// Computes the SHA256 hash of the string
		/// </summary>
		/// <param name="value">String to be hashed</param>
		/// <returns>Hash as hex string</returns>
		public static string SHA256Hash(this string value)
		{
			var buffer = Encoding.UTF8.GetBytes(value);
			var hash = SHA256.Create().ComputeHash(buffer);
			return string.Concat(hash.Select(x => x.ToString("X2")));
		}

		/// <summary>
		/// Computes key which can be used in the cache which is unique given the config
		/// </summary>
		/// <param name="value">String which will influence the key</param>
		/// <param name="config">Config which will influence the key</param>
		/// <returns>Cache key</returns>
		internal static string CacheKey(this string value, Config config)
		{
			return value.CacheKey(config.GetHash());
		}

		/// <summary>
		/// Computes key which can be used in the cache which is unique given the config
		/// </summary>
		/// <param name="value">String which will influence the key</param>
		/// <param name="configHash">Hash of the config which will influence the key</param>
		/// <returns>Cache key</returns>
		internal static string CacheKey(this string value, string configHash)
		{
			return (value + configHash).SHA256Hash();
		}

		#endregion
	}
}
