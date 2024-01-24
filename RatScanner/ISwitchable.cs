namespace RatScanner;

public interface ISwitchable
{
	public static ISwitchable Instance { get; }

	void UtilizeState(object state);

	void OnClose();

	void OnOpen();
}
