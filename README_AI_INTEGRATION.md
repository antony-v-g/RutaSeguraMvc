# AI Integration Guide - Ruta Segura MVC

## Overview

This project has been enhanced with **Semantic Kernel**, **LLM** (Large Language Models), and **AI Agents** to provide intelligent route safety analysis and personalized safety advice.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│            API Controllers (REST Endpoints)              │
│                    AiController                          │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│              AI Orchestrator                             │
│      (Coordinates multiple agents)                       │
└────────────────────────┬────────────────────────────────┘
                         │
         ┌────────────────┼────────────────┐
         │                │                │
    ┌────▼──────┐   ┌────▼──────┐   ┌────▼──────┐
    │   Route    │   │  Safety   │   │Semantic   │
    │ Analysis   │   │ Assistant │   │  Kernel   │
    │  Agent     │   │   Agent   │   │  Service  │
    └───────────┘   └───────────┘   └────┬──────┘
                                           │
                                     ┌─────▼──────┐
                                     │  LLM/AI    │
                                     │  (OpenAI)  │
                                     └────────────┘
```

## Components

### 1. **Semantic Kernel Service** (`Services/AI/SemanticKernelService.cs`)
- Wrapper around Microsoft Semantic Kernel
- Handles LLM interactions
- Supports prompt execution and multi-turn conversations
- Features:
  - Simple prompt execution
  - Structured output parsing (JSON)
  - Multi-turn conversations
  - Function/plugin invocation

### 2. **Route Analysis Agent** (`Services/Agents/RouteAnalysisAgent.cs`)
- Analyzes routes for safety concerns
- Provides safety scores (0-100)
- Identifies specific hazards
- Suggests alternative safer routes
- Key methods:
  - `AnalyzeRouteAsync()` - Comprehensive route safety analysis
  - `GetAlternativeRoutesAsync()` - Suggests safer alternatives

### 3. **Safety Assistant Agent** (`Services/Agents/SafetyAssistantAgent.cs`)
- Provides personalized safety tips
- Location and time-aware advice
- Emergency response guidance
- Key methods:
  - `GetSafetyAdviceAsync()` - Context-specific safety tips
  - `GetEmergencyAdviceAsync()` - Emergency guidance

### 4. **AI Orchestrator** (`Services/Agents/AiOrchestrator.cs`)
- Coordinates multiple agents for complex requests
- Executes comprehensive safety analysis workflow
- Combines route analysis, advice, and alternatives

## Configuration

### Setup Steps

1. **Install Dependencies**
   ```bash
   dotnet restore
   ```

2. **Configure API Keys**
   
   Set your OpenAI API key in `appsettings.json`:
   ```json
   {
     "AiSettings": {
       "ApiType": "OpenAI",
       "ApiKey": "sk-...",
       "ModelName": "gpt-4o-mini",
       "MaxTokens": 2000,
       "Temperature": 0.7
     }
   }
   ```

   Or use User Secrets:
   ```bash
   dotnet user-secrets set "AiSettings:ApiKey" "sk-..."
   ```

3. **Alternative: Azure OpenAI**
   ```json
   {
     "AiSettings": {
       "ApiType": "AzureOpenAI",
       "Endpoint": "https://<your-resource>.openai.azure.com/",
       "DeploymentName": "gpt-4-deployment",
       "ApiKey": "..."
     }
   }
   ```

## API Endpoints

### 1. Complete Safety Analysis
```
POST /api/ai/analyze-route
Content-Type: application/json

{
  "origin": "123 Main Street, City",
  "destination": "456 Park Avenue, City",
  "timeOfDay": "Evening",
  "transportMode": "Walking"
}
```

**Response:**
```json
{
  "requestId": "guid",
  "routeAnalysis": {
    "origin": "...",
    "destination": "...",
    "safetyScore": 75,
    "safetyLevel": "High",
    "safetyConcerns": ["..."],
    "recommendations": ["..."]
  },
  "personalSafetyAdvice": {
    "tips": ["..."],
    "thingsToAvoid": ["..."],
    "emergencyContacts": ["..."]
  },
  "alternativeRoutes": [
    {
      "routeDescription": "...",
      "safetyScore": 85,
      "benefits": "..."
    }
  ],
  "processedAt": "2024-01-10T10:30:00Z"
}
```

### 2. Route Analysis Only
```
POST /api/ai/route-analysis
Content-Type: application/json

{
  "origin": "...",
  "destination": "...",
  "timeOfDay": "Night",
  "transportMode": "Walking"
}
```

### 3. Alternative Routes
```
GET /api/ai/alternative-routes?origin=123%20Main%20St&destination=456%20Park%20Ave
```

### 4. Safety Advice
```
POST /api/ai/safety-advice
Content-Type: application/json

