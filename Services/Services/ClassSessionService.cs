using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ClassSessionService : IClassSessionService
{
    private readonly ClassSessionRepository _classSessionRepository;
    private readonly WeekRepository _weekRepository; // Inject cho kiểm tra khóa ngoại
    private readonly ClassRepository _classRepository; // Inject cho kiểm tra khóa ngoại
    private readonly TimeslotRepository _timeSlotRepository; // Inject cho kiểm tra khóa ngoại

    public ClassSessionService(ClassSessionRepository classSessionRepository,
        WeekRepository weekRepository,
        ClassRepository classRepository,
        TimeslotRepository timeSlotRepository)
    {
        _classSessionRepository = classSessionRepository;
        _weekRepository = weekRepository;
        _classRepository = classRepository;
        _timeSlotRepository = timeSlotRepository;
    }
    
    public async Task<IEnumerable<class_session>> GetAll()
    {
        return await  _classSessionRepository.GetAllAsync();
    }

    public async Task<ClassSessionDto?> GetByIdAsync(int id)
    {
        var classSession = await _classSessionRepository.GetByIdAsync(id);
        return classSession != null ? MapToClassSessionDto(classSession) : null;
    }

    public async Task<ClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto)
        {
            // Kiểm tra sự tồn tại của các khóa ngoại
            var weekExists = await _weekRepository.GetByIdAsync(createClassSessionDto.WeekId);
            if (weekExists == null)
            {
                throw new KeyNotFoundException($"Week with ID {createClassSessionDto.WeekId} not found.");
            }

            var classExists = await _classRepository.GetByIdAsync(createClassSessionDto.ClassId);
            if (classExists == null)
            {
                throw new KeyNotFoundException($"Class with ID {createClassSessionDto.ClassId} not found.");
            }

            var timeSlotExists = await _timeSlotRepository.GetByIdAsync(createClassSessionDto.TimeSlotId);
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

            var addedClassSession = await _classSessionRepository.AddAsync(classSessionEntity);
            return MapToClassSessionDto(addedClassSession);
        }

        // UPDATE Class Session
        public async Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto)
        {
            var existingClassSession = await _classSessionRepository.GetByIdAsync(updateClassSessionDto.ClassSessionId);

            if (existingClassSession == null)
            {
                throw new KeyNotFoundException($"Class Session with ID {updateClassSessionDto.ClassSessionId} not found.");
            }

            // Kiểm tra sự tồn tại của các khóa ngoại (nếu chúng được cập nhật)
            if (updateClassSessionDto.WeekId.HasValue && updateClassSessionDto.WeekId != existingClassSession.week_id)
            {
                var weekExists = await _weekRepository.GetByIdAsync(updateClassSessionDto.WeekId.Value);
                if (weekExists == null)
                {
                    throw new KeyNotFoundException($"Week with ID {updateClassSessionDto.WeekId} not found for update.");
                }
                existingClassSession.week_id = updateClassSessionDto.WeekId.Value;
            }

            if (updateClassSessionDto.ClassId.HasValue && updateClassSessionDto.ClassId != existingClassSession.class_id)
            {
                var classExists = await _classRepository.GetByIdAsync(updateClassSessionDto.ClassId.Value);
                if (classExists == null)
                {
                    throw new KeyNotFoundException($"Class with ID {updateClassSessionDto.ClassId} not found for update.");
                }
                existingClassSession.class_id = updateClassSessionDto.ClassId.Value;
            }

            if (updateClassSessionDto.TimeSlotId.HasValue && updateClassSessionDto.TimeSlotId != existingClassSession.time_slot_id)
            {
                var timeSlotExists = await _timeSlotRepository.GetByIdAsync(updateClassSessionDto.TimeSlotId.Value);
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

            await _classSessionRepository.UpdateAsync(existingClassSession);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _classSessionRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<class_session>> SearchClassSessionsAsync(DateOnly? date = null, string? roomCode = null, int? weekId = null, int? classId = null,
        int? timeSlotId = null)
    {
        return await _classSessionRepository.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
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