using System.Text.Json;
using System.Threading.Channels;
using Chronos.Chain.Api.Hub;
using Chronos.Chain.Api.Hub.Interface;
using Chronos.Chain.Api.Model;
using Microsoft.AspNetCore.SignalR;

namespace Chronos.Chain.Api.Worker;

public class TaskHandler : BackgroundService
{
    private readonly ILogger<TaskHandler> _logger;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    public static Channel<TaskContext> TaskChannel { get; set; }

    public TaskHandler(
        ILogger<TaskHandler> logger,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _logger = logger;
        _chatHubContext = chatHubContext;
        TaskChannel = Channel.CreateUnbounded<TaskContext>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () => { await ConsumeWithNestedWhileAsync(TaskChannel.Reader, stoppingToken); }, stoppingToken);
    }

    async ValueTask ConsumeWithNestedWhileAsync(ChannelReader<TaskContext> reader, CancellationToken cancellationToken)
    {
        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out var taskContext))
            {
                var json = JsonSerializer.Serialize(taskContext);
                await _chatHubContext.Clients.All.ClientReceiveTaskMessage(json);
            }
        }
    }
}
