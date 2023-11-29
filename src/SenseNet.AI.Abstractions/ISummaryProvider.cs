namespace SenseNet.AI.Abstractions;

/// <summary>
/// Defines methods for summarizing text.
/// </summary>
public interface ISummaryProvider
{
    Task<string> GetSummary(string text, CancellationToken cancel);
}
