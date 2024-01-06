using Microsoft.Extensions.DependencyInjection;
using SenseNet.AI.Vision;
using SenseNet.AI.Vision.Azure;

namespace SenseNet.Extensions.DependencyInjection;

public static class AzureVisionExtensions
{
    /// <summary>
    /// Adds the Azure Vision API service to the service collection and 
    /// the Azure Vision provider feature to sensenet.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions">Configures AI values.</param>
    /// <returns></returns>
    public static IServiceCollection AddSenseNetAzureVision(this IServiceCollection services, 
        Action<AzureVisionOptions>? configureOptions = null)
    {
        services.AddSingleton<IImageGenerator, ImageGenerator>();
        services.AddSenseNetFeature<ImageGenerator>();
        services.Configure(configureOptions ?? (options => { }));
        services.AddLogging();

        return services;
    }
}
