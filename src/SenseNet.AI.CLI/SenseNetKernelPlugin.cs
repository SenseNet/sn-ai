using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SenseNet.AI.Text.SemanticKernel;

/// <summary>
/// Sample plugins for the semantic kernel.
/// </summary>
public class SenseNetKernelPlugin
{
    [KernelFunction, Description("Determines the path of a folder based on its name")]
    public static Task<string> GetContainerPath(
        [Description("The name of the folder")] string name)
    {
        var pathResult = $"{{ \"Path\": \"/Root/Content/{name}\" }}";
        
        return Task.FromResult(pathResult);
    }

    [KernelFunction, Description("Determines the id of a user based on its name, login name or username")]
    public static Task<string> GetUserId(
        [Description("The name of the user")] string name)
    {
        return Task.FromResult("123");
    }    
}
