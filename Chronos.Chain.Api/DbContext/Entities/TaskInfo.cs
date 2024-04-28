using Chronos.Chain.Api.Model.ViewModel;

namespace Chronos.Chain.Api.DbContext.Entities;

public class TaskInfo
{
    public Guid Id { get; set; }
    public int TaskActionId { get; set; }
    public TaskType Type { get; set; }
    public string Name { get; set; }
    public string Creator { get; set; }
    public string Description { get; set; }
    public TaskState Status { get; set; }
    public int ProgressPercentage { get; set; }
    public long Timestamp { get; set; }

    public TaskContext BuildToTaskContext()
    {
        return new TaskContext
        {
            Id = Id,
            ActionId = TaskActionId,
            Type = Type,
            Name = Name,
            Creator = Creator,
            Description = Description,
            Status = Status,
            ProgressPercentage = ProgressPercentage,
            Timestamp = Timestamp,
        };
    }
}

public enum TaskState : short
{
    Created = 0,
    Running = 1,
    WaitingVerification = 2,
    Completed = 3,
    Failed = 4,
}

public enum TaskType : int
{
    None = 0,
    Test = 99,
}
