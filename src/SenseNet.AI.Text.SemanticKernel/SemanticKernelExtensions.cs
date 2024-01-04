using Microsoft.Extensions.DependencyInjection;
using SenseNet.AI.Text;
using SenseNet.AI.Text.SemanticKernel;

namespace SenseNet.Extensions.DependencyInjection;

public static class SemanticKernelExtensions
{
    /// <summary>
    /// Adds the Semantic Kernel service to the service collection and the Summary provider feature to sensenet.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions">Configures AI values.</param>
    /// <returns></returns>
    public static IServiceCollection AddSenseNetSemanticKernel(this IServiceCollection services, 
        Action<SemanticKernelOptions>? configureOptions = null)
    {
        services.AddSingleton<ISummaryProvider, SummaryProvider>();
        services.AddSenseNetFeature<SummaryProvider>();
        services.Configure(configureOptions ?? (options => { }));
        services.AddLogging();

        return services;
    }
}
