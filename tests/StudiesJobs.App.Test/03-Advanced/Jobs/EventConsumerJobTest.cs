using Microsoft.Extensions.Logging;
using Moq;
using StudiesJobs.App._03_Advanced.Jobs;
using StudiesJobs.App._03_Advanced.Models;
using StudiesJobs.App._03_Advanced.Queues;

namespace StudiesJobs.App.Test._03_Advanced.Jobs;

public class EventConsumerJobTest
{
    [Fact]
    public async Task ExecuteAsync_WhenItemIsReceived_ShouldProcessAndLogSuccessfully()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();
        var loggerMock = new Mock<ILogger<EventConsumerJob>>();
        var job = new EventConsumerJob(queue, loggerMock.Object);

        var item = new IntegrationItem(Guid.NewGuid(), "Valid Event Payload");
        await queue.WriteToQueueAsync(item);

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);

        await Task.Delay(100);

        cts.Cancel();

        await startTask;

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "Consumer job started and waiting for items...", Times.Once());
        VerifyLog(loggerMock, LogLevel.Information, $"<= CONSUMER: Processing event ID: {item.Id}", Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenIsCancelledBeforeStarting_ShouldStopImmediately()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();
        var loggerMock = new Mock<ILogger<EventConsumerJob>>();
        var job = new EventConsumerJob(queue, loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => job.StartAsync(cts.Token));

        VerifyLog(loggerMock, LogLevel.Information, "Consumer job started and waiting for items...", Times.Once());
        VerifyLog(loggerMock, LogLevel.Information, "<= CONSUMER: Processing event ID:", Times.Never());
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
