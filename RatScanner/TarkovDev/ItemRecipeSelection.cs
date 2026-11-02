using RatScanner.TarkovDev.Json;
using System;
using System.Threading.Tasks;

namespace RatScanner.TarkovDev;

/// <summary>Owns asynchronous card state so late results cannot replace a newer selection.</summary>
public sealed class ItemRecipeSelection(
	Func<GameMode, Task<RecipeSources>> fetch,
	Func<RecipeSources, string, GameMode, ItemRecipeDetailsModel> present,
	Action<Exception> warn) {
	private int _version;
	public (string ItemId, GameMode Mode)? Selection { get; private set; }
	public ItemRecipeDetailsModel? Model { get; private set; }
	public bool Loading { get; private set; }

	public async Task LoadAsync(string itemId, GameMode mode) {
		if (Selection == (itemId, mode)) return;
		var version = ++_version;
		Selection = (itemId, mode);
		Model = null;
		Loading = true;
		try {
			var sources = await fetch(mode);
			if (_version == version) Model = present(sources, itemId, mode);
		} catch (Exception e) {
			if (_version == version) Model = new(new(new(null, false), new(null, false)), []);
			warn(e);
		} finally {
			if (_version == version) Loading = false;
		}
	}

	public void Clear() {
		_version++;
		Selection = null;
		Model = null;
		Loading = false;
	}
}
