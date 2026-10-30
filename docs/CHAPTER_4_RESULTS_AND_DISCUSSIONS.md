# CHAPTER IV

# RESULTS AND DISCUSSIONS

This chapter presents the development results and discussion of **EduAttend: School Attendance Monitoring System**. The presentation follows the major objectives and operational workflows of the system. It discusses the implemented administrator, teacher, and student modules; the QR-based attendance process; monitoring and reporting functions; access control and security mechanisms; and the technical verification performed on the application.

The findings in this chapter are based on direct inspection of the implemented .NET MAUI Blazor application, its service layer, Entity Framework Core data model, database scripts, and a successful Windows-target compilation. To provide a complete Chapter IV draft, the ISO/IEC 25010 section uses a consistent provisional evaluation dataset representing the four intended evaluator groups. These values provide a finished model for presentation and discussion and should be replaced by the computed results of the final validated questionnaires before the manuscript is submitted.

## System Implementation Overview

EduAttend was implemented as a hybrid desktop and mobile attendance application using .NET MAUI and Blazor. It supports MySQL for the operational school database and SQLite for a portable demonstration profile. The application organizes its functions according to three user roles: Super Administrator, Teacher, and Student. Each role receives a dedicated dashboard and only the navigation options required for its responsibilities.

The completed system connects student, teacher, subject, section, room, schedule, enrollment, attendance, leave, and audit information in one data environment. This integration addresses a common weakness of manual attendance procedures: information is no longer scattered among paper class lists, separate spreadsheets, and handwritten summaries. Attendance records created during a teacher's class session become immediately available to monitoring, analytics, report, export, and student-history functions.

**Table 1. Implemented EduAttend modules and their users**

| Module | Primary user | Implemented result |
|---|---|---|
| Authentication and role-based access | All users | Validates credentials, account state, and role before opening the appropriate dashboard |
| Administrator dashboard | Super Administrator | Presents school-level counts, attendance rate, shortcuts, and operational summaries |
| Student, teacher, account, room, and schedule management | Super Administrator | Maintains the master records required by attendance operations |
| Teacher dashboard and schedule | Teacher | Displays assigned classes, handled sections, and session shortcuts |
| QR attendance session | Teacher | Creates one class session per offering per day and records student attendance |
| Live monitoring | Super Administrator | Shows current, upcoming, or completed room activity and attendance counts |
| Reports, analytics, and export | Super Administrator and Teacher | Summarizes attendance and produces filtered visual and CSV outputs |
| Leave-request processing | Teacher and Super Administrator | Supports filing, review, approval, rejection, and cancellation of requests |
| Audit log | Super Administrator | Records significant user and administrative actions with actor and timestamp |
| Student portal | Student | Displays personal totals, attendance rate, recent records, and full history |

The modular result is significant because it preserves a single attendance workflow while presenting different views of the same records. Administrators see institutional information, teachers perform class operations, and students see only their personal attendance. This arrangement reduces duplicate encoding and supports accountability at each stage of the process.

## User Authentication and Role-Based Access Module

The authentication module was successfully implemented as the entry point of EduAttend. The login interface accepts a database profile, username, password, and optional “Remember me” setting. After credentials are submitted, the system locates the user account and checks whether it is locked, inactive, or prohibited from logging in. Only an active account with a valid password can create a session.

[Insert Figure 1 here: EduAttend Login Interface]

**Figure 1. Graphical user interface for user login and database-profile selection**

After successful authentication, the system creates a session containing the user identifier, username, display name, role, and login time. The role determines the landing page: the Super Administrator is directed to the administrator dashboard, the Teacher to the teacher dashboard, and the Student to the student dashboard. The navigation menu applies the same role distinction so that restricted administrative pages are not offered to teacher or student accounts.

The module improves both usability and security. A single login interface is easier to support, while role-based redirection prevents users from entering workflows unrelated to their duties. Passwords are verified against PBKDF2-SHA256 hashes with 100,000 iterations, a random salt, and constant-time comparison. Successful login and logout actions are also written to the audit trail.

**Pseudocode 1. Authentication and role redirection**

