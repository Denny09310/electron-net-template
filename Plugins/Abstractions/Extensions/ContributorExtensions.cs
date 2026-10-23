using Microsoft.Extensions.DependencyInjection;

public static class ContributorServiceExtensions
{
    public static IServiceCollection AddContributor<TContract, TImplementation>(
        this IServiceCollection services)
        where TContract : class
        where TImplementation : class, TContract
    {
        services.AddSingleton<TContract, TImplementation>();
        return services;
    }
}
