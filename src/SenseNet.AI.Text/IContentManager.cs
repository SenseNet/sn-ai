namespace SenseNet.AI.Text;

/// <summary>
/// Defines methods for having a conversation with the AI Content Manager assistant.
/// </summary>
public interface IContentManager
{
    /// <summary>
    /// Sends a message to the AI Content Manager assistant and returns the response.
    /// </summary>
    public Task<ContentManagerData> InvokeAsync(string text, string threadId, CancellationToken cancel);
}