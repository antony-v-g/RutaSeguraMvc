using System.Text.Json;
using RutaSeguraMvc.Services.AI;

namespace RutaSeguraMvc.Services.Agents;

/// <summary>
/// Implementation of safety assistant agent for personalized advice
/// </summary>
public class SafetyAssistantAgent : ISafetyAssistantAgent
{
    private readonly ISemanticKernelService _skService;
    private readonly ILogger<SafetyAssistantAgent> _logger;

    public SafetyAssistantAgent(ISemanticKernelService skService, ILogger<SafetyAssistantAgent> logger)
    {
        _skService = skService;
        _logger = logger;
    }

    public async Task<SafetyAdvice> GetSafetyAdviceAsync(SafetyScenario scenario, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting safety advice for {Location} at {TimeOfDay} using {TransportMode}",
                scenario.Location, scenario.TimeOfDay, scenario.TransportMode);

            var prompt = BuildSafetyAdvicePrompt(scenario);
            var response = await _skService.ExecutePromptAsync(prompt, cancellationToken);

            var advice = ParseSafetyAdvice(response);
            _logger.LogInformation("Safety advice generated with {TipsCount} tips", advice.Tips.Count);

            return advice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting safety advice");
            throw;
        }
    }

    public async Task<string> GetEmergencyAdviceAsync(EmergencyType emergencyType, string context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting emergency advice for {EmergencyType}", emergencyType);

            var prompt = $"""
                You are an expert emergency response advisor. Provide immediate guidance for this situation:
                
                Emergency Type: {emergencyType}
                Context: {context}
                
                Provide clear, actionable steps to follow immediately. Be concise and specific.
                Focus on safety and getting help.
                """;

            var response = await _skService.ExecutePromptAsync(prompt, cancellationToken);
            _logger.LogInformation("Emergency advice generated");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emergency advice");
            throw;
        }
    }

    private string BuildSafetyAdvicePrompt(SafetyScenario scenario)
    {
        return $"""
            You are a safety expert providing personalized advice. Based on this scenario, provide actionable safety tips:
            
            Location: {scenario.Location}
            Time of Day: {scenario.TimeOfDay}
            Transport Mode: {scenario.TransportMode}
            {(string.IsNullOrEmpty(scenario.AdditionalContext) ? "" : $"Additional Context: {scenario.AdditionalContext}")}
            
            Respond ONLY with valid JSON (no markdown, no extra text):
            {{
                "tips": [
                    "tip 1",
                    "tip 2",
                    "tip 3"
                ],
                "thingsToAvoid": [
                    "thing to avoid 1",
                    "thing to avoid 2"
                ],
                "emergencyContacts": [
                    "Police: 911",
                    "Ambulance: 911"
                ]
            }}
            """;
    }

    private SafetyAdvice ParseSafetyAdvice(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                _logger.LogWarning("No JSON found in response");
                return new SafetyAdvice();
            }

            var jsonContent = response[jsonStart..(jsonEnd + 1)];
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var advice = new SafetyAdvice();

            if (root.TryGetProperty("tips", out var tipsEl) && tipsEl.ValueKind == JsonValueKind.Array)
            {
                advice.Tips = tipsEl.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            if (root.TryGetProperty("thingsToAvoid", out var avoidEl) && avoidEl.ValueKind == JsonValueKind.Array)
            {
                advice.ThingsToAvoid = avoidEl.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            if (root.TryGetProperty("emergencyContacts", out var contactsEl) && contactsEl.ValueKind == JsonValueKind.Array)
            {
                advice.EmergencyContacts = contactsEl.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }

            return advice;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing safety advice");
            return new SafetyAdvice();
        }
    }
}