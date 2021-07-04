using System;
using System.Linq;
using RatStash;

namespace RatScanner
{
	/// <summary>
	/// Minimal item class used primarily for icon related code
	/// </summary>
	public class ItemInfo
	{
		/// <summary>
		/// Id of the item
		/// </summary>
		public readonly string Id;

		/// <summary>
		/// Mod id's of the item
		/// </summary>
		public readonly string[] Mods;

		/// <summary>
		/// Metadata to distinguish items with folded stocks or other states
		/// </summary>
		public readonly string Meta;

		public bool HasMods => Mods?.Length > 0;

		public ItemInfo(string id, string[] mods = null, string meta = null)
		{
			Id = id;
			Mods = mods;
			Meta = meta;
		}

		public Item GetItem()
		{
			try
			{
				var itemDB = RatScannerMain.Instance.ItemDB;

				var baseItem = itemDB.GetItem(Id);
				if (baseItem == null) return null;

				if (HasMods)
				{
					var mods = Mods.Select(modUid => itemDB.GetItem(modUid));
					if (baseItem is CompoundItem baseItemC)
					{
						baseItemC.Slots.Clear();
						foreach (var mod in mods) baseItemC.Slots.Add(new Slot() {ContainedItem = mod});
					}
				}

				// TODO Oh god no why do I do this
				// URGENT!!! Fix this when adding RatEye
				baseItem.ItemSound = Meta?.Length > 0 ? "$meta$" + Meta : null;

				return baseItem;
			}
			catch (Exception e)
			{
				Logger.LogWarning("Failed to find market item for:\n" + this, e);
				return null;
			}
		}

		public static implicit operator ItemInfo(Item item)
		{
			// TODO Oh god no why do I do this
			// URGENT!!! Fix this when adding RatEye
			var meta = item.ItemSound?.StartsWith("$meta$") == true ? item.ItemSound.Substring(6) : null;

			if (item is CompoundItem itemC)
			{
				if (!(itemC.Slots.Count > 0)) return new ItemInfo(item.Id, null, meta);

				var mods = itemC.Slots.Select(slot => slot.ContainedItem?.Id).ToArray();

				return new ItemInfo(item.Id, mods, meta);
			}

			return new ItemInfo(item.Id, null, meta);
		}

		public override string ToString()
		{
			if (HasMods) return "Id: " + Id + string.Join("\nMod: ", Mods);
			else return "Id: " + Id;
		}

		public override int GetHashCode()
		{
			if (!HasMods) return Id.GetHashCode();
			var modUidNotNullOrEmpty = Mods.Where(mod => !string.IsNullOrEmpty(mod));
			return (Id + string.Join("", modUidNotNullOrEmpty)).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ItemInfo icon)) return false;

			if (icon.Meta != Meta) return false;
			if (icon.Id != Id) return false;
			if (icon.HasMods != HasMods) return false;
			if (icon.Mods != null && Mods != null) return icon.Mods.SequenceEqual(Mods);
			return true;
		}
	}
}
