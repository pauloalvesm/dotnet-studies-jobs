using StudiesJobs.App._03_Advanced.Models;
using StudiesJobs.App._03_Advanced.Queues;

namespace StudiesJobs.App.Test._03_Advanced.Queues;

public class InMemoryIntegrationQueueTest
{
    [Fact]
    public async Task WriteToQueueAsync_And_ReadFromQueueAsync_ShouldProduceAndConsumeSuccessfully()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();
        var expectedItem = new IntegrationItem(Guid.NewGuid(), "Test Item");
        using var cts = new CancellationTokenSource();

        // Act
        await queue.WriteToQueueAsync(expectedItem);

        var iterator = queue.ReadFromQueueAsync(cts.Token).GetAsyncEnumerator();

        var hasItem = await iterator.MoveNextAsync();
        var resultItem = iterator.Current;

        // Assert
        Assert.True(hasItem);
        Assert.NotNull(resultItem);
        Assert.Equal(expectedItem.Id, resultItem.Id);
        Assert.Equal(expectedItem.Payload, resultItem.Payload); // Correção: Acessando 'Payload'
    }

    [Fact]
    public async Task ReadFromQueueAsync_WhenTokenIsCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var queue = new InMemoryIntegrationQueue();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        var iterator = queue.ReadFromQueueAsync(cts.Token).GetAsyncEnumerator();

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await iterator.MoveNextAsync();
        });
    }
}
