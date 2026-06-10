namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Orchestrator that coordinates multiple AI agents for complex tasks
/// </summary>
public interface IAiOrchestrator
{
    /// <summary>
    /// Process a complete safety request using multiple agents
    /// </summary>
    Task<CompleteSafetyAnalysis> ProcessSafetyRequestAsync(SafetyRequest request, CancellationToken cancellationToken = default);
}

public class SafetyRequest
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public TimeOfDay TimeOfDay { get; set; } = TimeOfDay.Daytime;
    public TransportMode TransportMode { get; set; } = TransportMode.Walking;
    public string? UserId { get; set; }
}

public class CompleteSafetyAnalysis
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public RouteAnalysisResult RouteAnalysis { get; set; } = new();
    public SafetyAdvice PersonalSafetyAdvice { get; set; } = new();
    public List<AlternativeRoute> AlternativeRoutes { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}