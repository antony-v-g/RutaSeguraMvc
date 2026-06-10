namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Agent for analyzing routes and providing safety recommendations
/// </summary>
public interface IRouteAnalysisAgent
{
    /// <summary>
    /// Analyze a route for safety concerns
    /// </summary>
    Task<RouteAnalysisResult> AnalyzeRouteAsync(RouteAnalysisRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get alternative safer routes
    /// </summary>
    Task<List<AlternativeRoute>> GetAlternativeRoutesAsync(string origin, string destination, CancellationToken cancellationToken = default);
}

public class RouteAnalysisRequest
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? RoutePath { get; set; }
    public TimeOfDay TimeOfDay { get; set; } = TimeOfDay.Daytime;
    public TransportMode TransportMode { get; set; } = TransportMode.Walking;
}

public class RouteAnalysisResult
{
    public string RouteId { get; set; } = Guid.NewGuid().ToString();
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int SafetyScore { get; set; } // 0-100
    public SafetyLevel SafetyLevel { get; set; }
    public List<string> SafetyConcerns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; } = DateTime.UtcNow;
}

public class AlternativeRoute
{
    public string RouteDescription { get; set; } = string.Empty;
    public int SafetyScore { get; set; }
    public string? Benefits { get; set; }
}

public enum SafetyLevel
{
    VeryHigh = 0,
    High = 1,
    Medium = 2,
    Low = 3,
    VeryLow = 4
}

public enum TimeOfDay
{
    Night,
    EarlyMorning,
    Morning,
    Afternoon,
    Evening,
    Daytime
}

public enum TransportMode
{
    Walking,
    Driving,
    PublicTransport,
    Cycling
}