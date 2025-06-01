using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class DocumentService : IDocumentService
{
    private readonly DocumentRepository _documentRepository;
    private readonly InstrumentRepository _instrumentRepository; // Inject cho kiểm tra khóa ngoại

    public DocumentService(DocumentRepository documentRepository,
        InstrumentRepository instrumentRepository)
    {
        _documentRepository = documentRepository;
        _instrumentRepository = instrumentRepository;
    }
    
    public async Task<IEnumerable<document>> GetAllAsync()
    {
        return await _documentRepository.GetAllAsync();
    }

    public async Task<document> GetByIdAsync(int id)
    {
        return await _documentRepository.GetByIdAsync(id);
    }

    public async Task<DocumentDto> AddAsync(CreateDocumentDto createDocumentDto)
        {
            // Kiểm tra sự tồn tại của khóa ngoại Instrument
            var instrumentExists = await _instrumentRepository.GetByIdAsync(createDocumentDto.InstrumentId);
            if (instrumentExists == null)
            {
                throw new KeyNotFoundException($"Instrument with ID {createDocumentDto.InstrumentId} not found.");
            }

            var documentEntity = new document
            {
                lesson = createDocumentDto.Lesson,
                lesson_name = createDocumentDto.LessonName,
                link = createDocumentDto.Link,
                instrument_id = createDocumentDto.InstrumentId
            };

            var addedDocument = await _documentRepository.AddAsync(documentEntity);
            return MapToDocumentDto(addedDocument);
        }

        // UPDATE Document
        public async Task UpdateAsync(UpdateDocumentDto updateDocumentDto)
        {
            var existingDocument = await _documentRepository.GetByIdAsync(updateDocumentDto.DocumentId);

            if (existingDocument == null)
            {
                throw new KeyNotFoundException($"Document with ID {updateDocumentDto.DocumentId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (updateDocumentDto.Lesson.HasValue)
            {
                existingDocument.lesson = updateDocumentDto.Lesson.Value;
            }
            if (!string.IsNullOrEmpty(updateDocumentDto.LessonName))
            {
                existingDocument.lesson_name = updateDocumentDto.LessonName;
            }
            if (!string.IsNullOrEmpty(updateDocumentDto.Link))
            {
                existingDocument.link = updateDocumentDto.Link;
            }

            // Kiểm tra và cập nhật khóa ngoại Instrument nếu có giá trị mới được cung cấp
            if (updateDocumentDto.InstrumentId.HasValue && updateDocumentDto.InstrumentId.Value != existingDocument.instrument_id)
            {
                var instrumentExists = await _instrumentRepository.GetByIdAsync(updateDocumentDto.InstrumentId.Value);
                if (instrumentExists == null)
                {
                    throw new KeyNotFoundException($"Instrument with ID {updateDocumentDto.InstrumentId} not found for update.");
                }
                existingDocument.instrument_id = updateDocumentDto.InstrumentId.Value;
            }

            await _documentRepository.UpdateAsync(existingDocument);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _documentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<document>> SearchDocumentsAsync(int? lesson = null, string? lessonName = null, string? link = null, int? instrumentId = null)
    {
        return await _documentRepository.SearchDocumentsAsync(lesson, lessonName, link, instrumentId);
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