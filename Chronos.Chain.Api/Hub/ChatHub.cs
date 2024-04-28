using Chronos.Chain.Api.Hub.Interface;
using Microsoft.AspNetCore.SignalR;

namespace Chronos.Chain.Api.Hub;

public class ChatHub : Hub<IChatHub>
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task BroadcastMessage(string message)
    {
        _logger.LogInformation($"BoradcastMessage: {message}");
        await Clients.All.ClientReceiveMessage($"from server send:{message}");
    }
}
