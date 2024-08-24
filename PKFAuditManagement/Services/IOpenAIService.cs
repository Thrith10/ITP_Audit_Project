using OpenAI.Chat;

namespace PKFAuditManagement.Services
{
    public interface IOpenAIService
    {
        Task<string> GetChatResponseAsync(string userInput);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly string _apiKey;
        private readonly ChatClient _chatClient;

        public OpenAIService(string apiKey)
        {
            _apiKey = apiKey;
            _chatClient = new ChatClient("gpt-4o-mini", _apiKey);
        }

        public async Task<string> GetChatResponseAsync(string userInput)
        {
            try
            {
                var chatCompletion = await _chatClient.CompleteChatAsync(
                    new[] { new UserChatMessage(userInput) }
                );


                // Ensure the response is not null and contains content
                if (chatCompletion?.Value?.Content != null && chatCompletion.Value.Content.Any())
                {
                    return chatCompletion.Value.Content[0].ToString();
                }
                else
                {
                    // Handle the case where there is no content or an empty response
                    return "No response received from the chat service.";
                }
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        }
    }
}
