using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class UserRepository : GenericRepository<user>, IUserRepository
{
    // KHÔNG CẦN _context NỮA VÌ ĐÃ SỬ DỤNG _dbSet TỪ BASE CLASS
    public UserRepository(AppDbContext context) : base(context)
    {
        // _context = context; // ĐÃ XÓA DÒNG NÀY
    }

    public async Task<IEnumerable<user>> GetAllAsync()
    {
        return await _dbSet
            .Include(g => g.gender)
            .Include(a => a.attendances)
            .Include(r => r.role)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
                .ThenInclude(cls => cls.instrument) // Tải nhạc cụ của Class
            .Include(c => c.classes)
                .ThenInclude(cls => cls.class_sessions) // Tải class_sessions của Class
                    .ThenInclude(cs => cs.day) // Tải Day của ClassSession
                        .ThenInclude(d => d.week) // **ĐÃ SỬA:** Tải Week của Day
            .Include(c => c.classes)
                .ThenInclude(cls => cls.class_sessions)
                    .ThenInclude(cs => cs.time_slot) // Tải TimeSlot của ClassSession
            .Include(d => d.documents)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<user?> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking()
            .Include(u => u.gender) 
            .Include(a => a.attendances)
            .Include(r => r.role)
            .Include(s => s.statistic)
            .Include(u => u.user_favorite_sheets)
            .Include(c => c.classes)
                .ThenInclude(cls => cls.instrument) // Tải nhạc cụ của Class
            .Include(c => c.classes)
                .ThenInclude(cls => cls.class_sessions) // Tải class_sessions của Class
                    .ThenInclude(cs => cs.day) // Tải Day của ClassSession
                        .ThenInclude(d => d.week) // **ĐÃ SỬA:** Tải Week của Day
            .Include(c => c.classes)
                .ThenInclude(cls => cls.class_sessions)
                    .ThenInclude(cs => cs.time_slot) // Tải TimeSlot của ClassSession
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

    public async Task<IEnumerable<user>> SearchUsersAsync(
        string? username = null,
        string? accountName = null,
        string? password = null, 
        string? address = null,
        string? phoneNumber = null,
        bool? isDisabled = null,
        DateTime? createAt = null,
        DateOnly? birthday = null,
        int? roleId = null,
        string? email = null,
        int? genderId = null)
    {
        IQueryable<user> query = _dbSet;

        query = query
            .Include(u => u.role)
            .Include(u => u.statistic)
            .Include(u => u.gender) 
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
        
        if (!string.IsNullOrEmpty(email))
        {
            var lowerEmail = email.ToLower();
            predicates.Add(u => u.email != null && u.email.ToLower().Contains(lowerEmail));
        }

        if (genderId.HasValue)
        {
            predicates.Add(u => u.gender_id == genderId.Value);
        }


        if (predicates.Any())
        {
            Expression<Func<user, bool>> combinedPredicate = predicates.First();
            for (int i = 1; i < predicates.Count; i++)
            {
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
            .Include(u => u.role) 
            .FirstOrDefaultAsync(u => u.user_id == userId);
    }
    
    public async Task<IEnumerable<user>> GetUsersByRoleIdsAsync(List<int> roleIds)
    {
        return await _dbSet
            .Include(u => u.role)
            .Include(u => u.gender)
            .Where(u => u.role_id.HasValue && roleIds.Contains(u.role_id.Value))
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<user>> GetUsersByRoleNamesAsync(List<string> roleNames)
    {
        // PHƯƠNG THỨC NÀY CẦN TRUY CẬP TRỰC TIẾP VÀO _context ĐỂ LẤY DBSET CỦA ROLE.
        // NẾU BẠN CHỈ MUỐN SỬ DỤNG _dbSet CỦA USER VÀ KHÔNG MUỐN CÓ _context TRONG REPOSITORY,
        // THÌ PHƯƠNG THỨC NÀY CẦN ĐƯỢC CHUYỂN HOẶC CƠ CHẾ KHÁC ĐỂ TRUY VẤN ROLE.
        // HÌNH NHƯ CÁC REPOSITORY CỦA BẠN ĐỀU CÓ THỂ TRUY CẬP AppDbContext THÔNG QUA BASE CLASS, VẬY NÊN DÒNG DƯỚI NÊN LÀ:
        // var roleIds = await _context.roles (ĐÚNG NHƯ BẠN ĐANG DÙNG TRONG CODE GỐC)
        // HOẶC NẾU CHỈ DÙNG _dbSet, THÌ CÓ THỂ CẦN MỘT REPOSITORY KHÁC CHO ROLE.
        
        // GIẢ ĐỊNH `_context` VẪN CÓ THỂ TRUY CẬP ĐƯỢC TỪ BASE CLASS HOẶC TRUYỀN VÀO NHƯ TRƯỚC.
        // NẾU AppDbContext CÓ THỂ TRUY CẬP TỪ BASE CLASS GenericRepository thông qua một property như `Context`,
        // THÌ CÓ THỂ SỬ DỤNG `Context.roles`.
        // TẠM THỜI GIỮ NGUYÊN DÒNG NÀY VÌ NÓ CÓ TRONG CODE GỐC CỦA BẠN VÀ CẦN _context ĐỂ LẤY `roles` DBSet.
        // NẾU BẠN MUỐN LOẠI BỎ HOÀN TOÀN `_context` KHỎI ĐÂY, PHƯƠNG THỨC NÀY CẦN ĐƯỢC CƠ CẤU LẠI HOẶC CHUYỂN ĐI.
        var roleIds = await ((AppDbContext)_context).roles // SỬ DỤNG _context CỦA BASE CLASS, ÉP KIỂU VỀ AppDbContext
            .Where(r => roleNames.Contains(r.role_name))
            .Select(r => r.role_id)
            .ToListAsync();

        if (!roleIds.Any())
        {
            return new List<user>();
        }

        return await _dbSet
            .Include(u => u.role)
            .Include(u => u.gender)
            .Where(u => u.role_id.HasValue && roleIds.Contains(u.role_id.Value))
            .AsSplitQuery()
            .ToListAsync();
    }
    
    public async Task<user?> GetUserWithClassesAndRoleAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.role)
            .Include(u => u.gender) 
            .Include(u => u.classes) 
            .ThenInclude(c => c.instrument) 
            .Include(u => u.classes)
            .ThenInclude(c => c.class_sessions) 
            .ThenInclude(cs => cs.day) // Từ class_session đến day
            .ThenInclude(d => d.week) // Từ day đến week
            .Include(u => u.classes)
            .ThenInclude(c => c.class_sessions)
            .ThenInclude(cs => cs.time_slot) 
            .FirstOrDefaultAsync(u => u.user_id == userId);
    }
    
    public async Task<user?> GetUserByIdWithClassesAndRoleAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.classes) // Include user's classes (Many-to-Many)
            .Include(u => u.role)    // Include user's role
            .Include(u => u.gender)  // Include user's gender
            .FirstOrDefaultAsync(u => u.user_id == userId);
    }
}