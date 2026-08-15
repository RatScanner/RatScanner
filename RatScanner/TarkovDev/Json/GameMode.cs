using System.Runtime.Serialization;

namespace RatScanner;

public enum GameMode {
	[EnumMember(Value = "regular")]
	Regular,

	[EnumMember(Value = "pve")]
	Pve,

	[EnumMember(Value = "pvp-season")]
	PvpSeason,
}

internal static class GameModeExtensions {
	internal static string ToTranslationKey(this GameMode gameMode) => $"GameMode{gameMode}";

	internal static string ToApiString(this GameMode gameMode) {
		System.Type type = typeof(GameMode);
		System.Reflection.MemberInfo[] member = type.GetMember(gameMode.ToString());
		if (member.Length > 0) {
			object[] attributes = member[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
			if (attributes.Length > 0) {
				return ((EnumMemberAttribute)attributes[0]).Value!;
			}
		}
		return "regular";
	}
}
