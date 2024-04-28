using Chronos.Chain.Api.DbContext;
using Chronos.Chain.Api.DbContext.Entities;
using Chronos.Chain.Api.Model.ViewModel;
using Chronos.Chain.Api.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Chain.Api.Controllers;

[ApiController]
[Route("api/deployment")]
public class DeploymentController : ControllerBase
{
    private readonly ILogger<DeploymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DeploymentController(
        ILogger<DeploymentController> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    [HttpGet(Name = "")]
    public async Task<IActionResult> GetTasks(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskContexts = await dbContext.TasksInfo.Select(x => x.BuildToTaskContext()).ToListAsync(cancellationToken: cancellationToken);
        return Ok(taskContexts);
    }

    [HttpPost(Name = "")]
    public async Task<IActionResult> CreateTaskTest(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
        var taskContext = new TaskContext
        {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            Creator = "lynn",
            Description = "Test Task Description",
            ProgressPercentage = 0,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Status = TaskState.Created,
            Type = TaskType.Test,
        };
        dbContext.TasksInfo.Add(taskContext.BuildToTaskInfo());
        await dbContext.SaveChangesAsync(cancellationToken);
        await TaskHandler.TaskChannel.Writer.WriteAsync(taskContext, cancellationToken);
        return Ok();
    }

    [HttpPost("retry")]
    public async Task<IActionResult> RetryTask(TaskContext taskContext, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskInfo = await dbContext.TasksInfo
            .Where(x => x.Type == taskContext.Type)
            .Where(x => x.Id == taskContext.Id)
            .FirstOrDefaultAsync(x => x.Id == taskContext.Id, cancellationToken: cancellationToken);
        if (taskInfo == null)
        {
            return NotFound();
        }
        if(taskInfo.TaskActionId != taskContext.ActionId)
        {
            _logger.LogWarning("Task action id not match");
            return BadRequest();
        }

        if (taskInfo.Status == TaskState.Completed)
        {
            return Ok("Task already completed");
        }
        await TaskHandler.TaskChannel.Writer.WriteAsync(taskInfo.BuildToTaskContext(), cancellationToken);

        return Ok();
    }

    [HttpPut("confirm")]
    public async Task<IActionResult> UpdateTask(TaskContext taskContext, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskInfo = await dbContext.TasksInfo
            .Where(x => x.Type == taskContext.Type)
            .Where(x => x.Id == taskContext.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (taskInfo == null)
        {
            _logger.LogWarning("Task not found");
            return NotFound();
        }

        if (taskInfo.TaskActionId > taskContext.ActionId)
        {
            _logger.LogWarning("Task already confirmed");
            await TaskHandler.TaskChannel.Writer.WriteAsync(taskInfo.BuildToTaskContext(), cancellationToken);
            return Ok();
        }

        taskInfo.Status = TaskState.Running;
        taskInfo.TaskActionId++;
        dbContext.Update(taskInfo);
        await dbContext.SaveChangesAsync(cancellationToken);
        await TaskHandler.TaskChannel.Writer.WriteAsync(taskInfo.BuildToTaskContext(), cancellationToken);
        return Ok();
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteTasks()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        dbContext.TasksInfo.RemoveRange(dbContext.TasksInfo);
        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(string id)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

        var taskInfo = await dbContext.TasksInfo.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
        if (taskInfo == null)
        {
            return NotFound();
        }

        dbContext.TasksInfo.Remove(taskInfo);
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}
