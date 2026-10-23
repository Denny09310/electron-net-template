using Microsoft.Extensions.DependencyInjection;

namespace Plugins.Abstractions;

/// <summary>
/// Represents a plugin that extends the host application's functionality.
/// </summary>
/// <remarks>
/// A plugin participates in the application lifecycle through service registration,
/// initialization, and handler registration.
/// </remarks>
public interface IPlugin
{
    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    /// <remarks>
    /// The identifier should remain stable across versions and be unique among
    /// all plugins loaded by the host.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the human-readable name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Registers the services required by the plugin in the dependency injection
    /// container.
    /// </summary>
    /// <param name="services">
    /// The service collection to which the plugin's service registrations are added.
    /// </param>
    /// <remarks>
    /// This method is called during application service registration, before the
    /// service provider is built. Implementations should register services and
    /// contributors but must not attempt to resolve services from the final provider.
    /// </remarks>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Registers the plugin's event handlers and other handlers that must be
    /// registered after the application has completed initialization.
    /// </summary>
    /// <param name="provider">
    /// The application's service provider, which can be used to resolve the
    /// services required to register handlers.
    /// </param>
    /// <remarks>
    /// This method is called after <see cref="InitializeAsync"/> has completed.
    /// Use it to register event handlers, callbacks, and other integrations that
    /// depend on the application already being ready.
    /// </remarks>
    void RegisterHandlers(IServiceProvider provider);

    /// <summary>
    /// Initializes the plugin when the host application is ready.
    /// </summary>
    /// <param name="provider">
    /// The application's service provider, which can be used to resolve
    /// dependencies required during initialization.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous initialization operation.
    /// </returns>
    /// <remarks>
    /// This method is conceptually equivalent to Electron's application
    /// <c>ready</c> event. It is called after the dependency injection container
    /// has been built and the application is ready for plugin initialization.
    /// Use it to perform startup tasks, initialize plugin state, and prepare
    /// resources required before registering post-initialization handlers.
    ///
    /// The host should await this method before calling
    /// <see cref="RegisterHandlers"/>.
    /// </remarks>
    Task InitializeAsync(IServiceProvider provider);
}