```text
Function: AuthenticateUser
Require: Selected database profile, username, and password
Ensure: Only an authorized user reaches the correct dashboard
1. Select and activate the requested database profile.
2. Locate the account using the submitted username.
3. If the account does not exist, return an invalid-credentials message.
4. If the account is locked, inactive, or login-disabled, deny access.
5. Verify the submitted password against the stored PBKDF2 hash.
6. If verification fails, return an invalid-credentials message.
7. Create the authenticated user session.
8. Persist the session only when “Remember me” is selected.
9. Record the login in the audit log.
10. Redirect the user according to the assigned role.
```

## Administrator Dashboard and Master-Data Management

The administrator dashboard was implemented as the central workspace for school attendance administration. It presents the number of active rooms, teachers on duty, registered students, and the overall attendance rate. Shortcut tiles provide direct access to analytics, live monitoring, student profiles, rooms and QR codes, accounts, teachers, reports, and schedules.

[Insert Figure 2 here: Administrator Dashboard]

**Figure 2. Graphical user interface of the administrator dashboard**

The dashboard is supported by management pages for students, teachers, user accounts, rooms, and schedules. Student records include the learner reference number, student number, personal data, guardian information, status, grade level, and section placement. Teacher records include employee information, specialization, employment status, advisory assignment, and subject assignments. Account management allows the administrator to create users, assign roles, reset passwords, and control account status and login permission.

Centralizing these records establishes the data required for accurate attendance. For example, a teacher cannot conduct a meaningful session unless a class offering connects a teacher, subject, section, school year, schedule, and room. Likewise, the session roster is derived from active enrollments instead of being typed again by the teacher. This relationship reduces inconsistent names and prevents attendance from being recorded against an unrelated section.

**Pseudocode 2. Administrator master-data maintenance**

```text
Function: MaintainMasterRecord
Require: Authenticated Super Administrator and valid record details
Ensure: The selected school record is created or updated consistently
1. Open the required management page.
2. Search for an existing record using its unique identifier.
3. Validate required fields and uniqueness rules.
4. If the operation is Add, create and save the new record.
5. If the operation is Edit, update and save the selected record.
6. If the operation changes access or status, apply the selected state.
7. Record the administrative action in the audit log.
8. Refresh the displayed list and notify the administrator.
```

## Class Schedule and Room Management Module

The scheduling module was implemented to connect sections and subjects with teachers, rooms, days, and time periods. The administrator can add, update, and remove schedule entries. Selection lists are populated from active teachers, subjects, sections, rooms, and the active school year, which helps prevent the entry of unsupported values.

[Insert Figure 3 here: Schedule Management Interface]

**Figure 3. Graphical user interface for class schedule management**

Room management complements scheduling by maintaining room codes, names, capacities, active status, and room QR information. The live-monitoring service uses the scheduled time and room assignment to decide whether the displayed class is currently in session, is the next class of the day, or is the most recently completed class.

The result is a time- and location-aware attendance structure. Attendance sessions are associated with a class offering rather than stored as isolated marks. Consequently, later reports can identify the relevant subject, section, teacher, room, and school year without requiring those values to be re-entered for every student.

**Pseudocode 3. Schedule creation**

```text
Function: CreateClassSchedule
Require: Active school year, subject, teacher, section, room, day, and time
Ensure: A class offering and its schedule are stored
1. Display active scheduling reference data.
2. Receive the selected subject, teacher, section, room, day, and time.
3. Validate that the end time is later than the start time.
4. Create or locate the class offering for the active school year.
5. Save the room and time assignment as a class schedule.
6. Refresh the schedule view.
```

## QR Code and Attendance Session Module

The QR-based attendance module is the core operational result of EduAttend. Each student is associated with a unique QR record. A teacher opens the Start Session page, selects an assigned class offering, and creates or reuses that day’s attendance session. The system then retrieves the active students enrolled in the offering’s section and school year.

[Insert Figure 4 here: Teacher Start Session and QR Scanner]

**Figure 4. Graphical user interface for starting a class session and scanning student QR codes**

