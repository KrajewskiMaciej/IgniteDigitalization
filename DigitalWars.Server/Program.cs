using backend.Data;
using backend.Services;
using backend.Initializers;
using backend.Server.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuestPDF.Infrastructure;
using backend.Hubs;
using Microsoft.AspNetCore.HttpOverrides;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- 1. INFRASTRUKTURA ---
builder.Services.AddSignalR();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. MAKSYMALNIE OTWARTY CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Przyjmij każde połączenie
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Pozwól na ciasteczka
    });
});

// --- 3. BAZA DANYCH ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 4. SETTINGS ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));

// --- 5. SERWISY (Zarejestrowane raz, czysto) ---
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IProvisioningService, ProvisioningService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IPlayerQueryService, PlayerQueryService>();
builder.Services.AddScoped<IPlayerActionService, PlayerActionService>();
builder.Services.AddScoped<ICheatsheetService, CheatsheetService>();
builder.Services.AddScoped<IEconomyService, EconomyService>();

builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx => new BackgroundTaskQueue(100));
builder.Services.AddHostedService<QueuedHostedService>();

// --- 6. AUTENTYKACJA ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "itm.auth.cookie";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- 7. MIDDLEWARE (Kolejność jest kluczowa dla działania CORS i Auth) ---

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// CORS MUSI BYĆ TUTAJ (przed Auth i przed MapControllers)
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Serwowanie frontendu (Vue) ze wwwroot – aktywne gdy ServeStaticFiles=true lub w trybie produkcyjnym bez jawnego ustawienia
var serveStatic = app.Configuration.GetValue<bool?>("FrontendSettings:ServeStaticFiles") ?? !app.Environment.IsDevelopment();
if (serveStatic)
{
    app.UseDefaultFiles();   // index.html jako domyślny
    app.UseStaticFiles();    // pliki z wwwroot/
}

app.MapControllers();
app.MapHub<GameHub>("/api/gameHub");

if (serveStatic)
{
    // SPA fallback – wszystkie trasy nieznane API trafiają do index.html
    app.MapFallbackToFile("index.html");
}

// --- 8. INICJALIZACJA BAZY ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<IProvisioningService>();
        context.Database.Migrate();
        DbInitializer.Initialize(context, provisioningService);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Błąd bazy");
    }
}

app.Run();