namespace ClinicManagement.Application.DTOs.Common
{
    public class PagedResponse<TItem>
    {
        public PagedResponse(
            IReadOnlyList<TItem> items,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public IReadOnlyList<TItem> Items { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages { get; }
    }
}
