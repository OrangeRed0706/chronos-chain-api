using Chronos.Chain.Api.DbContext;
using Chronos.Chain.Api.DbContext.Entities;
using Chronos.Chain.Api.Hub;
using Chronos.Chain.Api.Hub.Interface;
using Chronos.Chain.Api.Model;
using Chronos.Chain.Api.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Chain.Api.Controllers;

[ApiController]
[Route("api/deployment")]
public class DeploymentController : ControllerBase
{
    private readonly ILogger<DeploymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public DeploymentController(
        ILogger<DeploymentController> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _chatHubContext = chatHubContext;
    }

    [HttpGet(Name = "")]
    public async Task<IActionResult> GetTasks()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskContexts = await dbContext.TasksInfo.Select(x => new TaskContext
        {
            Id = x.Id,
            Name = x.Name,
            Creator = x.Creator,
            Description = x.Description,
            ProgressPercentage = x.ProgressPercentage,
            Timestamp = x.Timestamp,
            Status = x.Status,
        }).ToListAsync();

        return Ok(taskContexts);
    }

    [HttpPost(Name = "")]
    public async Task<IActionResult> CreateTask()
    {
        var random = new Random();
        var createUser = new string[] { "lynn", "jim" };
        Enumerable
            .Range(0, 5)
            .Select(i => new TaskContext
            {
                Id = Guid.NewGuid(),
                Name = $"Task:{i}",
                Creator = createUser[random.Next(0, 2)],
                Description = "Task Description",
            })
            .ToList()
            .ForEach(async taskContext =>
            {
                for (var i = 0; i <= 100; i++)
                {
                    taskContext.ProgressPercentage = i;
                    taskContext.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (i == 0)
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var _dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
                            _dbContext.TasksInfo.Add(new TaskInfo
                            {
                                Id = taskContext.Id,
                                Name = taskContext.Name,
                                Creator = taskContext.Creator,
                                Description = taskContext.Description,
                                ProgressPercentage = taskContext.ProgressPercentage,
                                Timestamp = taskContext.Timestamp,
                                Status = TaskState.Created,
                            });
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                    else if (i == 100)
                    {
                        taskContext.Status = TaskState.Completed;
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            await using var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

                            var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == taskContext.Id);
                            if (taskInfo == null)
                            {
                                return;
                            }

                            taskInfo.Status = TaskState.Completed;
                            taskInfo.ProgressPercentage = 100;
                            dbContext.Update(taskInfo);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        taskContext.Status = TaskState.Running;
                    }

                    await TaskHandler.TaskChannel.Writer.WriteAsync(taskContext);
                    await Task.Delay(random.Next(100, 500));
                }
            });
        return Ok();
    }

    [HttpPut("verify/{id}")]
    public async Task<IActionResult> UpdateTask(Guid id)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == id && x.Status == TaskState.WaitingVerification);
        if (taskInfo == null)
        {
            return NotFound();
        }
        taskInfo.Status = TaskState.Running;
        dbContext.Update(taskInfo);
        await dbContext.SaveChangesAsync();
        await TaskHandler.TaskChannel.Writer.WriteAsync(new TaskContext
        {
            Id = taskInfo.Id,
            Name = taskInfo.Name,
            Creator = taskInfo.Creator,
            Description = taskInfo.Description,
            ProgressPercentage = taskInfo.ProgressPercentage,
            Timestamp = taskInfo.Timestamp,
            Status = taskInfo.Status,
        });
        return Ok();
    }

    [HttpDelete("all" )]
    public async Task<IActionResult> DeleteTasks()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        dbContext.TasksInfo.RemoveRange(dbContext.TasksInfo);
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}
