using Microsoft.Extensions.Logging;
using Moq;
using StudiesJobs.App._03_Advanced.Jobs;
using StudiesJobs.App._03_Advanced.Models;
using StudiesJobs.App._03_Advanced.Queues;

namespace StudiesJobs.App.Test._03_Advanced.Jobs;

public class EventProducerJobTest
{
    [Fact]
    public async Task ExecuteAsync_WhenLoopRuns_ShouldProduceEventAndWriteToQueueSuccessfully()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();
        var loggerMock = new Mock<ILogger<EventProducerJob>>();
        var job = new EventProducerJob(queue, loggerMock.Object);

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);

        await Task.Delay(100);

        cts.Cancel();
        await startTask;

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "=> PRODUCER: Generating event and sending to internal queue.", Times.Once());
        var iterator = queue.ReadFromQueueAsync(CancellationToken.None).GetAsyncEnumerator();
        var hasItem = await iterator.MoveNextAsync();

        Assert.True(hasItem);
        Assert.NotNull(iterator.Current);
        Assert.Contains("Event payload number 1", iterator.Current.Payload);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenIsCancelledBeforeStarting_ShouldStopImmediatelyAndNotProduce()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();
        var loggerMock = new Mock<ILogger<EventProducerJob>>();
        var job = new EventProducerJob(queue, loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await job.StartAsync(cts.Token);

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "=> PRODUCER: Generating event and sending to internal queue.", Times.Never());

        var channelField = typeof(InMemoryIntegrationQueue).GetField("_queue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var channel = channelField?.GetValue(queue) as System.Threading.Channels.Channel<IntegrationItem>;

        var hasItem = channel!.Reader.TryRead(out _);
        Assert.False(hasItem);
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
