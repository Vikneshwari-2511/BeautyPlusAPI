using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Extensions;
using Serilog;
using Serilog.Events;

// ── Bootstrap logger (captures startup errors) ────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

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
        .AddControllers();

    var app = builder.Build();

    // ── Middleware pipeline ────────────────────────────────────────────────
    app.UseGlobalExceptionHandler();
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

    app.UseHttpsRedirection();
    app.UseSecureHeaders(); 
    app.UseCors("AllowAngular");
    app.UseRateLimiter();    
    app.UseAuthentication();
    app.UseAuthorization();
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