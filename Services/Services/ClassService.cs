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

public class ClassService : IClassService
{
    // private readonly IClassRepository _classRepository;
    //
    // public ClassService (IClassRepository classRepository) => _classRepository = classRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public ClassService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClassDto>> GetAllAsync()
    {
        var classes = await _unitOfWork.Classes.GetAllAsync();
        return classes.Select(MapToClassDto);
    }

    public async Task<ClassDto> GetByIdAsync(int id)
    {
        var cls = await _unitOfWork.Classes.GetByIdAsync(id);
        if (cls == null)
        {
            throw new NotFoundException("Class", "Id", id);
        }
        return MapToClassDto(cls);
    }

    public async Task<ClassDto> AddAsync(CreateClassDto createClassDto)
    {
        var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(createClassDto.InstrumentId);
        if (instrumentExists == null)
        {
            throw new NotFoundException("Instrument", "Id", createClassDto.InstrumentId);
        }

        if (!string.IsNullOrEmpty(createClassDto.ClassCode))
        {
            var existingClass = await _unitOfWork.Classes.FindOneAsync(
                c => c.class_code != null && c.class_code.ToLower() == createClassDto.ClassCode.ToLower());
            if (existingClass != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassCode", new[] { $"Mã lớp học '{createClassDto.ClassCode}' đã tồn tại." } } // Sử dụng new[] thay vì new string[]
                });
            }
        }

        var classEntity = new _class
        {
            class_code = createClassDto.ClassCode,
            instrument_id = createClassDto.InstrumentId
        };

        try
        {
            var addedClass = await _unitOfWork.Classes.AddAsync(classEntity);
            await _unitOfWork.CompleteAsync();
            return MapToClassDto(addedClass);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lớp học vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the class.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateClassDto updateClassDto)
    {
        var existingClass = await _unitOfWork.Classes.GetByIdAsync(updateClassDto.ClassId);

        if (existingClass == null)
        {
            throw new NotFoundException("Class", "Id", updateClassDto.ClassId);
        }

        // Kiểm tra và cập nhật InstrumentId nếu có giá trị mới được cung cấp và khác giá trị cũ
        // Đảm bảo updateClassDto.InstrumentId có giá trị trước khi truy cập .Value
        if (updateClassDto.InstrumentId.HasValue && updateClassDto.InstrumentId.Value != existingClass.instrument_id)
        {
            var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(updateClassDto.InstrumentId.Value);
            if (instrumentExists == null)
            {
                throw new NotFoundException("Instrument", "Id", updateClassDto.InstrumentId.Value);
            }
            existingClass.instrument_id = updateClassDto.InstrumentId.Value;
        }
        // Nếu client gửi null cho InstrumentId (và nó là nullable trong DB), bạn có thể gán null:
        // else if (updateClassDto.InstrumentId == null)
        // {
        //     existingClass.instrument_id = null; // Chỉ làm điều này nếu instrument_id trong model là nullable
        // }


        // Kiểm tra tính duy nhất của ClassCode nếu ClassCode được cập nhật và khác giá trị cũ
        // Cần kiểm tra null cho updateClassDto.ClassCode trước khi gọi ToLower()
        if (!string.IsNullOrEmpty(updateClassDto.ClassCode) && updateClassDto.ClassCode.ToLower() != existingClass.class_code?.ToLower())
        {
            var classWithSameCode = await _unitOfWork.Classes.FindOneAsync(
                c => c.class_code != null && c.class_code.ToLower() == updateClassDto.ClassCode.ToLower());
            if (classWithSameCode != null && classWithSameCode.class_id != updateClassDto.ClassId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassCode", new[] { $"Mã lớp học '{updateClassDto.ClassCode}' đã được sử dụng bởi một lớp học khác." } }
                });
            }
            existingClass.class_code = updateClassDto.ClassCode;
        }
        // Nếu bạn muốn cho phép gán null cho class_code (nếu DB cho phép), bạn có thể thêm:
        // else if (updateClassDto.ClassCode == null)
        // {
        //     existingClass.class_code = null;
        // }

        try
        {
            await _unitOfWork.Classes.UpdateAsync(existingClass);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật lớp học trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the class.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var classToDelete = await _unitOfWork.Classes.GetByIdAsync(id);
        if (classToDelete == null)
        {
            throw new NotFoundException("Class", "Id", id);
        }

        try
        {
            var hasRelatedSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.class_id == id);
            if (hasRelatedSessions)
            {
                throw new ApiException("Không thể xóa lớp học này vì có các buổi học liên quan.", null, (int)HttpStatusCode.Conflict);
            }

            // Kiểm tra xem có bất kỳ user nào liên quan đến lớp này không
            // Đây là cách kiểm tra cho mối quan hệ Many-to-Many
            // Đảm bảo rằng Users Repository của bạn có phương thức Find/Any có thể thực hiện Join hoặc Include để kiểm tra.
            // Hoặc bạn có thể cần một repo riêng cho bảng user_class nếu có.
            var hasRelatedUsers = await _unitOfWork.Users.AnyAsync(u => u.classes.Any(c => c.class_id == id));
            if (hasRelatedUsers)
            {
                throw new ApiException("Không thể xóa lớp học này vì có người dùng (học viên/giáo viên) đang tham gia lớp.", null, (int)HttpStatusCode.Conflict);
            }

            await _unitOfWork.Classes.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa lớp học khỏi cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the class.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ClassDto>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        var classes = await _unitOfWork.Classes.SearchClassesAsync(instrumentId, classCode);
        return classes.Select(MapToClassDto);
    }
    
    private ClassDto MapToClassDto(_class cls)
    {
        return new ClassDto
        {
            ClassId = cls.class_id,
            ClassCode = cls.class_code,
            InstrumentId = cls.instrument_id
            // Không ánh xạ các navigation properties nếu không được yêu cầu hoặc nếu chúng quá sâu
            // Nếu bạn muốn bao gồm ClassSessions hoặc Users, bạn sẽ cần tạo DTOs cho chúng
            // và ánh xạ ở đây (ví dụ: ClassSessions = cls.class_sessions.Select(cs => MapToClassSessionDto(cs)).ToList(),)
        };
    }
}