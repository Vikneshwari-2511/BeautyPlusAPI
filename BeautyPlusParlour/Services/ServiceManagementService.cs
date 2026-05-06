using BeautyPlus.Helpers;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Common;
using BeautyPlusParlour.Models.DTOs.Service;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BeautyPlusParlour.Services;

public sealed class ServiceManagementService : IServiceManagementService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<ServiceManagementService> _logger;

    public ServiceManagementService(
        AppDbContext db,
        IAuditService audit,
        ILogger<ServiceManagementService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<ServiceDto> CreateAsync(
        CreateServiceRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        await ValidateForeignKeysAsync(
            request.CategoryId, request.SubCategoryId, ct);

        var nameExists = await _db.Services
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower().Trim()
                        && s.CategoryId == request.CategoryId, ct);

        if (nameExists)
            throw new AppException(ResponseMessages.ServiceNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), null, ct);

        // Auto-calculate points, allow override
        var loyaltyPoints = request.LoyaltyPointsOverride.HasValue
            ? request.LoyaltyPointsOverride.Value
            : Service.CalculateLoyaltyPoints(
                request.BasePrice, request.ServiceTypeActual);

        var isOverridden = request.LoyaltyPointsOverride.HasValue;

        var service = Service.Create(
            request.CategoryId, request.SubCategoryId,
            request.Name, slug, request.Description,
            request.ServiceTypeActual, request.Gender,
            request.BasePrice, request.DiscountedPrice,
            request.DurationMinutes, request.BufferMinutes,
            loyaltyPoints, isOverridden,
            request.ImageUrl, request.DisplayOrder,
            request.IsFeatured, request.IsPopular,
            request.RequiresConsultation,
            request.IsTaxInclusive, request.TaxPercent,
            adminId);

        _db.Services.Add(service);

        // Add OnSiteDetail if needed
        if (request.OnSiteDetail is not null &&
            (request.ServiceTypeActual == ServiceType.OnSite ||
             request.ServiceTypeActual == ServiceType.Both))
        {
            var onSiteDetail = OnSiteDetail.Create(
                service.Id,
                request.OnSiteDetail.TravelCharge,
                request.OnSiteDetail.AdvancePercent,
                request.OnSiteDetail.MinBookingDays,
                request.OnSiteDetail.SpecialNotes);

            _db.OnSiteDetails.Add(onSiteDetail);
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "Service", service.Id.ToString(),
            null, JsonSerializer.Serialize(new
            {
                service.Name,
                service.BasePrice,
                service.ServiceTypeActual
            }), ct: ct);

        _logger.LogInformation(
            "Service created: {Name} (₹{Price}) by AdminId {AdminId}",
            service.Name, service.BasePrice, adminId);

        return await GetByIdAsync(service.Id, ct);
    }

    public async Task<ServiceDto> UpdateAsync(
        Guid id, UpdateServiceRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var service = await _db.Services
            .Include(s => s.OnSiteDetail)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        await ValidateForeignKeysAsync(
            request.CategoryId, request.SubCategoryId, ct);

        var nameExists = await _db.Services
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower().Trim()
                        && s.CategoryId == request.CategoryId
                        && s.Id != id, ct);

        if (nameExists)
            throw new AppException(ResponseMessages.ServiceNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), id, ct);

        var loyaltyPoints = request.LoyaltyPointsOverride.HasValue
            ? request.LoyaltyPointsOverride.Value
            : Service.CalculateLoyaltyPoints(
                request.BasePrice, request.ServiceTypeActual);

        var oldValues = JsonSerializer.Serialize(new
        {
            service.Name,
            service.BasePrice,
            service.ServiceTypeActual
        });

        service.Update(
            request.CategoryId, request.SubCategoryId,
            request.Name, slug, request.Description,
            request.ServiceTypeActual, request.Gender,
            request.BasePrice, request.DiscountedPrice,
            request.DurationMinutes, request.BufferMinutes,
            loyaltyPoints, request.LoyaltyPointsOverride.HasValue,
            request.ImageUrl, request.DisplayOrder,
            request.IsFeatured, request.IsPopular,
            request.RequiresConsultation,
            request.IsTaxInclusive, request.TaxPercent,
            adminId);

        // Update or create OnSiteDetail
        var needsOnSiteDetail =
            request.ServiceTypeActual == ServiceType.OnSite ||
            request.ServiceTypeActual == ServiceType.Both;

        if (needsOnSiteDetail && request.OnSiteDetail is not null)
        {
            if (service.OnSiteDetail is not null)
                service.OnSiteDetail.Update(
                    request.OnSiteDetail.TravelCharge,
                    request.OnSiteDetail.AdvancePercent,
                    request.OnSiteDetail.MinBookingDays,
                    request.OnSiteDetail.SpecialNotes);
            else
                _db.OnSiteDetails.Add(OnSiteDetail.Create(
                    service.Id,
                    request.OnSiteDetail.TravelCharge,
                    request.OnSiteDetail.AdvancePercent,
                    request.OnSiteDetail.MinBookingDays,
                    request.OnSiteDetail.SpecialNotes));
        }
        else if (!needsOnSiteDetail && service.OnSiteDetail is not null)
        {
            // Service type changed to Parlour — remove onsite detail
            _db.OnSiteDetails.Remove(service.OnSiteDetail);
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Service", id.ToString(),
            oldValues,
            JsonSerializer.Serialize(new { request.Name, request.BasePrice }),
            ct: ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        service.SoftDelete(adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "Service", id.ToString(), ct: ct);
    }

    public async Task ToggleActiveAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        service.ToggleActive(adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Service", id.ToString(),
            null,
            JsonSerializer.Serialize(new { IsActive = service.IsActive }),
            ct: ct);
    }

    public async Task<ServiceDto> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var service = await _db.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Include(s => s.OnSiteDetail)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.ServiceNotFound);

        return MapToDto(service);
    }

    public async Task<PagedResponse<ServiceListDto>> GetAllAsync(
        ServiceFilterRequest filter, CancellationToken ct = default)
    {
        var pageSize = Math.Min(
            filter.PageSize, ServiceConstants.MaxPageSize);

        var query = _db.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Where(s => s.IsActive)
            .AsQueryable();

        if (filter.CategoryId.HasValue)
            query = query.Where(s => s.CategoryId == filter.CategoryId);

        if (filter.SubCategoryId.HasValue)
            query = query.Where(s => s.SubCategoryId == filter.SubCategoryId);

        if (filter.ServiceType.HasValue)
            query = query.Where(s => s.ServiceTypeActual == filter.ServiceType);

        if (filter.Gender.HasValue)
            query = query.Where(s =>
                s.Gender == filter.Gender ||
                s.Gender == Gender.Both);

        if (filter.MinPrice.HasValue)
            query = query.Where(s => s.BasePrice >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(s => s.BasePrice <= filter.MaxPrice);

        if (filter.IsFeatured.HasValue)
            query = query.Where(s => s.IsFeatured == filter.IsFeatured);

        if (filter.IsPopular.HasValue)
            query = query.Where(s => s.IsPopular == filter.IsPopular);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower().Trim();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Description != null &&
                 s.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);

        var services = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Skip((filter.Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<ServiceListDto>(
            services.Select(MapToListDto).ToList(),
            totalCount,
            filter.Page,
            pageSize);
    }

    public async Task<IReadOnlyList<ServiceListDto>> GetFeaturedAsync(
        CancellationToken ct = default)
    {
        var services = await _db.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Where(s => s.IsActive && s.IsFeatured)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

        return services.Select(MapToListDto).ToList().AsReadOnly();
    }

    // ── private ─────────────────────────────────────────────────────────────
    private async Task ValidateForeignKeysAsync(
        Guid categoryId, Guid? subCategoryId,
        CancellationToken ct)
    {
        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == categoryId && c.IsActive, ct);

        if (!categoryExists)
            throw new NotFoundException(ResponseMessages.CategoryNotFound);

        if (subCategoryId.HasValue)
        {
            var subCategoryExists = await _db.SubCategories
                .AnyAsync(s => s.Id == subCategoryId && s.IsActive, ct);

            if (!subCategoryExists)
                throw new NotFoundException(ResponseMessages.SubCategoryNotFound);
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string baseSlug, Guid? excludeId,
        CancellationToken ct)
    {
        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            var query = _db.Services.Where(s => s.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            var exists = await query.AnyAsync(ct);
            if (!exists) return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }

    private static ServiceDto MapToDto(Service s) =>
        new(s.Id, s.CategoryId, s.Category.Name,
            s.SubCategoryId, s.SubCategory?.Name,
            s.Name, s.Slug, s.Description,
            s.ServiceTypeActual, s.Gender,
            s.BasePrice, s.DiscountedPrice,
            s.DiscountedPrice ?? s.BasePrice,
            s.DurationMinutes, s.BufferMinutes,
            s.DurationMinutes + s.BufferMinutes,
            s.LoyaltyPoints, s.ImageUrl,
            s.DisplayOrder, s.IsActive,
            s.IsFeatured, s.IsPopular,
            s.RequiresConsultation,
            s.IsTaxInclusive, s.TaxPercent,
            s.OnSiteDetail is null ? null : new OnSiteDetailDto(
                s.OnSiteDetail.TravelCharge,
                s.OnSiteDetail.AdvancePercent,
                s.OnSiteDetail.MinBookingDays,
                s.OnSiteDetail.SpecialNotes),
            s.CreatedAt);

    private static ServiceListDto MapToListDto(Service s) =>
        new(s.Id, s.Name, s.Slug,
            s.Category.Name, s.SubCategory?.Name,
            s.ServiceTypeActual, s.Gender,
            s.DiscountedPrice ?? s.BasePrice,
            s.DurationMinutes, s.LoyaltyPoints,
            s.ImageUrl, s.IsFeatured, s.IsPopular);
}