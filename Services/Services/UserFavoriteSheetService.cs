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

public class UserFavoriteSheetService : IUserFavoriteSheetService
{
    // private readonly IUserFavoriteSheetRepository _userFavoriteSheetRepository;
    // private readonly ISheetMusicRepository _sheetMusicRepository; // Có thể cần để lấy SheetMusic chi tiết
    // private readonly IUserRepository _userRepository; // Có thể cần để lấy User chi tiết
    //
    // public UserFavoriteSheetService(
    //     IUserFavoriteSheetRepository userFavoriteSheetRepository,
    //     ISheetMusicRepository sheetMusicRepository, // Inject if needed
    //     IUserRepository userRepository) // Inject if needed
    // {
    //     _userFavoriteSheetRepository = userFavoriteSheetRepository;
    //     _sheetMusicRepository = sheetMusicRepository;
    //     _userRepository = userRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork;

    public UserFavoriteSheetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Phương thức thêm bản nhạc yêu thích
    public async Task<UserFavoriteSheetDto> AddUserFavoriteSheetAsync(CreateUserFavoriteSheetDto createDto)
    {
        // 1. Kiểm tra sự tồn tại của User và SheetMusic
        var userExists = await _unitOfWork.Users.GetByIdAsync(createDto.UserId);
        if (userExists == null)
        {
            throw new NotFoundException("User", "Id", createDto.UserId);
        }

        var sheetMusicExists = await _unitOfWork.SheetMusics.GetByIdAsync(createDto.SheetMusicId);
        if (sheetMusicExists == null)
        {
            throw new NotFoundException("Sheet Music", "Id", createDto.SheetMusicId);
        }

        // 2. Kiểm tra xem cặp đã tồn tại chưa (logic nghiệp vụ)
        if (await _unitOfWork.UserFavoriteSheets.IsSheetFavoriteForUserAsync(createDto.UserId, createDto.SheetMusicId))
        {
            // Ném ValidationException cho lỗi nghiệp vụ (đã yêu thích rồi)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Conflict", new string[] { $"Người dùng {createDto.UserId} đã yêu thích bản nhạc {createDto.SheetMusicId} rồi." } }
            });
        }

        var entity = new user_favorite_sheet
        {
            user_id = createDto.UserId,
            sheet_music_id = createDto.SheetMusicId,
            is_favorite = createDto.IsFavorite ?? true // Mặc định là true nếu không cung cấp
        };

        try
        {
            var addedEntity = await _unitOfWork.UserFavoriteSheets.AddAsync(entity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToUserFavoriteSheetDto(addedEntity);
        }
        catch (DbUpdateException dbEx) // Bắt lỗi từ Entity Framework (ví dụ: trùng khóa chính nếu không kiểm tra ở trên)
        {
            // Kiểm tra lỗi trùng lặp từ DB nếu IsSheetFavoriteForUserAsync không đủ
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DbError", new string[] { "Dữ liệu đã tồn tại trong cơ sở dữ liệu." } }
                }, dbEx);
            }
            throw new ApiException("Có lỗi xảy ra khi thêm bản nhạc yêu thích vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex) // Bắt các lỗi không mong muốn khác
        {
            throw new ApiException("An unexpected error occurred while adding the user favorite sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // Update User Favorite Sheet (primarily for is_favorite status)
    public async Task UpdateUserFavoriteSheetAsync(UpdateUserFavoriteSheetDto updateDto)
    {
        var existingEntity = await _unitOfWork.UserFavoriteSheets.GetByIdAsync(updateDto.UserId, updateDto.SheetMusicId);

        if (existingEntity == null)
        {
            throw new NotFoundException("User Favorite Sheet", new { UserId = updateDto.UserId, SheetMusicId = updateDto.SheetMusicId });
        }

        existingEntity.is_favorite = updateDto.IsFavorite;

        try
        {
            await _unitOfWork.UserFavoriteSheets.UpdateAsync(existingEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật bản nhạc yêu thích trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the user favorite sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // Phương thức xóa bản ghi yêu thích
    public async Task DeleteUserFavoriteSheetAsync(int userId, int sheetMusicId)
    {
        var entityToDelete = await _unitOfWork.UserFavoriteSheets.GetByIdAsync(userId, sheetMusicId);
        if (entityToDelete == null)
        {
            throw new NotFoundException("User Favorite Sheet", new { UserId = userId, SheetMusicId = sheetMusicId });
        }

        try
        {
            // Sử dụng DeleteAsync hoặc Delete2ArgumentsAsync của repository
            // Giả định DeleteAsync trong GenericRepository có thể xóa bằng entity hoặc ID
            await _unitOfWork.UserFavoriteSheets.Delete2ArgumentsAsync(userId, sheetMusicId);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx) // Ví dụ: lỗi ràng buộc toàn vẹn nếu có
        {
            throw new ApiException("Không thể xóa bản nhạc yêu thích do lỗi cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the user favorite sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<UserFavoriteSheetDto?> GetByIdAsync(int userId, int sheetMusicId)
    {
        var userFavoriteSheet = await _unitOfWork.UserFavoriteSheets.GetByIdAsync(userId, sheetMusicId);
        if (userFavoriteSheet == null)
        {
            throw new NotFoundException("User Favorite Sheet", new { UserId = userId, SheetMusicId = sheetMusicId });
        }
        return MapToUserFavoriteSheetDto(userFavoriteSheet);
    }

    // Lấy một bản ghi user_favorite_sheet cụ thể
    public async Task<user_favorite_sheet?> GetUserFavoriteSheetEntryAsync(int userId, int sheetMusicId)
    {
        return await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
    }

    // Lấy danh sách các bản nhạc yêu thích của một người dùng
    public async Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync(int userId)
    {
        var userExists = await _unitOfWork.Users.GetByIdAsync(userId);
        if (userExists == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }
        var favoriteEntries = await _unitOfWork.UserFavoriteSheets.GetUserFavoriteSheetsAsync(userId);
        return favoriteEntries.Select(ufs => ufs.sheet_music).Where(sm => sm != null)!;
    }

    // Lấy danh sách người dùng yêu thích một bản nhạc
    public async Task<IEnumerable<user>> GetUsersFavoritingSheetAsync(int sheetMusicId)
    {
        var sheetMusicExists = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
        if (sheetMusicExists == null)
        {
            throw new NotFoundException("Sheet Music", "Id", sheetMusicId);
        }
        var favoritingEntries = await _unitOfWork.UserFavoriteSheets.GetUsersWhoFavoritedSheetAsync(sheetMusicId);
        return favoritingEntries.Select(ufs => ufs.user).Where(u => u != null)!;
    }

    // Kiểm tra xem bản nhạc có được yêu thích bởi người dùng không
    public async Task<bool> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId)
    {
        return await _unitOfWork.UserFavoriteSheets.IsSheetFavoriteForUserAsync(userId, sheetMusicId);
    }

    private UserFavoriteSheetDto MapToUserFavoriteSheetDto(user_favorite_sheet model)
    {
        return new UserFavoriteSheetDto
        {
            UserId = model.user_id,
            SheetMusicId = model.sheet_music_id,
            IsFavorite = model.is_favorite
        };
    }
}