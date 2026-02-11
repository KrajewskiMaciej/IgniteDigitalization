using backend.Data;
using backend.Initializers;
using backend.Services;
using DigitalWars.Server.Services;
using DigitalWars.Server.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using backend.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- 1. KONFIGURACJA DLA AZURE (Forwarded Headers) ---
// Musi być skonfigurowane przed buildem, aby aplikacja ufała nagłówkom HTTPS z proxy Azure
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var loggerFactory = LoggerFactory.Create(config => { config.AddConsole(); });
var startupLogger = loggerFactory.CreateLogger("Startup");
startupLogger.LogInformation("[API] Rozpoczynam konfigurację aplikacji DigitalWars...");

// Konfiguracja bazy danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )));

// Konfiguracja CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // Kluczowe dla Azure, gdy adresy frontendu/backendu się różnią
    });
});

// Rejestracja serwisów
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Rejestracja wszystkich serwisów (Email, Jwt, Game itd.) - bez zmian
builder.Services.Configure<backend.Services.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));
builder.Services.AddScoped<backend.Services.IEmailService, backend.Services.EmailService>();
builder.Services.AddScoped<backend.Services.JwtService>();
builder.Services.AddScoped<backend.Services.IProvisioningService, backend.Services.ProvisioningService>();
builder.Services.AddScoped<backend.Services.IAuthService, backend.Services.AuthService>();
builder.Services.AddScoped<backend.Services.IPlayerService, backend.Services.PlayerService>();
builder.Services.AddScoped<backend.Services.IGameService, backend.Services.GameService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IPlayerQueryService, PlayerQueryService>();
builder.Services.AddScoped<backend.Services.IPlayerActionService, backend.Services.PlayerActionService>();
builder.Services.AddScoped<ICheatsheetService, CheatsheetService>();
builder.Services.AddSingleton<backend.Services.IBackgroundTaskQueue>(ctx => new backend.Services.BackgroundTaskQueue(100));
builder.Services.AddHostedService<QueuedHostedService>();

// Serwisy DigitalWars.Server
builder.Services.AddScoped<DigitalWars.Server.Services.IPlayerService, DigitalWars.Server.Services.PlayerService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IAuthService, DigitalWars.Server.Services.AuthService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IPlayerActionService, DigitalWars.Server.Services.PlayerActionService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IGameService, DigitalWars.Server.Services.GameService>();
builder.Services.AddScoped<DigitalWars.Server.Services.JwtService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IEmailService, DigitalWars.Server.Services.EmailService>();
builder.Services.AddSingleton<DigitalWars.Server.Services.IBackgroundTaskQueue>(ctx => new DigitalWars.Server.Services.BackgroundTaskQueue(100));
builder.Services.AddScoped<DigitalWars.Server.Services.IProvisioningService, DigitalWars.Server.Services.ProvisioningService>();

// --- 2. KONFIGURACJA AUTH & COOKIES ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "itm.auth.cookie";
        options.Cookie.HttpOnly = true;
        // Na Azure SameAsRequest jest bezpieczniejszy przy proxy
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity != null &&
            context.User.Identity.IsAuthenticated &&
            !context.User.HasClaim(c => c.Type == "Teams_Id")
        ));
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<DigitalWars.Server.Settings.ConfigureSwaggerOptions>();

var app = builder.Build();

// --- 3. KOLEJNOŚĆ MIDDLEWARE (Krytyczna dla Azure) ---

// 1. Zawsze pierwsze - obsługa nagłówków z proxy Azure
app.UseForwardedHeaders();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("[API] Środowisko aplikacji: {Environment}", app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
    app.UseCors("CorsPolicy");
}
else
{
    // Produkcja na Azure
    app.UseCors("CorsPolicy");

    // UWAGA: Jeśli w portalu Azure masz "HTTPS Only: On", poniższa linia może być zbędna 
    // i czasami powoduje błąd 400. Jeśli problem wróci, zakomentuj ją.
    app.UseHttpsRedirection();

    var serveFrontend = builder.Configuration.GetValue<bool>("FrontendSettings:ServeStaticFiles");
    if (serveFrontend)
    {
        app.UseDefaultFiles();
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".glb"] = "model/gltf-binary";
        provider.Mappings[".gltf"] = "model/gltf+json";
        app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/api/gameHub");

if (builder.Configuration.GetValue<bool>("FrontendSettings:ServeStaticFiles"))
{
    app.MapFallbackToFile("/index.html").AllowAnonymous();
}

// Migracje i Inicjalizacja
using (var scope = app.Services.CreateScope())
{
    try
    {
        logger.LogInformation("[API] Rozpoczynam migrację bazy danych...");
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<backend.Services.IProvisioningService>();
        context.Database.Migrate();
        DbInitializer.Initialize(context, provisioningService);
        logger.LogInformation("[API] Inicjalizacja bazy zakończona pomyślnie.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[API] Błąd podczas migracji lub inicjalizacji.");
    }
}

logger.LogInformation("[API] Aplikacja DigitalWars została uruchomiona");
app.Run();