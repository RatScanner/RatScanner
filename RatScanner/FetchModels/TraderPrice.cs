using System;

namespace RatScanner.FetchModels;

[Serializable]
public class TraderPrice
{
	public string TraderId;
	public int Price;

	public static readonly string[] TraderIds = new[]
	{
		"54cb57776803fa99248b456e",
		"58330581ace78e27b8b10cee",
		"5935c25fb3acc3127c3d8cd9",
		"54cb50c76803fa8b248b4571",
		"579dc571d53a0658a154fbec",
		"5a7c2eca46aef81a7ca2145d",
		"5ac3b934156ae10c4430e83c",
		"5c0647fdd443bc2504c2d371",
	};

	public string TraderName => GetTraderName(TraderId);

	public static string GetTraderName(string traderId)
	{
		return traderId switch
		{
			"54cb57776803fa99248b456e" => "Therapist",
			"58330581ace78e27b8b10cee" => "Skier",
			"5935c25fb3acc3127c3d8cd9" => "Peacekeeper",
			"54cb50c76803fa8b248b4571" => "Prapor",
			"579dc571d53a0658a154fbec" => "Fence",
			"5a7c2eca46aef81a7ca2145d" => "Mechanic",
			"5ac3b934156ae10c4430e83c" => "Ragman",
			"5c0647fdd443bc2504c2d371" => "Jaeger",
			"" => "",
			_ => "Unknown",
		};
	}
}
