using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace AttendanceUI.Services;

/// <summary>
/// One worksheet to write: its tab name, header row, data rows, and which columns hold numbers.
/// <paramref name="TitleRows"/> are optional lines written above the header (e.g. a report title).
/// </summary>
public sealed record XlsxSheet(
    string Name,
    IReadOnlyList<string> Headers,
    IReadOnlyList<string?[]> Rows,
    IReadOnlySet<int>? NumericColumns = null,
    IReadOnlyList<string?[]>? TitleRows = null)
{
    /// <summary>Row number (1-based) of the header row.</summary>
    internal int HeaderRow => (TitleRows?.Count ?? 0) + 1;
}

/// <summary>
/// Writes a minimal Office Open XML (.xlsx) workbook without a third-party library. Each sheet
/// gets a bold, shaded header row that stays frozen while scrolling, an auto-filter on the
/// header, and column widths sized to the content. Cells are written as inline strings, so a
/// value can never be evaluated as a formula; only columns listed in
/// <see cref="XlsxSheet.NumericColumns"/> are stored as numbers (LRNs stay text, so Excel
/// does not turn them into 1.23E+11).
/// </summary>
public static class XlsxWriter
{
    private const string MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string PkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

    // Width is estimated from the first rows only; long exports don't need a full scan.
    private const int WidthSampleRows = 500;

    public static byte[] Write(IReadOnlyList<XlsxSheet> sheets)
    {
        if (sheets.Count == 0)
            throw new ArgumentException("A workbook needs at least one sheet.", nameof(sheets));

        var names = UniqueSheetNames(sheets.Select(s => s.Name));

        using var buffer = new MemoryStream();
        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "[Content_Types].xml", w => WriteContentTypes(w, sheets.Count));
            WriteEntry(zip, "_rels/.rels", WriteRootRels);
            WriteEntry(zip, "xl/workbook.xml", w => WriteWorkbook(w, sheets, names));
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", w => WriteWorkbookRels(w, sheets.Count));
            WriteEntry(zip, "xl/styles.xml", WriteStyles);

