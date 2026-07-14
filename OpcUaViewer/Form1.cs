using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OpcUaViewer
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Path to browse, relative to the Objects folder, expressed as "namespaceIndex:BrowseName"
        /// segments separated by '/'. Equivalent to:
        /// /Root/Objects/4:PLC/6:Modules/6:::/6:Global PV/6:Monitoring
        /// Change this if your server's address space uses a different structure.
        /// </summary>
        private const string MonitoringFolderPath = "4:PLC/6:Modules/6:::/6:Global PV/6:Monitoring";

        /// <summary>
        /// Any monitored item whose name contains this text (case-insensitive) is treated as the
        /// "current product id" — its value drives which PDF gets shown in the Document Viewer tab.
        /// </summary>
        private const string ProductIdNameMatch = "ProductId";

        // Hardcoded OPC UA nodes. Namespace index 6 is the application namespace on
        // this server — update OperatorActionNamespaceIndex if the server config changes.
        private const ushort OperatorActionNamespaceIndex = 6;
        private const string OperatorActionNodeId         = "::AsGlobalPV:Monitoring.OperatorActionRequested";

        // Statistics nodes (same namespace)
        private const string StatsAutoModeNodeId       = "::AsGlobalPV:Monitoring.AutomaticMode";
        private const string StatsManualModeNodeId     = "::AsGlobalPV:Monitoring.ManualMode";
        private const string StatsSetupModeNodeId      = "::AsGlobalPV:Monitoring.SetupMode";
        private const string StatsTotalHoursNodeId     = "::AsGlobalPV:Monitoring.TotalOperatingHours";
        private const string StatsProducingHoursNodeId = "::AsGlobalPV:Monitoring.TotalOperatingHoursProducing";

        private const string StatsPartCountNodeId      = "::AsGlobalPV:Monitoring.CurrentProductCount";
        private const string StatsCurrentStepNodeId    = "::AsGlobalPV:Monitoring.CurrentProductionStep";
        private const string StatsBendingNowNodeId     = "::AsGlobalPV:Monitoring.BendingNow";

        private ISession session;
        private Subscription subscription;
        private CancellationTokenSource cts;

        // The variables discovered under MonitoringFolderPath, populated on connect.
        private List<(string Name, NodeId NodeId)> monitoredNodes = new List<(string Name, NodeId NodeId)>();

        // Maps a MonitoredItem's ClientHandle to the grid row that displays its value.
        private readonly Dictionary<uint, DataGridViewRow> rowsByClientHandle = new Dictionary<uint, DataGridViewRow>();

        // ClientHandle of the item identified as the product id, if one was found.
        private uint? productIdClientHandle;
        private uint? camFileClientHandle;
        private uint? machineStateClientHandle;
        private uint? operatorActionClientHandle;
        private uint? statsAutoModeHandle, statsManualModeHandle, statsSetupModeHandle;
        private uint? statsTotalHoursHandle, statsProducingHoursHandle;
        private uint? statsPartCountHandle, statsCurrentStepHandle;

        private uint? statsBendingNowHandle;

        // ── implied timing state machine ──────────────────────────────────────────
        // All timestamps are UTC; all fields written only on the UI thread.
        // Setup phase: armed by step→0 or product-id change; closed by first bend.
        // Bending phase: from first bend to step→0.
        private bool     _waitingForFirstBend = false;
        private DateTime _setupStartTime      = DateTime.MinValue; // start of setup phase
        private bool     _firstBendHappened   = false;             // true once first bend seen
        private DateTime _firstBendTime       = DateTime.MinValue; // when first bend started

        private bool     _isBendingNow        = false;
        private int      _lastProductionStep  = -1;

        private Label     _operatorActionLabel;
        private StatsStore _statsStore;
        private StatsPage  _statsPage;
        private Button     _statsNavButton;
        private Panel      _statsContentPanel;

        // Last-known values from the three monitoring nodes.
        private string _activeCamFileName;
        private string _activeProductId;
        private int    _machineState    = -1;
        private int    _activeCamRowIndex = -1;  // row locked by OPC — -1 when no active file
        private bool   _lockingSelection;        // re-entry guard for selection snap-back

        // Avoids reloading the same PDF repeatedly.
        private string lastLoadedProductId;

        // Row-state dictionaries used by CellFormatting — keyed by row index.
        // null entry = normal (no override). Updated by OnCamFileChanged / HighlightActiveProduct.
        private enum RowState { Normal, Active, Dim }
        private readonly Dictionary<int, RowState> _orderRowStates   = new();
        private readonly Dictionary<int, RowState> _productRowStates = new();

        private static readonly System.Drawing.Color RowActiveBg        = System.Drawing.Color.FromArgb(25, 70, 35);
        private static readonly System.Drawing.Color RowActiveFg        = System.Drawing.Color.FromArgb(180, 255, 180);
        private static readonly System.Drawing.Color RowActiveSelBg     = System.Drawing.Color.FromArgb(40, 100, 50);
        private static readonly System.Drawing.Color RowActiveProductBg = System.Drawing.Color.FromArgb(90, 55, 10);
        private static readonly System.Drawing.Color RowActiveProductFg = System.Drawing.Color.FromArgb(255, 210, 140);
        private static readonly System.Drawing.Color RowDimBg           = System.Drawing.Color.FromArgb(22, 22, 22);
        private static readonly System.Drawing.Color RowDimFg           = System.Drawing.Color.FromArgb(65, 65, 65);

        private static readonly System.Drawing.Color NavAccent   = System.Drawing.Color.FromArgb(255, 140, 0);
        private static readonly System.Drawing.Color NavInactive = System.Drawing.Color.FromArgb(48, 48, 48);
        private static readonly System.Drawing.Color NavTextOn   = System.Drawing.Color.White;
        private static readonly System.Drawing.Color NavTextOff  = System.Drawing.Color.FromArgb(170, 170, 170);

        // Prevents the keyboard re-opening when focus returns after dismissal.
        private bool _suppressKeyboard;

        public Form1()
        {
            InitializeComponent();
            BuildGroupInfoPanel();
            BuildOperatorActionLabel();
            BuildStatsPanel();
            SetupScrollBars();
            ordersDataGridView.CellFormatting   += OrdersGrid_CellFormatting;
            productsDataGridView.CellFormatting += ProductsGrid_CellFormatting;
            productsDataGridView.SelectionChanged += (s, e) =>
            {
                if (IsRunningLocked())
                    productsDataGridView.ClearSelection();
            };
            FormClosing += Form1_FormClosing;
            Shown        += Form1_Shown;
            LoadSettings();

            HookKeyboard(endpointTextBox);
            HookKeyboard(pdfFolderTextBox);
            HookKeyboard(camFolderTextBox);
            HookKeyboard(camOutputTextBox);
            HookKeyboard(camProductsTextBox);
            HookKeyboard(camProductPrefixTextBox);
            productsDataGridView.CellBeginEdit += productsDataGridView_CellBeginEdit;
        }

        // ── edit mode state ───────────────────────────────────────────────────────
        private bool   _editMode;
        private string _editingFilePath; // null when creating a new group

        private static readonly System.Drawing.Color InfoPanelBg     = System.Drawing.Color.FromArgb(36, 36, 36);
        private static readonly System.Drawing.Color InfoPanelEditBg = System.Drawing.Color.FromArgb(30, 36, 30);
        private static readonly System.Drawing.Color InfoFieldBg     = System.Drawing.Color.FromArgb(50, 50, 50);
        private static readonly System.Drawing.Color InfoFieldFg     = System.Drawing.Color.FromArgb(230, 230, 230);

        private void BuildGroupInfoPanel()
        {
            var keyFont  = new System.Drawing.Font("Segoe UI", 9F);
            var valFont  = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            var keyColor = System.Drawing.Color.FromArgb(160, 160, 160);

            groupInfoPanel.Dock      = System.Windows.Forms.DockStyle.Top;
            groupInfoPanel.Height    = 125;
            groupInfoPanel.BackColor = InfoPanelBg;
            groupInfoPanel.Padding   = new System.Windows.Forms.Padding(6, 6, 6, 6);

            // Separator strip — same colour as the SplitContainer splitter, slightly wider for visual parity
            var sep = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Bottom,
                Height    = 6,
                BackColor = System.Drawing.Color.FromArgb(55, 55, 55),
            };
            groupInfoPanel.Controls.Add(sep);

            // TableLayoutPanel: 3 rows × 6 columns
            var tlp = new System.Windows.Forms.TableLayoutPanel
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 6,
                RowCount    = 3,
                BackColor   = InfoPanelBg,
                Padding     = new System.Windows.Forms.Padding(0, 0, 0, 0),
            };
            // Column styles: label|value|label|value|label|value
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));

            // Labels and TextBoxes share the same top margin so their baselines align.
            // TextBoxes use Anchor rather than Dock so they keep their natural single-line height.
            const int rowTop = 6;

            System.Windows.Forms.Label K(string t) => new System.Windows.Forms.Label
            {
                Text      = t, Font = keyFont, ForeColor = keyColor, AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.TopRight,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Margin    = new System.Windows.Forms.Padding(4, rowTop + 3, 2, 0),
            };

            System.Windows.Forms.TextBox V(bool numericOnly = false)
            {
                var tb = new System.Windows.Forms.TextBox
                {
                    ReadOnly    = true,
                    BorderStyle = System.Windows.Forms.BorderStyle.None,
                    BackColor   = InfoPanelBg,
                    ForeColor   = InfoFieldFg,
                    Font        = valFont,
                    TabStop     = false,
                    Dock        = System.Windows.Forms.DockStyle.Fill,
                    Margin      = new System.Windows.Forms.Padding(0, rowTop, 8, 0),
                };
                return tb;
            }

            // Row 0: File name | Order ID
            tlp.Controls.Add(K("File:"),       0, 0);
            infoFileNameBox = V();             tlp.Controls.Add(infoFileNameBox, 1, 0);
            tlp.Controls.Add(K("Order:"),      2, 0);
            infoOrderIdBox  = V();             tlp.Controls.Add(infoOrderIdBox,  3, 0);
            // span remaining two columns for visual balance
            tlp.SetColumnSpan(infoOrderIdBox, 3);

            // Row 1: Customer | Qty Ordered | Qty Produced
            tlp.Controls.Add(K("Customer:"),   0, 1);
            infoCustomerBox = V();             tlp.Controls.Add(infoCustomerBox, 1, 1);
            tlp.Controls.Add(K("Qty Ord:"),    2, 1);
            infoQtyBox      = V(true);         tlp.Controls.Add(infoQtyBox,      3, 1);
            tlp.Controls.Add(K("Qty Prod:"),   4, 1);
            infoCompletedBox = V();            tlp.Controls.Add(infoCompletedBox, 5, 1);

            // Row 2: Info text spanning all value columns
            tlp.Controls.Add(K("Info:"),       0, 2);
            infoInfoTextBox = V();             tlp.Controls.Add(infoInfoTextBox, 1, 2);
            tlp.SetColumnSpan(infoInfoTextBox, 5);

            groupInfoPanel.Controls.Add(tlp);

            // Keyboard hooks (guard in HookKeyboard skips ReadOnly boxes)
            foreach (var tb in new[] { infoFileNameBox, infoOrderIdBox,
                                       infoCustomerBox, infoInfoTextBox })
                HookKeyboard(tb);
            HookKeyboard(infoQtyBox, numericOnly: true);
        }

        // ── operator action status label ─────────────────────────────────────────

        private void BuildOperatorActionLabel()
        {
            _operatorActionLabel = new Label
            {
                Visible     = false,
                Anchor      = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                Location    = new System.Drawing.Point(8, 8),
                Size        = new System.Drawing.Size(runGroupButton.Left - 16, groupsButtonPanel.Height - 16),
                Font        = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold),
                TextAlign   = System.Drawing.ContentAlignment.MiddleCenter,
                BackColor   = System.Drawing.Color.FromArgb(20, 80, 30),
                ForeColor   = System.Drawing.Color.FromArgb(120, 220, 120),
                Text        = "Processing taking place automatically",
                BorderStyle = BorderStyle.None,
            };
            groupsButtonPanel.Controls.Add(_operatorActionLabel);
            _operatorActionLabel.BringToFront();

            groupsButtonPanel.Resize += (s, e) =>
                _operatorActionLabel.Width = runGroupButton.Left - 16;
        }

        private void UpdateOperatorActionLabel(bool waiting)
        {
            if (_machineState != 3) return;
            if (waiting)
            {
                _operatorActionLabel.BackColor = System.Drawing.Color.FromArgb(100, 75, 0);
                _operatorActionLabel.ForeColor = System.Drawing.Color.FromArgb(255, 220, 60);
                _operatorActionLabel.Text      = "Waiting for operator action...";
            }
            else
            {
                _operatorActionLabel.BackColor = System.Drawing.Color.FromArgb(20, 80, 30);
                _operatorActionLabel.ForeColor = System.Drawing.Color.FromArgb(120, 220, 120);
                _operatorActionLabel.Text      = "Processing taking place automatically";
            }
        }

        // ── statistics tab ───────────────────────────────────────────────────────

        private void BuildStatsPanel()
        {
            // Nav button — sits after groupsNavButton (y=341) and before settings (bottom-anchored)
            _statsNavButton = new Button
            {
                Text      = "Statistics",
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new System.Drawing.Point(0, 344),
                Size      = new System.Drawing.Size(100, 90),
                BackColor = NavInactive,
                ForeColor = NavTextOff,
                FlatStyle = FlatStyle.Flat,
                Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                UseVisualStyleBackColor = false,
            };
            _statsNavButton.FlatAppearance.BorderSize = 0;
            _statsNavButton.Click += (s, e) => ShowPage(_statsContentPanel, _statsNavButton);
            navPanel.Controls.Add(_statsNavButton);

            // Content panel — same position/anchor as all other content panels
            _statsContentPanel = new Panel
            {
                Visible   = false,
                Anchor    = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new System.Drawing.Point(106, 0),
                Size      = new System.Drawing.Size(994, 660),
                BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            };
            Controls.Add(_statsContentPanel);

            try
            {
                _statsStore = StatsStore.Load();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StatsStore load failed: {ex.Message}");
                _statsStore = new StatsStore();
            }
            _statsPage = new StatsPage(_statsStore);
            _statsContentPanel.Controls.Add(_statsPage);
        }

        // ── touch-friendly dark scrollbars ───────────────────────────────────────

        private const int ScrollBarW = 36;

        private void SetupScrollBars()
        {
            AttachScrollBar(dataGridView1);
            AttachScrollBar(ordersDataGridView);
            AttachScrollBar(productsDataGridView);
            SetupSettingsScroll();
        }

        private void AttachScrollBar(System.Windows.Forms.DataGridView dgv)
        {
            var parent = dgv.Parent;
            dgv.ScrollBars = System.Windows.Forms.ScrollBars.None;

            // Height taken by any top-docked siblings (e.g. groupInfoPanel above the products grid)
            int topOffset = parent.Controls
                .Cast<System.Windows.Forms.Control>()
                .Where(c => c != dgv && c.Dock == System.Windows.Forms.DockStyle.Top)
                .Sum(c => c.Height);

            // Shrink the DGV to leave room for our scrollbar
            if (dgv.Dock == System.Windows.Forms.DockStyle.Fill)
            {
                dgv.Dock   = System.Windows.Forms.DockStyle.None;
                dgv.Anchor = System.Windows.Forms.AnchorStyles.Top    |
                             System.Windows.Forms.AnchorStyles.Bottom  |
                             System.Windows.Forms.AnchorStyles.Left    |
                             System.Windows.Forms.AnchorStyles.Right;
                dgv.SetBounds(0, topOffset,
                    parent.ClientSize.Width - ScrollBarW,
                    parent.ClientSize.Height - topOffset);
            }
            else
            {
                dgv.Width -= ScrollBarW;
            }

            var sb = new DarkScrollBar
            {
                Left          = parent.ClientSize.Width - ScrollBarW,
                Top           = topOffset,
                Height        = parent.ClientSize.Height - topOffset,
                Anchor        = System.Windows.Forms.AnchorStyles.Top   |
                                System.Windows.Forms.AnchorStyles.Bottom |
                                System.Windows.Forms.AnchorStyles.Right,
                HeaderHeight    = dgv.ColumnHeadersHeight,
                HeaderBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor,
            };
            parent.Controls.Add(sb);
            sb.BringToFront();

            void UpdateRange()
            {
                int visible = Math.Max(1, dgv.DisplayedRowCount(false));
                int total   = dgv.Rows.Count;
                sb.LargeChange = visible;
                sb.Maximum     = Math.Max(0, total - visible);
                // Always visible — thumb disappears when nothing to scroll
            }

            sb.Scroll += (s, e) =>
            {
                if (dgv.Rows.Count > 0)
                    dgv.FirstDisplayedScrollingRowIndex =
                        Math.Min(sb.Value, dgv.Rows.Count - 1);
            };

            dgv.Scroll      += (s, e) => { sb.Value = dgv.FirstDisplayedScrollingRowIndex; };
            dgv.RowsAdded   += (s, e) => UpdateRange();
            dgv.RowsRemoved += (s, e) => UpdateRange();
            dgv.Resize      += (s, e) => UpdateRange();
            dgv.MouseWheel  += (s, e) =>
            {
                sb.HandleWheel(e.Delta);
                if (dgv.Rows.Count > 0)
                    dgv.FirstDisplayedScrollingRowIndex =
                        Math.Min(sb.Value, dgv.Rows.Count - 1);
            };

            UpdateRange();
        }

        private void SetupSettingsScroll()
        {
            // AutoScroll handles overflow without touching any designer controls.
            // Controls anchored Left+Right resize naturally; native OS scrollbar
            // appears only when content is taller than the visible area.
            settingsPanel.AutoScroll = true;
        }

        private void SetInfoEditStyle(bool editing)
        {
            var bg = editing ? InfoFieldBg : InfoPanelBg;
            var border = editing ? System.Windows.Forms.BorderStyle.FixedSingle
                                 : System.Windows.Forms.BorderStyle.None;
            foreach (var tb in new[] { infoFileNameBox, infoOrderIdBox, infoQtyBox,
                                       infoCustomerBox, infoInfoTextBox })
            {
                tb.ReadOnly    = !editing;
                tb.BorderStyle = border;
                tb.BackColor   = bg;
                tb.TabStop     = editing;
            }
            var panelBg = editing ? InfoPanelEditBg : InfoPanelBg;
            groupInfoPanel.BackColor = panelBg;
            // Propagate to TableLayoutPanel so the label gaps change colour too
            foreach (System.Windows.Forms.Control c in groupInfoPanel.Controls)
                if (c is System.Windows.Forms.TableLayoutPanel)
                    c.BackColor = panelBg;
        }

        private void EnterEditMode(CamOrder existing)
        {
            _editMode        = true;
            _editingFilePath = existing?.FilePath;

            if (existing != null)
            {
                infoFileNameBox.Text  = existing.FileName;
                infoOrderIdBox.Text   = existing.OrderId;
                infoQtyBox.Text       = existing.Quantity.ToString();
                infoCompletedBox.Text = existing.Completed.ToString();
                infoCustomerBox.Text  = existing.CustomerName;
                infoInfoTextBox.Text  = existing.InfoText;
                // Products already in the grid — they stay
            }
            else
            {
                var (defName, defId) = NextOrderDefaults();
                infoFileNameBox.Text  = defName;
                infoOrderIdBox.Text   = defId;
                infoQtyBox.Text       = "1";
                infoCompletedBox.Text = "0";
                infoCustomerBox.Text  = "";
                infoInfoTextBox.Text  = "";
                productsDataGridView.Rows.Clear();
            }

            SetInfoEditStyle(true);

            // Make relevant product columns editable
            productsDataGridView.Columns["prodListIdColumn"].ReadOnly = false;
            productsDataGridView.Columns["prodNameColumn"].ReadOnly   = false;
            productsDataGridView.Columns["prodQtyColumn"].ReadOnly    = false;
            productsDataGridView.Columns["prodHintColumn"].ReadOnly   = false;

            // Swap buttons
            newGroupButton.Visible       = false;
            editGroupButton.Visible      = false;
            deleteGroupButton.Visible    = false;
            runGroupButton.Visible       = false;
            cancelGroupButton.Visible    = false;
            saveGroupButton.Visible      = true;
            cancelEditButton.Visible     = true;
            addProductsButton.Visible    = true;
            removeProductsButton.Visible = true;

            ordersDataGridView.Enabled = false;
        }

        private void ExitEditMode(bool reload)
        {
            string pathToRestore = _editingFilePath;
            _editMode        = false;
            _editingFilePath = null;

            SetInfoEditStyle(false);

            productsDataGridView.Columns["prodListIdColumn"].ReadOnly = true;
            productsDataGridView.Columns["prodNameColumn"].ReadOnly   = true;
            productsDataGridView.Columns["prodQtyColumn"].ReadOnly    = true;
            productsDataGridView.Columns["prodHintColumn"].ReadOnly   = true;

            newGroupButton.Visible       = true;
            editGroupButton.Visible      = true;
            deleteGroupButton.Visible    = true;
            runGroupButton.Visible       = true;
            cancelGroupButton.Visible    = true;
            saveGroupButton.Visible      = false;
            cancelEditButton.Visible     = false;
            addProductsButton.Visible    = false;
            removeProductsButton.Visible = false;

            ordersDataGridView.Enabled = true;

            if (reload) LoadCamFiles(pathToRestore);
        }

        private void HookKeyboard(TextBox tb, bool numericOnly = false)
        {
            // Record when focus arrives so Click can distinguish the focusing tap
            // from a subsequent tap. On touchscreens Enter fires before MouseDown,
            // so checking tb.Focused in MouseDown is unreliable.
            DateTime focusedAt = DateTime.MinValue;
            tb.Enter += (s, e) => focusedAt = DateTime.UtcNow;

            tb.Click += (s, e) =>
            {
                if (tb.ReadOnly) return; // don't open keyboard for display-mode fields
                // Skip if this click is what gave us focus (within ~300 ms of Enter).
                if ((DateTime.UtcNow - focusedAt).TotalMilliseconds < 300) return;
                if (_suppressKeyboard || !keyboardToggle.Checked) return;
                _suppressKeyboard = true;
                string result = TouchKeyboard.Show(this, tb.Text, numericOnly);
                if (result != null) tb.Text = result;
                BeginInvoke(new Action(() => _suppressKeyboard = false));
            };
        }

        private void productsDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!keyboardToggle.Checked) return;
            string col = productsDataGridView.Columns[e.ColumnIndex].Name;

            if (col == "prodRunQtyColumn")
            {
                e.Cancel = true;
                string current = productsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "0";
                string result  = TouchKeyboard.Show(this, current, numericOnly: true);
                if (result != null && int.TryParse(result, out int val) && val >= 0)
                {
                    productsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = val;
                    if (ordersDataGridView.SelectedRows.Count > 0)
                    {
                        int orderIdx = ordersDataGridView.SelectedRows[0].Index;
                        if (orderIdx < _camOrders.Count && e.RowIndex < _camOrders[orderIdx].Products.Count)
                            _camOrders[orderIdx].Products[e.RowIndex].RunQuantity = val;
                    }
                }
            }
            else if (_editMode && (col == "prodQtyColumn" || col == "prodListIdColumn" ||
                                   col == "prodNameColumn" || col == "prodHintColumn"))
            {
                e.Cancel = true;
                int row = e.RowIndex, colIdx = e.ColumnIndex;
                bool numeric = col == "prodQtyColumn";
                BeginInvoke(new Action(() =>
                {
                    var cell = productsDataGridView.Rows[row].Cells[colIdx];
                    string cur = cell.Value?.ToString() ?? "";
                    string result = numeric
                        ? TouchKeyboard.Show(this, cur, numericOnly: true)
                        : TouchKeyboard.Show(this, cur);
                    if (result == null) return;
                    if (numeric && int.TryParse(result, out int v)) cell.Value = v;
                    else if (!numeric) cell.Value = result;
                }));
            }
        }

        private bool _autoRetrying = false;
        private const int RetryIntervalSeconds = 10;

        private async void Form1_Shown(object sender, EventArgs e)
        {
            // Initialise WebView2 in the background — must not block OPC UA connect.
            docViewer.CoreWebView2InitializationCompleted += (s2, e2) =>
            {
                if (e2.IsSuccess)
                    ClearPdfView("Waiting for a product ID...");
                else
                    ShowWebView2MissingPanel();
            };
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpcUaViewer", "WebView2");
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null, userDataFolder: userDataFolder);
            _ = docViewer.EnsureCoreWebView2Async(env);

            string url = endpointTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;
            await AutoConnectWithRetry(url);
        }

        private async Task AutoConnectWithRetry(string url)
        {
            _autoRetrying        = true;
            connectButton.Text   = "Cancel";
            connectButton.Enabled = true;
            endpointTextBox.Enabled = false;
            cts = new CancellationTokenSource();

            int attempt = 0;
            while (session == null && !cts.IsCancellationRequested)
            {
                attempt++;
                UpdateStatus(attempt == 1 ? "Connecting..." : $"Reconnecting... (attempt {attempt})");
                await StartClient(url, cts.Token, silent: true);

                if (session != null || cts.IsCancellationRequested) break;

                for (int i = RetryIntervalSeconds; i > 0; i--)
                {
                    if (cts.IsCancellationRequested) break;
                    UpdateStatus($"Connection failed — retrying in {i}s...");
                    try { await Task.Delay(1000, cts.Token); } catch (OperationCanceledException) { break; }
                }
            }

            _autoRetrying           = false;
            connectButton.Text      = session != null ? "Disconnect" : "Connect";
            connectButton.Enabled   = true;
            endpointTextBox.Enabled = session == null;
            if (session == null && cts.IsCancellationRequested)
                UpdateStatus("Not connected");
        }

        // Loaded .p3cam orders; index matches groupsListView item index.
        private readonly List<CamOrder> _camOrders = new List<CamOrder>();

        private void monitorNavButton_Click(object sender, EventArgs e)  => ShowPage(monitoringPanel, monitorNavButton);
        private void documentNavButton_Click(object sender, EventArgs e) => ShowPage(documentPanel,  documentNavButton);
        private void settingsNavButton_Click(object sender, EventArgs e) => ShowPage(settingsPanel,  settingsNavButton);
        private void groupsNavButton_Click(object sender, EventArgs e)
        {
            ShowPage(groupsPanel, groupsNavButton);
            if (_camOrders.Count == 0)
                LoadCamFiles();
        }

        private void ShowPage(System.Windows.Forms.Panel page, Button activeButton)
        {
            monitoringPanel.Visible    = false;
            documentPanel.Visible      = false;
            settingsPanel.Visible      = false;
            groupsPanel.Visible        = false;
            _statsContentPanel.Visible = false;
            page.Visible = true;

            foreach (var btn in new[] { monitorNavButton, documentNavButton, settingsNavButton, groupsNavButton, _statsNavButton })
            {
                btn.BackColor = btn == activeButton ? NavAccent   : NavInactive;
                btn.ForeColor = btn == activeButton ? NavTextOn   : NavTextOff;
            }
        }

        private void LoadCamFiles(string preserveFilePath = null)
        {
            string folder = camFolderTextBox.Text.Trim();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                ordersDataGridView.Rows.Clear();
                productsDataGridView.Rows.Clear();
                _camOrders.Clear();
                return;
            }

            var files = Directory.GetFiles(folder, "*.p3cam", SearchOption.TopDirectoryOnly);

            ordersDataGridView.Rows.Clear();
            productsDataGridView.Rows.Clear();
            _camOrders.Clear();

            foreach (var file in files)
            {
                try
                {
                    var order = new CamOrder(file);
                    _camOrders.Add(order);
                    ordersDataGridView.Rows.Add(order.OrderId, order.FileName, order.Quantity);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping malformed CAM file '{file}': {ex.Message}");
                }
            }

            if (ordersDataGridView.Rows.Count == 0) return;

            // Restore previously selected file if it still exists, else fall back to row 0.
            if (preserveFilePath != null)
            {
                for (int i = 0; i < _camOrders.Count; i++)
                {
                    if (string.Equals(_camOrders[i].FilePath, preserveFilePath,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ordersDataGridView.Rows[i].Selected = true;
                        return;
                    }
                }
            }
            ordersDataGridView.Rows[0].Selected = true;
        }

        // Returns auto-incremented (fileName, orderId) for a new group based on existing names.
        private static readonly System.Text.RegularExpressions.Regex _fileOrderRx =
            new(@"- Order (\d+)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        private static readonly System.Text.RegularExpressions.Regex _orderIdRx =
            new(@"^O(\d+)$",         System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

        private (string fileName, string orderId) NextOrderDefaults()
        {
            var fileRx    = _fileOrderRx;
            var orderIdRx = _orderIdRx;

            int maxNum = 0;
            foreach (var o in _camOrders)
            {
                var m = fileRx.Match(o.FileName);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int n))
                    maxNum = Math.Max(maxNum, n);
                m = orderIdRx.Match(o.OrderId);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int n2))
                    maxNum = Math.Max(maxNum, n2);
            }

            int next = maxNum + 1;
            string date = DateTime.Today.ToString("yyyy-MM-dd");
            return ($"{date} - Order {next:D3}", $"O{next:D3}");
        }

        private void SnapToActiveRow()
        {
            if (_activeCamRowIndex < 0 || _activeCamRowIndex >= ordersDataGridView.Rows.Count) return;
            _lockingSelection = true;
            try { ordersDataGridView.CurrentCell = ordersDataGridView.Rows[_activeCamRowIndex].Cells[0]; }
            catch { }
            finally { _lockingSelection = false; }
        }

        private void ordersDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_lockingSelection) return;

            // Snap back to the active row while the machine is running
            if (!_editMode && IsRunningLocked())
            {
                SnapToActiveRow();
                return;
            }

            if (ordersDataGridView.SelectedRows.Count == 0) return;
            int idx = ordersDataGridView.SelectedRows[0].Index;
            if (idx < 0 || idx >= _camOrders.Count) return;
            var order = _camOrders[idx];
            PopulateProductsGrid(order);
            infoFileNameBox.Text  = order.FileName;
            infoCustomerBox.Text  = order.CustomerName;
            infoOrderIdBox.Text   = order.OrderId;
            infoQtyBox.Text       = order.Quantity.ToString();
            infoCompletedBox.Text = order.Completed.ToString();
            infoInfoTextBox.Text  = order.InfoText;

            if (_activeProductId != null && IsRunningLocked())
                HighlightActiveProduct(_activeProductId);
        }

        private void PopulateProductsGrid(CamOrder order)
        {
            productsDataGridView.Rows.Clear();
            foreach (var p in order.Products)
            {
                productsDataGridView.Rows.Add(
                    p.ListId, p.DisplayName,
                    p.MaterialId, p.MaterialThickness,
                    p.Quantity, p.RunQuantity, p.OperatorHint,
                    p.ProductId);   // hidden prodPathColumn
            }
        }

        private void productsDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (productsDataGridView.Columns[e.ColumnIndex].Name != "prodRunQtyColumn") return;

            if (!int.TryParse(e.FormattedValue?.ToString(), out int val) || val < 0)
            {
                e.Cancel = true;
                productsDataGridView.Rows[e.RowIndex].ErrorText = "Run Qty must be a whole number ≥ 0";
            }
            else
            {
                productsDataGridView.Rows[e.RowIndex].ErrorText = string.Empty;

                // Write back to the model so the value survives a re-selection.
                if (ordersDataGridView.SelectedRows.Count > 0)
                {
                    int orderIdx = ordersDataGridView.SelectedRows[0].Index;
                    if (orderIdx < _camOrders.Count && e.RowIndex < _camOrders[orderIdx].Products.Count)
                        _camOrders[orderIdx].Products[e.RowIndex].RunQuantity = val;
                }
            }
        }

        private void camFolderBrowseButton_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (Directory.Exists(camFolderTextBox.Text))
                dialog.SelectedPath = camFolderTextBox.Text;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                camFolderTextBox.Text = dialog.SelectedPath;
                SaveSettings();
                LoadCamFiles();
            }
        }

        private void camProductsBrowseButton_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (Directory.Exists(camProductsTextBox.Text))
                dialog.SelectedPath = camProductsTextBox.Text;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                camProductsTextBox.Text = dialog.SelectedPath;
                SaveSettings();
            }
        }

        private void newGroupButton_Click(object sender, EventArgs e)
        {
            EnterEditMode(null);
        }

        private void editGroupButton_Click(object sender, EventArgs e)
        {
            if (ordersDataGridView.SelectedRows.Count == 0)
            {
                DarkMessageBox.Show(this, "Please select a product group to edit.",
                    "Edit Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int idx = ordersDataGridView.SelectedRows[0].Index;
            EnterEditMode(_camOrders[idx]);
        }

        private void saveGroupButton_Click(object sender, EventArgs e)
        {
            string fileName = infoFileNameBox.Text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                DarkMessageBox.Show(this, "Please enter a file name.", "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string camFolder = camFolderTextBox.Text.Trim();
            if (string.IsNullOrEmpty(camFolder) || !Directory.Exists(camFolder))
            {
                DarkMessageBox.Show(this, "CAM folder not configured. Set it in Settings.",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(infoQtyBox.Text.Trim(), out int qty) || qty < 0) qty = 1;
            if (!int.TryParse(infoCompletedBox.Text.Trim(), out int completed) || completed < 0) completed = 0;

            XNamespace ns = "http://eu.schroedergroup.de/xml-schemas/P3CAMSchema.xsd";
            var root = new XElement(ns + "POS3000CAMData",
                new XAttribute("InfoText",     infoInfoTextBox.Text.Trim()),
                new XAttribute("OrderId",      infoOrderIdBox.Text.Trim()),
                new XAttribute("CustomerName", infoCustomerBox.Text.Trim()),
                new XAttribute("Quantity",     qty),
                new XAttribute("Completed",    completed));

            foreach (DataGridViewRow row in productsDataGridView.Rows)
            {
                string listId = row.Cells["prodListIdColumn"].Value?.ToString() ?? "";
                string prodId = row.Cells["prodPathColumn"].Value?.ToString() ?? "";
                string hint   = row.Cells["prodHintColumn"].Value?.ToString() ?? "";
                int.TryParse(row.Cells["prodQtyColumn"].Value?.ToString(), out int rowQty);
                if (rowQty <= 0) rowQty = 1;
                root.Add(new XElement(ns + "Product",
                    new XAttribute("ListId",       listId),
                    new XAttribute("ProductId",    prodId),
                    new XAttribute("Quantity",     rowQty),
                    new XAttribute("Completed",    0),
                    new XAttribute("LoadError",    0),
                    new XAttribute("InfoText",     ""),
                    new XAttribute("DetailsHtml",  ""),
                    new XAttribute("OperatorHint", hint),
                    new XAttribute("UserData",     "")));
            }

            if (!fileName.EndsWith(".p3cam", StringComparison.OrdinalIgnoreCase))
                fileName += ".p3cam";

            string savePath;
            if (_editingFilePath != null &&
                string.Equals(Path.GetFileNameWithoutExtension(_editingFilePath),
                              Path.GetFileNameWithoutExtension(fileName),
                              StringComparison.OrdinalIgnoreCase))
            {
                savePath = _editingFilePath;
            }
            else
            {
                savePath = Path.Combine(camFolder, fileName);
                if (File.Exists(savePath) && savePath != _editingFilePath)
                {
                    if (DarkMessageBox.Show(this, $"'{fileName}' already exists. Overwrite?",
                            "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                }
            }

            try
            {
                new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(savePath);
                ExitEditMode(false);
                LoadCamFiles(savePath);
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this, "Failed to save:\n\n" + ex.Message,
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelEditButton_Click(object sender, EventArgs e)
        {
            ExitEditMode(true);
        }

        private void addProductsButton_Click(object sender, EventArgs e)
        {
            string prodFolder = camProductsTextBox.Text.Trim();
            if (string.IsNullOrEmpty(prodFolder) || !Directory.Exists(prodFolder))
            {
                DarkMessageBox.Show(this,
                    "Please configure the CAM Products Folder in Settings first.",
                    "Add Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var dlg = new OpenFileDialog
            {
                Title = "Select product .zip files",
                InitialDirectory = prodFolder,
                Filter = "CAM Product files (*.zip)|*.zip",
                Multiselect = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string prefix = camProductPrefixTextBox.Text;
            // Ensure prefix ends with a backslash
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('\\'))
                prefix += '\\';

            foreach (string path in dlg.FileNames)
            {
                // Path relative to the products folder, preserving any subdirectory structure
                string relative = Path.GetRelativePath(prodFolder, path);
                string listId   = Path.GetFileNameWithoutExtension(path);
                string prodId   = prefix + relative;
                bool dup = productsDataGridView.Rows.Cast<DataGridViewRow>()
                    .Any(r => r.Cells["prodListIdColumn"].Value?.ToString() == listId);
                if (!dup)
                    productsDataGridView.Rows.Add(listId, listId, "", "", 1, 1, "", prodId);
            }
        }

        private void removeProductsButton_Click(object sender, EventArgs e)
        {
            var toRemove = productsDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .OrderByDescending(r => r.Index).ToList();
            foreach (var row in toRemove)
                productsDataGridView.Rows.Remove(row);
        }

        private void deleteGroupButton_Click(object sender, EventArgs e)
        {
            if (ordersDataGridView.SelectedRows.Count == 0)
            {
                DarkMessageBox.Show(this, "Please select a product group to delete.",
                    "Delete Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int idx   = ordersDataGridView.SelectedRows[0].Index;
            var order = _camOrders[idx];
            var ans   = DarkMessageBox.Show(this,
                $"Delete '{order.FileName}'?\n\nThis cannot be undone.",
                "Delete Group", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ans != DialogResult.Yes) return;
            try
            {
                File.Delete(order.FilePath);
                LoadCamFiles();
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this, "Failed to delete:\n\n" + ex.Message,
                    "Delete Group", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void camOutputBrowseButton_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (Directory.Exists(camOutputTextBox.Text))
                dialog.SelectedPath = camOutputTextBox.Text;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                camOutputTextBox.Text = dialog.SelectedPath;
                SaveSettings();
            }
        }

        private void runGroupButton_Click(object sender, EventArgs e)
        {
            if (ordersDataGridView.SelectedRows.Count == 0)
            {
                DarkMessageBox.Show(this,"Please select a product group first.", "Run Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string outputBase = camOutputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(outputBase))
            {
                DarkMessageBox.Show(this,"Please set the CAM Output Directory in Settings first.", "Run Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idx = ordersDataGridView.SelectedRows[0].Index;
            var order = _camOrders[idx];

            try
            {
                string inDir = Path.Combine(outputBase, "in");
                Directory.CreateDirectory(inDir);
                string dest = Path.Combine(inDir, Path.GetFileName(order.FilePath));
                File.Copy(order.FilePath, dest, overwrite: true);

                DarkMessageBox.Show(this,$"Copied '{Path.GetFileName(order.FilePath)}' to:\n{inDir}",
                    "Run Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this,"Failed to copy CAM file:\n\n" + ex.Message, "Run Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelGroupButton_Click(object sender, EventArgs e)
        {
            string outputBase = camOutputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(outputBase))
            {
                DarkMessageBox.Show(this, "Please set the CAM Output Directory in Settings first.", "Cancel Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsRunningLocked())
            {
                // Machine is actively processing — delete the file from \processing\
                string processingDir = Path.Combine(outputBase, "processing");
                string fileName      = Path.GetFileName(_activeCamFileName);
                string processingPath = Path.Combine(processingDir, fileName);

                if (!File.Exists(processingPath))
                {
                    DarkMessageBox.Show(this, $"'{fileName}' was not found in the 'processing' folder.",
                        "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var ans = DarkMessageBox.Show(this,
                    $"Delete '{fileName}' from the processing folder?\n\nThis will interrupt the current run.",
                    "Cancel Group", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ans != DialogResult.Yes) return;

                try
                {
                    string cancelDir = Path.Combine(outputBase, "canceled");
                    Directory.CreateDirectory(cancelDir);
                    File.Move(processingPath, Path.Combine(cancelDir, fileName), overwrite: true);
                    DarkMessageBox.Show(this, $"Moved '{fileName}' to:\n{cancelDir}",
                        "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    DarkMessageBox.Show(this, "Failed to move CAM file:\n\n" + ex.Message, "Cancel Group",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Normal (not running): move selected group's file out of \in\
            if (ordersDataGridView.SelectedRows.Count == 0)
            {
                DarkMessageBox.Show(this, "Please select a product group first.", "Cancel Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = ordersDataGridView.SelectedRows[0].Index;
            var order = _camOrders[idx];
            string inFileName = Path.GetFileName(order.FilePath);

            string inDir       = Path.Combine(outputBase, "in");
            string canceledDir = Path.Combine(outputBase, "canceled");
            string inPath      = Path.Combine(inDir, inFileName);

            if (!File.Exists(inPath))
            {
                DarkMessageBox.Show(this, $"'{inFileName}' is not currently in the 'in' folder — nothing to cancel.",
                    "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Directory.CreateDirectory(canceledDir);
                string dest = Path.Combine(canceledDir, inFileName);
                File.Move(inPath, dest, overwrite: true);

                DarkMessageBox.Show(this, $"Moved '{inFileName}' to:\n{canceledDir}",
                    "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this, "Failed to move CAM file:\n\n" + ex.Message, "Cancel Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            string savedFolder = Properties.Settings.Default.PdfFolderPath;
            if (!string.IsNullOrWhiteSpace(savedFolder))
                pdfFolderTextBox.Text = savedFolder;

            string savedUrl = Properties.Settings.Default.EndpointUrl;
            if (!string.IsNullOrWhiteSpace(savedUrl))
                endpointTextBox.Text = savedUrl;

            string savedCamFolder = Properties.Settings.Default.CamFolderPath;
            if (!string.IsNullOrWhiteSpace(savedCamFolder))
                camFolderTextBox.Text = savedCamFolder;

            string savedCamOutput = Properties.Settings.Default.CamOutputPath;
            if (!string.IsNullOrWhiteSpace(savedCamOutput))
                camOutputTextBox.Text = savedCamOutput;

            string savedCamProducts = Properties.Settings.Default.CamProductsPath;
            if (!string.IsNullOrWhiteSpace(savedCamProducts))
                camProductsTextBox.Text = savedCamProducts;

            camProductPrefixTextBox.Text = Properties.Settings.Default.ProductPathPrefix;

            keyboardToggle.Checked = Properties.Settings.Default.KeyboardEnabled;

            RestoreWindowPlacement();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PdfFolderPath  = pdfFolderTextBox.Text.Trim();
            Properties.Settings.Default.EndpointUrl    = endpointTextBox.Text.Trim();
            Properties.Settings.Default.CamFolderPath  = camFolderTextBox.Text.Trim();
            Properties.Settings.Default.CamOutputPath    = camOutputTextBox.Text.Trim();
            Properties.Settings.Default.CamProductsPath    = camProductsTextBox.Text.Trim();
            Properties.Settings.Default.ProductPathPrefix  = camProductPrefixTextBox.Text;
            Properties.Settings.Default.KeyboardEnabled = keyboardToggle.Checked;
            SaveWindowPlacement();
            Properties.Settings.Default.Save();
        }

        private void RestoreWindowPlacement()
        {
            var s = Properties.Settings.Default;
            if (s.WindowLeft == -1) return; // first run — use designer defaults

            var saved = new System.Drawing.Rectangle(s.WindowLeft, s.WindowTop, s.WindowWidth, s.WindowHeight);

            // Only restore if the saved bounds overlap at least one connected screen.
            bool onScreen = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(saved)) { onScreen = true; break; }
            }

            if (onScreen)
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = saved;
            }

            if (System.Enum.TryParse(s.WindowState, out FormWindowState state) && state == FormWindowState.Maximized)
                SetFullscreen(true);

            fullscreenToggle.Checked = (WindowState == FormWindowState.Maximized);
        }

        private void fullscreenToggle_CheckedChanged(object sender, EventArgs e)
        {
            SetFullscreen(fullscreenToggle.Checked);
        }

        private void SetFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
            }
        }

        private void SaveWindowPlacement()
        {
            var s = Properties.Settings.Default;
            s.WindowState = WindowState.ToString();

            // Save the restored (non-maximised) bounds so we know where to place the window next time.
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            s.WindowLeft   = bounds.Left;
            s.WindowTop    = bounds.Top;
            s.WindowWidth  = bounds.Width;
            s.WindowHeight = bounds.Height;
        }

        private void pdfBrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (Directory.Exists(pdfFolderTextBox.Text))
                    dialog.SelectedPath = pdfFolderTextBox.Text;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    pdfFolderTextBox.Text = dialog.SelectedPath;
                    SaveSettings();

                    // Re-trigger a load attempt with the new folder if we already have a product id.
                    if (!string.IsNullOrEmpty(lastLoadedProductId))
                    {
                        string productId = lastLoadedProductId;
                        lastLoadedProductId = null;
                        OpenProductPdf(productId);
                    }
                }
            }
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            if (_autoRetrying)
            {
                cts?.Cancel();
                connectButton.Enabled   = false;
                endpointTextBox.Enabled = true;
                UpdateStatus("Cancelling...");
                return;
            }

            if (session != null)
            {
                DisconnectClient();
                connectButton.Text = "Connect";
                return;
            }

            string endpointUrl = endpointTextBox.Text.Trim();
            if (string.IsNullOrEmpty(endpointUrl))
            {
                DarkMessageBox.Show(this,"Please enter an endpoint URL.", "OPC UA Client",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            connectButton.Enabled = false;
            endpointTextBox.Enabled = false;
            UpdateStatus("Connecting...");
            SaveSettings();

            cts = new CancellationTokenSource();
            await StartClient(endpointUrl, cts.Token);

            connectButton.Enabled = true;
            connectButton.Text = session != null ? "Disconnect" : "Connect";
            endpointTextBox.Enabled = session == null;
        }

        private async Task StartClient(string endpointUrl, CancellationToken cancellationToken, bool silent = false)
        {
            try
            {
                var config = BuildApplicationConfiguration();
                await config.Validate(ApplicationType.Client);

                // NOTE: accepting all certs is fine for a lab/dev network, but don't ship this
                // to anything internet-facing. Swap for proper trust-list handling in production.
                config.CertificateValidator.CertificateValidation += (s, ce) => { ce.Accept = true; };

                // SelectEndpoint is a synchronous network probe that ignores cancellation internally.
                // Race it against the token so Cancel responds immediately; the probe is abandoned.
                var probeTask = Task.Run(
                    () => CoreClientUtils.SelectEndpoint(config, endpointUrl, useSecurity: false));
                await Task.WhenAny(probeTask, Task.Delay(Timeout.Infinite, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
                var endpointDescription = await probeTask;

                var configuredEndpoint = new ConfiguredEndpoint(
                    null,
                    endpointDescription,
                    EndpointConfiguration.Create(config));

                session = await new DefaultSessionFactory().CreateAsync(
                    config,
                    configuredEndpoint,
                    updateBeforeConnect: false,
                    sessionName: "Session",
                    sessionTimeout: 60000,
                    identity: new UserIdentity(),
                    preferredLocales: null,
                    ct: cancellationToken);

                UpdateStatus($"Connected to {endpointDescription.EndpointUrl} — browsing...");

                // Browse and subscribe are also synchronous OPC UA calls — keep off the UI thread.
                await Task.Run(() =>
                {
                    PopulateMonitoredNodesFromServer();
                    CreateSubscription();
                }, cancellationToken);

                UpdateStatus(monitoredNodes.Count > 0
                    ? $"Connected to {endpointDescription.EndpointUrl} ({monitoredNodes.Count} items)"
                    : $"Connected to {endpointDescription.EndpointUrl} — no items found at the configured path");
            }
            catch (OperationCanceledException)
            {
                // form closed/cancelled during connect — nothing to report
            }
            catch (Exception ex)
            {
                string brief = ex.InnerException?.Message ?? ex.Message;
                UpdateStatus("Not connected");
                if (!silent)
                    DarkMessageBox.Show(this, $"Could not connect to the OPC UA server.\n\n{brief}",
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static ApplicationConfiguration BuildApplicationConfiguration()
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "OPC UA Client",
                ApplicationType = ApplicationType.Client,

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = CertificateStoreType.Directory,
                        StorePath = "CertificateStores/UA_MachineDefault",
                        SubjectName = "CN=OPC UA Client"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = CertificateStoreType.Directory,
                        StorePath = "CertificateStores/UA Applications"
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = CertificateStoreType.Directory,
                        StorePath = "CertificateStores/UA Certificate Authorities"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = CertificateStoreType.Directory,
                        StorePath = "CertificateStores/RejectedCertificates"
                    },
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false
                },

                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration(),

                TraceConfiguration = new TraceConfiguration
                {
                    OutputFilePath = "opcua.log",
                    TraceMasks = Utils.TraceMasks.All
                }
            };

            config.TraceConfiguration.ApplySettings();
            return config;
        }

        /// <summary>
        /// Resolves MonitoringFolderPath to a NodeId and browses its children.
        /// Safe to call from a background thread — does not touch UI controls.
        /// </summary>
        private void PopulateMonitoredNodesFromServer()
        {
            try
            {
                NodeId folderNodeId = ResolveBrowsePath(MonitoringFolderPath);
                monitoredNodes = BrowseVariables(folderNodeId);
            }
            catch (Exception ex)
            {
                monitoredNodes = new List<(string Name, NodeId NodeId)>();
                this.Invoke(new Action(() =>
                    DarkMessageBox.Show(this, "Failed to browse the monitoring folder.\n\n" + (ex.InnerException?.Message ?? ex.Message),
                        "OPC UA Client", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }

            var nodes = monitoredNodes;
            this.Invoke(new Action(() =>
            {
                dataGridView1.Rows.Clear();
                rowsByClientHandle.Clear();
                foreach (var node in nodes)
                    dataGridView1.Rows.Add(node.Name, node.NodeId.ToString(), "-", "-", "-");
            }));
        }

        /// <summary>
        /// Walks a relative path of "ns:BrowseName" segments starting at the Objects folder
        /// and returns the NodeId of the final segment.
        /// </summary>
        private NodeId ResolveBrowsePath(string relativePath)
        {
            string[] segments = relativePath.Split('/');
            var elements = new RelativePathElementCollection();

            foreach (string segment in segments)
            {
                int colonIndex = segment.IndexOf(':');
                if (colonIndex < 0)
                    throw new FormatException($"Invalid path segment '{segment}'. Expected format 'namespaceIndex:BrowseName'.");

                ushort namespaceIndex = ushort.Parse(segment.Substring(0, colonIndex));
                string browseName = segment.Substring(colonIndex + 1);

                elements.Add(new RelativePathElement
                {
                    ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                    IsInverse = false,
                    IncludeSubtypes = true,
                    TargetName = new QualifiedName(browseName, namespaceIndex)
                });
            }

            var browsePaths = new BrowsePathCollection
            {
                new BrowsePath
                {
                    StartingNode = ObjectIds.ObjectsFolder,
                    RelativePath = new RelativePath { Elements = elements }
                }
            };

            // TranslateBrowsePathsToNodeIds is a core, synchronous service call. Some library
            // versions flag it [Obsolete] in favor of an async overload — safe to ignore for now.
#pragma warning disable CS0618
            session.TranslateBrowsePathsToNodeIds(null, browsePaths, out BrowsePathResultCollection results, out _);
#pragma warning restore CS0618

            if (results == null || results.Count == 0 || StatusCode.IsBad(results[0].StatusCode) || results[0].Targets.Count == 0)
            {
                string statusText = results?.Count > 0 ? results[0].StatusCode.ToString() : "no result";
                throw new Exception($"Could not resolve path '/Root/Objects/{relativePath}' ({statusText}).");
            }

            return ExpandedNodeId.ToNodeId(results[0].Targets[0].TargetId, session.NamespaceUris);
        }

        /// <summary>
        /// Browses the direct children of a folder and returns every Variable node found.
        /// </summary>
        private List<(string Name, NodeId NodeId)> BrowseVariables(NodeId folderNodeId)
        {
            var found = new List<(string Name, NodeId NodeId)>();

#pragma warning disable CS0618
            session.Browse(
                null, null, folderNodeId, 0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable,
                out byte[] continuationPoint,
                out ReferenceDescriptionCollection references);
#pragma warning restore CS0618

            foreach (var reference in references)
            {
                var nodeId = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);
                string name = !string.IsNullOrEmpty(reference.DisplayName?.Text)
                    ? reference.DisplayName.Text
                    : reference.BrowseName.Name;
                found.Add((name, nodeId));
            }

            return found;
        }

        private void CreateSubscription()
        {
            if (session == null || monitoredNodes.Count == 0)
                return;

            productIdClientHandle    = null;
            camFileClientHandle      = null;
            machineStateClientHandle = null;

            try
            {
                subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = 500 };

                for (int i = 0; i < monitoredNodes.Count; i++)
                {
                    var node = monitoredNodes[i];

                    var item = new MonitoredItem(subscription.DefaultItem)
                    {
                        DisplayName = node.Name,
                        StartNodeId = node.NodeId,
                        AttributeId = Attributes.Value,
                        SamplingInterval = 500
                    };

                    item.Notification += OnValueChanged;
                    subscription.AddItem(item);

                    // ClientHandle isn't assigned until the item is added to the subscription,
                    // so map it here before Create() so notifications can be matched to rows.
                    rowsByClientHandle[item.ClientHandle] = dataGridView1.Rows[i];

                    if (node.Name.IndexOf(ProductIdNameMatch, StringComparison.OrdinalIgnoreCase) >= 0)
                        productIdClientHandle = item.ClientHandle;

                    if (node.Name.IndexOf("CAMFileInProcess", StringComparison.OrdinalIgnoreCase) >= 0)
                        camFileClientHandle = item.ClientHandle;

                    if (node.Name.IndexOf("CurrentMachineState", StringComparison.OrdinalIgnoreCase) >= 0)
                        machineStateClientHandle = item.ClientHandle;
                }

                // Extra hardcoded node — not in the browsed monitoring list
                operatorActionClientHandle = null;
                var opActionItem = new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName      = "OperatorActionRequested",
                    StartNodeId      = new NodeId(OperatorActionNodeId, OperatorActionNamespaceIndex),
                    AttributeId      = Attributes.Value,
                    SamplingInterval = 500
                };
                opActionItem.Notification += OnValueChanged;
                subscription.AddItem(opActionItem);
                operatorActionClientHandle = opActionItem.ClientHandle;

                // Statistics nodes
                statsAutoModeHandle       = AddStatsItem(subscription, StatsAutoModeNodeId);
                statsManualModeHandle     = AddStatsItem(subscription, StatsManualModeNodeId);
                statsSetupModeHandle      = AddStatsItem(subscription, StatsSetupModeNodeId);
                statsTotalHoursHandle     = AddStatsItem(subscription, StatsTotalHoursNodeId);
                statsProducingHoursHandle = AddStatsItem(subscription, StatsProducingHoursNodeId);
                statsPartCountHandle      = AddStatsItem(subscription, StatsPartCountNodeId);
                statsCurrentStepHandle    = AddStatsItem(subscription, StatsCurrentStepNodeId);
                statsBendingNowHandle     = AddStatsItem(subscription, StatsBendingNowNodeId);

                session.AddSubscription(subscription);
                subscription.Create();

                if (productIdClientHandle == null)
                    SetPdfStatus($"No monitored item matching '{ProductIdNameMatch}' was found.");
            }
            catch (Exception ex)
            {
                UpdateStatus("Subscription failed");
                DarkMessageBox.Show(this, "Failed to create data subscription.\n\n" + (ex.InnerException?.Message ?? ex.Message),
                    "OPC UA Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private uint? AddStatsItem(Subscription sub, string nodePath)
        {
            try
            {
                var it = new MonitoredItem(sub.DefaultItem)
                {
                    DisplayName      = nodePath.Split('.').Last(),
                    StartNodeId      = new NodeId(nodePath, OperatorActionNamespaceIndex),
                    AttributeId      = Attributes.Value,
                    SamplingInterval = 500,
                };
                it.Notification += OnValueChanged;
                sub.AddItem(it);
                return it.ClientHandle;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stats node subscribe failed '{nodePath}': {ex.Message}");
                return null;
            }
        }

        private void OnValueChanged(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var v in item.DequeueValues())
            {
                UpdateRow(item.ClientHandle, v);

                string strVal = v.Value?.ToString() ?? "";

                if (productIdClientHandle.HasValue && item.ClientHandle == productIdClientHandle.Value)
                {
                    OpenProductPdf(strVal);
                    BeginInvokeIfRequired(() =>
                    {
                        bool newPart = strVal != _activeProductId && !string.IsNullOrEmpty(strVal);
                        _activeProductId = strVal;
                        _statsPage.SetActiveProduct(StatsStore.ProductKey(strVal));
                        if (newPart && _machineState == 3)
                            ArmSetupTimer();
                        ApplyRunningLockState();
                    });
                }
                else if (camFileClientHandle.HasValue && item.ClientHandle == camFileClientHandle.Value)
                {
                    BeginInvokeIfRequired(() => OnCamFileChanged(strVal));
                }
                else if (machineStateClientHandle.HasValue && item.ClientHandle == machineStateClientHandle.Value)
                {
                    if (int.TryParse(strVal, out int state))
                        BeginInvokeIfRequired(() => OnMachineStateChanged(state));
                }
                else if (operatorActionClientHandle.HasValue && item.ClientHandle == operatorActionClientHandle.Value)
                {
                    bool waiting = v.Value is bool b ? b : v.Value is int wi ? wi != 0 : strVal == "True" || strVal == "1";
                    BeginInvokeIfRequired(() => { UpdateOperatorActionLabel(waiting); _statsPage.UpdateOperatorWait(waiting); });
                }
                else
                {
                    RouteStatsValue(item.ClientHandle, v.Value, strVal);
                }
            }
        }

        private void RouteStatsValue(uint handle, object rawVal, string strVal)
        {
            bool   AsBool(object v)   => v is bool b ? b : v is int i ? i != 0 : strVal == "True" || strVal == "1";
            double AsDouble(object v) => v is double d ? d : v is float f ? f : double.TryParse(strVal, out double r) ? r : 0;
            int    AsInt(object v)    => v is int i ? i : int.TryParse(strVal, out int r) ? r : 0;

            if      (statsAutoModeHandle.HasValue       && handle == statsAutoModeHandle.Value)
                BeginInvokeIfRequired(() => _statsPage.UpdateAutoMode(AsBool(rawVal)));
            else if (statsManualModeHandle.HasValue     && handle == statsManualModeHandle.Value)
                BeginInvokeIfRequired(() => _statsPage.UpdateManualMode(AsBool(rawVal)));
            else if (statsSetupModeHandle.HasValue      && handle == statsSetupModeHandle.Value)
                BeginInvokeIfRequired(() => _statsPage.UpdateSetupMode(AsBool(rawVal)));
            else if (statsBendingNowHandle.HasValue     && handle == statsBendingNowHandle.Value)
            {
                bool bending = AsBool(rawVal);
                BeginInvokeIfRequired(() => OnBendingNowChanged(bending));
            }
            else if (statsTotalHoursHandle.HasValue     && handle == statsTotalHoursHandle.Value)
            {
                double total = AsDouble(rawVal);
                // defer until both values are potentially known; reuse last producing value via closure capture
                BeginInvokeIfRequired(() => _statsPage.UpdateHours(total, _statsLastProducingHours));
                _statsLastTotalHours = total;
            }
            else if (statsProducingHoursHandle.HasValue && handle == statsProducingHoursHandle.Value)
            {
                double producing = AsDouble(rawVal);
                BeginInvokeIfRequired(() => _statsPage.UpdateHours(_statsLastTotalHours, producing));
                _statsLastProducingHours = producing;
            }
            else if (statsPartCountHandle.HasValue      && handle == statsPartCountHandle.Value)
            {
                int count = AsInt(rawVal);
                BeginInvokeIfRequired(() => OnPartCountChanged(count));
            }
            else if (statsCurrentStepHandle.HasValue    && handle == statsCurrentStepHandle.Value)
            {
                BeginInvokeIfRequired(() => OnProductionStepChanged(strVal));
            }
        }

        private double _statsLastTotalHours;
        private double _statsLastProducingHours;

        private void BeginInvokeIfRequired(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        private void OnCamFileChanged(string camFileName)
        {
            _activeCamFileName = camFileName;
            _statsPage.SetJob(StatsStore.JobKey(camFileName));
            ApplyRunningLockState();
        }

        private void HighlightActiveProduct(string productId)
        {
            _activeProductId = productId;
            bool hasActive = !string.IsNullOrEmpty(productId);

            _productRowStates.Clear();
            for (int i = 0; i < productsDataGridView.Rows.Count; i++)
            {
                var row = productsDataGridView.Rows[i];
                string rowPath = row.Cells["prodPathColumn"].Value?.ToString() ?? "";
                string rowId   = row.Cells["prodListIdColumn"].Value?.ToString() ?? "";
                bool match = hasActive &&
                             (string.Equals(rowPath, productId, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(rowId,   productId, StringComparison.OrdinalIgnoreCase) ||
                              rowPath.IndexOf(productId, StringComparison.OrdinalIgnoreCase) >= 0 ||
                              productId.IndexOf(rowId,   StringComparison.OrdinalIgnoreCase) >= 0);
                _productRowStates[i] = match ? RowState.Active : RowState.Dim;
            }

            // If nothing matched, don't grey everything — just leave normal
            if (!_productRowStates.Values.Any(s => s == RowState.Active))
                _productRowStates.Clear();

            productsDataGridView.Invalidate();
        }

        private void OrdersGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!_orderRowStates.TryGetValue(e.RowIndex, out var state)) return;
            switch (state)
            {
                case RowState.Active:
                    e.CellStyle.BackColor          = RowActiveBg;
                    e.CellStyle.ForeColor          = RowActiveFg;
                    e.CellStyle.SelectionBackColor = RowActiveSelBg;
                    e.CellStyle.SelectionForeColor = RowActiveFg;
                    break;
                case RowState.Dim:
                    e.CellStyle.BackColor          = RowDimBg;
                    e.CellStyle.ForeColor          = RowDimFg;
                    e.CellStyle.SelectionBackColor = RowDimBg;
                    e.CellStyle.SelectionForeColor = RowDimFg;
                    break;
            }
        }

        private void ProductsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!_productRowStates.TryGetValue(e.RowIndex, out var state)) return;
            switch (state)
            {
                case RowState.Active:
                    e.CellStyle.BackColor          = RowActiveProductBg;
                    e.CellStyle.ForeColor          = RowActiveProductFg;
                    e.CellStyle.SelectionBackColor = RowActiveProductBg;
                    e.CellStyle.SelectionForeColor = RowActiveProductFg;
                    break;
                case RowState.Dim:
                    e.CellStyle.BackColor          = RowDimBg;
                    e.CellStyle.ForeColor          = RowDimFg;
                    e.CellStyle.SelectionBackColor = RowDimBg;
                    e.CellStyle.SelectionForeColor = RowDimFg;
                    break;
            }
        }

        private void OnMachineStateChanged(int state)
        {
            _machineState = state;
            if (state == 3)
            {
                // Arm setup timer when entering state 3, but only if we're not already mid-cycle.
                // This handles: connect while running, or machine enters state 3 after connect.
                if (!_waitingForFirstBend && !_firstBendHappened)
                    ArmSetupTimer();
            }
            else
            {
                _waitingForFirstBend = false;
                _firstBendHappened   = false;
                _lastProductionStep  = -1;
                _statsPage.StopLiveTimers();
            }
            ApplyRunningLockState();
        }

        // Arms the setup timer — called on step→0 or product-id change.
        private void ArmSetupTimer()
        {
            _setupStartTime      = DateTime.UtcNow;
            _waitingForFirstBend = true;
            _firstBendHappened   = false;
            _firstBendTime       = DateTime.MinValue;
            _statsPage.StartSetupTimer(_setupStartTime);
        }

        private void OnBendingNowChanged(bool bending)
        {
            _statsPage.UpdateBendingNow(bending);

            if (bending && !_isBendingNow && _machineState == 3)
            {
                _statsStore.IncrementBendCount();
                _statsPage.UpdateTotalBends(_statsStore.TotalBendCount);
            }

            if (bending && !_isBendingNow && _waitingForFirstBend && _setupStartTime != DateTime.MinValue)
            {
                // First bend — close setup phase, open bending phase
                double setup      = (DateTime.UtcNow - _setupStartTime).TotalSeconds;
                string productKey = StatsStore.ProductKey(_activeProductId);
                _statsStore.AddSetupTime(StatsStore.JobKey(_activeCamFileName), productKey, setup);
                _statsPage.UpdateSetupTime(productKey, setup);
                _waitingForFirstBend = false;
                _firstBendTime       = DateTime.UtcNow;
                _firstBendHappened   = true;
                _statsPage.StartPartTimer(_firstBendTime);
            }

            _isBendingNow = bending;
        }

        private void OnPartCountChanged(int count)
        {
            _statsPage.UpdatePartCount(count);
        }

        private void OnProductionStepChanged(string step)
        {
            _statsPage.UpdateProductionStep(step);

            if (!int.TryParse(step, out int stepNum)) return;

            _lastProductionStep = stepNum;

            if (_machineState != 3) return;
            if (stepNum != 0) return;

            // Step reset to 0 — close bending phase and record times
            if (_firstBendHappened && _firstBendTime != DateTime.MinValue)
            {
                string productKey = StatsStore.ProductKey(_activeProductId);
                double bendTime   = (DateTime.UtcNow - _firstBendTime).TotalSeconds;
                _statsStore.AddBendingTime(StatsStore.JobKey(_activeCamFileName), productKey, bendTime);
                _statsPage.UpdateBendingTime(productKey, bendTime);

                if (_setupStartTime != DateTime.MinValue)
                {
                    double totalTime = (DateTime.UtcNow - _setupStartTime).TotalSeconds;
                    _statsPage.UpdateLastCycleTime(productKey, totalTime);
                }
            }

            // Arm setup timer for the next part
            ArmSetupTimer();
        }

        /// <summary>
        /// Single source of truth for the "machine running" locked state.
        /// Locked = machineState==3 AND a CAM file is active.
        /// </summary>
        private void ApplyRunningLockState()
        {
            bool locked    = IsRunningLocked();
            bool machineOn = _machineState == 3;

            newGroupButton.Visible        = !machineOn;
            editGroupButton.Visible       = !machineOn;
            deleteGroupButton.Visible     = !machineOn;

            runGroupButton.Enabled        = !locked;
            cancelGroupButton.Enabled     = true;
            runGroupButton.BackColor      = locked
                ? System.Drawing.Color.FromArgb(25, 80, 25)
                : System.Drawing.Color.FromArgb(34, 139, 34);
            cancelGroupButton.BackColor   = System.Drawing.Color.FromArgb(180, 50, 50);

            _operatorActionLabel.Visible  = machineOn;

            _orderRowStates.Clear();
            _productRowStates.Clear();

            if (locked)
            {
                int matchIdx = -1;
                for (int i = 0; i < ordersDataGridView.Rows.Count; i++)
                {
                    string rowFile = ordersDataGridView.Rows[i].Cells["ordersCustomerColumn"].Value?.ToString() ?? "";
                    bool match = FileNamesMatch(rowFile, _activeCamFileName);
                    _orderRowStates[i] = match ? RowState.Active : RowState.Dim;
                    if (match) matchIdx = i;
                }
                _activeCamRowIndex = matchIdx;

                if (matchIdx >= 0 && matchIdx < _camOrders.Count)
                {
                    _lockingSelection = true;
                    try
                    {
                        ordersDataGridView.CurrentCell = ordersDataGridView.Rows[matchIdx].Cells[0];
                        ordersDataGridView.FirstDisplayedScrollingRowIndex = matchIdx;
                    }
                    catch { }
                    finally { _lockingSelection = false; }

                    // Load the matching order's products so the right group is shown
                    var order = _camOrders[matchIdx];
                    PopulateProductsGrid(order);
                    infoFileNameBox.Text  = order.FileName;
                    infoCustomerBox.Text  = order.CustomerName;
                    infoOrderIdBox.Text   = order.OrderId;
                    infoQtyBox.Text       = order.Quantity.ToString();
                    infoCompletedBox.Text = order.Completed.ToString();
                    infoInfoTextBox.Text  = order.InfoText;
                }

                HighlightActiveProduct(_activeProductId);
            }
            else
            {
                _activeCamRowIndex = -1;
            }

            ordersDataGridView.Invalidate();
            productsDataGridView.Invalidate();
        }


        private bool IsRunningLocked() =>
            _machineState == 3 && !string.IsNullOrEmpty(_activeCamFileName);

        private static bool FileNamesMatch(string a, string b) =>
            string.Equals(Path.GetFileNameWithoutExtension(a),
                          Path.GetFileNameWithoutExtension(b),
                          StringComparison.OrdinalIgnoreCase);

        private static System.Drawing.Color Lighten(System.Drawing.Color c, int amt) =>
            System.Drawing.Color.FromArgb(
                Math.Min(255, c.R + amt),
                Math.Min(255, c.G + amt),
                Math.Min(255, c.B + amt));

        private void UpdateRow(uint clientHandle, DataValue value)
        {
            if (!rowsByClientHandle.TryGetValue(clientHandle, out var row) || dataGridView1.IsDisposed)
                return;

            void Apply()
            {
                row.Cells[2].Value = value.Value?.ToString() ?? "(null)";
                row.Cells[3].Value = value.StatusCode.ToString();
                row.Cells[4].Value = value.SourceTimestamp != DateTime.MinValue
                    ? value.SourceTimestamp.ToLocalTime().ToString("HH:mm:ss.fff")
                    : DateTime.Now.ToString("HH:mm:ss.fff");
            }

            if (dataGridView1.InvokeRequired)
                dataGridView1.Invoke(new Action(Apply));
            else
                Apply();
        }

        /// <summary>
        /// Looks for "{productId}.pdf" in the configured folder and loads it into the viewer.
        /// </summary>
        private void OpenProductPdf(string productId)
        {
            if (docViewer.IsDisposed)
                return;

            if (docViewer.InvokeRequired)
            {
                docViewer.Invoke(new Action(() => OpenProductPdf(productId)));
                return;
            }

            if (string.IsNullOrWhiteSpace(productId))
            {
                if (lastLoadedProductId != null)
                {
                    ClearPdfView("Waiting for a product id...");
                    lastLoadedProductId = null;
                }
                return;
            }

            if (productId == lastLoadedProductId)
                return;

            lastLoadedProductId = productId;

            string folder = pdfFolderTextBox.Text.Trim();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                ClearPdfView($"PDF folder not found: {folder}");
                return;
            }

            string fileName = SanitizeFileName(productId) + ".pdf";
            string fullPath = Path.Combine(folder, fileName);

            if (!File.Exists(fullPath))
            {
                ClearPdfView($"No PDF found for product '{productId}' (expected {fullPath}).");
                return;
            }

            try
            {
                var uri = new Uri(fullPath).AbsoluteUri;
                docViewer.CoreWebView2?.Navigate(uri);
                if (docViewer.CoreWebView2 == null)
                {
                    docViewer.CoreWebView2InitializationCompleted += OnCoreReady;
                    _ = docViewer.EnsureCoreWebView2Async();
                    void OnCoreReady(object s, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
                    {
                        docViewer.CoreWebView2InitializationCompleted -= OnCoreReady;
                        if (e.IsSuccess) docViewer.CoreWebView2.Navigate(new Uri(fullPath).AbsoluteUri);
                    }
                }
                SetPdfStatus($"Showing: {fileName}");
            }
            catch (Exception ex)
            {
                ClearPdfView("Failed to open PDF: " + ex.Message);
            }
        }

        private void ShowWebView2MissingPanel()
        {
            if (documentPanel.InvokeRequired)
            {
                documentPanel.Invoke(new Action(ShowWebView2MissingPanel));
                return;
            }

            docViewer.Visible = false;

            var panel = new System.Windows.Forms.Panel
            {
                Dock    = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(24),
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
            };

            var label = new System.Windows.Forms.Label
            {
                Text      = "Microsoft Edge WebView2 Runtime is required to display documents.\nPlease download and install it, then restart the application.",
                ForeColor = System.Drawing.Color.FromArgb(200, 200, 200),
                AutoSize  = false,
                Dock      = System.Windows.Forms.DockStyle.Top,
                Height    = 60,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            };

            var link = new System.Windows.Forms.LinkLabel
            {
                Text      = "Download WebView2 Runtime",
                AutoSize  = true,
                LinkColor = System.Drawing.Color.FromArgb(100, 180, 255),
                Dock      = System.Windows.Forms.DockStyle.Top,
                Padding   = new System.Windows.Forms.Padding(0, 8, 0, 0),
            };
            link.LinkClicked += (s, e) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "https://developer.microsoft.com/microsoft-edge/webview2/") { UseShellExecute = true });

            panel.Controls.Add(link);
            panel.Controls.Add(label);
            documentPanel.Controls.Add(panel);
            panel.BringToFront();

            SetPdfStatus("WebView2 Runtime not installed");
        }

        private void ClearPdfView(string statusText)
        {
            if (docViewer.CoreWebView2 != null)
            {
                string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "no-document.html");
                if (File.Exists(htmlPath))
                {
                    string escaped = System.Security.SecurityElement.Escape(statusText) ?? statusText;
                    string html = File.ReadAllText(htmlPath).Replace("{{MESSAGE}}", escaped);
                    docViewer.CoreWebView2.NavigateToString(html);
                }
                else
                {
                    docViewer.CoreWebView2.Navigate("about:blank");
                }
            }
            SetPdfStatus(statusText);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        private void SetPdfStatus(string text)
        {
            if (pdfStatusLabel.IsDisposed) return;

            if (pdfStatusLabel.InvokeRequired)
                pdfStatusLabel.Invoke(new Action(() => pdfStatusLabel.Text = text));
            else
                pdfStatusLabel.Text = text;
        }

        private void UpdateStatus(string text)
        {
            if (statusLabel.IsDisposed) return;

            if (statusLabel.InvokeRequired)
                statusLabel.Invoke(new Action(() => statusLabel.Text = text));
            else
                statusLabel.Text = text;
        }

        private void DisconnectClient()
        {
            cts?.Cancel();

            try
            {
                if (subscription != null && session != null)
                {
                    session.RemoveSubscription(subscription);
                    subscription.Dispose();
                }

                session?.Close();
                session?.Dispose();
            }
            catch
            {
                // best-effort cleanup
            }
            finally
            {
                subscription = null;
                session = null;
                rowsByClientHandle.Clear();
                monitoredNodes.Clear();
                productIdClientHandle       = null;
                camFileClientHandle         = null;
                machineStateClientHandle    = null;
                operatorActionClientHandle  = null;
                statsAutoModeHandle = statsManualModeHandle = statsSetupModeHandle = null;
                statsTotalHoursHandle = statsProducingHoursHandle = null;
                statsPartCountHandle = statsCurrentStepHandle = null;
                statsBendingNowHandle = null;
                _statsLastTotalHours = 0; _statsLastProducingHours = 0;
                _waitingForFirstBend = false;
                _firstBendHappened   = false;
                _setupStartTime      = DateTime.MinValue;
                _firstBendTime       = DateTime.MinValue;
                _isBendingNow        = false;
                _lastProductionStep  = -1;
                _statsPage.Reset();
                _activeCamFileName          = null;
                _activeProductId            = null;
                _machineState               = -1;
                _activeCamRowIndex          = -1;
                newGroupButton.Visible          = true;
                editGroupButton.Visible         = true;
                deleteGroupButton.Visible       = true;
                runGroupButton.Enabled          = true;
                runGroupButton.BackColor        = System.Drawing.Color.FromArgb(34, 139, 34);
                _operatorActionLabel.Visible    = false;
                _orderRowStates.Clear();
                _productRowStates.Clear();
                ordersDataGridView.Invalidate();
                productsDataGridView.Invalidate();
                dataGridView1.Rows.Clear();
                UpdateStatus("Disconnected");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();

            // session.Close() is a blocking network call — if the server is unreachable it
            // can stall for the full socket timeout.  Cancel the CTS immediately, then fire
            // the cleanup on a background thread and let the process exit without waiting.
            cts?.Cancel();

            var sessionToClose   = session;
            var subToDispose     = subscription;
            session      = null;
            subscription = null;

            if (sessionToClose != null)
            {
                Task.Run(() =>
                {
                    try { subToDispose?.Dispose(); } catch { }
                    try { sessionToClose.Close();   } catch { }
                    try { sessionToClose.Dispose(); } catch { }
                });
            }
        }
    }
}
