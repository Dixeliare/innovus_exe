using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class OpeningScheduleService : IOpeningScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassSessionService _classSessionService;
    private readonly ILogger<OpeningScheduleService> _logger;

    public OpeningScheduleService(IUnitOfWork unitOfWork, IClassSessionService classSessionService, ILogger<OpeningScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _classSessionService = classSessionService;
        _logger = logger;
    }

    public async Task<IEnumerable<OpeningScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.OpeningSchedules.GetAllAsync();
        return schedules.Select(MapToOpeningScheduleDto);
    }

    public async Task<OpeningScheduleDto> GetByIdAsync(int id)
    {
        var schedule = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        // Tải các navigation properties cần thiết một cách rõ ràng
        // vì GetByIdAsync và FindOneAsync không có chức năng include.
        if (_unitOfWork.Context != null) // Đảm bảo Context có sẵn
        {
            await _unitOfWork.Context.Entry(schedule).Reference(s => s.teacher_user).LoadAsync();
            if (schedule.teacher_user != null)
            {
                await _unitOfWork.Context.Entry(schedule.teacher_user).Reference(u => u.role).LoadAsync();
            }
            await _unitOfWork.Context.Entry(schedule).Reference(s => s.instrument).LoadAsync();
            await _unitOfWork.Context.Entry(schedule).Collection(s => s.day_of_weeks).LoadAsync();
        }

        return MapToOpeningScheduleDto(schedule);
    }

    public async Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto)
    {
        var existingScheduleWithClassCode =
            await _unitOfWork.OpeningSchedules.FindOneAsync(os =>
                os.class_code == createOpeningScheduleDto.ClassCode);
        if (existingScheduleWithClassCode != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                {
                    "ClassCode",
                    new string[] { $"Mã lớp '{createOpeningScheduleDto.ClassCode}' đã có lịch khai giảng." }
                }
            });
        }

        if (createOpeningScheduleDto.EndDate.HasValue &&
            createOpeningScheduleDto.EndDate.Value < createOpeningScheduleDto.OpeningDay)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndDate", new string[] { "Ngày kết thúc không được trước ngày khai giảng." } }
            });
        }

        user? teacherUser = null;
        if (createOpeningScheduleDto.TeacherUserId.HasValue)
        {
            // Giả sử GetUserWithRoleAsync tồn tại trong IUserRepository
            teacherUser = await _unitOfWork.Users.GetUserWithRoleAsync(createOpeningScheduleDto.TeacherUserId.Value);

            if (teacherUser == null)
            {
                throw new NotFoundException("Teacher User", "Id", createOpeningScheduleDto.TeacherUserId.Value);
            }

            if (teacherUser.role?.role_name?.ToLower() != "teacher")
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "TeacherUserId", new string[] { "Người dùng được chọn không phải là giáo viên." } }
                });
            }
        }

        if (createOpeningScheduleDto.InstrumentId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "InstrumentId", new string[] { "Mã nhạc cụ không hợp lệ." } }
            });
        }

        var existingInstrument = await _unitOfWork.Instruments.GetByIdAsync(createOpeningScheduleDto.InstrumentId);
        if (existingInstrument == null)
        {
            throw new NotFoundException("Instrument", "Id", createOpeningScheduleDto.InstrumentId);
        }
        
        if (createOpeningScheduleDto.DefaultRoomId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "DefaultRoomId", new string[] { "Mã phòng mặc định không hợp lệ." } }
            });
        }
        var existingRoom = await _unitOfWork.Rooms.GetByIdAsync(createOpeningScheduleDto.DefaultRoomId);
        if (existingRoom == null)
        {
            throw new NotFoundException("Room", "Id", createOpeningScheduleDto.DefaultRoomId);
        }

        if (createOpeningScheduleDto.TimeSlotIds == null || !createOpeningScheduleDto.TimeSlotIds.Any())
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "TimeSlotIds", new string[] { "Phải cung cấp ít nhất một khung giờ (TimeSlotId) để tạo buổi học." } }
            });
        }
        foreach (var tsId in createOpeningScheduleDto.TimeSlotIds)
        {
            var existingTimeSlot = await _unitOfWork.Timeslots.GetByIdAsync(tsId);
            if (existingTimeSlot == null)
            {
                throw new NotFoundException("TimeSlot", "Id", tsId);
            }
        }

        var scheduleEntity = new opening_schedule
        {
            class_code = createOpeningScheduleDto.ClassCode,
            opening_day = createOpeningScheduleDto.OpeningDay,
            end_date = createOpeningScheduleDto.EndDate,
            student_quantity = createOpeningScheduleDto.StudentQuantity,
            is_advanced_class = createOpeningScheduleDto.IsAdvancedClass ?? false,
            teacher_user_id = createOpeningScheduleDto.TeacherUserId,
            instrument_id = createOpeningScheduleDto.InstrumentId,
            total_sessions = createOpeningScheduleDto.TotalSessions
        };

        if (createOpeningScheduleDto.SelectedDayOfWeekIds != null && createOpeningScheduleDto.SelectedDayOfWeekIds.Any())
        {
            foreach (var dayId in createOpeningScheduleDto.SelectedDayOfWeekIds)
            {
                var dayOfWeekLookup = await _unitOfWork.DayOfWeekLookups.GetByIdAsync(dayId);
                if (dayOfWeekLookup == null)
                {
                    throw new NotFoundException("DayOfWeekLookup", "Id", dayId);
                }
                scheduleEntity.day_of_weeks.Add(dayOfWeekLookup);
            }
        }

        try
        {
            var addedSchedule = await _unitOfWork.OpeningSchedules.AddAsync(scheduleEntity); 

            var classEntity = new _class
            {
                class_code = createOpeningScheduleDto.ClassCode,
                instrument_id = createOpeningScheduleDto.InstrumentId
            };
            
            var addedClass = await _unitOfWork.Classes.AddAsync(classEntity);

            // Do AddAsync trong GenericRepository của bạn không gọi SaveChangesAsync(),
            // nên cần gọi CompleteAsync() ở đây để lưu cả schedule và class.
            await _unitOfWork.CompleteAsync(); 

            _logger.LogInformation($"Created OpeningSchedule ID: {addedSchedule.opening_schedule_id} and Class ID: {addedClass.class_id}");

            await GenerateClassSessionsForOpeningScheduleAsync(
                addedSchedule.opening_schedule_id, 
                addedClass.class_id,
                createOpeningScheduleDto.DefaultRoomId,
                createOpeningScheduleDto.TimeSlotIds
            );

            // Fetch the schedule again to ensure all navigation properties are loaded for mapping
            // (vì GetByIdAsync không có chức năng include).
            var finalSchedule = await _unitOfWork.OpeningSchedules.GetByIdAsync(addedSchedule.opening_schedule_id);
            if (finalSchedule != null && _unitOfWork.Context != null)
            {
                await _unitOfWork.Context.Entry(finalSchedule).Reference(s => s.teacher_user).LoadAsync();
                if (finalSchedule.teacher_user != null)
                {
                    await _unitOfWork.Context.Entry(finalSchedule.teacher_user).Reference(u => u.role).LoadAsync();
                }
                await _unitOfWork.Context.Entry(finalSchedule).Reference(s => s.instrument).LoadAsync();
                await _unitOfWork.Context.Entry(finalSchedule).Collection(s => s.day_of_weeks).LoadAsync();
            }

            return MapToOpeningScheduleDto(finalSchedule ?? addedSchedule);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during OpeningSchedule AddAsync.");
            throw new ApiException("Có lỗi xảy ra khi thêm lịch khai giảng hoặc lớp học vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while adding the opening schedule and class.");
            throw new ApiException("An unexpected error occurred while adding the opening schedule and class.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto)
    {
        var existingSchedule =
            await _unitOfWork.OpeningSchedules.GetByIdAsync(updateOpeningScheduleDto.OpeningScheduleId);

        if (existingSchedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", updateOpeningScheduleDto.OpeningScheduleId);
        }

        // TẢI CÁC NAVIGATION PROPERTIES CẦN THIẾT BẰNG CÁCH THỦ CÔNG
        // Vì GenericRepository.FindOneAsync và GetByIdAsync không có tham số include
        if (_unitOfWork.Context != null) // Đảm bảo Context có sẵn
        {
            await _unitOfWork.Context.Entry(existingSchedule).Collection(os => os.day_of_weeks).LoadAsync();
            await _unitOfWork.Context.Entry(existingSchedule).Reference(os => os.teacher_user).LoadAsync();
            if (existingSchedule.teacher_user != null)
            {
                await _unitOfWork.Context.Entry(existingSchedule.teacher_user).Reference(u => u.role).LoadAsync();
            }
            await _unitOfWork.Context.Entry(existingSchedule).Reference(os => os.instrument).LoadAsync();
        }

        if (updateOpeningScheduleDto.TeacherUserId.HasValue)
        {
            if (updateOpeningScheduleDto.TeacherUserId.Value != existingSchedule.teacher_user_id)
            {
                var teacherUser =
                    await _unitOfWork.Users.GetUserWithRoleAsync(updateOpeningScheduleDto.TeacherUserId.Value);
                if (teacherUser == null)
                {
                    throw new NotFoundException("Teacher User", "Id", updateOpeningScheduleDto.TeacherUserId.Value);
                }

                if (teacherUser.role?.role_name?.ToLower() != "teacher")
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "TeacherUserId", new string[] { "Người dùng được chọn không phải là giáo viên." } }
                    });
                }
                existingSchedule.teacher_user_id = updateOpeningScheduleDto.TeacherUserId.Value;
            }
        }
        else
        {
            existingSchedule.teacher_user_id = null;
        }

        if (updateOpeningScheduleDto.InstrumentId != existingSchedule.instrument_id)
        {
            var existingInstrument =
                await _unitOfWork.Instruments.GetByIdAsync(updateOpeningScheduleDto.InstrumentId);
            if (existingInstrument == null)
            {
                throw new NotFoundException("Instrument", "Id", updateOpeningScheduleDto.InstrumentId);
            }
            existingSchedule.instrument_id = updateOpeningScheduleDto.InstrumentId;
        }
        
        existingSchedule.class_code = updateOpeningScheduleDto.ClassCode;
        existingSchedule.opening_day = updateOpeningScheduleDto.OpeningDay;
        existingSchedule.total_sessions = updateOpeningScheduleDto.TotalSessions; 

        existingSchedule.end_date = updateOpeningScheduleDto.EndDate;
        existingSchedule.student_quantity = updateOpeningScheduleDto.StudentQuantity;
        existingSchedule.is_advanced_class = updateOpeningScheduleDto.IsAdvancedClass;
        
        if (updateOpeningScheduleDto.SelectedDayOfWeekIds != null)
        {
            // Collection 'day_of_weeks' đã được tải ở trên
            var currentDayOfWeekIds = existingSchedule.day_of_weeks.Select(d => d.day_of_week_id).ToList();
            
            // Loại bỏ những ngày không còn được chọn
            foreach (var existingDayId in currentDayOfWeekIds.Except(updateOpeningScheduleDto.SelectedDayOfWeekIds))
            {
                var dayToRemove = existingSchedule.day_of_weeks.FirstOrDefault(d => d.day_of_week_id == existingDayId);
                if (dayToRemove != null)
                {
                    // Vì GenericRepository của bạn có phương thức Remove(T entity)
                    // và không gọi SaveChangesAsync(), chúng ta có thể sử dụng nó.
                    _unitOfWork.DayOfWeekLookups.Remove(dayToRemove); 
                    existingSchedule.day_of_weeks.Remove(dayToRemove); // Cập nhật collection trên entity
                }
            }

            // Thêm những ngày mới được chọn
            foreach (var newDayId in updateOpeningScheduleDto.SelectedDayOfWeekIds.Except(currentDayOfWeekIds))
            {
                var dayToAdd = await _unitOfWork.DayOfWeekLookups.GetByIdAsync(newDayId);
                if (dayToAdd == null)
                {
                    throw new NotFoundException("DayOfWeekLookup", "Id", newDayId);
                }
                // Vì GenericRepository của bạn không có AddRangeAsync hoặc AddAsync(IEnumerable<T>) cho collection
                // chúng ta chỉ thêm vào collection của entity.
                // Các thay đổi sẽ được theo dõi bởi DbContext và lưu khi UpdateAsync/CompleteAsync được gọi.
                existingSchedule.day_of_weeks.Add(dayToAdd);
            }
        }

        try
        {
            await _unitOfWork.OpeningSchedules.UpdateAsync(existingSchedule);
            // CompleteAsync() sẽ lưu các thay đổi từ UpdateAsync (nếu UpdateAsync không tự lưu) và các thay đổi trên collection day_of_weeks
            // LƯU Ý: GenericRepository của bạn có UpdateAsync tự gọi SaveChangesAsync().
            // Nếu bạn muốn toàn bộ giao dịch được điều khiển bởi UnitOfWork.CompleteAsync(), bạn cần loại bỏ
            // SaveChangesAsync() khỏi UpdateAsync trong GenericRepository.
            await _unitOfWork.CompleteAsync(); 
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during OpeningSchedule UpdateAsync.");
            throw new ApiException("Có lỗi xảy ra khi cập nhật lịch khai giảng trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating the opening schedule.");
            throw new ApiException("An unexpected error occurred while updating the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleToDelete = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (scheduleToDelete == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        try
        {
            var relatedClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == scheduleToDelete.class_code);

            if (relatedClass != null)
            {
                var hasRelatedSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.class_id == relatedClass.class_id);
                if (hasRelatedSessions)
                {
                    throw new ApiException("Không thể xóa lịch khai giảng này vì lớp học liên quan có các buổi học.", null, (int)HttpStatusCode.Conflict);
                }

                // Giả sử Users có navigation property là classes để kiểm tra join table
                var hasUsersInJoinTable = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == relatedClass.class_id));

                if (hasUsersInJoinTable)
                {
                    throw new ApiException("Không thể xóa lịch khai giảng này vì lớp học liên quan có người dùng (học viên/giáo viên) đang tham gia.", null, (int)HttpStatusCode.Conflict);
                }
                
                // DeleteAsync trong GenericRepository của bạn không gọi SaveChangesAsync()
                await _unitOfWork.Classes.DeleteAsync(relatedClass.class_id);
            }

            // DeleteAsync trong GenericRepository của bạn không gọi SaveChangesAsync()
            await _unitOfWork.OpeningSchedules.DeleteAsync(id);
            
            // Cần gọi CompleteAsync() để lưu các thay đổi xóa (nếu DeleteAsync trong repo không tự lưu)
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during OpeningSchedule DeleteAsync.");
            throw new ApiException(
                "Có lỗi xảy ra khi xóa lịch khai giảng khỏi cơ sở dữ liệu. Vui lòng kiểm tra các ràng buộc liên quan.", dbEx,
                (int)HttpStatusCode.InternalServerError); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting the opening schedule.");
            throw new ApiException("An unexpected error occurred while deleting the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<OpeningScheduleDto>> SearchOpeningSchedulesAsync(
        string? classCode = null, DateOnly? openingDay = null,
        DateOnly? endDate = null,
        int? studentQuantity = null, bool? isAdvancedClass = null)
    {
        // VÌ GenericRepository của bạn không có FindAllAsync với predicate và includeProperties,
        // chúng ta sẽ phải GetAllAsync và lọc tại đây. Điều này có thể không hiệu quả với tập dữ liệu lớn.
        var schedules = await _unitOfWork.OpeningSchedules.GetAllAsync();

        var query = schedules.AsQueryable();

        if (!string.IsNullOrEmpty(classCode))
        {
            query = query.Where(os => os.class_code.Contains(classCode));
        }
        if (openingDay.HasValue)
        {
            query = query.Where(os => os.opening_day == openingDay.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(os => os.end_date == endDate.Value);
        }
        if (studentQuantity.HasValue)
        {
            query = query.Where(os => os.student_quantity == studentQuantity.Value);
        }
        if (isAdvancedClass.HasValue)
        {
            query = query.Where(os => os.is_advanced_class == isAdvancedClass.Value);
        }
        
        return query.Select(MapToOpeningScheduleDto);
    }

    private async Task GenerateClassSessionsForOpeningScheduleAsync(
        int openingScheduleId, int classId, int defaultRoomId, List<int> timeSlotIds)
    {
        // Vì FindOneAsync không có includeProperties, phải tải thủ công
        var openingSchedule = await _unitOfWork.OpeningSchedules
            .FindOneAsync(os => os.opening_schedule_id == openingScheduleId);
        
        if (openingSchedule != null && _unitOfWork.Context != null) // Đảm bảo Context có sẵn
        {
            await _unitOfWork.Context.Entry(openingSchedule).Collection(os => os.day_of_weeks).LoadAsync();
        }

        if (openingSchedule == null || !openingSchedule.opening_day.HasValue || !openingSchedule.end_date.HasValue)
        {
            _logger.LogWarning($"Could not find opening schedule {openingScheduleId} or its dates for class session generation.");
            return;
        }

        var startDate = openingSchedule.opening_day.Value;
        var endDate = openingSchedule.end_date.Value;
        int sessionsCreated = 0;
        var maxSessions = openingSchedule.total_sessions;

        var selectedDayNumbers = openingSchedule.day_of_weeks.Select(d => d.day_number).ToHashSet();

        List<CreateClassSessionDto> sessionsToCreate = new List<CreateClassSessionDto>();

        // THAY THẾ: Vì GenericRepository không có FindAllAsync(predicate).
        // Phải GetAllAsync và lọc/chuyển đổi tại service.
        var allDays = await _unitOfWork.Days.GetAllAsync();
        var daysInRange = allDays.Where(d => d.date_of_day >= startDate && d.date_of_day <= endDate)
                                 .ToDictionary(d => d.date_of_day, d => d.day_id);


        for (DateOnly currentDate = startDate; currentDate <= endDate && sessionsCreated < maxSessions; currentDate = currentDate.AddDays(1))
        {
            int currentDayNumber = (int)currentDate.DayOfWeek;
            if (currentDayNumber == 0) currentDayNumber = 7; // Chuyển Chủ Nhật từ 0 thành 7

            if (selectedDayNumbers.Contains(currentDayNumber))
            {
                if (daysInRange.TryGetValue(currentDate, out int dayEntityId))
                {
                    foreach (var timeSlotId in timeSlotIds)
                    {
                        if (sessionsCreated >= maxSessions) break;

                        sessionsToCreate.Add(new CreateClassSessionDto
                        {
                            ClassId = classId,
                            DayId = dayEntityId,
                            Date = currentDate,
                            RoomId = defaultRoomId,
                            TimeSlotId = timeSlotId,
                            SessionNumber = sessionsCreated + 1
                        });
                        sessionsCreated++;
                    }
                }
                else
                {
                    _logger.LogWarning($"Day entity not found in pre-loaded dictionary for date {currentDate:yyyy-MM-dd}. Skipping class session creation for this date.");
                }
            }
        }

        if (sessionsToCreate.Any())
        {
            _logger.LogInformation($"Attempting to create {sessionsToCreate.Count} class sessions for opening schedule {openingScheduleId}.");
            foreach (var sessionDto in sessionsToCreate)
            {
                try
                {
                    // LƯU Ý: Service này đang gọi _classSessionService.AddAsync(sessionDto);
                    // Đảm bảo ClassSessionService xử lý lưu thay đổi hoặc UnitOfWork.CompleteAsync()
                    // được gọi ở đâu đó sau nhiều thao tác AddAsync nếu muốn giao dịch toàn diện.
                    await _classSessionService.AddAsync(sessionDto); 
                }
                catch (ValidationException vex)
                {
                    _logger.LogWarning(vex, $"Validation error creating class session for ClassId {sessionDto.ClassId}, DayId {sessionDto.DayId}, TimeSlotId {sessionDto.TimeSlotId}: {string.Join("; ", vex.Errors.SelectMany(e => e.Value))}");
                }
                catch (ApiException apiEx)
                {
                    _logger.LogWarning(apiEx, $"API error creating class session for ClassId {sessionDto.ClassId}, DayId {sessionDto.DayId}, TimeSlotId {sessionDto.TimeSlotId}: {apiEx.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error creating class session for ClassId {sessionDto.ClassId}, DayId {sessionDto.DayId}, TimeSlotId {sessionDto.TimeSlotId}.");
                }
            }
            _logger.LogInformation($"Finished attempting to create class sessions for opening schedule {openingScheduleId}. Total sessions to create: {maxSessions}. Actual created: {sessionsToCreate.Count(s => true)} (may be less due to errors/duplicates).");
        }
        else
        {
            _logger.LogInformation($"No class sessions to create for opening schedule {openingScheduleId}.");
        }
    }

    private OpeningScheduleDto MapToOpeningScheduleDto(opening_schedule model)
    {
        return new OpeningScheduleDto
        {
            OpeningScheduleId = model.opening_schedule_id,
            ClassCode = model.class_code,
            OpeningDay = model.opening_day,
            EndDate = model.end_date,
            StudentQuantity = model.student_quantity,
            IsAdvancedClass = model.is_advanced_class,
            TeacherUser = model.teacher_user != null
                ? new UserForOpeningScheduleDto
                {
                    AccountName = model.teacher_user.account_name
                }
                : null,
            InstrumentId = model.instrument_id,
            Instrument = model.instrument != null
                ? new InstrumentDto
                {
                    InstrumentId = model.instrument.instrument_id,
                    InstrumentName = model.instrument.instrument_name
                }
                : null,
            TotalSessions = model.total_sessions,
            // Đảm bảo day_of_weeks đã được tải trước khi truy cập
            SelectedDayOfWeekIds = model.day_of_weeks?.Select(d => d.day_of_week_id).ToList()
        };
    }
}