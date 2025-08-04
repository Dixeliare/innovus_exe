using DTOs;
using Repository.Basic.IRepositories;
using Services.IServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Basic.UnitOfWork;

namespace Services.Services
{
    public class AttendanceStatusService : IAttendanceStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<AttendanceStatusDto>> GetAllAsync()
        {
            var statuses = await _unitOfWork.AttendanceStatuses.GetAllAsync();
            return statuses.Select(s => new AttendanceStatusDto
            {
                StatusId = s.status_id,
                StatusName = s.status_name
            });
        }
        public async Task UpdateAsync(UpdateAttendanceStatusDto dto)
        {
            var entity = await _unitOfWork.AttendanceStatuses.GetByIdAsync(dto.StatusId);
            if (entity == null)
                throw new KeyNotFoundException($"Attendance status with id {dto.StatusId} not found");
            entity.status_name = dto.StatusName;
            await _unitOfWork.AttendanceStatuses.UpdateAsync(entity);
        }
    }
} 