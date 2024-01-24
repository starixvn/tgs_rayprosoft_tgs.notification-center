using System.Text.Json;
using Google.Protobuf;
using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Notification.MessageQueue;

internal class ResendNotificationHandler(INotificationStore notificationStroe, IMessageSender messageSender) : IMessageHandler<ReadOnlyMemory<byte>>
{
	private readonly INotificationStore _notificationStroe = notificationStroe ?? throw new ArgumentNullException(nameof(notificationStroe));
	private readonly IMessageSender _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));

	public async ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = ResendNotificationRequest.Parser.ParseFrom(data.Span);

		var isLocked = await _notificationStroe.LockResendProcessAsync(
			request.User,
			cancellationToken).ConfigureAwait(false);

		if (!isLocked)
			return;

		await foreach (var notify in _notificationStroe.GetNotificationsAsync(
			request.User,
			cancellationToken).ConfigureAwait(false))
		{
			var broadcastRequest = new BroadcastNotificationRequest
			{
				Id = notify.Id,
				Type = notify.Type,
				User = notify.User,
				Payload = JsonSerializer.Serialize(notify.Payload, JsonSerializerOptions.Default)
			};

			await _messageSender.PublishAsync<ReadOnlyMemory<byte>>(
				"tgs.notify.broadcast",
				broadcastRequest.ToByteArray(),
				cancellationToken).ConfigureAwait(false);
		}
	}
}
