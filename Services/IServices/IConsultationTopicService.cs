using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IConsultationTopicService
{
    Task<IEnumerable<consultation_topic>> GetAllAsync();
    Task<consultation_topic> GetByIdAsync(int id);
    Task<ConsultationTopicDto> AddAsync(CreateConsultationTopicDto createConsultationTopicDto);
    Task UpdateAsync(UpdateConsultationTopicDto updateConsultationTopicDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null);
}