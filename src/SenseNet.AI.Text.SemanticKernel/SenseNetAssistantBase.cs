using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Experimental.Assistants;

namespace SenseNet.AI.Text.SemanticKernel;

#pragma warning disable SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public abstract class SenseNetAssistantBase(IOptions<SemanticKernelOptions> options, ILogger<SenseNetAssistantBase> logger, IServiceProvider serviceProvider)
{
    protected readonly SemanticKernelOptions Options = options.Value;
    protected readonly ILogger Logger = logger;
    protected readonly IServiceProvider Services = serviceProvider;

    /// <summary>
    /// Gets the assistant ID. Derived classes must override this property and provide the assistant ID representing their feature.
    /// </summary>
    protected abstract string AssistantId { get; }

    /// <summary>
    /// Gets a new assistant instance configured with plugins.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task<IAssistant> GetAssistantAsync(CancellationToken cancel)
    {
        if (string.IsNullOrEmpty(Options.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not set.");
        if (string.IsNullOrEmpty(AssistantId))
            throw new InvalidOperationException("OpenAI assistant ID is not set.");
         
        var assistant = await AssistantBuilder.GetAssistantAsync(Options.OpenAiApiKey, AssistantId, cancellationToken: cancel);

        ConfigurePlugins(assistant);

        return assistant;
    }

    /// <summary>
    /// Configures the default plugins for the assistant. Derived classes can override this method to add more plugins.
    /// </summary>
    protected virtual void ConfigurePlugins(IAssistant assistant)
    {
        if (Options.ConfigureDefaultPlugins == null)
            return;
        
        Options.ConfigureDefaultPlugins(assistant.Plugins, Services);

        Logger.LogTrace($"Configured OpenAI Assistant default plugins: {string.Join(", ", assistant.Plugins.Select(p => p.Name))}");
    }

    /// <summary>
    /// Gets an existing assistant thread or creates a new one.
    /// </summary>
    protected async Task<IChatThread> GetOrCreateThreadAsync(IAssistant assistant, string? threadId, CancellationToken cancel)
    {
        IChatThread? thread = null;

        if (!string.IsNullOrEmpty(threadId))
        {
            Logger.LogTrace("Loading an existing OpenAI Assistant thread {threadId}", threadId);

            try
            {
                thread = await assistant.GetThreadAsync(threadId, cancel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading OpenAI Assistant thread {threadId}", threadId);
            }
        }
        
        if (thread == null)
        {
            Logger.LogTrace("Creating a new OpenAI Assistant thread");
            thread = await assistant.NewThreadAsync(cancel);
        }

        return thread;
    }

    /// <summary>
    /// Sends a message to the assistant, waits for the response and returns the last received message.
    /// </summary>
    /// <returns>The last response message.</returns>
    protected async Task<IChatMessage?> SendMessageAsync(IAssistant assistant, IChatThread thread, string text, CancellationToken cancel)
    {
        var response = await GetResponseAsync(assistant, thread, text, cancel);
        var message = await response.LastAsync(cancellationToken: cancel);

        return message;
    }

    /// <summary>
    /// Sends a message to the assistant and returns the response messages.
    /// </summary>
    protected async Task<IAsyncEnumerable<IChatMessage>> GetResponseAsync(IAssistant assistant, IChatThread thread, string text, CancellationToken cancel)
    {
        await thread.AddUserMessageAsync(text, cancel);

        return thread.InvokeAsync(assistant, cancel);
    }
}

#pragma warning restore SKEXP0101 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.