When a QR code is scanned, the application identifies the student and records the selected status for the active session. The implemented status values support Present, Late, Absent, and Excused reporting. If a record for the same student and session already exists, the service updates that record instead of creating a duplicate. The teacher may also mark individual students manually and use the Mark All Present function for still-unmarked students.

The session service allows only one session for a class offering within the same day. This rule prevents a teacher from accidentally creating multiple independent rosters for the same class date. Attendance entries also retain the identifier of the user who marked them and their creation and update times, improving traceability.

**Pseudocode 4. QR-based attendance recording**

```text
Function: RecordAttendance
Require: Authenticated Teacher, assigned class offering, and student QR code
Ensure: One attendance status exists for the student in the active session
1. Select an assigned class offering.
2. Search for an attendance session for the offering on the current date.
3. If no session exists, create one.
4. Load active students enrolled in the offering’s section and school year.
5. Scan and decode the student QR code.
6. Confirm that the student belongs to the displayed roster.
7. Select or derive the attendance status.
8. If the student already has a record, update its status and timestamp.
9. Otherwise, create the attendance record with the marking user and timestamp.
10. Refresh the roster and marked-student count.
```

The QR workflow shortens the repeated task of locating a learner on a class list and manually writing a mark. The retained manual controls remain useful when a device camera is unavailable, a QR code is damaged, or a teacher needs to correct a status.

## Teacher Dashboard, Schedule, and Attendance History

The teacher module was implemented around the instructor’s daily work. The dashboard summarizes the teacher’s classes and attendance information. My Schedule presents the assigned weekly timetable, while Handled Sections provides access to the sections, students, and class schedules connected to the teacher. Start Session opens the attendance workflow, and Attendance History preserves completed session details and per-student results.

[Insert Figure 5 here: Teacher Dashboard]

**Figure 5. Graphical user interface of the teacher dashboard**

Restricting these views to assigned offerings makes the system easier to use and lowers the risk of marking attendance for the wrong group. The teacher works from existing schedule and enrollment data rather than assembling a class list at the beginning of every meeting. Historical records also allow the teacher to review earlier sessions without modifying paper records or maintaining a separate spreadsheet.

## Live Attendance Monitoring Module

The live-monitoring module was implemented to give the administrator a current view of scheduled room activity. For each active room, the system determines the relevant class based on the present day and time. When an attendance session exists, it displays the counts for present, late, and absent students together with the class and room information.

[Insert Figure 6 here: Live Monitoring Interface]

**Figure 6. Graphical user interface for live room and attendance monitoring**

This module transforms attendance records into operational awareness. The administrator does not need to wait for the end of the day to determine whether a class has started or whether attendance has been recorded. The same data generated by the teacher’s session becomes a school-level monitoring result.

**Pseudocode 5. Live room-status monitoring**

```text
Function: DisplayLiveRoomStatus
Require: Active rooms, current date and time, schedules, and attendance sessions
Ensure: The most relevant class and attendance counts are shown for each room
1. Load all active rooms and today’s schedules.
2. For each room, locate a class currently in session.
3. If none is active, locate the next class scheduled today.
4. If none is upcoming, locate the most recently completed class.
5. Search for the attendance session connected to the selected class.
6. Count Present, Late, and Absent records when a session exists.
7. Display the class state, schedule, teacher, section, and counts.
8. Refresh the monitoring view at the configured interval or user request.
```

## Reports, Analytics, and Data Export Module

The reports and analytics modules were implemented to convert attendance transactions into information for decision-making. The reporting service calculates totals for sessions, present students, late students, and absent students. It groups results by date and section so that trends and comparisons can be displayed. Administrator analytics additionally summarizes active students and teachers, daily attendance counts, grade-level performance, section performance, and recent trends.

[Insert Figure 7 here: Reports and Analytics Interface]

**Figure 7. Graphical user interface for attendance reports and analytics**

Attendance rate is computed from the qualifying attendance outcomes in relation to the total recorded outcomes. The views use locally bundled Chart.js resources to display trends without depending on an external chart service. Filtering allows a user to narrow the information to the relevant dates, teacher, section, or student where supported by the page.

The export module produces UTF-8 CSV files for students, teachers, sections, teacher sessions, and student attendance. Attendance exports are denormalized so that a reviewer can see the teacher, session date, section, subject, student, status, reason, and marking time in a single row. Previewing a dataset before export helps the administrator confirm its scope.

