using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.MessageQueue;
using Tgs.Notification.Protos;
using Tgs.NotificationCenter.ViewModels;
using Tgs.WebApi.Toolkits;

namespace Tgs.NotificationCenter.Controller;

[Route("[controller]")]
[ApiController]
public class TestController : ControllerBase
{
	[HttpPut("push", Name = "SendTestNotification")]
	[SuccessStatusCode(System.Net.HttpStatusCode.Accepted, "Notify request accepted")]
	public ValueTask SendTestNotificationAsync(
		[FromServices] IMessageSender messageSender,
		NotificationRequestViewModel viewModel)
	{
		var request = new NotificationRequest
		{
			User = viewModel.PlayerCode,
			Type = viewModel.Type,
			Payload = viewModel.Payload is null
				? string.Empty
				: JsonSerializer.Serialize(viewModel.Payload),
		};

		return messageSender.PublishAsync(
			"tgs.notify.push",
			request.ToByteArray(),
			HttpContext.RequestAborted);
	}
}
