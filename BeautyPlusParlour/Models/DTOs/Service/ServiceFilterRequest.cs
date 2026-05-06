using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Models.DTOs.Service;

public sealed record ServiceFilterRequest
{
    public Guid? CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public ServiceType? ServiceType { get; init; }
    public Gender? Gender { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? IsPopular { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}