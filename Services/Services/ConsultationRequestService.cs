using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class ConsultationRequestService : IConsultationRequestService
{
    // private readonly IConsultationRequestRepository _consultationRequestRepository;
    // private readonly IStatisticRepository _statisticRepository; // Inject cho kiểm tra khóa ngoại
    // private readonly IConsultationTopicRepository _consultationTopicRepository; // Inject cho kiểm tra khóa ngoại
    //
    // public ConsultationRequestService(IConsultationRequestRepository consultationRequestRepository,
    //     IStatisticRepository statisticRepository,
    //     IConsultationTopicRepository consultationTopicRepository)
    // {
    //     _consultationRequestRepository = consultationRequestRepository;
    //     _statisticRepository = statisticRepository;
    //     _consultationTopicRepository = consultationTopicRepository;
    // }

    private readonly IUnitOfWork _unitOfWork;

    public ConsultationRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ConsultationRequestDto>> GetAllAsync()
    {
        var requests = await _unitOfWork.ConsultationRequests.GetAllAsync();
        return requests.Select(MapToConsultationRequestDto);
    }

    public async Task<ConsultationRequestDto> GetByIdAsync(int id)
    {
        var request = await _unitOfWork.ConsultationRequests.GetByIdAsync(id);
        if (request == null)
        {
            throw new NotFoundException("ConsultationRequest", "Id", id);
        }

        return MapToConsultationRequestDto(request);
    }

    public async Task<ConsultationRequestDto> AddAsync(CreateConsultationRequestDto createConsultationRequestDto)
    {
        // Kiểm tra sự tồn tại của các khóa ngoại (nếu chúng được cung cấp)
        if (createConsultationRequestDto.StatisticId.HasValue)
        {
            var statisticExists =
                await _unitOfWork.Statistics.GetByIdAsync(createConsultationRequestDto.StatisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", createConsultationRequestDto.StatisticId.Value);
            }
        }

        if (createConsultationRequestDto.ConsultationTopicId.HasValue)
        {
            var topicExists =
                await _unitOfWork.ConsultationTopics.GetByIdAsync(
                    createConsultationRequestDto.ConsultationTopicId.Value);
            if (topicExists == null)
            {
                throw new NotFoundException("ConsultationTopic", "Id",
                    createConsultationRequestDto.ConsultationTopicId.Value);
            }
        }

        // Kiểm tra tính duy nhất của Email
        var existingRequestWithEmail =
            await _unitOfWork.ConsultationRequests.FindOneAsync(cr => cr.email == createConsultationRequestDto.Email);
        if (existingRequestWithEmail != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                {
                    "Email",
                    new string[]
                        { $"Email '{createConsultationRequestDto.Email}' đã có yêu cầu tư vấn đang chờ xử lý." }
                }
            });
        }

        var requestEntity = new consultation_request
        {
            fullname = createConsultationRequestDto.Fullname,
            contact_number = createConsultationRequestDto.ContactNumber,
            email = createConsultationRequestDto.Email,
            note = createConsultationRequestDto.Note,
            has_contact = false, // Mặc định là false khi tạo mới (chưa liên hệ)
            statistic_id = createConsultationRequestDto.StatisticId,
            consultation_topic_id = createConsultationRequestDto.ConsultationTopicId,
            // handled_by và handled_at sẽ là null khi tạo mới
        };

        try
        {
            var addedRequest = await _unitOfWork.ConsultationRequests.AddAsync(requestEntity);
            await _unitOfWork.CompleteAsync();

            await UpdateConsultationCountStatistic(DateTime.UtcNow);

            return MapToConsultationRequestDto(addedRequest);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm yêu cầu tư vấn vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the consultation request.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateConsultationRequestDto updateConsultationRequestDto, int? currentUserId) // <--- CHỮ KÝ ĐÃ CẬP NHẬT
    {
        var existingRequest =
            await _unitOfWork.ConsultationRequests.GetByIdAsync(updateConsultationRequestDto.ConsultationRequestId);

        if (existingRequest == null)
        {
            throw new NotFoundException("ConsultationRequest", "Id",
                updateConsultationRequestDto.ConsultationRequestId);
        }

        // Kiểm tra và cập nhật khóa ngoại StatisticId nếu có giá trị mới được cung cấp và khác giá trị cũ
        if (updateConsultationRequestDto.StatisticId.HasValue &&
            updateConsultationRequestDto.StatisticId.Value != existingRequest.statistic_id)
        {
            var statisticExists =
                await _unitOfWork.Statistics.GetByIdAsync(updateConsultationRequestDto.StatisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", updateConsultationRequestDto.StatisticId.Value);
            }

            existingRequest.statistic_id = updateConsultationRequestDto.StatisticId.Value;
        }
        else if (updateConsultationRequestDto.StatisticId == null && existingRequest.statistic_id.HasValue)
        {
            existingRequest.statistic_id = null;
        }

        // Kiểm tra và cập nhật khóa ngoại ConsultationTopicId nếu có giá trị mới được cung cấp và khác giá trị cũ
        if (updateConsultationRequestDto.ConsultationTopicId.HasValue &&
            updateConsultationRequestDto.ConsultationTopicId.Value != existingRequest.consultation_topic_id)
        {
            var topicExists =
                await _unitOfWork.ConsultationTopics.GetByIdAsync(
                    updateConsultationRequestDto.ConsultationTopicId.Value);
            if (topicExists == null)
            {
                throw new NotFoundException("ConsultationTopic", "Id",
                    updateConsultationRequestDto.ConsultationTopicId.Value);
            }

            existingRequest.consultation_topic_id = updateConsultationRequestDto.ConsultationTopicId.Value;
        }
        else if (updateConsultationRequestDto.ConsultationTopicId == null &&
                 existingRequest.consultation_topic_id.HasValue)
        {
            existingRequest.consultation_topic_id = null;
        }

        // Kiểm tra tính duy nhất của Email nếu Email được cập nhật và khác giá trị cũ
        if (!string.IsNullOrEmpty(updateConsultationRequestDto.Email) &&
            updateConsultationRequestDto.Email != existingRequest.email)
        {
            var requestWithSameEmail =
                await _unitOfWork.ConsultationRequests.FindOneAsync(cr =>
                    cr.email == updateConsultationRequestDto.Email);
            if (requestWithSameEmail != null && requestWithSameEmail.consultation_request_id !=
                updateConsultationRequestDto.ConsultationRequestId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    {
                        "Email",
                        new string[]
                        {
                            $"Email '{updateConsultationRequestDto.Email}' đã được sử dụng bởi một yêu cầu tư vấn khác."
                        }
                    }
                });
            }

            existingRequest.email = updateConsultationRequestDto.Email;
        }

        // Cập nhật các trường còn lại (chỉ cập nhật nếu có giá trị mới được cung cấp trong DTO)
        if (updateConsultationRequestDto.Fullname != null)
        {
            existingRequest.fullname = updateConsultationRequestDto.Fullname;
        }

        if (updateConsultationRequestDto.ContactNumber != null)
        {
            existingRequest.contact_number = updateConsultationRequestDto.ContactNumber;
        }

        if (updateConsultationRequestDto.Note != null)
        {
            existingRequest.note = updateConsultationRequestDto.Note;
        }

        // --- LOGIC MỚI CHO HAS_CONTACT, HANDLED_BY, VÀ HANDLED_AT ---
        if (updateConsultationRequestDto.HasContact.HasValue)
        {
            // Nếu has_contact đang được đặt thành true và trước đó là false/null
            if (updateConsultationRequestDto.HasContact.Value && !(existingRequest.has_contact ?? false))
            {
                if (!currentUserId.HasValue)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "HandledBy", new string[] { "ID người dùng là bắt buộc để đánh dấu 'đã liên hệ'." } }
                    });
                }
                var handlerUser = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
                if (handlerUser == null)
                {
                    throw new NotFoundException("User (Handler)", "Id", currentUserId.Value);
                }

                existingRequest.has_contact = true;
                existingRequest.handled_by = currentUserId.Value;
                existingRequest.handled_at = DateTime.UtcNow; // Sử dụng UTC để nhất quán
            }
            // Nếu has_contact đang được đặt thành false và trước đó là true
            else if (!updateConsultationRequestDto.HasContact.Value && (existingRequest.has_contact ?? false))
            {
                existingRequest.has_contact = false;
                existingRequest.handled_by = null; // Xóa thông tin người xử lý
                existingRequest.handled_at = null; // Xóa thông tin thời gian xử lý
            }
            // Nếu has_contact là true và vẫn là true, hoặc false và vẫn là false, không làm gì với thông tin người xử lý.
        }

        try
        {
            await _unitOfWork.ConsultationRequests.UpdateAsync(existingRequest);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật yêu cầu tư vấn trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the consultation request.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task UpdateConsultationCountStatistic(DateTime targetDateTime)
    {
        var targetDateOnly = DateOnly.FromDateTime(targetDateTime);
        var firstDayOfMonth = new DateOnly(targetDateOnly.Year, targetDateOnly.Month, 1);

        var statistic = await _unitOfWork.Statistics.FindOneAsync(s => s.date == firstDayOfMonth);

        if (statistic == null)
        {
            statistic = new statistic
            {
                date = firstDayOfMonth,
                new_students = 0,
                monthly_revenue = 0m,
                consultation_count = 1,             // 👈 khởi tạo đã +1
                total_students = 0,
                consultation_request_count = 1      // 👈 khởi tạo đã +1
            };

            await _unitOfWork.Statistics.AddAsync(statistic);
        }
        else
        {
            statistic.consultation_count = (statistic.consultation_count ?? 0) + 1;
            statistic.consultation_request_count = (statistic.consultation_request_count ?? 0) + 1;

            await _unitOfWork.Statistics.UpdateAsync(statistic);
        }

        await _unitOfWork.CompleteAsync(); // 💾 lưu thay đổi sau cùng
    }

    public async Task DeleteAsync(int id)
    {
        var requestToDelete = await _unitOfWork.ConsultationRequests.GetByIdAsync(id);
        if (requestToDelete == null)
        {
            throw new NotFoundException("ConsultationRequest", "Id", id);
        }

        try
        {
            await _unitOfWork.ConsultationRequests.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException(
                "Có lỗi xảy ra khi xóa yêu cầu tư vấn khỏi cơ sở dữ liệu. Có thể có dữ liệu liên quan.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the consultation request.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ConsultationRequestDto>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null)
    {
        var requests =
            await _unitOfWork.ConsultationRequests.SearchConsultationRequestsAsync(fullname, contactNumber, email, note,
                hasContact);
        return requests.Select(MapToConsultationRequestDto);
    }

    public async Task UpdateConsultationRequestContactStatusAsync(UpdateConsultationRequestContactStatusDto dto, int? currentUserId)
    {
        var existingRequest = await _unitOfWork.ConsultationRequests.GetByIdAsync(dto.ConsultationRequestId);

        if (existingRequest == null)
        {
            throw new NotFoundException("ConsultationRequest", "Id", dto.ConsultationRequestId);
        }

        // Logic cho HAS_CONTACT, HANDLED_BY, và HANDLED_AT
        // Nếu has_contact đang được đặt thành true và trước đó là false/null
        if (dto.HasContact && !(existingRequest.has_contact ?? false))
        {
            if (!currentUserId.HasValue)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "HandledBy", new string[] { "ID người dùng là bắt buộc để đánh dấu 'đã liên hệ'." } }
                });
            }
            var handlerUser = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
            if (handlerUser == null)
            {
                throw new NotFoundException("User (Handler)", "Id", currentUserId.Value);
            }

            existingRequest.has_contact = true;
            existingRequest.handled_by = currentUserId.Value;
            existingRequest.handled_at = DateTime.UtcNow; // Sử dụng UTC để nhất quán
        }
        // Nếu has_contact đang được đặt thành false và trước đó là true
        else if (!dto.HasContact && (existingRequest.has_contact ?? false))
        {
            existingRequest.has_contact = false;
            existingRequest.handled_by = null; // Xóa thông tin người xử lý
            existingRequest.handled_at = null; // Xóa thông tin thời gian xử lý
        }
        // Các trường hợp khác:
        // - Nếu has_contact là true và vẫn là true, không làm gì với thông tin người xử lý.
        // - Nếu has_contact là false và vẫn là false, không làm gì với thông tin người xử lý.
        // DTO đã truyền một giá trị bool không nullable, nên không cần kiểm tra .HasValue cho dto.HasContact.

        try
        {
            await _unitOfWork.ConsultationRequests.UpdateAsync(existingRequest);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật trạng thái liên hệ của yêu cầu tư vấn trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the consultation request contact status.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    private ConsultationRequestDto MapToConsultationRequestDto(consultation_request model)
    {
        return new ConsultationRequestDto
        {
            ConsultationRequestId = model.consultation_request_id,
            Fullname = model.fullname,
            ContactNumber = model.contact_number,
            Email = model.email,
            Note = model.note,
            HasContact = model.has_contact,
            StatisticId = model.statistic_id,
            ConsultationTopicId = model.consultation_topic_id,

            // Ánh xạ cho các DTO lồng nhau (hãy bật các dòng này)
            ConsultationTopic = model.consultation_topic != null ? new ConsultationTopicDto
            {
                ConsultationTopicId = model.consultation_topic.consultation_topic_id,
                ConsultationTopicName = model.consultation_topic.consultation_topic_name
            } : null,
            Statistic = model.statistic != null ? new StatisticDto
            {
                StatisticId = model.statistic.statistic_id,
                Date = model.statistic.date,
                NewStudents = model.statistic.new_students, // Ánh xạ tất cả các trường liên quan
                MonthlyRevenue = model.statistic.monthly_revenue,
                ConsultationCount = model.statistic.consultation_count,
                TotalStudents = model.statistic.total_students,
                ConsultationRequestCount = model.statistic.consultation_request_count
            } : null,

            HandledAt = model.handled_at, // <--- ĐÃ ÁNH XẠ
            HandledBy = model.handled_byNavigation != null ? new UserForConsultationRequestDto
            {
                UserId = model.handled_byNavigation.user_id,
                AccountName = model.handled_byNavigation.username // Hoặc bất kỳ thuộc tính nào khác lưu tên người dùng (ví dụ: username, fullname)
            } : null // <--- ĐÃ ÁNH XẠ
        };
    }
}