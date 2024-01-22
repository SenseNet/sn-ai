using Microsoft.SemanticKernel;
using SenseNet.AI.BlazorWebApp;
using SenseNet.AI.BlazorWebApp.Client.Pages;
using SenseNet.AI.BlazorWebApp.Components;
using SenseNet.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSenseNetSemanticKernel(options =>
{
    builder.Configuration.GetSection("sensenet:ai:text:SemanticKernel").Bind(options);
    options.ConfigureDefaultPlugins = (plugins, serviceProvider) =>
    {
        // pass on the IServiceProvider to the API to let it resolve dependencies
        plugins.AddFromType<SenseNetKernelPlugin>(serviceProvider: serviceProvider);
    };
});

builder.Services.AddSenseNetClient()
        .ConfigureSenseNetRepository(options =>
        {
            builder.Configuration.GetSection("sensenet:repository").Bind(options);
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
