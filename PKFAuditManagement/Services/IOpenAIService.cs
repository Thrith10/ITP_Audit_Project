
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

namespace PKFAuditManagement.Services
{
    public interface IOpenAIService
    {
        Task<string> GetChatResponseAsync(string userInput, string retrievalInput, string documentNames);
        Task<string> GetNewChatResponseAsync(string userInput, IReadOnlyCollection<Document> retrievalInput);
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

        public async Task<string> GetChatResponseAsync(string userInput, string retrievalInput, string documentNames)
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
                - **Use headings** to separate different sections of the response (e.g., '1.Introduction', '2.Ethical Standards', etc.).
                - Ensure that the response is broken down clearly and logically so that the auditor can easily follow it.

                # EXAMPLE FORMAT #
                For example, format your response like this:
                **Serving as a Director or Officer of a Client**

                According to the guidelines and standards, staff members of an audit firm should not serve as directors or officers of a client due to the following reasons:

                **1. Independence Threats:**
                   - Serving as a director or officer of an audit client poses significant independence threats because it can compromise objectivity and impartiality in the audit process.
                
                Do not bold words once inside the paragraph as such  - **Compromise of Objectivity and Impartiality**:. Keep it - Compromise of Objectivity and Impartiality:
                
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
                return FormatResponse(messageContent, documentNames);
                //return messageContent;
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        }

        public async Task<string> GetNewChatResponseAsync(string userInput, IReadOnlyCollection<Document> retrievalInput)
        {
            try
            {
                // Convert to a Single String 
                string concatenatedContent = retrievalInput.AsString();

                // Retrieve the chat history from memory cache
                var chatHistory = _memoryCache.TryGetValue(ChatHistoryCacheKey, out List<Message> cachedHistory) ? cachedHistory : new List<Message>();

                // Initialise the OpenAI client
                using var api = new OpenAIClient(_apiKey);

                // Define the COSTAR components for PKF-CAP audit firm
                string context = $@"
                # CONTEXT #
                You are an assistant at PKF-CAP, an audit firm, responding to frequently asked questions by new auditors regarding documentation and auditing processes. You must use the knowledge base provided as the primary source of information, which is: {concatenatedContent}. The information provided from the knowledge base is strictly not to be changed, as each statement has a clause attached to it.

                Instructions:
                1. Provide responses based on the knowledge base, using only information that is relevant to the question.
                2. You are to reply to questions related to the audit domain only, if asked about other domains, mention that you are unable to respond and lead with a question about auditing.
                3. Include the clause number and text for each of the statements, if there are no clauses identified, just don't list the clause.

                Examples (for direct clause requests):
                - Clause: R114.2
                  - Explanation: A professional accountant must not disclose confidential information except under specific circumstances.
                - Clause: 114.3 A1
                  - Explanation: Confidentiality serves the public interest by ensuring information flow.
                - Clause: 120.10 A2
                  - Explanation: Safeguards are actions, individually or in combination, that the professional accountant takes that effectively reduce threats to compliance with the fundamental principles to an acceptable level.
            
                # OBJECTIVE #
                Your task is to provide clear, accurate, and instructional responses based solely on the knowledge base provided. 
            
                # Style #
                Write in an informative and instructional style, avoiding personal opinions and ensuring factual accuracy.
            
                # TONE #
                Maintain a professional and confident tone throughout the conversation. This is work, not a game.
            
                # AUDIENCE #
                Your target audience consists of new auditors who are currently in training and need guidance on audit-related documentation and processes.
            
                # RESPONSE FORMAT #
                Your response should ideally be structured as follows. Additionally, take note of [question here], this should be the question that is posed by the user, and follow-up with a relevant question you deem necessary.
                   <div class='response-section'>
                       <strong>{{{{summary}}}}</strong> <br>
                       <ul>
                           {{{{supportingDetails}}}}
                       </ul>
                       <div class='citation'>If you have more questions about [question here], please feel free to ask.</div>
                   </div>
                ";

                // Initialize chat history if it does not exist
                if (chatHistory == null || !chatHistory.Any())
                {
                    chatHistory = new List<Message> { new Message(Role.System, context) };
                }
                else
                {
                    // Update the system context message with the latest content
                    chatHistory[0] = new Message(Role.System, context);
                }

                // Add the user input to the chat history
                chatHistory.Add(new Message(Role.User, userInput));

                // Create the chat request with temperature setting
                var chatRequest = new ChatRequest(
                 messages: chatHistory,
                 model: Model.GPT4o,
                 temperature: 0.5 
                );

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
                return messageContent;
            }
            catch (Exception ex)
            {
                return "An error occurred while processing your request.";
            }
        } 

        private string FormatResponse(string messageContent, string documentNames)
        {
            // Convert '**' to HTML bold tags for text formatting
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

            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(?<=\n|^)(\d+\.\s+)([^\n:]+):", // Matches numbered headings like "1. Cooling-off Period:"
                "**$1$2**:" // Wraps the number and heading in bold (**)
            );


            // Format sections to ensure numbered headings are bold and appear together without line breaks
            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(\d+)\.\s*(\*\*[^*]+\*\*)",
                "<b>$1. $2</b>"
            );

            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(?<!^|\n)(<b>\d+\.)", // Look for a bold tag followed by a numbered heading, not already preceded by a line break
                "\n$1" // Add a line break before the <b> tag and numbered heading
            );

            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(?<!\n)-\s*<b>(.*?)<\/b>", // Matches a dash followed by bold text, without a preceding newline
                "\n- $1" // Adds a newline before the dash and removes the bold tags around the text
            );

            //
            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(?<=<br>|^|\n)-\s+(?=[A-Z])", // Matches dashes at the start of a list item, but not in compound words
                "<br>-" // Adds a <br> tag before the dash
            );


            messageContent = System.Text.RegularExpressions.Regex.Replace(
                messageContent,
                @"(?<=\n)(<b>\d+\.\s+[^<]+<\/b>)", // Matches bold numbered headings that are preceded by a newline
                "<br><br>$1" // Adds a <br> before the matched heading
            );


            // Append citation at the end with line breaks for clear separation
            return $"{messageContent}<br><br><i>Cited from \"{documentNames.ToUpper()}\"</i>";
        }







    }
}


