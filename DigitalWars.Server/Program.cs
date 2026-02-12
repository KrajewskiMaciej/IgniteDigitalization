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
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using backend.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- 1. KONFIGURACJA SIECIOWA (AZURE) ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- 2. MAKSYMALNIE OTWARTY CORS ---
builder.Services.AddCors(options =>
{
    // Używamy AddDefaultPolicy zamiast nazwanej polityki, aby działała automatycznie bez podawania nazwy w UseCors
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials() // Wymagane dla ciasteczek
              .SetIsOriginAllowed(_ => true); // Pozwala na dowolne origin (http/https)
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var loggerFactory = LoggerFactory.Create(config => { config.AddConsole(); });
var startupLogger = loggerFactory.CreateLogger("Startup");
startupLogger.LogInformation("[API] Start konfiguracji...");

// Baza danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Rejestracja serwisów (Email, Jwt, Game itd.)
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

// --- 3. AUTENTYKACJA (COOKIES TYLKO DO IDENTYFIKACJI) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "itm.auth.cookie";
        options.Cookie.HttpOnly = true;
        // ZMIANA: SameSite=None i Secure=Always są wymagane dla Cross-Site (Frontend i Backend na różnych subdomenach)
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

builder.Services.AddAuthorization(); // Brak polityk = brak dodatkowych restrykcji
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<DigitalWars.Server.Settings.ConfigureSwaggerOptions>();

var app = builder.Build();

// --- 4. MIDDLEWARE (Kolejność "Otwarta") ---

app.UseForwardedHeaders(); // Rozpoznawanie HTTPS na Azure

// DEBUG: Sprawdźmy, czy request w ogóle wchodzi do aplikacji i dodajmy nagłówek diagnostyczny
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-App-Version", "FixedCorsv2");
    await next();
});

app.UseCors(); // CORS (domyślna polityka z AddDefaultPolicy)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    });
}
else
{
    // Na Azure wyłączamy wymuszanie przekierowań, jeśli infrastruktura (App Service) już to robi.
    // To zapobiega błędom 400 (Bad Request) przy pętlach przekierowań.
    // app.UseHttpsRedirection(); 

    if (builder.Configuration.GetValue<bool>("FrontendSettings:ServeStaticFiles"))
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

// Baza danych
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<backend.Services.IProvisioningService>();
        context.Database.Migrate();
        DbInitializer.Initialize(context, provisioningService);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Błąd bazy");
    }
}

app.Run();