{
  "location": "Downtown District",
  "timeOfDay": "Evening",
  "transportMode": "Walking",
  "additionalContext": "First time visiting"
}
```

### 5. Emergency Guidance
```
POST /api/ai/emergency-advice
Content-Type: application/json

{
  "emergencyType": "Crime",
  "context": "Someone following me in the parking lot"
}
```

## Data Models

### SafetyLevel Enum
```csharp
public enum SafetyLevel
{
    VeryHigh = 0,  // 85-100
    High = 1,      // 70-84
    Medium = 2,    // 50-69
    Low = 3,       // 30-49
    VeryLow = 4    // 0-29
}
```

### TimeOfDay Enum
```csharp
public enum TimeOfDay
{
    Night,
    EarlyMorning,
    Morning,
    Afternoon,
    Evening,
    Daytime
}
```

### TransportMode Enum
```csharp
public enum TransportMode
{
    Walking,
    Driving,
    PublicTransport,
    Cycling
}
```

### EmergencyType Enum
```csharp
public enum EmergencyType
{
    HealthEmergency,
    Crime,
    Accident,
    LostChild,
    EnvironmentalDanger,
    Other
}
```

## Usage Examples

### C# Example
```csharp
[HttpGet("dashboard")]
public async Task<IActionResult> Dashboard(
    [FromServices] IAiOrchestrator orchestrator,
    CancellationToken cancellationToken)
{
    var request = new SafetyRequest
    {
        Origin = "Downtown Station",
        Destination = "Residential Area",
        TimeOfDay = TimeOfDay.Evening,
        TransportMode = TransportMode.Walking,
        UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
    };

    var analysis = await orchestrator.ProcessSafetyRequestAsync(request, cancellationToken);
    return View(analysis);
}
```

### JavaScript Example
```javascript
async function analyzeSafety(origin, destination) {
    const response = await fetch('/api/ai/analyze-route', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            origin: origin,
            destination: destination,
            timeOfDay: 'Evening',
            transportMode: 'Walking'
        })
    });
    
    const data = await response.json();
    console.log('Safety Score:', data.routeAnalysis.safetyScore);
    console.log('Recommendations:', data.routeAnalysis.recommendations);
}
```

## Error Handling

The service includes comprehensive error handling:

- **Invalid Configuration**: Throws on startup if API keys are missing
- **API Failures**: Returns 500 with error details
- **Timeout**: Returns 408 Request Timeout after cancellation
- **JSON Parsing**: Falls back to default results with warnings

## Performance Considerations

1. **Caching**: Consider implementing IDistributedCache for analysis results
2. **Concurrency**: Agents can run in parallel for independent tasks
3. **Token Limits**: Monitor LLM token usage with logging
4. **Timeout**: Default HTTP timeout is 100 seconds

## Testing

### Unit Test Example
```csharp
[Test]
public async Task AnalyzeRoute_WithValidInput_ReturnsAnalysis()
{
    var agent = new RouteAnalysisAgent(_mockSkService, _mockLogger);
    var request = new RouteAnalysisRequest
    {
        Origin = "A",
        Destination = "B",
        TimeOfDay = TimeOfDay.Evening
    };

    var result = await agent.AnalyzeRouteAsync(request);

    Assert.That(result.SafetyScore, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(100));
}
```

## Future Enhancements

1. **Multi-Language Support**: Translate prompts and responses
2. **Vector Database**: Store route analysis for similarity search
3. **Real-time Data**: Integrate with crime/incident APIs
4. **User Preferences**: Personalize based on user profile
5. **Caching Layer**: Redis for frequently analyzed routes
6. **Chat Interface**: Conversational AI for users
7. **Mobile Integration**: Companion mobile app with real-time alerts
8. **Advanced Analytics**: Dashboard for safety trends

## Troubleshooting

### Issue: "Connection string 'DefaultConnection' not found"
**Solution**: Ensure `appsettings.json` contains the connection string

### Issue: "AiSettings configuration is missing"
**Solution**: Configure `AiSettings` in `appsettings.json`

### Issue: "401 Unauthorized" from LLM API
**Solution**: Verify API key is correct and not expired

### Issue: "Timeout" errors
**Solution**: Increase HttpClient timeout or optimize prompts

## Security Notes

- ⚠️ Never commit API keys to version control
- Use Azure Key Vault or AWS Secrets Manager in production
- Implement rate limiting on API endpoints
- Validate and sanitize user input
- Log security events

## Support

For issues or questions:
1. Check logs in `bin/Debug` or application event viewer
2. Review error responses from API
3. Verify configuration in `appsettings.json`
4. Check OpenAI/Azure OpenAI dashboard for quota/usage

## License

This AI integration follows the same license as the main project.