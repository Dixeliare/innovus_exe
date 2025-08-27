# API Authorization Summary

## UserController
- `GET /api/User` - **[Authorize(Roles = "1,2")]** - Admin, Manager xem danh sách users
- `GET /api/User/{id}` - **[Authorize(Roles = "1,2")]** - Admin, Manager xem user theo ID
- `GET /api/User/username/{username}` - **[Authorize(Roles = "1,2")]** - Admin, Manager xem user theo username
- `GET /api/User/profile` - **[Authorize]** - User đã đăng nhập xem profile của mình
- `GET /api/User/my-profile` - **[Authorize]** - User đã đăng nhập xem profile của mình
- `POST /api/User` - **[Authorize(Roles = "1")]** - Chỉ Admin tạo user mới
- `PUT /api/User/{id}` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher cập nhật user
- `DELETE /api/User/{id}` - **[Authorize(Roles = "1")]** - Chỉ Admin xóa user
- `GET /api/User/search` - **[Authorize(Roles = "1,2")]** - Admin, Manager tìm kiếm user
- `GET /api/User/personal-schedule` - **[Authorize]** - User đã đăng nhập xem lịch cá nhân

## UserFavoriteSheetController
- `GET /api/UserFavoriteSheet/my-favorites` - **[Authorize]** - User đã đăng nhập xem favorites
- `GET /api/UserFavoriteSheet/check-my-favorite/{sheetMusicId}` - **[Authorize]** - User đã đăng nhập kiểm tra favorite
- `POST /api/UserFavoriteSheet/toggle/{sheetMusicId}` - **[Authorize]** - User đã đăng nhập toggle favorite

## AttendanceController
- **Tất cả endpoints** - **[Authorize]** - Yêu cầu đăng nhập để truy cập

## ClassController
- `GET /api/Class` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher xem danh sách lớp
- `GET /api/Class/{id}` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher xem lớp theo ID
- `GET /api/Class/search_by_instrument_id_or_class_code` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher tìm kiếm lớp
- `POST /api/Class` - **[Authorize(Roles = "1,2")]** - Admin, Manager tạo lớp mới
- `PUT /api/Class/{id}` - **[Authorize(Roles = "1,2")]** - Admin, Manager cập nhật lớp
- `DELETE /api/Class/{id}` - **[Authorize(Roles = "1,2")]** - Admin, Manager xóa lớp

## ScheduleController
- `GET /api/Schedule` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher xem lịch trình
- `GET /api/Schedule/search_id_or_note` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher tìm kiếm lịch
- `GET /api/Schedule/search_month_or_year` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher tìm kiếm lịch theo tháng/năm
- `GET /api/Schedule/{id}` - **[Authorize(Roles = "1,2,3")]** - Admin, Manager, Teacher xem lịch theo ID
- `POST /api/Schedule` - **[Authorize(Roles = "1,2")]** - Admin, Manager tạo lịch mới
- `PUT /api/Schedule/{id}` - **[Authorize(Roles = "1,2")]** - Admin, Manager cập nhật lịch

## StatisticController
- **Tất cả endpoints** - **[Authorize(Roles = "1,2")]** - Chỉ Admin và Manager mới được truy cập

## API Public (Không cần Authorization)
- `POST /api/User/Login` - Đăng nhập (không cần token)

## Role Mapping
- **Role 1**: Admin - Có quyền truy cập tất cả
- **Role 2**: Manager - Có quyền quản lý nhưng không xóa user
- **Role 3**: Teacher - Chỉ xem và cập nhật thông tin cơ bản

## Lưu ý
- Tất cả API có `[Authorize]` đều yêu cầu JWT token hợp lệ
- API có `[Authorize(Roles = "...")]` yêu cầu user có role tương ứng
- API không có `[Authorize]` có thể truy cập mà không cần authentication



