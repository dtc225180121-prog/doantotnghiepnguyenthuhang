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
// 🔥 FIX ENV CONNECTION STRING (QUAN TRỌNG NHẤT)
// Prefer cloud env vars in production; only fall back to localhost for local dev.
// ======================
var connStr =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("SUPABASE_DB_URL"))
    ?? Environment.GetEnvironmentVariable("CUSTOM_CONNECTION")
    ?? NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("DATABASE_URL"))
    ?? (builder.Environment.IsDevelopment()
        ? builder.Configuration.GetConnectionString("DefaultConnection")
        : null);

Console.WriteLine("🔥 CONN RAW: " + connStr);
Console.WriteLine("🔥 JWT KEY: " + builder.Configuration["Jwt:Key"]);

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "https://aoe-frontend.onrender.com"
            )
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
app.UseStaticFiles();

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