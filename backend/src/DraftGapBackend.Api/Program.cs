using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using DraftGapBackend.Infrastructure.Persistence;
using DraftGapBackend.Infrastructure.Riot;
using DraftGapBackend.Infrastructure.Sync;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================================
// SERVICE REGISTRATION
// ====================================

// Core ASP.NET Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ====================================
// DATABASE CONFIGURATION
// ====================================
// MySQL connection with automatic retry logic for transient failures
// Retry strategy: 5 attempts with exponential backoff up to 10 seconds
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// ====================================
// SWAGGER/OPENAPI CONFIGURATION
// ====================================
// Configures API documentation with JWT authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DraftGap API",
        Version = "v1.0.0",
        Description = "League of Legends stats tracker API with Riot Games integration",
        Contact = new OpenApiContact
        {
            Name = "DraftGap Team",
            Email = "dev@draftgap.local"
        }
    });

    // JWT Bearer token configuration for Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Format: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ====================================
// JWT AUTHENTICATION CONFIGURATION
// ====================================
// Validates JWT tokens on protected endpoints
// Token contains: user ID, email, role claims
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero  // No tolerance for expired tokens
    };
});

builder.Services.AddAuthorization();

// ====================================
// DEPENDENCY INJECTION
// ====================================
// Application layer services
builder.Services.AddScoped<IUserService, UserService>();

// Infrastructure layer repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Riot API services with HttpClient factory pattern
// HttpClient lifetime is managed automatically
builder.Services.AddHttpClient<IRiotService, RiotService>();
builder.Services.AddScoped<IRiotService, RiotService>();

// Data Dragon static data service
builder.Services.AddHttpClient<IDataDragonService, DataDragonService>();

// Data sync service consumed by AdminController via IDataSyncService.
builder.Services.AddScoped<IDataSyncService, DataSyncService>();

// Background worker that polls sync_jobs and executes them via DataSyncService.
// Declared as hosted service so the runtime manages its lifetime alongside the app.
builder.Services.AddHostedService<RiotSyncBackgroundService>();

// ====================================
// CORS CONFIGURATION
// ====================================
// Allows cross-origin requests from any source (development only)
// Production should restrict to specific frontend origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ====================================
// BUILD APPLICATION
// ====================================
var app = builder.Build();

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                   DRAFTGAP API STARTING                        ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ====================================
// STARTUP VALIDATION & INITIALIZATION
// ====================================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var dataDragon = scope.ServiceProvider.GetRequiredService<IDataDragonService>();

    Console.WriteLine("🔧 [1/3] Validating configuration...");

    // Validate critical configuration values
    var missingConfigs = new List<string>();
    if (string.IsNullOrEmpty(connectionString)) missingConfigs.Add("DefaultConnection");
    if (string.IsNullOrEmpty(secretKey)) missingConfigs.Add("Jwt:SecretKey");
    if (string.IsNullOrEmpty(jwtSettings["Issuer"])) missingConfigs.Add("Jwt:Issuer");
    if (string.IsNullOrEmpty(jwtSettings["Audience"])) missingConfigs.Add("Jwt:Audience");

    if (missingConfigs.Any())
    {
        Console.WriteLine($"❌ Missing configuration: {string.Join(", ", missingConfigs)}");
        throw new InvalidOperationException($"Critical configuration missing: {string.Join(", ", missingConfigs)}");
    }

    Console.WriteLine("   ✅ Configuration valid");
    Console.WriteLine();

    // Test database connectivity
    Console.WriteLine("🗄️  [2/3] Testing database connection...");
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            Console.WriteLine("   ❌ Database connection failed");
            throw new InvalidOperationException("Cannot connect to MySQL database");
        }

        var dbName = context.Database.GetDbConnection().Database;
        Console.WriteLine($"   ✅ Connected to database: {dbName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Database error: {ex.Message}");
        logger.LogError(ex, "Failed to connect to database on startup");
        throw;
    }

    Console.WriteLine();

    // Sync static data from Data Dragon
    Console.WriteLine("📦 [3/3] Synchronizing static game data...");
    try
    {
        var startTime = DateTime.UtcNow;

        await dataDragon.SyncChampionsAsync();
        var championCount = await context.Champions.CountAsync();

        await dataDragon.SyncItemsAsync();
        var itemCount = await context.Items.CountAsync();

        await dataDragon.SyncSummonerSpellsAsync();
        var spellCount = await context.SummonerSpells.CountAsync();

        await dataDragon.SyncRunesAsync();
        var pathCount = await context.RunePaths.CountAsync();
        var runeCount = await context.Runes.CountAsync();

        var duration = (DateTime.UtcNow - startTime).TotalSeconds;

        Console.WriteLine($"   ✅ Champions:       {championCount} loaded");
        Console.WriteLine($"   ✅ Items:           {itemCount} loaded");
        Console.WriteLine($"   ✅ Summoner Spells: {spellCount} loaded");
        Console.WriteLine($"   ✅ Rune Paths:      {pathCount} loaded");
        Console.WriteLine($"   ✅ Runes:           {runeCount} loaded");
        Console.WriteLine($"   📊 Total sync time: {duration:F2}s");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Static data sync failed: {ex.Message}");
        logger.LogWarning(ex, "Data Dragon sync failed - application will continue with limited functionality");
    }


}

Console.WriteLine();
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    STARTUP COMPLETE                            ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ====================================
// HTTP REQUEST PIPELINE MIDDLEWARE
// ====================================
// Middleware order matters - authentication must come before authorization

if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development mode only
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DraftGap API v1");
        c.RoutePrefix = string.Empty;  // Swagger UI at root URL
        c.DocumentTitle = "DraftGap API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// ====================================
// HEALTH CHECK ENDPOINT
// ====================================
// Simple endpoint to verify API is running
// Can be used by monitoring tools, load balancers, or Docker health checks
app.MapGet("/health", async (ApplicationDbContext context) =>
{
    var dbHealthy = false;
    try
    {
        dbHealthy = await context.Database.CanConnectAsync();
    }
    catch { }

    return Results.Ok(new
    {
        status = dbHealthy ? "healthy" : "degraded",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        database = dbHealthy ? "connected" : "disconnected",
        services = new
        {
            authentication = "enabled",
            riot_api = "enabled",
            data_dragon = "enabled"
        }
    });
}).WithName("HealthCheck").WithTags("System");

// ====================================
// DISPLAY STARTUP INFORMATION
// ====================================
var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5057";

Console.WriteLine("📋 CONFIGURATION:");
Console.WriteLine($"   Environment:     {app.Environment.EnvironmentName}");
Console.WriteLine($"   Base URL:        {urls}");
Console.WriteLine($"   Swagger UI:      {urls}");
Console.WriteLine($"   Health Check:    {urls}/health");
Console.WriteLine();

Console.WriteLine("🔐 SECURITY:");
Console.WriteLine($"   JWT Issuer:      {jwtSettings["Issuer"]}");
Console.WriteLine($"   JWT Audience:    {jwtSettings["Audience"]}");
Console.WriteLine($"   Token Expiry:    1 day");
Console.WriteLine();

Console.WriteLine("🚀 API READY - Listening for requests...");
Console.WriteLine("   Press Ctrl+C to shutdown");
Console.WriteLine();
Console.WriteLine("════════════════════════════════════════════════════════════════");
Console.WriteLine();

// ====================================
// START APPLICATION
// ====================================
app.Run();
