

-- Bảng cha (không phụ thuộc vào bảng khác hoặc chỉ phụ thuộc lẫn nhau)
INSERT INTO "role" ("role_name") VALUES
('Administrator'),
('Student');

INSERT INTO "statistic" ("date", "new_students", "monthly_revenue", "consultation_count", "total_students", "consultation_request_count") VALUES
('2024-01-01', 10, 1500.00, 5, 100, 12),
('2024-02-01', 8, 1200.50, 3, 108, 8);

INSERT INTO "consultation_topic" ("consultation_topic_name") VALUES
('Piano Lessons'),
('Guitar Lessons');

INSERT INTO "opening_schedule" ("subject", "class_code", "opening_day", "end_date", "schedule", "student_quantity", "is_advanced_class") VALUES
('Basic Piano', 'BP101', '2024-09-01', '2024-12-01', 'Mon/Wed 18:00', 15, FALSE),
('Advanced Guitar', 'AG202', '2024-10-01', '2025-01-01', 'Tue/Thu 19:00', 10, TRUE);

INSERT INTO "schedule" ("month_year", "note") VALUES
('2024-01-01', 'Personal schedule for John Doe'),
('2024-02-01', 'Personal schedule for Jane Smith');

INSERT INTO "instrument" ("instrument_name") VALUES
('Piano'),
('Guitar');

INSERT INTO "sheet" ("sheet_url") VALUES
('http://example.com/sheet1.pdf'),
('http://example.com/sheet2.pdf');

INSERT INTO "timeslot" ("start_time", "end_time") VALUES
('08:00:00', '09:00:00'),
('10:00:00', '11:00:00');

INSERT INTO "genre" ("genre_name") VALUES
('Classical'),
('Jazz');

-- Bảng có FK đến các bảng cha đã được chèn dữ liệu
-- "user" phụ thuộc vào "role", "statistic", "opening_schedule", "schedule"
INSERT INTO "user" ("username", "account_name", "password", "address", "phone_number", "avatar_url", "birthday", "role_id", "statistic_id", "opening_schedule_id", "schedule_id") VALUES
('john.doe', 'John Doe', 'hashed_pass_1', '123 Main St', '0901234567', 'http://example.com/avatar1.jpg', '1990-05-15', 2, 1, 1, 1),
('jane.smith', 'Jane Smith', 'hashed_pass_2', '456 Oak Ave', '0917654321', 'http://example.com/avatar2.jpg', '1992-11-20', 2, 2, 2, 2);

-- "document" phụ thuộc vào "instrument"
INSERT INTO "document" ("lesson", "lesson_name", "link", "instrument_id") VALUES
(1, 'Introduction to Piano', 'http://example.com/piano_intro.pdf', 1),
(5, 'Guitar Chords Basics', 'http://example.com/guitar_chords.pdf', 2);

-- "sheet_music" phụ thuộc vào "sheet"
INSERT INTO "sheet_music" ("number", "music_name", "composer", "cover_url", "sheet_quantity", "sheet_id") VALUES
(1, 'Für Elise', 'Beethoven', 'http://example.com/beethoven_cover.jpg', 5, 1),
(2, 'Canon in D', 'Pachelbel', 'http://example.com/pachelbel_cover.jpg', 3, 2);

-- "consultation_request" phụ thuộc vào "statistic", "consultation_topic"
INSERT INTO "consultation_request" ("fullname", "contact_number", "email", "note", "statistic_id", "consultation_topic_id") VALUES
('Alice Wonderland', '0987123456', 'alice@example.com', 'Interested in beginner piano lessons.', 1, 1),
('Bob The Builder', '0912345678', 'bob@example.com', 'Looking for advanced guitar techniques.', 2, 2);

-- "class" phụ thuộc vào "instrument"
INSERT INTO "class" ("class_code", "instrument_id") VALUES
('P-BEGIN-001', 1),
('G-ADV-001', 2);

-- "week" phụ thuộc vào "schedule"
INSERT INTO "week" ("week_number", "day_of_week", "schedule_id") VALUES
(1, '2024-01-01', 1),
(2, '2024-01-08', 2);

-- "class_session" phụ thuộc vào "week", "class", "timeslot"
INSERT INTO "class_session" ("session_number", "date", "room_code", "week_id", "class_id", "time_slot_id") VALUES
(1, '2024-01-01', 'Room A1', 1, 1, 1),
(2, '2024-01-08', 'Room B2', 2, 2, 2);

-- Bảng trung gian (sau khi tất cả các bảng tham chiếu của chúng đã có dữ liệu)
-- "user_favorite_sheet" phụ thuộc vào "user", "sheet_music"
INSERT INTO "user_favorite_sheet" ("user_id", "sheet_music_id", "is_favorite") VALUES
(1, 1, TRUE),
(2, 2, TRUE);

-- "sheet_music_genres" phụ thuộc vào "sheet_music", "genre"
INSERT INTO "sheet_music_genres" ("sheet_music_id", "genre_id") VALUES
(1, 1), -- sheet_music_id 1 (Für Elise), genre_id 1 (Classical)
(2, 2); -- sheet_music_id 2 (Canon in D), genre_id 2 (Jazz)

-- "user_doc" phụ thuộc vào "user", "document"
INSERT INTO "user_doc" ("user_id", "document_id") VALUES
(1, 1),
(2, 2);

-- "user_class" phụ thuộc vào "user", "class"
INSERT INTO "user_class" ("class_id", "user_id") VALUES
(1, 1),
(2, 2);

-- "attendance" phụ thuộc vào "user", "class_session"
INSERT INTO "attendance" ("status", "note", "user_id", "class_session_id") VALUES
(TRUE, 'Attended on time.', 1, 1),
(FALSE, 'Absent due to illness.', 2, 2);
