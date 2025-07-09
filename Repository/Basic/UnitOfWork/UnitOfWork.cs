using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Data;

namespace Repository.Basic.UnitOfWork;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public IAttendanceRepository Attendances { get; }
    public IClassRepository Classes { get; }
    public IClassSessionRepository ClassSessions { get; }
    public IConsultationRequestRepository ConsultationRequests { get; }
    public IConsultationTopicRepository ConsultationTopics { get; }
    public IDocumentRepository Documents { get; }
    public IGenreRepository Genres { get; }
    public IInstrumentRepository Instruments { get; }
    public IOpeningScheduleRepository OpeningSchedules { get; }
    public IRoleRepository Roles { get; }
    public IScheduleRepository Schedules { get; }
    public ISheetMusicRepository SheetMusics { get; }
    public ISheetRepository Sheets { get; }
    public IStatisticRepository Statistics { get; }
    public ITimeslotRepository Timeslots { get; }
    public IUserFavoriteSheetRepository UserFavoriteSheets { get; }
    public IUserRepository Users { get; }
    public IWeekRepository Weeks { get; }
    public IGenderRepository Genders { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Attendances = new AttendanceRepository(_context);
        Classes = new ClassRepository(_context);
        ClassSessions = new ClassSessionRepository(_context);
        ConsultationRequests = new ConsultationRequestRepository(_context);
        ConsultationTopics = new ConsultationTopicRepository(_context);
        Documents = new DocumentRepository(_context);
        Genres = new GenreRepository(_context); 
        Instruments = new InstrumentRepository(_context);
        OpeningSchedules = new OpeningScheduleRepository(_context);
        Roles = new RoleRepository(_context);
        Schedules = new ScheduleRepository(_context);
        SheetMusics = new SheetMusicRepository(_context);
        Sheets = new SheetRepository(_context);
        Statistics = new StatisticRepository(_context);
        Timeslots = new TimeslotRepository(_context);
        UserFavoriteSheets = new UserFavoriteSheetRepository(_context);
        Users = new UserRepository(_context);
        Weeks = new WeekRepository(_context);
        Genders = new GenderRepository(_context);
    }


    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}