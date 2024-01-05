using SenseNet.Tools.Configuration;

namespace SenseNet.AI.Vision.Azure;

/// <summary>
/// Options for the Azure Vision feature.
/// </summary>
[OptionsClass(sectionName: "sensenet:ai:AzureVision")]
public class AzureVisionOptions
{
    /// <summary>
    /// The model to use for image generation. Default: "dall-e-3".
    /// </summary>
    public string Model { get; set; } = "dall-e-3";
    /// <summary>
    /// The Azure endpoint to use. If not set, the default OpenAI endpoint will be used.
    /// </summary>
    public string? AzureEndpoint { get; set; }
    
    /// <summary>
    /// OpenAI API key
    /// </summary>
    public string? OpenAiApiKey { get; set; }
    /// <summary>
    /// OpenAI organization ID
    /// </summary>
    public string? OrgId { get; set; }
}
