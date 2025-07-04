using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IConsultationTopicService
{
    Task<IEnumerable<ConsultationTopicDto>> GetAllAsync();
    Task<ConsultationTopicDto> GetByIdAsync(int id);
    Task<ConsultationTopicDto> AddAsync(CreateConsultationTopicDto createConsultationTopicDto);
    Task UpdateAsync(UpdateConsultationTopicDto updateConsultationTopicDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ConsultationTopicDto>> SearchConsultationTopicsAsync(string? topicName = null);
}