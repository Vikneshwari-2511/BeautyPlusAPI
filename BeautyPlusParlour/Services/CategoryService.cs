using BeautyPlus.Helpers;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.Category;
using BeautyPlusParlour.Models.DTOs.SubCategory;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BeautyPlusParlour.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        AppDbContext db,
        IAuditService audit,
        ILogger<CategoryService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        Guid adminId,
        CancellationToken ct = default)
    {
        // Uniqueness check
        var nameExists = await _db.Categories
            .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower().Trim(), ct);

        if (nameExists)
            throw new AppException(ResponseMessages.CategoryNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), null, ct);

        var category = Category.Create(
            request.Name, slug,
            request.Description,
            request.ServiceTypeDefault,
            request.ImageUrl,
            request.DisplayOrder,
            adminId);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "Category", category.Id.ToString(),
            null, JsonSerializer.Serialize(new { category.Name, category.Slug }),
            ct: ct);

        _logger.LogInformation(
            "Category created: {Name} by AdminId {AdminId}",
            category.Name, adminId);

        return await GetByIdAsync(category.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid id, UpdateCategoryRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.CategoryNotFound);

        // Uniqueness check — exclude current
        var nameExists = await _db.Categories
            .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower().Trim()
                        && c.Id != id, ct);

        if (nameExists)
            throw new AppException(ResponseMessages.CategoryNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), id, ct);

        var oldValues = JsonSerializer.Serialize(
            new { category.Name, category.Slug });

        category.Update(
            request.Name, slug,
            request.Description,
            request.ServiceTypeDefault,
            request.ImageUrl,
            request.DisplayOrder,
            adminId);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "Category", id.ToString(),
            oldValues,
            JsonSerializer.Serialize(new { request.Name, slug }),
            ct: ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.CategoryNotFound);

        // Protect: cannot deactivate if active services exist
        var hasActiveServices = await _db.Services
            .AnyAsync(s => s.CategoryId == id && s.IsActive, ct);

        if (hasActiveServices)
            throw new AppException(ResponseMessages.CategoryHasServices);

        category.SoftDelete(adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "Category", id.ToString(),
            ct: ct);
    }

    public async Task<CategoryDto> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.CategoryNotFound);

        var serviceCount = await _db.Services
            .CountAsync(s => s.CategoryId == id && s.IsActive, ct);

        var subCategoryCount = await _db.SubCategories
            .CountAsync(sc => sc.CategoryId == id && sc.IsActive, ct);

        return MapToDto(category, serviceCount, subCategoryCount);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Categories.AsNoTracking();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var result = new List<CategoryDto>();

        foreach (var c in categories)
        {
            var count = await _db.Services
                .CountAsync(s => s.CategoryId == c.Id && s.IsActive, ct);
            var subCategoryCount = await _db.SubCategories
                .CountAsync(sc => sc.CategoryId == c.Id && sc.IsActive, ct);
            result.Add(MapToDto(c, count, subCategoryCount));
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<SubCategoryDto>> GetSubCategoriesAsync(
        Guid categoryId, CancellationToken ct = default)
    {
        var exists = await _db.Categories
            .AnyAsync(c => c.Id == categoryId, ct);

        if (!exists)
            throw new NotFoundException(ResponseMessages.CategoryNotFound);

        var subCategories = await _db.SubCategories
            .AsNoTracking()
            .Where(s => s.CategoryId == categoryId && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);

        var result = new List<SubCategoryDto>();

        foreach (var sc in subCategories)
        {
            var count = await _db.Services
                .CountAsync(s => s.SubCategoryId == sc.Id && s.IsActive, ct);

            result.Add(new SubCategoryDto(
                sc.Id, sc.CategoryId, string.Empty,
                sc.Name, sc.Slug,
                sc.DisplayOrder, sc.IsActive,
                count, sc.CreatedAt));
        }

        return result.AsReadOnly();
    }

    // ── private ─────────────────────────────────────────────────────────────
    private async Task<string> GenerateUniqueSlugAsync(
        string baseSlug, Guid? excludeId,
        CancellationToken ct)
    {
        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            var query = _db.Categories.Where(c => c.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            var exists = await query.AnyAsync(ct);
            if (!exists) return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }

    private static CategoryDto MapToDto(
        Category c, int serviceCount, int subCategoryCount) =>
        new(c.Id, c.Name, c.Slug, c.Description,
            c.ServiceTypeDefault, c.ImageUrl,
            c.DisplayOrder, c.IsActive,
            serviceCount, subCategoryCount, c.CreatedAt);
}