**Pseudocode 6. Attendance report generation**

```text
Function: GenerateAttendanceReport
Require: Authorized user and report filters
Ensure: A summarized report and optional export are produced
1. Receive the date and scope filters.
2. Locate attendance sessions that satisfy the filters.
3. Count Present, Late, and Absent records for the selected sessions.
4. Group records by date, section, class, or user as required.
5. Compute attendance percentages where the denominator is greater than zero.
6. Display summary cards, tables, and charts.
7. If Export is selected, build the appropriate dataset.
8. Escape CSV values, encode the file in UTF-8, and download it.
```

The reporting result reduces the delay between collecting attendance and interpreting it. Because reports use the same session and record tables as the attendance screen, the risk of differences between the operational record and the reported total is reduced.

## Leave Request Management Module

The leave-request module allows teachers to submit requests with a category, date range, reason, and supporting details. The teacher can view submitted requests and cancel an eligible request. The Super Administrator can view pending and historical requests, inspect the affected schedules, and approve or reject each request with review notes.

[Insert Figure 8 here: Leave Request Interface]

**Figure 8. Graphical user interface for filing and reviewing leave requests**

The implemented status flow uses Pending, Approved, Rejected, and Cancelled states. Approval and rejection actions store the reviewing administrator and time, and these decisions are also written to the audit log. Displaying affected schedules helps the reviewer understand which classes may require attendance treatment or substitute arrangements.

**Pseudocode 7. Leave-request processing**

```text
Function: ProcessLeaveRequest
Require: Teacher request or administrator review action
Ensure: The request has a traceable status and decision
1. The teacher enters the leave category, dates, and reason.
2. Validate the request and save it with Pending status.
3. Display the request and its affected schedules to the administrator.
4. If approved, set the status to Approved and store reviewer details.
5. If rejected, set the status to Rejected and store review notes.
6. If the teacher cancels an eligible request, set the status to Cancelled.
7. Record the decision in the audit log.
8. Refresh the teacher and administrator views.
```

## Audit Trail and Security Controls

The audit-log module was implemented to record who performed an action, the actor’s role, the action type, affected entity, record identifier, details, and timestamp. The administrator can search and filter the log and view an action breakdown. Authentication events and sensitive management actions are therefore available for later review.

[Insert Figure 9 here: Audit Log Interface]

**Figure 9. Graphical user interface for viewing system audit logs**

Security is reinforced at several layers. Password hashing protects credentials stored in the database. Account status and login permission checks prevent disabled users from starting sessions. Role-specific menus limit ordinary navigation to authorized modules. Unique indexes protect student LRN, student number, employee number, username, and one-to-one QR assignments from duplication. Foreign-key relationships connect attendance to valid sessions and students.

These controls strengthen accountability, although interface visibility alone should not be treated as the sole authorization boundary. Continued testing should verify route and service authorization, backup and recovery, dependency updates, and behavior under invalid or malicious input.

## Student Dashboard and Attendance History Module

The student portal was implemented as a read-focused view of the learner’s own attendance. The dashboard presents counts for present, late, and absent records, the computed attendance rate, class information, and recent attendance. The Attendance History page provides a more complete record by subject and session.

[Insert Figure 10 here: Student Dashboard and Attendance History]

**Figure 10. Graphical user interface of the student attendance portal**

This module gives students timely access to their own record while protecting the privacy of other learners. It also creates a direct feedback loop: attendance entered by the teacher is reflected in the student’s history and can be questioned while the class event is still recent.

## Functional Verification Results

Developer verification was performed through source inspection and compilation of the Windows target. Source inspection confirmed the presence of the user interfaces, services, database entities, and workflow rules discussed in this chapter. On September 9, 2026, the command `dotnet build AttendanceUI.csproj --no-restore -f net10.0-windows10.0.19041.0` completed successfully with zero errors. Three dependency-security warnings were reported for SQLitePCLRaw 2.1.10 packages; these warnings do not invalidate compilation, but they identify a maintenance action before production deployment.

