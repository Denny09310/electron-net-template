using System.Globalization;

using Plugins.Abstractions.Services;

namespace HostApp.Services;

public interface ILocalizationService
{
    /// <summary>
    /// Gets the cultures the application can be displayed in.
    /// </summary>
    IReadOnlyList<CultureInfo> SupportedCultures { get; }

    /// <summary>
    /// Gets the preferred culture stored in the application settings, or
    /// <c>null</c> when the user has not chosen one yet.
    /// </summary>
    Task<string?> GetPreferredCultureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the preferred culture in the application settings and applies
    /// it to the running application.
    /// </summary>
    Task SetPreferredCultureAsync(string cultureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the preferred culture from the application settings and applies
    /// it to the running application.
    /// </summary>
    Task ApplyPreferredCultureAsync(CancellationToken cancellationToken = default);
}

public sealed class LocalizationService(ISettingsPersistenceService persistence) : ILocalizationService
{
    private const string PreferenceKey = "preferred_culture";

    private static readonly CultureInfo[] Cultures =
    [
        new("en"),
        new("it"),
    ];

    public IReadOnlyList<CultureInfo> SupportedCultures => Cultures;

    public async Task<string?> GetPreferredCultureAsync(CancellationToken cancellationToken = default)
    {
        return await persistence.GetAsync<string>(PreferenceKey, cancellationToken);
    }

    public async Task SetPreferredCultureAsync(string cultureName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);

        var culture = ResolveCulture(cultureName);

        Apply(culture);

        await persistence.SetAsync(PreferenceKey, culture.Name, cancellationToken);
    }

    public async Task ApplyPreferredCultureAsync(CancellationToken cancellationToken = default)
    {
        var cultureName = await persistence.GetAsync<string>(PreferenceKey, cancellationToken);

        Apply(ResolveCulture(cultureName));
    }

    private static void Apply(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static CultureInfo ResolveCulture(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return Cultures[0];

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);

            return Cultures.Any(supported =>
                supported.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase))
                ? culture
                : Cultures[0];
        }
        catch (CultureNotFoundException)
        {
            return Cultures[0];
        }
    }
}
