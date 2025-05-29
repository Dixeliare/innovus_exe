// using Repository.Basic.Repositories;
// using Repository.Models;
// using Services.IServices;
//
// namespace Services.Services;
//
// public class UserFavoriteSheetService : IUserFavoriteSheetService
//     {
//         private readonly UserFavoriteSheetRepository _userFavoriteSheetRepository;
//         private readonly SheetMusicRepository _sheetMusicRepository; // Có thể cần để lấy SheetMusic chi tiết
//         private readonly UserRepository _userRepository; // Có thể cần để lấy User chi tiết
//
//         public UserFavoriteSheetService(
//             UserFavoriteSheetRepository userFavoriteSheetRepository,
//             ISheetMusicRepository sheetMusicRepository, // Inject if needed
//             IUserRepository userRepository) // Inject if needed
//         {
//             _userFavoriteSheetRepository = userFavoriteSheetRepository;
//             _sheetMusicRepository = sheetMusicRepository;
//             _userRepository = userRepository;
//         }
//
//         // Phương thức thêm bản nhạc yêu thích
//         public async Task<bool> AddUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite = true)
//         {
//             // Kiểm tra xem user và sheet_music có tồn tại không
//             var userExists = await _userRepository.GetByIdAsync(userId) != null;
//             var sheetExists = await _sheetMusicRepository.GetByIdAsync(sheetMusicId) != null;
//
//             if (!userExists || !sheetExists)
//             {
//                 // Xử lý lỗi: User hoặc SheetMusic không tồn tại
//                 return false;
//             }
//
//             var existingEntry = await _userFavoriteSheetRepository.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
//             if (existingEntry != null)
//             {
//                 // Nếu đã tồn tại, cập nhật trạng thái
//                 existingEntry.is_favorite = isFavorite;
//                 return await _userFavoriteSheetRepository.UpdateAsync(existingEntry) > 0;
//             }
//             else
//             {
//                 // Nếu chưa tồn tại, thêm mới
//                 var newEntry = new user_favorite_sheet
//                 {
//                     user_id = userId,
//                     sheet_music_id = sheetMusicId,
//                     is_favorite = isFavorite
//                 };
//                 return await _userFavoriteSheetRepository.AddAsync(newEntry) > 0;
//             }
//         }
//
//         // Phương thức cập nhật trạng thái yêu thích
//         public async Task<bool> UpdateUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite)
//         {
//             var existingEntry = await _userFavoriteSheetRepository.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
//             if (existingEntry == null)
//             {
//                 // Không tìm thấy mối quan hệ để cập nhật
//                 return false;
//             }
//             existingEntry.is_favorite = isFavorite;
//             return await _userFavoriteSheetRepository.UpdateAsync(existingEntry) > 0;
//         }
//
//         // Phương thức xóa bản ghi yêu thích
//         public async Task<bool> DeleteUserFavoriteSheetAsync(int userId, int sheetMusicId)
//         {
//             return await _userFavoriteSheetRepository.DeleteAsync(userId, sheetMusicId) > 0;
//         }
//
//         // Lấy một bản ghi user_favorite_sheet cụ thể
//         public async Task<user_favorite_sheet?> GetUserFavoriteSheetEntryAsync(int userId, int sheetMusicId)
//         {
//             return await _userFavoriteSheetRepository.GetByUserAndSheetMusicIdAsync(userId, sheetMusicId);
//         }
//
//         // Lấy danh sách các bản nhạc yêu thích của một người dùng
//         public async Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync(int userId)
//         {
//             var favoriteEntries = await _userFavoriteSheetRepository.GetUserFavoriteSheetsAsync(userId);
//             // Chỉ trả về danh sách SheetMusic, không phải user_favorite_sheet entity
//             return favoriteEntries.Select(ufs => ufs.sheet_music).Where(sm => sm != null)!;
//         }
//
//         // Lấy danh sách người dùng yêu thích một bản nhạc
//         public async Task<IEnumerable<user>> GetUsersFavoritingSheetAsync(int sheetMusicId)
//         {
//             var favoritingEntries = await _userFavoriteSheetRepository.GetUsersWhoFavoritedSheetAsync(sheetMusicId);
//             // Chỉ trả về danh sách User, không phải user_favorite_sheet entity
//             return favoritingEntries.Select(ufs => ufs.user).Where(u => u != null)!;
//         }
//
//         // Kiểm tra xem bản nhạc có được yêu thích bởi người dùng không
//         public async Task<bool> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId)
//         {
//             return await _userFavoriteSheetRepository.IsSheetFavoriteForUserAsync(userId, sheetMusicId);
//         }
//     }