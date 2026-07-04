-- ============================================================
-- CLEANUP - Deletes all existing data
-- Run this FIRST before seed_data.sql
-- ============================================================

SET SQL_SAFE_UPDATES = 0;
SET FOREIGN_KEY_CHECKS = 0;

DELETE FROM attendance_records;
DELETE FROM attendance_sessions;
DELETE FROM class_schedules;
DELETE FROM class_students;
DELETE FROM class_offerings;
DELETE FROM teacher_subject_tbl;
DELETE FROM student_qr_tbl;
DELETE FROM qr_tokens;
DELETE FROM enrollments;
DELETE FROM sections;
DELETE FROM subjects;
DELETE FROM students;
DELETE FROM teachers;
DELETE FROM users;
DELETE FROM rooms;
DELETE FROM grade_levels;
DELETE FROM school_years;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Cleanup complete. Run seed_data.sql next.' AS STATUS;
