using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Notification.MessageQueue;

internal class NotificationAcceptedHandler(INotificationStore notificationStore) : IMessageHandler<ReadOnlyMemory<byte>>
{
	private readonly INotificationStore _notificationStore = notificationStore ?? throw new ArgumentNullException(nameof(notificationStore));

	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = BroadcastNotificationAcceptedResponse.Parser.ParseFrom(data.Span);

		return _notificationStore.RemoveNotificationAsync(request.User, request.Id, cancellationToken);
	}
}
