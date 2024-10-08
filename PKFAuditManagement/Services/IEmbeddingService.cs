using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Models;

namespace PKFAuditManagement.Services
{
    public interface IEmbeddingService
    {
        Task<double[]> GenerateEmbeddingsAsync(string userInput);
    }

    public class EmbeddingService : IEmbeddingService
    {
        private readonly string _apiKey;

        public EmbeddingService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<double[]> GenerateEmbeddingsAsync(string userInput)
        {
            try
            {
                using var api = new OpenAIClient(_apiKey);
                var response = await api.EmbeddingsEndpoint.CreateEmbeddingAsync(userInput, Model.Embedding_Ada_002);

                // Notice that the resulting embedding is a list (also called a vector) of floating point numbers represented as an instance of ReadOnlyMemory<float>. By default, the length of the embedding vector will be 1536 when using the text-embedding-3-small model or 3072 when using the text-embedding-3-large model. Generally, larger embeddings perform better, but using them also tends to cost more in terms of compute, memory, and storage. You can reduce the dimensions of the embedding by creating an instance of the EmbeddingGenerationOptions class, setting the Dimensions property, and passing it as an argument in your call to the GenerateEmbedding method:
                IReadOnlyList<double> vector = response.Data[0].Embedding;

                return vector.ToArray(); // Return the float array
            }
            catch (Exception ex)
            {
                return Array.Empty<double>(); // Return an empty array on error
            }
        }
    }
}
