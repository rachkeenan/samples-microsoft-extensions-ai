using Microsoft.Extensions.AI;
using OpenAI;
using System.Numerics.Tensors;


internal class Program
{
    private static async Task Main(string[] args)
    {

        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator
    = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
        .AsEmbeddingGenerator("text-embedding-3-small");


        // Basic embedding
        var embeddingResult = BasicEmbedding(embeddingGenerator);
        Console.WriteLine(embeddingResult.Result);

        //// Semantic search
        var candidates = new string[] { "Onboarding process for new employees", "Understanding Our Company Values", "Navigating the Office Layout", "Accessing Car Park E", "Dress Code Guidelines", "Using the Company Intranet", "Employee Benefits Overview", "Requesting Time Off", "Reporting Workplace Incidents", "Office Etiquette and Conduct" };

        Console.WriteLine("Generating embeddings for candidates...");
        var candidateEmbeddings = await embeddingGenerator.GenerateAndZipAsync(candidates);

        Console.WriteLine(candidateEmbeddings);

        while (true)
        {
            Console.WriteLine("\nQuery: ");
            var input = Console.ReadLine();
            if (input == "") break;

            var inputEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(input);

            var closest =
                from candidate in candidateEmbeddings
                let similarity = TensorPrimitives.CosineSimilarity(
                    candidate.Embedding.Vector.Span, inputEmbedding.Vector.Span)
                orderby similarity descending
                select new { Text = candidate.Value, Similarity = similarity };

            foreach (var c in closest.Take(3))
            {
                Console.WriteLine($"({c.Similarity}): {c.Text}");
            }
        }
    }

    public static async Task<string> BasicEmbedding(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        var result = await embeddingGenerator.GenerateEmbeddingAsync("Cats are better than dogs");

        //Console.WriteLine($"Vector of length: {result.Vector.Length}");

        //foreach (var value in result.Vector.ToArray())
        //{
        //    Console.WriteLine("{0:0.00}, ", value);
        //}

        return @$"Vector of length: {result.Vector.Length}
                {string.Join("\t", result.Vector.ToArray())}";
    }
}