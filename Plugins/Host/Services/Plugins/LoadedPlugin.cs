using System.Reflection;

using Plugins.Abstractions;

namespace HostApp.Services;

/// <summary>
/// Describes a plugin loaded into its own assembly load context: its
/// instances, the loaded assembly, the context owning its lifetime and
/// its directory with static content root (when present).
/// </summary>
internal sealed record LoadedPlugin(
    IReadOnlyList<IPlugin> Instances,
    Assembly Assembly,
    PluginLoadContext LoadContext,
    string Directory,
    string? ContentRoot)
{
    /// <summary>
    /// Gets the assembly name under which the plugin's static content is
    /// served (<c>/_content/{assemblyName}/...</c>).
    /// </summary>
    public string AssemblyName =>
        Assembly.GetName().Name ?? Instances[0].Id;
}
