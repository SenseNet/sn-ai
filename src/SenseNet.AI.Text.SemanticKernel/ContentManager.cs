using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Text.SemanticKernel;

#pragma warning disable SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class ContentManager(
    IContentQueryGenerator queryGenerator,
    IContentTypeGenerator contentTypeGenerator,
    IOptions<SemanticKernelOptions> options, ILogger<ContentManager> logger,
    IServiceProvider serviceProvider) : SenseNetAssistantBase(options, logger, serviceProvider), IContentManager, ISnFeature
{
    private readonly IContentQueryGenerator _queryGenerator = queryGenerator;
    private readonly IContentTypeGenerator _contentTypeGenerator = contentTypeGenerator;

    #region ISnFeature implementation    

    public string Name => "AIContentManager";
    public string DisplayName => "AI Content Manager";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken _)
    {
        if (string.IsNullOrEmpty(Options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));
        if (string.IsNullOrEmpty(Options.Assistants.ContentManager))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI Content Manager assistant ID is not set."));

        //TODO: periodically check if the api key is valid and the service is available

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    protected override string AssistantId => Options.Assistants.ContentManager ?? string.Empty;

    public async Task<ContentManagerData> InvokeAsync(string text, string threadId, CancellationToken cancel)
    {
        if (string.IsNullOrEmpty(text))
        {
            Logger.LogTrace("Text is empty, OpenAI call skipped, returning empty result");
            return ContentManagerData.Empty;
        }

        var assistant = await GetAssistantAsync(cancel);

        // add assistant plugins
        if (_queryGenerator is SenseNetAssistantBase queryGeneratorAssistant)
        {
            var assistantPlugin = (await queryGeneratorAssistant.GetAssistantAsync(cancel).ConfigureAwait(false)).AsPlugin();
            assistant.Plugins.Add(assistantPlugin);
        }
        if (_contentTypeGenerator is SenseNetAssistantBase typeGeneratorAssistant)
        {
            var assistantPlugin = (await typeGeneratorAssistant.GetAssistantAsync(cancel).ConfigureAwait(false)).AsPlugin();
            assistant.Plugins.Add(assistantPlugin);
        }

        var thread = await GetOrCreateThreadAsync(assistant, threadId, cancel).ConfigureAwait(false);
        var responseMessage = await SendMessageAsync(assistant, thread, text, cancel).ConfigureAwait(false);

        var result = new ContentManagerData
        {
            ThreadId = thread.Id,
            Text = responseMessage?.Content ?? string.Empty,
        };

        return result;
    }
}

#pragma warning restore SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.