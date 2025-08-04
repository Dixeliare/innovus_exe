using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository.Basic.Repositories
{
    public class AttendanceStatusRepository : GenericRepository<attendance_status>, IAttendanceStatusRepository
    {
        public AttendanceStatusRepository(AppDbContext context) : base(context)
        {
        
        }

        public Task<gender> AddAsync(gender entity)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<gender> entities)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(attendance_status entity)
        {
            _context.attendance_statuses.Update(entity);
            await _context.SaveChangesAsync();
        }

        public void Remove(gender entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<gender> entities)
        {
            throw new NotImplementedException();
        }

        public Task<gender?> FindOneAsync(Expression<Func<gender, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<gender, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<attendance_status>> GetAllAsync()
        {
            return await _context.attendance_statuses.ToListAsync();
        }
        public async Task<attendance_status?> GetByIdAsync(int id)
        {
            return await _context.attendance_statuses.FirstOrDefaultAsync(s => s.status_id == id);
        }
    }
} 