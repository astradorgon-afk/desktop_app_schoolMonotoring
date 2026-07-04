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

-- Password hashes (PBKDF2):
-- admin password: PBKDF2$100000$bc/n4nBiwRHC5uMxiATBug==$IxOiO5DEm842CSzZNrtGIRFsilqCLOPkVLNv7OIx8L0=
-- password123: PBKDF2$100000$VWOPWwuLdFIolpljZDyuhA==$+ipd3UreYjxJkQHSiv6UCYlXCfYFYN7OQFi6uXDtymA=

SET @adminPw := 'PBKDF2$100000$bc/n4nBiwRHC5uMxiATBug==$IxOiO5DEm842CSzZNrtGIRFsilqCLOPkVLNv7OIx8L0=';
SET @teacherPw := 'PBKDF2$100000$VWOPWwuLdFIolpljZDyuhA==$+ipd3UreYjxJkQHSiv6UCYlXCfYFYN7OQFi6uXDtymA=';
SET @studentPw := 'PBKDF2$100000$VWOPWwuLdFIolpljZDyuhA==$+ipd3UreYjxJkQHSiv6UCYlXCfYFYN7OQFi6uXDtymA=';

-- Admin
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(1, 'System Admin', 'admin', @adminPw, 'SUPERADMIN', 'ACTIVE', 1, NOW(), NOW());

