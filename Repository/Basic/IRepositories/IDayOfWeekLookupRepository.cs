using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IDayOfWeekLookupRepository : IGenericRepository<day_of_week_lookup>
{
    // Có thể thêm các phương thức riêng nếu cần, nhưng thường GenericRepository là đủ cho lookup table
}