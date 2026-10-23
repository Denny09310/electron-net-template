namespace Plugins.Abstractions;

/// <summary>
/// Contributes a settings page or section to the host application's settings UI.
/// </summary>
public interface ISettingsContributor
{
    /// <summary>
    /// Gets the unique identifier of this settings contribution.
    /// </summary>
    /// <remarks>
    /// The identifier should remain stable and be unique among settings
    /// contributions.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the human-readable title of the settings page or section.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the type of the component that renders the settings page or section.
    /// </summary>
    /// <remarks>
    /// The component is typically a Razor component provided by the plugin.
    /// </remarks>
    Type ComponentType { get; }
}