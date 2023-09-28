using System.Text.Json;
using Google.Protobuf;
using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Notification.MessageQueue;

internal class ResendNotificationHandler : IMessageHandler
{
	private readonly INotificationStore _notificationStroe;
	private readonly IMessageSender _messageSender;

	public ResendNotificationHandler(INotificationStore notificationStroe, IMessageSender messageSender)
	{
		_notificationStroe = notificationStroe ?? throw new ArgumentNullException(nameof(notificationStroe));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
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

			await _messageSender.PublishAsync(
				"tgs.notify.broadcast",
				broadcastRequest.ToByteArray(),
				cancellationToken).ConfigureAwait(false);
		}
	}
}
