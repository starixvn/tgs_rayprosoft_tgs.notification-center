using System.Text.Json;

namespace Tgs.NotificationCenter.ViewModels;

public class NotificationRequestViewModel
{
	public required string PlayerCode { get; set; }

	public required string Type { get; set; }

	public JsonDocument? Payload { get; set; }
}
