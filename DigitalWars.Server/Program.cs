using backend.Data;
using backend.Initializers;
using backend.Services;
using DigitalWars.Server.Services;
using DigitalWars.Server.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using backend.Hubs;


QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var loggerFactory = LoggerFactory.Create(config =>
{
    config.AddConsole();
});
var startupLogger = loggerFactory.CreateLogger("Startup");
startupLogger.LogInformation("[API] Rozpoczynam konfigurację aplikacji DigitalWars...");


// Konfiguracja bazy danych z użyciem Connection String (działa lokalnie i w Azure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

startupLogger.LogInformation("[API] Konfiguruję połączenie z bazą danych: {ConnectionString}", connectionString);


// Konfiguracja CORS - potrzebna TYLKO do testowania lokalnego
builder.Services.AddCors();

// Rejestracja serwisów (bez zmian)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            // Opcjonalnie: ignoruj cykliczne referencje, co jest częstym problemem w EF
            // options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IProvisioningService, ProvisioningService>();
builder.Services.AddScoped<backend.Services.IAuthService, backend.Services.AuthService>();
builder.Services.AddScoped<backend.Services.IPlayerService, backend.Services.PlayerService>();
builder.Services.AddScoped<backend.Services.IGameService, backend.Services.GameService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IPlayerQueryService, PlayerQueryService>();
builder.Services.AddScoped<backend.Services.IPlayerActionService, backend.Services.PlayerActionService>();
builder.Services.AddScoped<ICheatsheetService, CheatsheetService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
{
    // Ustaw pojemność kolejki, np. 100
    return new BackgroundTaskQueue(100);
});
builder.Services.AddHostedService<QueuedHostedService>();

// SERWISY DO NOWYCH ENDPOINTÓW
builder.Services.AddScoped<DigitalWars.Server.Services.IPlayerService, DigitalWars.Server.Services.PlayerService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IAuthService, DigitalWars.Server.Services.AuthService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IPlayerActionService, DigitalWars.Server.Services.PlayerActionService>();
builder.Services.AddScoped<DigitalWars.Server.Services.IGameService, DigitalWars.Server.Services.GameService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "itm.auth.cookie";
        options.Cookie.HttpOnly = true;
        // Używaj bezpiecznych ciasteczek na produkcji
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

startupLogger.LogInformation("[API] Zarejestrowano serwisy aplikacji: EmailService, JwtService, GameService itd.");

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    // Dodaj opis dla każdej wersji API
    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo()
        {
            Title = $"DigitalWars API {description.ApiVersion}",
            Version = description.ApiVersion.ToString(),
            Description = "Dokumentacja API z wersjonowaniem"
        });
    }
});


var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("[API] Środowisko aplikacji: {Environment}", app.Environment.EnvironmentName);


// --- Konfiguracja zależna od środowiska ---
logger.LogInformation("[API] Konfiguracja Zależności");
if (app.Environment.IsDevelopment())
{
    // Konfiguracja dla LOKALNEGO TESTOWANIA
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });

    // Użyj CORS, aby pozwolić na komunikację z serwerem deweloperskim Vue
    app.UseCors(policy => policy
        .WithOrigins("http://localhost:9000") // Adres Twojego frontendu w trybie dev
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}
else
{
    // Konfiguracja dla PUBLIKACJI NA AZURE (i innych środowisk produkcyjnych)
    logger.LogInformation("[API] Konfiguracja Zależności dla: Plików statycznych i Przekierowania HTTPS");
    app.UseHttpsRedirection();
    app.UseDefaultFiles(); // Serwuj index.html
    app.UseStaticFiles(); // Serwuj pliki z wwwroot (zbudowany frontend)
}

logger.LogInformation("[API] Konfiguracja Autoryzacji i Autentykacji");
app.UseAuthentication();
app.UseAuthorization();

logger.LogInformation("[API] Konfiguracja Przekierowań");
app.MapControllers();
app.MapHub<GameHub>("/api/gameHub");

// Przekieruj wszystkie niepasujące do API ścieżki do frontendu (dla Vue Router)
app.MapFallbackToFile("/index.html");

logger.LogInformation("[API] Konfiguracja Migracji");
// Automatyczne migracje i inicjalizacja bazy danych przy starcie
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var provisioningService = scope.ServiceProvider.GetRequiredService<IProvisioningService>();
    try
    {
        logger.LogInformation("[API] Rozpoczynam migrację bazy danych...");
        logger.LogInformation("[API] Tworzę scope i pobieram serwisy...");

        context.Database.Migrate();
        logger.LogInformation("[API] Migracja zakończona.");

        DbInitializer.Initialize(context, provisioningService);
        logger.LogInformation("[API] Inicjalizacja zakończona.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[API] Błąd podczas migracji lub inicjalizacji.");
    }

}

logger.LogInformation("[API] Aplikacja DigitalWars została uruchomiona");

app.Run();