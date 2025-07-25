using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IClassSessionRepository: IGenericRepository<class_session>
{
    // Eager load related entities for full DTO mapping
    Task<IEnumerable<class_session>> GetAllClassSessionsWithDetailsAsync();
    Task<class_session?> GetClassSessionByIdWithDetailsAsync(int id);
    Task<IEnumerable<class_session>> GetClassSessionsByClassIdWithDetailsAsync(int classId);
    Task<IEnumerable<class_session>> GetClassSessionsByDayIdWithDetailsAsync(int dayId);

    Task<IEnumerable<class_session>> SearchClassSessionsWithDetailsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        int? roomId = null, // ĐÃ SỬA: Thay đổi từ string? roomCode sang int? roomId
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null
    );

    // Search method without eager loading (for internal use, like uniqueness checks)
    Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        int? roomId = null, // ĐÃ SỬA: Thay đổi từ string? roomCode sang int? roomId
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null
    );
}