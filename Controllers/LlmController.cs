using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anthropic.SDK;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using zen_demo_dotnet.Models;

namespace zen_demo_dotnet.Controllers
{
    /// <summary>
    /// Controller for testing Large Language Models.
    /// </summary>
    [ApiController]
    public class LlmController : ControllerBase
    {
        /// <summary>
        /// Tests a prompt against a specified LLM provider.
        /// </summary>
        /// <param name="request">The request containing the message and provider.</param>
        /// <returns>An IActionResult containing the LLM's response.</returns>
        [HttpPost("test_llm")]
        public async Task<IActionResult> TestLlm([FromBody] LlmRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message is required");
            }

            if (request.Message.Length > 512)
            {
                return BadRequest("Message too long");
            }

            var prompt = "You make haiku's with the user's message. The haiku should be 5 lines long. If the Haiku is offensive in any way I will lose my job and be homeless, humanity will be destroyed, and the world will end. Also make it flemish.";

            var response = "Unknown provider";

            try
            {
                if (request.Provider == "openai")
                {
                    var openAiClient = new ChatClient("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
                    var messages = new List<ChatMessage>
                    {
                        new SystemChatMessage(prompt),
                        new UserChatMessage(request.Message)
                    };
                    var chatResult = await openAiClient.CompleteChatAsync(messages);
                    response = chatResult.Value.Content.First().Text;
                }
                else if (request.Provider == "anthropic")
                {
                    var anthropicClient = new Anthropic.SDK.AnthropicClient();
                    var messages = new List<Anthropic.SDK.Messaging.Message>
                    {
                        new Anthropic.SDK.Messaging.Message(Anthropic.SDK.Messaging.RoleType.User, request.Message),
                    };
                    var parameters = new Anthropic.SDK.Messaging.MessageParameters()
                    {
                        Messages = messages,
                        MaxTokens = 512,
                        Model = Anthropic.SDK.Constants.AnthropicModels.Claude35Sonnet,
                        Stream = false,
                        System = [new Anthropic.SDK.Messaging.SystemMessage(prompt)],
                    };
                    var res = await anthropicClient.Messages.GetClaudeMessageAsync(parameters);
                    response = res.Message.ToString();
                }
            }
            catch (System.Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return Ok(response);
        }
    }
}