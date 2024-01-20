using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Text.SemanticKernel;

#pragma warning disable SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Generates a content query from user input using Microsoft Semantic Kernel.
/// </summary>
/// <inheritdoc />
public class ContentQueryGenerator(IOptions<SemanticKernelOptions> options, ILogger<ContentQueryGenerator> logger, 
    IServiceProvider serviceProvider) : SenseNetAssistantBase(options, logger, serviceProvider), IContentQueryGenerator, ISnFeature
{
    protected override string AssistantId => Options.Assistants.ContentQueryGenerator ?? string.Empty;

    #region ISnFeature implementation    

    public string Name => "AIContentQueryProvider";
    public string DisplayName => "AI Content Query Provider";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken _)
    {
        if (string.IsNullOrEmpty(Options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));
            if (string.IsNullOrEmpty(Options.Assistants.ContentQueryGenerator))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI Content Query Generator assistant ID is not set."));
        
        //TODO: periodically check if the api key is valid and the service is available

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    public Task<QueryData> GenerateQueryAsync(string text, CancellationToken cancel)
    {
        return GenerateQueryAsync(text, null, cancel);
    }
    public async Task<QueryData> GenerateQueryAsync(string text, string? threadId, CancellationToken cancel)
    {
        if (string.IsNullOrEmpty(text))
        {
            Logger.LogTrace("Text is empty, OpenAI call skipped, returning empty result");
            return new QueryData { Query = string.Empty, ThreadId = string.Empty };
        }

        if (string.IsNullOrEmpty(threadId))
        {
            // add more context to the first message
            text = $"{text}" + Environment.NewLine + 
                $"<currentdate>{DateTime.UtcNow.Date}</currentdate>" +
                $"<currenttime>{DateTime.UtcNow}</currenttime>";
        }

        var assistant = await GetAssistantAsync(cancel);
        var thread = await GetOrCreateThreadAsync(assistant, threadId, cancel);        
        var responseMessage = await SendMessageAsync(assistant, thread, text, cancel);  

        return new QueryData { Query = responseMessage?.Content ?? string.Empty, ThreadId = thread.Id ?? string.Empty };
    }
}

#pragma warning restore SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.