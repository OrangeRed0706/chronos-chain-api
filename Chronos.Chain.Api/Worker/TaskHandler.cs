using System.Text.Json;
using System.Threading.Channels;
using Chronos.Chain.Api.DbContext;
using Chronos.Chain.Api.DbContext.Entities;
using Chronos.Chain.Api.Hub;
using Chronos.Chain.Api.Hub.Interface;
using Chronos.Chain.Api.Model;
using Chronos.Chain.Api.Model.ViewModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Chain.Api.Worker;

public class TaskHandler : BackgroundService
{
    private readonly ILogger<TaskHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    public static Channel<TaskContext> TaskChannel { get; set; }

    public TaskHandler(
        ILogger<TaskHandler> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
                await _chatHubContext.Clients.All.ClientReceiveTaskMessage(JsonSerializer.Serialize(taskContext));
                if (taskContext.Type == TaskType.Test)
                {
                    await HandleTestTypeAsync(taskContext, cancellationToken);
                }
                else
                {
                    var json = JsonSerializer.Serialize(taskContext);
                    await _chatHubContext.Clients.All.ClientReceiveTaskMessage(json);
                }
            }
        }
    }

    private async Task HandleTestTypeAsync(TaskContext taskContext, CancellationToken cancellationToken)
    {
        var random = new Random();
        if (taskContext.ActionId == 0)
        {
            for (var i = 0; i <= 20; i++)
            {
                await Task.Delay(random.Next(10, 50), cancellationToken);
                taskContext.Status = TaskState.Running;
                taskContext.ProgressPercentage = i;
                var json = JsonSerializer.Serialize(taskContext);
                await _chatHubContext.Clients.All.ClientReceiveTaskMessage(json);
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
            var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == taskContext.Id && x.TaskActionId == taskContext.ActionId, cancellationToken: cancellationToken);
            if (taskInfo == null)
            {
                return;
            }
            taskContext.Status = TaskState.WaitingVerification;
            taskInfo.Status = TaskState.WaitingVerification;
            taskInfo.ProgressPercentage = 20;
            dbContext.Update(taskInfo);
            await dbContext.SaveChangesAsync(cancellationToken);
            await _chatHubContext.Clients.All.ClientReceiveTaskMessage(JsonSerializer.Serialize(taskContext));

            return;
        }


        if (taskContext.ActionId == 1)
        {
            for (var i = 20; i <= 40; i++)
            {
                taskContext.Status = TaskState.Running;
                taskContext.ProgressPercentage = i;
                var json = JsonSerializer.Serialize(taskContext);
                await _chatHubContext.Clients.All.ClientReceiveTaskMessage(json);
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
            var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == taskContext.Id && x.TaskActionId == taskContext.ActionId, cancellationToken: cancellationToken);
            if (taskInfo == null)
            {
                return;
            }
            taskContext.Status = TaskState.WaitingVerification;
            taskInfo.Status = TaskState.WaitingVerification;
            taskInfo.ProgressPercentage = 40;
            dbContext.Update(taskInfo);
            await dbContext.SaveChangesAsync(cancellationToken);
            await _chatHubContext.Clients.All.ClientReceiveTaskMessage(JsonSerializer.Serialize(taskContext));

            return;
        }

        if (taskContext.ActionId == 2)
        {
            for (var i = 40; i <= 100; i++)
            {
                taskContext.Status = TaskState.Running;
                taskContext.ProgressPercentage = i;
                var json = JsonSerializer.Serialize(taskContext);
                await _chatHubContext.Clients.All.ClientReceiveTaskMessage(json);
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
            var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == taskContext.Id && x.TaskActionId == taskContext.ActionId, cancellationToken: cancellationToken);
            if (taskInfo == null)
            {
                return;
            }
            taskContext.Status = TaskState.Completed;
            taskInfo.Status = TaskState.Completed;
            taskInfo.ProgressPercentage = 100;
            dbContext.Update(taskInfo);
            await dbContext.SaveChangesAsync(cancellationToken);
            await _chatHubContext.Clients.All.ClientReceiveTaskMessage(JsonSerializer.Serialize(taskContext));

        }
    }
}
