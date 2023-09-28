using System.Text.Json;
using Google.Protobuf;
using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Notification.Client;

internal class NotificationClient : INotificationClient
{
	private readonly IMessageSender _messageSender;

	public NotificationClient(IMessageSender messageSender)
	{
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public ValueTask PushNotificationAsync(string user, string type, JsonDocument? payload, CancellationToken cancellationToken = default)
	{
		var request = new NotificationRequest
		{
			User = user,
			Type = type,
			Payload = payload is null
				? string.Empty
				: JsonSerializer.Serialize(payload)
		};

		return _messageSender.PublishAsync(
			"tgs.notify.push",
			request.ToByteArray(),
			cancellationToken);
	}

	public ValueTask AcceptedNotificationAsync(string user, Guid id, CancellationToken cancellationToken = default)
	{
		var request = new BroadcastNotificationAcceptedResponse
		{
			Id = id,
			User = user
		};

		return _messageSender.PublishAsync(
			"tgs.notify.accepted",
			request.ToByteArray(),
			cancellationToken);
	}

	public ValueTask ResendNotificationsAsync(string user, CancellationToken cancellationToken = default)
	{
		var request = new ResendNotificationRequest
		{
			User = user
		};

		return _messageSender.PublishAsync(
			"tgs.notify.resend",
			request.ToByteArray(),
			cancellationToken);
	}
}
