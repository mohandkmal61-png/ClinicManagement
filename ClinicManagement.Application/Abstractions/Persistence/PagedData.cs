namespace ClinicManagement.Application.Abstractions.Persistence
{
    public class PagedData<TEntity>
    {
        public PagedData(IReadOnlyList<TEntity> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }

        public IReadOnlyList<TEntity> Items { get; }

        public int TotalCount { get; }
    }
}
