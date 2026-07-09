using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpcUaViewer
{
    internal sealed class StatsPage : UserControl
    {
        // ── palette ──────────────────────────────────────────────────────────────
        private static readonly Color BgColor       = Color.FromArgb(24, 24, 24);
        private static readonly Color CardBg         = Color.FromArgb(36, 36, 36);
        private static readonly Color CardBorder     = Color.FromArgb(55, 55, 55);
        private static readonly Color TextPrimary    = Color.FromArgb(220, 220, 220);
        private static readonly Color TextSecondary  = Color.FromArgb(120, 120, 120);
        private static readonly Color InactiveBg     = Color.FromArgb(42, 42, 42);
        private static readonly Color InactiveFg     = Color.FromArgb(70, 70, 70);
        private static readonly Color GreenBg        = Color.FromArgb(25, 90, 40);
        private static readonly Color GreenFg        = Color.FromArgb(120, 230, 140);
        private static readonly Color YellowBg       = Color.FromArgb(100, 75, 0);
        private static readonly Color YellowFg       = Color.FromArgb(255, 210, 60);
        private static readonly Color BlueBg         = Color.FromArgb(25, 70, 130);
        private static readonly Color BlueFg         = Color.FromArgb(120, 180, 255);
        private static readonly Color RedBg          = Color.FromArgb(100, 25, 25);
        private static readonly Color RedFg          = Color.FromArgb(255, 120, 120);
        private static readonly Color OrangeAccent   = Color.FromArgb(255, 140, 0);
        private static readonly Color BlueAccent     = Color.FromArgb(60, 140, 220);
        private static readonly Color GreenAccent    = Color.FromArgb(50, 180, 80);
        private static readonly Color PurpleAccent   = Color.FromArgb(140, 80, 200);
        private static readonly Color ResetBg        = Color.FromArgb(70, 40, 10);
        private static readonly Color ResetFg        = Color.FromArgb(220, 140, 50);

        private static readonly Color GridBg          = Color.FromArgb(28, 28, 28);
        private static readonly Color GridHeaderBg    = Color.FromArgb(45, 45, 45);
        private static readonly Color GridHeaderFg    = Color.FromArgb(220, 220, 220);
        private static readonly Color GridRowBg       = Color.FromArgb(32, 32, 32);
        private static readonly Color GridRowFg       = Color.FromArgb(220, 220, 220);
        private static readonly Color GridActiveRowBg = Color.FromArgb(30, 75, 35);
        private static readonly Color GridActiveRowFg = Color.FromArgb(150, 235, 160);
        private static readonly Color GridSelBg       = Color.FromArgb(255, 140, 0);
        private static readonly Color GridSelFg       = Color.White;

        // ── mode badges ──────────────────────────────────────────────────────────
        private Label _autoLabel, _manualLabel, _setupLabel, _bendingLabel, _operatorLabel;

        // ── metric cards ─────────────────────────────────────────────────────────
        private Label _totalHoursValue, _producingHoursValue, _efficiencyValue;
        private Label _partCountValue, _totalBendsValue;

        // ── parts grid ────────────────────────────────────────────────────────────
        private DataGridView _partsGrid;

        // ── live counters ─────────────────────────────────────────────────────────
        private Label  _liveSetupLabel, _livePartLabel;
        private System.Windows.Forms.Timer _liveTimer;
        private DateTime _liveSetupStart = DateTime.MinValue;
        private DateTime _livePartStart  = DateTime.MinValue;
        private bool     _setupPhase     = false;

        // ── state ─────────────────────────────────────────────────────────────────
        private readonly StatsStore _store;
        private string _currentJobKey    = "";
        private string _activeProductKey = "";
        private double _rawTotalHours;
        private double _rawProducingHours;

        // ── construction ─────────────────────────────────────────────────────────

        public StatsPage(StatsStore store)
        {
            _store    = store;
            BackColor = BgColor;
            Dock      = DockStyle.Fill;
            Build();
            // Seed counters from persisted store on startup
            if (_store.TotalPartCount > 0) _partCountValue.Text = _store.TotalPartCount.ToString("N0");
            if (_store.TotalBendCount > 0) _totalBendsValue.Text = _store.TotalBendCount.ToString("N0");
        }

        private void Build()
        {
            var root = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 3,
                ColumnCount = 1,
                BackColor   = BgColor,
                Padding     = new Padding(14, 10, 14, 10),
                Margin      = Padding.Empty,
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // mode badges
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));  // metrics cards
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // parts grid
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildModeRow(),    0, 0);
            root.Controls.Add(BuildMetricsRow(), 0, 1);
            root.Controls.Add(BuildPartsGrid(),  0, 2);

            Controls.Add(root);
        }

        // ── mode badges ──────────────────────────────────────────────────────────

        private Panel BuildModeRow()
        {
            var row = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                BackColor     = BgColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(0, 8, 0, 8),
            };
            _autoLabel     = MakeBadge("AUTO",          104);
            _manualLabel   = MakeBadge("MANUAL",        110);
            _setupLabel    = MakeBadge("SETUP",         104);
            _bendingLabel  = MakeBadge("BENDING",       116);
            _operatorLabel = MakeBadge("OPERATOR WAIT", 160);

            _liveSetupLabel = MakeLiveLabel("Setup: —", 140);
            _livePartLabel  = MakeLiveLabel("Part: —",  130);

            _liveTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _liveTimer.Tick += LiveTimer_Tick;

            row.Controls.AddRange(new Control[] {
                _autoLabel, _manualLabel, _setupLabel, _bendingLabel, _operatorLabel,
                _liveSetupLabel, _livePartLabel
            });
            return row;
        }

        private static Label MakeBadge(string text, int width) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            Size      = new Size(width, 38),
            Margin    = new Padding(0, 0, 10, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = InactiveBg,
            ForeColor = InactiveFg,
        };

        private static Label MakeLiveLabel(string text, int width) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            Size      = new Size(width, 38),
            Margin    = new Padding(20, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            ForeColor = InactiveFg,
        };

        // ── live timers ──────────────────────────────────────────────────────────

        public void StartSetupTimer(DateTime start)
        {
            _liveSetupStart         = start;
            _livePartStart          = DateTime.MinValue;
            _setupPhase             = true;
            _liveSetupLabel.Text    = "Setup: 0.0s";
            _liveSetupLabel.ForeColor = YellowFg;
            _livePartLabel.Text     = "Part: —";
            _livePartLabel.ForeColor  = InactiveFg;
            _liveTimer.Start();
        }

        public void StartPartTimer(DateTime bendStart)
        {
            _livePartStart            = bendStart;
            _setupPhase               = false;
            _livePartLabel.ForeColor  = GreenFg;
            if (!_liveTimer.Enabled) _liveTimer.Start(); // guard: in case setup timer wasn't running
        }

        public void StopLiveTimers()
        {
            _liveTimer.Stop();
            _liveSetupStart           = DateTime.MinValue;
            _livePartStart            = DateTime.MinValue;
            _liveSetupLabel.Text      = "Setup: —";
            _livePartLabel.Text       = "Part: —";
            _liveSetupLabel.ForeColor = InactiveFg;
            _livePartLabel.ForeColor  = InactiveFg;
        }

        private void LiveTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            if (_setupPhase && _liveSetupStart != DateTime.MinValue)
                _liveSetupLabel.Text = "Setup: " + FormatSeconds((now - _liveSetupStart).TotalSeconds);
            if (!_setupPhase && _livePartStart != DateTime.MinValue)
                _livePartLabel.Text = "Part: " + FormatSeconds((now - _livePartStart).TotalSeconds);
        }

        // ── metric cards ─────────────────────────────────────────────────────────

        private TableLayoutPanel BuildMetricsRow()
        {
            var row = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                BackColor   = BgColor,
                ColumnCount = 5,
                RowCount    = 1,
                Padding     = new Padding(0, 0, 0, 10),
                Margin      = Padding.Empty,
            };
            for (int i = 0; i < 5; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            void ResetHours()
            {
                var r = DarkMessageBox.Show(FindForm(),
                    "Zero out the hour and calculated efficiency displays?\r\n\r\n" +
                    "The machine's actual counters are not affected.\r\n" +
                    "This offset is saved and applied on every restart.",
                    "Reset Operating Hours", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                _store.ResetHours(_rawTotalHours, _rawProducingHours);
                _totalHoursValue.Text     = FormatHours(0);
                _producingHoursValue.Text = FormatHours(0);
                _efficiencyValue.Text     = "0.0%";
            }

            (_totalHoursValue,     var c0) = MakeCard("Total Op Hours",  "—", OrangeAccent,  ResetHours);
            (_producingHoursValue, var c1) = MakeCard("Producing Hours", "—", BlueAccent,    ResetHours);
            (_efficiencyValue,     var c2) = MakeCard("Efficiency",      "—", GreenAccent,   ResetHours);
            (_partCountValue,      var c3) = MakeCard("Total Parts",     "—", PurpleAccent,  () =>
            {
                var r = DarkMessageBox.Show(FindForm(), "Reset total part count to zero?",
                    "Reset Parts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                _store.ResetPartCount();
                _partCountValue.Text = "0";
            });
            (_totalBendsValue,     var c4) = MakeCard("Total Bends",     "—", TextSecondary, () =>
            {
                var r = DarkMessageBox.Show(FindForm(), "Reset total bend count to zero?",
                    "Reset Bends", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                _store.ResetBendCount();
                _totalBendsValue.Text = "0";
            });

            row.Controls.Add(c0, 0, 0);
            row.Controls.Add(c1, 1, 0);
            row.Controls.Add(c2, 2, 0);
            row.Controls.Add(c3, 3, 0);
            row.Controls.Add(c4, 4, 0);
            return row;
        }

        private (Label valueLabel, Panel card) MakeCard(string title, string initial, Color accent, Action onReset = null)
        {
            var card = new Panel
            {
                BackColor = CardBg,
                Dock      = DockStyle.Fill,
                Margin    = new Padding(0, 0, 8, 0),
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(CardBorder, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using var ab  = new SolidBrush(accent);
                e.Graphics.FillRectangle(ab, 1, 1, card.Width - 2, 3);
            };
            var titleLbl = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = TextSecondary,
                Height    = 22,
                Dock      = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Padding   = new Padding(0, 4, 0, 0),
            };
            var valueLbl = new Label
            {
                Text      = initial,
                Font      = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            card.Controls.Add(valueLbl);
            card.Controls.Add(titleLbl);

            if (onReset != null)
            {
                var resetBtn = new Button
                {
                    Text      = "⟳",
                    Size      = new Size(22, 22),
                    BackColor = ResetBg,
                    ForeColor = ResetFg,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                resetBtn.FlatAppearance.BorderSize = 0;
                resetBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 60, 15);
                resetBtn.Click += (s, e) => onReset();
                card.Controls.Add(resetBtn);
                resetBtn.BringToFront();
                card.Resize += (s, e) =>
                    resetBtn.Location = new Point(card.Width - resetBtn.Width - 4, card.Height - resetBtn.Height - 4);
            }

            return (valueLbl, card);
        }

        // ── parts grid ────────────────────────────────────────────────────────────

        private Panel BuildPartsGrid()
        {
            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = BgColor, Margin = Padding.Empty };

            _partsGrid = new DataGridView
            {
                Dock                        = DockStyle.Fill,
                BackgroundColor             = GridBg,
                BorderStyle                 = BorderStyle.None,
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.Single,
                GridColor                   = Color.FromArgb(55, 55, 55),
                RowHeadersVisible           = false,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                ReadOnly                    = true,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect                 = false,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 40,
                ScrollBars                  = ScrollBars.Vertical,
                EnableHeadersVisualStyles   = false,
                Font                        = new Font("Segoe UI", 11F),
            };
            _partsGrid.RowTemplate.Height = 44;

            var headerStyle = new DataGridViewCellStyle
            {
                BackColor          = GridHeaderBg,
                ForeColor          = GridHeaderFg,
                Font               = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment          = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = GridHeaderBg,
                SelectionForeColor = GridHeaderFg,
            };
            _partsGrid.ColumnHeadersDefaultCellStyle = headerStyle;

            var cellStyle = new DataGridViewCellStyle
            {
                BackColor          = GridRowBg,
                ForeColor          = GridRowFg,
                Font               = new Font("Segoe UI", 11F),
                SelectionBackColor = GridSelBg,
                SelectionForeColor = GridSelFg,
            };
            _partsGrid.DefaultCellStyle = cellStyle;

            AddCol("colPart",      "Part",        200);
            AddCol("colCount",     "Count",        70);
            AddCol("colLastCycle", "Last Cycle",  110);
            AddCol("colAvgCycle",  "Avg Cycle",   110);
            AddCol("colMinCycle",  "Min",          95);
            AddCol("colMaxCycle",  "Max",          95);
            AddCol("colSetup",     "Last Setup",  110);
            AddCol("colAvgSetup",  "Avg Setup",   110);
            AddCol("colAvgBend",   "Avg Bend",    110);

            _partsGrid.Columns["colPart"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _partsGrid.Columns["colPart"].MinimumWidth = 120;

            _partsGrid.CellFormatting += PartsGrid_CellFormatting;

            wrap.Controls.Add(_partsGrid);
            return wrap;
        }

        private void AddCol(string name, string header, int width)
        {
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name         = name,
                HeaderText   = header,
                Width        = width,
                MinimumWidth = 50,
                SortMode     = DataGridViewColumnSortMode.NotSortable,
                Resizable    = DataGridViewTriState.True,
            });
        }

        private void PartsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _partsGrid.Rows.Count) return;
            if (string.IsNullOrEmpty(_activeProductKey)) return;
            if ((_partsGrid.Rows[e.RowIndex].Tag as string) != _activeProductKey) return;
            e.CellStyle.BackColor          = GridActiveRowBg;
            e.CellStyle.ForeColor          = GridActiveRowFg;
            e.CellStyle.SelectionBackColor = GridActiveRowBg;
            e.CellStyle.SelectionForeColor = GridActiveRowFg;
        }

        private void RefreshGrid()
        {
            _partsGrid.SuspendLayout();
            _partsGrid.Rows.Clear();
            foreach (var key in _store.AllProductKeys.OrderBy(k => k))
                AddOrUpdateRow(key);
            _partsGrid.ResumeLayout();
        }

        private void AddOrUpdateRow(string productKey)
        {
            DataGridViewRow row = null;
            foreach (DataGridViewRow r in _partsGrid.Rows)
                if ((r.Tag as string) == productKey) { row = r; break; }

            if (row == null)
            {
                int idx = _partsGrid.Rows.Add();
                row = _partsGrid.Rows[idx];
                row.Tag = productKey;
            }

            var cycles = _store.GetProductCycleTimes(productKey);
            var setups = _store.GetProductSetupTimes(productKey);
            var bends  = _store.GetProductBendingTimes(productKey);

            row.Cells["colPart"].Value      = productKey;
            row.Cells["colCount"].Value     = cycles.Count > 0 ? cycles.Count.ToString() : "—";
            row.Cells["colLastCycle"].Value = cycles.Count > 0 ? FormatSeconds(cycles[cycles.Count - 1]) : "—";
            row.Cells["colAvgCycle"].Value  = cycles.Count > 0 ? FormatSeconds(cycles.Average()) : "—";
            row.Cells["colMinCycle"].Value  = cycles.Count > 0 ? FormatSeconds(cycles.Min()) : "—";
            row.Cells["colMaxCycle"].Value  = cycles.Count > 0 ? FormatSeconds(cycles.Max()) : "—";
            row.Cells["colSetup"].Value     = setups.Count > 0 ? FormatSeconds(setups[setups.Count - 1]) : "—";
            row.Cells["colAvgSetup"].Value  = setups.Count > 0 ? FormatSeconds(setups.Average()) : "—";
            row.Cells["colAvgBend"].Value   = bends.Count  > 0 ? FormatSeconds(bends.Average()) : "—";
        }

        // ── public update API ─────────────────────────────────────────────────────

        public void UpdateAutoMode(bool active)     => SetBadge(_autoLabel,     active, GreenBg,  GreenFg);
        public void UpdateManualMode(bool active)   => SetBadge(_manualLabel,   active, YellowBg, YellowFg);
        public void UpdateSetupMode(bool active)    => SetBadge(_setupLabel,    active, BlueBg,   BlueFg);
        public void UpdateBendingNow(bool active)   => SetBadge(_bendingLabel,  active, RedBg,    RedFg);
        public void UpdateOperatorWait(bool active) => SetBadge(_operatorLabel, active, YellowBg, YellowFg);

        public void UpdateHours(double rawTotalSeconds, double rawProducingSeconds)
        {
            _rawTotalHours     = rawTotalSeconds;
            _rawProducingHours = rawProducingSeconds;
            var (t, p) = _store.ApplyBaseline(rawTotalSeconds, rawProducingSeconds);
            _totalHoursValue.Text     = FormatHours(t / 3600.0);
            _producingHoursValue.Text = FormatHours(p / 3600.0);
            _efficiencyValue.Text     = $"{(t > 0 ? p / t * 100.0 : 0):F1}%";
        }

        public void UpdatePartCount(int count) { } // driven by cycle count instead

        public void UpdateProductionStep(string step) { } // card replaced by Total Bends

        public void UpdateTotalBends(int count) =>
            _totalBendsValue.Text = count.ToString("N0");

        public void SetActiveProduct(string productKey)
        {
            _activeProductKey = productKey;
            _partsGrid.Invalidate();
        }

        public void UpdateSetupTime(string productKey, double seconds)
        {
            AddOrUpdateRow(productKey);
            _partsGrid.Invalidate();
        }

        public void UpdatePartToPartTime(string productKey, double seconds) =>
            AddOrUpdateRow(productKey);

        public void UpdateBendingTime(string productKey, double seconds) =>
            AddOrUpdateRow(productKey);

        public void UpdateLastCycleTime(string productKey, double cycleTime)
        {
            if (cycleTime <= 0) return;
            _store.AddCycleTime(_currentJobKey, productKey, cycleTime);
            _partCountValue.Text = _store.TotalPartCount.ToString("N0");
            AddOrUpdateRow(productKey);
        }

        public void SetJob(string jobKey)
        {
            _currentJobKey    = jobKey;
            _activeProductKey = "";
            RefreshGrid();
        }

        public void Reset()
        {
            SetBadge(_autoLabel,     false, GreenBg,  GreenFg);
            SetBadge(_manualLabel,   false, YellowBg, YellowFg);
            SetBadge(_setupLabel,    false, BlueBg,   BlueFg);
            SetBadge(_bendingLabel,  false, RedBg,    RedFg);
            SetBadge(_operatorLabel, false, YellowBg, YellowFg);
            _totalHoursValue.Text     = "—";
            _producingHoursValue.Text = "—";
            _efficiencyValue.Text     = "—";
            _partCountValue.Text  = _store.TotalPartCount > 0 ? _store.TotalPartCount.ToString("N0") : "—";
            _totalBendsValue.Text = _store.TotalBendCount > 0 ? _store.TotalBendCount.ToString("N0") : "—";
            _currentJobKey            = "";
            _activeProductKey         = "";
            _partsGrid.Rows.Clear();
            StopLiveTimers();
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static void SetBadge(Label lbl, bool active, Color activeBg, Color activeFg)
        {
            lbl.BackColor = active ? activeBg : InactiveBg;
            lbl.ForeColor = active ? activeFg : InactiveFg;
        }

        private static string FormatHours(double hours)
        {
            if (hours < 0) return "—";
            int h = (int)hours;
            int m = (int)((hours - h) * 60);
            return $"{h}h {m:D2}m";
        }

        private static string FormatSeconds(double s) => s >= 60
            ? $"{(int)(s / 60)}m {(int)(s % 60):D2}s"
            : $"{s:F1}s";
    }
}
