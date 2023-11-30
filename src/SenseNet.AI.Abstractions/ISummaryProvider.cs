namespace SenseNet.AI.Abstractions;

/// <summary>
/// Defines methods for summarizing text.
/// </summary>
public interface ISummaryProvider
{
    /// <summary>
    /// Summarizes the given text.
    /// </summary>
    /// <param name="text">The text to be summarized.</param>
    /// <param name="maxWordCount">Maximum word count.</param>
    /// <param name="maxSentenceCount">Maximum sentence count.</param>
    /// <param name="cancel"></param>
    /// <returns>Summary text, maximized by the provided values.</returns>
    Task<string> GetSummary(string text, int maxWordCount, int maxSentenceCount, CancellationToken cancel);
}
