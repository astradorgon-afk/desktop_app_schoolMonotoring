#!/usr/bin/env python3
from docx import Document
from docx.shared import Pt, RGBColor, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH as ALIGN
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

doc = Document()
for s in doc.sections:
    s.page_width = Inches(8.5); s.page_height = Inches(11)
    s.top_margin = Inches(1); s.bottom_margin = Inches(1)
    s.left_margin = Inches(1.25); s.right_margin = Inches(1.25)

def shade(cell, color):
    el = OxmlElement("w:shd")
    el.set(qn("w:fill"), color)
    el.set(qn("w:val"), "clear")
    cell._tc.get_or_add_tcPr().append(el)

def brd(cell, sz="4"):
    tcPr = cell._tc.get_or_add_tcPr()
    tcB = OxmlElement("w:tcBorders")
    for b in ("top","left","bottom","right"):
        e = OxmlElement(f"w:{b}")
        e.set(qn("w:val"), "single")
        e.set(qn("w:sz"), sz)
        e.set(qn("w:color"), "6366F1")
        tcB.append(e)
    tcPr.append(tcB)

def hdr(cell, text):
    cell.text = ""
    p = cell.paragraphs[0]
    p.alignment = ALIGN.CENTER
    r = p.add_run(text)
    r.bold = True; r.font.size = Pt(10)
    r.font.color.rgb = RGBColor.from_string("FFFFFF")
    r.font.name = "Tahoma"
    shade(cell, "4338CA"); brd(cell)

def dat(cell, text, bold=False, size=10, align="left", bg=None):
    cell.text = ""
    p = cell.paragraphs[0]
    if align == "center": p.alignment = ALIGN.CENTER
    r = p.add_run(text)
    r.bold = bold; r.font.size = Pt(size)
    r.font.color.rgb = RGBColor.from_string("1F2937")
    r.font.name = "Tahoma"
    if bg: shade(cell, bg)
    brd(cell)

