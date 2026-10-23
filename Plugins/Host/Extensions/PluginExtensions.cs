using Host.Services;

namespace Microsoft.Extensions.DependencyInjection;

internal static class PluginExtensions
{
    public static IServiceCollection AddPlugins(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(PluginOptions));

        services.AddOptions<PluginOptions>()
            .Bind(section)
            .Validate(
                options =>
                    !options.Enabled ||
                    !string.IsNullOrWhiteSpace(options.Directory),
                "PluginOptions:Directory is required when plugins are enabled.")
            .ValidateOnStart();

        var options = section.Get<PluginOptions>() ?? new PluginOptions();

        var registry = new PluginRegistry();

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.Directory))
            {
                throw new InvalidOperationException(
                    "PluginOptions:Directory must be set when plugins are enabled.");
            }

            registry.Load(options.Directory);
        }

        // Make the registry available to the rest of the application.
        services.AddSingleton(registry);

        foreach (var plugin in registry.Plugins)
        {
            // Register plugin-specific services.
            plugin.ConfigureServices(services);

            // Allow other services to inject IEnumerable<IPlugin>.
            services.AddSingleton(plugin);
        }

        return services;
    }

    public static RazorComponentsEndpointConventionBuilder
        AddPluginsAssemblies(
            this RazorComponentsEndpointConventionBuilder builder,
            IServiceProvider provider)
    {
        var registry = provider.GetRequiredService<PluginRegistry>();

        return builder.AddAdditionalAssemblies(
            [.. registry.Loaded.Select(plugin => plugin.Assembly)]);
    }
}
