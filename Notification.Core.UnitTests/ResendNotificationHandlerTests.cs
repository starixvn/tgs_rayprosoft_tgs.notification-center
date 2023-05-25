using System.Text.Json;
using Google.Protobuf;
using NSubstitute;
using Tgs.MessageQueue;
using Tgs.Notification;
using Tgs.Notification.MessageQueue;
using Tgs.Notification.Protos;

namespace Notification.Core.UnitTests;

public class ResendNotificationHandlerTests
{
	[Fact]
	public async Task 處理重送事件()
	{
		// Arrange
		var fakeNotificationStore = Substitute.For<INotificationStore>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new ResendNotificationHandler(fakeNotificationStore, fakeMessageSender);

		var request = new ResendNotificationRequest
		{
			User = "test1"
		};

		_ = fakeNotificationStore.LockResendProcessAsync(Arg.Is(request.User))
			.Returns(true);

		_ = fakeNotificationStore.GetNotificationsAsync(Arg.Is(request.User))
			.Returns(new[]
			{
				new PushNotification(Guid.NewGuid(), request.User, "Test", JsonDocument.Parse("{}"))
			}.ToAsyncEnumerable());

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.notify.broadcast"), Arg.Any<ReadOnlyMemory<byte>>());
	}
}
