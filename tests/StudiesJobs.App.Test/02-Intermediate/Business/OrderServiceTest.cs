using Microsoft.Extensions.Logging;
using Moq;
using StudiesJobs.App._02_Intermediate.Business;

namespace StudiesJobs.App.Test._02_Intermediate.Business;

public class OrderServiceTest
{
    [Fact]
    public async Task ProcessPendingOrdersAsync_WhenCalled_ShouldExecuteSuccessfullyAndLogMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = new OrderService(loggerMock.Object);

        // Act
        await service.ProcessPendingOrdersAsync();

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "Executing business logic for pending orders...", Times.Once());
    }

    [Fact]
    public async Task ProcessPendingOrdersAsync_WhenLoggerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = new OrderService(loggerMock.Object);

        loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing business logic for pending orders...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
        .Throws(new InvalidOperationException("Simulated logging failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessPendingOrdersAsync());
    }

    private static void VerifyLog<T>(Mock<ILogger<T>> loggerMock, LogLevel logLevel, string expectedMessage, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }
}
