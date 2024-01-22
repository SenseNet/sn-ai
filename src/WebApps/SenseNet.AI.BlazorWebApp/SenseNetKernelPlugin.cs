using System.ComponentModel;
using Microsoft.SemanticKernel;
using SenseNet.Client;

namespace SenseNet.AI.BlazorWebApp;

/// <summary>
/// Sample plugin for the semantic kernel.
/// </summary>
public sealed class SenseNetKernelPlugin
{
    private readonly IRepositoryCollection _repositories;
    private readonly ILogger<SenseNetKernelPlugin> _logger;

    public SenseNetKernelPlugin(IRepositoryCollection repositories, ILogger<SenseNetKernelPlugin> logger)
    {
        _repositories = repositories;
        _logger = logger;
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

    [KernelFunction, Description("Executes a content query and returns the result items in json format. " +
        "Called when a user asks to find one or more content in the repository.")]
    public async Task<string> ExecuteContentQuery(
        [Description("Content query text")] string contentQuery,
        [Description("Array of fields that are needed by the business case. Leaving it empty is ok.")] string[]? select = null,
        [Description("Array of reference fields to expand in the result.")] string[]? expand = null,
        CancellationToken cancel = default)
    {
        var repo = await _repositories.GetRepositoryAsync(cancel);
        //var contentCollection = await repo.QueryAsync(new QueryContentRequest
        //{
        //    ContentQuery = contentQuery,
        //    Select = select,
        //    Expand = expand,
        //}, cancel);

        var responseText = await repo.GetResponseStringAsync(new ODataRequest
        {
            ContentQuery = contentQuery,
            Select = select,
            Expand = expand,
            Metadata = MetadataFormat.None,
        }, HttpMethod.Get, cancel);

        if (responseText != null && responseText.StartsWith("{\r\n  \"d\": {")) 
        { 
            responseText = responseText[10..^2];
        }       

        return responseText ?? string.Empty;
    }

    [KernelFunction, Description("Sends an email to the provided address containing a subject and body.")]
    public Task<bool> SendEmail(
        [Description("Email address")] string emailAddress,
        [Description("Email subject")] string subject,
        [Description("Email body")] string body)
    {
        _logger.LogInformation($"Sending email to {emailAddress} with subject '{subject}' and body '{body}'");

        return Task.FromResult(true);
    }
}
