"""Render the technical testing report as a paginated PDF with table layout."""

from __future__ import annotations

import html
import re
from pathlib import Path

import fitz


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "TECHNICAL_TESTING_REPORT.md"
OUTPUT = ROOT / "TECHNICAL_TESTING_REPORT.pdf"


def inline(text: str) -> str:
    text = html.escape(text.strip())
    text = re.sub(r"\*\*(.*?)\*\*", r"<b>\1</b>", text)
    text = re.sub(r"`(.*?)`", r"<code>\1</code>", text)
    text = re.sub(r"\[([^]]+)\]\(([^)]+)\)", r"\1", text)
    return text


def render_block(markdown: str) -> str:
    lines = markdown.strip().splitlines()
    output: list[str] = []
    index = 0
    while index < len(lines):
        line = lines[index].strip()
        if not line:
            index += 1
            continue
        if line.startswith("# "):
            output.append(f"<h1>{inline(line[2:])}</h1>")
        elif line.startswith("## "):
            output.append(f"<h2>{inline(line[3:])}</h2>")
        elif line.startswith("| "):
            rows = []
            while index < len(lines) and lines[index].strip().startswith("|"):
                cells = [cell.strip() for cell in lines[index].strip().strip("|").split("|")]
                if not all(re.fullmatch(r":?-{3,}:?", cell) for cell in cells):
                    rows.append(cells)
                index += 1
            output.append("<table>")
            for row_number, row in enumerate(rows):
                tag = "th" if row_number == 0 else "td"
                output.append("<tr>" + "".join(f"<{tag}>{inline(cell)}</{tag}>" for cell in row) + "</tr>")
            output.append("</table>")
            continue
        elif re.match(r"^\d+\. ", line):
            output.append(f"<p>{inline(line)}</p>")
        else:
            output.append(f"<p>{inline(line.replace('  ', ' '))}</p>")
        index += 1
    return "\n".join(output)


CSS = """
body { font-family: Arial, Helvetica, sans-serif; font-size: 9.5pt; color: #1f2937; }
h1 { font-size: 21pt; color: #17365d; margin: 0 0 18pt 0; }
h2 { font-size: 15pt; color: #17365d; margin: 0 0 13pt 0; }
p { margin: 0 0 9pt 0; line-height: 1.24; }
table { border-collapse: collapse; width: 100%; margin: 9pt 0 11pt 0; font-size: 8.2pt; }
th, td { border: 0.55pt solid #aab8c8; padding: 5pt; vertical-align: top; }
th { background: #e5edf5; color: #17365d; font-weight: bold; }
tr:nth-child(even) td { background: #f8fafc; }
code { font-family: Courier New, monospace; font-size: 8.8pt; }
"""


def main() -> None:
    markdown = SOURCE.read_text(encoding="utf-8")
    parts = re.split(r"(?=^## )", markdown, flags=re.MULTILINE)
    document = fitz.open()
    for part in parts:
        if not part.strip():
            continue
        page = document.new_page(width=595, height=842)
        content = f"<html><head><style>{CSS}</style></head><body>{render_block(part)}</body></html>"
        remaining, scale = page.insert_htmlbox(fitz.Rect(40, 42, 555, 792), content, scale_low=0.52)
        if remaining < 0:
            raise RuntimeError(f"Content did not fit page {document.page_count}: {part[:60]!r}")
        page.draw_line(fitz.Point(40, 810), fitz.Point(555, 810), color=(0.68, 0.73, 0.79), width=0.5)
        page.insert_text(fitz.Point(40, 826), "EduAttend | Technical Testing | 8 October 2026", fontsize=8, color=(0.35, 0.4, 0.45))
        page.insert_text(fitz.Point(530, 826), str(document.page_count), fontsize=8, color=(0.35, 0.4, 0.45))
        print(f"page {document.page_count}: scale={scale:.2f}")
    document.set_metadata({"title": "EduAttend Technical Testing Report", "subject": "Testing based on Lecture on Technical Testing"})
    document.save(OUTPUT, garbage=4, deflate=True)
    print(OUTPUT)


if __name__ == "__main__":
    main()