-- 30 Teachers
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(2,  'Maria Santos',      'teacher1',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(3,  'Juan dela Cruz',    'teacher2',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(4,  'Ana Gonzales',      'teacher3',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(5,  'Pedro Reyes',       'teacher4',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(6,  'Liza Mercado',      'teacher5',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(7,  'Carlos Villanueva', 'teacher6',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(8,  'Diana Lopez',       'teacher7',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(9,  'Eduardo Ramos',     'teacher8',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(10, 'Fe Bautista',       'teacher9',  @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(11, 'Gregorio Tan',      'teacher10', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(12, 'Helen Cruz',        'teacher11', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(13, 'Isidro Garcia',     'teacher12', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(14, 'Julia Fernandez',   'teacher13', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(15, 'Kevin Mendoza',     'teacher14', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(16, 'Linda Aquino',      'teacher15', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(17, 'Manuel Roxas',      'teacher16', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(18, 'Nena Alvarez',      'teacher17', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(19, 'Oscar Torres',      'teacher18', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(20, 'Patricia Lim',      'teacher19', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(21, 'Quirino Santos',    'teacher20', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(22, 'Rosa Villegas',     'teacher21', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(23, 'Samuel Diaz',       'teacher22', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(24, 'Teresa Flores',     'teacher23', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(25, 'Ulysses Rivera',    'teacher24', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(26, 'Vivian Castillo',   'teacher25', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(27, 'Wilfredo Gomez',    'teacher26', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(28, 'Xenia Navarro',     'teacher27', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(29, 'Yolanda Reyes',     'teacher28', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(30, 'Zachary Torres',    'teacher29', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW()),
(31, 'Aileen Mendoza',    'teacher30', @teacherPw, 'TEACHER', 'ACTIVE', 1, NOW(), NOW());

-- 99 Students (users 32-130)
INSERT INTO users (Id, name, username, password_hash, role, status, can_login, created_at, updated_at) VALUES
(32,  'Abigail Castro',    'student01', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(33,  'Benedict Santos',   'student02', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(34,  'Cassandra Lim',     'student03', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(35,  'Dante Villanueva',  'student04', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(36,  'Elena Gomez',       'student05', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(37,  'Francisco Reyes',   'student06', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(38,  'Gabriella Lopez',   'student07', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(39,  'Hector Mendoza',    'student08', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(40,  'Isabella Flores',   'student09', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(41,  'Jacob Rivera',      'student10', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(42,  'Katherine Tan',     'student11', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(43,  'Leonardo Cruz',     'student12', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(44,  'Megan Garcia',      'student13', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(45,  'Nathan Diaz',       'student14', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(46,  'Olivia Bautista',   'student15', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(47,  'Patrick Ramos',     'student16', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(48,  'Queenie Aquino',    'student17', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(49,  'Ramon Fernandez',   'student18', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(50,  'Sofia Mercado',     'student19', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(51,  'Tomas Alvarez',     'student20', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(52,  'Uma Gonzales',      'student21', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(53,  'Victor Tan',        'student22', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(54,  'Wendy Santos',      'student23', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(55,  'Xavier Villena',    'student24', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(56,  'Yvonne Ramirez',    'student25', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(57,  'Zandro Cruz',       'student26', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(58,  'Angela Dela Cruz',  'student27', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(59,  'Bernardo Lopez',    'student28', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(60,  'Catherine Ramos',   'student29', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(61,  'Dominic Torres',    'student30', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(62,  'Erica Villanueva',  'student31', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(63,  'Ferdinand Cruz',    'student32', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(64,  'Giselle Reyes',     'student33', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(65,  'Henry Navarro',     'student34', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(66,  'Iris Mendoza',      'student35', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(67,  'Jeremy Go',         'student36', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(68,  'Kristine Uy',       'student37', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(69,  'Luis Antonio',      'student38', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(70,  'Monica Reyes',      'student39', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(71,  'Noel Salvador',     'student40', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(72,  'Odessa Fernandez',  'student41', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(73,  'Paulo Gamboa',      'student42', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(74,  'Quinciano Lopez',   'student43', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(75,  'Rafaela Sison',     'student44', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(76,  'Stephanie Chua',    'student45', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(77,  'Timothy Reyes',     'student46', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(78,  'Ursula Tan',        'student47', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(79,  'Veronica Ponce',    'student48', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(80,  'William Sy',        'student49', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(81,  'Xena Morales',      'student50', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(82,  'Yuri Yamamoto',     'student51', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(83,  'Zion Garcia',       'student52', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(84,  'Alma Dela Torre',   'student53', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(85,  'Brian Uy',          'student54', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(86,  'Cindy Tan',         'student55', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(87,  'Darwin Estrada',    'student56', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(88,  'Eden Salvador',     'student57', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(89,  'Franklin Ong',      'student58', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(90,  'Grace Lim',         'student59', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(91,  'Harold Co',         'student60', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(92,  'Ireneo Dizon',      'student61', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(93,  'Jasmine Villanueva','student62', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(94,  'Kyle Chan',         'student63', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(95,  'Lara Quimpo',       'student64', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(96,  'Marco Sison',       'student65', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(97,  'Nikki Ramos',       'student66', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(98,  'Omar Hassan',       'student67', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(99,  'Pauline Uy',        'student68', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(100, 'Quentin Alcantara', 'student69', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(101, 'Rhea Mendoza',      'student70', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(102, 'Santi Gonzalez',    'student71', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(103, 'Trixie Samonte',    'student72', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(104, 'Uriel Manalo',      'student73', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(105, 'Vina Trinidad',     'student74', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(106, 'Winston Lozano',    'student75', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(107, 'Xyra Macapagal',    'student76', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(108, 'Yves Panganiban',   'student77', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(109, 'Zia Salcedo',       'student78', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(110, 'Alejandro Guevarra','student79', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(111, 'Beatrix Sarmiento', 'student80', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(112, 'Carlito Jacinto',   'student81', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(113, 'Darlene Castillo',  'student82', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(114, 'Emilio Aguirre',    'student83', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(115, 'Floribel Cortez',   'student84', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(116, 'Gregorio Lozada',   'student85', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(117, 'Hazel Pineda',      'student86', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(118, 'Ivan Castro',       'student87', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(119, 'Jenalyn Cruz',      'student88', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(120, 'Kiko Alcantara',    'student89', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(121, 'Lea Bustamante',    'student90', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(122, 'Miguel Valdez',     'student91', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(123, 'Nathalie Rivera',   'student92', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(124, 'Orlando Mendoza',   'student93', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(125, 'Perla Dimagiba',    'student94', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(126, 'Rogelio Apostol',   'student95', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(127, 'Shiela Tecson',     'student96', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(128, 'Tyrone Basa',       'student97', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(129, 'Vilma Santos',      'student98', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW()),
(130, 'Wally Bayola',      'student99', @studentPw, 'STUDENT', 'ACTIVE', 1, NOW(), NOW());
-- ============================================================
-- SEED DATA PART 3a: Teachers & Sections
-- Database: School_sms
-- ============================================================

-- 6. TEACHERS (30 teachers)
INSERT INTO teachers (Id, user_id, employee_no, first_name, last_name, middle_name, email, contact_no, hire_date, status, specialization, employment_status, created_at, updated_at) VALUES
(1, 2,  'EMP-001', 'Maria',    'Santos',     'Cruz',     'maria.santos@school.edu',    '09171234561', '2018-06-01', 'ACTIVE', 'English',           'PERMANENT', NOW(), NOW()),
(2, 3,  'EMP-002', 'Juan',     'dela Cruz',  'Ramos',    'juan.delacruz@school.edu',   '09171234562', '2019-06-01', 'ACTIVE', 'Mathematics',       'PERMANENT', NOW(), NOW()),
(3, 4,  'EMP-003', 'Ana',      'Gonzales',   'Lopez',    'ana.gonzales@school.edu',    '09171234563', '2019-06-01', 'ACTIVE', 'Science',           'PERMANENT', NOW(), NOW()),
(4, 5,  'EMP-004', 'Pedro',    'Reyes',      'Tan',      'pedro.reyes@school.edu',     '09171234564', '2020-06-01', 'ACTIVE', 'Filipino',          'PERMANENT', NOW(), NOW()),
(5, 6,  'EMP-005', 'Liza',     'Mercado',    'Villanueva','liza.mercado@school.edu',   '09171234565', '2020-06-01', 'ACTIVE', 'Araling Panlipunan','PERMANENT', NOW(), NOW()),
(6, 7,  'EMP-006', 'Carlos',   'Villanueva', 'Cruz',     'carlos.villanueva@school.edu','09171234566','2020-06-01','ACTIVE', 'TLE',              'PERMANENT', NOW(), NOW()),
(7, 8,  'EMP-007', 'Diana',    'Lopez',      'Garcia',   'diana.lopez@school.edu',     '09171234567', '2021-06-01', 'ACTIVE', 'MAPEH',             'PERMANENT', NOW(), NOW()),
(8, 9,  'EMP-008', 'Eduardo',  'Ramos',      'Santos',   'eduardo.ramos@school.edu',   '09171234568', '2021-06-01', 'ACTIVE', 'ESP',               'PERMANENT', NOW(), NOW()),
(9, 10, 'EMP-009', 'Fe',       'Bautista',   'Domingo',  'fe.bautista@school.edu',     '09171234569', '2022-06-01', 'ACTIVE', 'English',           'PROBATIONARY', NOW(), NOW()),
(10, 11,'EMP-010', 'Gregorio', 'Tan',        'Sy',       'gregorio.tan@school.edu',    '09171234570', '2022-06-01', 'ACTIVE', 'Mathematics',       'PROBATIONARY', NOW(), NOW()),
(11, 12,'EMP-011', 'Helen',    'Cruz',       'Mendoza',  'helen.cruz@school.edu',      '09171234571', '2022-06-01', 'ACTIVE', 'Science',           'PROBATIONARY', NOW(), NOW()),
(12, 13,'EMP-012', 'Isidro',   'Garcia',     'Rivera',   'isidro.garcia@school.edu',   '09171234572', '2023-06-01', 'ACTIVE', 'Filipino',          'PROBATIONARY', NOW(), NOW()),
(13, 14,'EMP-013', 'Julia',    'Fernandez',  'Lim',      'julia.fernandez@school.edu', '09171234573', '2023-06-01', 'ACTIVE', 'Araling Panlipunan','PROBATIONARY', NOW(), NOW()),
(14, 15,'EMP-014', 'Kevin',    'Mendoza',    'Torres',   'kevin.mendoza@school.edu',   '09171234574', '2023-06-01', 'ACTIVE', 'TLE',              'PROBATIONARY', NOW(), NOW()),
(15, 16,'EMP-015', 'Linda',    'Aquino',     'Reyes',    'linda.aquino@school.edu',    '09171234575', '2018-06-01', 'ACTIVE', 'MAPEH',             'PERMANENT', NOW(), NOW()),
(16, 17,'EMP-016', 'Manuel',   'Roxas',      'Castro',   'manuel.roxas@school.edu',    '09171234576', '2019-06-01', 'ACTIVE', 'ESP',               'PERMANENT', NOW(), NOW()),
(17, 18,'EMP-017', 'Nena',     'Alvarez',    'Sison',    'nena.alvarez@school.edu',    '09171234577', '2021-06-01', 'ACTIVE', 'English',           'PERMANENT', NOW(), NOW()),
(18, 19,'EMP-018', 'Oscar',    'Torres',     'Uy',       'oscar.torres@school.edu',    '09171234578', '2020-06-01', 'ACTIVE', 'Mathematics',       'PERMANENT', NOW(), NOW()),
(19, 20,'EMP-019', 'Patricia', 'Lim',        'Ong',      'patricia.lim@school.edu',    '09171234579', '2022-06-01', 'ACTIVE', 'Science',           'PROBATIONARY', NOW(), NOW()),
(20, 21,'EMP-020', 'Quirino',  'Santos',     'Diaz',     'quirino.santos@school.edu',  '09171234580', '2022-06-01', 'ACTIVE', 'Filipino',          'PROBATIONARY', NOW(), NOW()),
(21, 22,'EMP-021', 'Rosa',     'Villegas',   'Roque',    'rosa.villegas@school.edu',   '09171234581', '2023-06-01', 'ACTIVE', 'Araling Panlipunan','PROBATIONARY', NOW(), NOW()),
(22, 23,'EMP-022', 'Samuel',   'Diaz',       'Luna',     'samuel.diaz@school.edu',     '09171234582', '2023-06-01', 'ACTIVE', 'TLE',              'PROBATIONARY', NOW(), NOW()),
(23, 24,'EMP-023', 'Teresa',   'Flores',     'Magsaysay','teresa.flores@school.edu',   '09171234583', '2018-06-01', 'ACTIVE', 'MAPEH',             'PERMANENT', NOW(), NOW()),
(24, 25,'EMP-024', 'Ulysses',  'Rivera',     'Lopez',    'ulysses.rivera@school.edu',  '09171234584', '2019-06-01', 'ACTIVE', 'ESP',               'PERMANENT', NOW(), NOW()),
(25, 26,'EMP-025', 'Vivian',   'Castillo',   'Ngo',      'vivian.castillo@school.edu', '09171234585', '2020-06-01', 'ACTIVE', 'English',           'PERMANENT', NOW(), NOW()),
(26, 27,'EMP-026', 'Wilfredo', 'Gomez',      'Yap',      'wilfredo.gomez@school.edu',  '09171234586', '2021-06-01', 'ACTIVE', 'Mathematics',       'PERMANENT', NOW(), NOW()),
(27, 28,'EMP-027', 'Xenia',    'Navarro',    'Tan',      'xenia.navarro@school.edu',   '09171234587', '2022-06-01', 'ACTIVE', 'Science',           'PROBATIONARY', NOW(), NOW()),
(28, 29,'EMP-028', 'Yolanda',  'Reyes',      'Tecson',   'yolanda.reyes@school.edu',   '09171234588', '2023-06-01', 'ACTIVE', 'Filipino',          'PROBATIONARY', NOW(), NOW()),
(29, 30,'EMP-029', 'Zachary',  'Torres',     'Cruz',     'zachary.torres@school.edu',  '09171234589', '2023-06-01', 'ACTIVE', 'MAPEH',             'PROBATIONARY', NOW(), NOW()),
(30, 31,'EMP-030', 'Aileen',   'Mendoza',    'Flores',   'aileen.mendoza@school.edu',  '09171234590', '2021-06-01', 'ACTIVE', 'TLE',               'PERMANENT', NOW(), NOW());

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
(1,  32, '150001000001', 'student01', 'Abigail',    'Castro',     'Santos',    '2013-03-15', 'Female', 'Block 1 Lot 1, Brgy. San Juan', 'Marilou Castro',    '09170000001', 'ACTIVE', 12, '09170001001', NOW(), NOW()),
(2,  33, '150001000002', 'student02', 'Benedict',   'Santos',     'Reyes',     '2012-11-20', 'Male',   '123 Rizal St.',                  'Roberto Santos',    '09170000002', 'ACTIVE', 13, '09170001002', NOW(), NOW()),
(3,  34, '150001000003', 'student03', 'Cassandra',  'Lim',        'Tan',       '2013-07-08', 'Female', 'Unit 5, Sunshine Subd.',         'Grace Lim',         '09170000003', 'ACTIVE', 12, '09170001003', NOW(), NOW()),
(4,  35, '150001000004', 'student04', 'Dante',      'Villanueva', 'Cruz',      '2013-01-30', 'Male',   '456 Mabini St.',                 'Dante Villanueva Sr.','09170000004', 'ACTIVE', 13, '09170001004', NOW(), NOW()),
(5,  36, '150001000005', 'student05', 'Elena',      'Gomez',      'Lopez',     '2012-09-14', 'Female', '789 Bonifacio Ave.',              'Elena Gomez Sr.',   '09170000005', 'ACTIVE', 13, '09170001005', NOW(), NOW()),
(6,  37, '150001000006', 'student06', 'Francisco',  'Reyes',      'Tan',       '2013-05-22', 'Male',   '10 Veterans Lane',               'Francisco Reyes Sr.','09170000006', 'ACTIVE', 12, '09170001006', NOW(), NOW()),
(7,  38, '150001000007', 'student07', 'Gabriella',  'Lopez',      'Garcia',    '2012-12-03', 'Female', '22 Peace St.',                    'Gabriella Lopez Sr.','09170000007', 'ACTIVE', 13, '09170001007', NOW(), NOW()),
(8,  39, '150001000008', 'student08', 'Hector',     'Mendoza',    'Rivera',    '2013-08-19', 'Male',   '88 Freedom Blvd.',                'Hector Mendoza Sr.', '09170000008', 'ACTIVE', 12, '09170001008', NOW(), NOW());

-- Section 2 (7-B): students 9-16
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(9,  40, '150001000009', 'student09', 'Isabella',   'Flores',     'Magsaysay', '2012-10-10', 'Female', '15 Orchid St.',                  'Isabella Flores Sr.', '09170000009', 'ACTIVE', 13, '09170001009', NOW(), NOW()),
(10, 41, '150001000010', 'student10', 'Jacob',      'Rivera',     'Lopez',     '2013-04-25', 'Male',   '7A Dahlia Ave.',                  'Jacob Rivera Sr.',   '09170000010', 'ACTIVE', 12, '09170001010', NOW(), NOW()),
(11, 42, '150001000011', 'student11', 'Katherine',  'Tan',        'Sy',        '2012-08-14', 'Female', '33 Sunflower Rd.',                'Katherine Tan Sr.',  '09170000011', 'ACTIVE', 13, '09170001011', NOW(), NOW()),
(12, 43, '150001000012', 'student12', 'Leonardo',   'Cruz',       'Dela Cruz', '2013-02-28', 'Male',   '99 Sampaguita St.',               'Leonardo Cruz Sr.',  '09170000012', 'ACTIVE', 12, '09170001012', NOW(), NOW()),
(13, 44, '150001000013', 'student13', 'Megan',      'Garcia',     'Co',        '2012-06-17', 'Female', '14 Rose Ave.',                    'Megan Garcia Sr.',   '09170000013', 'ACTIVE', 13, '09170001013', NOW(), NOW()),
(14, 45, '150001000014', 'student14', 'Nathan',     'Diaz',       'Luna',      '2013-09-05', 'Male',   '55 Tulip Blvd.',                  'Nathan Diaz Sr.',    '09170000014', 'ACTIVE', 12, '09170001014', NOW(), NOW()),
(15, 46, '150001000015', 'student15', 'Olivia',     'Bautista',   'Domingo',   '2012-11-30', 'Female', '21 Jasmin Way',                   'Olivia Bautista Sr.', '09170000015', 'ACTIVE', 13, '09170001015', NOW(), NOW()),
(16, 47, '150001000016', 'student16', 'Patrick',    'Ramos',      'Santos',    '2013-01-12', 'Male',   '76 Camia St.',                    'Patrick Ramos Sr.',  '09170000016', 'ACTIVE', 12, '09170001016', NOW(), NOW());

-- Section 3 (8-A): students 17-24
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(17, 48, '150001000017', 'student17', 'Queenie',    'Aquino',     'Reyes',     '2011-05-12', 'Female', '44 Narra St.',                   'Queenie Aquino Sr.', '09170000017', 'ACTIVE', 14, '09170001017', NOW(), NOW()),
(18, 49, '150001000018', 'student18', 'Ramon',      'Fernandez',  'Lim',       '2011-09-23', 'Male',   '17 Mahogany Ave.',                'Ramon Fernandez Sr.','09170000018', 'ACTIVE', 14, '09170001018', NOW(), NOW()),
(19, 50, '150001000019', 'student19', 'Sofia',      'Mercado',    'Villanueva','2011-03-08', 'Female', '66 Acacia Blvd.',                 'Sofia Mercado Sr.',  '09170000019', 'ACTIVE', 14, '09170001019', NOW(), NOW()),
(20, 51, '150001000020', 'student20', 'Tomas',      'Alvarez',    'Sison',     '2011-07-19', 'Male',   '8B Molave St.',                   'Tomas Alvarez Sr.',  '09170000020', 'ACTIVE', 14, '09170001020', NOW(), NOW()),
(21, 52, '150001000021', 'student21', 'Uma',        'Gonzales',   'Lopez',     '2011-12-25', 'Female', '29 IPil-IPil Rd.',                'Uma Gonzales Sr.',   '09170000021', 'ACTIVE', 14, '09170001021', NOW(), NOW()),
(22, 53, '150001000022', 'student22', 'Victor',     'Tan',        'Ong',       '2011-02-14', 'Male',   '50 Yakal St.',                    'Victor Tan Sr.',     '09170000022', 'ACTIVE', 14, '09170001022', NOW(), NOW()),
(23, 54, '150001000023', 'student23', 'Wendy',      'Santos',     'Dela Cruz', '2011-08-30', 'Female', '12 Narra Ave.',                   'Wendy Santos Sr.',   '09170000023', 'ACTIVE', 14, '09170001023', NOW(), NOW()),
(24, 55, '150001000024', 'student24', 'Xavier',     'Villena',    'Cruz',      '2011-04-17', 'Male',   '37 Mahogany St.',                 'Xavier Villena Sr.', '09170000024', 'ACTIVE', 14, '09170001024', NOW(), NOW());

-- Section 4 (8-B): students 25-32
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(25, 56, '150001000025', 'student25', 'Yvonne',     'Ramirez',    'Torres',    '2011-06-11', 'Female', '88A Molave St.',                  'Yvonne Ramirez Sr.', '09170000025', 'ACTIVE', 14, '09170001025', NOW(), NOW()),
(26, 57, '150001000026', 'student26', 'Zandro',     'Cruz',       'Mendoza',   '2011-10-05', 'Male',   '23 IPil-IPil St.',                'Zandro Cruz Sr.',    '09170000026', 'ACTIVE', 14, '09170001026', NOW(), NOW()),
(27, 58, '150001000027', 'student27', 'Angela',     'Dela Cruz',  'Ramos',     '2011-01-29', 'Female', '34 Yakal Ave.',                   'Angela Dela Cruz Sr.', '09170000027','ACTIVE', 14, '09170001027', NOW(), NOW()),
(28, 59, '150001000028', 'student28', 'Bernardo',   'Lopez',      'Garcia',    '2012-03-22', 'Male',   '67 Narra Blvd.',                  'Bernardo Lopez Sr.', '09170000028', 'ACTIVE', 14, '09170001028', NOW(), NOW()),
(29, 60, '150001000029', 'student29', 'Catherine',  'Ramos',      'Santos',    '2011-09-15', 'Female', '9B Mahogany St.',                 'Catherine Ramos Sr.','09170000029', 'ACTIVE', 14, '09170001029', NOW(), NOW()),
(30, 61, '150001000030', 'student30', 'Dominic',    'Torres',     'Uy',        '2011-11-08', 'Male',   '52 Acacia St.',                   'Dominic Torres Sr.', '09170000030', 'ACTIVE', 14, '09170001030', NOW(), NOW()),
(31, 62, '150001000031', 'student31', 'Erica',      'Villanueva', 'Cruz',      '2011-07-02', 'Female', '41 Molave Blvd.',                 'Erica Villanueva Sr.','09170000031', 'ACTIVE', 14, '09170001031', NOW(), NOW()),
(32, 63, '150001000032', 'student32', 'Ferdinand',  'Cruz',       'Rivera',    '2011-04-09', 'Male',   '18 IPil-IPil Ave.',               'Ferdinand Cruz Sr.', '09170000032', 'ACTIVE', 14, '09170001032', NOW(), NOW());

-- Section 5 (9-A): students 33-40
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(33, 64, '150001000033', 'student33', 'Giselle',    'Reyes',      'Tan',       '2010-03-18', 'Female', '11 Diamond St.',                  'Giselle Reyes Sr.',  '09170000033', 'ACTIVE', 15, '09170001033', NOW(), NOW()),
(34, 65, '150001000034', 'student34', 'Henry',      'Navarro',    'Tan',       '2010-08-27', 'Male',   '63 Ruby Ave.',                    'Henry Navarro Sr.',  '09170000034', 'ACTIVE', 15, '09170001034', NOW(), NOW()),
(35, 66, '150001000035', 'student35', 'Iris',       'Mendoza',    'Flores',    '2010-12-14', 'Female', '26 Emerald St.',                  'Iris Mendoza Sr.',   '09170000035', 'ACTIVE', 15, '09170001035', NOW(), NOW()),
(36, 67, '150001000036', 'student36', 'Jeremy',     'Go',         'Chua',      '2010-06-30', 'Male',   '74 Sapphire Blvd.',               'Jeremy Go Sr.',      '09170000036', 'ACTIVE', 15, '09170001036', NOW(), NOW()),
(37, 68, '150001000037', 'student37', 'Kristine',   'Uy',         'Lim',       '2010-02-09', 'Female', '19 Pearl Ave.',                   'Kristine Uy Sr.',    '09170000037', 'ACTIVE', 15, '09170001037', NOW(), NOW()),
(38, 69, '150001000038', 'student38', 'Luis',       'Antonio',    'Cruz',      '2010-10-22', 'Male',   '82 Jade St.',                     'Luis Antonio Sr.',   '09170000038', 'ACTIVE', 15, '09170001038', NOW(), NOW()),
(39, 70, '150001000039', 'student39', 'Monica',     'Reyes',      'Garcia',    '2010-05-05', 'Female', '5 Topaz Ave.',                    'Monica Reyes Sr.',   '09170000039', 'ACTIVE', 15, '09170001039', NOW(), NOW()),
(40, 71, '150001000040', 'student40', 'Noel',       'Salvador',   'Dela Torre','2010-01-11', 'Male',   '39 Onyx St.',                     'Noel Salvador Sr.',  '09170000040', 'ACTIVE', 15, '09170001040', NOW(), NOW());

-- Section 6 (9-B): students 41-48
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(41, 72, '150001000041', 'student41', 'Odessa',     'Fernandez',  'Lim',       '2010-09-03', 'Female', '58 Garnet Rd.',                   'Odessa Fernandez Sr.','09170000041', 'ACTIVE', 15, '09170001041', NOW(), NOW()),
(42, 73, '150001000042', 'student42', 'Paulo',      'Gamboa',     'Mendoza',   '2010-04-28', 'Male',   '16 Amethyst St.',                 'Paulo Gamboa Sr.',   '09170000042', 'ACTIVE', 15, '09170001042', NOW(), NOW()),
(43, 74, '150001000043', 'student43', 'Quinciano',  'Lopez',      'Rivera',    '2010-11-15', 'Male',   '90 Coral Blvd.',                  'Quinciano Lopez Sr.','09170000043', 'ACTIVE', 15, '09170001043', NOW(), NOW()),
(44, 75, '150001000044', 'student44', 'Rafaela',    'Sison',      'Cruz',      '2010-07-07', 'Female', '3 Pearl St.',                     'Rafaela Sison Sr.',  '09170000044', 'ACTIVE', 15, '09170001044', NOW(), NOW()),
(45, 76, '150001000045', 'student45', 'Stephanie',  'Chua',       'Go',        '2010-01-26', 'Female', '47 Amber Ave.',                   'Stephanie Chua Sr.', '09170000045', 'ACTIVE', 15, '09170001045', NOW(), NOW()),
(46, 77, '150001000046', 'student46', 'Timothy',    'Reyes',      'Santos',    '2010-12-19', 'Male',   '73 Quartz St.',                   'Timothy Reyes Sr.',  '09170000046', 'ACTIVE', 15, '09170001046', NOW(), NOW()),
(47, 78, '150001000047', 'student47', 'Ursula',     'Tan',        'Sy',        '2010-08-01', 'Female', '12 Opal Blvd.',                   'Ursula Tan Sr.',     '09170000047', 'ACTIVE', 15, '09170001047', NOW(), NOW()),
(48, 79, '150001000048', 'student48', 'Veronica',   'Ponce',      'Navarro',   '2010-06-13', 'Female', '65 Platinum St.',                 'Veronica Ponce Sr.', '09170000048', 'ACTIVE', 15, '09170001048', NOW(), NOW());

-- Section 7 (10-A): students 49-56
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(49, 80, '150001000049', 'student49', 'William',    'Sy',         'Co',        '2009-04-20', 'Male',   '28 Silver St.',                   'William Sy Sr.',     '09170000049', 'ACTIVE', 16, '09170001049', NOW(), NOW()),
(50, 81, '150001000050', 'student50', 'Xena',       'Morales',    'Reyes',     '2009-09-15', 'Female', '54 Gold Ave.',                    'Xena Morales Sr.',   '09170000050', 'ACTIVE', 16, '09170001050', NOW(), NOW()),
(51, 82, '150001000051', 'student51', 'Yuri',       'Yamamoto',   'Tanaka',    '2009-12-01', 'Male',   '86 Bronze Blvd.',                 'Yuri Yamamoto Sr.',  '09170000051', 'ACTIVE', 16, '09170001051', NOW(), NOW()),
(52, 83, '150001000052', 'student52', 'Zion',       'Garcia',     'Lopez',     '2009-07-28', 'Male',   '13 Copper St.',                   'Zion Garcia Sr.',    '09170000052', 'ACTIVE', 16, '09170001052', NOW(), NOW()),
(53, 84, '150001000053', 'student53', 'Alma',       'Dela Torre', 'Santos',    '2009-02-14', 'Female', '42 Nickel Ave.',                  'Alma Dela Torre Sr.','09170000053', 'ACTIVE', 16, '09170001053', NOW(), NOW()),
(54, 85, '150001000054', 'student54', 'Brian',      'Uy',         'Lim',       '2009-10-09', 'Male',   '70 Zinc St.',                     'Brian Uy Sr.',       '09170000054', 'ACTIVE', 16, '09170001054', NOW(), NOW()),
(55, 86, '150001000055', 'student55', 'Cindy',      'Tan',        'Ong',       '2009-05-22', 'Female', '36 Tin Rd.',                      'Cindy Tan Sr.',      '09170000055', 'ACTIVE', 16, '09170001055', NOW(), NOW()),
(56, 87, '150001000056', 'student56', 'Darwin',     'Estrada',    'Cruz',      '2009-01-07', 'Male',   '61 Lead Blvd.',                   'Darwin Estrada Sr.', '09170000056', 'ACTIVE', 16, '09170001056', NOW(), NOW());

-- Section 8 (10-B): students 57-64
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(57, 88, '150001000057', 'student57', 'Eden',       'Salvador',   'Cruz',      '2009-08-03', 'Female', '32 Mercury St.',                  'Eden Salvador Sr.',  '09170000057', 'ACTIVE', 16, '09170001057', NOW(), NOW()),
(58, 89, '150001000058', 'student58', 'Franklin',   'Ong',        'Tan',       '2009-11-19', 'Male',   '78 Venus Ave.',                   'Franklin Ong Sr.',   '09170000058', 'ACTIVE', 16, '09170001058', NOW(), NOW()),
(59, 90, '150001000059', 'student59', 'Grace',      'Lim',        'Chua',      '2009-03-31', 'Female', '25 Mars Blvd.',                   'Grace Lim Sr.',      '09170000059', 'ACTIVE', 16, '09170001059', NOW(), NOW()),
(60, 91, '150001000060', 'student60', 'Harold',     'Co',         'Sy',        '2009-06-16', 'Male',   '49 Jupiter St.',                  'Harold Co Sr.',      '09170000060', 'ACTIVE', 16, '09170001060', NOW(), NOW()),
(61, 92, '150001000061', 'student61', 'Ireneo',     'Dizon',      'Mendoza',   '2009-12-28', 'Male',   '83 Saturn Ave.',                  'Ireneo Dizon Sr.',   '09170000061', 'ACTIVE', 16, '09170001061', NOW(), NOW()),
(62, 93, '150001000062', 'student62', 'Jasmine',    'Villanueva', 'Ramos',     '2009-10-11', 'Female', '15 Uranus Rd.',                   'Jasmine Villanueva Sr.','09170000062', 'ACTIVE', 16, '09170001062', NOW(), NOW()),
(63, 94, '150001000063', 'student63', 'Kyle',       'Chan',       'Dy',        '2009-07-04', 'Male',   '59 Neptune St.',                  'Kyle Chan Sr.',      '09170000063', 'ACTIVE', 16, '09170001063', NOW(), NOW()),
(64, 95, '150001000064', 'student64', 'Lara',       'Quimpo',     'Gonzales',  '2009-04-12', 'Female', '22 Pluto Blvd.',                  'Lara Quimpo Sr.',    '09170000064', 'ACTIVE', 16, '09170001064', NOW(), NOW());

-- Section 9 (11-STEM A): students 65-72
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(65, 96,  '150001000065', 'student65', 'Marco',      'Sison',       'Cruz',     '2008-05-10', 'Male',   '46 Proton St.',                   'Marco Sison Sr.',    '09170000065', 'ACTIVE', 17, '09170001065', NOW(), NOW()),
(66, 97,  '150001000066', 'student66', 'Nikki',      'Ramos',       'Tan',      '2008-09-22', 'Female', '71 Neutron Ave.',                 'Nikki Ramos Sr.',    '09170000066', 'ACTIVE', 17, '09170001066', NOW(), NOW()),
(67, 98,  '150001000067', 'student67', 'Omar',       'Hassan',      'Malik',    '2008-02-18', 'Male',   '38 Electron Blvd.',               'Omar Hassan Sr.',    '09170000067', 'ACTIVE', 17, '09170001067', NOW(), NOW()),
(68, 99,  '150001000068', 'student68', 'Pauline',    'Uy',          'Lim',      '2008-08-07', 'Female', '64 Atom St.',                     'Pauline Uy Sr.',     '09170000068', 'ACTIVE', 17, '09170001068', NOW(), NOW()),
(69, 100, '150001000069', 'student69', 'Quentin',    'Alcantara',   'Reyes',    '2008-12-13', 'Male',   '20 Mole Ave.',                    'Quentin Alcantara Sr.','09170000069','ACTIVE', 17, '09170001069', NOW(), NOW()),
(70, 101, '150001000070', 'student70', 'Rhea',       'Mendoza',     'Garcia',   '2008-06-29', 'Female', '52 Cell St.',                     'Rhea Mendoza Sr.',   '09170000070', 'ACTIVE', 17, '09170001070', NOW(), NOW()),
(71, 102, '150001000071', 'student71', 'Santi',      'Gonzalez',    'Castro',   '2008-03-25', 'Male',   '85 Nucleus Blvd.',                'Santi Gonzalez Sr.', '09170000071', 'ACTIVE', 17, '09170001071', NOW(), NOW()),
(72, 103, '150001000072', 'student72', 'Trixie',     'Samonte',     'Dela Cruz','2008-10-01', 'Female', '40 Orbit Rd.',                    'Trixie Samonte Sr.', '09170000072', 'ACTIVE', 17, '09170001072', NOW(), NOW());

-- Section 10 (11-ABM B): students 73-80
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(73, 104, '150001000073', 'student73', 'Uriel',      'Manalo',      'Dizon',    '2008-04-14', 'Male',   '31 Asset St.',                    'Uriel Manalo Sr.',   '09170000073', 'ACTIVE', 17, '09170001073', NOW(), NOW()),
(74, 105, '150001000074', 'student74', 'Vina',       'Trinidad',    'Sison',    '2008-11-26', 'Female', '68 Liability Ave.',               'Vina Trinidad Sr.',  '09170000074', 'ACTIVE', 17, '09170001074', NOW(), NOW()),
(75, 106, '150001000075', 'student75', 'Winston',    'Lozano',      'Cruz',     '2008-07-09', 'Male',   '14 Equity Blvd.',                 'Winston Lozano Sr.', '09170000075', 'ACTIVE', 17, '09170001075', NOW(), NOW()),
(76, 107, '150001000076', 'student76', 'Xyra',       'Macapagal',   'Ramos',    '2008-01-30', 'Female', '53 Capital St.',                  'Xyra Macapagal Sr.', '09170000076', 'ACTIVE', 17, '09170001076', NOW(), NOW()),
(77, 108, '150001000077', 'student77', 'Yves',       'Panganiban',  'Lopez',    '2008-09-17', 'Male',   '7 Market Ave.',                   'Yves Panganiban Sr.','09170000077', 'ACTIVE', 17, '09170001077', NOW(), NOW()),
(78, 109, '150001000078', 'student78', 'Zia',        'Salcedo',     'Tan',      '2008-02-22', 'Female', '91 Trade Blvd.',                  'Zia Salcedo Sr.',    '09170000078', 'ACTIVE', 17, '09170001078', NOW(), NOW()),
(79, 110, '150001000079', 'student79', 'Alejandro',  'Guevarra',    'Santos',   '2008-08-11', 'Male',   '27 Finance St.',                  'Alejandro Guevarra Sr.','09170000079','ACTIVE', 17, '09170001079', NOW(), NOW()),
(80, 111, '150001000080', 'student80', 'Beatrix',    'Sarmiento',   'Flores',   '2008-05-19', 'Female', '62 Budget Rd.',                   'Beatrix Sarmiento Sr.','09170000080','ACTIVE', 17, '09170001080', NOW(), NOW());

-- Section 11 (12-STEM A): students 81-88
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(81, 112, '150001000081', 'student81', 'Carlito',    'Jacinto',     'Reyes',    '2007-06-05', 'Male',   '44 Thesis St.',                   'Carlito Jacinto Sr.','09170000081', 'ACTIVE', 18, '09170001081', NOW(), NOW()),
(82, 113, '150001000082', 'student82', 'Darlene',    'Castillo',    'Mendoza',  '2007-10-21', 'Female', '77 Defense Ave.',                 'Darlene Castillo Sr.','09170000082','ACTIVE', 18, '09170001082', NOW(), NOW()),
(83, 114, '150001000083', 'student83', 'Emilio',     'Aguirre',     'Cruz',     '2007-03-15', 'Male',   '19 Capstone Blvd.',               'Emilio Aguirre Sr.', '09170000083', 'ACTIVE', 18, '09170001083', NOW(), NOW()),
(84, 115, '150001000084', 'student84', 'Floribel',   'Cortez',      'Lopez',    '2007-09-09', 'Female', '58 Research St.',                 'Floribel Cortez Sr.','09170000084', 'ACTIVE', 18, '09170001084', NOW(), NOW()),
(85, 116, '150001000085', 'student85', 'Gregorio',   'Lozada',      'Tan',      '2007-01-28', 'Male',   '23 Practicum Ave.',               'Gregorio Lozada Sr.','09170000085', 'ACTIVE', 18, '09170001085', NOW(), NOW()),
(86, 117, '150001000086', 'student86', 'Hazel',      'Pineda',      'Rivera',   '2007-07-13', 'Female', '35 Defense Blvd.',                'Hazel Pineda Sr.',   '09170000086', 'ACTIVE', 18, '09170001086', NOW(), NOW()),
(87, 118, '150001000087', 'student87', 'Ivan',       'Castro',      'Garcia',   '2007-11-30', 'Male',   '81 Capstone St.',                 'Ivan Castro Sr.',    '09170000087', 'ACTIVE', 18, '09170001087', NOW(), NOW()),
(88, 119, '150001000088', 'student88', 'Jenalyn',    'Cruz',        'Santos',   '2007-04-17', 'Female', '12 Thesis Ave.',                  'Jenalyn Cruz Sr.',   '09170000088', 'ACTIVE', 18, '09170001088', NOW(), NOW());

-- Section 12 (12-ABM B): students 89-99 (11 students)
INSERT INTO students (Id, user_id, lrn, student_number, first_name, last_name, middle_name, birthdate, sex, address, guardian_name, guardian_contact, status, age, contact_no, created_at, updated_at) VALUES
(89, 120, '150001000089', 'student89', 'Kiko',       'Alcantara',   'Reyes',    '2007-08-02', 'Male',   '49 Business St.',                 'Kiko Alcantara Sr.', '09170000089', 'ACTIVE', 18, '09170001089', NOW(), NOW()),
(90, 121, '150001000090', 'student90', 'Lea',        'Bustamante',  'Tan',      '2007-02-20', 'Female', '74 Management Ave.',              'Lea Bustamante Sr.', '09170000090', 'ACTIVE', 18, '09170001090', NOW(), NOW()),
(91, 122, '150001000091', 'student91', 'Miguel',     'Valdez',      'Lim',      '2007-10-08', 'Male',   '16 Marketing Blvd.',              'Miguel Valdez Sr.',  '09170000091', 'ACTIVE', 18, '09170001091', NOW(), NOW()),
(92, 123, '150001000092', 'student92', 'Nathalie',   'Rivera',      'Ong',      '2007-05-26', 'Female', '63 Accounting St.',               'Nathalie Rivera Sr.','09170000092', 'ACTIVE', 18, '09170001092', NOW(), NOW()),
(93, 124, '150001000093', 'student93', 'Orlando',    'Mendoza',     'Sison',    '2007-12-14', 'Male',   '29 Economics Ave.',               'Orlando Mendoza Sr.','09170000093', 'ACTIVE', 18, '09170001093', NOW(), NOW()),
(94, 125, '150001000094', 'student94', 'Perla',      'Dimagiba',    'Cruz',     '2007-06-30', 'Female', '55 Finance Blvd.',                'Perla Dimagiba Sr.', '09170000094', 'ACTIVE', 18, '09170001094', NOW(), NOW()),
(95, 126, '150001000095', 'student95', 'Rogelio',    'Apostol',     'Ramos',    '2007-03-11', 'Male',   '8 Governance St.',                'Rogelio Apostol Sr.','09170000095', 'ACTIVE', 18, '09170001095', NOW(), NOW()),
(96, 127, '150001000096', 'student96', 'Shiela',     'Tecson',      'Dela Cruz','2007-09-27', 'Female', '41 Tax Ave.',                     'Shiela Tecson Sr.',  '09170000096', 'ACTIVE', 18, '09170001096', NOW(), NOW()),
(97, 128, '150001000097', 'student97', 'Tyrone',     'Basa',        'Villanueva','2007-01-05', 'Male',   '70 Law Blvd.',                    'Tyrone Basa Sr.',    '09170000097', 'ACTIVE', 18, '09170001097', NOW(), NOW()),
(98, 129, '150001000098', 'student98', 'Vilma',      'Santos',      'Lopez',    '2007-11-19', 'Female', '33 Ethics St.',                   'Vilma Santos Sr.',   '09170000098', 'ACTIVE', 18, '09170001098', NOW(), NOW()),
(99, 130, '150001000099', 'student99', 'Wally',      'Bayola',      'Cruz',     '2007-07-22', 'Male',   '86 Policy Ave.',                  'Wally Bayola Sr.',   '09170000099', 'ACTIVE', 18, '09170001099', NOW(), NOW());
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
(9, 1, 9, 1, 2, 1, 'ACTIVE', NOW(), NOW(), NOW()),
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
(1,  1, 1, 1,  1,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(2,  1, 1, 2,  1,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(3,  1, 1, 3,  3,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(4,  1, 1, 4,  4,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(5,  1, 1, 5,  5,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(6,  1, 1, 6,  6,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(7,  1, 1, 7,  7,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW()),
(8,  1, 1, 8,  8,  NULL, 'ACTIVE', 'RM-101', NOW(), NOW());

-- Section 2 (7-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(9,  1, 2, 1,  9,  NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(10, 1, 2, 2,  1,  NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(11, 1, 2, 3,  11, NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(12, 1, 2, 4,  12, NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(13, 1, 2, 5,  13, NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(14, 1, 2, 6,  14, NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(15, 1, 2, 7,  15, NULL, 'ACTIVE', 'RM-102', NOW(), NOW()),
(16, 1, 2, 8,  16, NULL, 'ACTIVE', 'RM-102', NOW(), NOW());

-- Section 3 (8-A): subjects 9-16
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(17, 1, 3, 9,  17, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(18, 1, 3, 10, 18, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(19, 1, 3, 11, 19, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(20, 1, 3, 12, 20, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(21, 1, 3, 13, 21, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(22, 1, 3, 14, 22, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(23, 1, 3, 15, 23, NULL, 'ACTIVE', 'RM-201', NOW(), NOW()),
(24, 1, 3, 16, 24, NULL, 'ACTIVE', 'RM-201', NOW(), NOW());

-- Section 4 (8-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(25, 1, 4, 9,  25, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(26, 1, 4, 10, 26, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(27, 1, 4, 11, 27, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(28, 1, 4, 12, 28, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(29, 1, 4, 13, 29, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(30, 1, 4, 14, 30, NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(31, 1, 4, 15, 7,  NULL, 'ACTIVE', 'RM-202', NOW(), NOW()),
(32, 1, 4, 16, 8,  NULL, 'ACTIVE', 'RM-202', NOW(), NOW());

-- Section 5 (9-A): subjects 17-24
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(33, 1, 5, 17, 1,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(34, 1, 5, 18, 1,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(35, 1, 5, 19, 3,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(36, 1, 5, 20, 4,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(37, 1, 5, 21, 5,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(38, 1, 5, 22, 6,  NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(39, 1, 5, 23, 23, NULL, 'ACTIVE', 'RM-301', NOW(), NOW()),
(40, 1, 5, 24, 24, NULL, 'ACTIVE', 'RM-301', NOW(), NOW());

-- Section 6 (9-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(41, 1, 6, 17, 9,  NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(42, 1, 6, 18, 1,  NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(43, 1, 6, 19, 11, NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(44, 1, 6, 20, 12, NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(45, 1, 6, 21, 13, NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(46, 1, 6, 22, 14, NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(47, 1, 6, 23, 15, NULL, 'ACTIVE', 'RM-302', NOW(), NOW()),
(48, 1, 6, 24, 16, NULL, 'ACTIVE', 'RM-302', NOW(), NOW());

-- Section 7 (10-A): subjects 25-32
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(49, 1, 7, 25, 17, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(50, 1, 7, 26, 1,  NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(51, 1, 7, 27, 19, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(52, 1, 7, 28, 20, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(53, 1, 7, 29, 21, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(54, 1, 7, 30, 22, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(55, 1, 7, 31, 29, NULL, 'ACTIVE', 'RM-203', NOW(), NOW()),
(56, 1, 7, 32, 8,  NULL, 'ACTIVE', 'RM-203', NOW(), NOW());

-- Section 8 (10-B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(57, 1, 8, 25, 25, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(58, 1, 8, 26, 26, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(59, 1, 8, 27, 27, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(60, 1, 8, 28, 28, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(61, 1, 8, 29, 29, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(62, 1, 8, 30, 30, NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(63, 1, 8, 31, 7,  NULL, 'ACTIVE', 'RM-204', NOW(), NOW()),
(64, 1, 8, 32, 24, NULL, 'ACTIVE', 'RM-204', NOW(), NOW());

-- Section 9 (11-STEM A): select subjects
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(65, 1, 9, 1,  1,  NULL, 'ACTIVE', 'RM-303', NOW(), NOW()),
(66, 1, 9, 2,  1,  NULL, 'ACTIVE', 'RM-303', NOW(), NOW()),
(67, 1, 9, 3,  3,  NULL, 'ACTIVE', 'RM-303', NOW(), NOW()),
(68, 1, 9, 9,  9,  NULL, 'ACTIVE', 'RM-303', NOW(), NOW()),
(69, 1, 9, 10, 10, NULL, 'ACTIVE', 'RM-303', NOW(), NOW()),
(70, 1, 9, 11, 11, NULL, 'ACTIVE', 'RM-303', NOW(), NOW());

-- Section 10 (11-ABM B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(71, 1, 10, 1,  9,  NULL, 'ACTIVE', 'RM-304', NOW(), NOW()),
(72, 1, 10, 2,  1,  NULL, 'ACTIVE', 'RM-304', NOW(), NOW()),
(73, 1, 10, 9,  17, NULL, 'ACTIVE', 'RM-304', NOW(), NOW()),
(74, 1, 10, 10, 18, NULL, 'ACTIVE', 'RM-304', NOW(), NOW()),
(75, 1, 10, 25, 25, NULL, 'ACTIVE', 'RM-304', NOW(), NOW()),
(76, 1, 10, 26, 26, NULL, 'ACTIVE', 'RM-304', NOW(), NOW());

-- Section 11 (12-STEM A)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(77, 1, 11, 17, 1,  NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(78, 1, 11, 18, 1,  NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(79, 1, 11, 19, 3,  NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(80, 1, 11, 25, 17, NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(81, 1, 11, 26, 18, NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW()),
(82, 1, 11, 27, 19, NULL, 'ACTIVE', 'SCI-LAB-A', NOW(), NOW());

-- Section 12 (12-ABM B)
INSERT INTO class_offerings (Id, school_year_id, section_id, subject_id, teacher_id, curriculum_id, status, room, created_at, updated_at) VALUES
(83, 1, 12, 17, 9,  NULL, 'ACTIVE', 'RM-305', NOW(), NOW()),
(84, 1, 12, 18, 10, NULL, 'ACTIVE', 'RM-305', NOW(), NOW()),
(85, 1, 12, 25, 25, NULL, 'ACTIVE', 'RM-305', NOW(), NOW()),
(86, 1, 12, 26, 26, NULL, 'ACTIVE', 'RM-305', NOW(), NOW()),
(87, 1, 12, 9,  17, NULL, 'ACTIVE', 'RM-305', NOW(), NOW()),
(88, 1, 12, 10, 18, NULL, 'ACTIVE', 'RM-305', NOW(), NOW());
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

-- 12. CLASS_STUDENTS - link students to their section's class offerings
INSERT INTO class_students (class_offering_id, student_id, enrollment_id, status, created_at, updated_at)
SELECT co.Id, e.student_id, e.Id, 'ACTIVE', NOW(), NOW()
FROM class_offerings co
INNER JOIN enrollments e ON e.section_id = co.section_id AND e.school_year_id = co.school_year_id;
