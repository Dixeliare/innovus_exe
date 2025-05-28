using Repository.Models;

namespace Services.IServices;

public interface IConsultationTopicService
{
    Task<IEnumerable<consultation_topic>> GetAllAsync();
    Task<consultation_topic> GetByIdAsync(int id);
    Task<int> CreateAsync(consultation_topic consultation_topic);
    Task<int> UpdateAsync(consultation_topic consultation_topic);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null);
}