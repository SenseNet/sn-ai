namespace SenseNet.AI.Vision;

/// <summary>
/// Defines methods for generating images from text.
/// </summary>
public interface IImageGenerator
{
    /// <summary>
    /// Generates an image from the given text.
    /// </summary>
    /// <param name="text">The text to generate the image from.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <remarks>
    /// The maximum length of the text is 1000 characters in case of the 
    /// dall-e-2 engine and 4000 characters in case of dall-e-3.
    /// The size of the generated image depends on the provided width and height.
    /// The engine will choose one of the available sizes that fits the
    /// provided dimensions. The available sizes are: '256x256', '512x512', 
    /// '1024x1024', '1024x1792', '1792x1024'.
    /// </remarks>
    /// <returns>The image data of the generated image.</returns>
    Task<ImageData> GenerateImage(string text, int width, int height, CancellationToken cancel);
}
