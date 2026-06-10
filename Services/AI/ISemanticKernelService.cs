namespace RutaSeguraMvc.Services.AI;

/// <summary>
/// Interface for Semantic Kernel operations
/// </summary>
public interface ISemanticKernelService
{
    /// <summary>
    /// Execute a prompt and get a completion response
    /// </summary>
    Task<string> ExecutePromptAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a prompt with structured output
    /// </summary>
    Task<T> ExecutePromptAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Execute a multi-turn conversation
    /// </summary>
    Task<string> ExecuteConversationAsync(List<(string Role, string Content)> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoke a specific function/plugin
    /// </summary>
    Task<string> InvokeFunctionAsync(string pluginName, string functionName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
}