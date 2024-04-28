using Chronos.Chain.Api.Model;

namespace Chronos.Chain.Api.Hub.Interface;

public interface IChatHub
{
    Task ClientReceiveMessage(string message);
    Task ClientReceiveTaskMessage(string taskContext);
}
