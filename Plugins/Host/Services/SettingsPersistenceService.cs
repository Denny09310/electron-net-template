using System.Text.Json;

using ElectronNET.API;
using ElectronNET.API.Entities;

using Plugins.Abstractions.Services;

namespace Host.Services;

public sealed class SettingsPersistenceService : ISettingsPersistenceService
{
    private string? _filepath;

    private readonly SemaphoreSlim _gate = new(1, 1);

    private Dictionary<string, JsonElement> _items = [];
    private bool _loaded;

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureLoadedAsync(cancellationToken);

            if (!_items.TryGetValue(key, out var value))
                return default;

            try
            {
                return value.Deserialize<T>(JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                // The stored value cannot be deserialized as T.
                return default;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await EnsureLoadedAsync(cancellationToken);

            var items = new Dictionary<string, JsonElement>(_items)
            {
                [key] = JsonSerializer.SerializeToElement(value, JsonSerializerOptions.Web)
            };

            await SaveAsync(items, cancellationToken);

            _items = items;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync(
        CancellationToken cancellationToken = default)
    {
        if (_loaded)
            return;

        // Electron's userData folder is the storage root: the settings
        // document lives inside it as a plain file.
        _filepath ??= Path.Combine(
            await Electron.App.GetPathAsync(
                PathName.UserData,
                cancellationToken),
            "settings.json");

        Dictionary<string, JsonElement> items;

        try
        {
            await using var stream = new FileStream(
                _filepath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                options: FileOptions.Asynchronous |
                         FileOptions.SequentialScan);

            items = await JsonSerializer.DeserializeAsync<
                Dictionary<string, JsonElement>>(
                    stream,
                    JsonSerializerOptions.Web,
                    cancellationToken) ?? [];
        }
        catch (FileNotFoundException)
        {
            items = [];
        }
        catch (DirectoryNotFoundException)
        {
            items = [];
        }

        _items = items;
        _loaded = true;
    }

    private async Task SaveAsync(
        Dictionary<string, JsonElement> items,
        CancellationToken cancellationToken = default)
    {
        // Mirror the file resolution: store next to settings.json inside
        // the Electron userData folder (already resolved on first load).
        var root = Path.GetDirectoryName(_filepath);

        if (!string.IsNullOrWhiteSpace(root))
        {
            Directory.CreateDirectory(root);
        }

        // Use a temporary file in the same directory to reduce the
        // risk of leaving a partially written settings file.
        var temp = Path.Combine(
            root,
            $"settings.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                temp,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    items,
                    JsonSerializerOptions.Web,
                    cancellationToken);

                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temp, _filepath!, overwrite: true);
        }
        finally
        {
            try
            {
                File.Delete(temp);
            }
            catch (IOException)
            {
                // Best-effort cleanup of the temporary file.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup of the temporary file.
            }
        }
    }
}
