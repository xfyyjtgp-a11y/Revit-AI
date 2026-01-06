using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RevitAI.Services
{
    public class AIService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public AIService(string apiKey, string modelId = "gpt-4o-mini")
        {
            // Initialize Semantic Kernel with OpenAI
            // NOTE: Ideally, use dependency injection or configuration for API keys.
            var builder = Kernel.CreateBuilder();

            // 配置 HttpClient 以忽略 SSL 证书错误 (仅用于开发/调试环境)
            // 如果你在公司网络或使用了代理，可能需要这个
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            var httpClient = new HttpClient(handler);
            var endpoint = "https://api.openai-proxy.org/v1";
            httpClient.BaseAddress = new Uri(endpoint);
            builder.AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient);
            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        /// <summary>
        /// Helper method to return JSON string directly, easier for cross-context calls.
        /// </summary>
        public async Task<string?> ParseWallRequestJsonAsync(string userInput)
        {
            var result = await ParseWallRequestAsync(userInput);
            if (result == null) return null;
            return JsonSerializer.Serialize(result);
        }

        public async Task<WallRequest?> ParseWallRequestAsync(string userInput)
        {
            var history = new ChatHistory();
            history.AddSystemMessage(@"You are a Revit AI assistant. 
Your goal is to extract wall creation parameters from the user's natural language request.
Return ONLY a valid JSON object. Do not include markdown code blocks (```json ... ```).
The JSON must follow this schema:
{
    ""Length"": (number, length in meters),
    ""Height"": (number, height in meters),
    ""LevelName"": (string, name of the level, default to 'Level 1' if not specified)
}
Example output:
{ ""Length"": 5.5, ""Height"": 3.0, ""LevelName"": ""Level 1"" }");

            history.AddUserMessage(userInput);

            try
            {
                var result = await _chatService.GetChatMessageContentAsync(history);
                string content = result.Content ?? "{}";

                // Simple cleanup if LLM returns markdown code blocks despite instructions
                content = content.Replace("```json", "").Replace("```", "").Trim();

                return JsonSerializer.Deserialize<WallRequest>(content);
            }
            catch (Exception ex)
            {
                // Handle parsing errors or API errors
                System.Diagnostics.Debug.WriteLine($"AI Error: {ex.Message}");
                // Rethrow to see error in caller
                throw;
            }
        }
    }
}
