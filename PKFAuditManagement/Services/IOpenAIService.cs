
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

        public OpenAIService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetChatResponseAsync(string userInput, string retrievalInput)
        {
            try
            {
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

                // Create the messages list including context and user input
                var messages = new List<Message>
                {
                    new Message(Role.System, context),
                    new Message(Role.User, userInput)
                };

                // Create a ChatRequest with the messages and a model
                var chatRequest = new ChatRequest(messages, Model.GPT4o);

                // Get the completion from the API
                var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
                var choice = response.FirstChoice;

                return choice;
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        }
    }
}
