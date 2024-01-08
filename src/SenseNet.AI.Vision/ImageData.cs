namespace SenseNet.AI.Vision;

/// <summary>
/// A representation of a single generated image, represented by a URL.
/// </summary>
public struct ImageData
{
    /// <summary>
    /// The URL that provides temporary access to the generated image. 
    /// Expiration depends on the configured engine, in case of
    /// OpenAI API it is 60 minutes.
    /// </summary>
    public string Url { get; set; }
    // public string Base64Data { get; set; }
    // public string RevisedPrompt { get; set; }

    public override string ToString()
    {
        return Url;
    }
}
