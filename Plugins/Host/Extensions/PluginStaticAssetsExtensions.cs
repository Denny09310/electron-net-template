using Host.Services;

using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Serves each loaded plugin's static content from
/// <c>/_content/{assemblyName}/...</c>, matching the convention used for
/// Razor class libraries.
/// </summary>
internal static class PluginStaticAssetsExtensions
{
    public static WebApplication UsePluginStaticAssets(this WebApplication app)
    {
        var registry = app.Services.GetRequiredService<PluginRegistry>();

        foreach (var plugin in registry.Loaded)
        {
            if (plugin.ContentRoot is not { } contentRoot)
            {
                continue;
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentRoot),
                RequestPath = $"/_content/{plugin.AssemblyName}",
            });
        }

        return app;
    }
}
