using System.Drawing;

namespace RatScanner
{
	public class Vector2
	{
		public int X;
		public int Y;

		#region Constructors

		public Vector2(Point point) : this(point.X, point.Y) { }

		public Vector2(System.Windows.Point point) : this((int)point.X, (int)point.Y) { }

		public Vector2(OpenCvSharp.Point point) : this(point.X, point.Y) { }

		public Vector2(Size size) : this(size.Width, size.Height) { }

		public Vector2(OpenCvSharp.Size size) : this(size.Width, size.Height) { }

		#endregion

		public Vector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 Zero() => new Vector2(0, 0);
		public static Vector2 One() => new Vector2(1, 1);

		#region Implicit conversion from Vector2

		public static implicit operator OpenCvSharp.Point(Vector2 vec)
		{
			return new OpenCvSharp.Point(vec.X, vec.Y);
		}

		public static implicit operator System.Windows.Point(Vector2 vec)
		{
			return new System.Windows.Point(vec.X, vec.Y);
		}

		public static implicit operator Point(Vector2 vec)
		{
			return new Point(vec.X, vec.Y);
		}

		public static implicit operator Size(Vector2 vec)
		{
			return new Size(vec.X, vec.Y);
		}

		public static implicit operator OpenCvSharp.Size(Vector2 vec)
		{
			return new OpenCvSharp.Size(vec.X, vec.Y);
		}

		#endregion

		#region Implicit conversion to Vector2

		public static implicit operator Vector2(OpenCvSharp.Point point)
		{
			return new Vector2(point);
		}

		public static implicit operator Vector2(System.Windows.Point point)
		{
			return new Vector2(point);
		}

		public static implicit operator Vector2(Point point)
		{
			return new Vector2(point);
		}

		public static implicit operator Vector2(Size size)
		{
			return new Vector2(size);
		}

		public static implicit operator Vector2(OpenCvSharp.Size size)
		{
			return new Vector2(size);
		}

		#endregion

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

		public override string ToString()
		{
			return $"({X}, {Y})";
		}
	}
}
