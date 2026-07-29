using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace OpcUaViewer
{
    internal sealed class CsvImportDialog : Form
    {
        // ── colours matching the rest of the app ──────────────────────────────
        private static readonly Color BgColor     = Color.FromArgb(52, 52, 52);
        private static readonly Color TitleBg     = Color.FromArgb(36, 36, 36);
        private static readonly Color ButtonBarBg = Color.FromArgb(42, 42, 42);
        private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);
        private static readonly Color TextColor   = Color.FromArgb(230, 230, 230);
        private static readonly Color SubText     = Color.FromArgb(160, 160, 160);
        private static readonly Color InputBg     = Color.FromArgb(40, 40, 40);
        private static readonly Color AccentColor = Color.FromArgb(255, 140, 0);
        private static readonly Color ButtonGray  = Color.FromArgb(70, 70, 70);

        // ── public results ────────────────────────────────────────────────────
        public string GroupName      { get; private set; } = "";
        public string CsvFilePath    { get; private set; } = "";
        public string ColPartName    { get; private set; } = "";
        public string ColLength      { get; private set; } = "";
        public string ColWidth       { get; private set; } = "";

        // ── controls ──────────────────────────────────────────────────────────
        private readonly TextBox _csvPathBox;
        private readonly TextBox _colPartNameBox;
        private readonly TextBox _colLengthBox;
        private readonly TextBox _colWidthBox;
        private readonly TextBox _groupNameBox;

        public CsvImportDialog()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BgColor;
            Font            = new Font("Segoe UI", 11F);
            ShowInTaskbar   = false;
            KeyPreview      = true;
            KeyDown        += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };
            Paint += (s, e) =>
            {
                using var pen = new Pen(BorderColor, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
            };

            const int W = 520, hPad = 24, rowH = 36, labelW = 110, inputW = 340;

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
            int y = 56;

            var body = new Panel { BackColor = BgColor, Location = new Point(0, 0), Size = new Size(W, 1000) };

            // CSV file row
            AddLabel(body, "CSV File:", hPad, y, labelW);
            _csvPathBox = AddInput(body, hPad + labelW, y, inputW - 56, rowH);
            _csvPathBox.ReadOnly = true;
            var browseBtn = DarkButton("Browse...", ButtonGray, hPad + labelW + inputW - 52, y, 90, rowH);
            browseBtn.Click += (s, e) =>
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
            y += rowH + 8;

            // Section header
            AddSectionLabel(body, "Column Names", hPad, y);
            y += 28;

            // Column mapping rows
            AddLabel(body, "Part Name:", hPad, y, labelW);
            _colPartNameBox = AddInput(body, hPad + labelW, y, 200, rowH);
            _colPartNameBox.Text = Properties.Settings.Default.CsvPartNameColumn;
            y += rowH + 8;

            AddLabel(body, "Length:", hPad, y, labelW);
            _colLengthBox = AddInput(body, hPad + labelW, y, 200, rowH);
            _colLengthBox.Text = Properties.Settings.Default.CsvLengthColumn;
            y += rowH + 8;

            AddLabel(body, "Width:", hPad, y, labelW);
            _colWidthBox = AddInput(body, hPad + labelW, y, 200, rowH);
            _colWidthBox.Text = Properties.Settings.Default.CsvWidthColumn;
            y += rowH + 16;

            // Section header
            AddSectionLabel(body, "Group", hPad, y);
            y += 28;

            AddLabel(body, "Name:", hPad, y, labelW);
            _groupNameBox = AddInput(body, hPad + labelW, y, 200, rowH);
            y += rowH + 20;

            body.Size = new Size(W, y);

            // ── button bar ────────────────────────────────────────────────────
            const int barH = 68;
            var buttonBar = new Panel
            {
                BackColor = ButtonBarBg,
                Location  = new Point(0, 44 + 1 + y),
                Size      = new Size(W, barH)
            };
            var barDiv = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = BorderColor };
            buttonBar.Controls.Add(barDiv);

            var cancelBtn = DarkButton("Cancel", ButtonGray, W - 24 - 150, (barH - 44) / 2 + 1, 150, 44);
            cancelBtn.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            buttonBar.Controls.Add(cancelBtn);

            var importBtn = DarkButton("Import", AccentColor, W - 24 - 150 - 10 - 150, (barH - 44) / 2 + 1, 150, 44);
            importBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            importBtn.Click += ImportBtn_Click;
            AcceptButton = importBtn;
            buttonBar.Controls.Add(importBtn);

            ClientSize = new Size(W, 44 + 1 + y + barH);
            Controls.AddRange(new Control[] { titleBar, divider, body, buttonBar });
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_csvPathBox.Text) || !File.Exists(_csvPathBox.Text))
            {
                DarkMessageBox.Show(this, "Please select a CSV file.", "Import CSV");
                return;
            }
            if (string.IsNullOrWhiteSpace(_colPartNameBox.Text))
            {
                DarkMessageBox.Show(this, "Part Name column name is required.", "Import CSV");
                return;
            }
            if (string.IsNullOrWhiteSpace(_groupNameBox.Text))
            {
                DarkMessageBox.Show(this, "Please enter a group name.", "Import CSV");
                return;
            }

            // Save column settings
            Properties.Settings.Default.CsvPartNameColumn = _colPartNameBox.Text.Trim();
            Properties.Settings.Default.CsvLengthColumn   = _colLengthBox.Text.Trim();
            Properties.Settings.Default.CsvWidthColumn    = _colWidthBox.Text.Trim();
            Properties.Settings.Default.Save();

            GroupName   = _groupNameBox.Text.Trim();
            CsvFilePath = _csvPathBox.Text.Trim();
            ColPartName = _colPartNameBox.Text.Trim();
            ColLength   = _colLengthBox.Text.Trim();
            ColWidth    = _colWidthBox.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── CSV parsing ───────────────────────────────────────────────────────

        public record CsvRow(string PartName, string Length, string Width);

        public static List<CsvRow> ParseCsv(string path, string colPartName, string colLength, string colWidth)
        {
            var rows = new List<CsvRow>();
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) return rows;

            var headers = SplitCsvLine(lines[0]);
            int idxPart = FindCol(headers, colPartName);
            int idxLen  = FindCol(headers, colLength);
            int idxWid  = FindCol(headers, colWidth);

            if (idxPart < 0) return rows;

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = SplitCsvLine(lines[i]);
                if (cols.Count == 0) continue;
                string part = idxPart < cols.Count ? cols[idxPart] : "";
                if (string.IsNullOrWhiteSpace(part)) continue;
                string len = idxLen >= 0 && idxLen < cols.Count ? cols[idxLen] : "";
                string wid = idxWid >= 0 && idxWid < cols.Count ? cols[idxWid] : "";
                rows.Add(new CsvRow(part.Trim(), len.Trim(), wid.Trim()));
            }
            return rows;
        }

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
            var result = new List<string>();
            bool inQuotes = false;
            var sb = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; }
                else if (c == ',' && !inQuotes) { result.Add(sb.ToString()); sb.Clear(); }
                else { sb.Append(c); }
            }
            result.Add(sb.ToString());
            return result;
        }

        // ── drag ─────────────────────────────────────────────────────────────

        private Point _dragStart;
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragStart = e.Location;
                ((Control)sender).MouseMove += TitleBar_MouseMove;
                ((Control)sender).MouseUp   += TitleBar_MouseUp;
            }
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
        {
            parent.Controls.Add(new Label
            {
                Text      = text,
                Location  = new Point(x, y + 8),
                Size      = new Size(w, 24),
                ForeColor = SubText,
                Font      = new Font("Segoe UI", 10F),
                BackColor = Color.Transparent
            });
        }

        private void AddSectionLabel(Panel parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(300, 22),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent
            });
        }

        private TextBox AddInput(Panel parent, int x, int y, int w, int h)
        {
            var tb = new TextBox
            {
                Location    = new Point(x, y + 2),
                Size        = new Size(w, h),
                BackColor   = InputBg,
                ForeColor   = TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 11F)
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private Button DarkButton(string text, Color bg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text                    = text,
                BackColor               = bg,
                ForeColor               = Color.White,
                FlatStyle               = FlatStyle.Flat,
                Font                    = new Font("Segoe UI", 11F),
                UseVisualStyleBackColor = false,
                Location                = new Point(x, y),
                Size                    = new Size(w, h)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
