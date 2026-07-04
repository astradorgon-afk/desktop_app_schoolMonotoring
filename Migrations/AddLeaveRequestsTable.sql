-- Migration: Add leave_requests table (idempotent)
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
