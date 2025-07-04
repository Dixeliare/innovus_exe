using DTOs;

namespace Services.IServices;

public interface IStatisticService
{
    Task<IEnumerable<StatisticDto>> GetAllAsync();
    Task<StatisticDto> GetByIdAsync(int id);
    Task<StatisticDto> AddAsync(CreateStatisticDto createStatisticDto);
    Task UpdateAsync(UpdateStatisticDto updateStatisticDto);
    Task DeleteAsync(int id);
}