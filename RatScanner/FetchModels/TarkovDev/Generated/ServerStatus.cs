using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ServerStatus
{
	[JsonProperty("generalStatus")]
	public Status GeneralStatus { get; set; }

	[JsonProperty("currentStatuses")]
	public List<Status> CurrentStatuses { get; set; }

	[JsonProperty("messages")]
	public List<StatusMessage> Messages { get; set; }
}
