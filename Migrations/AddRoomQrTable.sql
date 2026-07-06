-- Migration: Add room_qr_tbl for room QR code storage (idempotent)
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
