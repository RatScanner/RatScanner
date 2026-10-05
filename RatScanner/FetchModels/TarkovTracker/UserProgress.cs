using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovTracker;

// Model representing the progress data of a TarkovTracker user
public class UserProgress {
	[JsonProperty("userId")]
	public string UserId { get; set; } = "";

	[JsonProperty("displayName")]
	public string DisplayName { get; set; } = "Tarkov Citizen";

	/// <summary>PMC level as tracked by TarkovTracker. Null when the API omits the field.</summary>
	[JsonProperty("playerLevel")]
	public int? PlayerLevel { get; set; }

	/// <summary>PMC faction ("USEC" / "BEAR") as tracked by TarkovTracker.</summary>
	[JsonProperty("pmcFaction")]
	public string? PmcFaction { get; set; }

	/// <summary>Per-trader standing, keyed by trader id.</summary>
	[JsonProperty("tradersProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<TraderProgress> Traders { get; set; } = [];

	[JsonProperty("tasksProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> Tasks { get; set; } = [];

	[JsonProperty("taskObjectivesProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> TaskObjectives { get; set; } = [];

	[JsonProperty("hideoutModulesProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> HideoutModules { get; set; } = [];

	[JsonProperty("hideoutPartsProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> HideoutParts { get; set; } = [];

	/// <summary>
	/// Trader standing recorded for one trader. Both fields are optional because
	/// the API and a manual override only ever populate the ones the user knows.
	/// </summary>
	public class TraderProgress {
		[JsonProperty("id")]
		public string Id { get; set; } = "";

		/// <summary>Trader loyalty level (1-4).</summary>
		[JsonProperty("level")]
		public int? Level { get; set; }

		/// <summary>
		/// Trader reputation on the same negative-to-positive scale tarkov.dev gates
		/// on, so -1 is "below zero" and 4 is a high standing.
		/// </summary>
		/// <remarks>
		/// Fractional, not an integer: the loyalty levels tarkov.dev publishes need
		/// real reputation like 0.5, 2.5 and 6.5, and the quest gates compare
		/// against whole numbers, so both have to be representable in one field.
		/// </remarks>
		[JsonProperty("reputation")]
		public double? Reputation { get; set; }
	}
}
