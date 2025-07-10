using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationRequestController : ControllerBase
    {
        private readonly IConsultationRequestService _consultationRequestService;
        
        public ConsultationRequestController(IConsultationRequestService consultationRequestService) => _consultationRequestService = consultationRequestService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationRequestDto>>> GetAllConsultationRequests() // Trả về DTOs
        {
            var requests = await _consultationRequestService.GetAllAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationRequestDto>> GetConsultationRequestById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var request = await _consultationRequestService.GetByIdAsync(id);
            return Ok(request); // Service đã trả về DTO
        }

        [HttpGet("search_by")]
        public async Task<ActionResult<IEnumerable<ConsultationRequestDto>>> SearchByAsync( // Trả về DTOs
            [FromQuery] string? fullname = null,
            [FromQuery] string? contactNumber = null,
            [FromQuery] string? email = null,
            [FromQuery] string? note = null,
            [FromQuery] bool? hasContact = null)
        {
            var requests = await _consultationRequestService.SearchConsultationRequestsAsync(fullname, contactNumber, email, note, hasContact);
            return Ok(requests); // Service đã trả về DTOs
        }

        [HttpPost]
        public async Task<ActionResult<ConsultationRequestDto>> CreateConsultationRequest([FromBody] CreateConsultationRequestDto createConsultationRequestDto)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            var createdRequest = await _consultationRequestService.AddAsync(createConsultationRequestDto);
            return CreatedAtAction(nameof(GetConsultationRequestById), new { id = createdRequest.ConsultationRequestId }, createdRequest);
        }

        // PUT: api/ConsultationRequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultationRequest(int id, [FromBody] UpdateConsultationRequestDto updateConsultationRequestDto)
        {
            if (id != updateConsultationRequestDto.ConsultationRequestId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ConsultationRequestId", new string[] { "ID yêu cầu tư vấn trong URL không khớp với ID trong body." } }
                });
            }

            // --- LẤY ID NGƯỜI DÙNG HIỆN TẠI TỪ CLAIMS ---
            int? currentUserId = null;
            // Giả sử ID người dùng của bạn được lưu trữ trong claim NameIdentifier
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier); 
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                currentUserId = parsedUserId;
            }
            // Tùy chọn: Nếu xác thực là bắt buộc cho hành động này, bạn có thể ném một ngoại lệ ở đây
            // if (currentUserId == null)
            // {
            //     return Unauthorized("Người dùng chưa được xác thực hoặc không thể truy xuất ID người dùng.");
            // }
            // --- KẾT THÚC LẤY ID NGƯỜI DÙNG HIỆN TẠI ---

            await _consultationRequestService.UpdateAsync(updateConsultationRequestDto, currentUserId); // <--- TRUYỀN ID NGƯỜI DÙNG HIỆN TẠI
            return NoContent();
        }
        
        [HttpPatch("{id}/contact-status")] // Sử dụng PATCH cho cập nhật một phần
        public async Task<IActionResult> UpdateConsultationRequestContactStatus(int id, [FromBody] UpdateConsultationRequestContactStatusDto dto)
        {
            if (id != dto.ConsultationRequestId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ConsultationRequestId", new string[] { "ID yêu cầu tư vấn trong URL không khớp với ID trong body." } }
                });
            }

            int? currentUserId = null;
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier); 
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                currentUserId = parsedUserId;
            }
            
            // Tùy chọn: Nếu việc cập nhật trạng thái yêu cầu xác thực, bạn có thể kiểm tra ở đây
            // if (!currentUserId.HasValue)
            // {
            //     return Unauthorized("Người dùng chưa được xác thực.");
            // }

            await _consultationRequestService.UpdateConsultationRequestContactStatusAsync(dto, currentUserId);
            return NoContent(); // Trả về 204 No Content nếu thành công
        } 

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultationRequest(int id)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _consultationRequestService.DeleteAsync(id);
            return NoContent();
        }
    }
}
