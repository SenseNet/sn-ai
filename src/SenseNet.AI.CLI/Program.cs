using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.AI.Text;
using SenseNet.AI.Vision;
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

        services.AddLogging((loggingBuilder) =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        });
    });

var host = hostBuilder.Build();

// Summary
var result = await TestSummary(host);

// Azure Vision
//var result = await TestImageGeneration(host);

Console.WriteLine(result);

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


