using StudiesJobs.App._03_Advanced.Models;
using System.Threading.Channels;

namespace StudiesJobs.App._03_Advanced.Queues;

public class InMemoryIntegrationQueue
{
    private readonly Channel<IntegrationItem> _queue;

    public InMemoryIntegrationQueue()
    {
        var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<IntegrationItem>(options);
    }

    public async ValueTask WriteToQueueAsync(IntegrationItem item) => await _queue.Writer.WriteAsync(item);
    public IAsyncEnumerable<IntegrationItem> ReadFromQueueAsync(CancellationToken cancellationToken) => _queue.Reader.ReadAllAsync(cancellationToken);
}
