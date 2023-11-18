namespace SenseNet.AI.Abstractions;

/// <summary>
/// Defines methods for simple text processing.
/// </summary>
public interface ITextService
{
    Task<string> GetSummary(string text, CancellationToken cancel);
}
