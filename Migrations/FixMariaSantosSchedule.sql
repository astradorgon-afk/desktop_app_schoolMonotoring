-- ============================================================
-- Fix Maria Santos (teacher_id=1) schedule conflicts
-- 
-- PROBLEM: Maria had 12 offerings all squeezed into Mon+Tue
-- at 07:30-09:30 and 12:30-13:30, causing massive overlaps.
--
-- FIX: Spread offerings across Mon-Fri with unique time slots.
-- Pattern: each offering meets 2 days/week.
--   Mon+Tue: Off 1(07:30), Off 2(08:30), Off 10(09:30),
--            Off 33(10:30), Off 50(12:30), Off 77(13:30)
--   Wed+Thu: Off 42(07:30), Off 72(08:30), Off 65(10:30),
--            Off 34(12:30), Off 66(13:30)
--   Thu+FRI: Off 78(09:30, 07:30)
-- ============================================================

-- Step 1: Delete existing schedules for Maria's offerings
DELETE FROM class_schedules
WHERE class_offering_id IN (1, 2, 10, 33, 34, 42, 50, 65, 66, 72, 77, 78);

-- Step 2: Insert clean conflict-free schedule

-- Off 1 (Sec 1 "7-A", Eng 7, RM-101): Mon+Tue 07:30-08:30 (unchanged)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(1,  1, 1, '07:30:00', '08:30:00', 1, NULL, NOW(), NOW()),
(9,  1, 2, '07:30:00', '08:30:00', 1, NULL, NOW(), NOW());

-- Off 2 (Sec 1 "7-A", Math 7, RM-101): Mon+Tue 08:30-09:30 (unchanged)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(2,  2, 1, '08:30:00', '09:30:00', 1, NULL, NOW(), NOW()),
(10, 2, 2, '08:30:00', '09:30:00', 1, NULL, NOW(), NOW());

-- Off 10 (Sec 2 "7-B", Math 7, RM-102): Mon+Tue 09:30-10:30 (was Mon+Tue 12:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(16, 10, 1, '09:30:00', '10:30:00', 2, NULL, NOW(), NOW()),
(24, 10, 2, '09:30:00', '10:30:00', 2, NULL, NOW(), NOW());

-- Off 33 (Sec 5 "9-A", Eng 9, RM-301): Mon+Tue 10:30-11:30 (was Mon+Tue 07:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(57, 33, 1, '10:30:00', '11:30:00', 11, NULL, NOW(), NOW()),
(65, 33, 2, '10:30:00', '11:30:00', 11, NULL, NOW(), NOW());

-- Off 34 (Sec 5 "9-A", Math 9, RM-301): Wed+Thu 12:30-13:30 (was Mon+Tue 08:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(58, 34, 3, '12:30:00', '13:30:00', 11, NULL, NOW(), NOW()),
(66, 34, 4, '12:30:00', '13:30:00', 11, NULL, NOW(), NOW());

-- Off 42 (Sec 6 "9-B", Math 9, RM-302): Wed+Thu 07:30-08:30 (was Mon+Tue 12:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(72, 42, 3, '07:30:00', '08:30:00', 12, NULL, NOW(), NOW()),
(80, 42, 4, '07:30:00', '08:30:00', 12, NULL, NOW(), NOW());

-- Off 50 (Sec 7 "10-A", Math 10, RM-203): Mon+Tue 12:30-13:30 (was Mon+Tue 08:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(86, 50, 1, '12:30:00', '13:30:00', 8, NULL, NOW(), NOW()),
(94, 50, 2, '12:30:00', '13:30:00', 8, NULL, NOW(), NOW());

-- Off 65 (Sec 9 "11-STEM A", English, RM-303): Wed+Thu 10:30-11:30 (was Mon+Tue 07:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(113, 65, 3, '10:30:00', '11:30:00', 13, NULL, NOW(), NOW()),
(119, 65, 4, '10:30:00', '11:30:00', 13, NULL, NOW(), NOW());

-- Off 66 (Sec 9 "11-STEM A", Math, RM-303): Wed+Thu 13:30-14:30 (was Mon+Tue 08:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(114, 66, 3, '13:30:00', '14:30:00', 13, NULL, NOW(), NOW()),
(120, 66, 4, '13:30:00', '14:30:00', 13, NULL, NOW(), NOW());

-- Off 72 (Sec 10 "11-ABM B", Math, RM-304): Wed+Thu 08:30-09:30 (was Mon 12:30 only)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(124, 72, 3, '08:30:00', '09:30:00', 14, NULL, NOW(), NOW()),
(153, 72, 4, '08:30:00', '09:30:00', 14, NULL, NOW(), NOW());

-- Off 77 (Sec 11 "12-STEM A", English, SCI-LAB-A): Mon+Tue 13:30-14:30 (was Mon+Tue 07:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(133, 77, 1, '13:30:00', '14:30:00', 16, NULL, NOW(), NOW()),
(139, 77, 2, '13:30:00', '14:30:00', 16, NULL, NOW(), NOW());

-- Off 78 (Sec 11 "12-STEM A", Math, SCI-LAB-A): Thu 09:30 + Fri 07:30 (was Mon+Tue 08:30)
INSERT INTO class_schedules (Id, class_offering_id, day_of_week, start_time, end_time, room_id, time_slot_id, created_at, updated_at) VALUES
(134, 78, 4, '09:30:00', '10:30:00', 16, NULL, NOW(), NOW()),
(140, 78, 5, '07:30:00', '08:30:00', 16, NULL, NOW(), NOW());
