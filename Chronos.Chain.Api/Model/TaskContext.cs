using System.Text.Json.Serialization;

namespace Chronos.Chain.Api.Model;

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

}
