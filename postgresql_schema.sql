-- **************************************************************************
-- Bước 1: Xóa Database cũ (Nếu tồn tại) và Tạo Database mới
-- LƯU Ý QUAN TRỌNG:
-- Bạn cần chạy đoạn này khi đang kết nối đến database 'postgres' hoặc một database khác,
-- KHÔNG phải 'innovus_db' nếu nó đang tồn tại.
-- Sau khi chạy xong, bạn sẽ cần kết nối lại vào database "innovus_db" mới tạo để tiếp tục.
-- **************************************************************************

DROP DATABASE IF EXISTS innovus_db;
CREATE DATABASE innovus_db;

-- **************************************************************************
-- HẾT BƯỚC 1.
-- Bây giờ, HÃY KẾT NỐI LẠI VÀO DATABASE "innovus_db" MỚI TẠO của bạn
-- (Ví dụ: Trong psql dùng lệnh \c innovus_db; hoặc chọn database trong pgAdmin/DBeaver).
-- SAU ĐÓ, copy và chạy toàn bộ các lệnh từ dòng dưới đây trở đi.
-- **************************************************************************


-- Phần 2: Định nghĩa các bảng và khóa ngoại trong innovus_db

-- Tạo các bảng chính trước (bảng cha)
-- Chú ý: Bảng "schedule" phải được tạo trước bảng "user" vì user sẽ tham chiếu schedule.
CREATE TABLE "schedule" (
	"schedule_id" SERIAL PRIMARY KEY,
	"month_year" DATE, -- Cân nhắc DATE nếu chỉ cần tháng/năm và luôn là ngày 1
	"note" TEXT
);

CREATE TABLE "user" (
	"user_id" SERIAL PRIMARY KEY,
	"username" VARCHAR(255) UNIQUE,
	"account_name" TEXT,
	"password" VARCHAR(255) NOT NULL,
	"address" TEXT,
	"phone_number" VARCHAR(255),
	"is_disabled" BOOLEAN DEFAULT FALSE,
	"create_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	"avatar_url" TEXT,
	"birthday" DATE,
	"role_id" INTEGER, -- Sẽ là FK
	"statistic_id" INTEGER, -- Sẽ là FK
	"opening_schedule_id" INTEGER, -- Sẽ là FK
    "schedule_id" INTEGER UNIQUE -- ĐÃ THÊM CỘT NÀY CHO QUAN HỆ 1-1 VỚI "schedule"
);

CREATE TABLE "sheet" (
	"sheet_id" SERIAL PRIMARY KEY,
	"sheet_url" TEXT NOT NULL
);

CREATE TABLE "genre" (
	"genre_id" SERIAL PRIMARY KEY,
	"genre_name" VARCHAR(255) UNIQUE
);

CREATE TABLE "instrument" (
	"instrument_id" SERIAL PRIMARY KEY,
	"instrument_name" VARCHAR(255) UNIQUE
);

CREATE TABLE "role" (
	"role_id" SERIAL PRIMARY KEY,
	"role_name" VARCHAR(255) UNIQUE
);

CREATE TABLE "statistic" (
	"statistic_id" SERIAL PRIMARY KEY,
	"date" DATE,
	"new_students" INTEGER DEFAULT 0,
	"monthly_revenue" NUMERIC(10, 2) DEFAULT 0.00,
	"consultation_count" INTEGER DEFAULT 0,
	"total_students" INTEGER DEFAULT 0,
	"consultation_request_count" INTEGER DEFAULT 0
);

CREATE TABLE "consultation_topic" (
	"consultation_topic_id" SERIAL PRIMARY KEY,
	"consultation_topic_name" VARCHAR(255) UNIQUE
);

CREATE TABLE "opening_schedule" (
	"opening_schedule_id" SERIAL PRIMARY KEY,
	"subject" VARCHAR(255),
	"class_code" VARCHAR(255) UNIQUE,
	"opening_day" DATE,
	"end_date" DATE,
	"schedule" VARCHAR(255),
	"student_quantity" INTEGER DEFAULT 0,
	"is_advanced_class" BOOLEAN DEFAULT FALSE
);

CREATE TABLE "class" (
	"class_id" SERIAL PRIMARY KEY,
	"class_code" VARCHAR(255) UNIQUE,
	"instrument_id" INTEGER NOT NULL
);

CREATE TABLE "timeslot" (
	"timeslot_id" SERIAL PRIMARY KEY,
	"start_time" TIME NOT NULL,
	"end_time" TIME NOT NULL,
	UNIQUE ("start_time", "end_time")
);


