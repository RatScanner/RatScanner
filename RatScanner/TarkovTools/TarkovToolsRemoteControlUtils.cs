using System.Collections.Generic;
using System.Threading.Tasks;
using RatStash;

namespace RatScanner.TarkovTools;

public static class TarkovToolsRemoteControlUtils
{
	private static readonly IDictionary<(string, AmmoType?), string> AmmoMap = new Dictionary<(string, AmmoType?), string>
	{
		{("Caliber1143x23ACP", null), ".45 ACP"},
		{("Caliber127x55", null), "12.7x55 mm"},
		{("Caliber12g", AmmoType.Buckshot), "12 Gauge Shot"},
		{("Caliber12g", AmmoType.Bullet), "12 Gauge Slug"},
		{("Caliber20g", null), "20 Gauge"},
		{("Caliber23x75", null), "23x75 mm"},
		{("Caliber366TKM", null), ".366"},
		{("Caliber46x30", null), "4.6x30 mm"},
		{("Caliber545x39", null), "5.45x39 mm"},
		{("Caliber556x45NATO", null), "5.56x45 mm"},
		{("Caliber57x28", null), "5.7x28 mm"},
		{("Caliber762x25TT", null), "7.62x25 mm TT"},
		{("Caliber762x35", null), ".300 Blackout"},
		{("Caliber762x39", null), "7.62x39 mm"},
		{("Caliber762x51", null), "7.62x51 mm"},
		{("Caliber762x54R", null), "7.62x54R"},
		{("Caliber86x70", null), ".338 Lapua Magnum"},
		{("Caliber9x18PM", null), "9x18 mm"},
		{("Caliber9x19PARA", null), "9x19 mm"},
		{("Caliber9x21", null), "9x21 mm"},
		{("Caliber9x39", null), "9x39 mm"},
	};

	public static Task OpenRemoteTarkovToolsAsync(this ITarkovToolsRemoteController controller, Item? item, bool openAmmoChart)
	{
		if (item == null) return Task.CompletedTask;

		if (openAmmoChart
			&& item is Ammo ammo
			&& GetAmmoChartType(ammo) is { } ammoType)
		{
			return controller.OpenAmmoChartAsync(ammoType);
		}

		return controller.OpenItemAsync(item.Id);
	}

	private static string? GetAmmoChartType(Ammo ammo)
	{
		AmmoMap.TryGetValue((ammo.Caliber, null), out var result);
		AmmoMap.TryGetValue((ammo.Caliber, ammo.AmmoType), out var secondaryResult);
		return result ?? secondaryResult;
	}
}
