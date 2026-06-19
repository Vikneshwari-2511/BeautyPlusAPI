using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Extensions;
using Serilog;
using Serilog.Events;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;
// ── Bootstrap logger (captures startup errors) ────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var allowedOrigins = builder.Configuration["AllowedOrigins"];
    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/beauty-parlour-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                    "{Message:lj}{NewLine}{Exception}");
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular",
            policy =>
            {
                policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
    });
    // ── Strongly typed settings ───────────────────────────────────────────
    builder.Services.Configure<JwtSettings>(
        builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection("EmailSettings"));

    // ── Services ──────────────────────────────────────────────────────────
    builder.Services
        .AddDatabase(builder.Configuration)
        .AddApplicationServices()
        .AddValidators()
        .AddJwtAuthentication(builder.Configuration)
        .AddCorsPolicy(builder.Configuration)
        .AddRateLimiting()               // ← NEW
        .AddSwagger()
        .AddControllers()
       .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy
                = JsonNamingPolicy.CamelCase;
        });
    // Redis Cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration
            .GetConnectionString("Redis");
        options.InstanceName = "BeautyPlus:";
    });

    var serviceAccountPath = builder.Configuration
    ["Firebase:ServiceAccountPath"];

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential
            .FromFile(serviceAccountPath)
    });
    var app = builder.Build();

    // ── Middleware pipeline ────────────────────────────────────────────────
    app.UseGlobalExceptionHandler();
    //if (app.Environment.IsDevelopment())
    //{
    //    app.UseDeveloperExceptionPage();
    //}

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append(
            "Cross-Origin-Opener-Policy",
            "same-origin-allow-popups"
        );

        await next();
    });

    app.UseHttpsRedirection();

    // ADD before app.UseRouting()
    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });

    app.UseStaticFiles();

    app.UseSecureHeaders();

    app.UseCors("AllowAngular");

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapControllers();

    await app.MigrateDatabaseAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}