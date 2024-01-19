using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Experimental.Assistants;
using SenseNet.Tools.Features;

namespace SenseNet.AI.Text.SemanticKernel;

#pragma warning disable SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Generates content query from user input using Microsoft Semantic Kernel.
/// </summary>
/// <inheritdoc />
public class ContentQueryGenerator : IContentQueryGenerator, ISnFeature
{
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<ContentQueryGenerator> _logger;
    private readonly IServiceProvider _serviceProvider;

    #region ISnFeature implementation    

    public string Name => "AIContentQueryProvider";
    public string DisplayName => "AI Content Query Provider";

    public Task<FeatureAvailability> GetStateAsync(CancellationToken _)
    {
        if (string.IsNullOrEmpty(_options.OpenAiApiKey))
            return Task.FromResult(new FeatureAvailability(FeatureState.NotConfigured, "OpenAI API key is not set."));
        
        //TODO: periodically check if the api key is valid and the service is available

        return Task.FromResult(new FeatureAvailability(FeatureState.Active));
    }

    #endregion

    public ContentQueryGenerator(IOptions<SemanticKernelOptions> options, ILogger<ContentQueryGenerator> logger, IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<QueryData> GenerateQueryAsync(string text, CancellationToken cancel)
    {
        return GenerateQueryAsync(text, null, cancel);
    }
    public async Task<QueryData> GenerateQueryAsync(string text, string? threadId, CancellationToken cancel)
    {   
        if (string.IsNullOrEmpty(_options.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not set.");
        if (string.IsNullOrEmpty(_options.AssistantId))
            throw new InvalidOperationException("OpenAI assistant ID is not set.");

        if (string.IsNullOrEmpty(text))
        {
            _logger.LogTrace("Text is empty, OpenAI call skipped, returning empty string");
            return QueryData.Empty;
        }

        //TODO: can we use a singleton kernel builder, defined during startup in DI?

        var builder = await AssistantBuilder.GetAssistantAsync(_options.OpenAiApiKey, 
            _options.AssistantId,             
            cancellationToken: cancel);

        if (_options.ConfigureDefaultPlugins != null)
        {            
            _options.ConfigureDefaultPlugins(builder.Plugins, _serviceProvider);

            _logger.LogTrace("Configured OpenAI Assistant query generator plugins: " +
                $"{string.Join(", ", builder.Plugins.Select(p => p.Name))}");
        }

        IChatThread? thread = null;

        if (!string.IsNullOrEmpty(threadId))
        {
            _logger.LogTrace("Loading existing OpenAI Assistant thread {threadId}", threadId);

            try
            {
                thread = await builder.GetThreadAsync(threadId, cancel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OpenAI Assistant thread {threadId}", threadId);
            }
        }
        
        if (thread == null)
        {
            _logger.LogTrace("Creating new OpenAI Assistant thread");
            thread = await builder.NewThreadAsync(cancel);
        }

        // add more context to the text
        text = $"{text}" + Environment.NewLine + 
            $"<currentdate>{DateTime.UtcNow.Date}</currentdate>" +
            $"<currenttime>{DateTime.UtcNow}</currenttime>";
        
        await thread.AddUserMessageAsync(text, cancel);
        var messages = thread.InvokeAsync(builder, cancel);

        var msg = await messages.LastAsync(cancellationToken: cancel);        

        return new QueryData { Query = msg.Content, ThreadId = thread.Id };
    }
}

#pragma warning restore SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.