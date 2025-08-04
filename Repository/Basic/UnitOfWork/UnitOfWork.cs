using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Data;

namespace Repository.Basic.UnitOfWork;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    
    
    public AppDbContext Context => _context; 
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
    public IDayRepository Days { get; }
    public IRoomRepository Rooms { get; }
    public IDayOfWeekLookupRepository DayOfWeekLookups { get; }
    public IAttendanceStatusRepository AttendanceStatuses { get; }

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
        Days = new DayRepository(_context);
        Rooms = new RoomRepository(_context);
        DayOfWeekLookups = new DayOfWeekLookupRepository(_context);
        AttendanceStatuses = new AttendanceStatusRepository(_context);
    }


    public async Task<int> CompleteAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx) // Bắt lỗi DbUpdateException cụ thể
        {
            Console.WriteLine("--- DbUpdateException in CompleteAsync ---");
            Console.WriteLine($"Message: {dbEx.Message}");
            if (dbEx.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                // Log chi tiết hơn nếu InnerException là PostgresException
                if (dbEx.InnerException is Npgsql.PostgresException pgEx)
                {
                    Console.WriteLine($"Postgres Error Code: {pgEx.SqlState}");
                    Console.WriteLine($"Postgres Detail: {pgEx.Detail}");
                    Console.WriteLine($"Postgres Hint: {pgEx.Hint}");
                }
            }
            //_logger?.LogError(dbEx, "Error saving changes in UnitOfWork.CompleteAsync."); // Sử dụng logger nếu có
            Console.WriteLine("-------------------------------------");
            throw; // Re-throw để lỗi được lan truyền lên service và API
        }
        catch (Exception ex) // Bắt các lỗi tổng quát khác
        {
            Console.WriteLine("--- General Exception in CompleteAsync ---");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            //_logger?.LogError(ex, "Unexpected error in UnitOfWork.CompleteAsync."); // Sử dụng logger nếu có
            Console.WriteLine("-------------------------------------");
            throw; // Re-throw
        }
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}