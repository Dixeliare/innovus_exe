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
    private readonly IStatisticService _statisticService;
    private readonly ILogger<OpeningScheduleService> _logger;

    public OpeningScheduleService(IUnitOfWork unitOfWork, IClassSessionService classSessionService, IStatisticService statisticService, ILogger<OpeningScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _classSessionService = classSessionService;
        _statisticService = statisticService;
        _logger = logger;
    }

    public async Task<IEnumerable<OpeningScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.OpeningSchedules.GetAllWithClassSessionsAsync(); // Sử dụng method mới
        var result = new List<OpeningScheduleDto>();
        foreach (var schedule in schedules)
        {
            result.Add(await MapToOpeningScheduleDtoAsync(schedule));
        }
        return result;
    }

    public async Task<OpeningScheduleDto> GetByIdAsync(int id)
    {
        var schedule = await _unitOfWork.OpeningSchedules.GetByIdWithClassSessionsAsync(id); // Sử dụng method mới
        if (schedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        // Tải role của teacher_user nếu cần
        if (_unitOfWork.Context != null && schedule.teacher_user != null)
        {
            await _unitOfWork.Context.Entry(schedule.teacher_user).Reference(u => u.role).LoadAsync();
        }

        return await MapToOpeningScheduleDtoAsync(schedule); // Sử dụng method mới
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
            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            
            try
            {
                var addedSchedule = await _unitOfWork.OpeningSchedules.AddAsync(scheduleEntity); 

                var classEntity = new _class
                {
                    class_code = createOpeningScheduleDto.ClassCode,
                    instrument_id = createOpeningScheduleDto.InstrumentId
                };
                
                var addedClass = await _unitOfWork.Classes.AddAsync(classEntity);

                // Lưu opening_schedule và class trước
                await _unitOfWork.CompleteAsync(); 

                _logger.LogInformation($"Created OpeningSchedule ID: {addedSchedule.opening_schedule_id} and Class ID: {addedClass.class_id}");

                // Tạo class_sessions
                await GenerateClassSessionsForOpeningScheduleAsync(
                    addedSchedule.opening_schedule_id, 
                    addedClass.class_id,
                    createOpeningScheduleDto.DefaultRoomId,
                    createOpeningScheduleDto.TimeSlotIds
                );

                // Commit transaction nếu mọi thứ thành công
                await transaction.CommitAsync();

                // Cập nhật thống kê sau khi tạo class thành công
                await _statisticService.UpdateStatisticsOnClassChangeAsync();

                // Fetch the schedule again to ensure all navigation properties are loaded for mapping
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

                return await MapToOpeningScheduleDtoAsync(finalSchedule ?? addedSchedule);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during opening schedule creation. Transaction rolled back.");
                
                // Cleanup orphan data nếu có
                await CleanupOrphanDataAsync(createOpeningScheduleDto.ClassCode);
                throw;
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during OpeningSchedule AddAsync.");
            
            // Cleanup orphan data nếu có
            await CleanupOrphanDataAsync(createOpeningScheduleDto.ClassCode);
            
            throw new ApiException("Có lỗi xảy ra khi thêm lịch khai giảng hoặc lớp học vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while adding the opening schedule and class.");
            
            // Cleanup orphan data nếu có
            await CleanupOrphanDataAsync(createOpeningScheduleDto.ClassCode);
            
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

        // Kiểm tra xem class_code mới đã tồn tại chưa
        var existingClassWithNewCode = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == updateOpeningScheduleDto.ClassCode);
        
        if (existingClassWithNewCode != null)
        {
            // Trường hợp 1: class_code đã có sẵn
            // Nếu người dùng để null instrument thì lấy instrument của class có sẵn
            // Nếu người dùng nhập instrument thì vẫn lấy instrument của class có sẵn
            existingSchedule.instrument_id = existingClassWithNewCode.instrument_id;
            _logger.LogInformation($"Class code '{updateOpeningScheduleDto.ClassCode}' already exists. Using existing instrument_id: {existingClassWithNewCode.instrument_id}");
        }
        else
        {
            // Trường hợp 2: class_code chưa có sẵn, tạo mới class
            if (updateOpeningScheduleDto.InstrumentId <= 0)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "InstrumentId", new string[] { "Mã nhạc cụ không hợp lệ." } }
                });
            }
            
            var existingInstrument = await _unitOfWork.Instruments.GetByIdAsync(updateOpeningScheduleDto.InstrumentId);
            if (existingInstrument == null)
            {
                throw new NotFoundException("Instrument", "Id", updateOpeningScheduleDto.InstrumentId);
            }
            existingSchedule.instrument_id = updateOpeningScheduleDto.InstrumentId;
            
            // Tạo mới class với class_code mới
            var newClass = new _class
            {
                class_code = updateOpeningScheduleDto.ClassCode,
                instrument_id = updateOpeningScheduleDto.InstrumentId
            };
            await _unitOfWork.Classes.AddAsync(newClass);
            _logger.LogInformation($"Created new class with code '{updateOpeningScheduleDto.ClassCode}' and instrument_id: {updateOpeningScheduleDto.InstrumentId}");
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
            
            // Loại bỏ những ngày không còn được chọn (chỉ xóa liên kết, không xóa entity gốc)
            foreach (var existingDayId in currentDayOfWeekIds.Except(updateOpeningScheduleDto.SelectedDayOfWeekIds))
            {
                var dayToRemove = existingSchedule.day_of_weeks.FirstOrDefault(d => d.day_of_week_id == existingDayId);
                if (dayToRemove != null)
                {
                    existingSchedule.day_of_weeks.Remove(dayToRemove); // chỉ xóa liên kết
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

            // --- BẮT ĐẦU: Xử lý class và generate lại class_session khi update opening_schedule ---
            var oldClassCode = existingSchedule.class_code;
            var relatedClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == oldClassCode);
            var newRelatedClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == updateOpeningScheduleDto.ClassCode);
            
            if (relatedClass != null && newRelatedClass != null && relatedClass.class_id != newRelatedClass.class_id)
            {
                // Nếu class_code thay đổi và class mới đã tồn tại, xóa class cũ nếu không có dữ liệu liên quan
                var hasOldClassSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.class_id == relatedClass.class_id);
                var hasOldClassUsers = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == relatedClass.class_id));
                
                if (!hasOldClassSessions && !hasOldClassUsers)
                {
                    await _unitOfWork.Classes.DeleteAsync(relatedClass.class_id);
                    _logger.LogInformation($"Deleted old class with code '{oldClassCode}' as it has no related data");
                }
            }
            
            // Xóa toàn bộ class_session của class mới (nếu có) và generate lại
            if (newRelatedClass != null)
            {
                var oldSessions = await _unitOfWork.ClassSessions.FindAllAsync(cs => cs.class_id == newRelatedClass.class_id);
                foreach (var session in oldSessions)
                {
                    _unitOfWork.ClassSessions.Remove(session);
                }
                await _unitOfWork.CompleteAsync();

                // Generate lại class_session mới
                await GenerateClassSessionsForOpeningScheduleAsync(
                    existingSchedule.opening_schedule_id,
                    newRelatedClass.class_id,
                    updateOpeningScheduleDto.DefaultRoomId,
                    updateOpeningScheduleDto.TimeSlotIds
                );
                
                // Cập nhật thống kê sau khi update class
                await _statisticService.UpdateStatisticsOnClassChangeAsync();
            }
            // --- KẾT THÚC ---
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
            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            
            try
            {
                var relatedClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == scheduleToDelete.class_code);

                if (relatedClass != null)
                {
                    _logger.LogInformation($"Found related class: ID={relatedClass.class_id}, Code={relatedClass.class_code}");
                    
                    // 1. Xóa tất cả class_sessions liên quan trước
                    var relatedClassSessions = await _unitOfWork.ClassSessions.GetAllAsync();
                    _logger.LogInformation($"Total class sessions in database: {relatedClassSessions.Count()}");
                    
                    var sessionsToDelete = relatedClassSessions.Where(cs => cs.class_id == relatedClass.class_id).ToList();
                    _logger.LogInformation($"Found {sessionsToDelete.Count} class sessions to delete for class {relatedClass.class_code}");
                    
                    // Log chi tiết từng session để debug
                    foreach (var session in sessionsToDelete)
                    {
                        _logger.LogInformation($"Session to delete: ID={session.class_session_id}, ClassID={session.class_id}, DayID={session.day_id}, TimeSlotID={session.time_slot_id}, RoomID={session.room_id}");
                    }
                    
                    foreach (var session in sessionsToDelete)
                    {
                        _logger.LogInformation($"Deleting class session ID: {session.class_session_id}");
                        await _unitOfWork.ClassSessions.DeleteAsync(session.class_session_id);
                    }

                    // 2. Kiểm tra xem có user nào đang tham gia class này không
                    var hasUsersInJoinTable = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == relatedClass.class_id));

                    if (hasUsersInJoinTable)
                    {
                        throw new ApiException("Không thể xóa lịch khai giảng này vì lớp học liên quan có người dùng (học viên/giáo viên) đang tham gia.", null, (int)HttpStatusCode.Conflict);
                    }
                    
                    // 3. Xóa class
                    _logger.LogInformation($"Deleting class ID: {relatedClass.class_id} with class code: {relatedClass.class_code}");
                    await _unitOfWork.Classes.DeleteAsync(relatedClass.class_id);
                }
                else
                {
                    _logger.LogWarning($"No related class found for opening schedule with class code: {scheduleToDelete.class_code}");
                }

                // 4. Xóa opening_schedule
                await _unitOfWork.OpeningSchedules.DeleteAsync(id);
                
                // 5. Lưu tất cả thay đổi
                await _unitOfWork.CompleteAsync();
                
                // 6. Commit transaction
                await transaction.CommitAsync();
                
                // Cập nhật thống kê sau khi xóa class
                await _statisticService.UpdateStatisticsOnClassChangeAsync();
                
                _logger.LogInformation($"Đã xóa thành công opening schedule với ID: {id}");
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
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
        var schedules = await _unitOfWork.OpeningSchedules.GetAllWithClassSessionsAsync(); // SỬA: Sử dụng method mới

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
        
        var result = new List<OpeningScheduleDto>();
        foreach (var schedule in query)
        {
            result.Add(await MapToOpeningScheduleDtoAsync(schedule));
        }
        return result;
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
            int successfullyCreated = 0;
            foreach (var sessionDto in sessionsToCreate)
            {
                try
                {
                    _logger.LogInformation($"Creating class session: ClassId={sessionDto.ClassId}, DayId={sessionDto.DayId}, TimeSlotId={sessionDto.TimeSlotId}, RoomId={sessionDto.RoomId}, SessionNumber={sessionDto.SessionNumber}");
                    
                    // LƯU Ý: Service này đang gọi _classSessionService.AddAsync(sessionDto);
                    // Đảm bảo ClassSessionService xử lý lưu thay đổi hoặc UnitOfWork.CompleteAsync()
                    // được gọi ở đâu đó sau nhiều thao tác AddAsync nếu muốn giao dịch toàn diện.
                    var createdSession = await _classSessionService.AddAsync(sessionDto);
                    successfullyCreated++;
                    _logger.LogInformation($"Successfully created class session with ID: {createdSession.ClassSessionId}");
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
            _logger.LogInformation($"Finished attempting to create class sessions for opening schedule {openingScheduleId}. Total sessions to create: {maxSessions}. Successfully created: {successfullyCreated}.");
        }
        else
        {
            _logger.LogInformation($"No class sessions to create for opening schedule {openingScheduleId}.");
        }
    }

    private async Task<OpeningScheduleDto> MapToOpeningScheduleDtoAsync(opening_schedule model)
    {
        // Lấy thông tin room và timeslot từ class sessions
        List<room> rooms = new List<room>();
        List<timeslot> timeslots = new List<timeslot>();
        
        try
        {
            // Nếu đã include class sessions thì lấy từ navigation property
            if (model.class_codeNavigation?.class_sessions != null)
            {
                rooms = model.class_codeNavigation.class_sessions.Select(cs => cs.room).Where(r => r != null).Distinct().ToList();
                timeslots = model.class_codeNavigation.class_sessions.Select(cs => cs.time_slot).Where(ts => ts != null).Distinct().ToList();
            }
            else
            {
                // Fallback: query từ database
                var classSessions = await _unitOfWork.ClassSessions.FindAllAsync(cs => 
                    cs._class.class_code == model.class_code);
                rooms = classSessions.Select(cs => cs.room).Where(r => r != null).Distinct().ToList();
                timeslots = classSessions.Select(cs => cs.time_slot).Where(ts => ts != null).Distinct().ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error getting room and timeslot info for opening schedule {model.opening_schedule_id}");
            // Nếu có lỗi thì để trống
            rooms = new List<room>();
            timeslots = new List<timeslot>();
        }
        
        try
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
                SelectedDayOfWeekIds = model.day_of_weeks?.Select(d => d.day_of_week_id).ToList(),
                
                // Thêm thông tin room và timeslot
                DefaultRoomId = rooms.FirstOrDefault()?.room_id,
                DefaultRoom = rooms.FirstOrDefault() != null
                    ? new RoomDto
                    {
                        RoomId = rooms.FirstOrDefault()!.room_id,
                        RoomCode = rooms.FirstOrDefault()!.room_code,
                        Capacity = rooms.FirstOrDefault()!.capacity,
                        Description = rooms.FirstOrDefault()!.description
                    }
                    : null,
                TimeSlotIds = timeslots.Select(ts => ts.timeslot_id).ToList(),
                TimeSlots = timeslots.Select(ts => new TimeslotDto
                {
                    TimeslotId = ts.timeslot_id,
                    StartTime = ts.start_time,
                    EndTime = ts.end_time
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error mapping opening schedule {model.opening_schedule_id} to DTO");
            throw;
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
            SelectedDayOfWeekIds = model.day_of_weeks?.Select(d => d.day_of_week_id).ToList(),
            
            // Thêm thông tin room và timeslot (sẽ được populate sau nếu cần)
            DefaultRoomId = null,
            DefaultRoom = null,
            TimeSlotIds = new List<int>(),
            TimeSlots = new List<TimeslotDto>()
        };
    }

    /// <summary>
    /// Cleanup method để xóa dữ liệu orphan khi có lỗi trong quá trình tạo opening_schedule
    /// </summary>
    public async Task CleanupOrphanDataAsync(string classCode, int? openingScheduleId = null)
    {
        try
        {
            // Tìm class theo class_code
            var orphanClass = await _unitOfWork.Classes.FindOneAsync(c => c.class_code == classCode);
            if (orphanClass != null)
            {
                // Xóa class_sessions orphan liên quan đến class này
                var orphanClassSessions = await _unitOfWork.ClassSessions.GetAllAsync();
                var sessionsToDelete = orphanClassSessions.Where(cs => cs.class_id == orphanClass.class_id).ToList();
                
                foreach (var session in sessionsToDelete)
                {
                    await _unitOfWork.ClassSessions.DeleteAsync(session.class_session_id);
                }

                // Xóa class orphan
                await _unitOfWork.Classes.DeleteAsync(orphanClass.class_id);
            }

            // Xóa opening_schedule orphan nếu có
            if (openingScheduleId.HasValue)
            {
                var orphanSchedule = await _unitOfWork.OpeningSchedules.GetByIdAsync(openingScheduleId.Value);
                if (orphanSchedule != null)
                {
                    await _unitOfWork.OpeningSchedules.DeleteAsync(openingScheduleId.Value);
                }
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"Cleaned up orphan data for class code: {classCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during cleanup for class code: {classCode}");
            // Không throw exception ở đây để tránh ảnh hưởng đến flow chính
        }
    }
}