def H1(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(18)
    p.paragraph_format.space_after = Pt(8)
    r = p.add_run(text)
    r.bold = True; r.font.size = Pt(16)
    r.font.color.rgb = RGBColor.from_string("4338CA")
    r.font.name = "Tahoma"
    pPr = p._p.get_or_add_pPr()
    pBdr = OxmlElement("w:pBdr")
    bt = OxmlElement("w:bottom")
    bt.set(qn("w:val"),"single"); bt.set(qn("w:sz"),"6"); bt.set(qn("w:color"),"E5E7EB")
    pBdr.append(bt); pPr.append(pBdr)

def H2(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(14)
    p.paragraph_format.space_after = Pt(6)
    r = p.add_run(text)
    r.bold = True; r.font.size = Pt(13)
    r.font.color.rgb = RGBColor.from_string("3730A3")
    r.font.name = "Tahoma"

def B(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(6)
    r = p.add_run(text)
    r.font.size = Pt(11)
    r.font.color.rgb = RGBColor.from_string("374151")
    r.font.name = "Tahoma"

def BL(text):
    p = doc.add_paragraph(style="List Bullet")
    p.paragraph_format.space_after = Pt(4)
    r = p.add_run(text)
    r.font.size = Pt(11)
    r.font.color.rgb = RGBColor.from_string("374151")
    r.font.name = "Tahoma"

def CAP(text):
    p = doc.add_paragraph()
    p.alignment = ALIGN.CENTER
    p.paragraph_format.space_before = Pt(4)
    p.paragraph_format.space_after = Pt(10)
    r = p.add_run(text)
    r.italic = True; r.font.size = Pt(9)
    r.font.color.rgb = RGBColor.from_string("6B7280")
    r.font.name = "Tahoma"

def CHK(text):
    p = doc.add_paragraph(style="List Bullet")
    p.paragraph_format.space_after = Pt(3)
    r1 = p.add_run("\u2713 ")
    r1.bold = True; r1.font.color.rgb = RGBColor.from_string("16A34A")
    r1.font.size = Pt(11); r1.font.name = "Tahoma"
    r2 = p.add_run(text)
    r2.font.size = Pt(11)
    r2.font.color.rgb = RGBColor.from_string("374151")
    r2.font.name = "Tahoma"

def PB(): doc.add_page_break()

def TBL(hdrs, hws, rows):
    tbl = doc.add_table(rows=1, cols=len(hws))
    tbl.alignment = 1
    hr = tbl.rows[0]
    for i,(t,w) in enumerate(zip(hdrs,hws)):
        hr.cells[i].width = Inches(w)
        hdr(hr.cells[i], t)
    for cv,wv,bgs,bolds,aligns in rows:
        tr = tbl.add_row()
        for i in range(len(wv)):
            tr.cells[i].width = Inches(wv[i])
            dat(tr.cells[i], cv[i], bold=bolds[i], bg=bgs[i], align=aligns[i])
    return tbl

# ═══ COVER PAGE ═══
for _ in range(12):
    doc.add_paragraph()
p = doc.add_paragraph(); p.alignment = 1
r = p.add_run("CHAPTER IV")
r.bold = True; r.font.size = Pt(30); r.font.color.rgb = RGBColor.from_string("4338CA"); r.font.name = "Tahoma"
p = doc.add_paragraph(); p.alignment = 1
r = p.add_run("RESULTS AND DISCUSSIONS")
r.bold = True; r.font.size = Pt(22); r.font.color.rgb = RGBColor.from_string("6366F1"); r.font.name = "Tahoma"
doc.add_paragraph(); doc.add_paragraph()
p = doc.add_paragraph(); p.alignment = 1
p.paragraph_format.space_before = Pt(20); p.paragraph_format.space_after = Pt(20)
r = p.add_run("UI REQUIREMENTS SPECIFICATION")
r.bold = True; r.font.size = Pt(24); r.font.color.rgb = RGBColor.from_string("1F2937"); r.font.name = "Tahoma"
p = doc.add_paragraph(); p.alignment = 1
r = p.add_run("EduAttend: School Attendance Monitoring System")
r.font.size = Pt(14); r.font.color.rgb = RGBColor.from_string("6B7280"); r.font.name = "Tahoma"; r.italic = True
doc.add_paragraph(); doc.add_paragraph(); doc.add_paragraph()
p = doc.add_paragraph(); p.alignment = 1
r = p.add_run("Derived from CHAPTER_4_ENHANCED.docx")
r.font.size = Pt(12); r.font.color.rgb = RGBColor.from_string("9CA3AF"); r.font.name = "Tahoma"
p = doc.add_paragraph(); p.alignment = 1
r = p.add_run(".NET MAUI + Blazor  |  MySQL + SQLite  |  3 User Roles")
r.font.size = Pt(12); r.font.color.rgb = RGBColor.from_string("9CA3AF"); r.font.name = "Tahoma"
PB()
print("Cover done")

# ═══ SECTION 1 ═══
H1("1. System Implementation Overview")
B("EduAttend was implemented as a hybrid desktop and mobile attendance application using .NET MAUI and Blazor. It supports MySQL for the operational school database and SQLite for a portable demonstration profile. The application organizes its functions according to three user roles: Super Administrator, Teacher, and Student. Each role receives a dedicated dashboard and only the navigation options required for its responsibilities.")
H2("1.1 Technology Stack")
BL(".NET MAUI — cross-platform desktop and mobile framework")
BL("Blazor — Razor component-based UI rendering")
BL("MySQL — operational school database")
BL("SQLite — portable demonstration profile")
BL("Entity Framework Core — data access layer")
BL("PBKDF2-SHA256 — password hashing (100,000 iterations, random salt, constant-time comparison)")
H2("1.2 User Roles and Landing Pages")
TBL(["Role","Landing Page","Navigation Access"],[2.5,2.5,2.5],
[
    (["Super Administrator","Administrator Dashboard","Full access — admin, monitoring, reports, analytics, export, audit, leave review"],
     [2.5,2.5,2.5],["EEF2FF","EEF2FF","EEF2FF"],[True,False,False],["left","left","left"]),
    (["Teacher","Teacher Dashboard","Schedule, handled sections, start session, attendance history, my attendance, leave requests, reports, notifications, profile"],
     [2.5,2.5,2.5],[None,None,None],[False,False,False],["left","left","left"]),
    (["Student","Student Dashboard","Personal attendance totals, rate, recent records, full history, notifications, profile"],
     [2.5,2.5,2.5],["F0F9FF","F0F9FF","F0F9FF"],[True,False,False],["left","left","left"]),
])
CAP("Table 1.1 — User roles, landing pages, and navigation access")
PB()
print("Section 1 done")

# ═══ SECTION 2 — FIGURE-BY-FIGURE UI SPECIFICATIONS ═══
print("Starting Section 2...")


# ── Figure 1: Login ──
H2("Figure 1: User Login and Database-Profile Selection")
B("Single login interface with database-profile selection. After successful authentication, the system creates a session containing user identifier, username, display name, role, and login time. Role determines the landing page. Passwords verified against PBKDF2-SHA256 hashes with 100,000 iterations, random salt, and constant-time comparison. Successful login and logout actions are written to the audit trail.")
TBL(["Element","Description"],[2.2,5.3],
[
    (["Database Profile Selector","Username Field","Password Field (show/hide toggle)","Remember Me Checkbox","Forgot Password Link","Sign In Button","Error Banner","Signup Prompt","Role-Based Redirect","Audit Trail Entry"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","FFFFFF","EEF2FF","FFF1F2","FFFFFF","FFFFFF","FFFFFF"],
     [True,True,True,False,False,True,False,False,False,False],["left"]*10),
    (["Dropdown to choose the active database connection profile before login (e.g., School MySQL, Demo SQLite)","Text input for the user's username","Password input with show/hide toggle; verified against PBKDF2-SHA256 hash (100,000 iterations, salt, constant-time compare)","Persist login preference across sessions","Hyperlink placeholder for password recovery flow","Submit button with loading spinner state; triggers authentication","Displays authentication failure message when credentials are invalid or account is locked","Footer text: \"Don't have an account? Create one\"","After successful auth: Super Admin → Admin Dashboard, Teacher → Teacher Dashboard, Student → Student Dashboard","Successful login and logout actions are written to the system audit log"],
     [2.2,5.3],[None]*10,[False]*10,["left"]*10),
])
CAP("Figure 1. Graphical user interface for user login and database-profile selection")
PB()
print("Fig 1 done")

# ── Figure 2: Admin Dashboard ──
H2("Figure 2: Administrator Dashboard")
B("The dashboard presents school-level counts, attendance rate, shortcuts, and operational summaries. It is supported by management pages for students, teachers, user accounts, rooms, and schedules.")
TBL(["Section","Contents"],[2.0,5.5],
[
    (["Banner Header","Metric Cards","Quick-Action Shortcuts","Management Pages (Supporting)"],
     [2.0,5.5],["EEF2FF","FFFFFF","F0F9FF","FFFFFF"],[True,True,False,False],["left"]*4),
    (["School branding, \"Administrator Dashboard\" title, subtitle, live clock (hh:mm tt), dark/light theme toggle","Active Rooms count, Total Teachers, Total Students, Overall Attendance Rate %","Buttons/cards linking to: Students, Teachers, Accounts, Rooms, Schedules, Live Monitoring, Reports, Admin Analytics, Export Data, Profile Settings, Exit Dashboard","Students (LRN, student #, personal data, guardian, status, grade level, section); Teachers (employee info, specialization, employment status, advisory, subject assignments); Accounts (create users, assign roles, reset passwords, control status & login permission); Rooms (codes, names, capacities, active status, QR info); Schedules (sections + subjects + teachers + rooms + days + time slots)"],
     [2.0,5.5],[None]*4,[False]*4,["left"]*4),
])
CAP("Figure 2. Graphical user interface of the administrator dashboard")
PB()
print("Fig 2 done")


# ── Figure 3: Schedule Management ──
H2("Figure 3: Class Schedule Management Interface")
B("The scheduling module was implemented to connect sections and subjects with teachers, rooms, days, and time periods. The administrator can add, update, and remove schedule entries. Selection lists are populated from active teachers, subjects, sections, rooms, and the active school year, which helps prevent the entry of unsupported values.")
TBL(["Element","Description"],[2.2,5.3],
[
    (["Weekly Calendar Grid","Cell Click → Modal","Add Schedule Form","Conflict Detection","Filters","Edit/Delete","Refresh"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","FFFFFF","FFFFFF","FFFFFF"],[True,False,False,False,False,False,False],["left"]*7),
    (["Monday–Sunday columns with time-slot rows; cells display scheduled classes","Clicking any empty or occupied cell opens an add/edit modal pre-filled with the selected day and time slot","Section dropdown, Subject dropdown, Teacher dropdown (filtered to available teachers at that time), Room text input, End Time picker; validated: end time > start time","Alert banner when overlapping classes share a teacher, section, or room; expands to show conflicting entries","Grade Level filter (G7–G10), Status filter (Active/Finalized), search by section or subject","Existing entries can be modified or removed; schedule changes reflect immediately in the grid and in teacher/student views","Manual refresh button to reload schedule data"],
     [2.2,5.3],[None]*7,[False]*7,["left"]*7),
])
CAP("Figure 3. Graphical user interface for class schedule management")
PB()
print("Fig 3 done")

# ── Figure 4: Teacher Start Session & QR Scanner ──
H2("Figure 4: Teacher Start Session and QR Scanner")
B("The QR-based attendance module is the core operational result of EduAttend. Each student is associated with a unique QR record. A teacher opens the Start Session page, selects an assigned class offering, and creates or reuses that day's attendance session. The system then retrieves the active students enrolled in the offering's section and school year. When a QR code is scanned, the application identifies the student and records the attendance.")
TBL(["Element","Description"],[2.2,5.3],
[
    (["Status Bar","Idle Badge","Class Offering Dropdown","Start/Resume Session","Student Roster","QR Scan Mode Toggle","Camera QR Scanner","Manual QR Input","Attendance Status Badges","Roster Search","Notification Cards","Sync Queue Counter","Attendance Undo"],
     [2.2,5.3],["EEF2FF"]+["FFFFFF"]*4+["F0F9FF"]*2+["FFFFFF"]*6,
     [True]+[False]*12,["left"]*13),
    (["Live badge (green dot + \"Live\") when session active; section name, subject name, room label; event sync counter; Scan button; End Session button","\"No Session\" badge with hint: \"Scan a QR code or select a class below\" shown when no session is active","List of class offerings assigned to the teacher; selecting one populates the session context","Creates a new attendance session for the day or resumes an existing one for the selected offering","List of active students enrolled in the section + school year; each row shows name, student number, LRN, and attendance status badge","Switch between Student QR scanning and Manual QR code input","Web API camera integration to scan student QR codes in real time","Text field to paste/type a QR token string for attendance marking","Per student: Present (green), Late (yellow), Absent (red), Unmarked (gray); click to toggle status","Filter roster by full name, student number, or LRN","Real-time feedback cards for scan events, session start/end, and sync status","Shows pending attendance events queued for server sync (offline-resilient design)","Ability to undo the last attendance mark per session"],
     [2.2,5.3],[None]*13,[False]*13,["left"]*13),
])
CAP("Figure 4. Graphical user interface for starting a class session and scanning student QR codes")
PB()
print("Fig 4 done")


# ── Figure 5: Teacher Dashboard ──
H2("Figure 5: Teacher Dashboard")
B("The teacher dashboard displays assigned classes, handled sections, and session shortcuts.")
TBL(["Section","Contents"],[2.0,5.5],
[
    (["Banner Header","Metric Cards","Today's Classes List","Weekly Mini Calendar","Quick-Action Shortcuts","Profile & Notifications"],
     [2.0,5.5],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","FFFFFF","FFFFFF"],[True,True,False,False,False,False],["left"]*6),
    (["\"Teacher Dashboard\" title, \"Welcome back, [Teacher Name]\", live clock, dark/light theme toggle","Today's Classes, Weekly Classes, Sections Handled, Subjects Taught","Time, subject, section, room, and status badge (Upcoming/In Progress/Completed) for each class today","Visual week overview with class count per day","My Schedule, Handled Sections, Start Session, Attendance History, My Attendance, Leave Requests, Reports","Links to teacher profile settings and notification feed"],
     [2.0,5.5],[None]*6,[False]*6,["left"]*6),
])
CAP("Figure 5. Graphical user interface of the teacher dashboard")
PB()
print("Fig 5 done")

# ── Figure 6: Live Monitoring ──
H2("Figure 6: Live Room and Attendance Monitoring")
B("Live monitoring shows current, upcoming, or completed room activity and attendance counts. The live-monitoring service uses the scheduled time and room assignment to decide whether the displayed class is currently in session, is the next class of the day, or is the most recently completed class. The result is a time- and location-aware attendance structure.")
TBL(["Element","Description"],[2.2,5.3],
[
    (["View Toggle","Fullscreen Mode","Live Clock","Auto-Refresh","In Session Group","Next Up Group","Recently Completed Group","No Scheduled Class Group","Room Card Contents"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF"],
     [True]+[False]*8,["left"]*9),
    (["Switch between Card view and List view","Expand to fullscreen for display on room monitors; shows clock + rooms only","Real-time clock (h:mm:ss tt) and date (dddd, MMMM dd, yyyy); updates every second","Rooms data refreshes every 30 seconds automatically; manual refresh button also available","Classes where current time falls within start–end time; highlighted priority position at top of list","Upcoming classes sorted by start time; shown after in-session classes","Classes whose end time has passed since the last refresh","Rooms with no class scheduled for the current day","Room name/code, section name, subject, teacher, start–end time, attendance count (present/late/absent)"],
     [2.2,5.3],[None]*9,[False]*9,["left"]*9),
])
CAP("Figure 6. Graphical user interface for live room and attendance monitoring")
PB()
print("Fig 6 done")


# ═══ SECTION 3 — COMPLETE UI PAGE INVENTORY ═══
H1("3. Complete UI Page Inventory")
B("The following table lists every UI page in the EduAttend system, its file location, its primary user role, and the module it belongs to.")
TBL(["Page","File","Role","Module"],[1.8,2.6,1.4,1.2],
[
    (["Login","Components/Pages/Login.razor","All","Authentication"],
     [1.8,2.6,1.4,1.2],["EEF2FF","EEF2FF","EEF2FF","EEF2FF"],[True,False,False,False],["left","left","left","left"]),
    (["Administrator Dashboard","Components/Pages/Dashboard.razor","Super Admin","Admin Dashboard"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Students Management","Components/Pages/Students.razor","Super Admin","Master Data"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Teachers Management","Components/Pages/Teachers.razor","Super Admin","Master Data"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Accounts Management","Components/Pages/Accounts.razor","Super Admin","Master Data"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Rooms Management","Components/Pages/AdminRooms.razor","Super Admin","Master Data"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Schedules Management","Components/Pages/Schedules.razor","Super Admin","Scheduling"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Live Monitoring","Components/Pages/LiveMonitoring.razor","Super Admin","Monitoring"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Teacher Dashboard","Components/Pages/TeacherDashboard.razor","Teacher","Teacher Dashboard"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["My Schedule","Components/Pages/TeacherSchedule.razor","Teacher","Schedule"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Handled Sections","Components/Pages/TeacherHandledSections.razor","Teacher","Sections"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Start Session (QR)","Components/Pages/TeacherStartSession.razor","Teacher","QR Attendance"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Attendance History","Components/Pages/TeacherAttendanceHistory.razor","Teacher","Attendance"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["My Attendance","Components/Pages/TeacherMyAttendance.razor","Teacher","Attendance"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Notifications","Components/Pages/TeacherNotifications.razor","Teacher + Student","Notifications"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Profile Settings","Components/Pages/TeacherProfile.razor","Teacher","Profile"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Admin Analytics","Components/Pages/AdminAnalytics.razor","Super Admin","Reports & Analytics"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Reports","Components/Pages/Reports.razor","Super Admin + Teacher","Reports & Analytics"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Export Data","Components/Pages/ExportData.razor","Super Admin","Export"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Leave Requests","Components/Pages/LeaveRequests.razor","Teacher + Super Admin","Leave Processing"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Audit Log","Components/Pages/AuditLog.razor","Super Admin","Audit"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Student Dashboard","Components/Pages/StudentDashboard.razor","Student","Student Portal"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Student Attendance History","Components/Pages/StudentAttendanceHistory.razor","Student","Student Portal"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["Login Layout","Components/Layout/LoginLayout.razor","All","Layout"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Main Layout (sidebar)","Components/Layout/MainLayout.razor","All","Layout"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
    (["NavMenu (role-aware)","Components/Layout/NavMenu.razor","All","Navigation"],
     [1.8,2.6,1.4,1.2],[None,None,None,None],[False,False,False,False],["left","left","left","left"]),
    (["Dashboard Layout","Components/Layout/DashboardLayout.razor","Admin + Teacher","Layout"],
     [1.8,2.6,1.4,1.2],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","left","left","left"]),
])
CAP("Table 3.1 — Complete UI page inventory with file locations and role assignments")
PB()
print("Section 3 done")


# ═══ SECTION 4 — MODULE-TO-UI MAPPING ═══
H1("4. Module-to-UI Mapping (Chapter 4, Table 1)")
B("Table 1 in Chapter 4 lists every implemented module and its primary user. The mapping below cross-references each module to its UI page(s).")
TBL(["Module","Primary User","Implemented UI Page(s)"],[2.2,1.4,3.4],
[
    (["Authentication and role-based access","All users"],
     [2.2,1.4],["EEF2FF","EEF2FF"],[True,True],["left","left"]),
    (["Administrator dashboard","Super Administrator"],
     [3.4],["EEF2FF"],[False],["left"]),
    (["Student, teacher, account, room, and schedule management","Super Administrator"],
     [3.4],["F0F9FF"],[False],["left"]),
    (["Teacher dashboard and schedule","Teacher"],
     [3.4],["FFFFFF"],[False],["left"]),
    (["QR attendance session","Teacher"],
     [3.4],["EEF2FF"],[False],["left"]),
    (["Live monitoring","Super Administrator"],
     [3.4],["F0F9FF"],[False],["left"]),
    (["Reports, analytics, and export","Super Admin + Teacher"],
     [3.4],["FFFFFF"],[False],["left"]),
    (["Leave-request processing","Teacher and Super Administrator"],
     [3.4],["EEF2FF"],[False],["left"]),
    (["Audit log","Super Administrator"],
     [3.4],["F0F9FF"],[False],["left"]),
    (["Student portal","Student"],
     [3.4],["FFFFFF"],[False],["left"]),
    (["Login.razor, NavMenu.razor (role-aware routing & navigation)","Dashboard.razor","Students.razor, Teachers.razor, Accounts.razor, AdminRooms.razor, Schedules.razor","TeacherDashboard.razor, TeacherSchedule.razor","TeacherStartSession.razor","LiveMonitoring.razor","AdminAnalytics.razor, Reports.razor, ExportData.razor","LeaveRequests.razor","AuditLog.razor","StudentDashboard.razor, StudentAttendanceHistory.razor"],
     [3.4],[None,None,None,None,None,None,None,None,None,None],[False]*10,["left"]*10),
])
CAP("Table 4.1 — Module-to-UI mapping corresponding to Chapter 4, Table 1")
PB()
print("Section 4 done")

# ═══ SECTION 5 — DATA FIELD REQUIREMENTS ═══
H1("5. Data Field Requirements by Management Page")
B("Chapter 4 specifies the data fields required for each master-data management page. These fields must be present in the corresponding UI forms and tables.")

H2("5.1 Student Records")
TBL(["Field","Description"],[2.2,5.3],
[
    (["Learner Reference Number (LRN)","Student Number","Personal Data","Guardian Information","Status","Grade Level","Section Placement","QR Code"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF","FFFFFF","F0F9FF","FFFFFF"],[True,False,False,False,True,False,True,False],["left"]*8),
    (["Unique learner identifier; used for roster and QR association","Internal student identification number","First name, last name, middle name, email, contact number","Guardian name and contact details","ACTIVE, INACTIVE, or LOCKED","Grade placement (e.g., G7, G8, G9, G10)","Assigned class section","Unique QR token associated with the student for attendance scanning"],
     [2.2,5.3],[None]*8,[False]*8,["left"]*8),
])
CAP("Table 5.1 — Student record fields")

H2("5.2 Teacher Records")
TBL(["Field","Description"],[2.2,5.3],
[
    (["Employee Information","Specialization","Employment Status","Advisory Assignment","Subject Assignments","Profile Picture"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF","F0F9FF"],[True,False,False,False,True,False],["left"]*6),
    (["Employee number, first name, last name, middle name, email, contact number","Subject area or teaching specialization","Active or inactive employment status","Section(s) advised by the teacher","Subjects taught and associated sections","Optional photo, max 5MB"],
     [2.2,5.3],[None]*6,[False]*6,["left"]*6),
])
CAP("Table 5.2 — Teacher record fields")

H2("5.3 Account Management Fields")
TBL(["Field","Description"],[2.2,5.3],
[
    (["Username","Password","Display Name","Role","Status","Login Permission (CanLogin)"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF","FFFFFF"],[True,False,False,False,True,False],["left"]*6),
    (["Unique login username","PBKDF2-SHA256 hashed; 100,000 iterations with random salt","Full name shown in the interface","SUPERADMIN, TEACHER, or STUDENT","ACTIVE, INACTIVE, or LOCKED","Boolean flag controlling whether the account can log in"],
     [2.2,5.3],[None]*6,[False]*6,["left"]*6),
])
CAP("Table 5.3 — Account management fields")

H2("5.4 Room Management Fields")
TBL(["Field","Description"],[2.2,5.3],
[
    (["Room Code","Room Name","Capacity","Active Status","Room QR Information"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF"],[True,False,False,False,True],["left"]*5),
    (["Unique room identifier code","Descriptive room name","Maximum number of students the room can hold","Whether the room is currently in use","QR code data for room-based attendance or identification"],
     [2.2,5.3],[None]*5,[False]*5,["left"]*5),
])
CAP("Table 5.4 — Room management fields")

H2("5.5 Schedule Entry Fields")
TBL(["Field","Description"],[2.2,5.3],
[
    (["Class Offering","Teacher","Subject","Section","School Year","Room","Day of Week","Start Time","End Time"],
     [2.2,5.3],["EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF","FFFFFF","F0F9FF","FFFFFF","EEF2FF"],[True,False,False,False,True,False,False,False,True],["left"]*9),
    (["Links teacher + subject + section + school year","Assigned teacher (from active teachers list)","Subject being taught (from active subjects list)","Student section (from active sections list)","Active school year context","Room assignment (from active rooms list)","Monday through Sunday","Class start time","Class end time (must be later than start time)"],
     [2.2,5.3],[None]*9,[False]*9,["left"]*9),
])
CAP("Table 5.5 — Schedule entry fields")
PB()
print("Section 5 done")


# ═══ SECTION 6 — FUNCTIONAL WORKFLOWS ═══
H1('6. Functional Workflows (Pseudocode Translations)')
B('Chapter 4 includes pseudocode for key administrative functions. These are translated into UI workflow steps below.')

H2('6.1 MaintainMasterRecord — Administrator Master-Data Maintenance')
TBL(['Step','UI Action'],[0.8,6.7],
[
    ([str(i),desc],[0.8,6.7],[bg],[bold],[align])
    for i,desc,bg,bold,align in [
        (1,'Open the required management page (Students, Teachers, Accounts, Rooms, or Schedules)','EEF2FF',True,'left'),
        (2,'Search for an existing record using its unique identifier (name, employee no, LRN, student #, room code, etc.)','FFFFFF',False,'left'),
        (3,'Validate required fields and uniqueness rules (form validation, duplicate username check, etc.)','F0F9FF',False,'left'),
        (4,'If Add: fill the create form with valid data and save the new record','FFFFFF',False,'left'),
        (5,'If Edit: modify the selected record\'s fields and save changes','EEF2FF',False,'left'),
        (6,'If the operation changes access or status (lock/unlock, role change, login permission): apply the selected state via toggle or dropdown','FFFFFF',False,'left'),
        (7,'The system records the administrative action in the audit log automatically','F0F9FF',False,'left'),
        (8,'The displayed list refreshes and the administrator receives a notification/confirmation of the action','FFFFFF',False,'left'),
    ]
])
CAP('Table 6.1 — MaintainMasterRecord workflow')

H2('6.2 CreateClassSchedule — Schedule Creation Workflow')
TBL(['Step','UI Action'],[0.8,6.7],
[
    ([str(i),desc],[0.8,6.7],[bg],[bold],[align])
    for i,desc,bg,bold,align in [
        (1,'Display active scheduling reference data: teachers, subjects, sections, rooms, school year, days, time slots','EEF2FF',True,'left'),
        (2,'Receive the selected subject, teacher, section, room, day, and time from the add/edit modal form','FFFFFF',False,'left'),
        (3,'Validate that the end time is later than the start time; show validation error if not','F0F9FF',False,'left'),
        (4,'Create or locate the class offering for the active school year','FFFFFF',False,'left'),
        (5,'Save the room and time assignment as a class schedule entry','EEF2FF',False,'left'),
        (6,'Refresh the schedule view to show the new or updated entry in the weekly calendar grid','FFFFFF',False,'left'),
    ]
])
CAP('Table 6.2 — CreateClassSchedule workflow')
PB()
print('Section 6 done')

# ═══ SECTION 7 — IMPLEMENTATION VERIFICATION CHECKLIST ═══
H1('7. Implementation Verification Checklist')
B('The following checklist verifies that every UI element described in Chapter 4 has a corresponding implementation in the codebase.')

H2('7.1 Authentication & Access Control')
CHK('Login page with database profile selector (Login.razor)')
CHK('Username + password fields with show/hide toggle')
CHK('PBKDF2-SHA256 password verification (100,000 iterations, salt, constant-time compare)')
CHK('Remember me checkbox')
CHK('Forgot password link placeholder')
CHK('Sign-in button with loading state')
CHK('Error banner for auth failures')
CHK('Role-based post-login redirect (Super Admin → Dashboard, Teacher → TeacherDashboard, Student → StudentDashboard)')
CHK('Login/logout actions written to audit log')
CHK('Role-aware navigation menu (NavMenu.razor) — admin pages hidden from teacher/student views')

H2('7.2 Administrator Dashboard & Management')
CHK('Administrator Dashboard with metrics (active rooms, teachers, students, attendance rate)')
CHK('Quick-action shortcut cards to all management pages')
CHK('Students management page: list, search (name/LRN/student #), filter (section/grade/status), QR print/download, detail modal')
CHK('Teachers management page: list, search (name/employee no/specialization), filter (status), detail modal')
CHK('Accounts management page: create user, assign role, reset password, toggle lock/status, control login permission, stats, search, pagination')
CHK('Rooms management page: list, search, filter (status/schedule), QR generation/print/download, stats')
CHK('Schedules management page: weekly calendar grid, add/edit/delete, conflict detection, filters (grade/status), search')

H2('7.3 Teacher Modules')
CHK('Teacher Dashboard with metrics (today's classes, weekly, sections, subjects)')
CHK('Today's classes list with time/subject/section/room/status')
CHK('Weekly mini calendar view')
CHK('My Schedule page (weekly timetable, stats, legend, inline leave request)')
CHK('Handled Sections page (section cards, student list, schedule per section)')
CHK('Start Session page with class offering dropdown, QR scanner (camera + manual input), roster with status badges, search, notification cards, sync queue')
CHK('Attendance History page (past sessions, expandable roster, date filter, summary stats)')
CHK('My Attendance page (teacher's own held/absent classes, date range filter)')
CHK('Notifications page (activity feed for teacher and student)')
CHK('Profile Settings page (photo upload up to 5MB, personal info edit, account settings)')
CHK('Leave Requests page (submit form with category/reason, affected classes preview, my requests list, cancel)')

H2('7.4 Monitoring & Reporting')
CHK('Live Monitoring page: card/list view toggle, fullscreen mode, live clock, auto-refresh (30s) + manual refresh, in-session/next-up/recently-completed/no-class grouping, room cards with attendance counts')
CHK('Admin Analytics page: donut charts (overall/today/late/absent rates), line chart (monthly trend), bar charts (day-of-week/sections/grade levels), stat cards')
CHK('Reports page: date range picker, summary cards, section breakdown table, daily trend table, CSV export with preview')
CHK('Export Data page (admin): export cards for 8 datasets, preview modal with tabs, Export All, save to Desktop as CSV')

H2('7.5 Student Portal')
CHK('Student Dashboard: profile photo (uploadable 2MB), name, student #, grade level, section, personal stats (total/sessions, present, late, absent, rate %), recent records, active classes')
CHK('Student Attendance History: full record table (date, subject, teacher, status badge, reason column)')
CHK('Student notifications feed (shared notifications page)')

H2('7.6 Cross-Cutting Features')
CHK('Dark/light theme toggle on admin and teacher dashboards')
CHK('Audit Log page: stats (today's events, action breakdown), filters (actor/date/action/entity), paginated table (50/page), entry detail modal, 15 action types tracked, refresh button')
CHK('Leave Requests: teacher submit form (dates, category, reason, affected classes preview), admin review (approve/reject/cancel, filters, detail view)')
CHK('.NET MAUI + Blazor hybrid architecture')
CHK('MySQL (operational) + SQLite (demo) dual database support')
CHK('Entity Framework Core data model with shared attendance-session and attendance-record entities')
CHK('QR token entity linking students to unique QR records')
CHK('Offline-resilient sync queue for attendance events')
PB()
print('Section 7 done')

# ═══ SECTION 8 — ISO IEC 25010 UI IMPLICATIONS ═══
H1("8. UI Implications from ISO/IEC 25010 Evaluation")
B("Chapter 4 includes a provisional ISO/IEC 25010 evaluation with an overall consolidated mean of 4.50 (Strongly Agree). The following UI implications are drawn from the evaluation results.")
TBL(["Quality Characteristic","Mean","Interpretation","UI Implication"],[1.8,0.7,1.2,3.8],
[
    (["Functional Suitability","4.61","Strongly Agree","Highest-rated characteristic. All required functions are present and role-specific. Continue to ensure every module has complete UI coverage."],
     [1.8,0.7,1.2,3.8],["EEF2FF","EEF2FF","EEF2FF","EEF2FF"],[True,False,False,False],["left","center","left","left"]),
    (["Performance Efficiency","4.41","Agree","Lowest-rated characteristic. Areas for improvement: concurrent-use performance, large-report generation speed, database optimization. UI should show loading states and progress indicators for heavy operations."],
     [1.8,0.7,1.2,3.8],[None,None,None,None],[False,False,False,False],["left","center","left","left"]),
    (["Compatibility","4.49","Agree","Interface adapts across supported screen sizes and device layouts. Test on all .NET MAUI targets (Windows, Android, iOS, MacCatalyst)."],
     [1.8,0.7,1.2,3.8],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","center","left","left"]),
    (["Usability","4.54","Strongly Agree","Role-based interfaces are understandable. Maintain clear navigation, consistent form layouts, and unambiguous status badges."],
     [1.8,0.7,1.2,3.8],[None,None,None,None],[False,False,False,False],["left","center","left","left"]),
    (["Reliability","4.49","Agree","Attendance records are consistent. UI should handle error states gracefully (retry buttons, error banners, offline indicators)."],
     [1.8,0.7,1.2,3.8],["EEF2FF","EEF2FF","EEF2FF","EEF2FF"],[True,False,False,False],["left","center","left","left"]),
    (["Security","4.53","Strongly Agree","PBKDF2-SHA256 hashing, role-based access, account lock/reset, audit trail. UI must not expose restricted functions to unauthorized roles."],
     [1.8,0.7,1.2,3.8],[None,None,None,None],[False,False,False,False],["left","center","left","left"]),
    (["Maintainability","4.50","Strongly Agree","Separation of Blazor pages, services, entities, and DB context. Support automated service and integration tests."],
     [1.8,0.7,1.2,3.8],["F0F9FF","F0F9FF","F0F9FF","F0F9FF"],[True,False,False,False],["left","center","left","left"]),
    (["Portability","4.43","Agree","Interface adapts across screen sizes. Final claims need installation/device testing on every platform in the project scope."],
     [1.8,0.7,1.2,3.8],[None,None,None,None],[False,False,False,False],["left","center","left","left"]),
])
CAP("Table 8.1 — ISO/IEC 25010 evaluation ratings and UI implications")
PB()
print("Section 8 done")

# ═══ SECTION 9 — CHAPTER SYNTHESIS ═══
H1("9. Chapter Synthesis — UI Readiness")
B("The development results show that EduAttend implements an integrated attendance workflow from master-data preparation to class scheduling, QR-supported attendance capture, live monitoring, analytics, reporting, export, leave processing, audit review, and student self-service.")
B("The use of shared attendance-session and attendance-record data allows information entered by a teacher to be reused by administrative and student modules without duplicate encoding.")

H2("9.1 Technical Inspection Confirmation")
CHK("Presence of required role structures (Super Admin, Teacher, Student)")
CHK("Presence of security structures (PBKDF2-SHA256, role-based access, account lock/reset)")
CHK("Presence of data structures (attendance session, attendance record, QR token, audit log)")
CHK("Presence of reporting structures (analytics charts, report tables, CSV export)")
CHK("Successful Windows build with zero compilation errors")

H2("9.2 Areas for Continued Verification")
BL("Concurrent-use performance testing under load")
BL("Large-report generation performance and memory usage")
BL("Database query optimization for dashboards and filtered lists")
BL("Platform-specific installation testing on all .NET MAUI targets")
BL("Replace provisional ISO/IEC 25010 ratings with validated questionnaire results before final submission")

H2("9.3 Current Gaps")
TBL(["Gap","Status","Recommendation"],[3.0,1.2,2.3],
[
    (["QRCodes.razor — standalone QR code management page is a stub (\"coming soon\")","Not implemented","Implement if a dedicated admin QR management page is required by the chapter figures; QR generation already exists in Students.razor and AdminRooms.razor"],
     [3.0,1.2,2.3],["FFF1F2","FFF1F2","FFF1F2"],[True,False,False],["left","center","left"]),
    (["Student profile edit page — students can upload photos from dashboard but no dedicated profile editing page exists","Partial","Consider adding StudentProfile.razor for full student self-service edit capabilities (name, contact, etc.)"],
     [3.0,1.2,2.3],[None,None,None],[False,False,False],["left","center","left"]),
])
CAP("Table 9.1 — Identified gaps and recommendations")
PB()
print("Section 9 done")

# ═══ SAVE DOCUMENT ═══
doc.save("UI_Requirements_Specification.docx")
print("SAVED: UI_Requirements_Specification.docx")
