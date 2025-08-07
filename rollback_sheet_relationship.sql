-- Script rollback để hoàn tác thay đổi quan hệ sheet_music và sheet từ 1-n về 1-1
-- Tạo bởi: Assistant
-- Ngày: 2025-08-05

-- Bước 1: Backup dữ liệu hiện tại (nếu chưa có)
CREATE TABLE IF NOT EXISTS sheet_music_backup_rollback AS 
SELECT * FROM sheet_music;

CREATE TABLE IF NOT EXISTS sheet_backup_rollback AS 
SELECT * FROM sheet;

-- Bước 2: Xóa foreign key constraint mới
ALTER TABLE sheet DROP CONSTRAINT IF EXISTS fk_sheet_sheet_music;

-- Bước 3: Xóa index mới
DROP INDEX IF EXISTS idx_sheet_sheet_music_id;

-- Bước 4: Xóa cột sheet_music_id khỏi bảng sheet
ALTER TABLE sheet DROP COLUMN IF EXISTS sheet_music_id;

-- Bước 5: Thêm lại cột sheet_id vào bảng sheet_music
ALTER TABLE sheet_music ADD COLUMN IF NOT EXISTS sheet_id integer;

-- Bước 6: Tạo lại foreign key constraint cũ
ALTER TABLE sheet_music ADD CONSTRAINT fk_sheet_music_sheet 
FOREIGN KEY (sheet_id) REFERENCES sheet(sheet_id) 
ON UPDATE CASCADE ON DELETE SET NULL;

-- Bước 7: Tạo lại unique constraint để đảm bảo quan hệ 1-1
ALTER TABLE sheet_music ADD CONSTRAINT sheet_music_sheet_id_key 
UNIQUE (sheet_id);

-- Bước 8: Cập nhật dữ liệu (nếu có dữ liệu)
-- Lưu ý: Cần chạy script này cẩn thận nếu có dữ liệu
-- UPDATE sheet_music sm 
-- SET sheet_id = s.sheet_id 
-- FROM sheet s 
-- WHERE s.sheet_music_id = sm.sheet_music_id;

-- Bước 9: Xóa bảng backup rollback (sau khi đã test thành công)
-- DROP TABLE IF EXISTS sheet_music_backup_rollback;
-- DROP TABLE IF EXISTS sheet_backup_rollback;

-- Kiểm tra kết quả
SELECT 
    'sheet_music' as table_name,
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_name = 'sheet_music' 
ORDER BY ordinal_position;

SELECT 
    'sheet' as table_name,
    column_name,
    data_type
FROM information_schema.columns 
WHERE table_name = 'sheet' 
ORDER BY ordinal_position;

-- Kiểm tra foreign key constraints
SELECT 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
  AND (tc.table_name = 'sheet' OR tc.table_name = 'sheet_music'); 