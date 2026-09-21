using DocumentProcessor.WebForms.Components;
using DocumentProcessor.WebForms.Configuration;
using DocumentProcessor.WebForms.Data;
using DocumentProcessor.WebForms.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------

// Blazor Server components + interactive server render mode.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// AppSettings: wraps IConfiguration; injected by ported components and services.
builder.Services.AddSingleton<AppSettings>();

// Database connection resolution (appsettings.json / AWS Secrets Manager fallback).
// Resolve once at startup and register the results as singletons so every
// consumer sees the same connection and the UI banner can display it.
var appSettings = new AppSettings(builder.Configuration);
var connection = await DatabaseConnectionResolver.ResolveAsync(appSettings);

if (connection.Warning != null)
{
    // Replaces the legacy Trace.TraceWarning in Application_Start.
    Console.WriteLine("[WARNING] " + connection.Warning);
}

// Register DatabaseInfo as a singleton so MainLayout can @inject it.
builder.Services.AddSingleton(connection.Info);

// EF Core DbContext — replaces the static DocumentDbContext.ConnectionString
// assignment from Application_Start and the parameterless constructor usage.
// Register as a factory so Blazor components can create short-lived scoped
// instances with IDbContextFactory<DocumentDbContext>.
builder.Services.AddDbContextFactory<DocumentDbContext>(options =>
    options.UseSqlServer(connection.ConnectionString));

// Application services consumed by ported Blazor components.
builder.Services.AddScoped<IDocumentStorage, FileSystemDocumentStorage>();
builder.Services.AddScoped<DocumentTextExtractor>();
builder.Services.AddScoped<IDocumentSummarizer, BedrockDocumentSummarizer>();
builder.Services.AddScoped<DocumentPipeline>();

// HttpContextAccessor — needed by any service that touches HttpContext.
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------------------
// Request-size limits
// ---------------------------------------------------------------------------
// Web.config had httpRuntime maxRequestLength="512000" (KB) and IIS
// requestLimits maxAllowedContentLength="524288000" (bytes ≈ 500 MB).
// Map those to the ASP.NET Core equivalents.

var maxRequestBytes = 524_288_000L; // ~500 MB

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxRequestBytes;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxRequestBytes;
});

// ---------------------------------------------------------------------------
// Build
// ---------------------------------------------------------------------------

var app = builder.Build();

// ---------------------------------------------------------------------------
// Database schema initialisation (replaces DatabaseInitializer.EnsureSchema
// called from Application_Start). Uses Microsoft.Data.SqlClient directly
// to run App_Data/Schema.sql, matching the legacy behaviour.
// ---------------------------------------------------------------------------
try
{
    var schemaPath = Path.Combine(app.Environment.ContentRootPath, "App_Data", "Schema.sql");
    DatabaseInitializer.EnsureSchema(connection.ConnectionString, schemaPath);
}
catch (Exception ex)
{
    // Do not take the whole application down — matches the original
    // Application_Start catch behaviour.
    app.Logger.LogError(ex, "Could not prepare the database.");
}

// ---------------------------------------------------------------------------
// Middleware pipeline — order is load-bearing; see
// https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#middleware-order
// ---------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/ErrorPage");
    app.UseHsts();
}

// Custom-error page for 404 and other status codes (replaces Web.config
// <customErrors> with status-code-based redirects).
app.UseStatusCodePagesWithReExecute("/ErrorPage", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
