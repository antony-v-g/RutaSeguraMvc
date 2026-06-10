namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Agent for providing personalized safety advice and tips
/// </summary>
public interface ISafetyAssistantAgent
{
    /// <summary>
    /// Get safety tips for a specific scenario
    /// </summary>
    Task<SafetyAdvice> GetSafetyAdviceAsync(SafetyScenario scenario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get emergency response recommendations
    /// </summary>
    Task<string> GetEmergencyAdviceAsync(EmergencyType emergencyType, string context, CancellationToken cancellationToken = default);
}

public class SafetyScenario
{
    public string Location { get; set; } = string.Empty;
    public TimeOfDay TimeOfDay { get; set; } = TimeOfDay.Daytime;
    public TransportMode TransportMode { get; set; } = TransportMode.Walking;
    public string? AdditionalContext { get; set; }
}

public class SafetyAdvice
{
    public string AdviceId { get; set; } = Guid.NewGuid().ToString();
    public List<string> Tips { get; set; } = new();
    public List<string> ThingsToAvoid { get; set; } = new();
    public List<string> EmergencyContacts { get; set; } = new();
    public DateTime ProvidedAt { get; set; } = DateTime.UtcNow;
}

public enum EmergencyType
{
    HealthEmergency,
    Crime,
    Accident,
    LostChild,
    EnvironmentalDanger,
    Other
}