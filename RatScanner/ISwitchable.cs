namespace RatScanner;

public interface ISwitchable {
	public static ISwitchable? Instance { get; }

	public void UtilizeState(object state);

	public void OnClose();

	public void OnOpen();
}
