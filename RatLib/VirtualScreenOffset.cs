using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatLib
{
	public class VirtualScreenOffset
	{
		public int XOffset { get; }
		public int YOffset { get; }

		public VirtualScreenOffset(int x, int y)
		{
			XOffset = x;
			YOffset = y;
		}
	}
}
