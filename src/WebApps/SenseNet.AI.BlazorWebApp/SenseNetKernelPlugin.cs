﻿using System.ComponentModel;
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
        _logger.LogTrace("GetContainerPath called with name {name}", name);

        try
        {
            var repo = await _repositories.GetRepositoryAsync(cancel);
            var folders = await repo.QueryAsync(new QueryContentRequest
            {
                ContentQuery = $"TypeIs:Folder AND Name:\"{name}\"",
                Select = new[] { "Id", "Path", "Type" }
            }, cancel);
            
            var paths = string.Join(", ", folders.Select(f => "\"" + f.Path + "\""));

            if (folders.Count < 5)
                _logger.LogTrace("GetContainerPath found paths: {PathList}", paths);
            else
                _logger.LogTrace("GetContainerPath found {ContainerCount} paths", folders.Count);

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
        _logger.LogTrace("GetUserId called with name {username}", name);

        if (string.IsNullOrEmpty(name))
            return JsonConvert.SerializeObject(new { error = "Name is required" });

        try
        {
            var repo = await _repositories.GetRepositoryAsync(cancel).ConfigureAwait(false);

            // If the name is a number, we assume it is an id.
            // If it is @@currentuser@@, we query for the current user.
            var queryText = 
                string.Equals(name, "@@currentuser@@", StringComparison.InvariantCultureIgnoreCase) ||
                int.TryParse(name, out _)
                    ? $"TypeIs:User AND Id:{name}"
                    : $"TypeIs:User AND (Name:\"{name}\" OR LoginName:\"{name}\" OR DisplayName:\"*{name}*\")";

            var users = await repo.QueryAsync(new QueryContentRequest
            {
                ContentQuery = queryText,
                Select = new[] { "Id", "Path", "Type", "Name", "LoginName", "Email" }
            }, cancel).ConfigureAwait(false);

            _logger.LogTrace("GetUserId found {usercount} users with name {username}: {UserIds}", users.Count, name,
                string.Join(", ", users.Select(u => u.Id)));

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

    [KernelFunction, Description("Executes a content query and returns the result content items in json format, " +
        "or an object with an error property. Called when it is required to find one or more content in the repository.")]
    public async Task<string> ExecuteContentQuery(
        [Description("Content query text")] string contentQuery,
        [Description("Comma separated array of field names that are needed by the business case or empty string.")] string? select = null,
        [Description("Comma separated array of reference fields to include in the result or empty string.")] string? expand = null,
        CancellationToken cancel = default)
    {
        _logger.LogTrace("ExecuteContentQuery called with query '{contentQuery}'", contentQuery);

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

            _logger.LogTrace("ExecuteContentQuery found {QueryResultCount} items.", (int)responseText.d.__count);

            var result = "{\"results\": " + JsonConvert.SerializeObject(responseText.d.results) + "}";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when executing ExecuteContentQuery method with the query: {contentQuery}", contentQuery);

            return JsonConvert.SerializeObject(new { error = ex.Message });
        }
    }

    [KernelFunction, Description("Copies a content to a target folder in the content repository.")]
    public async Task<string> CopyContent(
        [Description("The path of the content to copy")] string sourcePath,
        [Description("The target container path to copy the content to")] string targetPath,
        CancellationToken cancel)
    {
        _logger.LogTrace("CopyContent called with source {sourcePath} and target {targetPath}", sourcePath, targetPath);

        try
        {
            var repo = await _repositories.GetRepositoryAsync(cancel).ConfigureAwait(false);
            var sourceContent = await repo.LoadContentAsync(new LoadContentRequest()
            {
                Path = sourcePath,
                Select = new[] { "Id", "Name", "Path", "Type" }
            }, cancel).ConfigureAwait(false);

            if (sourceContent == null)
            {
                _logger.LogTrace("Source content not found: {sourcePath}", sourcePath);

                return JsonConvert.SerializeObject(new { error = "Source content not found" });
            }

            if (!(await repo.IsContentExistsAsync(targetPath, cancel).ConfigureAwait(false)))
            {
                _logger.LogTrace("Target container not found: {targetPath}", targetPath);

                return JsonConvert.SerializeObject(new { error = "Target container not found" });
            }

            await sourceContent.CopyToAsync(targetPath, cancel).ConfigureAwait(false);

            _logger.LogInformation("Content copied from {sourcePath} to {targetPath}", sourcePath, targetPath);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                path = RepositoryPath.Combine(targetPath, sourceContent.Name)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when copying {sourcePath} to {targetPath}", sourcePath, targetPath);
            return JsonConvert.SerializeObject(new { error = ex.Message });
        }
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
