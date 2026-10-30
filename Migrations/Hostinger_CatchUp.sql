-- ─────────────────────────────────────────────────────────────────────────────
-- Hostinger catch-up migration (idempotent — safe to run more than once).
--
-- Brings a Hostinger / older MySQL/MariaDB database up to the schema the app
-- currently expects. Fixes "Unknown column 'u.name' in 'SELECT'" (missing
-- users.name) and every other column/table added by later migrations.
--
-- HOW TO RUN: Hostinger hPanel → Databases → phpMyAdmin → select the
-- "u621755393_CSchoolManagen" database → SQL tab → paste this whole file → Go.
-- ─────────────────────────────────────────────────────────────────────────────

-- Helper: add a column only when it does not already exist.
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

-- Helper: run arbitrary SQL only when a given column exists. Needed for the
-- audit_logs backfill below, which reads legacy columns that may not be there.
DROP PROCEDURE IF EXISTS RunIfColumnExists;
DELIMITER //
CREATE PROCEDURE RunIfColumnExists(
    IN p_table VARCHAR(255),
    IN p_column VARCHAR(255),
    IN p_sql TEXT
)
BEGIN
    DECLARE col_count INT;
    SELECT COUNT(*) INTO col_count
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = p_table
      AND COLUMN_NAME = p_column;
    IF col_count > 0 THEN
        SET @sql = p_sql;
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

-- Helper: create an index only when it does not already exist (CREATE INDEX has no
-- portable IF NOT EXISTS across MySQL 8 and MariaDB).
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;
DELIMITER //
CREATE PROCEDURE AddIndexIfNotExists(
    IN p_table VARCHAR(255),
    IN p_index VARCHAR(255),
    IN p_cols VARCHAR(255)
)
BEGIN
    DECLARE idx_count INT;
    SELECT COUNT(*) INTO idx_count
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = p_table
      AND INDEX_NAME = p_index;
    IF idx_count = 0 THEN
        SET @sql = CONCAT('CREATE INDEX ', p_index, ' ON ', p_table, ' (', p_cols, ')');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

-- ── users ──  (this is the column the login error is about)
CALL AddColumnIfNotExists('users', 'name', 'VARCHAR(255) NOT NULL DEFAULT '''' ');
UPDATE users SET name = username WHERE name = '' OR name IS NULL;

-- ── attendance_sessions ──
CALL AddColumnIfNotExists('attendance_sessions', 'status', 'VARCHAR(20) NOT NULL DEFAULT ''ACTIVE''');
CALL AddColumnIfNotExists('attendance_sessions', 'room_id', 'BIGINT NULL');
CALL AddColumnIfNotExists('attendance_sessions', 'section_name', 'VARCHAR(255) NULL');
CALL AddColumnIfNotExists('attendance_sessions', 'subject_name', 'VARCHAR(255) NULL');

-- ── attendance_records ──
CALL AddColumnIfNotExists('attendance_records', 'hash', 'VARCHAR(255) NULL');

-- ── class_schedules ──
CALL AddColumnIfNotExists('class_schedules', 'room_id', 'BIGINT NULL');

-- ── qr_tokens (table) ──
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

