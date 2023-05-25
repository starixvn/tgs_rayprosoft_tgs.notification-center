using Google.Protobuf;
using NSubstitute;
using Tgs.Notification;
using Tgs.Notification.MessageQueue;
using Tgs.Notification.Protos;

namespace Notification.Core.UnitTests;

public class NotificationAcceptedHandlerTests
{
	[Fact]
	public async Task 處理回覆訊息發出()
	{
		// Arrange
		var fakeNotificationStore = Substitute.For<INotificationStore>();

		var sut = new NotificationAcceptedHandler(fakeNotificationStore);

		var request = new BroadcastNotificationAcceptedResponse
		{
			Id = Guid.NewGuid(),
			User = "test1"
		};

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeNotificationStore.Received(1)
			.RemoveNotificationAsync(
				Arg.Is(request.User),
				Arg.Is(request.Id.ToGuid()));
	}
}
