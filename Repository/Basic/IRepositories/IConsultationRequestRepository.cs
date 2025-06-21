using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IConsultationRequestRepository
{
    Task<IEnumerable<consultation_request>> GetAllAsync();
    Task<consultation_request> GetByIdAsync(int id);
    Task<consultation_request> AddAsync(consultation_request entity);
    Task UpdateAsync(consultation_request entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null);
}