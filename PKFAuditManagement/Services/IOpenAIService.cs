
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
                You are an assistant at PKF-CAP, an audit firm, responding to frequently asked questions by new auditors regarding documentation and auditing processes. You must use the knowledge base provided as the primary source of information. Do not rely on general knowledge unless the retrieved data from the knowledge base is insufficient.
            
                # OBJECTIVE #
                Your task is to provide clear, accurate, and instructional responses based solely on the knowledge base provided. The following data was retrieved from the knowledge base: {retrievalInput}.
            
                # Style #
                Write in an informative and instructional style, avoiding personal opinions and ensuring factual accuracy.
            
                # TONE #
                Maintain a professional and confident tone throughout the conversation. This is work, not a game.
            
                # AUDIENCE #
                Your target audience consists of new auditors who are currently in training and need guidance on audit-related documentation and processes.
            
                # RESPONSE FORMAT #
                - Provide your responses using the information from the knowledge base first and foremost.
                - **Format** the information retrieved from the knowledge base as **bullet points** or **numbered lists**.
                - **Use headings** to separate different sections of the response (e.g., '1. Introduction', '2. Ethical Standards', etc.).
                - Highlight any important points retrieved from the database with **bold** or _italic_ for emphasis.
                - Ensure that the response is broken down clearly and logically so that the auditor can easily follow it.

                # EXAMPLE FORMAT #
                For example, format your response like this:
                1. **Code of Conduct**
                   - _The code of conduct outlines the following principles:_
                   - **Integrity**: Auditors must act with honesty and fairness.
                   - **Objectivity**: Professional judgment should not be biased by outside influence.
                
                # DATABASE CONTENT #
                Here is the retrieved data from the knowledge base that must be used in your response:{retrievalInput}
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

                // Optionally, apply formatting to the response (custom formatting logic)
                return FormatResponse(messageContent);
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        }

        // Optional: Apply custom formatting to the response
        private string FormatResponse(string messageContent)
        {
            // Replace '**' with HTML-style bold tags.
            // Ensuring that alternating ** are correctly converted to <b> and </b>
            bool isBold = false;
            messageContent = System.Text.RegularExpressions.Regex.Replace(messageContent, @"\*\*", match =>
            {
                if (isBold)
                {
                    isBold = false;
                    return "</b>";
                }
                else
                {
                    isBold = true;
                    return "<b>";
                }
            });

            // Replace '_' with HTML-style italic tags.
            // Ensuring that alternating _ are correctly converted to <i> and </i>
            bool isItalic = false;
            messageContent = System.Text.RegularExpressions.Regex.Replace(messageContent, @"_", match =>
            {
                if (isItalic)
                {
                    isItalic = false;
                    return "</i>";
                }
                else
                {
                    isItalic = true;
                    return "<i>";
                }
            });

            return messageContent;
            //return $"{messageContent}{Environment.NewLine}{Environment.NewLine}Cited from \"{documentName}\"";
        }

    }
}


