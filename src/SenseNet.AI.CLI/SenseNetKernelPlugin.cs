using System.ComponentModel;
using Microsoft.SemanticKernel;
using SenseNet.Client;

namespace SenseNet.AI.Text.SemanticKernel;

/// <summary>
/// Sample plugin for the semantic kernel.
/// </summary>
public sealed class SenseNetKernelPlugin
{
    private readonly IRepositoryCollection _repositories;

    public SenseNetKernelPlugin(IRepositoryCollection repositories)
    {
        _repositories = repositories;
    }
    
    [KernelFunction, Description("Determines the path of a folder based on its name")]
    public async Task<string> GetContainerPath(
        [Description("The name of the folder")] string name, CancellationToken cancel)
    {
        var repo = await _repositories.GetRepositoryAsync(cancel);
        var folders = await repo.QueryAsync(new QueryContentRequest
        {
            ContentQuery = $"TypeIs:Folder AND Name:\"{name}\"",
            Select = new[] { "Id", "Path", "Type" }
        }, cancel);

        var paths = string.Join(", ", folders.Select(f => "\"" + f.Path + "\""));
        var pathResult = $"{{ \"Path\": [ {paths} ] }}";
        
        return pathResult;
    }

    [KernelFunction, Description("Determines the id of a user based on its name, login name or username")]
    public Task<string> GetUserId(
        [Description("The name of the user")] string name)
    {
        return Task.FromResult("123");
    }    
}