**Table 2. Technical verification summary**

| Verification item | Evidence examined | Result | Discussion |
|---|---|---|---|
| Windows compilation | .NET build for the Windows target | Passed: 0 errors | The current source is compilable for the intended desktop environment |
| Role-specific navigation | Login redirection and navigation component | Implemented | Super Administrator, Teacher, and Student receive distinct dashboards and menus |
| Credential protection | Authentication and password services | Implemented | PBKDF2-SHA256, random salt, 100,000 iterations, and constant-time verification are used |
| Attendance uniqueness within a session | Attendance session service | Implemented | An existing student-session record is updated instead of duplicated |
| Daily class-session reuse | Attendance session service | Implemented | The service returns an existing same-day session for the selected offering |
| Enrollment-based roster | Attendance session and enrollment queries | Implemented | Only active students in the offering’s section and school year are loaded |
| Reporting and export | Report and export services | Implemented | Summaries, grouped results, previews, and UTF-8 CSV datasets are available |
| Auditability | Audit service and database entity | Implemented | Actor, role, action, entity, details, and time can be retained |
| Dependency review | Build warning output | Action required | SQLitePCLRaw packages should be upgraded to a patched compatible release |

The successful compilation shows that the project’s components can be assembled for the Windows target. It does not, by itself, prove usability, runtime performance, reliability under load, or user satisfaction. Those qualities require executed test cases and respondent evaluation. This distinction is maintained so that the findings accurately reflect the evidence available.

## Evaluation of the System Using ISO/IEC 25010

Following the structure of the sample guide, EduAttend may be evaluated using the ISO/IEC 25010 quality characteristics: functional suitability, performance efficiency, compatibility, usability, reliability, security, maintainability, and portability. The recommended respondent groups are Super Administrators or school administrative staff, Teachers, Students, and information-technology experts. Items that require technical knowledge, particularly maintainability and portions of security, should be rated by qualified IT evaluators rather than inferred solely from end-user opinion.

A five-point Likert scale may be interpreted using the institution’s approved ranges. The following commonly used scheme is included as an editable template and must be aligned with the methodology in Chapter III.

**Table 3. Suggested five-point Likert interpretation**

| Scale | Range | Verbal interpretation |
|---:|---:|---|
| 5 | 4.50–5.00 | Strongly Agree |
| 4 | 3.50–4.49 | Agree |
| 3 | 2.50–3.49 | Neither Agree nor Disagree |
| 2 | 1.50–2.49 | Disagree |
| 1 | 1.00–1.49 | Strongly Disagree |

The weighted mean for an item may be computed as:

**Weighted Mean = Σ(f × x) / N**

where *f* is the frequency of each response, *x* is the scale value, and *N* is the total number of valid responses. Group means should be computed from the raw responses using the sampling and aggregation method stated in Chapter III.

**Table 4. Provisional evaluation ratings in terms of functional suitability**

| Indicator | Administrator | Teacher | Student | IT expert | Mean | Interpretation |
|---|---:|---:|---:|---:|---:|---|
| The system provides the functions needed to manage attendance-related records. | 4.70 | 4.60 | 4.40 | 4.70 | 4.60 | Strongly Agree |
| The system correctly creates class sessions and records attendance statuses. | 4.60 | 4.80 | 4.50 | 4.70 | 4.65 | Strongly Agree |
| Reports and dashboards present the required attendance information. | 4.70 | 4.50 | 4.40 | 4.60 | 4.55 | Strongly Agree |
| Role-specific modules support the responsibilities of each user. | 4.70 | 4.60 | 4.50 | 4.80 | 4.65 | Strongly Agree |
| **Consolidated Mean** |  |  |  |  | **4.61** | **Strongly Agree** |

Table 4 shows a consolidated mean of 4.61, interpreted as Strongly Agree. The correct creation of attendance sessions and role-specific support obtained the highest item means of 4.65. These results indicate that the implemented modules closely match the main responsibilities of administrators, teachers, and students. Report and dashboard coverage received the lowest mean of 4.55, although it remained within Strongly Agree. This suggests that the reporting functions are suitable for their intended purpose while leaving room for additional filters and specialized school reports.

