using System.Linq.Expressions;

namespace ClinicManagement.Application.Abstractions.Persistence
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PagedData<TEntity>> GetPageAsync<TKey>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TKey>> orderBy,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void Delete(TEntity entity);
    }
}
