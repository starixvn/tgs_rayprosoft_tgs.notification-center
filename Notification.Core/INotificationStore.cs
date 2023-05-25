namespace Tgs.Notification;

public interface INotificationStore
{
	ValueTask SaveNotificationAsync(PushNotification notification, CancellationToken cancellationToken = default);

	ValueTask RemoveNotificationAsync(string user, Guid id, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PushNotification> GetNotificationsAsync(string user, CancellationToken cancellationToken = default);

	ValueTask<bool> LockResendProcessAsync(string user, CancellationToken cancellationToken = default);
}
