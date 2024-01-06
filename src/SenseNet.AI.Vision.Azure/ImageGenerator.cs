using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Vision.Azure;

/// <inheritdoc/>
public class ImageGenerator(IOptions<AzureVisionOptions> options, 
    ILogger<ImageGenerator> logger) : IImageGenerator, ISnFeature
{
    #region ISnFeature implementation

    public string Name => "AzureVision";
    public string DisplayName => "Azure Vision";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(_options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    private readonly AzureVisionOptions _options = options.Value;

    public async Task<ImageData> GenerateImage(string text, int width, int height, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(_options.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not set.");

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(nameof(text));
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        
        // calculate new image width and height to be within the limits
        CalculateDimensions(ref width, ref height);

        logger.LogTrace("Generating image using the '{model}' model with dimensions {width} x {height}. Prompt: {text}", 
            _options.Model, width, height, 
            text.Length <= 500 ? text : text.Substring(0, 500));

        // If the Azure endpoint is not set, the default OpenAI endpoint 
        // will be used. If it is set, we pass it on to the client.

        OpenAIClient client = 
            string.IsNullOrWhiteSpace(_options.AzureEndpoint) 
                ? new(_options.OpenAiApiKey) 
                : new(new Uri(_options.AzureEndpoint), 
                    new AzureKeyCredential(_options.OpenAiApiKey!));

        Response<ImageGenerations> imageGenerations = await client.GetImageGenerationsAsync(
            new ImageGenerationOptions()
            {
                DeploymentName = _options.Model,
                Prompt = text,
                Size = new ImageSize($"{width}x{height}"),
            });

        return new ImageData
        {
            Url = imageGenerations.Value.Data[0].Url.ToString()
        };
    }

    static readonly int[] DimensionThresholds = [256, 512, 1024];

    private static void CalculateDimensions(ref int width, ref int height)
    {      
        // Choose a dimension that is equal to or slightly larger than the given one.
        // Available dimension are: '256x256', '512x512', '1024x1024', '1024x1792', '1792x1024'

        foreach (var threshold in DimensionThresholds)
        {
            if (width < threshold && height < threshold)
            {
                width = height = threshold;
                return;
            }
        }

        if (width < height)
        {
            // default tall size
            width = 1024;
            height = 1792;
            return;
        }

        // default wide size
        width = 1792;
        height = 1024;
    }
}
