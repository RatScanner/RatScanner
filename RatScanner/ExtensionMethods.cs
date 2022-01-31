using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RatScanner;

public static class ExtensionMethods
{
	/// <summary>
	/// Deep clone the object
	/// </summary>
	/// <typeparam name="T">Type of the object to be cloned</typeparam>
	/// <param name="a">The object to be cloned</param>
	/// <returns></returns>
	public static T DeepClone<T>(this T a)
	{
		using var stream = new MemoryStream();
		var formatter = new BinaryFormatter();
		formatter.Serialize(stream, a);
		stream.Position = 0;
		return (T)formatter.Deserialize(stream);
	}
}
