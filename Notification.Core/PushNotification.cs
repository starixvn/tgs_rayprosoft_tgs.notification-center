using System.Text.Json;

namespace Tgs.Notification;
public record PushNotification(
	Guid Id,
	string User,
	string Type,
	JsonDocument? Payload);
