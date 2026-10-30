const fs = require("fs");
const path = require("path");
const {
  AlignmentType,
  BorderStyle,
  Document,
  Footer,
  HeadingLevel,
  PageNumber,
  Packer,
  Paragraph,
  ShadingType,
  Table,
  TableCell,
  TableRow,
  TextRun,
  VerticalAlign,
  WidthType,
} = require("docx");

const sourcePath = path.join(__dirname, "CHAPTER_4_RESULTS_AND_DISCUSSIONS.md");
const outputPath = path.join(__dirname, "CHAPTER_4_RESULTS_AND_DISCUSSIONS.docx");
const source = fs.readFileSync(sourcePath, "utf8").replace(/\r\n/g, "\n");

const PAGE_WIDTH = 12240;
const PAGE_HEIGHT = 15840;
const CONTENT_WIDTH = 9360;
const thin = { style: BorderStyle.SINGLE, size: 4, color: "B7C1CE" };
const borders = { top: thin, bottom: thin, left: thin, right: thin };

function inlineRuns(text, options = {}) {
  const parts = text.split(/(\*\*[^*]+\*\*|`[^`]+`|\*[^*]+\*)/g).filter(Boolean);
  return parts.map((part) => {
    if (part.startsWith("**") && part.endsWith("**")) {
      return new TextRun({ text: part.slice(2, -2), bold: true, ...options });
    }
    if (part.startsWith("`") && part.endsWith("`")) {
      return new TextRun({ text: part.slice(1, -1), font: "Consolas", size: 20, ...options });
    }
    if (part.startsWith("*") && part.endsWith("*")) {
      return new TextRun({ text: part.slice(1, -1), italics: true, ...options });
    }
    return new TextRun({ text: part, ...options });
  });
}

function bodyParagraph(text) {
  return new Paragraph({
    alignment: AlignmentType.JUSTIFIED,
    spacing: { line: 360, after: 160 },
    indent: { firstLine: 720 },
    children: inlineRuns(text),
  });
}

function figurePlaceholder(text) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { before: 160, after: 120 },
    border: {
      top: { style: BorderStyle.DASHED, size: 6, color: "8B96A5" },
      bottom: { style: BorderStyle.DASHED, size: 6, color: "8B96A5" },
    },
    shading: { fill: "F2F4F7", type: ShadingType.CLEAR },
    children: [new TextRun({ text: text.slice(1, -1), italics: true, color: "5B6573", size: 22 })],
  });
}

function codeParagraph(line) {
  return new Paragraph({
    spacing: { line: 260, after: 0 },
    indent: { left: 360, right: 360 },
    shading: { fill: "F5F7FA", type: ShadingType.CLEAR },
    children: [new TextRun({ text: line || " ", font: "Consolas", size: 18 })],
  });
}

function makeTable(rows) {
  const columnCount = Math.max(...rows.map((row) => row.length));
  let widths;
  if (columnCount === 2) widths = [6500, 2860];
  else if (columnCount === 3) widths = [5000, 1450, 2910];
  else if (columnCount === 4) widths = [2500, 1900, 1900, 3060];
  else if (columnCount === 7) widths = [3300, 850, 850, 850, 850, 850, 1810];
  else widths = Array(columnCount).fill(Math.floor(CONTENT_WIDTH / columnCount));
  widths[widths.length - 1] += CONTENT_WIDTH - widths.reduce((a, b) => a + b, 0);

  const tableRows = rows.map((row, rowIndex) => new TableRow({
    tableHeader: rowIndex === 0,
    cantSplit: true,
    children: Array.from({ length: columnCount }, (_, columnIndex) => {
      const value = row[columnIndex] || "";
      return new TableCell({
        borders,
        width: { size: widths[columnIndex], type: WidthType.DXA },
        verticalAlign: VerticalAlign.CENTER,
        margins: { top: 90, bottom: 90, left: 100, right: 100 },
        shading: rowIndex === 0
          ? { fill: "DCE6F1", type: ShadingType.CLEAR }
          : undefined,
        children: [new Paragraph({
          alignment: columnIndex === 0 || columnIndex === columnCount - 1
            ? AlignmentType.LEFT
            : AlignmentType.CENTER,
          spacing: { after: 0 },
          children: inlineRuns(value, { size: 18, bold: rowIndex === 0 }),
        })],
      });
    }),
  }));

  return new Table({
    width: { size: CONTENT_WIDTH, type: WidthType.DXA },
    columnWidths: widths,
    rows: tableRows,
  });
}

function parseTable(lines, start) {
  const rows = [];
  let index = start;
  while (index < lines.length && lines[index].trim().startsWith("|")) {
    const cells = lines[index].trim().slice(1, -1).split("|").map((cell) => cell.trim());
    const isDivider = cells.every((cell) => /^:?-{3,}:?$/.test(cell));
    if (!isDivider) rows.push(cells);
    index += 1;
  }
  return { table: makeTable(rows), next: index };
}

const lines = source.split("\n");
const children = [];
let paragraphBuffer = [];
let inCode = false;

function flushParagraph() {
  if (!paragraphBuffer.length) return;
  children.push(bodyParagraph(paragraphBuffer.join(" ").trim()));
  paragraphBuffer = [];
}

for (let i = 0; i < lines.length; i += 1) {
  const line = lines[i];
  const trimmed = line.trim();

  if (trimmed === "```text") {
    flushParagraph();
    inCode = true;
    continue;
  }
  if (trimmed === "```" && inCode) {
    inCode = false;
    children.push(new Paragraph({ spacing: { after: 160 }, children: [] }));
    continue;
  }
  if (inCode) {
    children.push(codeParagraph(line));
    continue;
  }
  if (!trimmed) {
    flushParagraph();
    continue;
  }
  if (trimmed.startsWith("|")) {
    flushParagraph();
    const parsed = parseTable(lines, i);
    children.push(parsed.table);
    children.push(new Paragraph({ spacing: { after: 160 }, children: [] }));
    i = parsed.next - 1;
    continue;
  }
  if (/^\[Insert Figure \d+ here:/.test(trimmed)) {
    flushParagraph();
    children.push(figurePlaceholder(trimmed));
    continue;
  }
  if (trimmed.startsWith("# ")) {
    flushParagraph();
    const headingText = trimmed.slice(2);
    const isChapter = headingText === "CHAPTER IV";
    children.push(new Paragraph({
      heading: HeadingLevel.HEADING_1,
      alignment: AlignmentType.CENTER,
      pageBreakBefore: isChapter,
      spacing: { before: isChapter ? 0 : 320, after: 240 },
      children: [new TextRun({ text: headingText, bold: true, font: "Times New Roman", size: 28 })],
    }));
    continue;
  }
  if (trimmed.startsWith("## ")) {
    flushParagraph();
    children.push(new Paragraph({
      heading: HeadingLevel.HEADING_2,
      alignment: AlignmentType.LEFT,
      keepNext: true,
      spacing: { before: 260, after: 160 },
      children: [new TextRun({ text: trimmed.slice(3), bold: true, font: "Times New Roman", size: 26 })],
    }));
    continue;
  }
  if (/^\*\*(Table|Pseudocode|Figure) \d+\./.test(trimmed)) {
    flushParagraph();
    children.push(new Paragraph({
      alignment: AlignmentType.CENTER,
      keepNext: trimmed.includes("Table") || trimmed.includes("Pseudocode"),
      spacing: { before: 120, after: 100 },
      children: inlineRuns(trimmed, { size: 22 }),
    }));
    continue;
  }
  paragraphBuffer.push(trimmed);
}
flushParagraph();

const doc = new Document({
  creator: "OpenAI Codex",
  title: "Chapter IV - Results and Discussions - EduAttend",
  description: "Chapter IV based on the implemented EduAttend School Attendance Monitoring System.",
  styles: {
    default: {
      document: {
        run: { font: "Times New Roman", size: 24, color: "000000" },
        paragraph: { spacing: { line: 360 } },
      },
    },
    paragraphStyles: [
      {
        id: "Heading1",
        name: "Heading 1",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { font: "Times New Roman", size: 28, bold: true, color: "000000" },
        paragraph: { outlineLevel: 0, keepNext: true },
      },
      {
        id: "Heading2",
        name: "Heading 2",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { font: "Times New Roman", size: 26, bold: true, color: "000000" },
        paragraph: { outlineLevel: 1, keepNext: true },
      },
    ],
  },
  sections: [{
    properties: {
      page: {
        size: { width: PAGE_WIDTH, height: PAGE_HEIGHT },
        margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 },
      },
    },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ children: [PageNumber.CURRENT], font: "Times New Roman", size: 20 })],
        })],
      }),
    },
    children,
  }],
});

Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(outputPath, buffer);
  console.log(outputPath);
});
