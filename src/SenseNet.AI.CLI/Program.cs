using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.AI.Text;
using SenseNet.AI.Vision;
using SenseNet.Client;
using SenseNet.Extensions.DependencyInjection;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSenseNetSemanticKernel(options => 
                hostContext.Configuration.GetSection("SemanticKernel").Bind(options))
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
//var result = await TestSummary(host);

// Content Query
//var result = await TestContentQuery(host);

// Azure Vision
//var result = await TestImageGeneration(host);

//Console.WriteLine(result);

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

    var result = await TestContentQuery(host, text, threadId);
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
                ConsoleWriteLine(ConsoleColor.White, $"{contentType,-16}\t{item.Name}\t{item.Path}");
            }
            
            Console.WriteLine();
        }        
    } 
    catch (Exception ex) 
    { 
        ConsoleWriteLine(ConsoleColor.Red, ex.Message); 
    }
    
}

async Task<QueryData> TestContentQuery(IHost host1, string text, string threadId)
{
    var queryProvider = host1.Services.GetRequiredService<IContentQueryProvider>();
    //var text = @"get all tasks created yesterday";

    var result = await queryProvider.GetQuery(text, threadId, CancellationToken.None);

    return result;
}

async Task<string> TestSummary(IHost host1)
{
    var summaryProvider = host1.Services.GetRequiredService<ISummaryProvider>();
    var text = @"A very short text, but long enough to test the summary feature.";

    var result = await summaryProvider.GetSummary(text, 50, 2, CancellationToken.None);

    return result;
}

async Task<ImageData> TestImageGeneration(IHost host2)
{
    var imageGenerator = host2.Services.GetRequiredService<IImageGenerator>();
    var text = @"a fox wearing green trousers playing a flute, 
followed by many singing rats, photorealistic";
// var text = @"An old greek philosopher with a long beard and a toga 
// holds a book and teaches a young student in front of a beautiful 
// background of hills and blue sky, cartoon style";

    var imageData = await imageGenerator.GenerateImage(text, 400, 200, CancellationToken.None);

    return imageData;
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


