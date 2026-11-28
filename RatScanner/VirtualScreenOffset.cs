using System;

namespace RatScanner;

public class VirtualScreenOffset(int x, int y) {
	private sealed record Snapshot(int X, int Y, double DeviceScale);

	private volatile Snapshot _current = new(x, y, 1);

	public int XOffset => _current.X;
	public int YOffset => _current.Y;
	public double DeviceScale => _current.DeviceScale;

	internal event Action? Changed;

	internal void Update(int x, int y, double deviceScale) {
		var next = new Snapshot(x, y, double.IsFinite(deviceScale) && deviceScale > 0 ? deviceScale : 1);
		if (next == _current) return;

		_current = next;
		Changed?.Invoke();
	}

	internal void UpdateScale(double deviceScale) {
		var current = _current;
		Update(current.X, current.Y, deviceScale);
	}
}
