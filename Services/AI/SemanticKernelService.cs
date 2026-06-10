using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace RutaSeguraMvc.Services.AI;

/// <summary>
/// Service for interacting with Semantic Kernel and LLM
/// </summary>
public class SemanticKernelService : ISemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly ILogger<SemanticKernelService> _logger;
    private readonly AiSettings _aiSettings;

    public SemanticKernelService(Kernel kernel, ILogger<SemanticKernelService> logger, IOptionsSnapshot<AiSettings> aiSettings)
    {
        _kernel = kernel;
        _logger = logger;
        _aiSettings = aiSettings.Value;
    }

    public async Task<string> ExecutePromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing prompt: {Prompt}", prompt[..Math.Min(100, prompt.Length)]);

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
            var messages = new List<ChatMessageContent>
            {
                new(AuthorRole.User, prompt)
            };

            var result = await chatCompletion.GetChatMessageContentAsync(
                messages,
                new()
                {
                    MaxTokens = _aiSettings.MaxTokens,
                    Temperature = _aiSettings.Temperature
                },
                _kernel,
                cancellationToken);

            var response = result.Content ?? string.Empty;
            _logger.LogInformation("Prompt execution completed. Response length: {Length}", response.Length);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing prompt");
            throw;
        }
    }

    public async Task<T> ExecutePromptAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var response = await ExecutePromptAsync(prompt, cancellationToken);

            // Try to parse as JSON
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response[jsonStart..(jsonEnd + 1)];
                var deserialized = JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (deserialized != null)
                    return deserialized;
            }

            throw new InvalidOperationException($"Could not deserialize response to type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing structured prompt");
            throw;
        }
    }

    public async Task<string> ExecuteConversationAsync(List<(string Role, string Content)> messages, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing conversation with {MessageCount} messages", messages.Count);

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
            var chatMessages = messages.Select(m => new ChatMessageContent(
                m.Role == "assistant" ? AuthorRole.Assistant : m.Role == "system" ? AuthorRole.System : AuthorRole.User,
                m.Content)).ToList();

            var result = await chatCompletion.GetChatMessageContentAsync(
                chatMessages,
                new()
                {
                    MaxTokens = _aiSettings.MaxTokens,
                    Temperature = _aiSettings.Temperature
                },
                _kernel,
                cancellationToken);

            return result.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing conversation");
            throw;
        }
    }

    public async Task<string> InvokeFunctionAsync(string pluginName, string functionName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Invoking function {PluginName}.{FunctionName}", pluginName, functionName);

            var function = _kernel.Plugins.GetFunction(pluginName, functionName);
            if (function == null)
                throw new InvalidOperationException($"Function {pluginName}.{functionName} not found");

            var result = await _kernel.InvokeAsync(function, new KernelArguments(parameters) { }, cancellationToken);

            return result.GetValue<object>()?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking function {PluginName}.{FunctionName}", pluginName, functionName);
            throw;
        }
    }
}