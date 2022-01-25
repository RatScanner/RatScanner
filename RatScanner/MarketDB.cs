using RatScanner.FetchModels;
using System;
using System.Linq;

namespace RatScanner;

public class MarketDB
{
	private MarketItem[] _items;

	public void Init()
	{
		_items = ApiManager.GetMarketDB();
	}

	public MarketItem GetItemById(string uid)
	{
		if (uid?.Length > 0)
		{
			var item = _items.FirstOrDefault(i => i.Id == uid);
			if (item != null) return item.DeepClone();

			Logger.LogWarning("Could not find item with uid: " + uid);
			return null;
		}

		Logger.LogWarning("Trying to get item without supplying uid");
		throw new ArgumentException();
	}
}
