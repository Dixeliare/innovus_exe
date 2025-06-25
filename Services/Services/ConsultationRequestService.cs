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
            var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(createConsultationRequestDto.StatisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", createConsultationRequestDto.StatisticId.Value);
            }
        }

        if (createConsultationRequestDto.ConsultationTopicId.HasValue)
        {
            var topicExists = await _unitOfWork.ConsultationTopics.GetByIdAsync(createConsultationRequestDto.ConsultationTopicId.Value);
            if (topicExists == null)
            {
                throw new NotFoundException("ConsultationTopic", "Id", createConsultationRequestDto.ConsultationTopicId.Value);
            }
        }

        // Kiểm tra tính duy nhất của Email (giả sử mỗi Email chỉ có 1 yêu cầu duy nhất hoặc duy nhất trong 1 khoảng thời gian nhất định)
        // Nếu không cần duy nhất, bỏ qua đoạn này
        var existingRequestWithEmail = await _unitOfWork.ConsultationRequests.FindOneAsync(
            cr => cr.email == createConsultationRequestDto.Email);
        if (existingRequestWithEmail != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new string[] { $"Email '{createConsultationRequestDto.Email}' đã có yêu cầu tư vấn đang chờ xử lý." } }
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
            consultation_topic_id = createConsultationRequestDto.ConsultationTopicId
        };

        try
        {
            var addedRequest = await _unitOfWork.ConsultationRequests.AddAsync(requestEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToConsultationRequestDto(addedRequest);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm yêu cầu tư vấn vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the consultation request.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateConsultationRequestDto updateConsultationRequestDto)
    {
        var existingRequest = await _unitOfWork.ConsultationRequests.GetByIdAsync(updateConsultationRequestDto.ConsultationRequestId);

        if (existingRequest == null)
        {
            throw new NotFoundException("ConsultationRequest", "Id", updateConsultationRequestDto.ConsultationRequestId);
        }

        // Kiểm tra và cập nhật khóa ngoại StatisticId nếu có giá trị mới được cung cấp và khác giá trị cũ
        if (updateConsultationRequestDto.StatisticId.HasValue && updateConsultationRequestDto.StatisticId.Value != existingRequest.statistic_id)
        {
            var statisticExists = await _unitOfWork.Statistics.GetByIdAsync(updateConsultationRequestDto.StatisticId.Value);
            if (statisticExists == null)
            {
                throw new NotFoundException("Statistic", "Id", updateConsultationRequestDto.StatisticId.Value);
            }
            existingRequest.statistic_id = updateConsultationRequestDto.StatisticId.Value;
        }
        // Cho phép gán null cho StatisticId nếu DTO gửi null
        else if (updateConsultationRequestDto.StatisticId == null && existingRequest.statistic_id.HasValue)
        {
            existingRequest.statistic_id = null;
        }

        // Kiểm tra và cập nhật khóa ngoại ConsultationTopicId nếu có giá trị mới được cung cấp và khác giá trị cũ
        if (updateConsultationRequestDto.ConsultationTopicId.HasValue && updateConsultationRequestDto.ConsultationTopicId.Value != existingRequest.consultation_topic_id)
        {
            var topicExists = await _unitOfWork.ConsultationTopics.GetByIdAsync(updateConsultationRequestDto.ConsultationTopicId.Value);
            if (topicExists == null)
            {
                throw new NotFoundException("ConsultationTopic", "Id", updateConsultationRequestDto.ConsultationTopicId.Value);
            }
            existingRequest.consultation_topic_id = updateConsultationRequestDto.ConsultationTopicId.Value;
        }
        // Cho phép gán null cho ConsultationTopicId nếu DTO gửi null
        else if (updateConsultationRequestDto.ConsultationTopicId == null && existingRequest.consultation_topic_id.HasValue)
        {
            existingRequest.consultation_topic_id = null;
        }

        // Kiểm tra tính duy nhất của Email nếu Email được cập nhật và khác giá trị cũ
        if (!string.IsNullOrEmpty(updateConsultationRequestDto.Email) && updateConsultationRequestDto.Email != existingRequest.email)
        {
            var requestWithSameEmail = await _unitOfWork.ConsultationRequests.FindOneAsync(
                cr => cr.email == updateConsultationRequestDto.Email);
            if (requestWithSameEmail != null && requestWithSameEmail.consultation_request_id != updateConsultationRequestDto.ConsultationRequestId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new string[] { $"Email '{updateConsultationRequestDto.Email}' đã được sử dụng bởi một yêu cầu tư vấn khác." } }
                });
            }
            existingRequest.email = updateConsultationRequestDto.Email;
        }
        // Email là not-nullable trong model, nên không cần xử lý updateDto.Email == null.
        // Nếu bạn muốn cho phép update to empty string, thì cần xem xét logic thêm.
        // else if (string.IsNullOrEmpty(updateConsultationRequestDto.Email))
        // {
        //    existingRequest.email = string.Empty; // Hoặc gán giá trị mặc định khác nếu cần
        // }


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
        if (updateConsultationRequestDto.HasContact.HasValue)
        {
            existingRequest.has_contact = updateConsultationRequestDto.HasContact.Value;
        }
        else if (updateConsultationRequestDto.HasContact == null) // Cho phép client gửi null để không thay đổi giá trị hiện tại
        {
            // Do nothing, giữ giá trị hiện tại của existingRequest.has_contact
        }


        try
        {
            await _unitOfWork.ConsultationRequests.UpdateAsync(existingRequest);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật yêu cầu tư vấn trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the consultation request.", ex, (int)HttpStatusCode.InternalServerError);
        }
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
            // Trong trường hợp ConsultationRequest không có khóa ngoại đến các bảng khác (tức là không ai phụ thuộc vào nó)
            // thì DbUpdateException ở đây thường là do lỗi DB khác, không phải khóa ngoại bị phụ thuộc.
            // Tuy nhiên, nếu có ràng buộc khác, nó sẽ được bắt ở đây.
            throw new ApiException("Có lỗi xảy ra khi xóa yêu cầu tư vấn khỏi cơ sở dữ liệu. Có thể có dữ liệu liên quan.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the consultation request.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ConsultationRequestDto>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null)
    {
        var requests = await _unitOfWork.ConsultationRequests.SearchConsultationRequestsAsync(fullname, contactNumber, email, note, hasContact); // Giả định SearchConsultationRequestsAsync có sẵn
        return requests.Select(MapToConsultationRequestDto);
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
            ConsultationTopicId = model.consultation_topic_id
            // Nếu có DTO lồng nhau, map ở đây
            // ConsultationTopic = model.consultation_topic != null ? new ConsultationTopicDto { Id = model.consultation_topic.id, Name = model.consultation_topic.name } : null
            // ...
        };
    }
}