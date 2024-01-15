namespace SenseNet.AI.Text;

/// <summary>
/// Defines methods for generating a content query from user input.
/// </summary>
public interface IContentQueryGenerator
{
    /// <summary>
    /// Generates a content query from user input.
    /// </summary>
    /// <param name="text">Query expressed in natural language.</param>
    /// <param name="cancel"></param>
    /// <returns>A query data containing a query and the thread ID.</returns>
    Task<QueryData> GenerateQueryAsync(string text, CancellationToken cancel);
    /// <summary>
    /// Generates a content query from user input. This method can be called
    /// multiple times with a thread ID to fine-tune the query.
    /// </summary>
    /// <param name="text">Query expressed in natural language or a statement
    /// to fine-tune the previous query.</param>
    /// <param name="threadId">Thread identifier.</param>
    /// <param name="cancel"></param>
    /// <returns>A query data containing a query and the thread ID.</returns>
    Task<QueryData> GenerateQueryAsync(string text, string threadId, CancellationToken cancel);
}
