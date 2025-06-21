using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class UserFavoriteSheetService : IUserFavoriteSheetService
    {
        private readonly IUserFavoriteSheetRepository _userFavoriteSheetRepository;
        private readonly ISheetMusicRepository _sheetMusicRepository; // Có thể cần để lấy SheetMusic chi tiết
        private readonly IUserRepository _userRepository; // Có thể cần để lấy User chi tiết

        public UserFavoriteSheetService(
            IUserFavoriteSheetRepository userFavoriteSheetRepository,
            ISheetMusicRepository sheetMusicRepository, // Inject if needed
            IUserRepository userRepository) // Inject if needed
        {
            _userFavoriteSheetRepository = userFavoriteSheetRepository;
            _sheetMusicRepository = sheetMusicRepository;
            _userRepository = userRepository;
        }

        // Phương thức thêm bản nhạc yêu thích
        public async Task<UserFavoriteSheetDto> AddUserFavoriteSheetAsync(CreateUserFavoriteSheetDto createDto)
        {
            // Kiểm tra sự tồn tại của User và SheetMusic
            var userExists = await _userRepository.GetByIdAsync(createDto.UserId);
            if (userExists == null)
            {
                throw new KeyNotFoundException($"User with ID {createDto.UserId} not found.");
            }

            var sheetMusicExists = await _sheetMusicRepository.GetByIdAsync(createDto.SheetMusicId);
            if (sheetMusicExists == null)
            {
                throw new KeyNotFoundException($"Sheet Music with ID {createDto.SheetMusicId} not found.");
            }

            // Kiểm tra xem cặp đã tồn tại chưa
            if (await _userFavoriteSheetRepository.IsSheetFavoriteForUserAsync(createDto.UserId, createDto.SheetMusicId))
            {
                throw new InvalidOperationException($"User {createDto.UserId} has already favorited Sheet Music {createDto.SheetMusicId}.");
            }

            var entity = new user_favorite_sheet
            {
                user_id = createDto.UserId,
                sheet_music_id = createDto.SheetMusicId,
                is_favorite = createDto.IsFavorite ?? true // Mặc định là true nếu không cung cấp
            };

            var addedEntity = await _userFavoriteSheetRepository.AddAsync(entity);
            return MapToUserFavoriteSheetDto(addedEntity);
        }

        // Update User Favorite Sheet (primarily for is_favorite status)
        public async Task UpdateUserFavoriteSheetAsync(UpdateUserFavoriteSheetDto updateDto)
        {
            var existingEntity = await _userFavoriteSheetRepository.GetByIdAsync(updateDto.UserId, updateDto.SheetMusicId);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"User Favorite Sheet with User ID {updateDto.UserId} and Sheet Music ID {updateDto.SheetMusicId} not found.");
            }

            existingEntity.is_favorite = updateDto.IsFavorite;

            await _userFavoriteSheetRepository.UpdateAsync(existingEntity);
        }

        // Phương thức xóa bản ghi yêu thích
        public async Task<bool> DeleteUserFavoriteSheetAsync(int userId, int sheetMusicId)
        {
            return await _userFavoriteSheetRepository.Delete2ArgumentsAsync(userId, sheetMusicId) > 0;
        }
        
        public async Task<UserFavoriteSheetDto?> GetByIdAsync(int userId, int sheetMusicId)
        {
            var userFavoriteSheet = await _userFavoriteSheetRepository.GetByIdAsync(userId, sheetMusicId);
            return userFavoriteSheet != null ? MapToUserFavoriteSheetDto(userFavoriteSheet) : null;
        }

        // Lấy một bản ghi user_favorite_sheet cụ thể
        public async Task<user_favorite_sheet?> GetUserFavoriteSheetEntryAsync(int userId, int sheetMusicId)
        {
            return await _userFavoriteSheetRepository.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
        }

        // Lấy danh sách các bản nhạc yêu thích của một người dùng
        public async Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync(int userId)
        {
            var favoriteEntries = await _userFavoriteSheetRepository.GetUserFavoriteSheetsAsync(userId);
            // Chỉ trả về danh sách SheetMusic, không phải user_favorite_sheet entity
            return favoriteEntries.Select(ufs => ufs.sheet_music).Where(sm => sm != null)!;
        }

        // Lấy danh sách người dùng yêu thích một bản nhạc
        public async Task<IEnumerable<user>> GetUsersFavoritingSheetAsync(int sheetMusicId)
        {
            var favoritingEntries = await _userFavoriteSheetRepository.GetUsersWhoFavoritedSheetAsync(sheetMusicId);
            // Chỉ trả về danh sách User, không phải user_favorite_sheet entity
            return favoritingEntries.Select(ufs => ufs.user).Where(u => u != null)!;
        }

        // Kiểm tra xem bản nhạc có được yêu thích bởi người dùng không
        public async Task<bool> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId)
        {
            return await _userFavoriteSheetRepository.IsSheetFavoriteForUserAsync(userId, sheetMusicId);
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