using Tgs.Notification.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationClient(this IServiceCollection services)
		=> services
			.AddTransient<INotificationClient, NotificationClient>();
}
