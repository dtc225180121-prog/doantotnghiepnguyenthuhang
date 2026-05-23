using aoe.Models;
using aoe.Services.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// DATABASE CONNECTION
// Production hosts often keep a localhost value in appsettings.json. Never use
// that fallback outside Development; prefer explicit cloud database variables.
// ======================
var connStr = ResolveConnectionString(builder);

Console.WriteLine("DB CONFIG: " + DescribeConnectionString(connStr));
Console.WriteLine("JWT CONFIG: " + (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]) ? "missing" : "set"));

if (string.IsNullOrEmpty(connStr))
{
    throw new Exception("❌ CONNECTION STRING IS NULL");
}

// ======================
// DATABASE (POSTGRESQL)
// ======================
builder.Services.AddDbContext<AoeDbContext>(options =>
    options.UseNpgsql(connStr, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
    })
);

// ======================
// CONTROLLERS
// ======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    );

// ======================
// JWT AUTH
// ======================
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"];

if (string.IsNullOrEmpty(key))
{
    throw new Exception("❌ JWT KEY IS NULL");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],

            RoleClaimType = ClaimTypes.Role,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                )
        };
});

// ======================
builder.Services.AddAuthorization();

// ======================
// CORS
// ======================
var allowedFrontendOrigins = new[]
{
    "http://127.0.0.1:5500",
    "http://localhost:5500",
    "https://aoe-frontend.onrender.com"
}
.Concat(
    (Environment.GetEnvironmentVariable("FRONTEND_ORIGINS") ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
)
.ToHashSet(StringComparer.OrdinalIgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
            .SetIsOriginAllowed(origin =>
            {
                if (allowedFrontendOrigins.Contains(origin))
                {
                    return true;
                }

                return Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                    uri.Scheme == Uri.UriSchemeHttps &&
                    uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

// ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "AOE API",
            Version = "v1"
        });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter: Bearer YOUR_TOKEN"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIService, AIService>();
// ======================
var app = builder.Build();

// ======================
// GLOBAL ERROR
// ======================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<
            Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        Console.WriteLine("🔥 ERROR: " + error?.Error?.ToString());

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = error?.Error?.Message
        });
    });
});

// ======================
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (app.Environment.IsDevelopment())
        {
            context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            context.Context.Response.Headers.Pragma = "no-cache";
            context.Context.Response.Headers.Expires = "0";
        }
    }
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Content(@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>English Hub</title>
    <style>
        body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:#111827;color:#fff;font-family:Arial,sans-serif}
        .card{max-width:560px;padding:32px 28px;border-radius:18px;background:rgba(255,255,255,.04);border:1px solid rgba(255,255,255,.1);box-shadow:0 20px 60px rgba(0,0,0,.28)}
        h1{margin:0 0 12px;font-size:32px}
        p{margin:0 0 18px;line-height:1.6;color:#d1d5db}
        a{color:#fbbf24;text-decoration:none;font-weight:700}
        .links{display:flex;gap:12px;flex-wrap:wrap}
        .btn{display:inline-block;padding:12px 16px;border-radius:12px;background:#6b21a8;color:#fff}
    </style>
</head>
<body>
    <div class='card'>
        <h1>English Hub</h1>
        <p>The service is running. Open the login page below to use the app.</p>
        <div class='links'>
            <a class='btn' href='/pages/auth/login.html'>Open Login</a>
            <a href='/healthz'>Health Check</a>
        </div>
    </div>
</body>
</html>", "text/html"));

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapGet("/healthz/db", async (AoeDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();

        return canConnect
            ? Results.Ok(new { status = "ok", database = "connected" })
            : Results.StatusCode(503);
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB HEALTH ERROR: " + ex);
        return Results.Problem("Database is unavailable", statusCode: 503);
    }
});

// ======================
// DATABASE MIGRATION
// Keep startup resilient on cloud hosts: a temporary DB issue should not
// prevent the web app from starting and serving static pages.
// ======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AoeDbContext>();

    try
    {
        Console.WriteLine("🔥 START MIGRATION");
        db.Database.Migrate();
        Console.WriteLine("✅ MIGRATION DONE");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ MIGRATION ERROR: " + ex);
    }
}

// ======================
app.Run();

static string? ResolveConnectionString(WebApplicationBuilder appBuilder)
{
    var isDevelopment = appBuilder.Environment.IsDevelopment();

    var candidates = new[]
    {
        ("ConnectionStrings__DefaultConnection", Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")),
        ("SUPABASE_DB_URL", NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("SUPABASE_DB_URL"))),
        ("CUSTOM_CONNECTION", NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("CUSTOM_CONNECTION"))),
        ("DATABASE_URL", NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("DATABASE_URL"))),
        ("appsettings:DefaultConnection", isDevelopment
            ? appBuilder.Configuration.GetConnectionString("DefaultConnection")
            : null)
    };

    foreach (var (name, value) in candidates)
    {
        var cleaned = CleanConnectionString(value);

        if (string.IsNullOrEmpty(cleaned))
        {
            continue;
        }

        if (!isDevelopment && LooksLikeLocalDatabase(cleaned))
        {
            Console.WriteLine($"DB CONFIG: skipped local connection string from {name} in Production");
            continue;
        }

        Console.WriteLine($"DB CONFIG: using {name}");
        return cleaned;
    }

    return null;
}

static string? CleanConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return null;
    }

    connectionString = connectionString.Trim();

    if (connectionString.Length >= 2 &&
        ((connectionString[0] == '"' && connectionString[^1] == '"') ||
        (connectionString[0] == '\'' && connectionString[^1] == '\'')))
    {
        connectionString = connectionString[1..^1].Trim();
    }

    return connectionString;
}

static bool LooksLikeLocalDatabase(string connectionString)
{
    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = builder.Host?.Trim().ToLowerInvariant();

        return host is "localhost" or "127.0.0.1" or "::1";
    }
    catch
    {
        return connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }
}

static string DescribeConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return "missing";
    }

    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"Host={builder.Host};Port={builder.Port};Database={builder.Database};Username={builder.Username};SslMode={builder.SslMode}";
    }
    catch
    {
        return "set";
    }
}

static string? NormalizeDatabaseUrl(string? databaseUrl)
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        return null;
    }

    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
    {
        return databaseUrl;
    }

    if (!string.Equals(uri.Scheme, "postgres", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(uri.Scheme, "postgresql", StringComparison.OrdinalIgnoreCase))
    {
        return databaseUrl;
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]),
        Password = uri.UserInfo.Contains(':')
            ? Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[1])
            : string.Empty,
        Database = uri.AbsolutePath.Trim('/'),
        SslMode = SslMode.Require,
        IncludeErrorDetail = true
    };

    if (uri.Query.Contains("sslmode=disable", StringComparison.OrdinalIgnoreCase))
    {
        builder.SslMode = SslMode.Disable;
    }

    return builder.ConnectionString;
}
