using System.Text.Json;
using RutaSeguraMvc.Services.AI;

namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Agent implementation for route analysis and safety assessment
/// </summary>
public class RouteAnalysisAgent : IRouteAnalysisAgent
{
    private readonly ISemanticKernelService _skService;
    private readonly ILogger<RouteAnalysisAgent> _logger;

    public RouteAnalysisAgent(ISemanticKernelService skService, ILogger<RouteAnalysisAgent> logger)
    {
        _skService = skService;
        _logger = logger;
    }

    public async Task<RouteAnalysisResult> AnalyzeRouteAsync(RouteAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing route from {Origin} to {Destination} at {TimeOfDay}", 
                request.Origin, request.Destination, request.TimeOfDay);

            var prompt = BuildAnalysisPrompt(request);
            var response = await _skService.ExecutePromptAsync(prompt, cancellationToken);

            var result = ParseAnalysisResponse(response, request);
            _logger.LogInformation("Route analysis completed. Safety Score: {Score}", result.SafetyScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing route");
            throw;
        }
    }

    public async Task<List<AlternativeRoute>> GetAlternativeRoutesAsync(string origin, string destination, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting alternative routes from {Origin} to {Destination}", origin, destination);

            var prompt = $"""
                You are a safety expert. Suggest 3 safer alternative routes from "{origin}" to "{destination}".
                
                For each route, provide:
                1. Route description (landmark-based or street-based)
                2. Safety score (0-100, where 100 is safest)
                3. Key benefits for safety
                
                Respond in JSON format:
                {{
                    "routes": [
                        {{
                            "description": "...",
                            "safetyScore": 85,
                            "benefits": "..."
                        }}
                    ]
                }}
                """;

            var response = await _skService.ExecutePromptAsync(prompt, cancellationToken);
            var routes = ParseAlternativeRoutes(response);

            _logger.LogInformation("Found {Count} alternative routes", routes.Count);
            return routes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alternative routes");
            throw;
        }
    }

    private string BuildAnalysisPrompt(RouteAnalysisRequest request)
    {
        return $"""
            You are an expert in urban safety and route planning. Analyze the following route request:
            
            Origin: {request.Origin}
            Destination: {request.Destination}
            Time of Day: {request.TimeOfDay}
            Transport Mode: {request.TransportMode}
            {(string.IsNullOrEmpty(request.RoutePath) ? "" : $"Route Path: {request.RoutePath}")}
            
            Provide a comprehensive safety analysis including:
            1. Overall safety score (0-100)
            2. Safety level classification
            3. Specific safety concerns
            4. Practical recommendations
            
            Respond ONLY with valid JSON (no markdown, no extra text):
            {{
                "safetyScore": 75,
                "safetyLevel": "High",
                "concerns": ["concern 1", "concern 2"],
                "recommendations": ["recommendation 1", "recommendation 2"]
            }}
            """;
    }

    private RouteAnalysisResult ParseAnalysisResponse(string response, RouteAnalysisRequest request)
    {
        try
        {
            // Extract JSON from response
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                _logger.LogWarning("No JSON found in response, using default values");
                return CreateDefaultResult(request);
            }

            var jsonContent = response[jsonStart..(jsonEnd + 1)];
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var result = new RouteAnalysisResult
            {
                Origin = request.Origin,
                Destination = request.Destination,
                SafetyScore = root.GetProperty("safetyScore").GetInt32(),
                SafetyLevel = ParseSafetyLevel(root.GetProperty("safetyLevel").GetString()),
            };

            if (root.TryGetProperty("concerns", out var concernsEl) && concernsEl.ValueKind == JsonValueKind.Array)
            {
                result.SafetyConcerns = concernsEl.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            if (root.TryGetProperty("recommendations", out var recsEl) && recsEl.ValueKind == JsonValueKind.Array)
            {
                result.Recommendations = recsEl.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing response, using default result");
            return CreateDefaultResult(request);
        }
    }

    private List<AlternativeRoute> ParseAlternativeRoutes(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return new();

            var jsonContent = response[jsonStart..(jsonEnd + 1)];
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("routes", out var routesEl) || routesEl.ValueKind != JsonValueKind.Array)
                return new();

            var routes = new List<AlternativeRoute>();
            foreach (var routeEl in routesEl.EnumerateArray())
            {
                routes.Add(new AlternativeRoute
                {
                    RouteDescription = routeEl.GetProperty("description").GetString() ?? string.Empty,
                    SafetyScore = routeEl.GetProperty("safetyScore").GetInt32(),
                    Benefits = routeEl.TryGetProperty("benefits", out var benefitsEl) 
                        ? benefitsEl.GetString() 
                        : null
                });
            }

            return routes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing alternative routes");
            return new();
        }
    }

    private SafetyLevel ParseSafetyLevel(string? level)
    {
        return level?.ToLower() switch
        {
            "veryhigh" or "very high" => SafetyLevel.VeryHigh,
            "high" => SafetyLevel.High,
            "medium" => SafetyLevel.Medium,
            "low" => SafetyLevel.Low,
            "verylow" or "very low" => SafetyLevel.VeryLow,
            _ => SafetyLevel.Medium
        };
    }

    private RouteAnalysisResult CreateDefaultResult(RouteAnalysisRequest request)
    {
        return new RouteAnalysisResult
        {
            Origin = request.Origin,
            Destination = request.Destination,
            SafetyScore = 70,
            SafetyLevel = SafetyLevel.High,
            SafetyConcerns = new() { "Unable to complete full analysis" },
            Recommendations = new() { "Consider checking local safety reports for this area" }
        };
    }
}