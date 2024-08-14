using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.FetchModels.TarkovTracker;

// Model representing the progress data of a TarkovTracker user
public class UserProgress {
	[JsonProperty("userId")]
	public string UserId { get; set; } = "";

	[JsonProperty("displayName")]
	public string DisplayName { get; set; } = "Tarkov Citizen";

	[JsonProperty("tasksProgress")]
	public List<Progress> Tasks { get; set; } = new();

	[JsonProperty("taskObjectivesProgress")]
	public List<Progress> TaskObjectives { get; set; } = new();

	[JsonProperty("hideoutModulesProgress")]
	public List<Progress> HideoutModules { get; set; } = new();

	[JsonProperty("hideoutPartsProgress")]
	public List<Progress> HideoutParts { get; set; } = new();
}
