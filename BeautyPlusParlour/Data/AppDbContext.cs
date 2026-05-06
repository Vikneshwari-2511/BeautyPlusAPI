using BeautyPlusParlour.Data.Configurations;
using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<OnSiteDetail> OnSiteDetails => Set<OnSiteDetail>();
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<StaffSkill> StaffSkills => Set<StaffSkill>();
    public DbSet<StaffSchedule> StaffSchedules => Set<StaffSchedule>();
    public DbSet<StaffLeave> StaffLeaves => Set<StaffLeave>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<FavouriteService> FavouriteServices => Set<FavouriteService>();    
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Payment> Payments => Set<Payment>();    
    public DbSet<CustomerLoyaltyPoints> CustomerLoyaltyPoints => Set<CustomerLoyaltyPoints>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();    
    public DbSet<Review> Reviews => Set<Review>();
    
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
        
    }
}