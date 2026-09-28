using System.Runtime.Serialization;

namespace RatScanner.TarkovDev.Json;

public enum GameMode {
	[EnumMember(Value = "regular")]
	Regular,

	[EnumMember(Value = "pve")]
	Pve,

	[EnumMember(Value = "pvp-season")]
	PvpSeason,
}

internal static class GameModeExtensions {
	internal static string ToApiString(this GameMode gameMode) {
		var type = typeof(GameMode);
		var member = type.GetMember(gameMode.ToString());
		if (member.Length > 0) {
			var attributes = member[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
			if (attributes.Length > 0) {
				return ((EnumMemberAttribute)attributes[0]).Value!;
			}
		}
		return "regular";
	}
}
