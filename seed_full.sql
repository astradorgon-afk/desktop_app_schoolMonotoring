-- ============================================================
-- SEED DATA - School SMS Attendance System
-- Safe to re-run (clears existing data first)
-- ============================================================

SET SQL_SAFE_UPDATES = 0;
SET FOREIGN_KEY_CHECKS = 0;

DELETE FROM attendance_records;
DELETE FROM attendance_sessions;
DELETE FROM class_schedules;
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

-- Requests filed by or about the people being replaced. These tables are created by the app
-- on first use, so each delete is skipped when its table is not there yet.
SET @t := IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'leave_requests') > 0, 'DELETE FROM leave_requests', 'DO 0');
PREPARE seed_stmt FROM @t; EXECUTE seed_stmt; DEALLOCATE PREPARE seed_stmt;
SET @t := IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'student_excuse_requests') > 0, 'DELETE FROM student_excuse_requests', 'DO 0');
PREPARE seed_stmt FROM @t; EXECUTE seed_stmt; DEALLOCATE PREPARE seed_stmt;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- SEED DATA PART 1: Foundational Data
-- Database: School_sms
-- ============================================================

-- 1. SCHOOL YEAR
INSERT INTO school_years (Id, name, start_date, end_date, status, created_at, updated_at) VALUES
(1, 'SY 2025-2026', '2025-06-01', '2026-03-31', 'ACTIVE', NOW(), NOW());

-- 2. GRADE LEVELS (K-12)
INSERT INTO grade_levels (Id, code, name, created_at, updated_at) VALUES
(1, 'G07', 'Grade 7',  NOW(), NOW()),
(2, 'G08', 'Grade 8',  NOW(), NOW()),
(3, 'G09', 'Grade 9',  NOW(), NOW()),
(4, 'G10', 'Grade 10', NOW(), NOW()),
(5, 'G11', 'Grade 11', NOW(), NOW()),
(6, 'G12', 'Grade 12', NOW(), NOW());

-- 3. ROOMS (25 rooms)
INSERT INTO rooms (Id, code, name, capacity, is_active, created_at, updated_at) VALUES
(1,  'RM-101', 'Room 101', 40, 1, NOW(), NOW()),
(2,  'RM-102', 'Room 102', 40, 1, NOW(), NOW()),
(3,  'RM-103', 'Room 103', 40, 1, NOW(), NOW()),
(4,  'RM-104', 'Room 104', 40, 1, NOW(), NOW()),
(5,  'RM-105', 'Room 105', 40, 1, NOW(), NOW()),
(6,  'RM-201', 'Room 201', 40, 1, NOW(), NOW()),
(7,  'RM-202', 'Room 202', 40, 1, NOW(), NOW()),
(8,  'RM-203', 'Room 203', 40, 1, NOW(), NOW()),
(9,  'RM-204', 'Room 204', 40, 1, NOW(), NOW()),
(10, 'RM-205', 'Room 205', 40, 1, NOW(), NOW()),
(11, 'RM-301', 'Room 301', 35, 1, NOW(), NOW()),
(12, 'RM-302', 'Room 302', 35, 1, NOW(), NOW()),
(13, 'RM-303', 'Room 303', 35, 1, NOW(), NOW()),
(14, 'RM-304', 'Room 304', 35, 1, NOW(), NOW()),
(15, 'RM-305', 'Room 305', 35, 1, NOW(), NOW()),
(16, 'SCI-LAB-A', 'Science Lab A', 30, 1, NOW(), NOW()),
(17, 'SCI-LAB-B', 'Science Lab B', 30, 1, NOW(), NOW()),
(18, 'COMP-LAB-1', 'Computer Lab 1', 30, 1, NOW(), NOW()),
(19, 'COMP-LAB-2', 'Computer Lab 2', 30, 1, NOW(), NOW()),
(20, 'LIBRARY', 'School Library', 50, 1, NOW(), NOW()),
(21, 'AUDITORIUM', 'School Auditorium', 100, 1, NOW(), NOW()),
(22, 'GYM', 'School Gymnasium', 80, 1, NOW(), NOW()),
(23, 'RM-MUSIC', 'Music Room', 25, 1, NOW(), NOW()),
(24, 'RM-ART', 'Art Room', 25, 1, NOW(), NOW()),
(25, 'RM-SPED', 'SPED Room', 20, 1, NOW(), NOW());

-- 4. SUBJECTS (32 subjects across grade levels 7-10)
-- Grade 7
INSERT INTO subjects (Id, code, title, description, grade_level_id, is_active, created_at, updated_at) VALUES
(1,  'ENG7',   'English 7',      'Grade 7 English',          1, 1, NOW(), NOW()),
(2,  'MATH7',  'Mathematics 7',  'Grade 7 Mathematics',      1, 1, NOW(), NOW()),
(3,  'SCI7',   'Science 7',      'Grade 7 Science',          1, 1, NOW(), NOW()),
(4,  'FIL7',   'Filipino 7',     'Grade 7 Filipino',         1, 1, NOW(), NOW()),
(5,  'AP7',    'AralPan 7',      'Araling Panlipunan 7',     1, 1, NOW(), NOW()),
(6,  'TLE7',   'TLE 7',          'Technology & Livelihood 7',1, 1, NOW(), NOW()),
(7,  'MAPEH7', 'MAPEH 7',        'MAPEH 7',                  1, 1, NOW(), NOW()),
(8,  'ESP7',   'ESP 7',          'Edukasyon sa Pagpapakatao 7',1,1, NOW(), NOW());
-- Grade 8
INSERT INTO subjects (Id, code, title, description, grade_level_id, is_active, created_at, updated_at) VALUES
(9,  'ENG8',   'English 8',     'Grade 8 English',           2, 1, NOW(), NOW()),
(10, 'MATH8',  'Mathematics 8', 'Grade 8 Mathematics',       2, 1, NOW(), NOW()),
(11, 'SCI8',   'Science 8',     'Grade 8 Science',           2, 1, NOW(), NOW()),
(12, 'FIL8',   'Filipino 8',    'Grade 8 Filipino',          2, 1, NOW(), NOW()),
(13, 'AP8',    'AralPan 8',     'Araling Panlipunan 8',      2, 1, NOW(), NOW()),
(14, 'TLE8',   'TLE 8',         'Technology & Livelihood 8', 2, 1, NOW(), NOW()),
(15, 'MAPEH8', 'MAPEH 8',       'MAPEH 8',                   2, 1, NOW(), NOW()),
(16, 'ESP8',   'ESP 8',         'Edukasyon sa Pagpapakatao 8',2,1, NOW(), NOW());
-- Grade 9
INSERT INTO subjects (Id, code, title, description, grade_level_id, is_active, created_at, updated_at) VALUES
(17, 'ENG9',   'English 9',     'Grade 9 English',           3, 1, NOW(), NOW()),
(18, 'MATH9',  'Mathematics 9', 'Grade 9 Mathematics',       3, 1, NOW(), NOW()),
(19, 'SCI9',   'Science 9',     'Grade 9 Science',           3, 1, NOW(), NOW()),
(20, 'FIL9',   'Filipino 9',    'Grade 9 Filipino',          3, 1, NOW(), NOW()),
(21, 'AP9',    'AralPan 9',     'Araling Panlipunan 9',      3, 1, NOW(), NOW()),
(22, 'TLE9',   'TLE 9',         'Technology & Livelihood 9', 3, 1, NOW(), NOW()),
(23, 'MAPEH9', 'MAPEH 9',       'MAPEH 9',                   3, 1, NOW(), NOW()),
(24, 'ESP9',   'ESP 9',         'Edukasyon sa Pagpapakatao 9',3,1, NOW(), NOW());
-- Grade 10
INSERT INTO subjects (Id, code, title, description, grade_level_id, is_active, created_at, updated_at) VALUES
(25, 'ENG10',  'English 10',    'Grade 10 English',          4, 1, NOW(), NOW()),
(26, 'MATH10', 'Mathematics 10','Grade 10 Mathematics',      4, 1, NOW(), NOW()),
(27, 'SCI10',  'Science 10',    'Grade 10 Science',          4, 1, NOW(), NOW()),
(28, 'FIL10',  'Filipino 10',   'Grade 10 Filipino',         4, 1, NOW(), NOW()),
(29, 'AP10',   'AralPan 10',    'Araling Panlipunan 10',     4, 1, NOW(), NOW()),
(30, 'TLE10',  'TLE 10',        'Technology & Livelihood 10',4, 1, NOW(), NOW()),
(31, 'MAPEH10','MAPEH 10',      'MAPEH 10',                  4, 1, NOW(), NOW()),
(32, 'ESP10',  'ESP 10',        'Edukasyon sa Pagpapakatao 10',4,1, NOW(), NOW());
-- ============================================================
-- SEED DATA PART 2: Users
-- Database: School_sms
-- ============================================================

-- Demo logins (PBKDF2 hashes below):
--   admin              / (unchanged)
--   teacher            / teacher     (also teacher2 ... teacher30, same password)
--   student            / student     (also student02 ... student99, same password)
-- These are demo credentials. Change them before using this database for real records.

SET @adminPw := 'PBKDF2$100000$bc/n4nBiwRHC5uMxiATBug==$IxOiO5DEm842CSzZNrtGIRFsilqCLOPkVLNv7OIx8L0=';
SET @teacherPw := 'PBKDF2$100000$QksYLkiKY6mH7TCiTHkk7A==$rr71dscGcml4zw91Wujh7s5kTD4iHVxcw2sjR7zRFYc=';
SET @studentPw := 'PBKDF2$100000$I+wc0GCjwYYlL7O36PPuqw==$IhbkfGpeHbnV84P1e0kKAPTaOYy+BZ6O1nJ56YvfJ5c=';

-- Admin
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(1, 'System Admin', 'admin', @adminPw, 'SUPERADMIN', 'ACTIVE', 1, NOW(), NOW());

