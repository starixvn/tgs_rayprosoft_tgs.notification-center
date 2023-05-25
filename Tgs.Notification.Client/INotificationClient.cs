using System.Text.Json;

namespace Tgs.Notification.Client;

public interface INotificationClient
{
	ValueTask AcceptedNotificationAsync(string user, Guid id, CancellationToken cancellationToken = default);

	ValueTask PushNotificationAsync(string user, string type, JsonDocument? payload, CancellationToken cancellationToken = default);

	ValueTask ResendNotificationsAsync(string user, CancellationToken cancellationToken = default);
}
