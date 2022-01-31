using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Custom converters needed for tarkovdata
namespace RatTracking.FetchModels.tarkovdata;

// Source: https://stackoverflow.com/questions/18994685/how-to-handle-both-a-single-item-and-an-array-for-the-same-property-using-json-n
internal class ObjectiveTargetOrArrayConverter<T> : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(List<T>);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var token = JToken.Load(reader);
		if (token.Type == JTokenType.Array) return token.ToObject<List<T>>();
		return new List<T> { token.ToObject<T>() };
	}

	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
