using BlazorBlueprint.Components;
using ElectronNET.API;
using ElectronNET.API.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.UseElectron(args, ElectronBootstrap);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddElectron();
builder.Services.AddBlazorBlueprintComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
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
        TitleBarStyle = TitleBarStyle.hidden,
        TitleBarOverlay = new TitleBarOverlay()
        {
            Height = 32
        },
    };

    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
    {
        options.AutoHideMenuBar = true;
    }

    var window = await Electron.WindowManager.CreateWindowAsync(options);
    window.OnReadyToShow += window.Show;
}