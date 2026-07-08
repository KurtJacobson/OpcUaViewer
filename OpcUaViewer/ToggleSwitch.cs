using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OpcUaViewer
{
    /// <summary>
    /// Touch-friendly toggle switch control. Draws a sliding pill with a label on the left.
    /// Drop-in replacement for CheckBox in dark industrial UI.
    /// </summary>
    internal class ToggleSwitch : Control
    {
        private static readonly Color TrackOn  = Color.FromArgb(255, 140, 0);
        private static readonly Color TrackOff = Color.FromArgb(65, 65, 65);
        private static readonly Color KnobCol  = Color.FromArgb(230, 230, 230);

        private const int PillW = 58;
        private const int PillH = 28;
        private const int KnobD = 22;

        private bool _checked;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size   = new Size(340, 44);
            Font   = new Font("Segoe UI", 11F);
            ForeColor = Color.FromArgb(200, 200, 200);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            // Pill track (left side)
            int px = 2;
            int py = (Height - PillH) / 2;
            using (var brush = new SolidBrush(_checked ? TrackOn : TrackOff))
                FillPill(g, brush, px, py, PillW, PillH);

            // Sliding knob
            int kx = _checked ? px + PillW - KnobD - 3 : px + 3;
            int ky = (Height - KnobD) / 2;
            using (var knob = new SolidBrush(KnobCol))
                g.FillEllipse(knob, kx, ky, KnobD, KnobD);

            // Label to the right of the pill
            var textRect = new Rectangle(PillW + 12, 0, Width - PillW - 14, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        // Draws a stadium/pill shape (fully rounded ends).
        private static void FillPill(Graphics g, Brush brush, int x, int y, int w, int h)
        {
            using var path = new GraphicsPath();
            // Left semicircle: start at bottom (90°), sweep 180° CCW-looking = CW in GDI+ → left end
            path.AddArc(x, y, h, h, 90, 180);
            // Right semicircle: start at top (270°), sweep 180° → right end
            path.AddArc(x + w - h, y, h, h, 270, 180);
            path.CloseFigure();
            g.FillPath(brush, path);
        }

        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnFontChanged(EventArgs e)    { base.OnFontChanged(e); Invalidate(); }
        protected override void OnTextChanged(EventArgs e)    { base.OnTextChanged(e); Invalidate(); }
    }
}
