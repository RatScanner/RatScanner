using System;
using System.Runtime.Serialization;

namespace RatScanner.TarkovTools;

[Serializable]
public class TarkovToolsRemoteControllerException : Exception
{
	public TarkovToolsRemoteControllerException()
	{
	}

	public TarkovToolsRemoteControllerException(string message) : base(message)
	{
	}

	public TarkovToolsRemoteControllerException(string message, Exception inner) : base(message, inner)
	{
	}

	protected TarkovToolsRemoteControllerException(
		SerializationInfo info,
		StreamingContext context) : base(info, context)
	{
	}
}
