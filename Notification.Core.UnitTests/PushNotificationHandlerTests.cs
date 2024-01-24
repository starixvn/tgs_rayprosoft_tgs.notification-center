using Google.Protobuf;
using NSubstitute;
using Tgs.Notification;
using Tgs.Notification.MessageQueue;
using Tgs.Notification.Protos;
using Valhalla.MessageQueue;

namespace Notification.Core.UnitTests;

public class PushNotificationHandlerTests
{
	[Fact]
	public async Task 處理推播要求()
	{
		// Arrange
		var fakeNotificationStore = Substitute.For<INotificationStore>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new PushNotificationHandler(fakeNotificationStore, fakeMessageSender);

		var request = new NotificationRequest
		{
			User = "test1",
			Type = "Test",
			Payload = """{}"""
		};

		// Act
		await sut.HandleAsync("test", request.ToByteArray(), null);

		// Assert
		_ = fakeNotificationStore.Received(1)
			.SaveNotificationAsync(Arg.Any<PushNotification>());
		_ = fakeMessageSender.Received(1)
			.PublishAsync(
				Arg.Is("tgs.notify.broadcast"),
				Arg.Any<ReadOnlyMemory<byte>>());
	}
}
