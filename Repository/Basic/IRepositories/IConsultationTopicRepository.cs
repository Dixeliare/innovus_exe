using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IConsultationTopicRepository
{
    Task<IEnumerable<consultation_topic>> GetAllAsync();
    Task<consultation_topic> GetByIdAsync(int id);
    Task<consultation_topic> AddAsync(consultation_topic entity);
    Task UpdateAsync(consultation_topic entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null);
}