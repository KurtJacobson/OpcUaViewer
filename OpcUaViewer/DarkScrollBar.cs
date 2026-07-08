using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpcUaViewer
{
    /// <summary>
    /// Custom-drawn vertical scrollbar sized for touch (36 px wide) and dark-themed.
    /// </summary>
    internal sealed class DarkScrollBar : Control
    {
        private static readonly Color TrackColor  = Color.FromArgb(28, 28, 28);
        private static readonly Color ThumbNormal = Color.FromArgb(75, 75, 75);
        private static readonly Color ThumbHover  = Color.FromArgb(110, 110, 110);
        private static readonly Color ThumbPress  = Color.FromArgb(150, 150, 150);

        // When set, the top N pixels are painted with the header background colour
        // so the scrollbar blends with the DataGridView column header row.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int   HeaderHeight    { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color HeaderBackColor { get; set; } = Color.Empty;

        private int _min, _max = 100, _value, _largeChange = 10, _smallChange = 3;
        private bool _thumbHover, _dragging;
        private int  _dragStartY, _dragStartVal;

        public event EventHandler Scroll;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Minimum
        {
            get => _min;
            set { _min = value; Clamp(); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Maximum
        {
            get => _max;
            set { _max = value; Clamp(); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set { int prev = _value; _value = value; Clamp(); if (_value != prev) Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LargeChange
        {
            get => _largeChange;
            set { _largeChange = Math.Max(1, value); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SmallChange
        {
            get => _smallChange;
            set { _smallChange = Math.Max(1, value); }
        }

        public DarkScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint  |
                     ControlStyles.ResizeRedraw, true);
            Width  = 36;
            Cursor = Cursors.Default;
        }

        // ── painting ─────────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // Header strip — matches the DataGridView column header row
            if (HeaderHeight > 0 && HeaderBackColor != Color.Empty)
            {
                using var hb = new SolidBrush(HeaderBackColor);
                g.FillRectangle(hb, 0, 0, Width, HeaderHeight);
            }

            // Track below the header
            using var trackBr = new SolidBrush(TrackColor);
            g.FillRectangle(trackBr, 0, HeaderHeight, Width, Height - HeaderHeight);

            // Thumb — only when there is something to scroll
            if (_max > _min)
            {
                var thumb = ThumbRect();
                var color = _dragging ? ThumbPress : (_thumbHover ? ThumbHover : ThumbNormal);
                using var thumbBr = new SolidBrush(color);
                g.FillRectangle(thumbBr, thumb);
            }
        }

        private Rectangle ThumbRect()
        {
            int range = _max - _min;
            if (range <= 0) return Rectangle.Empty;

            int   scrollH = Height - HeaderHeight;
            float ratio   = (float)_largeChange / (range + _largeChange);
            int   thumbH  = Math.Max(40, (int)(scrollH * ratio));
            int   trackH  = scrollH - thumbH;
            int   y       = HeaderHeight + (trackH > 0
                ? (int)((float)(_value - _min) / range * trackH)
                : 0);
            return new Rectangle(4, y, Width - 8, thumbH);
        }

        // ── mouse ─────────────────────────────────────────────────────────────────

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _max <= _min) return;
            if (e.Y < HeaderHeight) return;
            var thumb = ThumbRect();
            if (thumb.Contains(e.Location))
            {
                _dragging    = true;
                _dragStartY  = e.Y;
                _dragStartVal = _value;
                Capture = true;
            }
            else
            {
                // Click on track: page up/down
                int newVal = e.Y < thumb.Top
                    ? Math.Max(_min, _value - _largeChange)
                    : Math.Min(_max, _value + _largeChange);
                Value = newVal;
                Scroll?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                int range  = _max - _min;
                int thumbH = ThumbRect().Height;
                int trackH = Height - thumbH;
                if (trackH > 0)
                {
                    int delta = (int)((e.Y - _dragStartY) * (float)range / trackH);
                    Value = _dragStartVal + delta;
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                bool was   = _thumbHover;
                _thumbHover = ThumbRect().Contains(e.Location);
                if (_thumbHover != was) Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _dragging = false;
            Capture   = false;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _thumbHover = false;
            Invalidate();
        }

        // ── mouse wheel (forwarded from the attached control) ────────────────────

        public void HandleWheel(int delta)
        {
            int lines  = SystemInformation.MouseWheelScrollLines;
            int newVal = Value + (-delta / 120) * lines * _smallChange;
            Value      = Math.Max(_min, Math.Min(_max, newVal));
            Scroll?.Invoke(this, EventArgs.Empty);
        }

        private void Clamp() => _value = Math.Max(_min, Math.Min(_max, _value));
    }
}
