using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Text.SemanticKernel;

#pragma warning disable SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Generates a content type from user input using Microsoft Semantic Kernel.
/// </summary>
/// <inheritdoc />
public class ContentTypeGenerator(IOptions<SemanticKernelOptions> options, ILogger<ContentTypeGenerator> logger, 
    IServiceProvider serviceProvider) : SenseNetAssistantBase(options, logger, serviceProvider), IContentTypeGenerator, ISnFeature
{
    protected override string AssistantId => Options.Assistants.ContentTypeGenerator ?? string.Empty;

    #region ISnFeature implementation    

    public string Name => "AIContentTypeGenerator";
    public string DisplayName => "AI Content Type Generator";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken _)
    {
        if (string.IsNullOrEmpty(Options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));
            if (string.IsNullOrEmpty(Options.Assistants.ContentTypeGenerator))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI Content Type Generator assistant ID is not set."));
        
        //TODO: periodically check if the api key is valid and the service is available

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    public Task<ContentTypeData> GenerateContentTypeAsync(string text, CancellationToken cancel)
    {
        return GenerateContentTypeAsync(text, null, cancel);
    }
    public async Task<ContentTypeData> GenerateContentTypeAsync(string text, string? threadId, CancellationToken cancel)
    {
        if (string.IsNullOrEmpty(text))
        {
            Logger.LogTrace("Text is empty, OpenAI call skipped, returning empty result");
            return new ContentTypeData { ContentTypeDefinition = string.Empty, ThreadId = string.Empty };
        }

        // we could extend the text with additional context information here

        var assistant = await GetAssistantAsync(cancel);
        var thread = await GetOrCreateThreadAsync(assistant, threadId, cancel);        
        var responseMessage = await SendMessageAsync(assistant, thread, text, cancel);  

        return new ContentTypeData { ContentTypeDefinition = responseMessage?.Content ?? string.Empty, ThreadId = thread.Id ?? string.Empty };
    }
}

#pragma warning restore SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.