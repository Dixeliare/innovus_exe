-- Script để thay đổi quan hệ sheet_music và sheet từ 1-1 thành 1-n
-- Tạo bởi: Assistant
-- Ngày: 2025-08-05

-- Bước 1: Backup dữ liệu hiện tại
CREATE TABLE IF NOT EXISTS sheet_music_backup AS 
SELECT * FROM sheet_music;

CREATE TABLE IF NOT EXISTS sheet_backup AS 
SELECT * FROM sheet;

-- Bước 2: Xóa foreign key constraint hiện tại
ALTER TABLE sheet_music DROP CONSTRAINT IF EXISTS fk_sheet_music_sheet;

-- Bước 3: Xóa unique constraint trên sheet_id
ALTER TABLE sheet_music DROP CONSTRAINT IF EXISTS sheet_music_sheet_id_key;

-- Bước 4: Xóa cột sheet_id khỏi bảng sheet_music
ALTER TABLE sheet_music DROP COLUMN IF EXISTS sheet_id;

-- Bước 5: Thêm cột sheet_music_id vào bảng sheet
ALTER TABLE sheet ADD COLUMN IF NOT EXISTS sheet_music_id integer;

-- Bước 6: Tạo foreign key constraint mới từ sheet đến sheet_music
ALTER TABLE sheet ADD CONSTRAINT fk_sheet_sheet_music 
FOREIGN KEY (sheet_music_id) REFERENCES sheet_music(sheet_music_id) 
ON UPDATE CASCADE ON DELETE CASCADE;

-- Bước 7: Tạo index cho foreign key để tối ưu performance
CREATE INDEX IF NOT EXISTS idx_sheet_sheet_music_id ON sheet(sheet_music_id);

-- Bước 8: Cập nhật dữ liệu (nếu có dữ liệu hiện tại)
-- Lưu ý: Cần chạy script này cẩn thận nếu có dữ liệu
-- UPDATE sheet s 
-- SET sheet_music_id = sm.sheet_music_id 
-- FROM sheet_music sm 
-- WHERE s.sheet_id = sm.sheet_id;

-- Bước 9: Xóa bảng backup (sau khi đã test thành công)
-- DROP TABLE IF EXISTS sheet_music_backup;
-- DROP TABLE IF EXISTS sheet_backup;

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