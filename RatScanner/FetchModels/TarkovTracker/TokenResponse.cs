using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.FetchModels.TarkovTracker;

// Model representing a TarkovTracker Token's metadata
public class TokenResponse
{
	// This token
	[JsonProperty("token")]
	public string Id { get; set; } = "";

	// An array of string representations of this token's permissions
	[JsonProperty("permissions")]
	public List<string> Permissions { get; set; } = new();
}
