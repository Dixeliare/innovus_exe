using DTOs;

namespace Services.IServices;

public interface IGenderService
{
    Task<IEnumerable<GenderDto>> GetAllGendersAsync();
    Task<GenderDto?> GetGenderByIdAsync(int id);
    Task<GenderDto> AddGenderAsync(CreateGenderDto createGenderDto);
    Task UpdateGenderAsync(UpdateGenderDto updateGenderDto);
    Task DeleteGenderAsync(int id);
}