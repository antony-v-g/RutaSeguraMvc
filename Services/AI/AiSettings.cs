namespace RutaSeguraMvc.Services.AI;

/// <summary>
/// Configuration settings for AI/LLM services
/// </summary>
public class AiSettings
{
    public string ApiType { get; set; } = "OpenAI"; // OpenAI or AzureOpenAI
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o-mini";
    public string Endpoint { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
}