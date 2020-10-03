using System.Drawing;

namespace RatScanner
{
	public class Vector2
	{
		public int X;
		public int Y;

		public Vector2(Point point) : this(point.X, point.Y) { }

		public Vector2(OpenCvSharp.Point point) : this(point.X, point.Y) { }

		public Vector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 Zero() => new Vector2(0, 0);
		public static Vector2 One() => new Vector2(1, 1);

		public static implicit operator OpenCvSharp.Point(Vector2 vec)
		{
			return new OpenCvSharp.Point(vec.X, vec.Y);
		}

		public static implicit operator Point(Vector2 vec)
		{
			return new Point(vec.X, vec.Y);
		}

		public static implicit operator OpenCvSharp.Size(Vector2 vec)
		{
			return new OpenCvSharp.Size(vec.X, vec.Y);
		}

		public static implicit operator Size(Vector2 vec)
		{
			return new Size(vec.X, vec.Y);
		}

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}

		public static Vector2 operator +(Vector2 a, int b)
		{
			return new Vector2(a.X + b, a.Y + b);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X - b.X, a.Y - b.Y);
		}

		public static Vector2 operator -(Vector2 a, int b)
		{
			return new Vector2(a.X - b, a.Y - b);
		}

		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X * b.X, a.Y * b.Y);
		}

		public static Vector2 operator *(Vector2 a, int b)
		{
			return new Vector2(a.X * b, a.Y * b);
		}

		public static Vector2 operator /(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X / b.X, a.Y / b.Y);
		}

		public static Vector2 operator /(Vector2 a, int b)
		{
			return new Vector2(a.X / b, a.Y / b);
		}
	}
}
