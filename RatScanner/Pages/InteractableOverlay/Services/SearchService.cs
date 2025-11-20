using RatScanner.TarkovDev.GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using TTask = RatScanner.TarkovDev.GraphQL.Task;

namespace RatScanner.Pages.InteractableOverlay.Services;

public class SearchService {
	public async System.Threading.Tasks.Task<IEnumerable<SearchResult>> SearchMapsAsync(string value) {
		if (string.IsNullOrEmpty(value)) return Enumerable.Empty<SearchResult>();

		Func<Map, SearchResult?> filter = (map) => {
			if (SanitizeSearch(map.Name) == value) return new(map, 3);
			if (SanitizeSearch(map.Name).StartsWith(value)) return new(map, 15);
			if (SanitizeSearch(map.Name).Contains(value)) return new(map, 45);
			return null;
		};

		List<SearchResult> matches = new();
		await System.Threading.Tasks.Task.Run(() => {
			foreach (var map in TarkovDevAPI.GetMaps()) {
				var match = filter(map);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});
		return matches;
	}

	public async System.Threading.Tasks.Task<IEnumerable<SearchResult>> SearchTasksAsync(string value) {
		if (string.IsNullOrEmpty(value)) return Enumerable.Empty<SearchResult>();

		Func<TTask, SearchResult?> filter = (task) => {
			if (SanitizeSearch(task.Name) == value) return new(task, 4);
			if (SanitizeSearch(task.Name).StartsWith(value)) return new(task, 10);
			string[] filters = value.Split(new[] { ' ' });
			if (filters.All(filter => SanitizeSearch(task.Name).Contains(filter))) return new(task, 30);
			if (SanitizeSearch(task.Name).Contains(value)) return new(task, 50);
			if (value.Length > 3 && SanitizeSearch(task.Id).StartsWith(value)) return new(task, 80);
			if (value.Length > 3 && SanitizeSearch(task.Id).Contains(value)) return new(task, 100);
			return null;
		};

		List<SearchResult> matches = new();
		await System.Threading.Tasks.Task.Run(() => {
			foreach (var task in TarkovDevAPI.GetTasks()) {
				var match = filter(task);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});
		return matches;
	}

	public async System.Threading.Tasks.Task<IEnumerable<SearchResult>> SearchItemsAsync(string value) {
		if (string.IsNullOrEmpty(value)) return Enumerable.Empty<SearchResult>();

		Func<Item, SearchResult?> filter = (item) => {
			if (SanitizeSearch(item.Name) == value) return new(item, 5);
			if (SanitizeSearch(item.ShortName) == value) return new(item, 10);
			if (SanitizeSearch(item.Name).StartsWith(value)) return new(item, 20);
			if (SanitizeSearch(item.ShortName).StartsWith(value)) return new(item, 20);
			string[] filters = value.Split(new[] { ' ' });
			if (filters.All(filter => SanitizeSearch(item.Name).Contains(filter))) return new(item, 40);
			if (filters.All(filter => SanitizeSearch(item.ShortName).Contains(filter))) return new(item, 40);
			if (SanitizeSearch(item.Name).Contains(value)) return new(item, 60);
			if (SanitizeSearch(item.ShortName).Contains(value)) return new(item, 60);
			if (value.Length > 3 && SanitizeSearch(item.Id).StartsWith(value)) return new(item, 80);
			if (value.Length > 3 && SanitizeSearch(item.Id).Contains(value)) return new(item, 100);
			return null;
		};

		List<SearchResult> matches = new();
		await System.Threading.Tasks.Task.Run(() => {
			foreach (var item in TarkovDevAPI.GetItems()) {
				var match = filter(item);
				if (match?.Data == null) continue;
				matches.Add(match);
			}
		});

		for (int i = 0; i < matches.Count; i++) {
			if (!(matches[i].Data is Item item)) continue;
			matches[i].Score += (item.Name?.Length ?? 0) * 0.002;
			if (item.Types != null && item.Types.Contains(ItemType.Mods))
				matches[i].Score += 5;
		}
		return matches;
	}

	public string SanitizeSearch(string? value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		value = value.ToLower().Trim();
		value = value.Replace("-", " ");
		value = new string(value.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
		return value;
	}
}

public class SearchResult {
	public SearchResult(object data, float score) {
		Score = score;
		Data = data;
	}
	public object Data;
	public double Score;
}
