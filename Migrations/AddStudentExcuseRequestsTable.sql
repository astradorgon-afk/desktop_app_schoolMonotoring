-- Migration: Add student_excuse_requests table (idempotent)
-- The app also creates this table on first use when its database user has CREATE
-- rights; run this by hand (e.g. in phpMyAdmin) where it does not.
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
