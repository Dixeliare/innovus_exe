using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IConsultationRequestService
{
    Task<IEnumerable<ConsultationRequestDto>> GetAllAsync();
    Task<ConsultationRequestDto> GetByIdAsync(int id);
    Task<ConsultationRequestDto> AddAsync(CreateConsultationRequestDto createConsultationRequestDto);
    Task UpdateAsync(UpdateConsultationRequestDto updateConsultationRequestDto, int? currentUserId);
    Task DeleteAsync(int id);

    Task<IEnumerable<ConsultationRequestDto>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null);
    
    Task UpdateConsultationRequestContactStatusAsync(UpdateConsultationRequestContactStatusDto dto, int? currentUserId);
}