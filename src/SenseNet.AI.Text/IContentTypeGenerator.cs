namespace SenseNet.AI.Text;

/// <summary>
/// Defines methods for generating content types from user input.
/// </summary>
public interface IContentTypeGenerator
{
    /// <summary>
    /// Generates a content type from user input.
    /// </summary>
    /// <param name="text">A content type definition expressed in natural language.</param>
    /// <param name="cancel"></param>
    /// <returns>A content type data containing a CTD xml and the thread ID.</returns>
    Task<ContentTypeData> GenerateContentTypeAsync(string text, CancellationToken cancel);
    /// <summary>
    /// Generates a content type from user input. This method can be called
    /// multiple times with a thread ID to fine-tune the CTD.
    /// </summary>
    /// <param name="text">A content type definition expressed in natural language or a statement
    /// to fine-tune the previous CTD.</param>
    /// <param name="threadId">Thread identifier.</param>
    /// <param name="cancel"></param>
    /// <returns>A content type data containing a CTD xml  and the thread ID.</returns>
    Task<ContentTypeData> GenerateContentTypeAsync(string text, string threadId, CancellationToken cancel);
}
