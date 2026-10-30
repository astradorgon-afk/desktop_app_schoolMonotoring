#!/usr/bin/env python3
"""Fix apostrophe issues in gen_ui_doc.py"""
with open('gen_ui_doc.py', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: apostrophe in "record's" 
old1 = "(5,'If Edit: modify the selected record's fields and save changes','EEF2FF',False,'left'),"
new1 = "(5,'If Edit: modify the selected record\\'s fields and save changes','EEF2FF',False,'left'),"
content = content.replace(old1, new1)

# Fix 2: any other unescaped apostrophes in single-quoted strings
# Check for remaining issues
import re
# Find all single-quoted strings that contain apostrophes (word characters before/after)
issues = re.findall(r"'[^']*'[a-zA-Z]", content)
if issues:
    print(f"Found {len(issues)} potential issues:")
    for issue in issues[:10]:
        print(f"  {issue}")

with open('gen_ui_doc.py', 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed. Verifying syntax...")
import py_compile
try:
    py_compile.compile('gen_ui_doc.py', doraise=True)
    print("Syntax OK!")
except py_compile.PyCompileError as e:
    print(f"Syntax error: {e}")
