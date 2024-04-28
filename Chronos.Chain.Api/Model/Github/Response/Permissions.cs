namespace Chronos.Chain.Api.Model.Github.Response;

public class Permissions
{
    public bool Admin { get; set; }
    public bool Maintain { get; set; }
    public bool Push { get; set; }
    public bool Triage { get; set; }
    public bool Pull { get; set; }
}
