using Microsoft.AspNetCore.Mvc;
using RutaSeguraMvc.Services.Agents;

namespace RutaSeguraMvc.Controllers;

/// <summary>
/// Controller for AI-powered safety analysis endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiOrchestrator _orchestrator;
    private readonly IRouteAnalysisAgent _routeAgent;
    private readonly ISafetyAssistantAgent _safetyAgent;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IAiOrchestrator orchestrator,
        IRouteAnalysisAgent routeAgent,
        ISafetyAssistantAgent safetyAgent,
        ILogger<AiController> logger)
    {
        _orchestrator = orchestrator;
        _routeAgent = routeAgent;
        _safetyAgent = safetyAgent;
        _logger = logger;
    }

    /// <summary>
    /// Perform complete safety analysis for a route
    /// </summary>
    [HttpPost("analyze-route")]
    [ProducesResponseType(typeof(CompleteSafetyAnalysis), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeSafetyRequest([FromBody] SafetyRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request model");
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
        {
            _logger.LogWarning("Origin or Destination is missing");
            return BadRequest("Origin and Destination are required");
        }

        try
        {
            _logger.LogInformation("Processing safety request");
            var analysis = await _orchestrator.ProcessSafetyRequestAsync(request, cancellationToken);
            return Ok(analysis);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request was cancelled");
            return StatusCode(StatusCodes.Status408RequestTimeout, "Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing safety request");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing your request", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze a specific route
    /// </summary>
    [HttpPost("route-analysis")]
    [ProducesResponseType(typeof(RouteAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeRoute([FromBody] RouteAnalysisRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
        {
            return BadRequest("Origin and Destination are required");
        }

        try
        {
            _logger.LogInformation("Analyzing route");
            var result = await _routeAgent.AnalyzeRouteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing route");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get alternative safer routes
    /// </summary>
    [HttpGet("alternative-routes")]
    [ProducesResponseType(typeof(List<AlternativeRoute>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAlternativeRoutes(
        [FromQuery] string origin,
        [FromQuery] string destination,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return BadRequest("Origin and destination parameters are required");
        }

        try
        {
            _logger.LogInformation("Getting alternative routes");
            var routes = await _routeAgent.GetAlternativeRoutesAsync(origin, destination, cancellationToken);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alternative routes");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get safety advice for a scenario
    /// </summary>
    [HttpPost("safety-advice")]
    [ProducesResponseType(typeof(SafetyAdvice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSafetyAdvice([FromBody] SafetyScenario scenario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(scenario.Location))
        {
            return BadRequest("Location is required");
        }

        try
        {
            _logger.LogInformation("Getting safety advice");
            var advice = await _safetyAgent.GetSafetyAdviceAsync(scenario, cancellationToken);
            return Ok(advice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting safety advice");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get emergency guidance
    /// </summary>
    [HttpPost("emergency-advice")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmergencyAdvice(
        [FromBody] EmergencyAdviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Context))
        {
            return BadRequest("Context is required");
        }

        try
        {
            _logger.LogInformation("Getting emergency advice");
            var advice = await _safetyAgent.GetEmergencyAdviceAsync(request.EmergencyType, request.Context, cancellationToken);
            return Ok(new { advice });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emergency advice");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for emergency advice
/// </summary>
public class EmergencyAdviceRequest
{
    public EmergencyType EmergencyType { get; set; }
    public string Context { get; set; } = string.Empty;
}