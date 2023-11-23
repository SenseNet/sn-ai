using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.AI.Abstractions;
using SenseNet.Extensions.DependencyInjection;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSemanticKernel(options => 
                hostContext.Configuration.GetSection("SemanticKernel").Bind(options));

        services.AddLogging((loggingBuilder) =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        });
    });

var host = hostBuilder.Build();

var summaryProvider = host.Services.GetRequiredService<ISummaryProvider>();
var text = @"";

var result = await summaryProvider.GetSummary(text, CancellationToken.None);

Console.WriteLine(result);
