namespace Plugins.Abstractions;

/// <summary>
/// Contributes a complete menu to the host application's menubar.
/// </summary>
/// <remarks>
/// The contributed component controls its own menu trigger, content, nested
/// submenus, items, and actions. The host is responsible for rendering the
/// component within its menubar.
/// </remarks>
public interface IMenuContributor
{
    /// <summary>
    /// Gets the unique identifier of this menu contribution.
    /// </summary>
    /// <remarks>
    /// The identifier should be stable and unique among menu contributions.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the display order of this menu contribution.
    /// </summary>
    /// <remarks>
    /// Hosts can use this value to sort menu contributions. Lower values should
    /// appear before higher values.
    /// </remarks>
    int Order { get; }

    /// <summary>
    /// Gets the type of the component that renders the complete menu contribution.
    /// </summary>
    /// <remarks>
    /// The component is typically a Razor component containing the menu trigger
    /// and its associated dropdown content.
    /// </remarks>
    Type ComponentType { get; }
}
