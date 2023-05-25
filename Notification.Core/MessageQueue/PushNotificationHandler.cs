using System.Text.Json;
using Google.Protobuf;
using Tgs.MessageQueue;
using Tgs.Notification.Protos;

namespace Tgs.Notification.MessageQueue;

internal class PushNotificationHandler : IMessageHandler
{
	private readonly INotificationStore _notificationStore;
	private readonly IMessageSender _messageSender;

	public PushNotificationHandler(INotificationStore notificationStore, IMessageSender messageSender)
	{
		_notificationStore = notificationStore ?? throw new ArgumentNullException(nameof(notificationStore));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = NotificationRequest.Parser.ParseFrom(data.Span);

		var notification = new PushNotification(
			Guid.NewGuid(),
			request.User,
			request.Type,
			string.IsNullOrEmpty(request.Payload)
				? null
				: JsonDocument.Parse(request.Payload));

		await _notificationStore.SaveNotificationAsync(
			notification,
			cancellationToken).ConfigureAwait(false);

		var broadcastRequest = new BroadcastNotificationRequest
		{
			Id = notification.Id,
			Type = notification.Type,
			User = notification.User,
			Payload = request.Payload
		};

		await _messageSender.PublishAsync(
			"tgs.notify.broadcast",
			broadcastRequest.ToByteArray(),
			cancellationToken).ConfigureAwait(false);
	}
}
