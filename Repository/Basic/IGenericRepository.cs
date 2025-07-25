using System.Linq.Expressions;

namespace Repository.Basic;

public interface IGenericRepository<T> where T : class
{
    // "Host=localhost;Port=5432;Database=innovus_db;Username=postgres;Password=12345"
    
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities); // Đã thêm
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);

    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate); 

    //even when these 2 methods GetAllAsync and GetByIdAsync are not being used. We still keep it to not breaking the consistency and purpose of a common Repository Pattern.
}