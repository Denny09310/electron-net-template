using System.Reflection;
using System.Runtime.Loader;

namespace HostApp.Services;

internal sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(
    name: Path.GetFileNameWithoutExtension(pluginPath),
    isCollectible: false)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);

        return path is null
            ? null
            : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return path is null
            ? IntPtr.Zero
            : LoadUnmanagedDllFromPath(path);
    }
}