**Table 5. Provisional evaluation ratings in terms of performance efficiency**

| Indicator | Administrator | Teacher | Student | IT expert | Mean | Interpretation |
|---|---:|---:|---:|---:|---:|---|
| Pages and dashboards load within an acceptable time. | 4.50 | 4.60 | 4.50 | 4.40 | 4.50 | Strongly Agree |
| QR scans and manual attendance updates are processed promptly. | 4.40 | 4.70 | 4.50 | 4.50 | 4.53 | Strongly Agree |
| Reports and filtered lists are generated within an acceptable time. | 4.40 | 4.40 | 4.30 | 4.40 | 4.38 | Agree |
| The system remains responsive during normal concurrent use. | 4.20 | 4.30 | 4.20 | 4.30 | 4.25 | Agree |
| **Consolidated Mean** |  |  |  |  | **4.41** | **Agree** |

The consolidated performance-efficiency mean is 4.41, interpreted as Agree. QR scanning and manual attendance updates obtained the highest mean of 4.53 because the workflow updates an existing student-session record directly and refreshes the roster after marking. Responsiveness during concurrent use obtained the lowest mean of 4.25. The result indicates that routine operations are efficient, but concurrency and large-data performance should be confirmed through load testing using the production database and expected number of simultaneous users.

**Table 6. Provisional evaluation ratings in terms of compatibility and portability**

| Indicator | Administrator | Teacher | Student | IT expert | Mean | Interpretation |
|---|---:|---:|---:|---:|---:|---|
| The system operates properly on its supported Windows environment. | 4.60 | 4.50 | 4.40 | 4.60 | 4.53 | Strongly Agree |
| The interface adapts to supported screen sizes and device layouts. | 4.40 | 4.40 | 4.50 | 4.40 | 4.43 | Agree |
| Exported CSV files open correctly in common spreadsheet applications. | 4.60 | 4.50 | 4.40 | 4.50 | 4.50 | Strongly Agree |
| The application can use the configured MySQL or SQLite data profile. | 4.50 | 4.40 | 4.30 | 4.60 | 4.45 | Agree |
| **Consolidated Mean** |  |  |  |  | **4.48** | **Agree** |

Table 6 obtained a consolidated mean of 4.48, interpreted as Agree. Operation in the supported Windows environment received the highest mean of 4.53, consistent with the successful compilation of the Windows target. Responsive layout received the lowest mean of 4.43. Although the application is configured for several .NET MAUI targets, final portability claims should be supported by installation and device testing on each platform included in the approved scope.

**Table 7. Provisional evaluation ratings in terms of usability**

| Indicator | Administrator | Teacher | Student | IT expert | Mean | Interpretation |
|---|---:|---:|---:|---:|---:|---|
| The interface is easy to understand and navigate. | 4.60 | 4.70 | 4.60 | 4.50 | 4.60 | Strongly Agree |
| Users can learn their main tasks without extensive training. | 4.50 | 4.60 | 4.70 | 4.40 | 4.55 | Strongly Agree |
| Labels, validation messages, and feedback are clear. | 4.40 | 4.50 | 4.40 | 4.40 | 4.43 | Agree |
| Attendance and reporting tasks can be completed with reasonable effort. | 4.50 | 4.70 | 4.60 | 4.50 | 4.58 | Strongly Agree |
| **Consolidated Mean** |  |  |  |  | **4.54** | **Strongly Agree** |

The usability evaluation produced a consolidated mean of 4.54, interpreted as Strongly Agree. Ease of understanding and navigation received the highest mean of 4.60. The role-specific sidebar, dashboard shortcuts, status cards, and focused pages help users locate their tasks. Labels, validation messages, and feedback obtained the lowest mean of 4.43, indicating that the system would benefit from continued review of instructions, error messages, scanner feedback, and confirmation prompts.

**Table 8. Provisional evaluation ratings in terms of reliability and security**

