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

public class DocumentService : IDocumentService
{
    // private readonly IDocumentRepository _documentRepository;
    // private readonly IInstrumentRepository _instrumentRepository; // Inject cho kiểm tra khóa ngoại
    //
    // public DocumentService(IDocumentRepository documentRepository,
    //     IInstrumentRepository instrumentRepository)
    // {
    //     _documentRepository = documentRepository;
    //     _instrumentRepository = instrumentRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DocumentDto>> GetAllAsync()
    {
        var documents = await _unitOfWork.Documents.GetAllAsync();
        return documents.Select(MapToDocumentDto);
    }

    public async Task<DocumentDto> GetByIdAsync(int id)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new NotFoundException("Document", "Id", id);
        }
        return MapToDocumentDto(document);
    }

    public async Task<DocumentDto> AddAsync(CreateDocumentDto createDocumentDto)
    {
        // Kiểm tra sự tồn tại của khóa ngoại Instrument
        var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(createDocumentDto.InstrumentId);
        if (instrumentExists == null)
        {
            throw new NotFoundException("Instrument", "Id", createDocumentDto.InstrumentId);
        }

        // Kiểm tra tính duy nhất của Link (giả sử Link là duy nhất)
        var existingDocumentWithLink = await _unitOfWork.Documents.FindOneAsync(
            d => d.link == createDocumentDto.Link);
        if (existingDocumentWithLink != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Link", new string[] { $"Link tài liệu '{createDocumentDto.Link}' đã tồn tại." } }
            });
        }

        // Kiểm tra tính duy nhất của Lesson + InstrumentId (giả sử mỗi bài học cho một nhạc cụ là duy nhất)
        if (createDocumentDto.Lesson.HasValue)
        {
            var existingDocumentWithLesson = await _unitOfWork.Documents.FindOneAsync(
                d => d.lesson == createDocumentDto.Lesson.Value && d.instrument_id == createDocumentDto.InstrumentId);
            if (existingDocumentWithLesson != null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Lesson", new string[] { $"Bài học số '{createDocumentDto.Lesson.Value}' đã tồn tại cho nhạc cụ ID '{createDocumentDto.InstrumentId}'." } }
                });
            }
        }


        var documentEntity = new document
        {
            lesson = createDocumentDto.Lesson,
            lesson_name = createDocumentDto.LessonName,
            link = createDocumentDto.Link,
            instrument_id = createDocumentDto.InstrumentId
        };

        try
        {
            var addedDocument = await _unitOfWork.Documents.AddAsync(documentEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToDocumentDto(addedDocument);
        }
        catch (DbUpdateException dbEx)
        {
            // Có thể catch thêm DbUpdateException nếu có lỗi ràng buộc khác từ DB (ví dụ, UNIQUE constraint)
            throw new ApiException("Có lỗi xảy ra khi thêm tài liệu vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the document.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateDocumentDto updateDocumentDto)
    {
        var existingDocument = await _unitOfWork.Documents.GetByIdAsync(updateDocumentDto.DocumentId);

        if (existingDocument == null)
        {
            throw new NotFoundException("Document", "Id", updateDocumentDto.DocumentId);
        }

        // Kiểm tra và cập nhật khóa ngoại Instrument nếu có giá trị mới được cung cấp và khác giá trị cũ
        if (updateDocumentDto.InstrumentId.HasValue && updateDocumentDto.InstrumentId.Value != existingDocument.instrument_id)
        {
            var instrumentExists = await _unitOfWork.Instruments.GetByIdAsync(updateDocumentDto.InstrumentId.Value);
            if (instrumentExists == null)
            {
                throw new NotFoundException("Instrument", "Id", updateDocumentDto.InstrumentId.Value);
            }
            existingDocument.instrument_id = updateDocumentDto.InstrumentId.Value;
        }

        // Kiểm tra tính duy nhất của Link nếu Link được cập nhật và khác giá trị cũ
        if (!string.IsNullOrEmpty(updateDocumentDto.Link) && updateDocumentDto.Link != existingDocument.link)
        {
            var documentWithSameLink = await _unitOfWork.Documents.FindOneAsync(
                d => d.link == updateDocumentDto.Link);
            if (documentWithSameLink != null && documentWithSameLink.document_id != updateDocumentDto.DocumentId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Link", new string[] { $"Link tài liệu '{updateDocumentDto.Link}' đã được sử dụng bởi một tài liệu khác." } }
                });
            }
            existingDocument.link = updateDocumentDto.Link;
        }
        // Cho phép gán null cho Link nếu DTO và DB cho phép, và Link được truyền vào là null (hoặc rỗng và bạn muốn set null)
        else if (updateDocumentDto.Link == null) // Nếu Link được truyền rõ ràng là null
        {
             existingDocument.link = null!; // Cần đảm bảo cột này trong DB cho phép null
        }


        // Kiểm tra tính duy nhất của Lesson + InstrumentId nếu một trong hai hoặc cả hai được cập nhật
        if (updateDocumentDto.Lesson.HasValue && updateDocumentDto.Lesson.Value != existingDocument.lesson ||
            (updateDocumentDto.InstrumentId.HasValue && updateDocumentDto.InstrumentId.Value != existingDocument.instrument_id))
        {
            int targetLesson = updateDocumentDto.Lesson ?? existingDocument.lesson ?? default;
            int targetInstrumentId = updateDocumentDto.InstrumentId ?? existingDocument.instrument_id;

            var documentWithSameLessonAndInstrument = await _unitOfWork.Documents.FindOneAsync(
                d => d.lesson == targetLesson && d.instrument_id == targetInstrumentId);

            if (documentWithSameLessonAndInstrument != null && documentWithSameLessonAndInstrument.document_id != updateDocumentDto.DocumentId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Lesson", new string[] { $"Bài học số '{targetLesson}' đã tồn tại cho nhạc cụ ID '{targetInstrumentId}'." } }
                });
            }
        }


        // Cập nhật các trường còn lại
        existingDocument.lesson = updateDocumentDto.Lesson ?? existingDocument.lesson;
        existingDocument.lesson_name = updateDocumentDto.LessonName ?? existingDocument.lesson_name;


        try
        {
            await _unitOfWork.Documents.UpdateAsync(existingDocument);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật tài liệu trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the document.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var documentToDelete = await _unitOfWork.Documents.GetByIdAsync(id);
        if (documentToDelete == null)
        {
            throw new NotFoundException("Document", "Id", id);
        }

        try
        {
            await _unitOfWork.Documents.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có user nào đó đang liên kết với tài liệu này, sẽ ném lỗi FK
            throw new ApiException("Không thể xóa tài liệu này vì nó đang được sử dụng bởi một hoặc nhiều người dùng.", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the document.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(int? lesson = null, string? lessonName = null, string? link = null, int? instrumentId = null)
    {
        var documents = await _unitOfWork.Documents.SearchDocumentsAsync(lesson, lessonName, link, instrumentId); // Giả định SearchDocumentsAsync có sẵn
        return documents.Select(MapToDocumentDto);
    }
    
    private DocumentDto MapToDocumentDto(document model)
    {
        return new DocumentDto
        {
            DocumentId = model.document_id,
            Lesson = model.lesson,
            LessonName = model.lesson_name,
            Link = model.link,
            InstrumentId = model.instrument_id
            // Nếu có DTO lồng nhau cho Instrument, map ở đây
            // Instrument = model.instrument != null ? new InstrumentDto { InstrumentId = model.instrument.instrument_id, Name = model.instrument.instrument_name } : null
        };
    }
}