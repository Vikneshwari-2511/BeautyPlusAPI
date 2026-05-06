using BeautyPlus.Helpers;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.DTOs.SubCategory;
using BeautyPlusParlour.Models.Entities;
using BeautyPlusParlour.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BeautyPlusParlour.Services;

public sealed class SubCategoryService : ISubCategoryService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<SubCategoryService> _logger;

    public SubCategoryService(
        AppDbContext db,
        IAuditService audit,
        ILogger<SubCategoryService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<SubCategoryDto> CreateAsync(
        CreateSubCategoryRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);

        if (!categoryExists)
            throw new NotFoundException(ResponseMessages.CategoryNotFound);

        var nameExists = await _db.SubCategories
            .AnyAsync(s => s.CategoryId == request.CategoryId
                        && s.Name.ToLower() == request.Name.ToLower().Trim(), ct);

        if (nameExists)
            throw new AppException(ResponseMessages.SubCategoryNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), null, ct);

        var subCategory = SubCategory.Create(
            request.CategoryId, request.Name,
            slug, request.DisplayOrder, adminId);

        _db.SubCategories.Add(subCategory);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Created,
            "SubCategory", subCategory.Id.ToString(),
            null, JsonSerializer.Serialize(new { subCategory.Name }),
            ct: ct);

        return await GetByIdAsync(subCategory.Id, ct);
    }

    public async Task<SubCategoryDto> UpdateAsync(
        Guid id, UpdateSubCategoryRequest request,
        Guid adminId, CancellationToken ct = default)
    {
        var subCategory = await _db.SubCategories
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.SubCategoryNotFound);

        var nameExists = await _db.SubCategories
            .AnyAsync(s => s.CategoryId == request.CategoryId
                        && s.Name.ToLower() == request.Name.ToLower().Trim()
                        && s.Id != id, ct);

        if (nameExists)
            throw new AppException(ResponseMessages.SubCategoryNameExists);

        var slug = await GenerateUniqueSlugAsync(
            SlugHelper.Generate(request.Name), id, ct);

        var oldValues = JsonSerializer.Serialize(
            new { subCategory.Name });

        subCategory.Update(request.Name, slug, request.DisplayOrder, adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Updated,
            "SubCategory", id.ToString(),
            oldValues,
            JsonSerializer.Serialize(new { request.Name }),
            ct: ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var subCategory = await _db.SubCategories
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.SubCategoryNotFound);

        var hasActiveServices = await _db.Services
            .AnyAsync(s => s.SubCategoryId == id && s.IsActive, ct);

        if (hasActiveServices)
            throw new AppException(ResponseMessages.SubCategoryHasServices);

        subCategory.SoftDelete(adminId);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            adminId, AuditAction.Deleted,
            "SubCategory", id.ToString(), ct: ct);
    }

    public async Task<SubCategoryDto> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var sc = await _db.SubCategories
            .AsNoTracking()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(ResponseMessages.SubCategoryNotFound);

        var count = await _db.Services
            .CountAsync(s => s.SubCategoryId == id && s.IsActive, ct);

        return new SubCategoryDto(
            sc.Id, sc.CategoryId, sc.Category.Name,
            sc.Name, sc.Slug, sc.DisplayOrder,
            sc.IsActive, count, sc.CreatedAt);
    }

    public async Task<IReadOnlyList<SubCategoryDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var list = await _db.SubCategories
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);

        var result = new List<SubCategoryDto>();

        foreach (var sc in list)
        {
            var count = await _db.Services
                .CountAsync(s => s.SubCategoryId == sc.Id && s.IsActive, ct);

            result.Add(new SubCategoryDto(
                sc.Id, sc.CategoryId, sc.Category.Name,
                sc.Name, sc.Slug, sc.DisplayOrder,
                sc.IsActive, count, sc.CreatedAt));
        }

        return result.AsReadOnly();
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string baseSlug, Guid? excludeId,
        CancellationToken ct)
    {
        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            var query = _db.SubCategories.Where(s => s.Slug == slug);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            var exists = await query.AnyAsync(ct);
            if (!exists) return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }
}