# Hướng dẫn Quản lý Lớp học và Học sinh

## Tổng quan

Hệ thống đã được cập nhật để hỗ trợ:
1. **Tự động tạo Class khi tạo Opening Schedule**
2. **Tự động generate Class Sessions cho thời khóa biểu**
3. **Kiểm soát giới hạn số học sinh trong lớp**
4. **Quản lý học sinh và giáo viên trong lớp**

## Cấu trúc Database Mới

### Bảng `class`
```sql
CREATE TABLE public.class (
    class_id integer NOT NULL,
    class_code text NOT NULL,
    instrument_id integer NOT NULL,
    total_students integer DEFAULT 0,      -- Giới hạn số học sinh
    current_students_count integer DEFAULT 0  -- Số học sinh hiện tại
);
```

### Bảng `user_class` (Many-to-Many)
```sql
CREATE TABLE public.user_class (
    user_id integer NOT NULL,
    class_id integer NOT NULL
);
```

### Cập nhật bảng `opening_schedule`
- Thêm `class_code`, `instrument_id`, `total_sessions`, `student_quantity`
- Liên kết với `instrument` và `teacher_user`

### Cập nhật bảng `class_session`
- Liên kết với `class`, `day`, `timeslot`, `room`
- Thêm `session_number` để đếm buổi học

## Workflow Tự động

### 1. Tạo Opening Schedule → Tự động tạo Class
```csharp
// Khi POST /api/OpeningSchedule
{
    "classCode": "PIANO_2024_01",
    "openingDay": "2024-01-15",
    "endDate": "2024-06-15",
    "studentQuantity": 10,         // Giới hạn số học sinh
    "isAdvancedClass": false,
    "teacherUserId": 5,
    "instrumentId": 1,             // Piano
    "totalSessions": 48,
    "selectedDayOfWeekIds": [1, 3, 5], // Thứ 2, 4, 6
    "defaultRoomId": 1,
    "timeSlotIds": [1, 2]          // 8:00-9:30, 9:45-11:15
}
```

**Tự động thực hiện:**
1. Tạo `opening_schedule` record
2. Tạo `class` record với:
   - `class_code = "PIANO_2024_01"`
   - `instrument_id = 1`
   - `total_students = 10`
   - `current_students_count = 0`
3. Tự động generate 48 `class_session` records cho thời khóa biểu

### 2. Auto-generate Class Sessions
Hệ thống tự động tạo class sessions dựa trên:
- Ngày bắt đầu và kết thúc
- Các ngày trong tuần được chọn
- Khung giờ học
- Tổng số buổi học (`totalSessions`)

## API Endpoints Mới

### Class Management

#### 1. Tạo Class mới
```http
POST /api/Class
{
    "classCode": "GUITAR_2024_01",
    "instrumentId": 2,
    "totalStudents": 15
}
```

#### 2. Cập nhật giới hạn học sinh
```http
PUT /api/Class/{id}
{
    "classId": 1,
    "totalStudents": 20
}
```

#### 3. Kiểm tra thông tin sức chứa lớp
```http
GET /api/Class/{classId}/student-capacity

Response:
{
    "classId": 1,
    "classCode": "PIANO_2024_01",
    "totalStudents": 10,
    "currentStudentsCount": 7,
    "availableSlots": 3,
    "isAtCapacity": false,
    "canAddStudents": true
}
```

#### 4. Kiểm tra khả năng thêm học sinh
```http
POST /api/Class/{classId}/check-can-add-students
[1001, 1002, 1003]  // Danh sách student IDs

Response:
{
    "classId": 1,
    "classCode": "PIANO_2024_01",
    "totalStudents": 10,
    "currentStudentsCount": 7,
    "studentsToAdd": 3,
    "canAdd": true,
    "message": "Có thể thêm 3 học sinh"
}
```

### Student Management

