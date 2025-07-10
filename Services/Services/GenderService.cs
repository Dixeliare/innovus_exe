using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class GenderService : IGenderService
{
    private readonly IUnitOfWork _unitOfWork;

    public GenderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<GenderDto>> GetAllGendersAsync()
    {
        var genders = await _unitOfWork.Genders.GetAllAsync();
        return genders.Select(g => new GenderDto
        {
            GenderId = g.gender_id,
            GenderName = g.gender_name
        });
    }

    public async Task<GenderDto?> GetGenderByIdAsync(int id)
    {
        var gender = await _unitOfWork.Genders.GetByIdAsync(id);
        if (gender == null)
        {
            throw new NotFoundException("Gender", "Id", id);
        }
        return new GenderDto
        {
            GenderId = gender.gender_id,
            GenderName = gender.gender_name
        };
    }

    public async Task<GenderDto> AddGenderAsync(CreateGenderDto createGenderDto)
    {
        // Kiểm tra tên giới tính đã tồn tại chưa
        var existingGender = await _unitOfWork.Genders.FindOneAsync(g => g.gender_name == createGenderDto.GenderName);
        if (existingGender != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "GenderName", new string[] { $"Tên giới tính '{createGenderDto.GenderName}' đã tồn tại." } }
            });
        }

        var genderEntity = new gender
        {
            gender_name = createGenderDto.GenderName
        };

        try
        {
            var addedGender = await _unitOfWork.Genders.AddAsync(genderEntity);
            await _unitOfWork.CompleteAsync();
            return new GenderDto
            {
                GenderId = addedGender.gender_id,
                GenderName = addedGender.gender_name
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm giới tính vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the gender.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateGenderAsync(UpdateGenderDto updateGenderDto)
    {
        var existingGender = await _unitOfWork.Genders.GetByIdAsync(updateGenderDto.GenderId);
        if (existingGender == null)
        {
            throw new NotFoundException("Gender", "Id", updateGenderDto.GenderId);
        }

        // Kiểm tra tên giới tính nếu thay đổi và trùng lặp
        if (existingGender.gender_name != updateGenderDto.GenderName)
        {
            var genderWithSameName = await _unitOfWork.Genders.FindOneAsync(g => g.gender_name == updateGenderDto.GenderName);
            if (genderWithSameName != null && genderWithSameName.gender_id != updateGenderDto.GenderId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "GenderName", new string[] { $"Tên giới tính '{updateGenderDto.GenderName}' đã tồn tại." } }
                });
            }
        }

        existingGender.gender_name = updateGenderDto.GenderName;

        try
        {
            await _unitOfWork.Genders.UpdateAsync(existingGender);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật giới tính trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the gender.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteGenderAsync(int id)
    {
        var genderToDelete = await _unitOfWork.Genders.GetByIdAsync(id);
        if (genderToDelete == null)
        {
            throw new NotFoundException("Gender", "Id", id);
        }

        try
        {
            await _unitOfWork.Genders.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            // Kiểm tra lỗi ràng buộc khóa ngoại
            if (dbEx.InnerException?.Message?.Contains("violates foreign key constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new ApiException("Không thể xóa giới tính này vì có người dùng đang tham chiếu đến nó.", dbEx,
                    (int)HttpStatusCode.Conflict); // 409 Conflict
            }
            throw new ApiException("Có lỗi xảy ra khi xóa giới tính khỏi cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the gender.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }
}