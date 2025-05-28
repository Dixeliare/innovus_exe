using Repository.Models;

namespace Services.IServices;

public interface IConsultationRequestService
{
    Task<IEnumerable<consultation_request>> GetAllAsync();
    Task<consultation_request> GetByIdAsync(int id);
    Task<int> CreateAsync(consultation_request entity);
    Task<int> UpdateAsync(consultation_request entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null);
}