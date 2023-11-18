using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using SenseNet.AI.Abstractions;

namespace SenseNet.AI.SemanticKernel;

public class TextService : ITextService
{
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<TextService> _logger;

    public TextService(IOptions<SemanticKernelOptions> options, ILogger<TextService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

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
