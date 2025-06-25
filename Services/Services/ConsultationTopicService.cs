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

public class ConsultationTopicService : IConsultationTopicService
{
    // private readonly IConsultationTopicRepository _consultationTopicRepository;
    //
    // public ConsultationTopicService(IConsultationTopicRepository consultationTopicRepository) => _consultationTopicRepository = consultationTopicRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public ConsultationTopicService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ConsultationTopicDto>> GetAllAsync()
    {
        var topics = await _unitOfWork.ConsultationTopics.GetAllAsync();
        return topics.Select(MapToConsultationTopicDto);
    }

    public async Task<ConsultationTopicDto> GetByIdAsync(int id)
    {
        var topic = await _unitOfWork.ConsultationTopics.GetByIdAsync(id);
        if (topic == null)
        {
            throw new NotFoundException("ConsultationTopic", "Id", id);
        }
        return MapToConsultationTopicDto(topic);
    }

    public async Task<ConsultationTopicDto> AddAsync(CreateConsultationTopicDto createConsultationTopicDto)
    {
        // Kiểm tra tên chủ đề đã tồn tại chưa (không phân biệt chữ hoa chữ thường)
        var existingTopic = await _unitOfWork.ConsultationTopics.FindOneAsync(
            t => t.consultation_topic_name != null && t.consultation_topic_name.ToLower() == createConsultationTopicDto.ConsultationTopicName.ToLower());
        if (existingTopic != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "ConsultationTopicName", new string[] { $"Tên chủ đề tư vấn '{createConsultationTopicDto.ConsultationTopicName}' đã tồn tại." } }
            });
        }

        var topicEntity = new consultation_topic
        {
            consultation_topic_name = createConsultationTopicDto.ConsultationTopicName
        };

        try
        {
            var addedTopic = await _unitOfWork.ConsultationTopics.AddAsync(topicEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToConsultationTopicDto(addedTopic);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm chủ đề tư vấn vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the consultation topic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateConsultationTopicDto updateConsultationTopicDto)
    {
        var existingTopic = await _unitOfWork.ConsultationTopics.GetByIdAsync(updateConsultationTopicDto.ConsultationTopicId);

        if (existingTopic == null)
        {
            throw new NotFoundException("ConsultationTopic", "Id", updateConsultationTopicDto.ConsultationTopicId);
        }

        // Kiểm tra tên chủ đề đã tồn tại chưa nếu tên đang được cập nhật và khác biệt
        if (!string.IsNullOrEmpty(updateConsultationTopicDto.ConsultationTopicName) && updateConsultationTopicDto.ConsultationTopicName.ToLower() != existingTopic.consultation_topic_name?.ToLower())
        {
            var topicWithSameName = await _unitOfWork.ConsultationTopics.FindOneAsync(
                t => t.consultation_topic_name != null && t.consultation_topic_name.ToLower() == updateConsultationTopicDto.ConsultationTopicName.ToLower());
            if (topicWithSameName != null && topicWithSameName.consultation_topic_id != updateConsultationTopicDto.ConsultationTopicId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ConsultationTopicName", new string[] { $"Tên chủ đề tư vấn '{updateConsultationTopicDto.ConsultationTopicName}' đã được sử dụng bởi một chủ đề khác." } }
                });
            }
        }

        // Cập nhật tên nếu có giá trị được cung cấp
        if (updateConsultationTopicDto.ConsultationTopicName != null) // Cho phép null nếu DTO và DB cho phép
        {
            existingTopic.consultation_topic_name = updateConsultationTopicDto.ConsultationTopicName;
        }

        try
        {
            await _unitOfWork.ConsultationTopics.UpdateAsync(existingTopic);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật chủ đề tư vấn trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the consultation topic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var topicToDelete = await _unitOfWork.ConsultationTopics.GetByIdAsync(id);
        if (topicToDelete == null)
        {
            throw new NotFoundException("ConsultationTopic", "Id", id);
        }

        try
        {
            // Kiểm tra xem có bất kỳ consultation_request nào liên quan đến topic này không
            // Giả sử ConsultationRequests có phương thức CountAsync hoặc tương tự để kiểm tra sự tồn tại
            var hasRelatedRequests = await _unitOfWork.ConsultationRequests.AnyAsync(cr => cr.consultation_topic_id == id);
            if (hasRelatedRequests)
            {
                throw new ApiException("Không thể xóa chủ đề tư vấn này vì có các yêu cầu tư vấn đang sử dụng nó.", null, (int)HttpStatusCode.Conflict); // 409 Conflict
            }

            await _unitOfWork.ConsultationTopics.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Đây là catch dự phòng nếu kiểm tra AnyAsync không đủ, hoặc có ràng buộc khác
            throw new ApiException("Có lỗi xảy ra khi xóa chủ đề tư vấn trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the consultation topic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ConsultationTopicDto>> SearchConsultationTopicsAsync(string? topicName = null)
    {
        var topics = await _unitOfWork.ConsultationTopics.SearchConsultationTopicsAsync(topicName); // Giả định SearchConsultationTopicsAsync có sẵn
        return topics.Select(MapToConsultationTopicDto);
    }
    
    private ConsultationTopicDto MapToConsultationTopicDto(consultation_topic model)
    {
        return new ConsultationTopicDto
        {
            ConsultationTopicId = model.consultation_topic_id,
            ConsultationTopicName = model.consultation_topic_name
        };
    }
}