using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ConsultationRequestService : IConsultationRequestService
{
    private readonly ConsultationRequestRepository _consultationRequestRepository;
    private readonly StatisticRepository _statisticRepository; // Inject cho kiểm tra khóa ngoại
    private readonly ConsultationTopicRepository _consultationTopicRepository; // Inject cho kiểm tra khóa ngoại

    public ConsultationRequestService(ConsultationRequestRepository consultationRequestRepository,
        StatisticRepository statisticRepository,
        ConsultationTopicRepository consultationTopicRepository)
    {
        _consultationRequestRepository = consultationRequestRepository;
        _statisticRepository = statisticRepository;
        _consultationTopicRepository = consultationTopicRepository;
    }
    
    public async Task<IEnumerable<consultation_request>> GetAllAsync()
    {
        return await _consultationRequestRepository.GetAllAsync();
    }

    public async Task<consultation_request> GetByIdAsync(int id)
    {
        return await _consultationRequestRepository.GetByIdAsync(id);
    }

    public async Task<ConsultationRequestDto> AddAsync(CreateConsultationRequestDto createConsultationRequestDto)
        {
            // Kiểm tra sự tồn tại của các khóa ngoại (nếu chúng được cung cấp)
            if (createConsultationRequestDto.StatisticId.HasValue)
            {
                var statisticExists = await _statisticRepository.GetByIdAsync(createConsultationRequestDto.StatisticId.Value);
                if (statisticExists == null)
                {
                    throw new KeyNotFoundException($"Statistic with ID {createConsultationRequestDto.StatisticId} not found.");
                }
            }

            if (createConsultationRequestDto.ConsultationTopicId.HasValue)
            {
                var topicExists = await _consultationTopicRepository.GetByIdAsync(createConsultationRequestDto.ConsultationTopicId.Value);
                if (topicExists == null)
                {
                    throw new KeyNotFoundException($"Consultation Topic with ID {createConsultationRequestDto.ConsultationTopicId} not found.");
                }
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

            var addedRequest = await _consultationRequestRepository.AddAsync(requestEntity);
            return MapToConsultationRequestDto(addedRequest);
        }

        // UPDATE Consultation Request
        public async Task UpdateAsync(UpdateConsultationRequestDto updateConsultationRequestDto)
        {
            var existingRequest = await _consultationRequestRepository.GetByIdAsync(updateConsultationRequestDto.ConsultationRequestId);

            if (existingRequest == null)
            {
                throw new KeyNotFoundException($"Consultation Request with ID {updateConsultationRequestDto.ConsultationRequestId} not found.");
            }

            // Cập nhật các trường nếu có giá trị mới được cung cấp
            if (!string.IsNullOrEmpty(updateConsultationRequestDto.Fullname))
            {
                existingRequest.fullname = updateConsultationRequestDto.Fullname;
            }
            if (!string.IsNullOrEmpty(updateConsultationRequestDto.ContactNumber))
            {
                existingRequest.contact_number = updateConsultationRequestDto.ContactNumber;
            }
            if (!string.IsNullOrEmpty(updateConsultationRequestDto.Email))
            {
                existingRequest.email = updateConsultationRequestDto.Email;
            }
            if (!string.IsNullOrEmpty(updateConsultationRequestDto.Note))
            {
                existingRequest.note = updateConsultationRequestDto.Note;
            }
            if (updateConsultationRequestDto.HasContact.HasValue)
            {
                existingRequest.has_contact = updateConsultationRequestDto.HasContact.Value;
            }

            // Kiểm tra và cập nhật khóa ngoại nếu có giá trị mới được cung cấp
            if (updateConsultationRequestDto.StatisticId.HasValue)
            {
                var statisticExists = await _statisticRepository.GetByIdAsync(updateConsultationRequestDto.StatisticId.Value);
                if (statisticExists == null)
                {
                    throw new KeyNotFoundException($"Statistic with ID {updateConsultationRequestDto.StatisticId} not found for update.");
                }
                existingRequest.statistic_id = updateConsultationRequestDto.StatisticId.Value;
            }
            else if (updateConsultationRequestDto.StatisticId == null && existingRequest.statistic_id.HasValue)
            {
                // Nếu client gửi null cho một trường nullable, hãy gán null cho nó trong entity
                existingRequest.statistic_id = null;
            }


            if (updateConsultationRequestDto.ConsultationTopicId.HasValue)
            {
                var topicExists = await _consultationTopicRepository.GetByIdAsync(updateConsultationRequestDto.ConsultationTopicId.Value);
                if (topicExists == null)
                {
                    throw new KeyNotFoundException($"Consultation Topic with ID {updateConsultationRequestDto.ConsultationTopicId} not found for update.");
                }
                existingRequest.consultation_topic_id = updateConsultationRequestDto.ConsultationTopicId.Value;
            }
            else if (updateConsultationRequestDto.ConsultationTopicId == null && existingRequest.consultation_topic_id.HasValue)
            {
                existingRequest.consultation_topic_id = null;
            }


            await _consultationRequestRepository.UpdateAsync(existingRequest);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _consultationRequestRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(string? fullname = null, string? contactNumber = null, string? email = null,
        string? note = null, bool? hasContact = null)
    {
        return await _consultationRequestRepository.SearchConsultationRequestsAsync(fullname , contactNumber, email, note, hasContact);
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