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

    public async Task<int> CreateAsync(consultation_topic consultation_topic)
    {
        return await _consultationTopicRepository.CreateAsync(consultation_topic);
    }

    public async Task<int> UpdateAsync(consultation_topic consultation_topic)
    {
        return await _consultationTopicRepository.UpdateAsync(consultation_topic);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _consultationTopicRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null)
    {
        return await _consultationTopicRepository.SearchConsultationTopicsAsync(topicName);
    }
}