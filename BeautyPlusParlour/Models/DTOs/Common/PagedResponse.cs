namespace BeautyPlusParlour.Models.DTOs.Common
{
    public sealed class PagedResponse<T>
    {
        //public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
        //public int TotalCount { get; init; }
        //public int PageNumber { get; init; }
        //public int PageSize { get; init; }
        //public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        //public bool HasNextPage => PageNumber < TotalPages;
        //public bool HasPrevPage => PageNumber > 1;
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
