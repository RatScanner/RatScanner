namespace RatScanner;

internal interface ISwitchable
{
	void UtilizeState(object state);

	void OnClose();

	void OnOpen();
}
