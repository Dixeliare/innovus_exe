using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ClassSessionService : IClassSessionService
{
    // private readonly IClassSessionRepository _classSessionRepository;
    // private readonly IWeekRepository _weekRepository; // Inject cho kiểm tra khóa ngoại
    // private readonly IClassRepository _classRepository; // Inject cho kiểm tra khóa ngoại
    // private readonly ITimeslotRepository _timeSlotRepository; // Inject cho kiểm tra khóa ngoại
    //
    // public ClassSessionService(IClassSessionRepository classSessionRepository,
    //     IWeekRepository weekRepository,
    //     IClassRepository classRepository,
    //     ITimeslotRepository timeSlotRepository)
    // {
    //     _classSessionRepository = classSessionRepository;
    //     _weekRepository = weekRepository;
    //     _classRepository = classRepository;
    //     _timeSlotRepository = timeSlotRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork;

    public ClassSessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<class_session>> GetAll()
    {
        return await  _unitOfWork.ClassSessions.GetAllAsync();
    }

    public async Task<ClassSessionDto?> GetByIdAsync(int id)
    {
        var classSession = await _unitOfWork.ClassSessions.GetByIdAsync(id);
        return classSession != null ? MapToClassSessionDto(classSession) : null;
    }

    public async Task<ClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto)
        {
            // Kiểm tra sự tồn tại của các khóa ngoại
            var weekExists = await _unitOfWork.Weeks.GetByIdAsync(createClassSessionDto.WeekId);
            if (weekExists == null)
            {
                throw new KeyNotFoundException($"Week with ID {createClassSessionDto.WeekId} not found.");
            }

            var classExists = await _unitOfWork.Classes.GetByIdAsync(createClassSessionDto.ClassId);
            if (classExists == null)
            {
                throw new KeyNotFoundException($"Class with ID {createClassSessionDto.ClassId} not found.");
            }

            var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(createClassSessionDto.TimeSlotId);
            if (timeSlotExists == null)
            {
                throw new KeyNotFoundException($"Time Slot with ID {createClassSessionDto.TimeSlotId} not found.");
            }

            var classSessionEntity = new class_session
            {
                session_number = createClassSessionDto.SessionNumber,
                date = createClassSessionDto.Date,
                room_code = createClassSessionDto.RoomCode,
                week_id = createClassSessionDto.WeekId,
                class_id = createClassSessionDto.ClassId,
                time_slot_id = createClassSessionDto.TimeSlotId
            };

            var addedClassSession = await _unitOfWork.ClassSessions.AddAsync(classSessionEntity);
            return MapToClassSessionDto(addedClassSession);
        }

        // UPDATE Class Session
        public async Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto)
        {
            var existingClassSession = await _unitOfWork.ClassSessions.GetByIdAsync(updateClassSessionDto.ClassSessionId);

            if (existingClassSession == null)
            {
                throw new KeyNotFoundException($"Class Session with ID {updateClassSessionDto.ClassSessionId} not found.");
            }

            // Kiểm tra sự tồn tại của các khóa ngoại (nếu chúng được cập nhật)
            if (updateClassSessionDto.WeekId.HasValue && updateClassSessionDto.WeekId != existingClassSession.week_id)
            {
                var weekExists = await _unitOfWork.Weeks.GetByIdAsync(updateClassSessionDto.WeekId.Value);
                if (weekExists == null)
                {
                    throw new KeyNotFoundException($"Week with ID {updateClassSessionDto.WeekId} not found for update.");
                }
                existingClassSession.week_id = updateClassSessionDto.WeekId.Value;
            }

            if (updateClassSessionDto.ClassId.HasValue && updateClassSessionDto.ClassId != existingClassSession.class_id)
            {
                var classExists = await _unitOfWork.Classes.GetByIdAsync(updateClassSessionDto.ClassId.Value);
                if (classExists == null)
                {
                    throw new KeyNotFoundException($"Class with ID {updateClassSessionDto.ClassId} not found for update.");
                }
                existingClassSession.class_id = updateClassSessionDto.ClassId.Value;
            }

            if (updateClassSessionDto.TimeSlotId.HasValue && updateClassSessionDto.TimeSlotId != existingClassSession.time_slot_id)
            {
                var timeSlotExists = await _unitOfWork.Timeslots.GetByIdAsync(updateClassSessionDto.TimeSlotId.Value);
                if (timeSlotExists == null)
                {
                    throw new KeyNotFoundException($"Time Slot with ID {updateClassSessionDto.TimeSlotId} not found for update.");
                }
                existingClassSession.time_slot_id = updateClassSessionDto.TimeSlotId.Value;
            }

            // Cập nhật các trường khác nếu có giá trị
            if (updateClassSessionDto.SessionNumber.HasValue)
            {
                existingClassSession.session_number = updateClassSessionDto.SessionNumber.Value;
            }
            if (updateClassSessionDto.Date.HasValue)
            {
                existingClassSession.date = updateClassSessionDto.Date.Value;
            }
            if (!string.IsNullOrEmpty(updateClassSessionDto.RoomCode))
            {
                existingClassSession.room_code = updateClassSessionDto.RoomCode;
            }

            await _unitOfWork.ClassSessions.UpdateAsync(existingClassSession);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _unitOfWork.ClassSessions.DeleteAsync(id);
    }

    public async Task<IEnumerable<class_session>> SearchClassSessionsAsync(DateOnly? date = null, string? roomCode = null, int? weekId = null, int? classId = null,
        int? timeSlotId = null)
    {
        return await _unitOfWork.ClassSessions.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
    }
    
    private ClassSessionDto MapToClassSessionDto(class_session model)
    {
        return new ClassSessionDto
        {
            ClassSessionId = model.class_session_id,
            SessionNumber = model.session_number,
            Date = model.date,
            RoomCode = model.room_code,
            WeekId = model.week_id,
            ClassId = model.class_id,
            TimeSlotId = model.time_slot_id
            // Nếu bạn có DTO lồng nhau, bạn sẽ map ở đây:
            // Class = model._class != null ? new ClassDto { ClassId = model._class.class_id, Name = model._class.name } : null
            // ...
        };
    }
}