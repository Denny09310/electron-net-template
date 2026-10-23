namespace Plugins.Abstractions;

/// <summary>
/// Contributes a route to the host application's routing system.
/// </summary>
public interface IRouteContributor
{
    /// <summary>
    /// Gets the route template contributed by the plugin.
    /// </summary>
    /// <remarks>
    /// The format and interpretation of the route depend on the host application's
    /// routing implementation.
    /// </remarks>
    string Href { get; }
}
