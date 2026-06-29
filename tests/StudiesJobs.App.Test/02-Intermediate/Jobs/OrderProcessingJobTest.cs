using Microsoft.Extensions.DependencyInjection;
using Moq;
using StudiesJobs.App._02_Intermediate;
using StudiesJobs.App._02_Intermediate.Interfaces;
using System.Reflection;

namespace StudiesJobs.App.Test._02_Intermediate.Jobs;

public class OrderProcessingJobTest
{
    [Fact]
    public async Task ExecuteAsync_WhenLoopRuns_ShouldProcessOrdersSuccessfully()
    {
        // Arrange
        var orderServiceMock = new Mock<IOrderService>();

        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        scopeServiceProviderMock
            .Setup(x => x.GetService(typeof(IOrderService)))
            .Returns(orderServiceMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(scopeServiceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(serviceScopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var job = new OrderProcessingJob(rootServiceProviderMock.Object);

        var periodField = typeof(OrderProcessingJob).GetField("_period", BindingFlags.NonPublic | BindingFlags.Instance);
        periodField?.SetValue(job, TimeSpan.FromMilliseconds(1));

        using var cts = new CancellationTokenSource();

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(50); 

        cts.Cancel();
        await startTask;

        // Assert
        orderServiceMock.Verify(x => x.ProcessPendingOrdersAsync(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderServiceFails_ShouldPropagateExceptionAndStopJob()
    {
        // Arrange
        var orderServiceMock = new Mock<IOrderService>();
        orderServiceMock
            .Setup(x => x.ProcessPendingOrdersAsync())
            .ThrowsAsync(new InvalidOperationException("Database failure"));

        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        scopeServiceProviderMock.Setup(x => x.GetService(typeof(IOrderService))).Returns(orderServiceMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);

        var job = new OrderProcessingJob(rootServiceProviderMock.Object);

        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => job.StartAsync(cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceIsNotRegistered_ShouldThrowException()
    {
        // Arrange
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        scopeServiceProviderMock.Setup(x => x.GetService(typeof(IOrderService))).Returns(null!);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);

        var job = new OrderProcessingJob(rootServiceProviderMock.Object);

        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => job.StartAsync(cts.Token));
    }
}
