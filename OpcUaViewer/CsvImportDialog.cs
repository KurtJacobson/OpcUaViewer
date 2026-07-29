using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OpcUaViewer
{
    internal sealed class CsvImportDialog : Form
    {
        // ── colours ───────────────────────────────────────────────────────────
        private static readonly Color BgColor     = Color.FromArgb(52, 52, 52);
        private static readonly Color TitleBg     = Color.FromArgb(36, 36, 36);
        private static readonly Color ButtonBarBg = Color.FromArgb(42, 42, 42);
        private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);
        private static readonly Color TextColor   = Color.FromArgb(230, 230, 230);
        private static readonly Color SubText     = Color.FromArgb(160, 160, 160);
        private static readonly Color InputBg     = Color.FromArgb(40, 40, 40);
        private static readonly Color AccentColor = Color.FromArgb(255, 140, 0);
        private static readonly Color ButtonGray  = Color.FromArgb(70, 70, 70);

        // layout
        private const int W      = 720;
        private const int HPad   = 20;
        private const int LabelW = 158;
        private const int ColX   = HPad + LabelW;
        private const int ColW   = 175;
        private const int RegexX = ColX + ColW + 10;
        private const int RegexW = W - RegexX - HPad;
        private const int RowH   = 36;

        // ── public results ────────────────────────────────────────────────────
        public string GroupName           { get; private set; } = "";
        public string CsvFilePath         { get; private set; } = "";
        public string ColPartName         { get; private set; } = "";
        public string ColPartNameRegex    { get; private set; } = "";
        public string ColTemplateFile     { get; private set; } = "";
        public string ColTemplateRegex    { get; private set; } = "";
        public string ColLength           { get; private set; } = "";
        public string ColLengthRegex      { get; private set; } = "";
        public string ColWidth            { get; private set; } = "";
        public string ColWidthRegex       { get; private set; } = "";
        public string ColQty              { get; private set; } = "";
        public string ColQtyRegex         { get; private set; } = "";
        public string ColMaterial         { get; private set; } = "";
        public string ColMaterialRegex    { get; private set; } = "";
        public string ColThickness        { get; private set; } = "";
        public string ColThicknessRegex   { get; private set; } = "";

        // ── controls ──────────────────────────────────────────────────────────
        private readonly TextBox _csvPathBox;
        private readonly TextBox _colPartNameBox,     _colPartNameRegexBox;
        private readonly TextBox _colTemplateFileBox, _colTemplateRegexBox;
        private readonly TextBox _colLengthBox,       _colLengthRegexBox;
        private readonly TextBox _colWidthBox,        _colWidthRegexBox;
        private readonly TextBox _colQtyBox,          _colQtyRegexBox;
        private readonly TextBox _colMaterialBox,     _colMaterialRegexBox;
        private readonly TextBox _colThicknessBox,    _colThicknessRegexBox;
        private readonly TextBox _groupNameBox;

        public CsvImportDialog()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BgColor;
            Font            = new Font("Segoe UI", 11F);
            ShowInTaskbar   = false;
            KeyPreview      = true;
            KeyDown        += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };
            Paint += (_, e) =>
            {
                using var pen = new Pen(BorderColor, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
            };

            var s = AppSettings.Current;

            // ── title bar ─────────────────────────────────────────────────────
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = TitleBg };
            var titleLbl = new Label
            {
                Text      = "Import CSV",
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(210, 210, 210),
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            };
            titleBar.Controls.Add(titleLbl);
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLbl.MouseDown += TitleBar_MouseDown;

            var divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = BorderColor };

            // ── body ──────────────────────────────────────────────────────────
            var body = new Panel { BackColor = BgColor, Location = new Point(0, 45), Size = new Size(W, 1000) };
            int y = HPad;

            // CSV file row
            AddLabel(body, "CSV File:", HPad, y, LabelW);
            _csvPathBox = AddInput(body, ColX, y, ColW + 10 + RegexW - 94, RowH);
            _csvPathBox.ReadOnly = true;
            var browseBtn = DarkButton("Browse...", ButtonGray, W - HPad - 86, y, 86, RowH);
            browseBtn.Click += (_, _) =>
            {
                using var dlg = new OpenFileDialog
                {
                    Title  = "Select CSV file",
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
                };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _csvPathBox.Text = dlg.FileName;
            };
            body.Controls.Add(browseBtn);
            y += RowH + 12;

            // Section: Column Mapping
            AddSectionLabel(body, "Column Mapping", HPad, y);
            AddSubHeader(body, "Column Name", ColX, y);
            AddSubHeader(body, "RegEx — optional, capture group extracts a substring", RegexX, y);
            y += 24;

            var tip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 300, ShowAlways = true };

            // helper to add a mapped row
            (TextBox col, TextBox rx) AddMappedRow(string label, string colVal, string rxVal,
                string colTip, string rxTip)
            {
                AddLabel(body, label, HPad, y, LabelW);
                var colBox = AddInput(body, ColX,   y, ColW,   RowH, colVal);
                var rxBox  = AddInput(body, RegexX, y, RegexW, RowH, rxVal);
                if (!string.IsNullOrEmpty(colTip)) tip.SetToolTip(colBox, colTip);
                if (!string.IsNullOrEmpty(rxTip))  tip.SetToolTip(rxBox,  rxTip);
                y += RowH + 8;
                return (colBox, rxBox);
            }

            (_colPartNameBox,     _colPartNameRegexBox)  = AddMappedRow("Part Name:",     s.CsvPartNameColumn,     s.CsvPartNameRegex,
                "",
                "Applied to the Part Name column value.\nResult becomes the List ID.\nExample: ^([A-Z]+-\\d+)");

            (_colTemplateFileBox, _colTemplateRegexBox)  = AddMappedRow("Template Name:", s.CsvTemplateFileColumn, s.CsvTemplateFileRegex,
                "Column that identifies the template/product file.\nLeave blank to use Part Name as the Product ID.",
                "Applied to the Template column value.\nResult is used to look up the product zip.\nExample: ([^\\\\/]+)(?:\\.zip)?$");

            (_colQtyBox,          _colQtyRegexBox)        = AddMappedRow("Quantity:",      s.CsvQtyColumn,          s.CsvQtyRegex,
                "",
                "Applied to the Quantity column value to extract a number.\nExample: (\\d+)");

            (_colMaterialBox,     _colMaterialRegexBox)   = AddMappedRow("Material:",      s.CsvMaterialColumn,     s.CsvMaterialRegex,
                "",
                "Applied to the Material column value.\nExample: ^(\\S+)  takes the first word.");

            (_colThicknessBox,    _colThicknessRegexBox)  = AddMappedRow("Thickness:",     s.CsvThicknessColumn,    s.CsvThicknessRegex,
                "",
                "Applied to the Thickness column value to extract a number (inches).\nExample: (\\d+\\.?\\d*)");

            (_colLengthBox,       _colLengthRegexBox)     = AddMappedRow("Length:",        s.CsvLengthColumn,       s.CsvLengthRegex,
                "",
                "Applied to the Length column value to extract a number (inches).\nExample: L=(\\d+\\.?\\d*)");

            (_colWidthBox,        _colWidthRegexBox)      = AddMappedRow("Width:",         s.CsvWidthColumn,        s.CsvWidthRegex,
                "",
                "Applied to the Width column value to extract a number (inches).\nExample: W=(\\d+\\.?\\d*)");

            y += 6;

            // Section: Group
            AddSectionLabel(body, "Group", HPad, y);
            y += 24;
            AddLabel(body, "Name:", HPad, y, LabelW);
            _groupNameBox = AddInput(body, ColX, y, ColW, RowH);
            y += RowH + HPad;

            body.Size = new Size(W, y);

            // ── button bar ────────────────────────────────────────────────────
            const int barH = 68;
            var buttonBar = new Panel
            {
                BackColor = ButtonBarBg,
                Location  = new Point(0, 44 + 1 + y),
                Size      = new Size(W, barH)
            };
            buttonBar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = BorderColor });

            var cancelBtn = DarkButton("Cancel", ButtonGray, W - HPad - 150, (barH - 44) / 2 + 1, 150, 44);
            cancelBtn.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            buttonBar.Controls.Add(cancelBtn);

            var importBtn = DarkButton("Import", AccentColor, W - HPad - 310, (barH - 44) / 2 + 1, 150, 44);
            importBtn.Font   = new Font("Segoe UI", 11F, FontStyle.Bold);
            importBtn.Click += ImportBtn_Click;
            AcceptButton     = importBtn;
            buttonBar.Controls.Add(importBtn);

            ClientSize = new Size(W, 44 + 1 + y + barH);
            Controls.AddRange(new Control[] { titleBar, divider, body, buttonBar });
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_csvPathBox.Text) || !File.Exists(_csvPathBox.Text))
            {
                DarkMessageBox.Show(this, "Please select a CSV file.", "Import CSV"); return;
            }
            if (string.IsNullOrWhiteSpace(_colPartNameBox.Text))
            {
                DarkMessageBox.Show(this, "Part Name column is required.", "Import CSV"); return;
            }
            if (string.IsNullOrWhiteSpace(_groupNameBox.Text))
            {
                DarkMessageBox.Show(this, "Please enter a group name.", "Import CSV"); return;
            }

            // Validate all regex fields
            var regexFields = new[]
            {
                (_colPartNameRegexBox,   "Part Name RegEx"),
                (_colTemplateRegexBox,   "Template RegEx"),
                (_colQtyRegexBox,        "Quantity RegEx"),
                (_colMaterialRegexBox,   "Material RegEx"),
                (_colThicknessRegexBox,  "Thickness RegEx"),
                (_colLengthRegexBox,     "Length RegEx"),
                (_colWidthRegexBox,      "Width RegEx"),
            };
            foreach (var (box, label) in regexFields)
            {
                string pat = box.Text.Trim();
                if (string.IsNullOrEmpty(pat)) continue;
                try { _ = new Regex(pat); }
                catch (ArgumentException ex)
                {
                    DarkMessageBox.Show(this, $"Invalid {label}:\n\n{ex.Message}", "Import CSV",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Persist
            var s = AppSettings.Current;
            s.CsvPartNameColumn     = _colPartNameBox.Text.Trim();
            s.CsvPartNameRegex      = _colPartNameRegexBox.Text.Trim();
            s.CsvTemplateFileColumn = _colTemplateFileBox.Text.Trim();
            s.CsvTemplateFileRegex  = _colTemplateRegexBox.Text.Trim();
            s.CsvQtyColumn          = _colQtyBox.Text.Trim();
            s.CsvQtyRegex           = _colQtyRegexBox.Text.Trim();
            s.CsvMaterialColumn     = _colMaterialBox.Text.Trim();
            s.CsvMaterialRegex      = _colMaterialRegexBox.Text.Trim();
            s.CsvThicknessColumn    = _colThicknessBox.Text.Trim();
            s.CsvThicknessRegex     = _colThicknessRegexBox.Text.Trim();
            s.CsvLengthColumn       = _colLengthBox.Text.Trim();
            s.CsvLengthRegex        = _colLengthRegexBox.Text.Trim();
            s.CsvWidthColumn        = _colWidthBox.Text.Trim();
            s.CsvWidthRegex         = _colWidthRegexBox.Text.Trim();
            AppSettings.Save();

            GroupName          = _groupNameBox.Text.Trim();
            CsvFilePath        = _csvPathBox.Text.Trim();
            ColPartName        = s.CsvPartNameColumn;
            ColPartNameRegex   = s.CsvPartNameRegex;
            ColTemplateFile    = s.CsvTemplateFileColumn;
            ColTemplateRegex   = s.CsvTemplateFileRegex;
            ColQty             = s.CsvQtyColumn;
            ColQtyRegex        = s.CsvQtyRegex;
            ColMaterial        = s.CsvMaterialColumn;
            ColMaterialRegex   = s.CsvMaterialRegex;
            ColThickness       = s.CsvThicknessColumn;
            ColThicknessRegex  = s.CsvThicknessRegex;
            ColLength          = s.CsvLengthColumn;
            ColLengthRegex     = s.CsvLengthRegex;
            ColWidth           = s.CsvWidthColumn;
            ColWidthRegex      = s.CsvWidthRegex;

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── CSV parsing ───────────────────────────────────────────────────────

        public record CsvRow(
            string ListId, string ProductIdRaw,
            string Qty, string Material, string Thickness,
            string Length, string Width);

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
            var rows    = new List<CsvRow>();
            var lines   = File.ReadAllLines(path);
            if (lines.Length < 2) return rows;

            var headers  = SplitCsvLine(lines[0]);
            int idxPart  = FindCol(headers, colPartName);
            int idxTmpl  = FindCol(headers, colTemplate);
            int idxQty   = FindCol(headers, colQty);
            int idxMat   = FindCol(headers, colMaterial);
            int idxThk   = FindCol(headers, colThickness);
            int idxLen   = FindCol(headers, colLength);
            int idxWid   = FindCol(headers, colWidth);

            if (idxPart < 0) return rows;

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = SplitCsvLine(lines[i]);
                if (cols.Count == 0) continue;

                string rawPart = Get(cols, idxPart);
                if (string.IsNullOrWhiteSpace(rawPart)) continue;

                rows.Add(new CsvRow(
                    ListId:       ApplyRegex(rawPart,           partNameRegex),
                    ProductIdRaw: ApplyRegex(Get(cols, idxTmpl), templateRegex),
                    Qty:          ApplyRegex(Get(cols, idxQty),  qtyRegex),
                    Material:     ApplyRegex(Get(cols, idxMat),  materialRegex),
                    Thickness:    ApplyRegex(Get(cols, idxThk),  thicknessRegex),
                    Length:       ApplyRegex(Get(cols, idxLen),  lengthRegex),
                    Width:        ApplyRegex(Get(cols, idxWid),  widthRegex)));
            }
            return rows;
        }

        // Empty pattern → return value as-is. Pattern with capture group → group 1. No match → "".
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

        private static List<string> SplitCsvLine(string line)
        {
            var result    = new List<string>();
            bool inQuotes = false;
            var sb        = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '"')                   inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes) { result.Add(sb.ToString()); sb.Clear(); }
                else                            sb.Append(c);
            }
            result.Add(sb.ToString());
            return result;
        }

        // ── drag ─────────────────────────────────────────────────────────────

        private Point _dragStart;
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragStart = e.Location;
            ((Control)sender).MouseMove += TitleBar_MouseMove;
            ((Control)sender).MouseUp   += TitleBar_MouseUp;
        }
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
        }
        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            ((Control)sender).MouseMove -= TitleBar_MouseMove;
            ((Control)sender).MouseUp   -= TitleBar_MouseUp;
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private void AddLabel(Panel parent, string text, int x, int y, int w)
            => parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 8), Size = new Size(w, 24),
                ForeColor = SubText, Font = new Font("Segoe UI", 10F), BackColor = Color.Transparent
            });

        private void AddSectionLabel(Panel parent, string text, int x, int y)
            => parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y), Size = new Size(400, 22),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), BackColor = Color.Transparent
            });

        private void AddSubHeader(Panel parent, string text, int x, int y)
            => parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 4), Size = new Size(RegexW + ColW, 18),
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), BackColor = Color.Transparent
            });

        private TextBox AddInput(Panel parent, int x, int y, int w, int h, string text = "")
        {
            var tb = new TextBox
            {
                Text = text, Location = new Point(x, y + 2), Size = new Size(w, h),
                BackColor = InputBg, ForeColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11F)
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private static Button DarkButton(string text, Color bg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text, BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F),
                UseVisualStyleBackColor = false, Location = new Point(x, y), Size = new Size(w, h)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
