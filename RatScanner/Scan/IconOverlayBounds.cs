using System;
using System.Drawing;

namespace RatScanner.Scan;

internal static class IconOverlayBounds {
	private static readonly Color GridColor = Color.FromArgb(0x49, 0x51, 0x54);
	private const int GridColorTolerance = 3;
	private const int SearchRadius = 2;

	internal static Rectangle FitToGrid(Bitmap screenshot, Rectangle match) {
		var left = FindGridEdge(screenshot, match.Left, match.Top, match.Height, vertical: true);
		var right = FindGridEdge(screenshot, match.Right - 1, match.Top, match.Height, vertical: true);
		var top = FindGridEdge(screenshot, match.Top, match.Left, match.Width, vertical: false);
		var bottom = FindGridEdge(screenshot, match.Bottom - 1, match.Left, match.Width, vertical: false);

		// Require a matching pair of grid lines before moving either edge on an axis.
		return Rectangle.FromLTRB(
			left.HasValue && right.HasValue ? left.Value : match.Left,
			top.HasValue && bottom.HasValue ? top.Value : match.Top,
			left.HasValue && right.HasValue ? right.Value + 1 : match.Right,
			top.HasValue && bottom.HasValue ? bottom.Value + 1 : match.Bottom);
	}

	private static int? FindGridEdge(Bitmap screenshot, int edge, int start, int length, bool vertical) {
		if (IsGridEdge(screenshot, edge, start, length, vertical)) return edge;
		for (var offset = 1; offset <= SearchRadius; offset++) {
			if (IsGridEdge(screenshot, edge + offset, start, length, vertical)) return edge + offset;
			if (IsGridEdge(screenshot, edge - offset, start, length, vertical)) return edge - offset;
		}
		return null;
	}

	private static bool IsGridEdge(Bitmap screenshot, int edge, int start, int length, bool vertical) {
		var edgeLimit = vertical ? screenshot.Width : screenshot.Height;
		var lineLimit = vertical ? screenshot.Height : screenshot.Width;
		var from = start + length / 4;
		var to = start + length - length / 4;
		if (edge < 0 || edge >= edgeLimit || from < 0 || to > lineLimit || from >= to) return false;

		var matches = 0;
		for (var i = from; i < to; i++) {
			var pixel = vertical ? screenshot.GetPixel(edge, i) : screenshot.GetPixel(i, edge);
			if (Math.Abs(pixel.R - GridColor.R) <= GridColorTolerance
			    && Math.Abs(pixel.G - GridColor.G) <= GridColorTolerance
			    && Math.Abs(pixel.B - GridColor.B) <= GridColorTolerance) matches++;
		}
		return matches * 4 >= (to - from) * 3;
	}
}
