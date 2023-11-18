namespace SenseNet.AI.SemanticKernel;

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
