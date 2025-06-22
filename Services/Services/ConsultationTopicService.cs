using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ConsultationTopicService : IConsultationTopicService
{
    // private readonly IConsultationTopicRepository _consultationTopicRepository;
    //
    // public ConsultationTopicService(IConsultationTopicRepository consultationTopicRepository) => _consultationTopicRepository = consultationTopicRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public ConsultationTopicService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<consultation_topic>> GetAllAsync()
    {
        return await _unitOfWork.ConsultationTopics.GetAllAsync();
    }

    public async Task<consultation_topic> GetByIdAsync(int id)
    {
        return await _unitOfWork.ConsultationTopics.GetByIdAsync(id);
    }

    public async Task<ConsultationTopicDto> AddAsync(CreateConsultationTopicDto createConsultationTopicDto)
    {
        var topicEntity = new consultation_topic
        {
            consultation_topic_name = createConsultationTopicDto.ConsultationTopicName
        };

        var addedTopic = await _unitOfWork.ConsultationTopics.AddAsync(topicEntity);
        return MapToConsultationTopicDto(addedTopic);
    }

    // UPDATE Consultation Topic
    public async Task UpdateAsync(UpdateConsultationTopicDto updateConsultationTopicDto)
    {
        var existingTopic = await _unitOfWork.ConsultationTopics.GetByIdAsync(updateConsultationTopicDto.ConsultationTopicId);

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

        await _unitOfWork.ConsultationTopics.UpdateAsync(existingTopic);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _unitOfWork.ConsultationTopics.DeleteAsync(id);
    }

    public async Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null)
    {
        return await _unitOfWork.ConsultationTopics.SearchConsultationTopicsAsync(topicName);
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