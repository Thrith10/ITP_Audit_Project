
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

namespace PKFAuditManagement.Services
{
    public interface IOpenAIService
    {
        Task<string> GetChatResponseAsync(string userInput, string retrievalInput);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly string _apiKey;
        private readonly IMemoryCache _memoryCache;
        private const string ChatHistoryCacheKey = "ChatHistory";

        // List to hold chat history (stored in memory during the session)
        private List<Message> chatHistory = new List<Message>();

        public OpenAIService(string apiKey, IMemoryCache memoryCache)
        {
            _apiKey = apiKey;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetChatResponseAsync(string userInput, string retrievalInput)
        {
            try
            {
                // Retrieve the chat history from memory cache
                var chatHistory = _memoryCache.TryGetValue(ChatHistoryCacheKey, out List<Message> cachedHistory) ? cachedHistory : new List<Message>();

                // Initialise the OpenAI client
                using var api = new OpenAIClient(_apiKey);

                // Define the COSTAR components for PKF-CAP audit firm
                string context = $@"
                # CONTEXT #
                You are an assistant at PKF-CAP, an audit firm, responding to frequently asked questions by new auditors regarding documentation and auditing processes. There is an existing knowledge base provided and you are to use this knowledge base.
            
                # OBJECTIVE #
                Your task is to provide clear, accurate, and instructional responses to FAQs posed by new auditors undergoing training.
            
                # Style #
                Write in an informative and instructional style, avoiding personal opinions and ensuring factual accuracy.
            
                # TONE #
                Maintain a professional and confident tone throughout the conversation. This is work, not a game.
            
                # AUDIENCE #
                Your target audience consists of new auditors who are currently in training and need guidance on audit-related documentation and processes.
            
                # RESPONSE FORMAT #
                Start off by providing responses based on the knowledge base that is retrieved which is: {retrievalInput}.
                ";

                // Initialize chat history if it does not exist
                if (chatHistory == null || !chatHistory.Any())
                {
                    chatHistory = new List<Message> { new Message(Role.System, context) };
                }

                // Add the user input to the chat history
                chatHistory.Add(new Message(Role.User, userInput));

                // Create the chat request
                var chatRequest = new ChatRequest(chatHistory, Model.GPT4o);

                // Get the response from the OpenAI API
                var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
                var choice = response.FirstChoice;
                
                // Retrieve message content from the response
                string messageContent = choice.Message.Content.ToString();

                // Add the assistant's response to the chat history
                chatHistory.Add(new Message(Role.Assistant, messageContent));

                // Store the updated chat history in memory cache
                _memoryCache.Set(ChatHistoryCacheKey, chatHistory);

                // Return message 
                return messageContent;
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        }
    }
}
