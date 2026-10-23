# Electron.NET Template

A `dotnet new` template for an Electron.NET desktop application: Blazor +
ASP.NET Core backend. Pass `--plugins` to scaffold the **plugin-based**
variant: plugin discovery/loading into isolated assembly load contexts,
menu, settings and route contribution, per-plugin static assets,
localization and publish deployment.

## Usage

```shell
# plain application (no plugin infrastructure)
dotnet new electron-net -n MyApp

# plugin-based application
dotnet new electron-net -n MyApp --plugins true
```

The plugin-based variant ships a pre-wired solution (`MyApp.sln`) plus a
`Plugins` infrastructure ready to receive plugins:

```shell
cd MyApp

# scaffold a plugin into the host's Plugins directory
dotnet new electron-net-plugin -n Plugins.Notes

# (optional) add the plugin to the solution
dotnet sln add Plugins/Plugins.Notes/Plugins.Notes.csproj
```

Companion plugin template: [`electron-net-plugin-template`](../electron-net-plugin-template).

## Options

| Option      | Default  | Description                                                                                                            |
|-------------|----------|------------------------------------------------------------------------------------------------------------------------|
| `-n` / `--name` | (required) | Application (and solution) name, e.g. `MyApp`                                                                 |
| `--plugins` | `false`  | Scaffold the **plugin-based** variant: plugin loading, contributors, `Plugins.Abstractions` and publish deployment      |
| `--framework` | `net10.0` | Target framework (`net9.0` or `net10.0`)                                                                            |

## What the plugin-based variant (`--plugins true`) provides

```
MyApp/
├── MyApp.sln                       # pre-wired solution: host + Plugins.Abstractions
├── Program.cs                      # registers AddPlugins, UsePluginStaticAssets,
│                                   # AddPluginsAssemblies, applies the preferred
│                                   # culture and runs plugin lifecycle hooks before
│                                   # opening the window
├── Assets / Components / Electron / Properties / wwwroot
├── Services/Plugins/               # PluginRegistry, PluginLoadContext (isolated
│                                   # AssemblyLoadContext per plugin), LoadedPlugin
├── Services/                       # SettingsPersistenceService (settings.json inside
│                                   # Electron's userData folder), LocalizationService
├── Extensions/                     # AddPlugins / AddPluginsAssemblies /
│                                   # UsePluginStaticAssets (serves plugin content
│                                   # from /_content/{assemblyName})
├── Components/                     # Routes.razor + App.razor include plugin routes and
│                                   # plugin stylesheets (PluginStylesheets.razor);
│                                   # Titlebar.razor renders every IMenuContributor
├── Components/Pages/Settings.razor # settings page: language selector +
│                                   # a card per ISettingsContributor
├── Resources/Localization/         # en/it resx for the host UI
├── appsettings.json                # PluginOptions: Enabled + Directory
└── Plugins.Abstractions/           # IPlugin + IMenuContributor / IRouteContributor /
                                    # ISettingsContributor / ISettingsPersistenceService
Plugins/
├── Directory.Build.props           # compile-only package references for plugins
└── Directory.Build.targets         # dotnet publish deploys into
                                    # %LOCALAPPDATA%\{app}\plugins\{AssemblyName}
```

## How plugins work

1. Plugins are **published** (not copied by build) into
   `%LOCALAPPDATA%\{name_of_the_assembly_snake_case}\plugins\<AssemblyName>`.
2. At startup the host's `PluginRegistry` discovers each plugin folder
   (every `*.dll` that ships a `.deps.json`), loads it into its own
   `AssemblyLoadContext`, instantiates `IPlugin` implementations and calls
   `ConfigureServices`.
3. After Electron's `app-ready` event the host runs `InitializeAsync`, then
   `RegisterHandlers`, for every plugin, before creating the window.
4. Plugin assemblies flow into Blazor's router (`AddAdditionalAssemblies`),
   so plugin `@page` components are routable; their `IMenuContributor`
   components render inside the titlebar's menubar; their
   `ISettingsContributor` components render as cards of the host settings
   page; their `wwwroot` content is served under the standard
   `/_content/{assemblyName}` path.

## Packaging / publishing the host

The project ships with the usual Electron.NET packaging values
(`<ElectronVersion>`, `<ElectronBuilderVersion>`, `<ElectronIcon>` and
`win-x64`), so packaging behaves as configured by the Electron.NET
build targets.
