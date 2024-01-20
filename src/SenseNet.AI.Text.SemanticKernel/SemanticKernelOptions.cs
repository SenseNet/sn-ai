using Microsoft.SemanticKernel;
using SenseNet.Tools.Configuration;

namespace SenseNet.AI.Text.SemanticKernel;

/// <summary>
/// Options for the Semantic Kernel feature.
/// </summary>
[OptionsClass(sectionName: "sensenet:ai:text:SemanticKernel")]
public class SemanticKernelOptions
{
    //public string Model { get; set; }

    /// <summary>
    /// Azure service endpoint
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

    /// <summary>
    /// Available assistants
    /// </summary>
    public Assistants Assistants { get; set; } = new();


    /// <summary>
    /// Configures the plugins that will be registered by all kernels.
    /// </summary>
    public Action<KernelPluginCollection, IServiceProvider?> ConfigureDefaultPlugins { get; set; } = (_, _) => { };
}

public struct Assistants
{
        /// <summary>
        /// Content query generator assistant ID
        /// </summary>
        public string? ContentQueryGenerator { get; set; }
        /// <summary>
        /// Content type generator assistant ID
        /// </summary>
        public string? ContentTypeGenerator { get; set; }
}