-- Tạo các bảng có khóa ngoại đơn lẻ
CREATE TABLE "document" (
	"document_id" SERIAL PRIMARY KEY,
	"lesson" INTEGER,
	"lesson_name" TEXT,
	"link" TEXT NOT NULL,
	"instrument_id" INTEGER NOT NULL,
	CONSTRAINT fk_document_instrument
	    FOREIGN KEY ("instrument_id") REFERENCES "instrument"("instrument_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE TABLE "sheet_music" (
	"sheet_music_id" SERIAL PRIMARY KEY,
	"number" INTEGER,
	"music_name" VARCHAR(255),
	"composer" VARCHAR(255) NOT NULL,
	"cover_url" TEXT NOT NULL,
	"sheet_quantity" INTEGER,
	"favorite_count" INTEGER DEFAULT 0,
	"sheet_id" INTEGER UNIQUE,
	CONSTRAINT fk_sheet_music_sheet
	    FOREIGN KEY ("sheet_id") REFERENCES "sheet"("sheet_id")
	    ON UPDATE CASCADE ON DELETE SET NULL
);

CREATE TABLE "consultation_request" (
	"consultation_request_id" SERIAL PRIMARY KEY,
	"fullname" VARCHAR(255),
	"contact_number" TEXT,
	"email" VARCHAR(255) NOT NULL,
	"note" TEXT NOT NULL,
	"has_contact" BOOLEAN DEFAULT FALSE,
	"statistic_id" INTEGER,
	"consultation_topic_id" INTEGER,
	CONSTRAINT fk_consultation_request_statistic
	    FOREIGN KEY ("statistic_id") REFERENCES "statistic"("statistic_id")
	    ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT fk_consultation_request_topic
	    FOREIGN KEY ("consultation_topic_id") REFERENCES "consultation_topic"("consultation_topic_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT
);


-- Tạo các bảng trung gian (Junction Tables)
CREATE TABLE "user_favorite_sheet" (
	"user_id" INTEGER NOT NULL,
	"sheet_music_id" INTEGER NOT NULL,
	"is_favorite" BOOLEAN DEFAULT TRUE,
	PRIMARY KEY("user_id", "sheet_music_id"),
	CONSTRAINT fk_user_favorite_sheet_user
	    FOREIGN KEY ("user_id") REFERENCES "user"("user_id")
	    ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT fk_user_favorite_sheet_sheet_music
	    FOREIGN KEY ("sheet_music_id") REFERENCES "sheet_music"("sheet_music_id")
	    ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE "sheet_music_genres" (
	"sheet_music_id" INTEGER NOT NULL,
	"genre_id" INTEGER NOT NULL,
	PRIMARY KEY("sheet_music_id", "genre_id"),
	CONSTRAINT fk_sheet_music_genres_sheet_music
	    FOREIGN KEY ("sheet_music_id") REFERENCES "sheet_music"("sheet_music_id")
	    ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT fk_sheet_music_genres_genre
	    FOREIGN KEY ("genre_id") REFERENCES "genre"("genre_id")
	    ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE "user_doc" (
	"user_id" INTEGER NOT NULL,
	"document_id" INTEGER NOT NULL,
	PRIMARY KEY("user_id", "document_id"),
	CONSTRAINT fk_user_doc_user
	    FOREIGN KEY ("user_id") REFERENCES "user"("user_id")
	    ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT fk_user_doc_document
	    FOREIGN KEY ("document_id") REFERENCES "document"("document_id")
	    ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE "user_class" (
	"class_id" INTEGER NOT NULL,
	"user_id" INTEGER NOT NULL,
	PRIMARY KEY("class_id", "user_id"),
	CONSTRAINT fk_user_class_class
	    FOREIGN KEY ("class_id") REFERENCES "class"("class_id")
	    ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT fk_user_class_user
	    FOREIGN KEY ("user_id") REFERENCES "user"("user_id")
	    ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE "week" (
	"week_id" SERIAL PRIMARY KEY,
	"week_number" INTEGER,
	"day_of_week" DATE,
	"schedule_id" INTEGER NOT NULL,
	CONSTRAINT fk_week_schedule
	    FOREIGN KEY ("schedule_id") REFERENCES "schedule"("schedule_id")
	    ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE "class_session" (
	"class_session_id" SERIAL PRIMARY KEY,
	"session_number" INTEGER,
	"date" DATE,
	"room_code" VARCHAR(255) NOT NULL,
	"week_id" INTEGER NOT NULL,
	"class_id" INTEGER NOT NULL,
	"time_slot_id" INTEGER NOT NULL,
	CONSTRAINT fk_class_session_week
	    FOREIGN KEY ("week_id") REFERENCES "week"("week_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT,
	CONSTRAINT fk_class_session_class
	    FOREIGN KEY ("class_id") REFERENCES "class"("class_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT,
	CONSTRAINT fk_class_session_timeslot
	    FOREIGN KEY ("time_slot_id") REFERENCES "timeslot"("timeslot_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE TABLE "attendance" (
	"attendance_id" SERIAL PRIMARY KEY,
	"status" BOOLEAN,
	"check_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	"note" TEXT,
	"user_id" INTEGER NOT NULL,
	"class_session_id" INTEGER NOT NULL,
	CONSTRAINT fk_attendance_user
	    FOREIGN KEY ("user_id") REFERENCES "user"("user_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT,
	CONSTRAINT fk_attendance_class_session
	    FOREIGN KEY ("class_session_id") REFERENCES "class_session"("class_session_id")
	    ON UPDATE CASCADE ON DELETE RESTRICT
);


-- Thêm các khóa ngoại còn lại cho bảng "user"
-- Đặt ALTER TABLE sau khi tất cả các bảng đã được tạo.
ALTER TABLE "user"
ADD CONSTRAINT fk_user_role
    FOREIGN KEY("role_id") REFERENCES "role"("role_id")
    ON UPDATE CASCADE ON DELETE RESTRICT;

ALTER TABLE "user"
ADD CONSTRAINT fk_user_statistic
    FOREIGN KEY("statistic_id") REFERENCES "statistic"("statistic_id")
    ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE "user"
ADD CONSTRAINT fk_user_opening_schedule
    FOREIGN KEY("opening_schedule_id") REFERENCES "opening_schedule"("opening_schedule_id")
    ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE "user"
ADD CONSTRAINT fk_user_schedule_personal -- ĐÃ THÊM KHÓA NGOẠI CHO QUAN HỆ 1-1 VỚI "schedule"
    FOREIGN KEY("schedule_id") REFERENCES "schedule"("schedule_id")
    ON UPDATE CASCADE ON DELETE SET NULL; -- SET NULL nếu schedule bị xóa, hoặc RESTRICT
