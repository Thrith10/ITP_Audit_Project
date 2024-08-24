using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Services;

namespace PKFAuditManagement.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IOpenAIService _openAIService;

        public ChatbotController(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        [HttpPost]
        public async Task<IActionResult> GetChatResponse(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return BadRequest("User input cannot be empty.");
            }

            var response = await _openAIService.GetChatResponseAsync(userInput);
            return Json(new { response });
        }
    }
}
