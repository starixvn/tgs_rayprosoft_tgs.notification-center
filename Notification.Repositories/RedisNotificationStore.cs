using System.Text.Json;
using StackExchange.Redis;

namespace Tgs.Notification.Repositories;

internal class RedisNotificationStore : INotificationStore
{
	private static readonly RedisKey _NotificationKeyPrefix = "Tgs:Notification";
	private static readonly JsonSerializerOptions _JsonSerializerOptions = new(JsonSerializerDefaults.Web);
	private readonly IDatabase _redisDatabase;

	public RedisNotificationStore(IDatabase redisDatabase)
	{
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
	}

	public async ValueTask SaveNotificationAsync(PushNotification notification, CancellationToken cancellationToken = default)
	{
		var key = _NotificationKeyPrefix.Append(":").Append(notification.User);

		_ = await _redisDatabase.HashSetAsync(
			key,
			notification.Id.ToString(),
			JsonSerializer.Serialize(notification, _JsonSerializerOptions)).ConfigureAwait(false);
	}

	public IAsyncEnumerable<PushNotification> GetNotificationsAsync(string user, CancellationToken cancellationToken = default)
	{
		var key = _NotificationKeyPrefix.Append(":").Append(user);

		return _redisDatabase.HashGetAllAsync(key)
			.ToAsyncEnumerable()
			.SelectMany(entries => entries.ToAsyncEnumerable())
			.Where(entry => entry.Value.HasValue)
			.Select(entry => ((string)entry.Value)!)
			.Select(value => JsonSerializer.Deserialize<PushNotification>(value, _JsonSerializerOptions))
			.Where(notification => notification is not null)
			.Select(notification => notification!);
	}

	public async ValueTask RemoveNotificationAsync(string user, Guid id, CancellationToken cancellationToken = default)
	{
		var key = _NotificationKeyPrefix.Append(":").Append(user);

		_ = await _redisDatabase.HashDeleteAsync(
			key,
			id.ToString()).ConfigureAwait(false);
	}

	public async ValueTask<bool> LockResendProcessAsync(string user, CancellationToken cancellationToken = default)
	{
		var key = _NotificationKeyPrefix.Append(":").Append(user).Append(":").Append("Lock");

		return await _redisDatabase.StringSetAsync(
			key,
			Environment.MachineName,
			TimeSpan.FromSeconds(5d),
			when: When.NotExists).ConfigureAwait(false);
	}
}
