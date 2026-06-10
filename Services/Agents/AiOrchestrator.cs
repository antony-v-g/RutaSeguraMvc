namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Orchestrates multiple AI agents to provide comprehensive safety analysis
/// </summary>
public class AiOrchestrator : IAiOrchestrator
{
    private readonly IRouteAnalysisAgent _routeAgent;
    private readonly ISafetyAssistantAgent _safetyAgent;
    private readonly ILogger<AiOrchestrator> _logger;

    public AiOrchestrator(
        IRouteAnalysisAgent routeAgent,
        ISafetyAssistantAgent safetyAgent,
        ILogger<AiOrchestrator> logger)
    {
        _routeAgent = routeAgent;
        _safetyAgent = safetyAgent;
        _logger = logger;
    }

    public async Task<CompleteSafetyAnalysis> ProcessSafetyRequestAsync(SafetyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing complete safety request from {Origin} to {Destination}", 
                request.Origin, request.Destination);

            var analysis = new CompleteSafetyAnalysis();

            // Step 1: Analyze the route
            _logger.LogInformation("Step 1: Analyzing route");
            var routeRequest = new RouteAnalysisRequest
            {
                Origin = request.Origin,
                Destination = request.Destination,
                TimeOfDay = request.TimeOfDay,
                TransportMode = request.TransportMode
            };
            analysis.RouteAnalysis = await _routeAgent.AnalyzeRouteAsync(routeRequest, cancellationToken);

            // Step 2: Get personalized safety advice
            _logger.LogInformation("Step 2: Generating safety advice");
            var safetyScenario = new SafetyScenario
            {
                Location = request.Destination,
                TimeOfDay = request.TimeOfDay,
                TransportMode = request.TransportMode,
                AdditionalContext = $"Coming from {request.Origin}"
            };
            analysis.PersonalSafetyAdvice = await _safetyAgent.GetSafetyAdviceAsync(safetyScenario, cancellationToken);

            // Step 3: Get alternative routes (parallel with step 2 could improve performance)
            _logger.LogInformation("Step 3: Generating alternative routes");
            analysis.AlternativeRoutes = await _routeAgent.GetAlternativeRoutesAsync(
                request.Origin, request.Destination, cancellationToken);

            _logger.LogInformation("Complete safety analysis finished. RequestId: {RequestId}", analysis.RequestId);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing safety request");
            throw;
        }
    }
}