using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RatScanner.FetchModels;

namespace RatScanner
{
	public class QuestItem
	{
		public string Uid;
		public float Count;
		public Boolean inRaid;
	}
	public class MarketDB
	{
		private MarketItem[] _items;

		public void Init()
		{
			_items = ApiManager.GetMarketDB(RatConfig.NameScan.Language);

			var _questItems = LoadQuestJson();
// Behold; my inability to properly use lists!
			foreach(var item in _items)
			{
				foreach(var qi in _questItems)
				{
					if(qi.Uid == item.Uid)
					{
						item.Quest = true;
						item.QuestCount = (int)qi.Count;
						item.QuestInRaid = qi.inRaid;
						break;
					}
				}
			}
		}

		private QuestItem[] LoadQuestJson()
		{
			using StreamReader r = new StreamReader(RatConfig.Paths.QuestItemPath);
			string json = r.ReadToEnd();
			return JsonConvert.DeserializeObject<QuestItem[]>(json);
		}

		

		public MarketItem GetItemByUid(string uid)
		{
			if (uid?.Length > 0)
			{
				var item = _items.FirstOrDefault(i => i.Uid == uid);
				if (item != null) return item.DeepClone();

				Logger.LogWarning("Could not find item with uid: " + uid);
				return null;
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}

		public MarketItem GetItemByName(string name, bool useShortName, out float confidence, string regexFilter = null, bool caseSensitive = true)
		{
			// Filter name
			if (regexFilter != null) name = Regex.Replace(name, regexFilter, "");

			var bestMatchDistance = int.MaxValue;
			var bestMatchItem = _items[0];

			// Find best fit item
			foreach (var item in _items)
			{
				var compareName = item.Name;
				if (useShortName && item.ShortName != null) compareName = item.ShortName;

				// Filter compare name
				if (regexFilter != null) compareName = Regex.Replace(compareName, regexFilter, "");

				if (!caseSensitive)
				{
					name = name.ToLower();
					compareName = compareName.ToLower();
				}

				var distance = Distance(name, compareName);

				if (bestMatchDistance < distance) continue;

				var currentBestName = bestMatchItem.Name;
				if (useShortName && bestMatchItem.ShortName != null) currentBestName = bestMatchItem.ShortName;

				if (distance == 0 && bestMatchDistance == 0 && compareName.Length >= currentBestName.Length) continue;
				bestMatchItem = item;
				bestMatchDistance = distance;
			}

			confidence = bestMatchDistance;
			if (useShortName && bestMatchItem.ShortName != null) confidence /= bestMatchItem.ShortName.Length;
			else confidence /= bestMatchItem.Name.Length;
			confidence = 1f - confidence;

			return bestMatchItem.DeepClone();
		}

		public static int Distance(string value1, string value2)
		{
			if (value1.Length == 0) return 0;

			var costs = new int[value1.Length];

			// Add indexing for insertion to first row
			for (var i = 0; i < costs.Length;) costs[i] = ++i;

			var minSize = value1.Length < value2.Length ? value1.Length : value2.Length;

			for (var i = 0; i < minSize; i++)
			{
				// Cost of the first index
				var cost = i;
				var additionCost = i;

				// Cache value for inner loop to avoid index lookup and bonds checking, profiled this is quicker
				var value2Char = value2[i];

				for (var j = 0; j < value1.Length; j++)
				{
					var insertionCost = cost;
					cost = additionCost;

					// Assigning this here reduces the array reads we do
					additionCost = costs[j];

					if (value2Char != value1[j])
					{
						if (insertionCost < cost) cost = insertionCost;
						if (additionCost < cost) cost = additionCost;
						++cost;
					}
					costs[j] = cost;
				}
			}
			return costs[^1];
		}
	}
}
