using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpcUaViewer
{
    /// <summary>
    /// Borderless on-screen keyboard dialog for touchscreen input.
    /// Opens with existing text fully selected — typing immediately overwrites it.
    /// Tap anywhere in the preview to place the cursor and edit in-place.
    /// Keyboard buttons never steal focus, so the caret stays visible throughout.
    /// </summary>
    internal sealed class TouchKeyboard : Form
    {
        // ── colours ────────────────────────────────────────────────────────────
        private static readonly Color BgColor     = Color.FromArgb(38, 38, 38);
        private static readonly Color KeyColor    = Color.FromArgb(62, 62, 62);
        private static readonly Color SpecialKey  = Color.FromArgb(50, 50, 50);
        private static readonly Color ShiftColor  = Color.FromArgb(255, 140, 0);
        private static readonly Color EnterColor  = Color.FromArgb(34, 139, 34);
        private static readonly Color CancelColor = Color.FromArgb(180, 50, 50);
        private static readonly Color BkspColor   = Color.FromArgb(100, 50, 50);
        private static readonly Color KeyFg       = Color.FromArgb(230, 230, 230);
        private static readonly Color PreviewBg   = Color.FromArgb(22, 22, 22);
        private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);

        // ── state ──────────────────────────────────────────────────────────────
        private readonly TextBox _preview;
        private Button   _shiftBtn;
        private bool     _shift;

        public string Result { get; private set; }

        // ── public API ─────────────────────────────────────────────────────────
        public static string Show(IWin32Window owner, string initialText, bool numericOnly = false)
        {
            using var kb = new TouchKeyboard(initialText ?? "", numericOnly);
            return kb.ShowDialog(owner) == DialogResult.OK ? kb.Result : null;
        }

        // ── construction ───────────────────────────────────────────────────────
        private TouchKeyboard(string initialText, bool numericOnly)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = BgColor;
            ShowInTaskbar   = false;
            // Do NOT set TopMost — owner relationship keeps keyboard above Form1,
            // and without TopMost modal dialogs can appear in front normally.

            KeyPreview = true;
            KeyDown   += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };
            Deactivate += (s, e) => { if (Visible) { DialogResult = DialogResult.Cancel; Close(); } };

            Paint += (s, e) =>
            {
                using var pen = new Pen(BorderColor, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
            };

            // Preview TextBox — ReadOnly prevents physical keyboard from typing but
            // still shows the caret; we manipulate Text/SelectionStart programmatically.
            _preview = new TextBox
            {
                BackColor   = PreviewBg,
                ForeColor   = Color.FromArgb(235, 235, 235),
                Font        = new Font("Segoe UI", 15F),
                BorderStyle = BorderStyle.None,
                Text        = initialText,
                ReadOnly    = true,
                TabStop     = false,
                Multiline   = false,
                TextAlign   = HorizontalAlignment.Left
            };

            if (numericOnly)
                BuildNumpad();
            else
                BuildQwerty();

            // Select all so the first key press replaces the existing value.
            // Focus the preview in Shown so the caret is visible from the start.
            Shown += (s, e) =>
            {
                _preview.Focus();
                _preview.SelectionStart  = 0;
                _preview.SelectionLength = _preview.Text.Length;
            };
        }

        // ── QWERTY layout ──────────────────────────────────────────────────────
        private void BuildQwerty()
        {
            const int kW = 64, kH = 58, gap = 5, padX = 12, padY = 12;

            // 840px row width gives 75px Space with ← → arrows in the bottom row
            const int numRowW = 840;
            const int formW   = numRowW + padX * 2;   // 864

            var previewPanel = new Panel
            {
                Location  = new Point(padX, padY),
                Size      = new Size(numRowW, 52),
                BackColor = PreviewBg,
                Padding   = new Padding(8, 8, 8, 8)
            };
            _preview.Dock = DockStyle.Fill;
            previewPanel.Controls.Add(_preview);
            Controls.Add(previewPanel);

            int y = padY + 52 + 8;

            // ── row 1: numbers ─────────────────────────────────────────────────
            int bkspW = numRowW - (10 * (kW + gap) - gap) - gap;  // 150
            int x = padX;
            string[] digits = { "1","2","3","4","5","6","7","8","9","0" };
            foreach (var d in digits) { MakeKey(d, x, y, kW, kH, KeyColor); x += kW + gap; }
            MakeKey("⌫", x, y, bkspW, kH, BkspColor, 16F);
            y += kH + gap;

            // ── row 2: QWERTY ──────────────────────────────────────────────────
            string[] r2 = { "Q","W","E","R","T","Y","U","I","O","P" };
            int r2w = r2.Length * (kW + gap) - gap;
            x = padX + (numRowW - r2w) / 2;
            foreach (var k in r2) { MakeKey(k, x, y, kW, kH, KeyColor); x += kW + gap; }
            y += kH + gap;

            // ── row 3: ASDF ────────────────────────────────────────────────────
            string[] r3 = { "A","S","D","F","G","H","J","K","L" };
            int r3w = r3.Length * (kW + gap) - gap;
            x = padX + (numRowW - r3w) / 2;
            foreach (var k in r3) { MakeKey(k, x, y, kW, kH, KeyColor); x += kW + gap; }
            y += kH + gap;

            // ── row 4: ZXCV ────────────────────────────────────────────────────
            string[] r4 = { "Z","X","C","V","B","N","M" };
            int r4w = r4.Length * (kW + gap) - gap;
            x = padX + (numRowW - r4w) / 2;
            foreach (var k in r4) { MakeKey(k, x, y, kW, kH, KeyColor); x += kW + gap; }
            y += kH + gap;

            // ── row 5: ⇧  .  :  /  -  ◄  ►  Space  Cancel  Enter ─────────────
            // 840 = 96+5+271+5+64+5+64+5+75+5+120+5+120
            const int shiftW = 96, arrowW = 64, cancelW = 120, enterW = 120;
            string[] specials = { ".", ":", "/", "-" };
            int specialsW = specials.Length * (kW + gap) - gap;   // 271
            int spaceW    = numRowW - shiftW  - gap
                          - specialsW         - gap
                          - arrowW - gap - arrowW - gap
                          - cancelW - gap - enterW;               // 75

            x = padX;
            _shiftBtn = MakeKey("⇧", x, y, shiftW, kH, ShiftColor, 16F);
            x += shiftW + gap;
            foreach (var s in specials) { MakeKey(s, x, y, kW, kH, SpecialKey); x += kW + gap; }
            MakeKey("◄", x, y, arrowW, kH, SpecialKey, 13F); x += arrowW + gap;
            MakeKey("►", x, y, arrowW, kH, SpecialKey, 13F); x += arrowW + gap;
            MakeKey("Space",  x, y, spaceW,  kH, SpecialKey,  10F); x += spaceW  + gap;
            MakeKey("Cancel", x, y, cancelW, kH, CancelColor, 11F); x += cancelW + gap;
            MakeKey("Enter",  x, y, enterW,  kH, EnterColor,  11F);
            y += kH + padY;

            ClientSize = new Size(formW, y);
        }

        // ── numeric pad layout ─────────────────────────────────────────────────
        private void BuildNumpad()
        {
            const int kW = 100, kH = 75, gap = 6, padX = 14, padY = 12;
            int gridW = 3 * kW + 2 * gap;   // 312
            int formW = gridW + padX * 2;    // 340

            var previewPanel = new Panel
            {
                Location  = new Point(padX, padY),
                Size      = new Size(gridW, 52),
                BackColor = PreviewBg,
                Padding   = new Padding(8, 8, 8, 8)
            };
            _preview.Dock = DockStyle.Fill;
            previewPanel.Controls.Add(_preview);
            Controls.Add(previewPanel);

            int y = padY + 52 + 8;

            string[][] rows = { new[]{"7","8","9"}, new[]{"4","5","6"}, new[]{"1","2","3"} };
            foreach (var row in rows)
            {
                int x = padX;
                foreach (var k in row) { MakeKey(k, x, y, kW, kH, KeyColor); x += kW + gap; }
                y += kH + gap;
            }

            // ⌫  0  .
            {
                int x = padX;
                MakeKey("⌫", x, y, kW, kH, BkspColor, 16F); x += kW + gap;
                MakeKey("0", x, y, kW, kH, KeyColor);        x += kW + gap;
                MakeKey(".", x, y, kW, kH, SpecialKey);
                y += kH + gap;
            }

            // Cancel (left) / Enter (right)
            {
                int half = (gridW - gap) / 2;
                MakeKey("Cancel", padX,          y, half, kH, CancelColor, 11F);
                MakeKey("Enter",  padX+half+gap, y, half, kH, EnterColor,  11F);
                y += kH + padY;
            }

            ClientSize = new Size(formW, y);
        }

        // ── key factory ────────────────────────────────────────────────────────
        // Uses NoFocusButton so clicking a key never moves focus away from _preview.
        private Button MakeKey(string label, int x, int y, int w, int h,
            Color bg, float fontSize = 14F)
        {
            var btn = new NoFocusButton
            {
                Text                    = label,
                Tag                     = label,
                Location                = new Point(x, y),
                Size                    = new Size(w, h),
                BackColor               = bg,
                ForeColor               = KeyFg,
                FlatStyle               = FlatStyle.Flat,
                Font                    = new Font("Segoe UI", fontSize, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize          = 0;
            btn.FlatAppearance.MouseOverBackColor  = Lighten(bg, 18);
            btn.FlatAppearance.MouseDownBackColor  = Lighten(bg, 35);
            btn.Click += Key_Click;
            Controls.Add(btn);
            return btn;
        }

        // ── key handler ────────────────────────────────────────────────────────
        private void Key_Click(object sender, EventArgs e)
        {
            string key = ((Button)sender).Tag as string ?? "";

            switch (key)
            {
                case "Enter":
                    Result = _preview.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                    return;

                case "Cancel":
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;

                case "⌫":
                {
                    int start = _preview.SelectionStart;
                    int len   = _preview.SelectionLength;
                    if (len > 0)
                    {
                        // Delete selected region
                        _preview.Text           = _preview.Text[..start] + _preview.Text[(start + len)..];
                        _preview.SelectionStart = start;
                    }
                    else if (start > 0)
                    {
                        _preview.Text           = _preview.Text[..(start - 1)] + _preview.Text[start..];
                        _preview.SelectionStart = start - 1;
                    }
                    return;
                }

                case "◄":
                {
                    int start = _preview.SelectionStart;
                    int len   = _preview.SelectionLength;
                    // If selection exists, collapse to its start; otherwise move left by 1
                    _preview.SelectionLength = 0;
                    _preview.SelectionStart  = len > 0 ? start : Math.Max(0, start - 1);
                    return;
                }

                case "►":
                {
                    int start = _preview.SelectionStart;
                    int len   = _preview.SelectionLength;
                    // If selection exists, collapse to its end; otherwise move right by 1
                    int newPos = len > 0 ? start + len : Math.Min(_preview.Text.Length, start + 1);
                    _preview.SelectionLength = 0;
                    _preview.SelectionStart  = newPos;
                    return;
                }

                case "⇧":
                    _shift = !_shift;
                    _shiftBtn.BackColor = _shift
                        ? Color.FromArgb(255, 170, 30)
                        : ShiftColor;
                    foreach (Control c in Controls)
                        if (c is Button b && b.Tag is string t && t.Length == 1 && char.IsLetter(t[0]))
                            b.Text = _shift ? t.ToUpper() : t.ToLower();
                    return;

                case "Space":
                    Append(" ");
                    return;

                default:
                    if (key.Length == 1)
                    {
                        string ch = char.IsLetter(key[0])
                            ? (_shift ? key.ToUpper() : key.ToLower())
                            : key;
                        Append(ch);
                        if (_shift && char.IsLetter(key[0]))
                        {
                            _shift = false;
                            if (_shiftBtn != null) _shiftBtn.BackColor = ShiftColor;
                            foreach (Control c in Controls)
                                if (c is Button b && b.Tag is string t && t.Length == 1 && char.IsLetter(t[0]))
                                    b.Text = t.ToLower();
                        }
                    }
                    return;
            }
        }

        // Replace any active selection, then insert at cursor.
        private void Append(string s)
        {
            int start = _preview.SelectionStart;
            int len   = _preview.SelectionLength;
            _preview.Text           = _preview.Text[..start] + s + _preview.Text[(start + len)..];
            _preview.SelectionStart = start + s.Length;
        }

        private static Color Lighten(Color c, int amt) => Color.FromArgb(
            Math.Min(255, c.R + amt),
            Math.Min(255, c.G + amt),
            Math.Min(255, c.B + amt));

        // ── NoFocusButton ──────────────────────────────────────────────────────
        // Fires Click normally but never acquires focus, so the preview TextBox
        // keeps the caret visible throughout keyboard interaction.
        private sealed class NoFocusButton : Button
        {
            public NoFocusButton()
            {
                SetStyle(ControlStyles.Selectable, false);
                TabStop = false;
            }
        }
    }
}
