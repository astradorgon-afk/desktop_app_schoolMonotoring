# TECHNICAL TESTING

**System:** EduAttend — .NET MAUI Blazor Hybrid QR attendance application
**Date tested:** 8 October 2026 (Asia/Singapore, host clock UTC+08:00)
**Reference:** Lecture-on-Technical-Testing (1).pdf — methods and example table columns follow the lecture
**Evidence:** raw runner output in TEST_RESULTS.txt; reproduction source in TechnicalTestingRunner.cs.txt, TechnicalTestingRunner.csproj.txt and TechnicalTestingMauiStubs.cs.txt; this file is rendered to PDF by make_pdf.py

**Result key:** Pass = observed result matched the expected result. Fail = observed result differed. Not tested = no execution evidence. In Section 10, Accepted / Not accepted / Not tested apply to the acceptance criteria only.

**Scope.** Every number in this report was produced by re-running the test harness against the current source on 8 October 2026. The harness compiles the production service, entity and resource files into an isolated console program and calls them directly. It does not exercise the graphical interface, the camera, a device or a server.

## Test Environment and Method

| Item | Detail |
|---|---|
| Host | Microsoft Windows 11 Pro 10.0.26200, Intel Core i7-8550U (4 cores), 16 GB RAM |
| Toolchain | .NET SDK 10.0.301, .NET 10, MAUI workload |
| Data store | SQLite through Microsoft.EntityFrameworkCore.Sqlite 9.0.0; in-memory database for functional cases, file database with WAL for load, stress and recovery |
| Code under test | Services/TeacherSessionService.cs, Services/ReportService.cs, Services/AuthService.cs, Services/UserService.cs, Data/SchoolDbContext.cs, Resources/*.resx — linked unmodified |
| Test doubles | No-op audit service; dictionary-backed Preferences implementation. Neither affects login, scanning, reporting or storage outcomes. |
| Out of scope | Graphical user interface, camera capture, device or emulator execution, live server, network recovery, production-scale data |

**Method.** For each lecture method the table keeps the lecture's own columns. Where the lecture's example table has no Result column, the actual observations and the pass/fail tally are given in an **Observed** line directly under the table, so no finding is lost. Cases are executed once against a controlled fixture: one teacher, one active session, one section, students 1 to 5 enrolled, class start 14:00 local on the test date.

## 1. Black-Box Testing

| Test Case | Input | Expected Output | Result |
|---|---|---|---|
| Login | Valid username and password | Authenticated session created | Pass |
| Login | Valid username, wrong password | Error message displayed | Pass |
| Login | Unknown username | Error message displayed | Pass |
| Login | Locked account | Account locked message | Pass |
| Login | Inactive account | Account inactive message | Pass |
| Login | Account with login disabled | Login disabled message | Pass |
| QR Scan | Valid QR, enrolled student, active session, 14 minutes after start | Attendance recorded as PRESENT | Pass |
| QR Scan | Unregistered token | QR rejected | Pass |
| QR Scan | Token expired one minute earlier | QR rejected | Pass |
| QR Scan | Revoked but otherwise unexpired token | QR rejected | Pass |
| QR Scan | Room QR passed to the student scanner | QR rejected as wrong type | Pass |
| QR Scan | Empty QR input | QR rejected | Pass |
| QR Scan | Valid QR for a student not enrolled in the section | QR rejected | Pass |
| Duplicate Scan | Same QR scanned again within two minutes | Duplicate prevented | Pass |
| Duplicate Scan | Same QR scanned three minutes after the first record | Duplicate prevented | **Fail** |
| Attendance | Active session, schedule starts one hour later, valid QR | Transaction rejected or flagged | **Fail** |
| Attendance | Valid QR, 16 minutes after start | Attendance recorded as LATE | Pass |
| Attendance | Valid QR after the session has ended | Transaction rejected | Pass |
| Report | Select the local date on which the session ran | Correct attendance report generated | **Fail** |
| Report | Select the UTC date of the stored session (control) | Correct attendance report generated | Pass |

**Observed:** 20 cases executed — 17 Pass, 3 Fail. Failures: a repeat scan three minutes after the first record created a second PRESENT row; a scan one hour before the scheduled start created a PRESENT row; a report filtered on the session's local date returned zero sessions and zero records. The two grace-period boundary cases (14 min 59 s and 15 min 01 s) are reported in Section 3, so 22 cases were executed in total across Sections 1 and 3.

## 2. Equivalence Partitioning

| Input Class | Example | Expected Result | Result |
|---|---|---|---|
| Valid QR | Unexpired token for an enrolled student in an active session | Accepted | Pass |
| Invalid QR | Unregistered token | Rejected | Pass |
| Expired QR | Token expired one minute earlier | Rejected | Pass |
| Revoked QR | Revoked, otherwise unexpired token | Rejected | Pass |
| Wrong QR type | Room token passed to the student scanner | Rejected | Pass |
| Duplicate QR | Previously scanned within two minutes | Rejected | Pass |
| Duplicate QR | Previously scanned more than two minutes earlier | Rejected | **Fail** |
| Empty input | Empty token | Rejected | Pass |
| Unenrolled student | Valid token for a student outside the section | Rejected | Pass |

**Observed:** 9 classes tested — 8 Pass, 1 Fail. The duplicate class behaves differently inside and outside the two-minute window, so it is really two partitions and the second one is unimplemented. The empty input is rejected with the generic "Invalid or expired QR code." message rather than a distinct "no QR detected" message; the outcome matches the expected result even though the wording differs from the lecture's example.

## 3. Boundary Value Analysis

The lecture's 7:00–8:00 example is illustrative. This system's rule is a 15-minute grace period after the scheduled start (`Services/TeacherSessionService.cs:841-844`, late when `now > startTime + 15 min`), and a two-minute duplicate window (`Services/TeacherSessionService.cs:396`).

| Test Time | Expected Result |
|---|---|
| 14 min 59 s after scheduled start | Allowed, inside the grace period |
| 15 min 00 s after scheduled start | Exact limit; under `now > graceEnd` the result is PRESENT |
| 15 min 01 s after scheduled start | LATE |
| 16 min after scheduled start | LATE |
| One hour before scheduled start | Rejected, session has not started |
| Repeat scan 0 to 2 minutes after the first record | Rejected as duplicate |
| Repeat scan 3 minutes after the first record | Rejected as duplicate |

**Observed:** 14 min 59 s produced PRESENT (Pass); 15 min 01 s produced LATE (Pass); 16 min produced LATE (Pass); a repeat two minutes after the first record was rejected (Pass). The exact 15 min 00 s instant was not hit (Not tested — the comparison runs on `DateTime.UtcNow` with sub-second precision). One hour before start produced a PRESENT row (**Fail**), and a repeat at three minutes produced a second PRESENT row (**Fail**). **Result: 4 Pass, 2 Fail, 1 Not tested.**

## 4. Decision Table Testing

| Valid QR | Registered | Within Schedule | Already Recorded | Expected Result |
|---|---|---|---|---|
| Yes | Yes | Yes | No | Attendance Accepted |
| Yes | Yes | Yes | Yes | Duplicate Rejected |
| Yes | Yes | No | No | Late/Invalid |
| Yes | No | Yes | No | Rejected |
| No | Yes | Yes | No | Rejected |

**Observed:**
Row 1 — a valid, enrolled scan inside the schedule was accepted and stored as PRESENT (Pass).
Row 2 — a repeat within two minutes was rejected (Pass); a repeat at three minutes was accepted and stored a second row (**Fail**).
Row 3 — a valid scan one hour before start was accepted as PRESENT (**Fail**); a valid scan after the session ended was rejected with "No active session." (Pass).
Row 4 — a valid token for a non-enrolled student was rejected (Pass).
Row 5 — unknown, expired and revoked tokens were each rejected (Pass).
**Result: 6 Pass, 2 Fail.**

## 5. Stress Testing

| Users | Transactions | Successful | Failed | Avg. Response |
|---:|---:|---:|---:|---:|
| 100 | 100 | 100 | 0 | 150.4 ms |
| 200 | 200 | 200 | 0 | 111.9 ms |
| 500 | 500 | 500 | 0 | 254.5 ms |

**Observed:** Workload exceeded the expected load of approximately 50 simultaneous scans at every level. Failed transactions: 0 at all three levels. Server errors: none (single-process harness). Database errors: none. System crashes: none. Attendance-record accuracy: 100 % — 100, 200 and 500 rows were persisted at the corresponding levels with no lost or extra rows. Peak response 1282 ms (100 users), 1580 ms (200 users) and 4598 ms (500 users); p95 806 ms, 638 ms and 1610 ms; wall time 2097 ms, 2966 ms and 9006 ms; throughput 47.7, 67.4 and 55.5 scans per second. **Result: 3 levels run, 0 failures.**

## 6. Load Testing

| Response Time | Throughput | Error Rate | Resource Utilization |
|---|---|---|---|
| avg 136.5 ms, p95 822.0 ms, max 1152.0 ms, wall 1154 ms for 50 concurrent scans | 43.3 scans per second | 0.00 % (0 of 50) | Not measured |

**Observed:** 50 concurrent scan requests were issued at once against the file-backed database. All 50 completed and all 50 rows were stored; the runner reported `errors=none`. The 1152 ms maximum is the first request, which pays one-time database and JIT start-up cost; the median request was far faster. As a sequential baseline, a single-user probe over 50 scans measured avg 11.0 ms, p50 8.1 ms, p95 23.8 ms and max 61.1 ms for a scan, and avg 9.3 ms, p95 17.3 ms and max 27.4 ms for a report. CPU, memory, handle count and disk utilization were not sampled. **Result: 50-user expected workload met, 0 errors.**

## 7. Usability Testing

| Task | Users Tested | Successfully Completed | Success Rate |
|---|---:|---:|---|
| Log in | 0 | Not measured | Not tested |
| Register a user | 0 | Not measured | Not tested |
| Generate QR code | 0 | Not measured | Not tested |
| Scan QR code | 0 | Not measured | Not tested |
| View attendance | 0 | Not measured | Not tested |
| Generate report | 0 | Not measured | Not tested |

**Observed:** No representative users were available, so completion rates, task times, user errors, assistance required and satisfaction were not measured and no success rate is claimed. What was done instead was a static inspection of the six screens, which is not user testing: Log in maps to Components/Pages/Login.razor (4 inputs, 6 label or aria-label attributes, 2 buttons); Register a user maps to Components/Pages/Accounts.razor (11 inputs, 20 labels, 20 buttons); Generate QR code maps to Components/Pages/QRCodes.razor, which is an eight-line placeholder reading "QR code generation module — coming soon", although working QR generation exists on Components/Pages/Students.razor and Components/Pages/AdminRooms.razor; Scan QR code maps to Components/Pages/TeacherStartSession.razor (camera reader plus a manual input labelled `aria-label="Paste QR token"` and inline colour-coded feedback); View attendance maps to Components/Pages/TeacherAttendanceHistory.razor (3 inputs, 2 labels); Generate report maps to Components/Pages/Reports.razor (5 buttons, 2 inputs). **Result: Not tested.**

## 8. Compatibility Testing

| Device/Platform | Browser | Result |
|---|---|---|
| Windows 11 PC | WebView2 154.0.4258.62 (embedded) | **Build passed:** 0 errors, 4 distinct warnings; runtime not tested |
| Android phone | MAUI Android WebView, target net10.0-android | **Build passed:** 0 errors, 4 distinct warnings; runtime not tested |
| iPhone and iPad | MAUI iOS WebView, target net10.0-ios | Not tested — iOS builds require a macOS host |
| Mac | Mac Catalyst, target net10.0-maccatalyst | Not tested — requires a macOS host |
| Windows 11 PC | Chrome or Edge as a standalone website | Not applicable — the product ships as a MAUI hybrid application, not a web site |
| Small screen, 360 x 640 | Responsive layout | Not tested — no device or emulator was executed |

**Observed:** Both platform targets were rebuilt from clean. The four distinct warnings are three NU1903 advisories for the transitive packages SQLitePCLRaw.lib.e_sqlite3, .android and .ios version 2.1.10 (high severity, GHSA-2m69-gcr7-jv3q) and one CS8619 nullability warning at Services/AuthService.cs:138 where `Task<UserSession>` is returned where `Task<UserSession?>` is expected. MSBuild's own tally is 5 warnings on the Windows target and 10 on the Android target because it repeats each NU1903 advisory once per restore pass; the table reports distinct warnings. No browser, device, screen-size or network-condition runtime check was executed. **Result: 2 of 6 rows built, 0 of 6 rows executed.**

## 9. Recovery Testing

| Interruption | Result |
|---|---|
| Restart the application, closing and reopening every pooled database handle | **Pass** — 3 of 3 committed records preserved |
| Transaction torn down immediately before COMMIT | **Pass** — rolled back cleanly, 0 partial rows (expected 3, actual 3) |
| Resume normal operation after every database handle is released | **Pass** — report read returned 1 session, a scan returned the defined duplicate-rejection message, record count unchanged at 3 |
| Disconnect the network during a QR scan | Not tested — no server or network path in scope |
| Temporarily stop the database during a transaction | Not tested |

**Observed:** Runner log lines `RECOVERY|restart|expected=3|actual=3|result=recovered`, `RECOVERY|interrupted-transaction|expected=3|actual=3|result=rolled back cleanly`, `RECOVERY|resume-after-release|read=ok|sessions=1|scanResponded=yes|scanSuccess=False|scanMessage=Already marked as PRESENT less than 2 minutes ago.`, `RECOVERY|final-record-count|expected=3|actual=3`. The resume check deliberately re-scans an already-recorded student, so a well-formed business rejection is the healthy outcome; an exception or a changed record count would have been the failure. Committed attendance records survived both interruptions with no duplication and no partial row. **Result: 3 Pass, 2 Not tested.**

## 10. User Acceptance Testing (UAT)

| Criterion | Result |
|---|---|
| QR scanning works correctly | **Not accepted** — 12 of 14 scan cases behaved as specified; a pre-start scan and a repeat scan after two minutes were accepted when they should have been refused |
| Attendance records are accurate | **Not accepted** — 6 rows were stored where only 4 scans met the acceptance rule; two rows should not exist |
| Duplicate attendance is prevented | **Not accepted** — prevention holds only inside a two-minute window; a repeat at three minutes created a second row |
| Reports are accurate | **Not accepted** — a report filtered on the session's local date returned zero sessions and zero records |
| System is easy to use | **Not tested** — no representative users participated |
| System performs within acceptable response time | **Accepted** — p95 23.8 ms per scan and 17.3 ms per report for a single user; 500 concurrent scans completed with 0 failures at avg 254.5 ms |

**Observed:** 1 Accepted, 4 Not accepted, 1 Not tested. No real users participated, so no criterion can be marked Accepted on usability grounds. The ISO/IEC 25010 evaluation recommended at the end of the lecture was not conducted and no quality score is claimed.

## Defect Summary

| ID | Defect | Location | Severity |
|---|---|---|---|
| D1 | Attendance is accepted before the class starts. `MarkAttendanceAsync` computes lateness from the schedule start but never checks that the current time falls within the scheduled interval, so a scan one hour before start is stored as PRESENT. | Services/TeacherSessionService.cs, `MarkAttendanceAsync` | High |
| D2 | Duplicate attendance is only blocked for two minutes. The guard is `TotalMinutes < 2`, and the model carries no unique index on (AttendanceSessionId, StudentId), so a later scan inserts a second row. | Services/TeacherSessionService.cs:396 | High |
| D3 | Reports filtered on a local date miss records. `GetReportAsync` builds bounds with `DateOnly.ToDateTime(...)` without converting to UTC and compares them against `SessionDate`, which is stored as `DateTime.UtcNow`. The same file stores UTC, so a session at 23:00 UTC on 7 October is a local 8 October session: filtering 8 October returns nothing while filtering 7 October returns the data. Elsewhere the codebase converts correctly (`TeacherSessionService.cs:722-723` and `AuditService.cs:46` both call `ToUniversalTime()`), so this is an inconsistency, not a design choice. | Services/ReportService.cs:41-42 | High |
| D4 | The dedicated QR page is an unimplemented placeholder, so the lecture's "Generate QR code" task has no destination at `/qr-codes` even though generation works on the Students and Rooms pages. | Components/Pages/QRCodes.razor | Low |

## Limitations

1. The harness calls services directly. The Blazor user interface, camera capture, modal flows, navigation and visual layout were not executed, so no UI defect can be reported or ruled out.
2. Functional cases run against an in-memory SQLite database; load, stress and recovery run against a file database with WAL. Neither resembles production data volume.
3. Load and stress issue concurrent tasks from a single local process. There is no network, no server process and no independent clients, so the figures bound local service throughput only and will not predict deployment behaviour.
4. Two test doubles are in use: a no-op audit service and a dictionary-backed Preferences implementation. Neither participates in login, scanning, reporting or storage decisions.
5. Login was verified through the credential and account-state rules, not through the graphical login form.
6. No representative users were available, so usability testing and the ISO/IEC 25010 evaluation were not conducted.
7. No clock-skew, timezone-misconfigured or low-end device was simulated beyond the host's own UTC+08:00 offset, which is what exposed defect D3.

## Overall Result

The application builds for the Windows target (0 errors) and the Android target (0 errors) from clean. Functional testing produced 17 Pass and 3 Fail across the 20 black-box tabled cases, 8 Pass and 1 Fail across 9 equivalence classes, 4 Pass, 2 Fail and 1 Not tested across 7 boundary values, and 6 Pass and 2 Fail across the decision table. Stress testing completed 100, 200 and 500 concurrent scans with zero failed transactions and 100 % record accuracy. Load testing met the 50-user expected workload with a 0.00 % error rate. Recovery testing preserved every committed record across an interrupted transaction and a full restart. Usability, runtime device and browser compatibility, network recovery and user acceptance remain unmeasured, and four defects — three High and one Low — remain open.
