using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class UserFavoriteSheetService : IUserFavoriteSheetService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserFavoriteSheetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserFavoriteSheetDto>> GetAllAsync()
    {
        var favorites = await _unitOfWork.UserFavoriteSheets.GetAllAsync();
        return favorites.Select(MapToUserFavoriteSheetDto);
    }

    public async Task<UserFavoriteSheetDto> GetByIdAsync(int userId, int sheetMusicId)
    {
        var favorite = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
        
        if (favorite == null)
        {
            throw new NotFoundException("UserFavoriteSheet", $"UserId={userId}, SheetMusicId={sheetMusicId}", 0);
        }
        
        return MapToUserFavoriteSheetDto(favorite);
    }

    public async Task<UserFavoriteSheetListDto> GetUserFavoritesAsync(int userId)
    {
        // Kiểm tra user có tồn tại không
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", "Id", userId);
        }

        // Lấy tất cả favorite của user
        var favorites = await _unitOfWork.UserFavoriteSheets.GetUserFavoriteSheetsAsync(userId);

        var favoriteSheetMusics = new List<SheetMusicDto>();
        foreach (var favorite in favorites)
        {
            if (favorite.sheet_music != null)
            {
                favoriteSheetMusics.Add(new SheetMusicDto
                {
                    SheetMusicId = favorite.sheet_music.sheet_music_id,
                    Number = favorite.sheet_music.number,
                    MusicName = favorite.sheet_music.music_name,
                    Composer = favorite.sheet_music.composer,
                    CoverUrl = favorite.sheet_music.cover_url,
                    SheetQuantity = favorite.sheet_music.sheet_quantity,
                    FavoriteCount = favorite.sheet_music.favorite_count,
                    Sheets = new List<SheetDto>() // Tránh circular reference
                });
            }
        }

        return new UserFavoriteSheetListDto
        {
            UserId = userId,
            UserName = user.account_name,
            FavoriteSheetMusics = favoriteSheetMusics
        };
    }

    public async Task<UserFavoriteSheetDto> AddAsync(CreateUserFavoriteSheetDto createDto)
    {
        // Kiểm tra user có tồn tại không
        var user = await _unitOfWork.Users.GetByIdAsync(createDto.UserId);
        if (user == null)
        {
            throw new NotFoundException("User", "Id", createDto.UserId);
        }

        // Kiểm tra sheet music có tồn tại không
        var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(createDto.SheetMusicId);
        if (sheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", createDto.SheetMusicId);
        }

        // Kiểm tra đã tồn tại favorite này chưa
        var existingFavorite = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(createDto.UserId, createDto.SheetMusicId);
        
        if (existingFavorite != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "UserFavoriteSheet", new string[] { "User đã yêu thích bài hát này rồi." } }
            });
        }

        var favoriteEntity = new user_favorite_sheet
        {
            user_id = createDto.UserId,
            sheet_music_id = createDto.SheetMusicId,
            is_favorite = createDto.IsFavorite ?? true
        };

        try
        {
            var addedFavorite = await _unitOfWork.UserFavoriteSheets.AddAsync(favoriteEntity);
            await _unitOfWork.CompleteAsync();
            
            // Cập nhật favorite_count của sheet_music
            sheetMusic.favorite_count = (sheetMusic.favorite_count ?? 0) + 1;
            await _unitOfWork.SheetMusics.UpdateAsync(sheetMusic);
            await _unitOfWork.CompleteAsync();
            
            return MapToUserFavoriteSheetDto(addedFavorite);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm favorite vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the favorite.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateUserFavoriteSheetDto updateDto)
    {
        var existingFavorite = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(updateDto.UserId, updateDto.SheetMusicId);
        
        if (existingFavorite == null)
        {
            throw new NotFoundException("UserFavoriteSheet", $"UserId={updateDto.UserId}, SheetMusicId={updateDto.SheetMusicId}", 0);
        }

        // Cập nhật trạng thái favorite
        if (updateDto.IsFavorite.HasValue)
        {
            existingFavorite.is_favorite = updateDto.IsFavorite.Value;
        }

        try
        {
            await _unitOfWork.UserFavoriteSheets.UpdateAsync(existingFavorite);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật favorite trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the favorite.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int userId, int sheetMusicId)
    {
        var favoriteToDelete = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
        
        if (favoriteToDelete == null)
        {
            throw new NotFoundException("UserFavoriteSheet", $"UserId={userId}, SheetMusicId={sheetMusicId}", 0);
        }

        try
        {
            await _unitOfWork.UserFavoriteSheets.Delete2ArgumentsAsync(userId, sheetMusicId);
            await _unitOfWork.CompleteAsync();
            
            // Giảm favorite_count của sheet_music
            var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
            if (sheetMusic != null && sheetMusic.favorite_count > 0)
            {
                sheetMusic.favorite_count = sheetMusic.favorite_count - 1;
                await _unitOfWork.SheetMusics.UpdateAsync(sheetMusic);
                await _unitOfWork.CompleteAsync();
            }
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa favorite khỏi cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the favorite.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task ToggleFavoriteAsync(int userId, int sheetMusicId)
    {
        var existingFavorite = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
        
        if (existingFavorite == null)
        {
            // Tạo mới favorite
            await AddAsync(new CreateUserFavoriteSheetDto
            {
                UserId = userId,
                SheetMusicId = sheetMusicId,
                IsFavorite = true
            });
        }
        else
        {
            // Toggle trạng thái
            existingFavorite.is_favorite = !(existingFavorite.is_favorite ?? false);
            
            try
            {
                await _unitOfWork.UserFavoriteSheets.UpdateAsync(existingFavorite);
                await _unitOfWork.CompleteAsync();
                
                // Cập nhật favorite_count của sheet_music
                var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
                if (sheetMusic != null)
                {
                    if (existingFavorite.is_favorite == true)
                    {
                        sheetMusic.favorite_count = (sheetMusic.favorite_count ?? 0) + 1;
                    }
                    else
                    {
                        sheetMusic.favorite_count = Math.Max(0, (sheetMusic.favorite_count ?? 0) - 1);
                    }
                    await _unitOfWork.SheetMusics.UpdateAsync(sheetMusic);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new ApiException("Có lỗi xảy ra khi toggle favorite trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                throw new ApiException("An unexpected error occurred while toggling the favorite.", ex, (int)HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<bool> IsFavoriteAsync(int userId, int sheetMusicId)
    {
        var favorite = await _unitOfWork.UserFavoriteSheets.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
        
        return favorite?.is_favorite == true;
    }

    private UserFavoriteSheetDto MapToUserFavoriteSheetDto(user_favorite_sheet model)
    {
        return new UserFavoriteSheetDto
        {
            UserId = model.user_id,
            SheetMusicId = model.sheet_music_id,
            IsFavorite = model.is_favorite,
            SheetMusic = model.sheet_music != null ? new SheetMusicDto
            {
                SheetMusicId = model.sheet_music.sheet_music_id,
                Number = model.sheet_music.number,
                MusicName = model.sheet_music.music_name,
                Composer = model.sheet_music.composer,
                CoverUrl = model.sheet_music.cover_url,
                SheetQuantity = model.sheet_music.sheet_quantity,
                FavoriteCount = model.sheet_music.favorite_count,
                Sheets = new List<SheetDto>() // Tránh circular reference
            } : null,
            User = model.user != null ? new UserDto
            {
                UserId = model.user.user_id,
                AccountName = model.user.account_name,
                Username = model.user.username,
                Email = model.user.email,
                PhoneNumber = model.user.phone_number,
                Address = model.user.address,
                Birthday = model.user.birthday,
                AvatarUrl = model.user.avatar_url,
                IsDisabled = model.user.is_disabled,
                CreateAt = model.user.create_at
            } : null
        };
    }
}