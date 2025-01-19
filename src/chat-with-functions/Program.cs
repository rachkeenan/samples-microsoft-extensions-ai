using System.ComponentModel;
using Microsoft.Extensions.AI;
using OpenAI;

IChatClient innerChatClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .AsChatClient("gpt-4o-mini");

var chatClientBuilder = new ChatClientBuilder(innerChatClient).UseFunctionInvocation();

IChatClient client = chatClientBuilder.Build();

var messages = new List<ChatMessage>()
{
    new(ChatRole.System, """
                         You answer any questions, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
                         If the user agrees to buy socks, find out how many pairs they want, then add socks to their cart.
                         """)
};

var cart = new Cart();
ChatOptions chatOptions = new()
{
    Tools = [AIFunctionFactory.Create(cart.GetPrice), AIFunctionFactory.Create(cart.AddSocksToCart)]
};

while (true)
{
    // Get input
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(("\n\n> "));
    var input = Console.ReadLine();
    messages.Add(new(ChatRole.User, input));

    // Get reply
    try
    {
        var response = await client.CompleteAsync(messages, chatOptions);
        messages.Add(response.Message);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(response.Message.Text);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
    }
}


class Cart
{
    public int NumPairsOfSocks { get; set; }

    public void AddSocksToCart(int numPairs)
    {
        NumPairsOfSocks += numPairs;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"********");
        Console.WriteLine($"Added {numPairs} pairs to cart. \n\nTotal: {NumPairsOfSocks} pairs");
        Console.WriteLine($"********");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Computes the price of socks, returning the value in dollars")]
    public float GetPrice(
        [Description("The number of pairs of socks to calculate price for")]
        int count)
        => count * 15.99f;
}