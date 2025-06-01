using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ClassService : IClassService
{
    private readonly ClassRepository _classRepository;
    
    public ClassService (ClassRepository classRepository) => _classRepository = classRepository;
    
    public async Task<IEnumerable<_class>> GetAll()
    {
        return await _classRepository.GetAll();
    }

    public async Task<_class> GetById(int id)
    {
        return await _classRepository.GetById(id);
    }

    public async Task<ClassDto> AddAsync(CreateClassDto createClassDto)
    {
        // Ánh xạ từ CreateClassDto sang Model _class
        var classEntity = new _class
        {
            class_code = createClassDto.ClassCode,
            instrument_id = createClassDto.InstrumentId
            // Các navigation properties (class_sessions, users) không cần thiết khi tạo mới
        };

        var addedClass = await _classRepository.AddAsync(classEntity);
        return MapToClassDto(addedClass); // Ánh xạ từ Model đã thêm sang ClassDto
    }

    // Method UpdateAsync (PUT)
    public async Task UpdateAsync(UpdateClassDto updateClassDto)
    {
        // Lấy entity hiện có từ DB để đảm bảo theo dõi bởi DbContext
        var existingClass = await _classRepository.GetByIdAsync(updateClassDto.ClassId);

        if (existingClass == null)
        {
            // Xử lý trường hợp không tìm thấy lớp học
            throw new KeyNotFoundException($"Class with ID {updateClassDto.ClassId} not found.");
        }

        // Cập nhật các thuộc tính từ UpdateClassDto sang entity hiện có
        existingClass.class_code = updateClassDto.ClassCode;
        existingClass.instrument_id = updateClassDto.InstrumentId;
        // Không cập nhật navigation properties ở đây nếu bạn không muốn thay đổi mối quan hệ

        await _classRepository.UpdateAsync(existingClass);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _classRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        return await _classRepository.SearchClassesAsync(instrumentId, classCode);
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