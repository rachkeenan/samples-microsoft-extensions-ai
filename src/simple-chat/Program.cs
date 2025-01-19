using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using OpenAI;

var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddEnvironmentVariables("OPENAI_API_KEY");

IChatClient innerChatClient = new OpenAIClient(hostBuilder.Configuration["OPENAI_API_KEY"])
    .AsChatClient("gpt-4o-mini");

hostBuilder.Services.AddSingleton<IChatClient>(innerChatClient);

hostBuilder.Services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace));

var app = hostBuilder.Build();
var chatClient = app.Services.GetRequiredService<IChatClient>();

var responseStream = chatClient.CompleteStreamingAsync("What is AI?");
await foreach (var message in responseStream)
{
    Console.Write(message.Text);
}