using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RatScanner.Pages.InteractableOverlay.Services;

public class SearchService {
	/// <summary>
	/// Maps that share their artwork with another map and so duplicate an entry
	/// that is already listed.
	///
	/// The Lab, The Lab (Dark), Ground Zero and Ground Zero 21 all come back as
	/// separate results but point at the same map data as their base map, so
	/// searching showed the same artwork repeatedly under slightly different
	/// names. Matching is on <see cref="Map.NormalizedName"/> rather than the
	/// display name, because the name is translated and a translated string
	/// would not match.
	///
	/// The Labyrinth is deliberately NOT in this list: it is a distinct map that
	/// happens to share a name prefix with The Lab.
	/// </summary>
	private static readonly HashSet<string> HiddenMapIds = new(StringComparer.OrdinalIgnoreCase) {
		"the-lab-dark",
		"ground-zero-21",
		"ground-zero-tutorial",
	};

	public static async Task<IEnumerable<SearchResult>> SearchMapsAsync(string value) {
		if (string.IsNullOrEmpty(value)) return [];

		SearchResult? filter(Map map) {
			if (HiddenMapIds.Contains(map.NormalizedName)) return null;

			if (SanitizeSearch(map.Name) == value) return new(map, 3);
			if (SanitizeSearch(map.Name).StartsWith(value)) return new(map, 15);
			if (SanitizeSearch(map.Name).Contains(value)) return new(map, 45);
			return null;
		}

		List<SearchResult> matches = [];
		await Task.Run(() => {
			foreach (var map in TarkovDevAPI.GetMaps()) {
				var match = filter(map);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});
		return matches;
	}

	public static async Task<IEnumerable<SearchResult>> SearchTasksAsync(string value) {
		if (string.IsNullOrEmpty(value)) return [];

		SearchResult? filter(TarkovTask task) {
			if (SanitizeSearch(task.Name) == value) return new(task, 4);
			if (SanitizeSearch(task.Name).StartsWith(value)) return new(task, 10);
			var filters = value.Split([' ']);
			if (filters.All(filter => SanitizeSearch(task.Name).Contains(filter))) return new(task, 30);
			if (SanitizeSearch(task.Name).Contains(value)) return new(task, 50);
			if (value.Length > 3 && SanitizeSearch(task.Id).StartsWith(value)) return new(task, 80);
			if (value.Length > 3 && SanitizeSearch(task.Id).Contains(value)) return new(task, 100);
			return null;
		}

		List<SearchResult> matches = [];
		await Task.Run(() => {
			foreach (var task in TarkovDevAPI.GetTasks()) {
				var match = filter(task);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});
		return matches;
	}

	public static async Task<IEnumerable<SearchResult>> SearchItemsAsync(string value) {
		if (string.IsNullOrEmpty(value)) return [];

		SearchResult? filter(Item item) {
			if (SanitizeSearch(item.Name) == value) return new(item, 5);
			if (SanitizeSearch(item.ShortName) == value) return new(item, 10);
			if (SanitizeSearch(item.Name).StartsWith(value)) return new(item, 20);
			if (SanitizeSearch(item.ShortName).StartsWith(value)) return new(item, 20);
			var filters = value.Split([' ']);
			if (filters.All(filter => SanitizeSearch(item.Name).Contains(filter))) return new(item, 40);
			if (filters.All(filter => SanitizeSearch(item.ShortName).Contains(filter))) return new(item, 40);
			if (SanitizeSearch(item.Name).Contains(value)) return new(item, 60);
			if (SanitizeSearch(item.ShortName).Contains(value)) return new(item, 60);
			if (value.Length > 3 && SanitizeSearch(item.Id).StartsWith(value)) return new(item, 80);
			if (value.Length > 3 && SanitizeSearch(item.Id).Contains(value)) return new(item, 100);
			return null;
		}

		List<SearchResult> matches = [];
		await Task.Run(() => {
			foreach (var item in TarkovDevAPI.GetItems()) {
				var match = filter(item);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});

		for (var i = 0; i < matches.Count; i++) {
			if (matches[i].Data is not Item item) continue;
			matches[i].Score += (item.Name?.Length ?? 0) * 0.002;
			if (item.Types != null && item.Types.Contains("mods"))
				matches[i].Score += 5;
		}
		return matches;
	}

	public static string SanitizeSearch(string? value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		value = value.ToLower().Trim();
		value = value.Replace("-", " ");
		value = new string([.. value.Where(c => char.IsLetterOrDigit(c) || c == ' ')]);
		return value;
	}
}

public class SearchResult(object data, double score) {
	public object Data = data;
	public double Score = score;

	public override bool Equals(object? obj) {
		if (obj is not SearchResult other) return false;
		if (ReferenceEquals(this, other)) return true;
		return Data switch {
			Item item => other.Data is Item otherItem && otherItem.Id == item.Id,
			TarkovTask task => other.Data is TarkovTask otherTask && otherTask.Id == task.Id,
			Map map => other.Data is Map otherMap && otherMap.Id == map.Id,
			_ => ReferenceEquals(Data, other.Data),
		};
	}

	public override int GetHashCode() => Data switch {
		Item item => item.Id.GetHashCode(),
		TarkovTask task => task.Id.GetHashCode(),
		Map map => map.Id.GetHashCode(),
		_ => Data.GetHashCode(),
	};
}
