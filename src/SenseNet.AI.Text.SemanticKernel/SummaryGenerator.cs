using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Text.SemanticKernel;

/// <summary>
/// Generates a summary of the given text using the OpenAI API.
/// </summary>
/// <inheritdoc/>
public class SummaryGenerator : ISummaryGenerator, ISnFeature
{
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<SummaryGenerator> _logger;

    public SummaryGenerator(IOptions<SemanticKernelOptions> options, ILogger<SummaryGenerator> logger)
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

    public async Task<string> GenerateSummaryAsync(string text, int maxWordCount, int maxSentenceCount, CancellationToken cancel)
    {
        //TODO: can we use a singleton kernel builder, defined during startup in DI?
        var builder = Kernel.CreateBuilder();
        
        if (string.IsNullOrEmpty(_options.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not set.");

        if (string.IsNullOrEmpty(text))
        {
            _logger.LogTrace("Text is empty, OpenAI call skipped, returning empty string");
            return string.Empty;
        }
        
        if (string.IsNullOrEmpty(_options.AzureEndpoint))
        {
            _logger.LogTrace("Initializing OpenAI kernel with the default endpoint.");

            // use the default OpenAI endpoint
            builder.AddOpenAIChatCompletion("gpt-3.5-turbo", new OpenAIClient(_options.OpenAiApiKey));
        }
        else
        {
            _logger.LogTrace("Initializing OpenAI kernel with the endpoint {endpoint}", _options.AzureEndpoint);

            // Azure mode
            builder.AddAzureOpenAIChatCompletion("gpt-3.5-turbo", 
                _options.AzureEndpoint,
                _options.OpenAiApiKey);
        }

        var kernel = builder.Build();

        // make sure that max word and sentence counts are reasonable (1-10000, 1-500)
        maxWordCount = Math.Max(1, Math.Min(maxWordCount, 10000));
        maxSentenceCount = Math.Max(1, Math.Min(maxSentenceCount, 500));

        var prompt = @"{{$input}}
" + $"TLDR in maximum {maxWordCount} words, maximum {maxSentenceCount} sentences.";

        var summarize = kernel.CreateFunctionFromPrompt(prompt, 
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 500
            });

        _logger.LogTrace("Running OpenAI operation");

        var result = await kernel.InvokeAsync(summarize, new KernelArguments
        {
            ["input"] = text
        }, cancellationToken: cancel);

        return result.ToString();
    }
}
