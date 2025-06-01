using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IConsultationRequestService
{
    Task<IEnumerable<consultation_request>> GetAllAsync();
    Task<consultation_request> GetByIdAsync(int id);
    Task<ConsultationRequestDto> AddAsync(CreateConsultationRequestDto createConsultationRequestDto);
    Task UpdateAsync(UpdateConsultationRequestDto updateConsultationRequestDto);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null);
}