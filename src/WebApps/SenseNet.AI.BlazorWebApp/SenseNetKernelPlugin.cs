using System.ComponentModel;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when executing GetContainerPath with name {name}", name);
        }

        return string.Empty;
    }

    [KernelFunction, Description("Determines the id of a user based on its name, login name or username")]
    public async Task<string> GetUserId(
        [Description("The name of the user")] string name, CancellationToken cancel)
    {
        try
        {
            var repo = await _repositories.GetRepositoryAsync(cancel).ConfigureAwait(false);
            var users = await repo.QueryAsync(new QueryContentRequest
            {
                ContentQuery = $"TypeIs:User AND " +
                               $"(Name:\"{name}\" OR LoginName:\"{name}\" OR DisplayName:\"*{name}*\")",
                Select = new[] { "Id", "Path", "Type", "Name", "LoginName", "Email" }
            }, cancel).ConfigureAwait(false);

            return JsonConvert.SerializeObject(new
            {
                userIds = users.Select(u => u.Id).ToArray(),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when executing GetUserId with name {name}", name);
        }

        return string.Empty;
    }

    private static readonly string[] DefaultSelectFields = { "Id", "Path", "Type" };

    [KernelFunction, Description("Executes a content query and returns the result content items in json format. " +
        "Called when it is required to find one or more content in the repository.")]
    public async Task<string> ExecuteContentQuery(
        [Description("Content query text")] string contentQuery,
        [Description("Comma separated array of field names that are needed by the business case or empty string.")] string select,
        [Description("Comma separated array of reference fields to include in the result or empty string.")] string expand,
        CancellationToken cancel)
    {
        string[]? selectFields = null;
        if (!string.IsNullOrEmpty(select))
        {
            selectFields = select.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Union(DefaultSelectFields).Distinct().ToArray();
        }

        string[]? expandFields = null;
        if (!string.IsNullOrEmpty(expand))
        {
            expandFields = expand.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        try
        {
            var repo = await _repositories.GetRepositoryAsync(cancel).ConfigureAwait(false);
            var responseText = await repo.GetResponseJsonAsync(new ODataRequest(repo.Server)
            {
                Path = "/Root",
                IsCollectionRequest = true,
                ContentQuery = contentQuery,
                Select = selectFields,
                Expand = expandFields,
                Metadata = MetadataFormat.None,

            }, HttpMethod.Get, cancel).ConfigureAwait(false);

            var result = "{\"results\": " + JsonConvert.SerializeObject(responseText.d.results) + "}";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when executing ExecuteContentQuery method with the query: {contentQuery}", contentQuery);
        }

        return string.Empty;
    }

    [KernelFunction, Description("Sends an email containing a subject and body to the provided address.")]
    public Task<string> SendEmail(
        [Description("Email address")] string emailAddress,
        [Description("Email subject")] string subject,
        [Description("Email body")] string body)
    {
        _logger.LogInformation($"Sending email to {emailAddress} with subject '{subject}' and body '{body}'");

        return Task.FromResult("ok");
    }
}
