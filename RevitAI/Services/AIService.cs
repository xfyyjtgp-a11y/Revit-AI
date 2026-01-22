using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RevitAI.Plugins;
using RevitAI.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RevitAI.Services
{
    public class AIService
    {
        private readonly Kernel _baseKernel;
        private readonly IChatCompletionService _chatService;

        public AIService(string apiKey, string modelId = "gpt-4o-mini")
        {
            // Initialize Semantic Kernel with OpenAI
            var builder = Kernel.CreateBuilder();

            // Configure HttpClient (Dev/Debug)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            var httpClient = new HttpClient(handler);
            var endpoint = "https://api.openai-proxy.org/v1";
            httpClient.BaseAddress = new Uri(endpoint);
            
            builder.AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient);
            
            _baseKernel = builder.Build();
            _chatService = _baseKernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string?> ProcessRequestJsonAsync(string userInput)
        {
            var tasks = await ProcessRequestAsync(userInput);
            if (tasks == null || !tasks.Any()) return null;
            return JsonSerializer.Serialize(tasks);
        }

        public async Task<List<RevitTask>> ProcessRequestAsync(string userInput)
        {
            // Create a plugin instance to capture the tool call
            var designPlugin = new RevitDesignPlugin();
            
            // Create a plugin collection and add our plugin
            var plugins = new KernelPluginCollection();
            plugins.AddFromObject(designPlugin, "RevitDesign");

            // Create a scoped kernel that shares the base services but has specific plugins
            var scopedKernel = new Kernel(_baseKernel.Services, plugins);

            var history = new ChatHistory();
            history.AddSystemMessage("You are a Revit AI assistant. Help the user create architectural elements by calling the appropriate functions. Always use the tools provided.");
            history.AddUserMessage(userInput);

            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            try
            {
                // Pass the scoped kernel so the chat service can find and invoke the plugin
                await _chatService.GetChatMessageContentAsync(history, settings, scopedKernel);

                // Return all captured tasks
                return designPlugin.PendingTasks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI Error: {ex.Message}");
                throw;
            }
        }
    }
}
