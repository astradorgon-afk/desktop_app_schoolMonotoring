#!/usr/bin/env python3
"""Append Sections 8-9 + save call to gen_ui_doc.py"""

content = r'''
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
'''

with open('gen_ui_doc.py', 'a', encoding='utf-8') as f:
    f.write(content)

print(f"Appended {len(content)} bytes to gen_ui_doc.py")
