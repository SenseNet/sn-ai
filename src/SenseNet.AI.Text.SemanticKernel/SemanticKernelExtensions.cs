using Microsoft.Extensions.DependencyInjection;
using SenseNet.AI.Text;
using SenseNet.AI.Text.SemanticKernel;

namespace SenseNet.Extensions.DependencyInjection;

public static class SemanticKernelExtensions
{
    /// <summary>
    /// Adds the Semantic Kernel service to the service collection and the 
    /// following features to sensenet: Summary provider, Content Query generator.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions">Configures AI values.</param>
    /// <returns></returns>
    public static IServiceCollection AddSenseNetSemanticKernel(this IServiceCollection services, 
        Action<SemanticKernelOptions>? configureOptions = null)
    {
        services.AddSingleton<ISummaryProvider, SummaryProvider>();
        services.AddSingleton<IContentQueryGenerator, ContentQueryGenerator>();
        services.AddSenseNetFeature<SummaryProvider>();
        services.Configure(configureOptions ?? (options => { }));
        services.AddLogging();

        return services;
    }
}
