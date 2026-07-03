-- Safe migration: Add missing columns and tables (idempotent)

-- Step 1: Create helper procedure if not exists
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;
DELIMITER //
CREATE PROCEDURE AddColumnIfNotExists(
    IN p_table VARCHAR(255),
    IN p_column VARCHAR(255),
    IN p_type VARCHAR(255)
)
BEGIN
    DECLARE col_count INT;
    SELECT COUNT(*) INTO col_count
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = p_table
      AND COLUMN_NAME = p_column;
    IF col_count = 0 THEN
        SET @sql = CONCAT('ALTER TABLE ', p_table, ' ADD COLUMN ', p_column, ' ', p_type);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

-- Step 2: Add columns to attendance_sessions
CALL AddColumnIfNotExists('attendance_sessions', 'status', 'VARCHAR(20) NOT NULL DEFAULT ''ACTIVE''');
CALL AddColumnIfNotExists('attendance_sessions', 'room_id', 'BIGINT NULL');
CALL AddColumnIfNotExists('attendance_sessions', 'section_name', 'VARCHAR(255) NULL');
CALL AddColumnIfNotExists('attendance_sessions', 'subject_name', 'VARCHAR(255) NULL');

-- Step 3: Add columns to attendance_records
CALL AddColumnIfNotExists('attendance_records', 'hash', 'VARCHAR(255) NULL');

-- Step 4: Add columns to class_schedules
CALL AddColumnIfNotExists('class_schedules', 'room_id', 'BIGINT NULL');

-- Step 5: Add columns to users
CALL AddColumnIfNotExists('users', 'name', 'VARCHAR(255) NOT NULL DEFAULT '''' ');

-- Step 6: Create qr_tokens table if not exists
CREATE TABLE IF NOT EXISTS qr_tokens (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    token VARCHAR(255) NOT NULL,
    model_type VARCHAR(50) NOT NULL,
    model_id BIGINT NOT NULL,
    borrow_purpose VARCHAR(255) NULL,
    is_revoked TINYINT(1) NOT NULL DEFAULT 0,
    expires_at DATETIME NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE INDEX idx_qr_tokens_token (token)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Step 7: Clean up
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;
