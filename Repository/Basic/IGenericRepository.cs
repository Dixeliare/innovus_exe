namespace Repository.Basic;

public interface IGenericRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    
    //even when these 2 methods GetAllAsync and GetByIdAsync are not being used. We still keep it to not breaking the consistency and purpose of a common Repository Pattern.
}