-- ── room_qr_tbl (table) ──
CREATE TABLE IF NOT EXISTS room_qr_tbl (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    room_id BIGINT NOT NULL,
    qr_content TEXT NOT NULL,
    qr_code LONGBLOB NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE INDEX idx_room_qr_room_id (room_id),
    CONSTRAINT fk_room_qr_room FOREIGN KEY (room_id) REFERENCES rooms(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── leave_requests (table + later category column) ──
CREATE TABLE IF NOT EXISTS leave_requests (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    TeacherId BIGINT NOT NULL,
    reason TEXT NOT NULL,
    category VARCHAR(50) NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    ReviewedByUserId BIGINT NULL,
    review_notes TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_leave_requests_teacher FOREIGN KEY (TeacherId) REFERENCES teachers(Id) ON DELETE CASCADE,
    CONSTRAINT fk_leave_requests_reviewed_by FOREIGN KEY (ReviewedByUserId) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- Older leave_requests tables may predate the category column.
CALL AddColumnIfNotExists('leave_requests', 'category', 'VARCHAR(50) NULL');

-- ── audit_logs (table) ──
-- Created only if missing so existing log rows are preserved.
CREATE TABLE IF NOT EXISTS audit_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    actor_id BIGINT NOT NULL,
    actor_name VARCHAR(100) NOT NULL,
    actor_role VARCHAR(20) NOT NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id BIGINT NULL,
    summary VARCHAR(500) NOT NULL,
    metadata JSON NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_audit_created (created_at),
    INDEX idx_audit_actor (actor_id),
    INDEX idx_audit_action (action),
    INDEX idx_audit_entity (entity_type, entity_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── audit_logs: upgrade a pre-existing legacy table ──
-- CREATE TABLE IF NOT EXISTS above deliberately leaves an existing audit_logs alone
-- so its rows survive — but an older table uses the pre-rename column set
-- (user_id / entity / payload) and is missing actor_name, actor_role and summary
-- entirely. Without this block the app still fails on every audit read and write.
-- Everything here is additive: no legacy column is dropped.
CALL AddColumnIfNotExists('audit_logs', 'actor_id',    'BIGINT NOT NULL DEFAULT 0');
CALL AddColumnIfNotExists('audit_logs', 'actor_name',  'VARCHAR(100) NOT NULL DEFAULT '''' ');
CALL AddColumnIfNotExists('audit_logs', 'actor_role',  'VARCHAR(20) NOT NULL DEFAULT '''' ');
CALL AddColumnIfNotExists('audit_logs', 'entity_type', 'VARCHAR(50) NOT NULL DEFAULT '''' ');
CALL AddColumnIfNotExists('audit_logs', 'summary',     'VARCHAR(500) NOT NULL DEFAULT '''' ');
CALL AddColumnIfNotExists('audit_logs', 'metadata',    'JSON NULL');

-- Carry legacy values across. Each UPDATE is guarded on its own emptiness check, so
-- re-running the file touches nothing.
CALL RunIfColumnExists('audit_logs', 'user_id',
    'UPDATE audit_logs SET actor_id = user_id WHERE actor_id = 0 AND user_id IS NOT NULL');
CALL RunIfColumnExists('audit_logs', 'entity',
    'UPDATE audit_logs SET entity_type = LEFT(entity, 50) WHERE entity_type = '''' AND entity IS NOT NULL');
CALL RunIfColumnExists('audit_logs', 'payload',
    'UPDATE audit_logs SET metadata = payload WHERE metadata IS NULL AND payload IS NOT NULL AND JSON_VALID(payload)');

UPDATE audit_logs a JOIN users u ON u.Id = a.actor_id
   SET a.actor_name = LEFT(u.name, 100), a.actor_role = LEFT(u.role, 20)
 WHERE a.actor_name = '';
UPDATE audit_logs SET actor_name = CONCAT('User#', actor_id) WHERE actor_name = '';
UPDATE audit_logs SET actor_role = 'UNKNOWN' WHERE actor_role = '';
UPDATE audit_logs SET summary = LEFT(CONCAT(action, ' ', COALESCE(entity_type, '')), 500) WHERE summary = '';

-- The legacy columns are NOT NULL with no default, and the app never populates them —
-- every future audit INSERT would be rejected. Relax them rather than dropping.
CALL RunIfColumnExists('audit_logs', 'user_id', 'ALTER TABLE audit_logs MODIFY COLUMN user_id BIGINT NULL');
CALL RunIfColumnExists('audit_logs', 'entity',  'ALTER TABLE audit_logs MODIFY COLUMN entity LONGTEXT NULL');

-- Older tables typed `action` as LONGTEXT; idx_audit_action cannot index that without a
-- prefix length, and the model declares VARCHAR(50).
DROP PROCEDURE IF EXISTS NarrowAuditAction;
DELIMITER //
CREATE PROCEDURE NarrowAuditAction()
BEGIN
    DECLARE dtype VARCHAR(64);
    SELECT DATA_TYPE INTO dtype
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'audit_logs'
      AND COLUMN_NAME = 'action';
    IF dtype IS NOT NULL AND dtype <> 'varchar' THEN
        ALTER TABLE audit_logs MODIFY COLUMN action VARCHAR(50) NOT NULL;
    END IF;
END//
DELIMITER ;
CALL NarrowAuditAction();
DROP PROCEDURE IF EXISTS NarrowAuditAction;

CALL AddIndexIfNotExists('audit_logs', 'idx_audit_created', 'created_at');
CALL AddIndexIfNotExists('audit_logs', 'idx_audit_actor',   'actor_id');
CALL AddIndexIfNotExists('audit_logs', 'idx_audit_action',  'action');
CALL AddIndexIfNotExists('audit_logs', 'idx_audit_entity',  'entity_type, entity_id');

-- ── student_excuse_requests (table) ──
CREATE TABLE IF NOT EXISTS student_excuse_requests (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    student_id BIGINT NOT NULL,
    reason TEXT NOT NULL,
    category VARCHAR(50) NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    reviewed_by_user_id BIGINT NULL,
    review_notes TEXT NULL,
    attachment LONGBLOB NULL,
    attachment_name VARCHAR(255) NULL,
    attachment_type VARCHAR(100) NULL,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NOT NULL,
    INDEX idx_excuse_student (student_id),
    INDEX idx_excuse_status (status),
    CONSTRAINT fk_excuse_student FOREIGN KEY (student_id) REFERENCES students(Id) ON DELETE CASCADE,
    CONSTRAINT fk_excuse_reviewer FOREIGN KEY (reviewed_by_user_id) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ── app_settings (table) ──
CREATE TABLE IF NOT EXISTS app_settings (
    setting_key VARCHAR(100) NOT NULL PRIMARY KEY,
    setting_value TEXT NOT NULL,
    updated_by_user_id BIGINT NULL,
    updated_at DATETIME(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Clean up helpers.
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;
DROP PROCEDURE IF EXISTS RunIfColumnExists;
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;
