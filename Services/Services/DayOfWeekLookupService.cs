using DTOs;
using Repository.Basic.UnitOfWork;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class DayOfWeekLookupService : IDayOfWeekLookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public DayOfWeekLookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DayOfWeekLookupDto>> GetAllDayOfWeekLookupsAsync()
    {
        var dayOfWeekLookups = await _unitOfWork.DayOfWeekLookups.GetAllAsync();
        return dayOfWeekLookups.Select(d => new DayOfWeekLookupDto
        {
            DayOfWeekId = d.day_of_week_id,
            DayName = d.day_name,
            DayNumber = d.day_number
        }).ToList();
    }

    public async Task<DayOfWeekLookupDto> GetDayOfWeekLookupByIdAsync(int id)
    {
        var dayOfWeekLookup = await _unitOfWork.DayOfWeekLookups.GetByIdAsync(id);
        if (dayOfWeekLookup == null)
        {
            throw new NotFoundException("DayOfWeekLookup", "Id", id);
        }

        return new DayOfWeekLookupDto
        {
            DayOfWeekId = dayOfWeekLookup.day_of_week_id,
            DayName = dayOfWeekLookup.day_name,
            DayNumber = dayOfWeekLookup.day_number
        };
    }
}