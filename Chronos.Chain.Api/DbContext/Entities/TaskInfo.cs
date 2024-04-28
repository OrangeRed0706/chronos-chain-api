namespace Chronos.Chain.Api.DbContext.Entities;

public class TaskInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Creator { get; set; }
    public string Description { get; set; }
    public TaskState Status { get; set; }
    public int ProgressPercentage { get; set; }
    public long Timestamp { get; set; }
}

public enum TaskState : short
{
    Created = 0,
    Running = 1,
    WaitingVerification = 2,
    Completed = 3,
    Failed 4,
}
