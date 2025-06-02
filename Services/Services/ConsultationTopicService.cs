using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ConsultationTopicService : IConsultationTopicService
{
    private readonly ConsultationTopicRepository _consultationTopicRepository;
    
    public ConsultationTopicService(ConsultationTopicRepository consultationTopicRepository) => _consultationTopicRepository = consultationTopicRepository;
    
    public async Task<IEnumerable<consultation_topic>> GetAllAsync()
    {
        return await _consultationTopicRepository.GetAllAsync();
    }

    public async Task<consultation_topic> GetByIdAsync(int id)
    {
        return await _consultationTopicRepository.GetByIdAsync(id);
    }

    public async Task<ConsultationTopicDto> AddAsync(CreateConsultationTopicDto createConsultationTopicDto)
    {
        var topicEntity = new consultation_topic
        {
            consultation_topic_name = createConsultationTopicDto.ConsultationTopicName
        };

        var addedTopic = await _consultationTopicRepository.AddAsync(topicEntity);
        return MapToConsultationTopicDto(addedTopic);
    }

    // UPDATE Consultation Topic
    public async Task UpdateAsync(UpdateConsultationTopicDto updateConsultationTopicDto)
    {
        var existingTopic = await _consultationTopicRepository.GetByIdAsync(updateConsultationTopicDto.ConsultationTopicId);

        if (existingTopic == null)
        {
            throw new KeyNotFoundException($"Consultation Topic with ID {updateConsultationTopicDto.ConsultationTopicId} not found.");
        }

        // Cập nhật tên nếu có giá trị được cung cấp (chỉ cập nhật nếu không null)
        if (!string.IsNullOrEmpty(updateConsultationTopicDto.ConsultationTopicName))
        {
            existingTopic.consultation_topic_name = updateConsultationTopicDto.ConsultationTopicName;
        }
        // Nếu bạn muốn cho phép gán null cho tên topic (nếu DB cho phép), bạn có thể thêm:
        // else if (updateConsultationTopicDto.ConsultationTopicName == null)
        // {
        //     existingTopic.consultation_topic_name = null;
        // }

        await _consultationTopicRepository.UpdateAsync(existingTopic);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _consultationTopicRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null)
    {
        return await _consultationTopicRepository.SearchConsultationTopicsAsync(topicName);
    }
    
    private ConsultationTopicDto MapToConsultationTopicDto(consultation_topic model)
    {
        return new ConsultationTopicDto
        {
            ConsultationTopicId = model.consultation_topic_id,
            ConsultationTopicName = model.consultation_topic_name
        };
    }
}