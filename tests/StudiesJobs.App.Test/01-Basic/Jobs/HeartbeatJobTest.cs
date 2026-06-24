using Microsoft.Extensions.Logging;
using Moq;
using StudiesJobs.App._01_Basic;
using System.Reflection;

namespace StudiesJobs.App.Test._01_Basic.Jobs;

public class HeartbeatJobTest
{
    [Fact]
    public async Task ExecuteAsync_WhenApplicationStarts_ShouldExecuteSuccessfullyAndLogMessages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HeartbeatJob>>();
        var job = new HeartbeatJob(loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await job.StartAsync(cts.Token);

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "Heartbeat Job started.", Times.Once());
        VerifyLog(loggerMock, LogLevel.Information, "Heartbeat Job finished.", Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoopRuns_ShouldLogExecutionAndRespectDelay()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HeartbeatJob>>();
        var job = new HeartbeatJob(loggerMock.Object);

        var periodField = typeof(HeartbeatJob).GetField("_period", BindingFlags.NonPublic | BindingFlags.Instance);
        periodField?.SetValue(job, TimeSpan.FromMilliseconds(1));

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(50); 

        cts.Cancel(); 
        await startTask;

        // Assert
        VerifyLog(loggerMock, LogLevel.Information, "Checking services status at:", Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutionFails_ShouldCatchExceptionAndLogError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HeartbeatJob>>();
        var job = new HeartbeatJob(loggerMock.Object);

        var periodField = typeof(HeartbeatJob).GetField("_period", BindingFlags.NonPublic | BindingFlags.Instance);
        periodField?.SetValue(job, TimeSpan.FromMilliseconds(1));

        loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Checking services status at:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
        .Throws(new InvalidOperationException("Simulated database timeout or infrastructure failure"));

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(30);

        cts.Cancel();
        await startTask;

        // Assert
        VerifyLog(loggerMock, LogLevel.Error, "Error while executing the Heartbeat Job.", Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoggerThrowsUnexpectedException_ShouldHandleAndContinueLoop()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HeartbeatJob>>();
        var job = new HeartbeatJob(loggerMock.Object);

        var periodField = typeof(HeartbeatJob).GetField("_period", BindingFlags.NonPublic | BindingFlags.Instance);
        periodField?.SetValue(job, TimeSpan.FromMilliseconds(1));

        loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Checking services status at:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
        .Throws(new InvalidOperationException("Simulated database/log link failure"));

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(40);
        
        cts.Cancel();
        await startTask;

        // Assert
        VerifyLog(loggerMock, LogLevel.Error, "Error while executing the Heartbeat Job.", Times.AtLeastOnce());
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
