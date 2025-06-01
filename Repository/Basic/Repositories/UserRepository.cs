using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class UserRepository: GenericRepository<user>
{
    public UserRepository()
    {
    }

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<user>> GetAllAsync()
    {
        return await _context.users
            .Include(a => a.attendances)
            .Include(o => o.opening_schedule)
            .Include(r => r.role)
            .Include(s=> s.schedule)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
            .Include(d => d.documents)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<user> GetByIdAsync(int id)
    {
        return await _context.users
            .Include(a => a.attendances)
            .Include(o => o.opening_schedule)
            .Include(r => r.role)
            .Include(s=> s.schedule)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
            .Include(d => d.documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.user_id == id);
    }
    
    public async Task<user?> GetByUsernameAsync(string username)
    {
        return await _context.users.AsNoTracking().FirstOrDefaultAsync(u => u.username == username);
    }

    public async Task<user> AddAsync(user entity)
    {
        _context.users.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(user entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.users.FindAsync(id);
        if (item == null) return false;
        _context.users.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<user>> SearchUsersAsync(
            string? username = null,
            string? accountName = null,
            string? password = null,
            string? address = null,
            string? phoneNumber = null,
            bool? isDisabled = null,
            DateTime? createAt = null,
            DateOnly? birthday = null,
            int? roleId = null)
        {
            IQueryable<user> query = _context.users;

            // Luôn bao gồm các navigation property bạn muốn trả về cùng kết quả
            query = query.Include(u => u.role)
                         .Include(u => u.statistic)
                         .Include(u => u.opening_schedule)
                         .Include(u => u.schedule)
                         .AsSplitQuery();

            // Xây dựng danh sách các biểu thức điều kiện (predicates)
            var predicates = new List<Expression<Func<user, bool>>>();

            if (!string.IsNullOrEmpty(username))
            {
                var lowerUsername = username.ToLower();
                predicates.Add(u => u.username != null && u.username.ToLower().Contains(lowerUsername));
            }

            if (!string.IsNullOrEmpty(accountName))
            {
                var lowerAccountName = accountName.ToLower();
                predicates.Add(u => u.account_name != null && u.account_name.ToLower().Contains(lowerAccountName));
            }

            if (!string.IsNullOrEmpty(password))
            {
                // Cẩn thận: Tìm kiếm theo mật khẩu thô không được khuyến khích trong thực tế.
                // Mật khẩu nên được hash và không thể tìm kiếm ngược lại.
                var lowerPassword = password.ToLower();
                predicates.Add(u => u.password.ToLower().Contains(lowerPassword));
            }

            if (!string.IsNullOrEmpty(address))
            {
                var lowerAddress = address.ToLower();
                predicates.Add(u => u.address != null && u.address.ToLower().Contains(lowerAddress));
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var lowerPhoneNumber = phoneNumber.ToLower();
                predicates.Add(u => u.phone_number != null && u.phone_number.ToLower().Contains(lowerPhoneNumber));
            }

            if (isDisabled.HasValue)
            {
                predicates.Add(u => u.is_disabled == isDisabled.Value);
            }

            if (createAt.HasValue)
            {
                // So sánh ngày chính xác (có thể cần điều chỉnh để so sánh theo khoảng ngày nếu muốn)
                predicates.Add(u => u.create_at != null && u.create_at.Value.Date == createAt.Value.Date);
            }

            if (birthday.HasValue)
            {
                // So sánh ngày sinh chính xác (DateOnly)
                predicates.Add(u => u.birthday == birthday.Value);
            }

            if (roleId.HasValue)
            {
                predicates.Add(u => u.role_id == roleId.Value);
            }

            // Nếu có ít nhất một điều kiện tìm kiếm được cung cấp
            if (predicates.Any())
            {
                // Bắt đầu với biểu thức đầu tiên
                Expression<Func<user, bool>> combinedPredicate = predicates.First();

                // Nối các biểu thức còn lại bằng toán tử OR
                for (int i = 1; i < predicates.Count; i++)
                {
                    combinedPredicate = Expression.Lambda<Func<user, bool>>(
                        Expression.OrElse(combinedPredicate.Body, predicates[i].Body),
                        combinedPredicate.Parameters);
                }

                // Áp dụng biểu thức tổng hợp vào truy vấn
                query = query.Where(combinedPredicate);
            }
            // Nếu predicates rỗng (không có tiêu chí nào được cung cấp), query sẽ trả về tất cả.

            return await query.ToListAsync();
        }
}