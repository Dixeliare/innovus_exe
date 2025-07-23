using Repository.Basic.IRepositories;

namespace Repository.Basic.UnitOfWork;

public interface IUnitOfWork: IDisposable
{
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
    
    Task<int> CompleteAsync();
}