#### 1. Thêm học sinh vào lớp (với kiểm tra giới hạn)
```http
POST /api/Class/{classId}/add-users
{
    "classId": 1,
    "userIds": [1001, 1002]
}
```

**Validation:**
- Kiểm tra vai trò (chỉ Student hoặc Teacher)
- Kiểm tra giới hạn số học sinh
- Cập nhật `current_students_count`

#### 2. Gán toàn bộ danh sách user cho lớp
```http
POST /api/Class/{classId}/assign-users
{
    "classId": 1,
    "userIds": [1001, 1002, 1003, 2001] // Students + Teacher
}
```

#### 3. Xóa học sinh khỏi lớp
```http
POST /api/Class/{classId}/remove-users
{
    "classId": 1,
    "userIds": [1001]
}
```

## Quy tắc Kiểm soát Giới hạn

### 1. Thêm học sinh
- **Nếu `total_students = 0`**: Không giới hạn
- **Nếu `total_students > 0`**: Kiểm tra `current_students_count + new_students <= total_students`
- **Teachers**: Không tính vào giới hạn

### 2. Validation Messages
```json
{
    "StudentLimit": [
        "Không thể thêm 5 học sinh. Lớp hiện có 8 học sinh, giới hạn 10 học sinh. Chỉ có thể thêm tối đa 2 học sinh nữa."
    ]
}
```

### 3. Cập nhật giới hạn
- Không được đặt `total_students` < `current_students_count`
- Validation: "Không thể đặt giới hạn 5 học sinh. Lớp hiện đã có 8 học sinh."

## Migration Database

Chạy script migration:
```sql
-- Thực hiện migration
\i add_missing_tables.sql
```

Hoặc chạy từng bước:
1. Tạo các bảng mới (`class`, `user_class`, `instrument`, `timeslot`, `room`, `day`, `day_of_week_lookup`)
2. Cập nhật `opening_schedule` và `class_session`
3. Thêm foreign keys và constraints
4. Insert dữ liệu mẫu

## Sample Data

Hệ thống sẽ tự động thêm:
- **Instruments**: Piano, Guitar, Violin, Drums, Flute
- **Timeslots**: 5 khung giờ từ 8:00 đến 20:30
- **Rooms**: 5 phòng học với sức chứa khác nhau
- **Day of Week**: Thứ 2 → Chủ nhật
- **Role**: Teacher role

## Luồng sử dụng thực tế

### 1. Admin tạo Opening Schedule
```json
POST /api/OpeningSchedule
{
    "classCode": "PIANO_ADV_2024",
    "openingDay": "2024-02-01",
    "endDate": "2024-05-31",
    "studentQuantity": 8,
    "isAdvancedClass": true,
    "teacherUserId": 101,
    "instrumentId": 1,
    "totalSessions": 36,
    "selectedDayOfWeekIds": [1, 3, 5],
    "defaultRoomId": 1,
    "timeSlotIds": [1]
}
```

### 2. Hệ thống tự động tạo
- 1 Class record
- 36 Class Session records trong thời khóa biểu

### 3. Admin thêm học sinh
```json
POST /api/Class/1/add-users
{
    "classId": 1,
    "userIds": [201, 202, 203, 204, 205]
}
```

### 4. Kiểm tra tình trạng lớp
```http
GET /api/Class/1/student-capacity
```

### 5. Nếu cần thêm học sinh
```json
POST /api/Class/1/check-can-add-students
[206, 207, 208]
```

## Error Handling

### ValidationException
- Giới hạn học sinh vượt quá
- Class code trùng lặp
- Invalid role (không phải Student/Teacher)

### NotFoundException
- Class không tồn tại
- User không tồn tại
- Instrument/Room/Timeslot không tồn tại

### API Response Format
```json
{
    "error": "ValidationException",
    "message": "Validation errors occurred",
    "details": {
        "StudentLimit": ["Không thể thêm 3 học sinh..."]
    }
}
```