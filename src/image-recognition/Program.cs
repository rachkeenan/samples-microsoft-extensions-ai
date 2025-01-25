using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using OpenAI;

var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddEnvironmentVariables("OPENAI_API_KEY");

IChatClient innerChatClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .AsChatClient("gpt-4o-mini");

hostBuilder.Services.AddSingleton<IChatClient>(innerChatClient);

hostBuilder.Services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace));

var app = hostBuilder.Build();
var chatClient = app.Services.GetRequiredService<IChatClient>();

// Single image
// var message = new ChatMessage(ChatRole.User, "What's in this image?");
// message.Contents.Add(new ImageContent(
//     File.ReadAllBytes(
//         "<path to your image"),
//     "image/jpg"));
//
// var response = await chatClient.CompleteAsync([message]);
// Console.WriteLine(response.Message.Text);

// Multiple images
var dir = Path.Combine(AppContext.BaseDirectory, "<path to your image folder>");
foreach (var imagePath in Directory.GetFiles(dir, "*.jpg"))
{
    var name = Path.GetFileNameWithoutExtension(imagePath);

    var message = new ChatMessage(ChatRole.User, $$"""
                                                   Extract information from this image from camera {{name}}.
                                                   """);
    message.Contents.Add(new ImageContent(File.ReadAllBytes(imagePath), "image/jpg"));

    var response = await chatClient.CompleteAsync<TrafficCamResult>([message]);
    // var response = await chatClient.CompleteAsync<AnimalsResult>([message]);

    if (response.TryGetResult(out var result))
    {
        Console.WriteLine($"{name} status: {result.Status} (cars: {result.NumCars}, trucks: {result.NumTrucks})");
        // Console.WriteLine($"{name} - total animals: {result.NumAnimals} - (dogs: {result.NumDogs}, cats: {result.NumCats}, racoons: {result.NumRacoons}, monkeys: {result.NumMonkeys}, red pandas: {result.NumRedPandas}, meerkats: {result.NumMeerkats})");
    }
}

class TrafficCamResult
{
    public TrafficStatus Status { get; set; }
    public int NumCars { get; set; }
    public int NumTrucks { get; set; }

    public enum TrafficStatus
    {
        Clear,
        Flowing,
        Congested,
        Blocked
    };
}

class AnimalsResult
{
    public int NumAnimals { get; set; }
    public int NumDogs { get; set; }
    public int NumCats { get; set; }
    public int NumRacoons { get; set; }
    public int NumMonkeys { get; set; }
    public int NumRedPandas { get; set; }
    public int NumMeerkats { get; set; }
}