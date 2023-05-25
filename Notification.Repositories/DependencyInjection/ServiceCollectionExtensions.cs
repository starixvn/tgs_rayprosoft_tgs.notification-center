using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Tgs.Notification.Repositories.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationRepositories(
		this IServiceCollection services,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
			.AddTransient<INotificationStore>(sp => ActivatorUtilities.CreateInstance<RedisNotificationStore>(
				sp,
				redisDatabaseResolver(sp)));
}
