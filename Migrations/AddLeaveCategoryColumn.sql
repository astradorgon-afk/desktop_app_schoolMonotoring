-- Migration: Add category column to leave_requests table (idempotent)
ALTER TABLE leave_requests
    ADD COLUMN category VARCHAR(50) NULL AFTER reason;
