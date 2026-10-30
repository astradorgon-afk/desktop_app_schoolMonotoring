#!/usr/bin/env python3
"""Meta-script: generates gen_ui_doc.py then runs it to produce the docx."""
import os

OUTPUT_SCRIPT = r'''#!/usr/bin/env python3
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
'''

with open('gen_ui_doc.py', 'w', encoding='utf-8') as f:
    f.write(OUTPUT_SCRIPT)

print("Base script written OK")
