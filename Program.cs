using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using RutaSeguraMvc.Data;
using RutaSeguraMvc.Services.AI;
using RutaSeguraMvc.Services.Agents;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ============== AI Services Configuration ==============
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AiSettings"));

// Register Semantic Kernel
var aiSettings = builder.Configuration.GetSection("AiSettings").Get<AiSettings>()
    ?? throw new InvalidOperationException("AiSettings configuration is missing");

var kernelBuilder = Kernel.CreateBuilder();

if (aiSettings.ApiType == "OpenAI")
{
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: aiSettings.ModelName,
        apiKey: aiSettings.ApiKey);
}
else if (aiSettings.ApiType == "AzureOpenAI")
{
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: aiSettings.DeploymentName,
        endpoint: aiSettings.Endpoint,
        apiKey: aiSettings.ApiKey);
}

var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// Register AI Services
builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();
builder.Services.AddScoped<IRouteAnalysisAgent, RouteAnalysisAgent>();
builder.Services.AddScoped<ISafetyAssistantAgent, SafetyAssistantAgent>();
builder.Services.AddScoped<IAiOrchestrator, AiOrchestrator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
