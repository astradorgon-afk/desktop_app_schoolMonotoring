-- Migration: Add app_settings table (idempotent)
-- Holds admin-tunable values such as the late grace period. The app also creates this
-- table on first use when its database user has CREATE rights.
CREATE TABLE IF NOT EXISTS app_settings (
    setting_key VARCHAR(100) NOT NULL PRIMARY KEY,
    setting_value TEXT NOT NULL,
    updated_by_user_id BIGINT NULL,
    updated_at DATETIME(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