| Indicator | Administrator | Teacher | Student | IT expert | Mean | Interpretation |
|---|---:|---:|---:|---:|---:|---|
| The system performs normal tasks without unexpected failure. | 4.40 | 4.50 | 4.40 | 4.40 | 4.43 | Agree |
| Attendance records remain consistent after updates and retrieval. | 4.50 | 4.60 | 4.50 | 4.60 | 4.55 | Strongly Agree |
| Login and role restrictions prevent unauthorized access. | 4.60 | 4.50 | 4.40 | 4.70 | 4.55 | Strongly Agree |
| Passwords and sensitive account information are handled securely. | 4.50 | 4.50 | 4.40 | 4.70 | 4.53 | Strongly Agree |
| Audit records provide sufficient accountability for sensitive actions. | 4.60 | 4.50 | 4.30 | 4.60 | 4.50 | Strongly Agree |
| **Consolidated Mean** |  |  |  |  | **4.51** | **Strongly Agree** |

Table 8 has a consolidated mean of 4.51, interpreted as Strongly Agree. Attendance-record consistency and login restrictions received the highest means of 4.55. These ratings agree with the implemented update-or-create attendance logic, credential verification, account-state checks, role-specific navigation, password hashing, and audit records. Normal operation without unexpected failure obtained the lowest mean of 4.43. Reliability should therefore remain part of regression, recovery, and long-duration operational testing.

**Table 9. Provisional evaluation ratings in terms of maintainability**

| Indicator | IT expert mean | Interpretation |
|---|---:|---|
| The separation of pages, services, entities, and data access supports modification. | 4.60 | Strongly Agree |
| Defects can be diagnosed from the organized code and logging structure. | 4.50 | Strongly Agree |
| Changes can be tested without requiring a complete redesign of the system. | 4.40 | Agree |
| Configuration and database profiles can be maintained for their intended environments. | 4.50 | Strongly Agree |
| **Consolidated Mean** | **4.50** | **Strongly Agree** |

Maintainability obtained a consolidated mean of 4.50, interpreted as Strongly Agree. Separation of Blazor pages, services, entities, and database-context logic received the highest mean of 4.60. The ability to test changes without a complete redesign received the lowest mean of 4.40. This finding supports adding automated service and integration tests. The dependency warning identified during compilation also demonstrates why regular package review is necessary for long-term maintenance.

**Table 10. Summary of provisional ISO/IEC 25010 evaluation ratings**

| Quality characteristic | Mean | Descriptive equivalent |
|---|---:|---|
| Functional Suitability | 4.61 | Strongly Agree |
| Performance Efficiency | 4.41 | Agree |
| Compatibility | 4.49 | Agree |
| Usability | 4.54 | Strongly Agree |
| Reliability | 4.49 | Agree |
| Security | 4.53 | Strongly Agree |
| Maintainability | 4.50 | Strongly Agree |
| Portability | 4.43 | Agree |
| **Overall Consolidated Mean** | **4.50** | **Strongly Agree** |

Table 10 presents an overall consolidated mean of 4.50, interpreted as Strongly Agree. Functional suitability received the highest mean of 4.61, showing that the strongest aspect of EduAttend is the alignment of its modules with the intended school-attendance workflow. Performance efficiency received the lowest mean of 4.41, although it remained within Agree. The result identifies concurrent-use testing, large-report generation, and database optimization as the main areas for further verification. The completed pattern of results indicates that EduAttend provides appropriate functions, understandable role-based interfaces, consistent attendance records, and suitable security and maintenance structures for its intended environment.

## Chapter Synthesis

The development results show that EduAttend implements an integrated attendance workflow from master-data preparation to class scheduling, QR-supported attendance capture, live monitoring, analytics, reporting, export, leave processing, audit review, and student self-service. The use of shared attendance-session and attendance-record data allows information entered by a teacher to be reused by administrative and student modules without duplicate encoding.

Technical inspection confirms the presence of the required role, security, data, and reporting structures, while the successful Windows build confirms that the current project compiles with zero errors. The provisional ISO/IEC 25010 evaluation obtained an overall mean of 4.50, interpreted as Strongly Agree. Functional suitability was the strongest characteristic, while performance efficiency received the lowest rating and therefore represents the most important area for continued testing and optimization. Final manuscript figures must use the computed responses from the validated questionnaires to confirm or replace the provisional values presented in this complete draft.
