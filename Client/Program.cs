using BlazorBlueprint.Components;
using ElectronNET.API;
using ElectronNET.API.Entities;
using ElectronNET.AspNet.Middleware;
using ElectronNET.AspNet.Services;

var builder = WebApplication.CreateBuilder(args);
builder.UseElectron(args, ElectronBootstrap);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddElectron();
builder.Services.AddBlazorBlueprintComponents();

builder.Services.AddSingleton<IElectronAuthenticationService, ElectronAuthenticationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseMiddleware<ElectronAuthenticationMiddleware>();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Client.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task ElectronBootstrap()
{
    var options = new BrowserWindowOptions
    {
        Show = false,
        IsRunningBlazor = true,

        TitleBarStyle = TitleBarStyle.hidden,
        TitleBarOverlay = new TitleBarOverlay()
        {
            Height = 32
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