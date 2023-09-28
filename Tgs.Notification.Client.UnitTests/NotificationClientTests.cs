using NSubstitute;
using Valhalla.MessageQueue;

namespace Tgs.Notification.Client.UnitTests;

public class NotificationClientTests
{
	[Fact]
	public async Task 發送推播()
	{
		// Arrange
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new NotificationClient(fakeMessageSender);

		var user = "test1";
		var type = "Test";

		// Act
		await sut.PushNotificationAsync(user, type, null);

		// Assert
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.notify.push"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 回覆已接收推播()
	{
		// Arrange
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new NotificationClient(fakeMessageSender);

		var id = Guid.NewGuid();
		var user = "test1";

		// Act
		await sut.AcceptedNotificationAsync(user, id);

		// Assert
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.notify.accepted"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 發出重送要求()
	{
		// Arrange
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new NotificationClient(fakeMessageSender);

		var user = "test1";

		// Act
		await sut.ResendNotificationsAsync(user);

		// Assert
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.notify.resend"), Arg.Any<ReadOnlyMemory<byte>>());
	}
}
