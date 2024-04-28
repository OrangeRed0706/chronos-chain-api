using Chronos.Chain.Api.Hub;
using Chronos.Chain.Api.Hub.Interface;
using Chronos.Chain.Api.Model;
using Chronos.Chain.Api.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chronos.Chain.Api.Controllers;

[ApiController]
[Route("api/deployment")]
public class DeploymentController : ControllerBase
{
    private readonly ILogger<DeploymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public DeploymentController(
        ILogger<DeploymentController> logger,
        IConfiguration configuration,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _chatHubContext = chatHubContext;
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
                    await TaskHandler.TaskChannel.Writer.WriteAsync(taskContext);
                    await Task.Delay(random.Next(100, 500));
                }
            });
        return Ok();
    }
}
