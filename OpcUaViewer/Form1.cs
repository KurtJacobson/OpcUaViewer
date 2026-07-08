using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private ISession session;
        private Subscription subscription;
        private CancellationTokenSource cts;

        // The variables discovered under MonitoringFolderPath, populated on connect.
        private List<(string Name, NodeId NodeId)> monitoredNodes = new List<(string Name, NodeId NodeId)>();

        // Maps a MonitoredItem's ClientHandle to the grid row that displays its value.
        private readonly Dictionary<uint, DataGridViewRow> rowsByClientHandle = new Dictionary<uint, DataGridViewRow>();

        // ClientHandle of the item identified as the product id, if one was found.
        private uint? productIdClientHandle;

        // Avoids reloading the same PDF repeatedly.
        private string lastLoadedProductId;

        private static readonly System.Drawing.Color NavAccent   = System.Drawing.Color.FromArgb(255, 140, 0);
        private static readonly System.Drawing.Color NavInactive = System.Drawing.Color.FromArgb(48, 48, 48);
        private static readonly System.Drawing.Color NavTextOn   = System.Drawing.Color.White;
        private static readonly System.Drawing.Color NavTextOff  = System.Drawing.Color.FromArgb(170, 170, 170);

        // Prevents the keyboard re-opening when focus returns after dismissal.
        private bool _suppressKeyboard;

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;
            Shown        += Form1_Shown;
            LoadSettings();
            docViewer.ClearPreview("Waiting for a product id...");

            HookKeyboard(endpointTextBox);
            HookKeyboard(pdfFolderTextBox);
            HookKeyboard(camFolderTextBox);
            HookKeyboard(camOutputTextBox);
            productsDataGridView.CellBeginEdit += productsDataGridView_CellBeginEdit;
        }

        private void HookKeyboard(TextBox tb)
        {
            // Record when focus arrives so Click can distinguish the focusing tap
            // from a subsequent tap. On touchscreens Enter fires before MouseDown,
            // so checking tb.Focused in MouseDown is unreliable.
            DateTime focusedAt = DateTime.MinValue;
            tb.Enter += (s, e) => focusedAt = DateTime.UtcNow;

            tb.Click += (s, e) =>
            {
                // Skip if this click is what gave us focus (within ~300 ms of Enter).
                if ((DateTime.UtcNow - focusedAt).TotalMilliseconds < 300) return;
                if (_suppressKeyboard || !keyboardToggle.Checked) return;
                _suppressKeyboard = true;
                string result = TouchKeyboard.Show(this, tb.Text);
                if (result != null) tb.Text = result;
                BeginInvoke(new Action(() => _suppressKeyboard = false));
            };
        }

        private void productsDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (productsDataGridView.Columns[e.ColumnIndex].Name != "prodRunQtyColumn") return;
            if (!keyboardToggle.Checked) return;
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

        private async void Form1_Shown(object sender, EventArgs e)
        {
            string url = endpointTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            connectButton.Enabled   = false;
            endpointTextBox.Enabled = false;
            UpdateStatus("Auto-connecting...");

            cts = new CancellationTokenSource();
            await StartClient(url, cts.Token);

            connectButton.Enabled = true;
            connectButton.Text    = session != null ? "Disconnect" : "Connect";
            endpointTextBox.Enabled = session == null;
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
            monitoringPanel.Visible = false;
            documentPanel.Visible   = false;
            settingsPanel.Visible   = false;
            groupsPanel.Visible     = false;
            page.Visible = true;

            foreach (var btn in new[] { monitorNavButton, documentNavButton, settingsNavButton, groupsNavButton })
            {
                btn.BackColor = btn == activeButton ? NavAccent   : NavInactive;
                btn.ForeColor = btn == activeButton ? NavTextOn   : NavTextOff;
            }
        }

        private void LoadCamFiles()
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
                    ordersDataGridView.Rows.Add(order.OrderId, order.CustomerName, order.Quantity);
                }
                catch
                {
                    // skip malformed files silently
                }
            }

            if (ordersDataGridView.Rows.Count > 0)
                ordersDataGridView.Rows[0].Selected = true;
        }

        private void ordersDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (ordersDataGridView.SelectedRows.Count == 0) return;
            int idx = ordersDataGridView.SelectedRows[0].Index;
            if (idx >= 0 && idx < _camOrders.Count)
                PopulateProductsGrid(_camOrders[idx]);
        }

        private void PopulateProductsGrid(CamOrder order)
        {
            productsDataGridView.Rows.Clear();
            foreach (var p in order.Products)
            {
                productsDataGridView.Rows.Add(
                    p.ListId,
                    p.DisplayName,
                    p.Parameters,
                    p.MaterialId,
                    p.MaterialThickness,
                    p.Quantity,
                    p.RunQuantity,
                    p.OperatorHint);
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
            if (ordersDataGridView.SelectedRows.Count == 0)
            {
                DarkMessageBox.Show(this,"Please select a product group first.", "Cancel Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string outputBase = camOutputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(outputBase))
            {
                DarkMessageBox.Show(this,"Please set the CAM Output Directory in Settings first.", "Cancel Group",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idx = ordersDataGridView.SelectedRows[0].Index;
            var order = _camOrders[idx];
            string fileName = Path.GetFileName(order.FilePath);

            string inDir        = Path.Combine(outputBase, "in");
            string canceledDir  = Path.Combine(outputBase, "canceled");
            string inPath       = Path.Combine(inDir, fileName);

            if (!File.Exists(inPath))
            {
                DarkMessageBox.Show(this,$"'{fileName}' is not currently in the 'in' folder — nothing to cancel.",
                    "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Directory.CreateDirectory(canceledDir);
                string dest = Path.Combine(canceledDir, fileName);
                File.Move(inPath, dest, overwrite: true);

                DarkMessageBox.Show(this,$"Moved '{fileName}' to:\n{canceledDir}",
                    "Cancel Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this,"Failed to move CAM file:\n\n" + ex.Message, "Cancel Group",
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

            keyboardToggle.Checked = Properties.Settings.Default.KeyboardEnabled;

            RestoreWindowPlacement();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PdfFolderPath  = pdfFolderTextBox.Text.Trim();
            Properties.Settings.Default.EndpointUrl    = endpointTextBox.Text.Trim();
            Properties.Settings.Default.CamFolderPath  = camFolderTextBox.Text.Trim();
            Properties.Settings.Default.CamOutputPath   = camOutputTextBox.Text.Trim();
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

        private async Task StartClient(string endpointUrl, CancellationToken cancellationToken)
        {
            try
            {
                var config = BuildApplicationConfiguration();
                await config.Validate(ApplicationType.Client);

                // NOTE: accepting all certs is fine for a lab/dev network, but don't ship this
                // to anything internet-facing. Swap for proper trust-list handling in production.
                config.CertificateValidator.CertificateValidation += (s, ce) => { ce.Accept = true; };

                // SelectEndpoint does a synchronous network probe — run it off the UI thread.
                var endpointDescription = await Task.Run(
                    () => CoreClientUtils.SelectEndpoint(config, endpointUrl, useSecurity: false),
                    cancellationToken);

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

            productIdClientHandle = null;

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
                }

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

        private void OnValueChanged(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var v in item.DequeueValues())
            {
                UpdateRow(item.ClientHandle, v);

                if (productIdClientHandle.HasValue && item.ClientHandle == productIdClientHandle.Value)
                    OpenProductPdf(v.Value?.ToString());
            }
        }

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
                docViewer.PreviewFile(fullPath);
                SetPdfStatus($"Showing: {fileName}");
            }
            catch (Exception ex)
            {
                ClearPdfView("Failed to open PDF: " + ex.Message);
            }
        }

        private void ClearPdfView(string statusText)
        {
            docViewer.ClearPreview(statusText);
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
                productIdClientHandle = null;
                dataGridView1.Rows.Clear();
                UpdateStatus("Disconnected");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            DisconnectClient();
        }
    }
}
