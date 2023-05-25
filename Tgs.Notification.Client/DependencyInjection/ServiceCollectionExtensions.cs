using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Notification.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationClient(this IServiceCollection services)
		=> services
			.AddTransient<INotificationClient, NotificationClient>();
}
