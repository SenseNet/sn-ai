using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SenseNet.AI.Text;
using SenseNet.AI.Text.SemanticKernel;
using SenseNet.AI.Vision;
using SenseNet.Client;
using SenseNet.Extensions.DependencyInjection;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSenseNetSemanticKernel(options =>
        {
            hostContext.Configuration.GetSection("sensenet:ai:text:SemanticKernel").Bind(options);
            options.ConfigureDefaultPlugins = (plugins, serviceProvider) =>
            {
                // pass on the IServiceProvider to the API to let it resolve dependencies
                plugins.AddFromType<SenseNetKernelPlugin>(serviceProvider: serviceProvider);
            };
        })
        .AddSenseNetAzureVision(options =>
        {
            hostContext.Configuration.GetSection("sensenet:ai:AzureVision").Bind(options);
        });        

        services.AddSenseNetClient()
        .ConfigureSenseNetRepository(options =>
        {
            hostContext.Configuration.GetSection("sensenet:repository").Bind(options);
        });

        services.AddLogging((loggingBuilder) =>
        {
            // removed console logging to let the tool interact with the user
            //loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Error);
        });
    });

var host = hostBuilder.Build();

// Summary
//await TestSummary();

// Content Query
//await TestContentQuery();

// Content Type
await TestContentType();

// Azure Vision
//await TestImageGeneration();

async Task TestContentQuery()
{
    var repositoryCollection = host.Services.GetRequiredService<IRepositoryCollection>();
    var repository = await repositoryCollection.GetRepositoryAsync(CancellationToken.None);

    var threadId = string.Empty;

    // ask for user input and generate a content query until the user enters an empty line
    while(true)
    {
        ConsoleWrite(ConsoleColor.Yellow, "User Query> ");

        var text = Console.ReadLine();
        if (string.IsNullOrEmpty(text))
            break;

        var result = await GenerateContentQuery(text, threadId);
        threadId = result.ThreadId;

        ConsoleWrite(ConsoleColor.Blue, "Content Query> ");
        Console.WriteLine(result.Query);
        Console.WriteLine();

        try 
        {
            var results = await repository.QueryAsync(new QueryContentRequest()
            {
                ContentQuery = result.Query,        
            }, CancellationToken.None);

            if (results.Count == 0)
            {
                ConsoleWriteLine(ConsoleColor.White, "No results.");
            }
            else
            {
                foreach (var item in results)
                {
                    var contentType = item["Type"].ToString();
                    ConsoleWriteLine(ConsoleColor.White, $"{contentType,-16}\t{item.Name,-25}\t{item.Path}");
                }
                
                Console.WriteLine();
            }        
        } 
        catch (Exception ex) 
        { 
            ConsoleWriteLine(ConsoleColor.Red, ex.Message); 
        }
        
    }
}

async Task<QueryData> GenerateContentQuery(string text, string threadId)
{
    var queryProvider = host.Services.GetRequiredService<IContentQueryGenerator>();
    var result = await queryProvider.GenerateQueryAsync(text, threadId, CancellationToken.None);

    return result;
}


async Task TestContentType()
{
    var threadId = string.Empty;

    // ask for user input and generate a content query until the user enters an empty line
    while(true)
    {
        ConsoleWrite(ConsoleColor.Yellow, "User CTD description> ");

        var text = Console.ReadLine();
        if (string.IsNullOrEmpty(text))
            break;

        var contentTypeProvider = host.Services.GetRequiredService<IContentTypeGenerator>();
        var result = await contentTypeProvider.GenerateContentTypeAsync(text, threadId, CancellationToken.None);
        threadId = result.ThreadId;

        ConsoleWrite(ConsoleColor.Blue, "Content Type> ");
        Console.WriteLine();
        ConsoleWrite(ConsoleColor.White, result.ContentTypeDefinition);
        Console.WriteLine();        
    }
}


async Task TestSummary()
{
    var summaryProvider = host.Services.GetRequiredService<ISummaryGenerator>();
    var text = @"A very short text, but long enough to test the summary feature.";

    var result = await summaryProvider.GenerateSummaryAsync(text, 50, 2, CancellationToken.None);

    Console.WriteLine(result);
}

async Task TestImageGeneration()
{
    var imageGenerator = host.Services.GetRequiredService<IImageGenerator>();
    var text = @"a fox wearing green trousers playing a flute, 
followed by many singing rats, photorealistic";
// var text = @"An old greek philosopher with a long beard and a toga 
// holds a book and teaches a young student in front of a beautiful 
// background of hills and blue sky, cartoon style";

    var imageData = await imageGenerator.GenerateImage(text, 400, 200, CancellationToken.None);

    Console.WriteLine(imageData.Url);
}

void ConsoleWrite(ConsoleColor color, string message)
{
    Console.ForegroundColor = color;
    Console.Write(message);
    Console.ResetColor();
}
void ConsoleWriteLine(ConsoleColor color, string message)
{
    ConsoleWrite(color, message);
    Console.WriteLine();
}


