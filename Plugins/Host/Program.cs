using Bit.Butil;

using BlazorBlueprint.Components;

using ElectronNET.API;
using ElectronNET.API.Entities;
using ElectronNET.AspNet.Middleware;
using ElectronNET.AspNet.Services;

using Host.Services;

using Plugins.Abstractions.Services;

var builder = WebApplication.CreateBuilder(args);
builder.UseElectron(args, ElectronBootstrap);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLocalization();

builder.Services.AddElectron();
builder.Services.AddBlazorBlueprintComponents();
builder.Services.AddBitButilServices();
builder.Services.AddPlugins(builder.Configuration);

builder.Services.AddSingleton<IElectronAuthenticationService, ElectronAuthenticationService>();
builder.Services.AddSingleton<ISettingsPersistenceService, SettingsPersistenceService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseMiddleware<ElectronAuthenticationMiddleware>();
app.UseAntiforgery();

app.MapStaticAssets();
app.UsePluginStaticAssets();
app.MapRazorComponents<Host.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddPluginsAssemblies(app.Services);

app.Run();

static async Task ElectronBootstrap(IServiceProvider services)
{
    // Apply the user's preferred culture before anything renders.
    await services
        .GetRequiredService<ILocalizationService>()
        .ApplyPreferredCultureAsync();

    var registry = services.GetRequiredService<PluginRegistry>();

    // 1. Initialize plugins after Electron's app-ready event.
    foreach (var plugin in registry.Plugins)
    {
        await plugin.InitializeAsync(services);
    }

    // 2. Register handlers once initialization has completed.
    foreach (var plugin in registry.Plugins)
    {
        plugin.RegisterHandlers(services);
    }

    // 3. Create the main window.
    var options = new BrowserWindowOptions
    {
        Show = false,
        IsRunningBlazor = true,

        TitleBarStyle = TitleBarStyle.hidden,
        TitleBarOverlay = new TitleBarOverlay
        {
            Height = 32
        },

        WebPreferences = new WebPreferences
        {
            Partition = "persist:{name_of_the_assembly_snake_case}"
        },

        Icon = Path.Combine(AppContext.BaseDirectory, "Assets", "appicon.ico")
    };

    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
    {
        options.AutoHideMenuBar = true;
    }

    var window = await Electron.WindowManager.CreateWindowAsync(options);

    window.OnReadyToShow += window.Show;
}
