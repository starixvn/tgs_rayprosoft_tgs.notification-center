namespace Tgs.Notification.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type PushNotificationHandlerType = typeof(PushNotificationHandler);
	public static readonly Type NotificationAcceptedHandlerType = typeof(NotificationAcceptedHandler);
	public static readonly Type ResendNotificationHandlerType = typeof(ResendNotificationHandler);
}