            for (var i = 0; i < sheets.Count; i++)
            {
                var sheet = sheets[i];
                WriteEntry(zip, $"xl/worksheets/sheet{i + 1}.xml", w => WriteSheet(w, sheet));
            }
        }

        return buffer.ToArray();
    }

    private static void WriteEntry(ZipArchive zip, string path, Action<XmlWriter> write)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false
        });

        writer.WriteStartDocument(standalone: true);
        write(writer);
        writer.WriteEndDocument();
    }

    private static void WriteContentTypes(XmlWriter w, int sheetCount)
    {
        const string ns = "http://schemas.openxmlformats.org/package/2006/content-types";
        w.WriteStartElement("Types", ns);

        WriteDefault("rels", "application/vnd.openxmlformats-package.relationships+xml");
        WriteDefault("xml", "application/xml");
        WriteOverride("/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        WriteOverride("/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
        for (var i = 1; i <= sheetCount; i++)
            WriteOverride($"/xl/worksheets/sheet{i}.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");

        w.WriteEndElement();

        void WriteDefault(string extension, string contentType)
        {
            w.WriteStartElement("Default", ns);
            w.WriteAttributeString("Extension", extension);
            w.WriteAttributeString("ContentType", contentType);
            w.WriteEndElement();
        }

        void WriteOverride(string part, string contentType)
        {
            w.WriteStartElement("Override", ns);
            w.WriteAttributeString("PartName", part);
            w.WriteAttributeString("ContentType", contentType);
            w.WriteEndElement();
        }
    }

    private static void WriteRootRels(XmlWriter w)
    {
        w.WriteStartElement("Relationships", PkgRelNs);
        WriteRelationship(w, "rId1", $"{RelNs}/officeDocument", "xl/workbook.xml");
        w.WriteEndElement();
    }

    private static void WriteWorkbook(XmlWriter w, IReadOnlyList<XlsxSheet> sheets, IReadOnlyList<string> names)
    {
        w.WriteStartElement("workbook", MainNs);
        w.WriteAttributeString("xmlns", "r", null, RelNs);

        w.WriteStartElement("sheets", MainNs);
        for (var i = 0; i < sheets.Count; i++)
        {
            w.WriteStartElement("sheet", MainNs);
            w.WriteAttributeString("name", names[i]);
            w.WriteAttributeString("sheetId", (i + 1).ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("id", RelNs, $"rId{i + 1}");
            w.WriteEndElement();
        }
        w.WriteEndElement();

        // Excel records each sheet's auto-filter range as a hidden defined name.
        w.WriteStartElement("definedNames", MainNs);
        for (var i = 0; i < sheets.Count; i++)
        {
            w.WriteStartElement("definedName", MainNs);
            w.WriteAttributeString("name", "_xlnm._FilterDatabase");
            w.WriteAttributeString("localSheetId", i.ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("hidden", "1");
            w.WriteString($"'{names[i].Replace("'", "''")}'!{AbsoluteRange(sheets[i])}");
            w.WriteEndElement();
        }
        w.WriteEndElement();

        w.WriteEndElement();
    }

    private static void WriteWorkbookRels(XmlWriter w, int sheetCount)
    {
        w.WriteStartElement("Relationships", PkgRelNs);
        for (var i = 1; i <= sheetCount; i++)
            WriteRelationship(w, $"rId{i}", $"{RelNs}/worksheet", $"worksheets/sheet{i}.xml");
        WriteRelationship(w, $"rId{sheetCount + 1}", $"{RelNs}/styles", "styles.xml");
        w.WriteEndElement();
    }

    private static void WriteRelationship(XmlWriter w, string id, string type, string target)
    {
        w.WriteStartElement("Relationship", PkgRelNs);
        w.WriteAttributeString("Id", id);
        w.WriteAttributeString("Type", type);
        w.WriteAttributeString("Target", target);
        w.WriteEndElement();
    }

    // Two cell formats: 0 = normal, 1 = header (bold on a light grey fill).
    private static void WriteStyles(XmlWriter w)
    {
        w.WriteStartElement("styleSheet", MainNs);
        w.WriteRaw(
            "<fonts count=\"2\">" +
                "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
                "<font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
            "</fonts>" +
            "<fills count=\"3\">" +
                "<fill><patternFill patternType=\"none\"/></fill>" +
                "<fill><patternFill patternType=\"gray125\"/></fill>" +
                "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFE5E7EB\"/><bgColor indexed=\"64\"/></patternFill></fill>" +
            "</fills>" +
            "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
            "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
            "<cellXfs count=\"2\">" +
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
            "</cellXfs>" +
            "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>");
        w.WriteEndElement();
    }

    private static void WriteSheet(XmlWriter w, XlsxSheet sheet)
    {
        var columnCount = Math.Max(1, sheet.Headers.Count);

        w.WriteStartElement("worksheet", MainNs);

        // Freeze the header row.
        w.WriteStartElement("sheetViews", MainNs);
        w.WriteStartElement("sheetView", MainNs);
        w.WriteAttributeString("workbookViewId", "0");
        w.WriteStartElement("pane", MainNs);
        w.WriteAttributeString("ySplit", sheet.HeaderRow.ToString(CultureInfo.InvariantCulture));
        w.WriteAttributeString("topLeftCell", "A" + (sheet.HeaderRow + 1).ToString(CultureInfo.InvariantCulture));
        w.WriteAttributeString("activePane", "bottomLeft");
        w.WriteAttributeString("state", "frozen");
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();

        w.WriteStartElement("cols", MainNs);
        for (var c = 0; c < columnCount; c++)
        {
            var index = (c + 1).ToString(CultureInfo.InvariantCulture);
            w.WriteStartElement("col", MainNs);
            w.WriteAttributeString("min", index);
            w.WriteAttributeString("max", index);
            w.WriteAttributeString("width", ColumnWidth(sheet, c).ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("customWidth", "1");
            w.WriteEndElement();
        }
        w.WriteEndElement();

        w.WriteStartElement("sheetData", MainNs);
        var titles = sheet.TitleRows ?? [];
        for (var t = 0; t < titles.Count; t++)
            WriteRow(w, t + 1, titles[t], style: 0, numeric: null);
        WriteRow(w, sheet.HeaderRow, [.. sheet.Headers], style: 1, numeric: null);
        for (var r = 0; r < sheet.Rows.Count; r++)
            WriteRow(w, sheet.HeaderRow + 1 + r, sheet.Rows[r], style: 0, numeric: sheet.NumericColumns);
        w.WriteEndElement();

        w.WriteStartElement("autoFilter", MainNs);
        w.WriteAttributeString("ref", RelativeRange(sheet));
        w.WriteEndElement();

        w.WriteEndElement();
    }

    private static void WriteRow(XmlWriter w, int rowNumber, string?[] values, int style, IReadOnlySet<int>? numeric)
    {
        var row = rowNumber.ToString(CultureInfo.InvariantCulture);
        w.WriteStartElement("row", MainNs);
        w.WriteAttributeString("r", row);

        for (var c = 0; c < values.Length; c++)
        {
            var value = values[c];
            if (string.IsNullOrEmpty(value)) continue;

            w.WriteStartElement("c", MainNs);
            w.WriteAttributeString("r", ColumnName(c) + row);
            if (style != 0)
                w.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));

            if (numeric is not null && numeric.Contains(c)
                && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                w.WriteElementString("v", MainNs, number.ToString("R", CultureInfo.InvariantCulture));
            }
            else
            {
                var text = CleanText(value);
                w.WriteAttributeString("t", "inlineStr");
                w.WriteStartElement("is", MainNs);
                w.WriteStartElement("t", MainNs);
                if (text.Length > 0 && (char.IsWhiteSpace(text[0]) || char.IsWhiteSpace(text[^1])))
                    w.WriteAttributeString("xml", "space", null, "preserve");
                w.WriteString(text);
                w.WriteEndElement();
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        w.WriteEndElement();
    }

    private static double ColumnWidth(XlsxSheet sheet, int column)
    {
        var longest = column < sheet.Headers.Count ? sheet.Headers[column].Length : 0;
        foreach (var row in sheet.Rows.Take(WidthSampleRows))
        {
            if (column < row.Length && row[column] is { } value)
                longest = Math.Max(longest, value.Length);
        }

        return Math.Clamp(longest + 2, 8, 60);
    }

    private static string RelativeRange(XlsxSheet sheet) =>
        $"A{sheet.HeaderRow}:{ColumnName(Math.Max(1, sheet.Headers.Count) - 1)}{sheet.HeaderRow + sheet.Rows.Count}";

    private static string AbsoluteRange(XlsxSheet sheet) =>
        $"$A${sheet.HeaderRow}:${ColumnName(Math.Max(1, sheet.Headers.Count) - 1)}${sheet.HeaderRow + sheet.Rows.Count}";

    /// <summary>Zero-based column index to its letter name (0 → A, 26 → AA).</summary>
    internal static string ColumnName(int index)
    {
        var name = "";
        for (var n = index + 1; n > 0; n = (n - 1) / 26)
            name = (char)('A' + (n - 1) % 26) + name;
        return name;
    }

    /// <summary>Drops characters XML 1.0 cannot carry (control characters other than tab and newlines).</summary>
    private static string CleanText(string value)
    {
        if (!value.Any(ch => ch < 0x20 && ch is not ('\t' or '\n' or '\r') || ch is '￾' or '￿'))
            return value;

        return string.Concat(value.Where(ch => !(ch < 0x20 && ch is not ('\t' or '\n' or '\r') || ch is '￾' or '￿')));
    }

    /// <summary>
    /// Makes each name a legal, unique Excel tab name: no <c>: \ / ? * [ ]</c>, no leading or
    /// trailing apostrophe, at most 31 characters, unique ignoring case.
    /// </summary>
    internal static IReadOnlyList<string> UniqueSheetNames(IEnumerable<string> requested)
    {
        const int maxLength = 31;
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var raw in requested)
        {
            var name = string.Concat(raw.Select(ch => ch is ':' or '\\' or '/' or '?' or '*' or '[' or ']' ? '-' : ch))
                .Trim().Trim('\'');
            if (name.Length == 0) name = "Sheet";
            if (name.Length > maxLength) name = name[..maxLength].TrimEnd();

            var candidate = name;
            for (var n = 2; !used.Add(candidate); n++)
            {
                var suffix = $" ({n})";
                candidate = (name.Length + suffix.Length > maxLength ? name[..(maxLength - suffix.Length)].TrimEnd() : name) + suffix;
            }

            result.Add(candidate);
        }

        return result;
    }
}
