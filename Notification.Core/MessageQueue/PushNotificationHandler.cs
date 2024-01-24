using System.Text.Json;
using Google.Protobuf;
using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Notification.MessageQueue;

internal class PushNotificationHandler(INotificationStore notificationStore, IMessageSender messageSender) : IMessageHandler<ReadOnlyMemory<byte>>
{
	private readonly INotificationStore _notificationStore = notificationStore ?? throw new ArgumentNullException(nameof(notificationStore));
	private readonly IMessageSender _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));

	public async ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
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

		await _messageSender.PublishAsync<ReadOnlyMemory<byte>>(
			"tgs.notify.broadcast",
			broadcastRequest.ToByteArray(),
			cancellationToken).ConfigureAwait(false);
	}
}
