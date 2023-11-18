using Microsoft.Extensions.DependencyInjection;
using SenseNet.AI.Abstractions;
using SenseNet.AI.SemanticKernel;

namespace SenseNet.Extensions.DependencyInjection;

public static class SemanticKernelExtensions
{
    /// <summary>
    /// Adds the Semantic Kernel service to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, 
        Action<SemanticKernelOptions>? configureOptions = null)
    {
        services.AddSingleton<ITextService, TextService>();
        services.Configure(configureOptions ?? (options => { }));
        services.AddLogging();

        return services;
    }
}
