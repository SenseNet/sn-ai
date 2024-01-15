namespace SenseNet.AI.Text;

/// <summary>
/// Defines methods to get content query from user input.
/// </summary>
public interface IContentQueryProvider
{
    /// <summary>
    /// Gets a content query from user input.
    /// </summary>
    /// <param name="text">Query expressed in natural language.</param>
    /// <param name="cancel"></param>
    /// <returns>A query data containing a query and the thread ID.</returns>
    Task<QueryData> GetQuery(string text, CancellationToken cancel);
    /// <summary>
    /// Gets a content query from user input. This method can be called
    /// multiple times with a thread ID to fine-tune the query.
    /// </summary>
    /// <param name="text">Query expressed in natural language or a statement
    /// to fine-tune the previous query.</param>
    /// <param name="threadId">Thread identifier.</param>
    /// <param name="cancel"></param>
    /// <returns>A query data containing a query and the thread ID.</returns>
    Task<QueryData> GetQuery(string text, string threadId, CancellationToken cancel);
}
