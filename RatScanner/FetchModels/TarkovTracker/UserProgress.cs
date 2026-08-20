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

	[JsonProperty("tasksProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> Tasks { get; set; } = new();

	[JsonProperty("taskObjectivesProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> TaskObjectives { get; set; } = new();

	[JsonProperty("hideoutModulesProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> HideoutModules { get; set; } = new();

	[JsonProperty("hideoutPartsProgress", NullValueHandling = NullValueHandling.Ignore)]
	public List<Progress> HideoutParts { get; set; } = new();
}
