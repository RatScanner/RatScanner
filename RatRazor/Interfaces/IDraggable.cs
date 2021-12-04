using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatRazor.Interfaces
{
	public interface IDraggable
	{
		public void DragMove();
		public static IDraggable? Instance { get; }
	}
}
