#!/usr/bin/env python3
import re

with open('gen_ui_doc.py', 'r', encoding='utf-8') as f:
    content = f.read()

lines = content.split('\n')
fixed = []

for line in lines:
    stripped = line.strip()
    # Skip comments, blank lines, and lines that already use double quotes
    if (not stripped or stripped.startswith('#') or 
        stripped.startswith('def ') or stripped.startswith('from ') or
        stripped.startswith('import ') or stripped.startswith('doc.') or
        stripped.startswith('for s') or stripped.startswith('    s.') or
        stripped.startswith('    el') or stripped.startswith('    tc') or
        stripped.startswith('        e') or stripped.startswith('    tcPr') or
        stripped.startswith('    p =') or stripped.startswith('    r.') or
        stripped.startswith('    pPr') or stripped.startswith('    bt') or
        stripped.startswith('    pBdr') or stripped.startswith('    r1') or
        stripped.startswith('    r2') or stripped.startswith('    tbl') or
        stripped.startswith('    hr') or stripped.startswith('        hr') or
        stripped.startswith('        tr') or stripped.startswith('            tr') or
        stripped.startswith('    for') or stripped.startswith('        for') or
        stripped.startswith('            dat') or stripped.startswith('    return') or
        stripped.startswith('print(') or stripped.startswith('#') or
        '"' in stripped.split("'", 1)[0]):  # uses double quotes
        fixed.append(line)
        continue
    
    # This is a line with single-quoted strings - escape internal apostrophes
    # We'll do simple replacement: find '...' patterns and escape internal '
    result = []
    i = 0
    in_str = False
    while i < len(line):
        ch = line[i]
        if not in_str:
            if ch == "'":
                in_str = True
                result.append(ch)
            else:
                result.append(ch)
        else:
            if ch == "\\":
                result.append(ch)
                if i+1 < len(line):
                    result.append(line[i+1])
                    i += 1
            elif ch == "'":
                in_str = False
                result.append(ch)
            elif ch == "'":
                # Unescaped apostrophe - escape it
                result.append("\\'")
            else:
                result.append(ch)
        i += 1
    
    fixed.append(''.join(result))

new_content = '\n'.join(fixed)

with open('gen_ui_doc.py', 'w', encoding='utf-8') as f:
    f.write(new_content)

print(f"Fixed content written. Length: {len(new_content)}")

# Verify
import py_compile
try:
    py_compile.compile('gen_ui_doc.py', doraise=True)
    print("SUCCESS: File compiles!")
except py_compile.PyCompileError as e:
    print(f"Still has error: {e}")
