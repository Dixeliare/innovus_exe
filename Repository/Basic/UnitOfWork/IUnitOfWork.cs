using Repository.Basic.IRepositories;
using Repository.Data;

namespace Repository.Basic.UnitOfWork;

public interface IUnitOfWork: IDisposable
{
    
    AppDbContext Context { get; }
    IAttendanceRepository Attendances { get; }
    IClassRepository Classes { get; }
    IClassSessionRepository ClassSessions { get; }
    IConsultationRequestRepository ConsultationRequests { get; }
    IConsultationTopicRepository ConsultationTopics { get; }
    IDocumentRepository Documents { get; }
    IGenreRepository Genres { get; }
    IInstrumentRepository Instruments { get; }
    IOpeningScheduleRepository OpeningSchedules { get; }
    IRoleRepository Roles { get; }
    IScheduleRepository Schedules { get; }
    ISheetMusicRepository SheetMusics { get; }
    ISheetRepository Sheets { get; }
    IStatisticRepository Statistics { get; }
    ITimeslotRepository Timeslots { get; }
    IUserFavoriteSheetRepository UserFavoriteSheets { get; }
    IUserRepository Users { get; }
    IWeekRepository Weeks { get; }
    IGenderRepository Genders { get; }
    IDayRepository Days { get; }
    IRoomRepository Rooms { get; }
    IDayOfWeekLookupRepository DayOfWeekLookups { get; }
    IAttendanceStatusRepository AttendanceStatuses { get; }
    
    Task<int> CompleteAsync();
}