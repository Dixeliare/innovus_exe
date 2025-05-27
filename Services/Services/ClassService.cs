using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ClassService : IClassService
{
    private readonly ClassRepository _classRepository;
    
    public ClassService (ClassRepository classRepository) => _classRepository = classRepository;
    
    public async Task<List<_class>> GetAll()
    {
        return await _classRepository.GetAll();
    }

    public async Task<_class> GetById(int id)
    {
        return await _classRepository.GetById(id);
    }

    public async Task<int> CreateAsync(_class entity)
    {
        return await _classRepository.CreateAsync(entity);
    }

    public async Task<int> UpdateAsync(_class entity)
    {
        return await _classRepository.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        return await _classRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        return await _classRepository.SearchClassesAsync(instrumentId, classCode);
    }
}