using backend.Data;
using backend.Initializers;
using backend.Services;
using backend.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja bazy danych z użyciem Connection String (działa lokalnie i w Azure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPlayerQueryService, PlayerQueryService>();
builder.Services.AddScoped<IPlayerActionService, PlayerActionService>();


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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Konfiguracja zależna od środowiska ---

if (app.Environment.IsDevelopment())
{
    // Konfiguracja dla LOKALNEGO TESTOWANIA
    app.UseSwagger();
    app.UseSwaggerUI();

    // Użyj CORS, aby pozwolić na komunikację z serwerem deweloperskim Vue
    app.UseCors(policy => policy
        .WithOrigins("http://localhost:61536") // Adres Twojego frontendu w trybie dev
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}
else
{
    // Konfiguracja dla PUBLIKACJI NA AZURE (i innych środowisk produkcyjnych)
    app.UseHttpsRedirection();
    app.UseDefaultFiles(); // Serwuj index.html
    app.UseStaticFiles(); // Serwuj pliki z wwwroot (zbudowany frontend)
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gameHub");

// Przekieruj wszystkie niepasujące do API ścieżki do frontendu (dla Vue Router)
app.MapFallbackToFile("/index.html");

// Automatyczne migracje i inicjalizacja bazy danych przy starcie
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var provisioningService = scope.ServiceProvider.GetRequiredService<IProvisioningService>();
    try
    {
        context.Database.Migrate();
        DbInitializer.Initialize(context, provisioningService);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Wystąpił błąd podczas migracji lub inicjalizacji bazy danych.");
    }
}

app.Run();