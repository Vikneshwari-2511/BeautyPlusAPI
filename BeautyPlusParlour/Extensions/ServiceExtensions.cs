using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Auth;
using BeautyPlusParlour.Models.DTOs.Booking;
using BeautyPlusParlour.Models.DTOs.Category;
using BeautyPlusParlour.Models.DTOs.Coupon;
using BeautyPlusParlour.Models.DTOs.Customer;
using BeautyPlusParlour.Models.DTOs.Loyalty;
using BeautyPlusParlour.Models.DTOs.Review;
using BeautyPlusParlour.Models.DTOs.Service;
using BeautyPlusParlour.Models.DTOs.Staff;
using BeautyPlusParlour.Models.DTOs.SubCategory;
using BeautyPlusParlour.Services;
using BeautyPlusParlour.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
namespace BeautyPlusParlour.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(
                    config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.EnableRetryOnFailure(3))
                .UseSnakeCaseNamingConvention());

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<SessionCleanupService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditService, AuditService>();
        // ── Module 2: Service Management ──────────────────────────────────────────
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISubCategoryService, SubCategoryService>();
        services.AddScoped<IServiceManagementService, ServiceManagementService>();
        // ── Module 3: Staff ────────────────────────────────────────────────────────
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IStaffSkillService, StaffSkillService>();
        services.AddScoped<IStaffScheduleService, StaffScheduleService>();
        services.AddScoped<IStaffLeaveService, StaffLeaveService>();
        // ── Module 4: Customer ────────────────────────────────────────────────────
        services.AddScoped<ICustomerService, CustomerService>();
        // ── Module 5: Booking ─────────────────────────────────────────────────────
        services.AddScoped<IBookingService, BookingService>();
        // ── Module 6: Loyalty & Coupons ────────────────────────────────────────────
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddHostedService<LoyaltyExpiryService>();
        // ── Module 7: Reviews ──────────────────────────────────────────────────────
        services.AddScoped<IReviewService, ReviewService>();
        // ── Module 8: Notifications ────────────────────────────────────────────────
        services.AddScoped<INotificationService, NotificationService>();
        // ── Module 9: Dashboard ────────────────────────────────────────────────────
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }

    public static IServiceCollection AddValidators(
        this IServiceCollection services)
    {
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<ResendVerificationRequest>, ResendVerificationRequestValidator>();
        // ── Module 2 validators ────────────────────────────────────────────────────
        services.AddScoped<IValidator<CreateCategoryRequest>, CreateCategoryValidator>();
        services.AddScoped<IValidator<UpdateCategoryRequest>, UpdateCategoryValidator>();
        services.AddScoped<IValidator<CreateSubCategoryRequest>, CreateSubCategoryValidator>();
        services.AddScoped<IValidator<UpdateSubCategoryRequest>, UpdateSubCategoryValidator>();
        services.AddScoped<IValidator<CreateServiceRequest>, CreateServiceValidator>();
        services.AddScoped<IValidator<UpdateServiceRequest>, UpdateServiceValidator>();
        // ── Module 3 validators ────────────────────────────────────────────────────
        services.AddScoped<IValidator<CreateStaffRequest>, CreateStaffValidator>();
        services.AddScoped<IValidator<UpdateStaffRequest>, UpdateStaffValidator>();
        services.AddScoped<IValidator<UpdateOwnProfileRequest>, UpdateOwnProfileValidator>();
        services.AddScoped<IValidator<AddSkillRequest>, AddSkillValidator>();
        services.AddScoped<IValidator<RequestLeaveRequest>, RequestLeaveValidator>();
        services.AddScoped<IValidator<RejectLeaveRequest>, RejectLeaveValidator>();
        // ── Module 4 validators ───────────────────────────────────────────────────
        services.AddScoped<IValidator<UpdateProfileRequest>, UpdateProfileValidator>();
        services.AddScoped<IValidator<CreateAddressRequest>, CreateAddressValidator>();
        services.AddScoped<IValidator<UpdateAddressRequest>, UpdateAddressValidator>();
        // ── Module 5 validators ───────────────────────────────────────────────────
        services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingValidator>();
        services.AddScoped<IValidator<RescheduleBookingRequest>, RescheduleBookingValidator>();
        services.AddScoped<IValidator<RecordPaymentRequest>, RecordPaymentValidator>();
        // ── Module 6 validators ────────────────────────────────────────────────────
        services.AddScoped<IValidator<AdjustPointsRequest>, AdjustPointsValidator>();
        services.AddScoped<IValidator<ValidateRedeemRequest>, ValidateRedeemValidator>();
        services.AddScoped<IValidator<CreateCouponRequest>, CreateCouponValidator>();
        services.AddScoped<IValidator<UpdateCouponRequest>, UpdateCouponValidator>();
        services.AddScoped<IValidator<ValidateCouponRequest>, ValidateCouponValidator>();
        // ── Module 7 validators ────────────────────────────────────────────────────
        services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewValidator>();
        services.AddScoped<IValidator<UpdateReviewRequest>, UpdateReviewValidator>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(opts =>
        {
            opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            opts.AddPolicy("StaffOrAdmin", p => p.RequireRole("Admin", "Staff"));
            opts.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));
        });

        return services;
    }

    public static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Beauty Plus Parlour API",
                Version = "v1",
                Description = "Authentication & Session Management"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                            { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration config)
    {
        var origin = config["App:FrontendUrl"] ?? "http://localhost:5173";

        services.AddCors(opts =>
            opts.AddPolicy("ReactPolicy", policy =>
                policy.WithOrigins(origin)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()));

        return services;
    }

    // Add this method to ServiceExtensions
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Login: 5 attempts per minute per IP
            opts.AddFixedWindowLimiter("login", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 5;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // OTP / forgot-password: 3 attempts per 10 minutes per IP
            opts.AddFixedWindowLimiter("otp", o =>
            {
                o.Window = TimeSpan.FromMinutes(10);
                o.PermitLimit = 3;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Resend verification: 2 per 5 minutes per IP
            opts.AddFixedWindowLimiter("resend", o =>
            {
                o.Window = TimeSpan.FromMinutes(5);
                o.PermitLimit = 2;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Token refresh: 10 per minute per IP
            opts.AddFixedWindowLimiter("refresh", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 10;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}