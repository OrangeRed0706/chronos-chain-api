using System.Text.Json.Serialization;

namespace Chronos.Chain.Api.Model.Github.Response;

public class License
{
    public string Key { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("spdx_id")]
    public string SpdxId { get; set; }
    public string Url { get; set; }
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }
}
