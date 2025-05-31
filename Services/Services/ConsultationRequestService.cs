using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ConsultationRequestService : IConsultationRequestService
{
    private readonly ConsultationRequestRepository _consultationRequestRepository;
    
    public ConsultationRequestService(ConsultationRequestRepository consultationRequestRepository) => _consultationRequestRepository = consultationRequestRepository;
    
    public async Task<IEnumerable<consultation_request>> GetAllAsync()
    {
        return await _consultationRequestRepository.GetAllAsync();
    }

    public async Task<consultation_request> GetByIdAsync(int id)
    {
        return await _consultationRequestRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(consultation_request entity)
    {
        return await _consultationRequestRepository.CreateAsync(entity);
    }

    public async Task<int> UpdateAsync(consultation_request entity)
    {
        return await _consultationRequestRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _consultationRequestRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(string? fullname = null, string? contactNumber = null, string? email = null,
        string? note = null, bool? hasContact = null)
    {
        return await _consultationRequestRepository.SearchConsultationRequestsAsync(fullname , contactNumber, email, note, hasContact);
    }
}