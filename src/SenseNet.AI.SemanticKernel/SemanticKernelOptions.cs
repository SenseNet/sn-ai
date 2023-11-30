using SenseNet.Tools.Configuration;

namespace SenseNet.AI.SemanticKernel;

/// <summary>
/// Options for the Semantic Kernel feature.
/// </summary>
[OptionsClass(sectionName: "sensenet:ai:SemanticKernel")]
public class SemanticKernelOptions
{
    //public string Model { get; set; }
    //public string AzureEndpoint { get; set; }
    
    /// <summary>
    /// OpenAI API key
    /// </summary>
    public string? OpenAiApiKey { get; set; }
    /// <summary>
    /// OpenAI organization ID
    /// </summary>
    public string? OrgId { get; set; }
}
