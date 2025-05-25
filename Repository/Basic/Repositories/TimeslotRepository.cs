using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class TimeslotRepository : GenericRepository<timeslot>
{
    public TimeslotRepository()
    {
    }
    
    public TimeslotRepository(AppDbContext context) => _context = context;

    public async Task<List<timeslot>> GetAllAsync()
    {
        var items = await _context.timeslots.Include(c => c.class_sessions).ToListAsync();
        return items ?? new List<timeslot>();
    }

    public async Task<timeslot> GetByIdAsync(int id)
    {
        var item = await _context.timeslots.Include(c => c.class_sessions).FirstOrDefaultAsync(c => c.timeslot_id == id);
        return item ?? new timeslot();
    }

    public async Task<int> AddAsync(timeslot timeslot)
    {
        await _context.timeslots.AddAsync(timeslot);
        return await _context.SaveChangesAsync();
    }
}