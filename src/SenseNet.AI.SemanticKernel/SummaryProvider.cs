using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using SenseNet.AI.Abstractions;
using SenseNet.Tools.Features;

namespace SenseNet.AI.SemanticKernel;

public class SummaryProvider : ISummaryProvider, ISnFeature
{
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<SummaryProvider> _logger;

    public SummaryProvider(IOptions<SemanticKernelOptions> options, ILogger<SummaryProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    #region ISnFeature implementation

    public string Name => "AISummary";
    public string DisplayName => "AI Summary";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken _)
    {
        if (string.IsNullOrEmpty(_options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));
        
        //TODO: periodically check if the api key is valid and the service is available

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    public async Task<string> GetSummary(string text, CancellationToken cancel)
    {
        var builder = new KernelBuilder();

        // Configure AI backend used by the kernel
        // var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();
        // if (useAzureOpenAI)
        //     builder.WithAzureOpenAIChatCompletionService(model, azureEndpoint, apiKey);
        // else
        //     builder.WithOpenAIChatCompletionService(model, apiKey, orgId);

        if (string.IsNullOrEmpty(_options.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not set.");

        if (string.IsNullOrEmpty(text))
        {
            _logger.LogTrace("Text is empty, OpenAI call skipped, returning empty string");
            return string.Empty;
        }

        _logger.LogTrace("Initializing OpenAI kernel");

        builder.WithOpenAIChatCompletionService("gpt-3.5-turbo", _options.OpenAiApiKey);

        IKernel kernel = builder.Build();

        var prompt = @"{{$input}}
Maximum three lines TLDR, maximum three sentences.";

        var summarize = kernel.CreateSemanticFunction(prompt,
            requestSettings: new OpenAIRequestSettings { MaxTokens = 100 });

        _logger.LogTrace("Running OpenAI operation");

        var result = await kernel.RunAsync(text, summarize);

        return result.ToString();
    }
}
