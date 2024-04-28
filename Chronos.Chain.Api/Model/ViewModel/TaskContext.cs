using System.Text.Json.Serialization;
using Chronos.Chain.Api.DbContext.Entities;

namespace Chronos.Chain.Api.Model.ViewModel;

public class TaskContext
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("creator")]
    public string Creator { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("progressPercentage")]
    public int ProgressPercentage { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("status")]
    public TaskState Status { get; set; }

    [JsonPropertyName("type")]
    public TaskType Type { get; set; }

    [JsonPropertyName("actionId")]
    public int ActionId { get; set; }

    public TaskInfo BuildToTaskInfo()
    {
        return new TaskInfo
        {
            Id = Id,
            TaskActionId = ActionId,
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
