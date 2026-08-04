using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OpcUaViewer.Core.Services;

public record CsvRow(
    string ListId,
    string ProductIdRaw,
    string Qty,
    string Material,
    string Thickness,
    string Length,
    string Width);

public static class CsvService
{
    public static List<CsvRow> ParseCsv(
        string path,
        string colPartName,  string partNameRegex,
        string colTemplate,  string templateRegex,
        string colQty,       string qtyRegex,
        string colMaterial,  string materialRegex,
        string colThickness, string thicknessRegex,
        string colLength,    string lengthRegex,
        string colWidth,     string widthRegex)
    {
        var rows  = new List<CsvRow>();
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2) return rows;

        var headers = SplitLine(lines[0]);
        int idxPart = FindCol(headers, colPartName);
        int idxTmpl = FindCol(headers, colTemplate);
        int idxQty  = FindCol(headers, colQty);
        int idxMat  = FindCol(headers, colMaterial);
        int idxThk  = FindCol(headers, colThickness);
        int idxLen  = FindCol(headers, colLength);
        int idxWid  = FindCol(headers, colWidth);

        if (idxPart < 0) return rows;

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = SplitLine(lines[i]);
            if (cols.Count == 0) continue;

            string rawPart = Get(cols, idxPart);
            if (string.IsNullOrWhiteSpace(rawPart)) continue;

            rows.Add(new CsvRow(
                ListId:       ApplyRegex(rawPart,              partNameRegex),
                ProductIdRaw: ApplyRegex(Get(cols, idxTmpl),  templateRegex),
                Qty:          ApplyRegex(Get(cols, idxQty),   qtyRegex),
                Material:     ApplyRegex(Get(cols, idxMat),   materialRegex),
                Thickness:    ApplyRegex(Get(cols, idxThk),   thicknessRegex),
                Length:       ApplyRegex(Get(cols, idxLen),   lengthRegex),
                Width:        ApplyRegex(Get(cols, idxWid),   widthRegex)));
        }
        return rows;
    }

    /// <summary>
    /// Empty pattern → value as-is. Pattern with capture group → group 1. No match → "".
    /// </summary>
    public static string ApplyRegex(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return value.Trim();
        if (string.IsNullOrWhiteSpace(value))   return "";
        var m = Regex.Match(value, pattern);
        if (!m.Success) return "";
        return m.Groups.Count > 1 ? m.Groups[1].Value : m.Value;
    }

    private static string Get(List<string> cols, int idx)
        => idx >= 0 && idx < cols.Count ? cols[idx].Trim() : "";

    private static int FindCol(List<string> headers, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return -1;
        for (int i = 0; i < headers.Count; i++)
            if (string.Equals(headers[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static List<string> SplitLine(string line)
    {
        var result    = new List<string>();
        bool inQuotes = false;
        var sb        = new StringBuilder();
        foreach (char c in line)
        {
            if      (c == '"')             inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes){ result.Add(sb.ToString()); sb.Clear(); }
            else                           sb.Append(c);
        }
        result.Add(sb.ToString());
        return result;
    }
}
