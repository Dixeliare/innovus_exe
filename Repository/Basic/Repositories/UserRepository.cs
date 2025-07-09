using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class UserRepository : GenericRepository<user>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<user>> GetAllAsync()
    {
        return await _dbSet
            .Include(g => g.gender)
            .Include(a => a.attendances)
            .Include(o => o.opening_schedule)
            .Include(r => r.role)
            .Include(s => s.schedule)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
            .Include(d => d.documents)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<user> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking()
            .Include(u => u.gender) 
            .Include(a => a.attendances)
            .Include(o => o.opening_schedule)
            .Include(r => r.role)
            .Include(s => s.schedule)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
            .Include(d => d.documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.user_id == id);
    }

    public async Task<user?> GetByUsernameAsync(string username)
    {
        return await _dbSet.AsNoTracking()
            .Include(u => u.gender) 
            .Include(r => r.role)
            .FirstOrDefaultAsync(u => u.username == username);
    }
    
    public async Task<user?> FindByEmailAsync(string email)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.email == email);
    }

    // public async Task<user> AddAsync(user entity)
    // {
    //     _context.users.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(user entity)
    // {
    //     _context.Entry(entity).State = EntityState.Modified;
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.users.FindAsync(id);
    //     if (item == null) return false;
    //     _context.users.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }

    public async Task<IEnumerable<user>> SearchUsersAsync(
        string? username = null,
        string? accountName = null,
        string? password = null, // Giữ nguyên ở đây, nhưng logic bên dưới sẽ không dùng
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null,
        string? email = null, // Thêm email
        int? genderId = null)
    {
        IQueryable<user> query = _dbSet;

        query = query.Include(u => u.role)
            .Include(u => u.statistic)
            .Include(u => u.opening_schedule)
            .Include(u => u.schedule)
            .Include(u => u.gender) // THÊM DÒNG NÀY
            .AsSplitQuery();

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
            predicates.Add(u => u.create_at != null && u.create_at.Value.Date == createAt.Value.Date);
        }

        if (birthday.HasValue)
        {
            predicates.Add(u => u.birthday == birthday.Value);
        }

        if (roleId.HasValue)
        {
            predicates.Add(u => u.role_id == roleId.Value);
        }
        
        // THÊM ĐIỀU KIỆN TÌM KIẾM THEO EMAIL
        if (!string.IsNullOrEmpty(email))
        {
            var lowerEmail = email.ToLower();
            predicates.Add(u => u.email != null && u.email.ToLower().Contains(lowerEmail));
        }

        // THÊM ĐIỀU KIỆN TÌM KIẾM THEO GENDER ID
        if (genderId.HasValue)
        {
            predicates.Add(u => u.gender_id == genderId.Value);
        }


        if (predicates.Any())
        {
            Expression<Func<user, bool>> combinedPredicate = predicates.First();
            for (int i = 1; i < predicates.Count; i++)
            {
                // Sử dụng AndAlso cho tìm kiếm kết hợp (AND)
                combinedPredicate = Expression.Lambda<Func<user, bool>>(
                    Expression.AndAlso(combinedPredicate.Body, predicates[i].Body),
                    combinedPredicate.Parameters);
            }

            query = query.Where(combinedPredicate);
        }

        return await query.ToListAsync();
    }

    public async Task<user?> GetUserWithRoleAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.gender) 
            .Include(u => u.role) // <--- Đảm bảo tải thông tin Role
            .FirstOrDefaultAsync(u => u.user_id == userId);
    }
    
    // THÊM CÁC PHƯƠNG THỨC NÀY:
    public async Task<IEnumerable<user>> GetUsersByRoleIdsAsync(List<int> roleIds)
    {
        return await _dbSet
            .Include(u => u.role)
            .Include(u => u.gender)// Bao gồm thông tin vai trò
            .Where(u => u.role_id.HasValue && roleIds.Contains(u.role_id.Value))
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<user>> GetUsersByRoleNamesAsync(List<string> roleNames)
    {
        // Lấy các Role ID từ tên Role
        var roleIds = await _context.roles // _context là AppDbContext, có thể truy cập DbSet<role>
            .Where(r => roleNames.Contains(r.role_name))
            .Select(r => r.role_id)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<user>(); // Không tìm thấy vai trò nào, trả về danh sách trống
        }

        return await _dbSet
            .Include(u => u.role)
            .Include(u => u.gender)// Bao gồm thông tin vai trò
            .Where(u => u.role_id.HasValue && roleIds.Contains(u.role_id.Value))
            .AsSplitQuery()
            .ToListAsync();
    }
}