namespace BeautyPlusParlour.Models.DTOs.Common
{
    public sealed class PagedResponse<T>
    {        
        public IReadOnlyList<T> Data { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;
        public PagedResponse(IReadOnlyList<T> data, int totalCount, int page, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }
    }

     
    }
