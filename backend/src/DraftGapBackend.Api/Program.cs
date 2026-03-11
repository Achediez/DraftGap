using DraftGapBackend.API.Middleware;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using DraftGapBackend.Infrastructure.Persistence;
using DraftGapBackend.Infrastructure.Riot;
using DraftGapBackend.Infrastructure.Sync;
using DraftGapBackend.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Net;
using Polly;
using StackExchange.Redis;

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
// FLUENTVALIDATION
// ====================================
// Registra todos los validadores desde el assembly de Application
// Los validadores se ejecutan automáticamente en los controladores
builder.Services.AddValidatorsFromAssemblyContaining<DraftGapBackend.Application.Validators.PaginationRequestValidator>();

// ====================================
// DEPENDENCY INJECTION
// ====================================
// ===== APPLICATION LAYER SERVICES =====
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IChampionService, ChampionService>();
builder.Services.AddScoped<IRankedService, RankedService>();
builder.Services.AddScoped<IFriendsService, FriendsService>();
builder.Services.AddScoped<IUserSyncService, UserSyncService>();

// ===== INFRASTRUCTURE LAYER REPOSITORIES =====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IChampionRepository, ChampionRepository>();
builder.Services.AddScoped<IRankedRepository, RankedRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

// ===== RIOT API SERVICES =====
// Configure Redis connection multiplexer for rate-limit pre-checks
builder.Services.AddSingleton(sp =>
{
    var cfg = builder.Configuration["Redis:ConnectionString"] ?? builder.Configuration.GetConnectionString("Redis") ?? "localhost";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(cfg);
});

// Register Redis-based pre-check handler
builder.Services.AddTransient<DraftGapBackend.Infrastructure.Riot.RedisRateLimitHandler>();

// Configure HttpClient for RiotService with resilience policies (Polly)
builder.Services.AddHttpClient<IRiotService, RiotService>()
    .AddHttpMessageHandler<DraftGapBackend.Infrastructure.Riot.RedisRateLimitHandler>()
    .AddPolicyHandler((sp, request) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<RiotService>>();

        // Read options from configuration with sensible defaults
        var retryCount = int.TryParse(configuration["Polly:RetryCount"], out var rc) ? rc : 3;
        var timeoutSeconds = int.TryParse(configuration["Polly:TimeoutSeconds"], out var ts) ? ts : 10;
        var failureThreshold = double.TryParse(configuration["Polly:FailureThreshold"], out var ft) ? ft : 0.3;
        var samplingDuration = int.TryParse(configuration["Polly:SamplingDurationSeconds"], out var sd) ? sd : 30;
        var minimumThroughput = int.TryParse(configuration["Polly:MinimumThroughput"], out var mt) ? mt : 8;
        var breakDuration = int.TryParse(configuration["Polly:DurationOfBreakSeconds"], out var bd) ? bd : 30;

        var jitterer = new Random();

        // Retry policy: handle 429 and 5xx, read Retry-After when present, add jitter
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429 || (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount,
                attempt =>
                {
                    // Exponential backoff + jitter
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    var jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, 1000));
                    return delay + jitter;
                },
                async (outcome, timespan, retryAttempt, context) =>
                {
                    // If server provided Retry-After, wait additionally to honor it
                    try
                    {
                        var resp = outcome.Result;
                        if (resp != null && resp.Headers.RetryAfter != null)
                        {
                            TimeSpan additional = TimeSpan.Zero;
                            if (resp.Headers.RetryAfter.Delta.HasValue)
                                additional = resp.Headers.RetryAfter.Delta.Value;
                            else if (resp.Headers.RetryAfter.Date.HasValue)
                            {
                                var computed = resp.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                                if (computed > TimeSpan.Zero)
                                    additional = computed;
                            }

                            if (additional > TimeSpan.Zero)
                            {
                                logger.LogWarning("Riot API requested Retry-After {RetryAfter}s. Waiting additional {Seconds}s.", additional.TotalSeconds, additional.TotalSeconds);
                                await Task.Delay(additional).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Error while honoring Retry-After header");
                    }

                    logger.LogWarning("Riot API retry attempt {Attempt} due to {StatusCode}. Waiting {Delay}s.", retryAttempt, outcome.Result?.StatusCode, timespan.TotalSeconds);
                });

        // Circuit breaker: advanced threshold over a sampling duration
        var breaker = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429 || (int)r.StatusCode >= 500)
            .AdvancedCircuitBreakerAsync(
                failureThreshold,
                TimeSpan.FromSeconds(samplingDuration),
                minimumThroughput,
                TimeSpan.FromSeconds(breakDuration),
                onBreak: (outcome, timespan) => logger.LogWarning("Circuit breaker opened for {Seconds}s due to {StatusCode}", timespan.TotalSeconds, outcome.Result?.StatusCode),
                onReset: () => logger.LogInformation("Circuit breaker reset"),
                onHalfOpen: () => logger.LogInformation("Circuit breaker half-open"));

        var timeoutPolicy = Polly.Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds));

        // Combine policies: timeout outside to bound total duration
        var wrap = Polly.Policy.WrapAsync(timeoutPolicy, breaker, retryPolicy);
        return wrap;
    });

builder.Services.AddScoped<IRiotService, RiotService>();

// Data Dragon static data service
builder.Services.AddHttpClient<IDataDragonService, DataDragonService>();

// ===== SYNC SERVICES =====
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

// Global exception handler
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
app.ConfigureExceptionHandler(startupLogger);

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
