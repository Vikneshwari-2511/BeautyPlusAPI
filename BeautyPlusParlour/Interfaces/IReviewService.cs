using BeautyPlusParlour.Models.DTOs.Review;

namespace BeautyPlusParlour.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken ct = default);
    Task<ReviewDto> UpdateAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken ct = default);
    Task<ReviewDto> HideAsync(Guid reviewId, Guid adminId, HideReviewRequest request, CancellationToken ct = default);
    Task<ReviewDto> UnhideAsync(Guid reviewId, Guid adminId, CancellationToken ct = default);
    Task<ReviewDto> GetByIdAsync(Guid reviewId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewDto>> GetByServiceAsync(Guid serviceId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewDto>> GetByStaffAsync(Guid staffId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewDto>> GetAllAsync(bool includeHidden, CancellationToken ct = default);
    Task<ReviewSummaryDto> GetServiceSummaryAsync(Guid serviceId, CancellationToken ct = default);
    Task<ReviewSummaryDto> GetStaffSummaryAsync(Guid staffId, CancellationToken ct = default);
}