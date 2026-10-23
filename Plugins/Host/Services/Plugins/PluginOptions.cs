namespace HostApp.Services;

public sealed record PluginOptions
{
    public bool Enabled { get; set; }
    public string Directory { get => Environment.ExpandEnvironmentVariables(field); init; } = default!;
}
