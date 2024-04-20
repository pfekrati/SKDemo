using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Options;
using Plugins;
using System;
using System.Threading.Tasks;

namespace SKDemo
{
    class SKDemo
    {
        static async Task Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            HostApplicationBuilder builder = Host.CreateApplicationBuilder();
            builder.Services.Configure<CallAutomationApiOptions>(builder.Configuration.GetSection("CallAutomationApiOptions"));
            builder.Services.Configure<BingSearchOptions>(builder.Configuration.GetSection("BingSearchOptions"));
            var host = builder.Build();

            // Create a kernel builder
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(host.Services.GetRequiredService<IOptions<BingSearchOptions>>().Value);
            kernelBuilder.Services.AddSingleton(host.Services.GetRequiredService<IOptions<CallAutomationApiOptions>>().Value);
            kernelBuilder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

            #region AddAzureOpenAIChatCompletion
            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: "",
                endpoint: "",
                apiKey: "",
                modelId: "gpt-4" // optional
            );
            #endregion


            Kernel kernel = kernelBuilder.Build();
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory history = new();
            history.AddSystemMessage("""
                You are an AI assistant that can help with answering questions, making phone calls, and searching the web.
            """);
            bool firstChat = true;
            while (true)
            {
                // Get the user's input
                Console.BackgroundColor = ConsoleColor.Black;
                if (firstChat) Console.Write("You > ");
                else
                {
                    Console.WriteLine("");
                    Console.Write("You > ");
                }
                var input = Console.ReadLine();
                history.AddUserMessage(input);

                // Generate the bot's response using the chat completion service
                var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                    chatHistory: history,
                    executionSettings: new OpenAIPromptExecutionSettings()
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    kernel: kernel
                ).ConfigureAwait(false);

                // Stream the bot's response to the console
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write("Bot > ");
                string botResponse = "";
                await foreach (var message in response)
                {
                    botResponse += message.ToString();
                    Console.Write(message.ToString());
                }
                history.AddAssistantMessage(botResponse);
                firstChat = false;
            }
        }

    }
}
