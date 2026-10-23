using System.Reflection;

using Plugins.Abstractions;

namespace HostApp.Services;

/// <summary>
/// Discovers plugins under the configured directory, loads them into
/// isolated assembly load contexts and exposes the resulting
/// <see cref="LoadedPlugin"/> descriptors to the rest of the application.
/// </summary>
internal sealed class PluginRegistry
{
    private readonly List<LoadedPlugin> _loaded = [];

    /// <summary>Gets every plugin discovered and loaded at startup.</summary>
    public IReadOnlyList<LoadedPlugin> Loaded => _loaded;

    /// <summary>Gets every <see cref="IPlugin"/> instance, in load order.</summary>
    public IEnumerable<IPlugin> Plugins =>
        _loaded.SelectMany(plugin => plugin.Instances);

    internal void Load(string directory)
    {
        var root = Path.GetFullPath(directory);

        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException(
                $"Plugin directory was not found: {root}");
        }

        var paths = Directory
            .EnumerateFiles(root, "*.dll", SearchOption.AllDirectories)
            .Where(path => File.Exists(Path.ChangeExtension(path, ".deps.json")))
            .ToArray();

        foreach (var path in paths)
        {
            var pluginDirectory = Path.GetDirectoryName(path);

            var loadContext = new PluginLoadContext(path);

            var assembly = loadContext.LoadFromAssemblyPath(
                Path.GetFullPath(path));

            var instances = GetLoadableTypes(assembly)
                .Where(type =>
                    type is { IsAbstract: false, IsInterface: false } &&
                    typeof(IPlugin).IsAssignableFrom(type))
                .Select(CreateInstance)
                .ToArray();

            if (instances.Length == 0)
            {
                continue;
            }

            EnsureUniqueIds(instances);

            string? contentRoot = null;

            if (pluginDirectory is not null)
            {
                var candidate = Path.Combine(pluginDirectory, "wwwroot");

                if (Directory.Exists(candidate))
                {
                    contentRoot = candidate;
                }
            }

            _loaded.Add(new LoadedPlugin(
                Instances: instances,
                Assembly: assembly,
                LoadContext: loadContext,
                Directory: pluginDirectory ?? root,
                ContentRoot: contentRoot));
        }
    }

    private static IPlugin CreateInstance(Type type)
    {
        return Activator.CreateInstance(type) as IPlugin
            ?? throw new InvalidOperationException(
                $"Plugin '{type.FullName}' must have a public " +
                "parameterless constructor.");
    }

    private void EnsureUniqueIds(IPlugin[] instances)
    {
        for (var index = 0; index < instances.Length; index++)
        {
            var plugin = instances[index];

            var duplicate = _loaded
                .SelectMany(existing => existing.Instances)
                .Concat(instances.Take(index))
                .Any(existing =>
                    string.Equals(
                        existing.Id,
                        plugin.Id,
                        StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                throw new InvalidOperationException(
                    $"Duplicate plugin ID: '{plugin.Id}'.");
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }
}
