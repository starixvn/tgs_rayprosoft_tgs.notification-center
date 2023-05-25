using Tgs.MessageQueue;
using Tgs.Notification.Protos;

namespace Tgs.Notification.MessageQueue;

internal class NotificationAcceptedHandler : IMessageHandler
{
	private readonly INotificationStore _notificationStore;

	public NotificationAcceptedHandler(INotificationStore notificationStore)
	{
		_notificationStore = notificationStore ?? throw new ArgumentNullException(nameof(notificationStore));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = BroadcastNotificationAcceptedResponse.Parser.ParseFrom(data.Span);

		return _notificationStore.RemoveNotificationAsync(request.User, request.Id, cancellationToken);
	}
}