-- 30 Teachers (teacher, teacher2 ... teacher30)
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(2, 'Gerald Dantes', 'teacher', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(3, 'Marvin Buenaventura', 'teacher2', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(4, 'Bryan Francisco', 'teacher3', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(5, 'Joanna Dantes', 'teacher4', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(6, 'Charmaine Ignacio', 'teacher5', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(7, 'Frances Malonzo', 'teacher6', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(8, 'Rico Buenaventura', 'teacher7', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(9, 'Gian Quiambao', 'teacher8', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(10, 'Alfonso Gatchalian', 'teacher9', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(11, 'Dominic Hernandez', 'teacher10', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(12, 'Katrina Ignacio', 'teacher11', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(13, 'Rico Samonte', 'teacher12', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(14, 'Vincent Panganiban', 'teacher13', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(15, 'Odessa Escobar', 'teacher14', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(16, 'Oliver Catapang', 'teacher15', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(17, 'Joaquin Mangubat', 'teacher16', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(18, 'Eunice Legaspi', 'teacher17', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(19, 'Bryan Tolentino', 'teacher18', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(20, 'Renz Robles', 'teacher19', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(21, 'Alfonso Cabrera', 'teacher20', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(22, 'Patrick Villareal', 'teacher21', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(23, 'Aubrey Concepcion', 'teacher22', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(24, 'Victor Belmonte', 'teacher23', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(25, 'Benjamin De Leon', 'teacher24', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(26, 'Denise Francisco', 'teacher25', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(27, 'Clarisse Macaraeg', 'teacher26', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(28, 'Hans Robles', 'teacher27', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(29, 'Ivan Natividad', 'teacher28', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(30, 'Bernadette Jimenez', 'teacher29', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(31, 'Vanessa Mangubat', 'teacher30', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW());

-- 99 Students (student, student02 ... student99; users 32-130)
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(32, 'Daphne Robles', 'student', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(33, 'Emmanuel Calderon', 'student02', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(34, 'Noel Catapang', 'student03', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(35, 'Samantha Robles', 'student04', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(36, 'Irene Ilagan', 'student05', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(37, 'Christian Nepomuceno', 'student06', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(38, 'Trisha Malonzo', 'student07', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(39, 'Marvin Alonzo', 'student08', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(40, 'Felix Pangilinan', 'student09', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(41, 'Darren Legaspi', 'student10', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(42, 'Charmaine Evangelista', 'student11', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(43, 'Winona Espiritu', 'student12', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(44, 'Hazel Enriquez', 'student13', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(45, 'Queenie Carpio', 'student14', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(46, 'Vincent Samonte', 'student15', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(47, 'Rhea Mangubat', 'student16', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(48, 'Gian Umali', 'student17', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(49, 'Denise Belmonte', 'student18', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(50, 'Alfonso Nepomuceno', 'student19', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(51, 'Felix Samonte', 'student20', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(52, 'Joaquin Cabrera', 'student21', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(53, 'Althea Lacson', 'student22', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(54, 'Katrina Villareal', 'student23', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(55, 'Althea Abad', 'student24', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(56, 'Aubrey Sarmiento', 'student25', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(57, 'Irene Umali', 'student26', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(58, 'Cedric Valenzuela', 'student27', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(59, 'Ulrich Robles', 'student28', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(60, 'Ysabel Dimaculangan', 'student29', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(61, 'Benjamin Concepcion', 'student30', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(62, 'Stephen Mangubat', 'student31', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(63, 'Vincent Lagman', 'student32', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(64, 'Sebastian Sarmiento', 'student33', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(65, 'Therese De Guzman', 'student34', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(66, 'Zara Quizon', 'student35', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(67, 'Matteo Samonte', 'student36', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(68, 'Lorraine Mangubat', 'student37', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(69, 'Oliver Umali', 'student38', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(70, 'Emmanuel Malonzo', 'student39', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(71, 'Charmaine Sarmiento', 'student40', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(72, 'Therese Dizon', 'student41', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(73, 'Victor Balagtas', 'student42', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(74, 'Christian Guevarra', 'student43', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(75, 'Erika Catapang', 'student44', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(76, 'Winona Pangilinan', 'student45', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(77, 'Ivan Balagtas', 'student46', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(78, 'Hans Estrada', 'student47', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(79, 'Ophelia Corpuz', 'student48', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(80, 'Maricar Dizon', 'student49', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(81, 'Gian Velasco', 'student50', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(82, 'Rhea Padilla', 'student51', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(83, 'Jerome Corpuz', 'student52', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(84, 'Ulrich Jimenez', 'student53', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(85, 'Katrina Dizon', 'student54', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(86, 'Oliver Dimaculangan', 'student55', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(87, 'Matteo Abad', 'student56', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(88, 'Precious Umali', 'student57', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(89, 'Alyssa Carpio', 'student58', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(90, 'Czarina Escobar', 'student59', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(91, 'Clarisse Buenaventura', 'student60', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(92, 'Sheena Ilagan', 'student61', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(93, 'Erika Ilagan', 'student62', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(94, 'Zara Corpuz', 'student63', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(95, 'Frances Quizon', 'student64', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(96, 'Noreen Abad', 'student65', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(97, 'Stephen De Guzman', 'student66', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(98, 'Odessa Robles', 'student67', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(99, 'Kenneth Dimaculangan', 'student68', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(100, 'Bianca Macaraeg', 'student69', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(101, 'Vincent Malonzo', 'student70', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(102, 'Therese Dimaculangan', 'student71', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(103, 'Felix Francisco', 'student72', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(104, 'Mikaela Abad', 'student73', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(105, 'Bianca De Guzman', 'student74', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(106, 'Ysabel Mangubat', 'student75', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(107, 'Joanna Andrada', 'student76', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(108, 'Zion Concepcion', 'student77', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(109, 'Isaac Pangilinan', 'student78', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(110, 'Rhea Lacson', 'student79', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(111, 'Gwen Zamora', 'student80', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(112, 'Vincent Villareal', 'student81', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(113, 'Faith Quiambao', 'student82', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(114, 'Lorraine Panganiban', 'student83', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(115, 'Ophelia Ilagan', 'student84', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(116, 'Margaux Gatchalian', 'student85', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(117, 'Margaux Buenaventura', 'student86', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(118, 'Mikaela De Leon', 'student87', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(119, 'Mikaela Magbanua', 'student88', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(120, 'Renz Quizon', 'student89', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(121, 'Quentin Galang', 'student90', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(122, 'Aubrey Zamora', 'student91', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(123, 'Joaquin Guevarra', 'student92', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(124, 'Karl Abad', 'student93', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(125, 'Liana Quiambao', 'student94', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(126, 'Ulrich Quiambao', 'student95', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(127, 'Marvin Sarmiento', 'student96', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(128, 'Karl Sarmiento', 'student97', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(129, 'Vanessa Zamora', 'student98', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(130, 'Vincent Valenzuela', 'student99', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW());
-- ============================================================
-- SEED DATA PART 3a: Teachers & Sections
-- Database: School_sms
-- ============================================================

-- 6. TEACHERS (30 teachers)
INSERT INTO teachers (Id, user_id, employee_no, first_name, last_name, middle_name, email, contact_no, hire_date, status, specialization, employment_status, created_at, updated_at) VALUES
(1, 2, 'EMP-001', 'Gerald', 'Dantes', 'Guevarra', 'gerald.dantes@school.edu', '09189655658', '2018-06-01', 'ACTIVE', 'English', 'PERMANENT', NOW(), NOW()),
(2, 3, 'EMP-002', 'Marvin', 'Buenaventura', 'Ilagan', 'marvin.buenaventura@school.edu', '09187878193', '2019-06-01', 'ACTIVE', 'Mathematics', 'PERMANENT', NOW(), NOW()),
(3, 4, 'EMP-003', 'Bryan', 'Francisco', 'Concepcion', 'bryan.francisco@school.edu', '09180815207', '2019-06-01', 'ACTIVE', 'Science', 'PERMANENT', NOW(), NOW()),
(4, 5, 'EMP-004', 'Joanna', 'Dantes', 'De Leon', 'joanna.dantes@school.edu', '09186567111', '2020-06-01', 'ACTIVE', 'Filipino', 'PERMANENT', NOW(), NOW()),
(5, 6, 'EMP-005', 'Charmaine', 'Ignacio', 'Agustin', 'charmaine.ignacio@school.edu', '09180656428', '2020-06-01', 'ACTIVE', 'Araling Panlipunan', 'PERMANENT', NOW(), NOW()),
(6, 7, 'EMP-006', 'Frances', 'Malonzo', 'Umali', 'frances.malonzo@school.edu', '09183785719', '2020-06-01', 'ACTIVE', 'TLE', 'PERMANENT', NOW(), NOW()),
(7, 8, 'EMP-007', 'Rico', 'Buenaventura', 'Francisco', 'rico.buenaventura@school.edu', '09188153208', '2021-06-01', 'ACTIVE', 'MAPEH', 'PERMANENT', NOW(), NOW()),
(8, 9, 'EMP-008', 'Gian', 'Quiambao', 'Estrada', 'gian.quiambao@school.edu', '09181086312', '2021-06-01', 'ACTIVE', 'ESP', 'PERMANENT', NOW(), NOW()),
(9, 10, 'EMP-009', 'Alfonso', 'Gatchalian', 'Escobar', 'alfonso.gatchalian@school.edu', '09180256937', '2022-06-01', 'ACTIVE', 'English', 'PROBATIONARY', NOW(), NOW()),
(10, 11, 'EMP-010', 'Dominic', 'Hernandez', 'Vergara', 'dominic.hernandez@school.edu', '09182275734', '2022-06-01', 'ACTIVE', 'Mathematics', 'PROBATIONARY', NOW(), NOW()),
(11, 12, 'EMP-011', 'Katrina', 'Ignacio', 'Arellano', 'katrina.ignacio@school.edu', '09188554698', '2022-06-01', 'ACTIVE', 'Science', 'PROBATIONARY', NOW(), NOW()),
(12, 13, 'EMP-012', 'Rico', 'Samonte', 'Natividad', 'rico.samonte@school.edu', '09184208272', '2023-06-01', 'ACTIVE', 'Filipino', 'PROBATIONARY', NOW(), NOW()),
(13, 14, 'EMP-013', 'Vincent', 'Panganiban', 'Hernandez', 'vincent.panganiban@school.edu', '09184547704', '2023-06-01', 'ACTIVE', 'Araling Panlipunan', 'PROBATIONARY', NOW(), NOW()),
(14, 15, 'EMP-014', 'Odessa', 'Escobar', 'De Guzman', 'odessa.escobar@school.edu', '09189400838', '2023-06-01', 'ACTIVE', 'TLE', 'PROBATIONARY', NOW(), NOW()),
(15, 16, 'EMP-015', 'Oliver', 'Catapang', 'Abad', 'oliver.catapang@school.edu', '09180856655', '2018-06-01', 'ACTIVE', 'MAPEH', 'PERMANENT', NOW(), NOW()),
(16, 17, 'EMP-016', 'Joaquin', 'Mangubat', 'De Leon', 'joaquin.mangubat@school.edu', '09189427643', '2019-06-01', 'ACTIVE', 'ESP', 'PERMANENT', NOW(), NOW()),
(17, 18, 'EMP-017', 'Eunice', 'Legaspi', 'Catapang', 'eunice.legaspi@school.edu', '09180419032', '2021-06-01', 'ACTIVE', 'English', 'PERMANENT', NOW(), NOW()),
(18, 19, 'EMP-018', 'Bryan', 'Tolentino', 'Umali', 'bryan.tolentino@school.edu', '09185048272', '2020-06-01', 'ACTIVE', 'Mathematics', 'PERMANENT', NOW(), NOW()),
(19, 20, 'EMP-019', 'Renz', 'Robles', 'Belmonte', 'renz.robles@school.edu', '09188676965', '2022-06-01', 'ACTIVE', 'Science', 'PROBATIONARY', NOW(), NOW()),
(20, 21, 'EMP-020', 'Alfonso', 'Cabrera', 'Macaraeg', 'alfonso.cabrera@school.edu', '09185540365', '2022-06-01', 'ACTIVE', 'Filipino', 'PROBATIONARY', NOW(), NOW()),
(21, 22, 'EMP-021', 'Patrick', 'Villareal', 'Sarmiento', 'patrick.villareal@school.edu', '09182567062', '2023-06-01', 'ACTIVE', 'Araling Panlipunan', 'PROBATIONARY', NOW(), NOW()),
(22, 23, 'EMP-022', 'Aubrey', 'Concepcion', 'Ilagan', 'aubrey.concepcion@school.edu', '09180878090', '2023-06-01', 'ACTIVE', 'TLE', 'PROBATIONARY', NOW(), NOW()),
(23, 24, 'EMP-023', 'Victor', 'Belmonte', 'Arellano', 'victor.belmonte@school.edu', '09185804860', '2018-06-01', 'ACTIVE', 'MAPEH', 'PERMANENT', NOW(), NOW()),
(24, 25, 'EMP-024', 'Benjamin', 'De Leon', 'Francisco', 'benjamin.deleon@school.edu', '09187555328', '2019-06-01', 'ACTIVE', 'ESP', 'PERMANENT', NOW(), NOW()),
(25, 26, 'EMP-025', 'Denise', 'Francisco', 'Calderon', 'denise.francisco@school.edu', '09187556839', '2020-06-01', 'ACTIVE', 'English', 'PERMANENT', NOW(), NOW()),
(26, 27, 'EMP-026', 'Clarisse', 'Macaraeg', 'De Guzman', 'clarisse.macaraeg@school.edu', '09183482083', '2021-06-01', 'ACTIVE', 'Mathematics', 'PERMANENT', NOW(), NOW()),
(27, 28, 'EMP-027', 'Hans', 'Robles', 'Villareal', 'hans.robles@school.edu', '09180706170', '2022-06-01', 'ACTIVE', 'Science', 'PROBATIONARY', NOW(), NOW()),
(28, 29, 'EMP-028', 'Ivan', 'Natividad', 'Balagtas', 'ivan.natividad@school.edu', '09182580341', '2023-06-01', 'ACTIVE', 'Filipino', 'PROBATIONARY', NOW(), NOW()),
(29, 30, 'EMP-029', 'Bernadette', 'Jimenez', 'Padilla', 'bernadette.jimenez@school.edu', '09185599862', '2023-06-01', 'ACTIVE', 'MAPEH', 'PROBATIONARY', NOW(), NOW()),
(30, 31, 'EMP-030', 'Vanessa', 'Mangubat', 'Samonte', 'vanessa.mangubat@school.edu', '09188041091', '2021-06-01', 'ACTIVE', 'TLE', 'PERMANENT', NOW(), NOW());

-- 7. SECTIONS (12 sections: 2 per grade level)
INSERT INTO sections (Id, school_year_id, grade_level_id, name, capacity, adviser_teacher_id, created_at, updated_at, is_archived) VALUES
(1,  1, 1, '7 - A',  40, 1,  NOW(), NOW(), 0),
(2,  1, 1, '7 - B',  40, 9,  NOW(), NOW(), 0),
(3,  1, 2, '8 - A',  40, 2,  NOW(), NOW(), 0),
(4,  1, 2, '8 - B',  40, 10, NOW(), NOW(), 0),
(5,  1, 3, '9 - A',  35, 3,  NOW(), NOW(), 0),
(6,  1, 3, '9 - B',  35, 11, NOW(), NOW(), 0),
(7,  1, 4, '10 - A', 35, 4,  NOW(), NOW(), 0),
(8,  1, 4, '10 - B', 35, 12, NOW(), NOW(), 0),
(9,  1, 5, '11 - STEM A', 40, 5,  NOW(), NOW(), 0),
(10, 1, 5, '11 - ABM B',  40, 13, NOW(), NOW(), 0),
(11, 1, 6, '12 - STEM A', 40, 6,  NOW(), NOW(), 0),
(12, 1, 6, '12 - ABM B',  40, 14, NOW(), NOW(), 0);
-- ============================================================
-- SEED DATA PART 3b: Students (99 records)
-- Database: School_sms
-- ============================================================

-- Section 1 (7-A): students 1-8
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(1, 32, '136220260001', '2025-0001', 'Daphne', 'Robles', 'Abrenica', '2013-01-07', 'Female', '161 Kalachuchi Ave., Brgy. San Isidro', 'Patrick Robles', '09171841868', 'ACTIVE', 12, '09194191893', NOW(), NOW()),
(2, 33, '136220260002', '2025-0002', 'Emmanuel', 'Calderon', 'Tolentino', '2012-01-28', 'Male', '23 Kalachuchi Ave., Brgy. Santa Lucia', 'Timothy Calderon', '09171470899', 'ACTIVE', 13, '09192085936', NOW(), NOW()),
(3, 34, '136220260003', '2025-0003', 'Noel', 'Catapang', 'Dizon', '2013-04-03', 'Male', '185 Kalachuchi Ave., Brgy. San Isidro', 'Irene Catapang', '09171233098', 'ACTIVE', 12, '09192237258', NOW(), NOW()),
(4, 35, '136220260004', '2025-0004', 'Samantha', 'Robles', 'Francisco', '2013-03-17', 'Female', '138 Ilang-Ilang St., Brgy. Pag-asa', 'Noreen Robles', '09179627081', 'ACTIVE', 12, '09193550830', NOW(), NOW()),
(5, 36, '136220260005', '2025-0005', 'Irene', 'Ilagan', 'Estrada', '2012-01-21', 'Female', '236 Molave Rd., Brgy. San Isidro', 'Karl Ilagan', '09177442617', 'ACTIVE', 13, '09191770344', NOW(), NOW()),
(6, 37, '136220260006', '2025-0006', 'Christian', 'Nepomuceno', 'Abrenica', '2013-05-27', 'Male', '80 Lirio St., Brgy. Bagong Silang', 'Hans Nepomuceno', '09174117995', 'ACTIVE', 12, '09196780638', NOW(), NOW()),
(7, 38, '136220260007', '2025-0007', 'Trisha', 'Malonzo', 'Pangilinan', '2012-07-01', 'Female', '182 Rosal St., Brgy. Bagong Silang', 'Kristine Malonzo', '09171290831', 'ACTIVE', 13, '09196903613', NOW(), NOW()),
(8, 39, '136220260008', '2025-0008', 'Marvin', 'Alonzo', 'Tolentino', '2013-02-22', 'Male', '139 Dama de Noche St., Brgy. Maligaya', 'Janine Alonzo', '09179705385', 'ACTIVE', 12, '09196730442', NOW(), NOW());

-- Section 2 (7-B): students 9-16
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(9, 40, '136220260009', '2025-0009', 'Felix', 'Pangilinan', 'Galang', '2012-05-14', 'Male', '22 Everlasting St., Brgy. San Roque', 'Marvin Pangilinan', '09179269607', 'ACTIVE', 13, '09199402576', NOW(), NOW()),
(10, 41, '136220260010', '2025-0010', 'Darren', 'Legaspi', 'Velasco', '2013-08-16', 'Male', '129 Kalachuchi Ave., Brgy. San Isidro', 'Noreen Legaspi', '09172658415', 'ACTIVE', 12, '09196556742', NOW(), NOW()),
(11, 42, '136220260011', '2025-0011', 'Charmaine', 'Evangelista', 'Abrenica', '2012-06-17', 'Female', '56 Bayanihan Rd., Brgy. Poblacion', 'Pauline Evangelista', '09176100048', 'ACTIVE', 13, '09196746969', NOW(), NOW()),
(12, 43, '136220260012', '2025-0012', 'Winona', 'Espiritu', 'Panganiban', '2013-05-25', 'Female', '224 Molave Rd., Brgy. San Roque', 'Oliver Espiritu', '09170502529', 'ACTIVE', 12, '09194284155', NOW(), NOW()),
(13, 44, '136220260013', '2025-0013', 'Hazel', 'Enriquez', 'Sumulong', '2012-06-24', 'Female', '168 Acacia St., Brgy. Santa Lucia', 'Lorraine Enriquez', '09174251722', 'ACTIVE', 13, '09199162793', NOW(), NOW()),
(14, 45, '136220260014', '2025-0014', 'Queenie', 'Carpio', 'Samonte', '2013-06-01', 'Female', '201 Ilang-Ilang St., Brgy. Pag-asa', 'Winona Carpio', '09176591876', 'ACTIVE', 12, '09198777836', NOW(), NOW()),
(15, 46, '136220260015', '2025-0015', 'Vincent', 'Samonte', 'Tiongson', '2012-05-08', 'Male', '48 Mahogany Ave., Brgy. Mabuhay', 'Noreen Samonte', '09177468485', 'ACTIVE', 13, '09193865704', NOW(), NOW()),
(16, 47, '136220260016', '2025-0016', 'Rhea', 'Mangubat', 'Dimaculangan', '2013-03-24', 'Female', '189 Molave Rd., Brgy. Santa Lucia', 'Jerome Mangubat', '09177694963', 'ACTIVE', 12, '09195940547', NOW(), NOW());

-- Section 3 (8-A): students 17-24
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(17, 48, '136220260017', '2025-0017', 'Gian', 'Umali', 'Hernandez', '2011-08-16', 'Male', '78 Santan St., Brgy. Bagong Silang', 'Pauline Umali', '09172574029', 'ACTIVE', 14, '09197747164', NOW(), NOW()),
(18, 49, '136220260018', '2025-0018', 'Denise', 'Belmonte', 'Dantes', '2011-06-19', 'Female', '45 Mahogany Ave., Brgy. Mabuhay', 'Arnel Belmonte', '09174234974', 'ACTIVE', 14, '09191467856', NOW(), NOW()),
(19, 50, '136220260019', '2025-0019', 'Alfonso', 'Nepomuceno', 'Lacson', '2011-04-23', 'Male', '231 Santan St., Brgy. Maligaya', 'Trisha Nepomuceno', '09170254811', 'ACTIVE', 14, '09196704099', NOW(), NOW()),
(20, 51, '136220260020', '2025-0020', 'Felix', 'Samonte', 'Andrada', '2011-03-17', 'Male', '213 Ilang-Ilang St., Brgy. Poblacion', 'Ivan Samonte', '09177880242', 'ACTIVE', 14, '09196905923', NOW(), NOW()),
(21, 52, '136220260021', '2025-0021', 'Joaquin', 'Cabrera', 'Concepcion', '2011-03-12', 'Male', '176 Bayanihan Rd., Brgy. Poblacion', 'Kristine Cabrera', '09171626538', 'ACTIVE', 14, '09196170595', NOW(), NOW()),
(22, 53, '136220260022', '2025-0022', 'Althea', 'Lacson', 'Ilagan', '2011-05-14', 'Female', '6 Ilang-Ilang St., Brgy. Santo Nino', 'Kenneth Lacson', '09173923325', 'ACTIVE', 14, '09199026872', NOW(), NOW()),
(23, 54, '136220260023', '2025-0023', 'Katrina', 'Villareal', 'Vergara', '2011-03-07', 'Female', '115 Mahogany Ave., Brgy. Pag-asa', 'Maricar Villareal', '09178807134', 'ACTIVE', 14, '09197000116', NOW(), NOW()),
(24, 55, '136220260024', '2025-0024', 'Althea', 'Abad', 'Samonte', '2011-04-22', 'Female', '141 Rosal St., Brgy. Malinis', 'Lorenzo Abad', '09175470508', 'ACTIVE', 14, '09194794211', NOW(), NOW());

-- Section 4 (8-B): students 25-32
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(25, 56, '136220260025', '2025-0025', 'Aubrey', 'Sarmiento', 'Balagtas', '2011-12-20', 'Female', '87 Rosal St., Brgy. San Roque', 'Lorraine Sarmiento', '09173238692', 'ACTIVE', 14, '09195129293', NOW(), NOW()),
(26, 57, '136220260026', '2025-0026', 'Irene', 'Umali', 'Malonzo', '2011-08-02', 'Female', '173 Acacia St., Brgy. Malinis', 'Rodel Umali', '09171161142', 'ACTIVE', 14, '09194294628', NOW(), NOW()),
(27, 58, '136220260027', '2025-0027', 'Cedric', 'Valenzuela', 'Agustin', '2011-06-16', 'Male', '199 Everlasting St., Brgy. Mabuhay', 'Therese Valenzuela', '09171875677', 'ACTIVE', 14, '09195225201', NOW(), NOW()),
(28, 59, '136220260028', '2025-0028', 'Ulrich', 'Robles', 'Zamora', '2012-11-04', 'Male', '108 Kalachuchi Ave., Brgy. Santo Nino', 'Gian Robles', '09178955183', 'ACTIVE', 13, '09190496566', NOW(), NOW()),
(29, 60, '136220260029', '2025-0029', 'Ysabel', 'Dimaculangan', 'Magbanua', '2011-03-18', 'Female', '83 Yakal Rd., Brgy. Mabuhay', 'Pauline Dimaculangan', '09171423759', 'ACTIVE', 14, '09198616073', NOW(), NOW()),
(30, 61, '136220260030', '2025-0030', 'Benjamin', 'Concepcion', 'Dizon', '2011-02-08', 'Male', '155 Camia St., Brgy. Pag-asa', 'Victor Concepcion', '09173246246', 'ACTIVE', 14, '09193393962', NOW(), NOW()),
(31, 62, '136220260031', '2025-0031', 'Stephen', 'Mangubat', 'Calderon', '2011-02-11', 'Male', '179 Acacia St., Brgy. Maligaya', 'Marvin Mangubat', '09172664803', 'ACTIVE', 14, '09198979619', NOW(), NOW()),
(32, 63, '136220260032', '2025-0032', 'Vincent', 'Lagman', 'Quizon', '2011-04-23', 'Male', '202 Ilang-Ilang St., Brgy. Maligaya', 'Margaux Lagman', '09174144988', 'ACTIVE', 14, '09196483528', NOW(), NOW());

-- Section 5 (9-A): students 33-40
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(33, 64, '136220260033', '2025-0033', 'Sebastian', 'Sarmiento', 'Nepomuceno', '2010-05-22', 'Male', '167 Rosal St., Brgy. Poblacion', 'Samantha Sarmiento', '09170289251', 'ACTIVE', 15, '09199671970', NOW(), NOW()),
(34, 65, '136220260034', '2025-0034', 'Therese', 'De Guzman', 'Evangelista', '2010-08-07', 'Female', '188 Camia St., Brgy. San Roque', 'Adrian De Guzman', '09179319086', 'ACTIVE', 15, '09199199352', NOW(), NOW()),
(35, 66, '136220260035', '2025-0035', 'Zara', 'Quizon', 'Quiambao', '2010-04-02', 'Female', '120 Rosal St., Brgy. Malinis', 'Frances Quizon', '09177951778', 'ACTIVE', 15, '09191920409', NOW(), NOW()),
(36, 67, '136220260036', '2025-0036', 'Matteo', 'Samonte', 'Tolentino', '2010-05-02', 'Male', '20 Molave Rd., Brgy. Bagong Silang', 'Cedric Samonte', '09173503745', 'ACTIVE', 15, '09199607117', NOW(), NOW()),
(37, 68, '136220260037', '2025-0037', 'Lorraine', 'Mangubat', 'Villareal', '2010-09-07', 'Female', '182 Kamagong St., Brgy. Malinis', 'Wesley Mangubat', '09179954459', 'ACTIVE', 15, '09196160363', NOW(), NOW()),
(38, 69, '136220260038', '2025-0038', 'Oliver', 'Umali', 'Sumulong', '2010-03-21', 'Male', '39 Kamagong St., Brgy. Poblacion', 'Mikaela Umali', '09170647616', 'ACTIVE', 15, '09192025205', NOW(), NOW()),
(39, 70, '136220260039', '2025-0039', 'Emmanuel', 'Malonzo', 'Catapang', '2010-12-18', 'Male', '19 Acacia St., Brgy. Santa Lucia', 'Bernadette Malonzo', '09176911187', 'ACTIVE', 15, '09199350148', NOW(), NOW()),
(40, 71, '136220260040', '2025-0040', 'Charmaine', 'Sarmiento', 'De Guzman', '2010-12-01', 'Female', '181 Rosal St., Brgy. Santa Lucia', 'Precious Sarmiento', '09172314763', 'ACTIVE', 15, '09190060363', NOW(), NOW());

-- Section 6 (9-B): students 41-48
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(41, 72, '136220260041', '2025-0041', 'Therese', 'Dizon', 'Dimaculangan', '2010-06-10', 'Female', '209 Molave Rd., Brgy. Pag-asa', 'Venice Dizon', '09173696764', 'ACTIVE', 15, '09193795662', NOW(), NOW()),
(42, 73, '136220260042', '2025-0042', 'Victor', 'Balagtas', 'Tolentino', '2010-02-04', 'Male', '114 Lirio St., Brgy. Malinis', 'Darren Balagtas', '09172361249', 'ACTIVE', 15, '09194735987', NOW(), NOW()),
(43, 74, '136220260043', '2025-0043', 'Christian', 'Guevarra', 'Villareal', '2010-02-27', 'Male', '102 Rosal St., Brgy. San Roque', 'Kenneth Guevarra', '09179663334', 'ACTIVE', 15, '09198714958', NOW(), NOW()),
(44, 75, '136220260044', '2025-0044', 'Erika', 'Catapang', 'Ilagan', '2010-12-07', 'Female', '109 Everlasting St., Brgy. Santa Lucia', 'Margaux Catapang', '09170168190', 'ACTIVE', 15, '09194086467', NOW(), NOW()),
(45, 76, '136220260045', '2025-0045', 'Winona', 'Pangilinan', 'Dizon', '2010-06-12', 'Female', '57 Kamagong St., Brgy. Bagong Silang', 'Xander Pangilinan', '09179735820', 'ACTIVE', 15, '09195825700', NOW(), NOW()),
(46, 77, '136220260046', '2025-0046', 'Ivan', 'Balagtas', 'Quizon', '2010-04-01', 'Male', '35 Camia St., Brgy. San Roque', 'Gwen Balagtas', '09173144450', 'ACTIVE', 15, '09199013257', NOW(), NOW()),
(47, 78, '136220260047', '2025-0047', 'Hans', 'Estrada', 'Agustin', '2010-09-23', 'Male', '126 Gumamela St., Brgy. Pag-asa', 'Zion Estrada', '09177854887', 'ACTIVE', 15, '09198999773', NOW(), NOW()),
(48, 79, '136220260048', '2025-0048', 'Ophelia', 'Corpuz', 'Galang', '2010-10-26', 'Female', '37 Rosal St., Brgy. Poblacion', 'Lorenzo Corpuz', '09178848398', 'ACTIVE', 15, '09196651822', NOW(), NOW());

-- Section 7 (10-A): students 49-56
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(49, 80, '136220260049', '2025-0049', 'Maricar', 'Dizon', 'Cabrera', '2009-09-05', 'Female', '93 Molave Rd., Brgy. San Roque', 'Hannah Dizon', '09171907774', 'ACTIVE', 16, '09190124279', NOW(), NOW()),
(50, 81, '136220260050', '2025-0050', 'Gian', 'Velasco', 'Alonzo', '2009-07-04', 'Male', '31 Banaba St., Brgy. Bagong Silang', 'Zara Velasco', '09173552319', 'ACTIVE', 16, '09190522045', NOW(), NOW()),
(51, 82, '136220260051', '2025-0051', 'Rhea', 'Padilla', 'Dantes', '2009-05-01', 'Female', '5 Santan St., Brgy. Maligaya', 'Jericho Padilla', '09174866675', 'ACTIVE', 16, '09191591354', NOW(), NOW()),
(52, 83, '136220260052', '2025-0052', 'Jerome', 'Corpuz', 'Hernandez', '2009-09-28', 'Male', '6 Kalachuchi Ave., Brgy. Santo Nino', 'Bryan Corpuz', '09175550851', 'ACTIVE', 16, '09190404784', NOW(), NOW()),
(53, 84, '136220260053', '2025-0053', 'Ulrich', 'Jimenez', 'Quiambao', '2009-06-02', 'Male', '198 Banaba St., Brgy. Santo Nino', 'Nadine Jimenez', '09174711973', 'ACTIVE', 16, '09193166192', NOW(), NOW()),
(54, 85, '136220260054', '2025-0054', 'Katrina', 'Dizon', 'Abad', '2009-06-03', 'Female', '54 Kamagong St., Brgy. Poblacion', 'Zara Dizon', '09170180615', 'ACTIVE', 16, '09197300717', NOW(), NOW()),
(55, 86, '136220260055', '2025-0055', 'Oliver', 'Dimaculangan', 'Tiongson', '2009-10-23', 'Male', '29 Banaba St., Brgy. San Roque', 'Harold Dimaculangan', '09177166386', 'ACTIVE', 16, '09196647581', NOW(), NOW()),
(56, 87, '136220260056', '2025-0056', 'Matteo', 'Abad', 'Magbanua', '2009-05-22', 'Male', '67 Yakal Rd., Brgy. San Roque', 'Matteo Abad', '09176981255', 'ACTIVE', 16, '09190117602', NOW(), NOW());

-- Section 8 (10-B): students 57-64
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(57, 88, '136220260057', '2025-0057', 'Precious', 'Umali', 'Sumulong', '2009-06-17', 'Female', '157 Lirio St., Brgy. Maligaya', 'Lance Umali', '09178558084', 'ACTIVE', 16, '09196286455', NOW(), NOW()),
(58, 89, '136220260058', '2025-0058', 'Alyssa', 'Carpio', 'Calderon', '2009-12-18', 'Female', '121 Everlasting St., Brgy. Malinis', 'Gian Carpio', '09171246558', 'ACTIVE', 16, '09190524649', NOW(), NOW()),
(59, 90, '136220260059', '2025-0059', 'Czarina', 'Escobar', 'Abrenica', '2009-04-09', 'Female', '157 Lirio St., Brgy. Poblacion', 'Rochelle Escobar', '09171908583', 'ACTIVE', 16, '09199096905', NOW(), NOW()),
(60, 91, '136220260060', '2025-0060', 'Clarisse', 'Buenaventura', 'Evangelista', '2009-03-16', 'Female', '155 Ilang-Ilang St., Brgy. Santa Lucia', 'Elijah Buenaventura', '09175049495', 'ACTIVE', 16, '09197593754', NOW(), NOW()),
(61, 92, '136220260061', '2025-0061', 'Sheena', 'Ilagan', 'Quizon', '2009-06-28', 'Female', '148 Molave Rd., Brgy. Pag-asa', 'Noreen Ilagan', '09170652009', 'ACTIVE', 16, '09198142088', NOW(), NOW()),
(62, 93, '136220260062', '2025-0062', 'Erika', 'Ilagan', 'Mangubat', '2009-07-17', 'Female', '99 Ilang-Ilang St., Brgy. Pag-asa', 'Odessa Ilagan', '09170403340', 'ACTIVE', 16, '09196962955', NOW(), NOW()),
(63, 94, '136220260063', '2025-0063', 'Zara', 'Corpuz', 'Hernandez', '2009-01-10', 'Female', '208 Yakal Rd., Brgy. Bagong Silang', 'Ursula Corpuz', '09179418412', 'ACTIVE', 16, '09191299851', NOW(), NOW()),
(64, 95, '136220260064', '2025-0064', 'Frances', 'Quizon', 'Carpio', '2009-01-27', 'Female', '164 Dama de Noche St., Brgy. San Isidro', 'Tristan Quizon', '09176489731', 'ACTIVE', 16, '09195597140', NOW(), NOW());

-- Section 9 (11-STEM A): students 65-72
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(65, 96, '136220260065', '2025-0065', 'Noreen', 'Abad', 'Ignacio', '2008-08-21', 'Female', '168 Yakal Rd., Brgy. San Roque', 'Frances Abad', '09179994131', 'ACTIVE', 17, '09198144908', NOW(), NOW()),
(66, 97, '136220260066', '2025-0066', 'Stephen', 'De Guzman', 'Corpuz', '2008-10-10', 'Male', '181 Kamagong St., Brgy. Poblacion', 'Venice De Guzman', '09176835957', 'ACTIVE', 17, '09194626161', NOW(), NOW()),
(67, 98, '136220260067', '2025-0067', 'Odessa', 'Robles', 'Macaraeg', '2008-12-04', 'Female', '43 Dama de Noche St., Brgy. Bagong Silang', 'Czarina Robles', '09178441489', 'ACTIVE', 17, '09197794630', NOW(), NOW()),
(68, 99, '136220260068', '2025-0068', 'Kenneth', 'Dimaculangan', 'Guevarra', '2008-04-11', 'Male', '82 Mahogany Ave., Brgy. Malinis', 'Queenie Dimaculangan', '09172605173', 'ACTIVE', 17, '09199400836', NOW(), NOW()),
(69, 100, '136220260069', '2025-0069', 'Bianca', 'Macaraeg', 'Jimenez', '2008-09-21', 'Female', '12 Camia St., Brgy. Mabuhay', 'Emmanuel Macaraeg', '09177360851', 'ACTIVE', 17, '09193840385', NOW(), NOW()),
(70, 101, '136220260070', '2025-0070', 'Vincent', 'Malonzo', 'Robles', '2008-09-09', 'Male', '157 Molave Rd., Brgy. Santa Lucia', 'Bryan Malonzo', '09177763590', 'ACTIVE', 17, '09199836191', NOW(), NOW()),
(71, 102, '136220260071', '2025-0071', 'Therese', 'Dimaculangan', 'Alonzo', '2008-07-19', 'Female', '99 Rosal St., Brgy. Maligaya', 'Oliver Dimaculangan', '09172756103', 'ACTIVE', 17, '09192248794', NOW(), NOW()),
(72, 103, '136220260072', '2025-0072', 'Felix', 'Francisco', 'Alonzo', '2008-11-22', 'Male', '100 Rosal St., Brgy. Santo Nino', 'Alfonso Francisco', '09176797788', 'ACTIVE', 17, '09199966762', NOW(), NOW());

-- Section 10 (11-ABM B): students 73-80
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(73, 104, '136220260073', '2025-0073', 'Mikaela', 'Abad', 'Enriquez', '2008-05-07', 'Female', '142 Acacia St., Brgy. Bagong Silang', 'Vanessa Abad', '09176321710', 'ACTIVE', 17, '09199453287', NOW(), NOW()),
(74, 105, '136220260074', '2025-0074', 'Bianca', 'De Guzman', 'Agustin', '2008-08-09', 'Female', '56 Banaba St., Brgy. San Roque', 'Trisha De Guzman', '09174165123', 'ACTIVE', 17, '09199961992', NOW(), NOW()),
(75, 106, '136220260075', '2025-0075', 'Ysabel', 'Mangubat', 'Villareal', '2008-04-04', 'Female', '130 Mahogany Ave., Brgy. Poblacion', 'Dominic Mangubat', '09176998817', 'ACTIVE', 17, '09196132288', NOW(), NOW()),
(76, 107, '136220260076', '2025-0076', 'Joanna', 'Andrada', 'Villareal', '2008-04-06', 'Female', '167 Acacia St., Brgy. San Roque', 'Zara Andrada', '09173988359', 'ACTIVE', 17, '09195035942', NOW(), NOW()),
(77, 108, '136220260077', '2025-0077', 'Zion', 'Concepcion', 'Dimaculangan', '2008-07-26', 'Male', '29 Banaba St., Brgy. San Isidro', 'Renz Concepcion', '09178914501', 'ACTIVE', 17, '09193263276', NOW(), NOW()),
(78, 109, '136220260078', '2025-0078', 'Isaac', 'Pangilinan', 'Gatchalian', '2008-07-19', 'Male', '40 Yakal Rd., Brgy. San Isidro', 'Ophelia Pangilinan', '09178733705', 'ACTIVE', 17, '09191612849', NOW(), NOW()),
(79, 110, '136220260079', '2025-0079', 'Rhea', 'Lacson', 'Nepomuceno', '2008-01-04', 'Female', '48 Molave Rd., Brgy. Bagong Silang', 'Vincent Lacson', '09170561002', 'ACTIVE', 17, '09199553181', NOW(), NOW()),
(80, 111, '136220260080', '2025-0080', 'Gwen', 'Zamora', 'Andrada', '2008-09-14', 'Female', '9 Ilang-Ilang St., Brgy. Santa Lucia', 'Queenie Zamora', '09172098250', 'ACTIVE', 17, '09197751210', NOW(), NOW());

-- Section 11 (12-STEM A): students 81-88
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(81, 112, '136220260081', '2025-0081', 'Vincent', 'Villareal', 'Lagman', '2007-11-11', 'Male', '159 Acacia St., Brgy. San Roque', 'Vincent Villareal', '09170331825', 'ACTIVE', 18, '09195603711', NOW(), NOW()),
(82, 113, '136220260082', '2025-0082', 'Faith', 'Quiambao', 'Panganiban', '2007-06-26', 'Female', '157 Gumamela St., Brgy. San Isidro', 'Adrian Quiambao', '09175347521', 'ACTIVE', 18, '09196980121', NOW(), NOW()),
(83, 114, '136220260083', '2025-0083', 'Lorraine', 'Panganiban', 'Francisco', '2007-08-22', 'Female', '98 Everlasting St., Brgy. Poblacion', 'Queenie Panganiban', '09171162657', 'ACTIVE', 18, '09196042993', NOW(), NOW()),
(84, 115, '136220260084', '2025-0084', 'Ophelia', 'Ilagan', 'Francisco', '2007-01-06', 'Female', '111 Santan St., Brgy. Mabuhay', 'Gian Ilagan', '09178730269', 'ACTIVE', 18, '09199776322', NOW(), NOW()),
(85, 116, '136220260085', '2025-0085', 'Margaux', 'Gatchalian', 'Catapang', '2007-09-16', 'Female', '85 Mahogany Ave., Brgy. San Isidro', 'Isaac Gatchalian', '09176239198', 'ACTIVE', 18, '09194241966', NOW(), NOW()),
(86, 117, '136220260086', '2025-0086', 'Margaux', 'Buenaventura', 'Legaspi', '2007-12-18', 'Female', '190 Everlasting St., Brgy. Pag-asa', 'Pauline Buenaventura', '09177697921', 'ACTIVE', 18, '09192736490', NOW(), NOW()),
(87, 118, '136220260087', '2025-0087', 'Mikaela', 'De Leon', 'Nepomuceno', '2007-09-13', 'Female', '214 Molave Rd., Brgy. Pag-asa', 'Hannah De Leon', '09179498734', 'ACTIVE', 18, '09196321388', NOW(), NOW()),
(88, 119, '136220260088', '2025-0088', 'Mikaela', 'Magbanua', 'Mangubat', '2007-04-09', 'Female', '28 Yakal Rd., Brgy. Poblacion', 'Zion Magbanua', '09178079966', 'ACTIVE', 18, '09193640716', NOW(), NOW());

-- Section 12 (12-ABM B): students 89-99 (11 students)
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(89, 120, '136220260089', '2025-0089', 'Renz', 'Quizon', 'Sarmiento', '2007-11-14', 'Male', '30 Bayanihan Rd., Brgy. Pag-asa', 'Venice Quizon', '09171775156', 'ACTIVE', 18, '09197558019', NOW(), NOW()),
(90, 121, '136220260090', '2025-0090', 'Quentin', 'Galang', 'Dantes', '2007-12-26', 'Male', '17 Molave Rd., Brgy. Bagong Silang', 'Eunice Galang', '09174292033', 'ACTIVE', 18, '09197119801', NOW(), NOW()),
(91, 122, '136220260091', '2025-0091', 'Aubrey', 'Zamora', 'Alonzo', '2007-03-24', 'Female', '167 Camia St., Brgy. San Roque', 'Christian Zamora', '09179204334', 'ACTIVE', 18, '09192218979', NOW(), NOW()),
(92, 123, '136220260092', '2025-0092', 'Joaquin', 'Guevarra', 'Natividad', '2007-02-09', 'Male', '82 Molave Rd., Brgy. Maligaya', 'Rochelle Guevarra', '09174163803', 'ACTIVE', 18, '09197136775', NOW(), NOW()),
(93, 124, '136220260093', '2025-0093', 'Karl', 'Abad', 'Buenaventura', '2007-06-11', 'Male', '235 Camia St., Brgy. Pag-asa', 'Karl Abad', '09170992833', 'ACTIVE', 18, '09198344530', NOW(), NOW()),
(94, 125, '136220260094', '2025-0094', 'Liana', 'Quiambao', 'De Leon', '2007-01-22', 'Female', '228 Kamagong St., Brgy. San Isidro', 'Erika Quiambao', '09173131780', 'ACTIVE', 18, '09195997802', NOW(), NOW()),
(95, 126, '136220260095', '2025-0095', 'Ulrich', 'Quiambao', 'Villareal', '2007-01-07', 'Male', '36 Lirio St., Brgy. Bagong Silang', 'Arnel Quiambao', '09178794166', 'ACTIVE', 18, '09199058399', NOW(), NOW()),
(96, 127, '136220260096', '2025-0096', 'Marvin', 'Sarmiento', 'Evangelista', '2007-11-20', 'Male', '202 Lirio St., Brgy. Maligaya', 'Faith Sarmiento', '09172615627', 'ACTIVE', 18, '09192179711', NOW(), NOW()),
(97, 128, '136220260097', '2025-0097', 'Karl', 'Sarmiento', 'Malonzo', '2007-11-12', 'Male', '14 Lirio St., Brgy. San Isidro', 'Quentin Sarmiento', '09176864116', 'ACTIVE', 18, '09197001829', NOW(), NOW()),
(98, 129, '136220260098', '2025-0098', 'Vanessa', 'Zamora', 'Ignacio', '2007-01-18', 'Female', '230 Bayanihan Rd., Brgy. Poblacion', 'Benjamin Zamora', '09179910918', 'ACTIVE', 18, '09197722109', NOW(), NOW()),
(99, 130, '136220260099', '2025-0099', 'Vincent', 'Valenzuela', 'Lagman', '2007-04-05', 'Male', '160 Everlasting St., Brgy. San Roque', 'Adrian Valenzuela', '09175876472', 'ACTIVE', 18, '09194685908', NOW(), NOW());
-- ============================================================
-- SEED DATA PART 3c: Enrollments (99 records)
-- Database: School_sms
-- ============================================================

INSERT INTO enrollments (Id, school_year_id, student_id, grade_level_id, section_id, curriculum_id, status, enrolled_at, created_at, updated_at) VALUES
(1, 1, 1, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(2, 1, 2, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(3, 1, 3, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(4, 1, 4, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(5, 1, 5, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(6, 1, 6, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(7, 1, 7, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(8, 1, 8, 1, 1,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(9, 1, 9, 1, 2,  1, 'ACTIVE', NOW(), NOW(), NOW()),
(10, 1, 10, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(11, 1, 11, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(12, 1, 12, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(13, 1, 13, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(14, 1, 14, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(15, 1, 15, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(16, 1, 16, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(17, 1, 17, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(18, 1, 18, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(19, 1, 19, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(20, 1, 20, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(21, 1, 21, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(22, 1, 22, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(23, 1, 23, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(24, 1, 24, 2, 3, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(25, 1, 25, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(26, 1, 26, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(27, 1, 27, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(28, 1, 28, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(29, 1, 29, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(30, 1, 30, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(31, 1, 31, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(32, 1, 32, 2, 4, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(33, 1, 33, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(34, 1, 34, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(35, 1, 35, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(36, 1, 36, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(37, 1, 37, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(38, 1, 38, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(39, 1, 39, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(40, 1, 40, 3, 5, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(41, 1, 41, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(42, 1, 42, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(43, 1, 43, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(44, 1, 44, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(45, 1, 45, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(46, 1, 46, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(47, 1, 47, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(48, 1, 48, 3, 6, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(49, 1, 49, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(50, 1, 50, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(51, 1, 51, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(52, 1, 52, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(53, 1, 53, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(54, 1, 54, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(55, 1, 55, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(56, 1, 56, 4, 7, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(57, 1, 57, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(58, 1, 58, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(59, 1, 59, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(60, 1, 60, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(61, 1, 61, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(62, 1, 62, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(63, 1, 63, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(64, 1, 64, 4, 8, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(65, 1, 65, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(66, 1, 66, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(67, 1, 67, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(68, 1, 68, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(69, 1, 69, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(70, 1, 70, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(71, 1, 71, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(72, 1, 72, 5, 9, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(73, 1, 73, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(74, 1, 74, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(75, 1, 75, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(76, 1, 76, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(77, 1, 77, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(78, 1, 78, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(79, 1, 79, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(80, 1, 80, 5, 10, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(81, 1, 81, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(82, 1, 82, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(83, 1, 83, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(84, 1, 84, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(85, 1, 85, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(86, 1, 86, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(87, 1, 87, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(88, 1, 88, 6, 11, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(89, 1, 89, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(90, 1, 90, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(91, 1, 91, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(92, 1, 92, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(93, 1, 93, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(94, 1, 94, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(95, 1, 95, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(96, 1, 96, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(97, 1, 97, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(98, 1, 98, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW()),
(99, 1, 99, 6, 12, 1, 'ACTIVE', NOW(), NOW(), NOW());
-- ============================================================
-- SEED DATA PART 4a: Class Offerings
-- Database: School_sms
-- ============================================================

-- Section 1 (7-A): subjects 1-8
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(1,  1, 1, 1,  1,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(2,  1, 1, 2,  1,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(3,  1, 1, 3,  3,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(4,  1, 1, 4,  4,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(5,  1, 1, 5,  5,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(6,  1, 1, 6,  6,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(7,  1, 1, 7,  7,  1, 'ACTIVE', 'RM-101', NOW(), NOW()),
(8,  1, 1, 8,  8,  1, 'ACTIVE', 'RM-101', NOW(), NOW());

-- Section 2 (7-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(9,  1, 2, 1,  9,  1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(10, 1, 2, 2,  1,  1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(11, 1, 2, 3,  11, 1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(12, 1, 2, 4,  12, 1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(13, 1, 2, 5,  13, 1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(14, 1, 2, 6,  14, 1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(15, 1, 2, 7,  15, 1, 'ACTIVE', 'RM-102', NOW(), NOW()),
(16, 1, 2, 8,  16, 1, 'ACTIVE', 'RM-102', NOW(), NOW());

-- Section 3 (8-A): subjects 9-16
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(17, 1, 3, 9,  17, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(18, 1, 3, 10, 18, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(19, 1, 3, 11, 19, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(20, 1, 3, 12, 20, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(21, 1, 3, 13, 21, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(22, 1, 3, 14, 22, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(23, 1, 3, 15, 23, 1, 'ACTIVE', 'RM-201', NOW(), NOW()),
(24, 1, 3, 16, 24, 1, 'ACTIVE', 'RM-201', NOW(), NOW());

-- Section 4 (8-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(25, 1, 4, 9,  25, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(26, 1, 4, 10, 26, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(27, 1, 4, 11, 27, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(28, 1, 4, 12, 28, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(29, 1, 4, 13, 29, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(30, 1, 4, 14, 30, 1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(31, 1, 4, 15, 7,  1, 'ACTIVE', 'RM-202', NOW(), NOW()),
(32, 1, 4, 16, 8,  1, 'ACTIVE', 'RM-202', NOW(), NOW());

-- Section 5 (9-A): subjects 17-24
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(33, 1, 5, 17, 1,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(34, 1, 5, 18, 1,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(35, 1, 5, 19, 3,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(36, 1, 5, 20, 4,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(37, 1, 5, 21, 5,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(38, 1, 5, 22, 6,  1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(39, 1, 5, 23, 23, 1, 'ACTIVE', 'RM-301', NOW(), NOW()),
(40, 1, 5, 24, 24, 1, 'ACTIVE', 'RM-301', NOW(), NOW());

-- Section 6 (9-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(41, 1, 6, 17, 9,  1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(42, 1, 6, 18, 1,  1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(43, 1, 6, 19, 11, 1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(44, 1, 6, 20, 12, 1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(45, 1, 6, 21, 13, 1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(46, 1, 6, 22, 14, 1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(47, 1, 6, 23, 15, 1, 'ACTIVE', 'RM-302', NOW(), NOW()),
(48, 1, 6, 24, 16, 1, 'ACTIVE', 'RM-302', NOW(), NOW());

-- Section 7 (10-A): subjects 25-32
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(49, 1, 7, 25, 17, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(50, 1, 7, 26, 1,  1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(51, 1, 7, 27, 19, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(52, 1, 7, 28, 20, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(53, 1, 7, 29, 21, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(54, 1, 7, 30, 22, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(55, 1, 7, 31, 29, 1, 'ACTIVE', 'RM-203', NOW(), NOW()),
(56, 1, 7, 32, 8,  1, 'ACTIVE', 'RM-203', NOW(), NOW());

-- Section 8 (10-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(57, 1, 8, 25, 25, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(58, 1, 8, 26, 26, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(59, 1, 8, 27, 27, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(60, 1, 8, 28, 28, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(61, 1, 8, 29, 29, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(62, 1, 8, 30, 30, 1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(63, 1, 8, 31, 7,  1, 'ACTIVE', 'RM-204', NOW(), NOW()),
(64, 1, 8, 32, 24, 1, 'ACTIVE', 'RM-204', NOW(), NOW());

-- Section 9 (11-STEM A): select subjects
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(65, 1, 9, 1,  1,  1, 'ACTIVE', 'RM-303', NOW(), NOW()),
(66, 1, 9, 2,  1,  1, 'ACTIVE', 'RM-303', NOW(), NOW()),
(67, 1, 9, 3,  3,  1, 'ACTIVE', 'RM-303', NOW(), NOW()),
(68, 1, 9, 9,  9,  1, 'ACTIVE', 'RM-303', NOW(), NOW()),
(69, 1, 9, 10, 10, 1, 'ACTIVE', 'RM-303', NOW(), NOW()),
(70, 1, 9, 11, 11, 1, 'ACTIVE', 'RM-303', NOW(), NOW());

-- Section 10 (11-ABM B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(71, 1, 10, 1,  9,  1, 'ACTIVE', 'RM-304', NOW(), NOW()),
(72, 1, 10, 2,  1,  1, 'ACTIVE', 'RM-304', NOW(), NOW()),
(73, 1, 10, 9,  17, 1, 'ACTIVE', 'RM-304', NOW(), NOW()),
(74, 1, 10, 10, 18, 1, 'ACTIVE', 'RM-304', NOW(), NOW()),
(75, 1, 10, 25, 25, 1, 'ACTIVE', 'RM-304', NOW(), NOW()),
(76, 1, 10, 26, 26, 1, 'ACTIVE', 'RM-304', NOW(), NOW());

-- Section 11 (12-STEM A)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(77, 1, 11, 17, 1,  1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(78, 1, 11, 18, 1,  1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(79, 1, 11, 19, 3,  1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(80, 1, 11, 25, 17, 1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(81, 1, 11, 26, 18, 1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(82, 1, 11, 27, 19, 1, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW());

-- Section 12 (12-ABM B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(83, 1, 12, 17, 9,  1, 'ACTIVE', 'RM-305', NOW(), NOW()),
(84, 1, 12, 18, 10, 1, 'ACTIVE', 'RM-305', NOW(), NOW()),
(85, 1, 12, 25, 25, 1, 'ACTIVE', 'RM-305', NOW(), NOW()),
(86, 1, 12, 26, 26, 1, 'ACTIVE', 'RM-305', NOW(), NOW()),
(87, 1, 12, 9,  17, 1, 'ACTIVE', 'RM-305', NOW(), NOW()),
(88, 1, 12, 10, 18, 1, 'ACTIVE', 'RM-305', NOW(), NOW());
-- ============================================================
-- SEED DATA PART 4b: Class Schedules
-- Day: 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri
-- Database: School_sms
-- ============================================================

-- Section 1 (7-A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(1,  1,  1, '07:30:00', '08:30:00', 1,  NULL, NOW(), NOW()),
(2,  2,  1, '08:30:00', '09:30:00', 1,  NULL, NOW(), NOW()),
(3,  3,  1, '09:30:00', '10:30:00', 1,  NULL, NOW(), NOW()),
(4,  4,  3, '07:30:00', '08:30:00', 1,  NULL, NOW(), NOW()),
(5,  5,  3, '08:30:00', '09:30:00', 1,  NULL, NOW(), NOW()),
(6,  6,  3, '09:30:00', '10:30:00', 1,  NULL, NOW(), NOW()),
(7,  7,  5, '07:30:00', '08:30:00', 1,  NULL, NOW(), NOW()),
(8,  8,  5, '08:30:00', '09:30:00', 1,  NULL, NOW(), NOW());

-- Section 1 (7-A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(9,  1,  2, '07:30:00', '08:30:00', 1,  NULL, NOW(), NOW()),
(10, 2,  2, '08:30:00', '09:30:00', 1,  NULL, NOW(), NOW()),
(11, 5,  2, '09:30:00', '10:30:00', 1,  NULL, NOW(), NOW()),
(12, 6,  4, '07:30:00', '08:30:00', 1,  NULL, NOW(), NOW()),
(13, 7,  4, '08:30:00', '09:30:00', 1,  NULL, NOW(), NOW()),
(14, 8,  4, '09:30:00', '10:30:00', 1,  NULL, NOW(), NOW());

-- Section 2 (7-B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(15, 9,  1, '10:30:00', '11:30:00', 2, NULL, NOW(), NOW()),
(16, 10, 1, '09:30:00', '10:30:00', 2, NULL, NOW(), NOW()),
(17, 11, 1, '13:30:00', '14:30:00', 2, NULL, NOW(), NOW()),
(18, 12, 3, '10:30:00', '11:30:00', 2, NULL, NOW(), NOW()),
(19, 13, 3, '12:30:00', '13:30:00', 2, NULL, NOW(), NOW()),
(20, 14, 3, '13:30:00', '14:30:00', 2, NULL, NOW(), NOW()),
(21, 15, 5, '10:30:00', '11:30:00', 2, NULL, NOW(), NOW()),
(22, 16, 5, '12:30:00', '13:30:00', 2, NULL, NOW(), NOW());

-- Section 2 (7-B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(23, 9,  2, '10:30:00', '11:30:00', 2, NULL, NOW(), NOW()),
(24, 10, 2, '09:30:00', '10:30:00', 2, NULL, NOW(), NOW()),
(25, 13, 2, '13:30:00', '14:30:00', 2, NULL, NOW(), NOW()),
(26, 14, 4, '10:30:00', '11:30:00', 2, NULL, NOW(), NOW()),
(27, 15, 4, '12:30:00', '13:30:00', 2, NULL, NOW(), NOW()),
(28, 16, 4, '13:30:00', '14:30:00', 2, NULL, NOW(), NOW());

-- Section 3 (8-A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(29, 17, 1, '07:30:00', '08:30:00', 6, NULL, NOW(), NOW()),
(30, 18, 1, '08:30:00', '09:30:00', 6, NULL, NOW(), NOW()),
(31, 19, 1, '09:30:00', '10:30:00', 6, NULL, NOW(), NOW()),
(32, 20, 3, '07:30:00', '08:30:00', 6, NULL, NOW(), NOW()),
(33, 21, 3, '08:30:00', '09:30:00', 6, NULL, NOW(), NOW()),
(34, 22, 3, '09:30:00', '10:30:00', 6, NULL, NOW(), NOW()),
(35, 23, 5, '07:30:00', '08:30:00', 6, NULL, NOW(), NOW()),
(36, 24, 5, '08:30:00', '09:30:00', 6, NULL, NOW(), NOW());

-- Section 3 (8-A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(37, 17, 2, '07:30:00', '08:30:00', 6, NULL, NOW(), NOW()),
(38, 18, 2, '08:30:00', '09:30:00', 6, NULL, NOW(), NOW()),
(39, 21, 2, '09:30:00', '10:30:00', 6, NULL, NOW(), NOW()),
(40, 22, 4, '07:30:00', '08:30:00', 6, NULL, NOW(), NOW()),
(41, 23, 4, '08:30:00', '09:30:00', 6, NULL, NOW(), NOW()),
(42, 24, 4, '09:30:00', '10:30:00', 6, NULL, NOW(), NOW());

-- Section 4 (8-B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(43, 25, 1, '10:30:00', '11:30:00', 7, NULL, NOW(), NOW()),
(44, 26, 1, '12:30:00', '13:30:00', 7, NULL, NOW(), NOW()),
(45, 27, 1, '13:30:00', '14:30:00', 7, NULL, NOW(), NOW()),
(46, 28, 3, '10:30:00', '11:30:00', 7, NULL, NOW(), NOW()),
(47, 29, 3, '12:30:00', '13:30:00', 7, NULL, NOW(), NOW()),
(48, 30, 3, '13:30:00', '14:30:00', 7, NULL, NOW(), NOW()),
(49, 31, 5, '10:30:00', '11:30:00', 7, NULL, NOW(), NOW()),
(50, 32, 5, '12:30:00', '13:30:00', 7, NULL, NOW(), NOW());

-- Section 4 (8-B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(51, 25, 2, '10:30:00', '11:30:00', 7, NULL, NOW(), NOW()),
(52, 26, 2, '12:30:00', '13:30:00', 7, NULL, NOW(), NOW()),
(53, 29, 2, '13:30:00', '14:30:00', 7, NULL, NOW(), NOW()),
(54, 30, 4, '10:30:00', '11:30:00', 7, NULL, NOW(), NOW()),
(55, 31, 4, '12:30:00', '13:30:00', 7, NULL, NOW(), NOW()),
(56, 32, 4, '13:30:00', '14:30:00', 7, NULL, NOW(), NOW());

-- Section 5 (9-A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(57, 33, 1, '10:30:00', '11:30:00', 11, NULL, NOW(), NOW()),
(58, 34, 3, '12:30:00', '13:30:00', 11, NULL, NOW(), NOW()),
(59, 35, 1, '09:30:00', '10:30:00', 11, NULL, NOW(), NOW()),
(60, 36, 3, '07:30:00', '08:30:00', 11, NULL, NOW(), NOW()),
(61, 37, 3, '08:30:00', '09:30:00', 11, NULL, NOW(), NOW()),
(62, 38, 3, '09:30:00', '10:30:00', 11, NULL, NOW(), NOW()),
(63, 39, 5, '07:30:00', '08:30:00', 11, NULL, NOW(), NOW()),
(64, 40, 5, '08:30:00', '09:30:00', 11, NULL, NOW(), NOW());

-- Section 5 (9-A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(65, 33, 2, '10:30:00', '11:30:00', 11, NULL, NOW(), NOW()),
(66, 34, 4, '12:30:00', '13:30:00', 11, NULL, NOW(), NOW()),
(67, 37, 2, '09:30:00', '10:30:00', 11, NULL, NOW(), NOW()),
(68, 38, 4, '07:30:00', '08:30:00', 11, NULL, NOW(), NOW()),
(69, 39, 4, '08:30:00', '09:30:00', 11, NULL, NOW(), NOW()),
(70, 40, 4, '09:30:00', '10:30:00', 11, NULL, NOW(), NOW());

-- Section 6 (9-B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(71, 41, 1, '10:30:00', '11:30:00', 12, NULL, NOW(), NOW()),
(72, 42, 3, '07:30:00', '08:30:00', 12, NULL, NOW(), NOW()),
(73, 43, 1, '13:30:00', '14:30:00', 12, NULL, NOW(), NOW()),
(74, 44, 3, '10:30:00', '11:30:00', 12, NULL, NOW(), NOW()),
(75, 45, 3, '12:30:00', '13:30:00', 12, NULL, NOW(), NOW()),
(76, 46, 3, '13:30:00', '14:30:00', 12, NULL, NOW(), NOW()),
(77, 47, 5, '10:30:00', '11:30:00', 12, NULL, NOW(), NOW()),
(78, 48, 5, '12:30:00', '13:30:00', 12, NULL, NOW(), NOW());

-- Section 6 (9-B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(79, 41, 2, '10:30:00', '11:30:00', 12, NULL, NOW(), NOW()),
(80, 42, 4, '07:30:00', '08:30:00', 12, NULL, NOW(), NOW()),
(81, 45, 2, '13:30:00', '14:30:00', 12, NULL, NOW(), NOW()),
(82, 46, 4, '10:30:00', '11:30:00', 12, NULL, NOW(), NOW()),
(83, 47, 4, '12:30:00', '13:30:00', 12, NULL, NOW(), NOW()),
(84, 48, 4, '13:30:00', '14:30:00', 12, NULL, NOW(), NOW());

-- Section 7 (10-A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(85, 49, 1, '07:30:00', '08:30:00', 8, NULL, NOW(), NOW()),
(86, 50, 1, '12:30:00', '13:30:00', 8, NULL, NOW(), NOW()),
(87, 51, 1, '09:30:00', '10:30:00', 8, NULL, NOW(), NOW()),
(88, 52, 3, '07:30:00', '08:30:00', 8, NULL, NOW(), NOW()),
(89, 53, 3, '08:30:00', '09:30:00', 8, NULL, NOW(), NOW()),
(90, 54, 3, '09:30:00', '10:30:00', 8, NULL, NOW(), NOW()),
(91, 55, 5, '07:30:00', '08:30:00', 8, NULL, NOW(), NOW()),
(92, 56, 5, '08:30:00', '09:30:00', 8, NULL, NOW(), NOW());

-- Section 7 (10-A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(93, 49, 2, '07:30:00', '08:30:00', 8, NULL, NOW(), NOW()),
(94, 50, 2, '12:30:00', '13:30:00', 8, NULL, NOW(), NOW()),
(95, 53, 2, '09:30:00', '10:30:00', 8, NULL, NOW(), NOW()),
(96, 54, 4, '07:30:00', '08:30:00', 8, NULL, NOW(), NOW()),
(97, 55, 4, '08:30:00', '09:30:00', 8, NULL, NOW(), NOW()),
(98, 56, 4, '09:30:00', '10:30:00', 8, NULL, NOW(), NOW());

-- Section 8 (10-B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(99,  57, 1, '10:30:00', '11:30:00', 9, NULL, NOW(), NOW()),
(100, 58, 1, '12:30:00', '13:30:00', 9, NULL, NOW(), NOW()),
(101, 59, 1, '13:30:00', '14:30:00', 9, NULL, NOW(), NOW()),
(102, 60, 3, '10:30:00', '11:30:00', 9, NULL, NOW(), NOW()),
(103, 61, 3, '12:30:00', '13:30:00', 9, NULL, NOW(), NOW()),
(104, 62, 3, '13:30:00', '14:30:00', 9, NULL, NOW(), NOW()),
(105, 63, 5, '10:30:00', '11:30:00', 9, NULL, NOW(), NOW()),
(106, 64, 5, '12:30:00', '13:30:00', 9, NULL, NOW(), NOW());

-- Section 8 (10-B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(107, 57, 2, '10:30:00', '11:30:00', 9, NULL, NOW(), NOW()),
(108, 58, 2, '12:30:00', '13:30:00', 9, NULL, NOW(), NOW()),
(109, 61, 2, '13:30:00', '14:30:00', 9, NULL, NOW(), NOW()),
(110, 62, 4, '10:30:00', '11:30:00', 9, NULL, NOW(), NOW()),
(111, 63, 4, '12:30:00', '13:30:00', 9, NULL, NOW(), NOW()),
(112, 64, 4, '13:30:00', '14:30:00', 9, NULL, NOW(), NOW());

-- Section 9 (11-STEM A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(113, 65, 3, '10:30:00', '11:30:00', 13, NULL, NOW(), NOW()),
(114, 66, 3, '13:30:00', '14:30:00', 13, NULL, NOW(), NOW()),
(115, 67, 1, '09:30:00', '10:30:00', 13, NULL, NOW(), NOW()),
(116, 68, 3, '07:30:00', '08:30:00', 13, NULL, NOW(), NOW()),
(117, 69, 3, '08:30:00', '09:30:00', 13, NULL, NOW(), NOW()),
(118, 70, 3, '09:30:00', '10:30:00', 13, NULL, NOW(), NOW());

-- Section 9 (11-STEM A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(119, 65, 4, '10:30:00', '11:30:00', 13, NULL, NOW(), NOW()),
(120, 66, 4, '13:30:00', '14:30:00', 13, NULL, NOW(), NOW()),
(121, 69, 2, '09:30:00', '10:30:00', 13, NULL, NOW(), NOW()),
(122, 70, 4, '07:30:00', '08:30:00', 13, NULL, NOW(), NOW());

-- Section 10 (11-ABM B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(123, 71, 1, '10:30:00', '11:30:00', 14, NULL, NOW(), NOW()),
(124, 72, 3, '08:30:00', '09:30:00', 14, NULL, NOW(), NOW()),
(125, 73, 3, '10:30:00', '11:30:00', 14, NULL, NOW(), NOW()),
(126, 74, 3, '12:30:00', '13:30:00', 14, NULL, NOW(), NOW()),
(127, 75, 5, '10:30:00', '11:30:00', 14, NULL, NOW(), NOW()),
(128, 76, 5, '12:30:00', '13:30:00', 14, NULL, NOW(), NOW());

-- Section 10 (11-ABM B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(129, 71, 2, '10:30:00', '11:30:00', 14, NULL, NOW(), NOW()),
(130, 73, 2, '12:30:00', '13:30:00', 14, NULL, NOW(), NOW()),
(131, 75, 4, '10:30:00', '11:30:00', 14, NULL, NOW(), NOW()),
(132, 76, 4, '12:30:00', '13:30:00', 14, NULL, NOW(), NOW()),
(153, 72, 4, '08:30:00', '09:30:00', 14, NULL, NOW(), NOW());

-- Section 11 (12-STEM A) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(133, 77, 1, '13:30:00', '14:30:00', 16, NULL, NOW(), NOW()),
(134, 78, 4, '09:30:00', '10:30:00', 16, NULL, NOW(), NOW()),
(135, 79, 1, '09:30:00', '10:30:00', 16, NULL, NOW(), NOW()),
(136, 80, 3, '07:30:00', '08:30:00', 16, NULL, NOW(), NOW()),
(137, 81, 3, '08:30:00', '09:30:00', 16, NULL, NOW(), NOW()),
(138, 82, 3, '09:30:00', '10:30:00', 16, NULL, NOW(), NOW());

-- Section 11 (12-STEM A) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(139, 77, 2, '13:30:00', '14:30:00', 16, NULL, NOW(), NOW()),
(140, 78, 5, '07:30:00', '08:30:00', 16, NULL, NOW(), NOW()),
(141, 81, 2, '09:30:00', '10:30:00', 16, NULL, NOW(), NOW()),
(142, 82, 4, '07:30:00', '08:30:00', 16, NULL, NOW(), NOW());

-- Section 12 (12-ABM B) MWF
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(143, 83, 1, '10:30:00', '11:30:00', 15, NULL, NOW(), NOW()),
(144, 84, 1, '12:30:00', '13:30:00', 15, NULL, NOW(), NOW()),
(145, 85, 3, '10:30:00', '11:30:00', 15, NULL, NOW(), NOW()),
(146, 86, 3, '12:30:00', '13:30:00', 15, NULL, NOW(), NOW()),
(147, 87, 5, '10:30:00', '11:30:00', 15, NULL, NOW(), NOW()),
(148, 88, 5, '12:30:00', '13:30:00', 15, NULL, NOW(), NOW());

-- Section 12 (12-ABM B) TTh
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(149, 83, 2, '10:30:00', '11:30:00', 15, NULL, NOW(), NOW()),
(150, 85, 2, '12:30:00', '13:30:00', 15, NULL, NOW(), NOW()),
(151, 87, 4, '10:30:00', '11:30:00', 15, NULL, NOW(), NOW()),
(152, 88, 4, '12:30:00', '13:30:00', 15, NULL, NOW(), NOW());
-- ============================================================
-- SEED DATA PART 4c: Teacher Subjects
-- Database: School_sms
-- ============================================================

INSERT INTO teacher_subject_tbl (idteacher_subject, teacher_id, subject_title, created_at) VALUES
(1, 1,  'English',            NOW()),
(2, 2,  'Mathematics',        NOW()),
(3, 3,  'Science',            NOW()),
(4, 4,  'Filipino',           NOW()),
(5, 5,  'Araling Panlipunan', NOW()),
(6, 6,  'TLE',                NOW()),
(7, 7,  'MAPEH',              NOW()),
(8, 8,  'ESP',                NOW()),
(9, 9,  'English',            NOW()),
(10, 10, 'Mathematics',       NOW()),
(11, 11, 'Science',           NOW()),
(12, 12, 'Filipino',          NOW()),
(13, 13, 'Araling Panlipunan',NOW()),
(14, 14, 'TLE',               NOW()),
(15, 15, 'MAPEH',             NOW()),
(16, 16, 'ESP',               NOW()),
(17, 17, 'English',           NOW()),
(18, 18, 'Mathematics',       NOW()),
(19, 19, 'Science',           NOW()),
(20, 20, 'Filipino',          NOW()),
(21, 21, 'Araling Panlipunan',NOW()),
(22, 22, 'TLE',               NOW()),
(23, 23, 'MAPEH',             NOW()),
(24, 24, 'ESP',               NOW()),
(25, 25, 'English',           NOW()),
(26, 26, 'Mathematics',       NOW()),
(27, 27, 'Science',           NOW()),
(28, 28, 'Filipino',          NOW()),
(29, 29, 'MAPEH',             NOW()),
(30, 30, 'TLE',               NOW());

-- ============================================================
-- SEED DATA PART 5: Recent attendance
-- Database: School_sms
-- ============================================================
-- One ended session for every scheduled class on each weekday of the last four weeks
-- (up to yesterday), so the dashboards, reports and exports have something to show.
-- Dates are relative to the day this script is run. Session times are stored in UTC
-- (Philippine time minus 8 hours), as the app stores them.

INSERT INTO attendance_sessions (class_offering_id, session_date, created_at, updated_at, status, room_id, section_name, subject_name)
WITH RECURSIVE seed_days AS (
    SELECT CURDATE() - INTERVAL 1 DAY AS d, 1 AS n
    UNION ALL
    SELECT d - INTERVAL 1 DAY, n + 1 FROM seed_days WHERE n < 28
)
SELECT cs.class_offering_id,
       TIMESTAMP(sd.d, cs.start_time) - INTERVAL 8 HOUR,
       TIMESTAMP(sd.d, cs.start_time) - INTERVAL 8 HOUR,
       TIMESTAMP(sd.d, cs.end_time) - INTERVAL 8 HOUR,
       'ENDED', cs.room_id, sec.name, sub.title
FROM seed_days sd
JOIN class_schedules cs ON cs.day_of_week = WEEKDAY(sd.d) + 1
JOIN class_offerings co ON co.Id = cs.class_offering_id
JOIN sections sec ON sec.Id = co.section_id
JOIN subjects sub ON sub.Id = co.subject_id;

-- One record per enrolled student per session (the table allows only one). Most are
-- present; about one student in eleven has a noticeably worse record, so the at-risk
-- list and the "attendance rate below" filters have matches.
INSERT INTO attendance_records (attendance_session_id, student_id, marked_by_user_id, status, reason, created_at, updated_at, hash)
SELECT x.session_id, x.student_id, x.user_id,
       CASE WHEN x.roll < 82 THEN 'PRESENT'
            WHEN x.roll < 91 THEN 'LATE'
            WHEN x.roll BETWEEN 97 AND 99 THEN 'EXCUSED'
            ELSE 'ABSENT' END,
       CASE WHEN x.roll BETWEEN 97 AND 99 THEN 'Excused: medical appointment' END,
       x.marked_at, x.marked_at,
       SHA2(CONCAT(x.session_id, ':', x.student_id, ':', x.marked_at), 256)
FROM (
    SELECT s.Id AS session_id, e.student_id, t.user_id,
           s.session_date + INTERVAL 5 MINUTE AS marked_at,
           (CRC32(CONCAT(s.Id, '-', e.student_id)) % 100) + IF(e.student_id % 11 = 0, 22, 0) AS roll
    FROM attendance_sessions s
    JOIN class_offerings co ON co.Id = s.class_offering_id
    JOIN teachers t ON t.Id = co.teacher_id
    JOIN enrollments e ON e.section_id = co.section_id
) x;
