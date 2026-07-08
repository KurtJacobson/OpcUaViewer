using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OpcUaViewer
{
    /// <summary>
    /// Dark-themed dialog for creating or editing a .p3cam product group file.
    /// Pass an existing CamOrder to edit it, or null to create a new one.
    /// </summary>
    internal sealed class GroupEditorForm : Form
    {
        // ── colours ────────────────────────────────────────────────────────────
        private static readonly Color Bg         = Color.FromArgb(28, 28, 28);
        private static readonly Color PanelBg    = Color.FromArgb(36, 36, 36);
        private static readonly Color FieldBg    = Color.FromArgb(50, 50, 50);
        private static readonly Color FieldFg    = Color.FromArgb(230, 230, 230);
        private static readonly Color LabelFg    = Color.FromArgb(190, 190, 190);
        private static readonly Color HeaderFg   = Color.FromArgb(220, 220, 220);
        private static readonly Color GridBg     = Color.FromArgb(32, 32, 32);
        private static readonly Color GridHeader = Color.FromArgb(45, 45, 45);
        private static readonly Color GridLine   = Color.FromArgb(55, 55, 55);
        private static readonly Color Accent     = Color.FromArgb(255, 140, 0);
        private static readonly Color DangerCol  = Color.FromArgb(180, 50, 50);
        private static readonly Color NavyBtn    = Color.FromArgb(50, 50, 100);
        private static readonly Color BorderCol  = Color.FromArgb(80, 80, 80);

        // ── state ──────────────────────────────────────────────────────────────
        private readonly string   _camFolder;       // where to save .p3cam files
        private readonly string   _productsFolder;  // where .zip product files live
        private readonly string   _existingPath;
        private readonly string   _defaultFileName;
        private readonly string   _defaultOrderId;

        // header fields
        private TextBox _fileNameBox;
        private TextBox _orderIdBox;
        private TextBox _customerBox;
        private TextBox _qtyBox;
        private TextBox _infoBox;

        // product list
        private DataGridView _grid;

        // ── public API ─────────────────────────────────────────────────────────
        /// <param name="camFolder">Folder where .p3cam files are saved.</param>
        /// <param name="productsFolder">Folder containing .zip product files.</param>
        /// <param name="existing">Order to edit, or null to create a new group.</param>
        public static void ShowEditor(IWin32Window owner,
            string camFolder, string productsFolder, CamOrder existing = null,
            string defaultFileName = null, string defaultOrderId = null)
        {
            using var f = new GroupEditorForm(camFolder, productsFolder, existing,
                defaultFileName, defaultOrderId);
            f.ShowDialog(owner);
        }

        // ── construction ───────────────────────────────────────────────────────
        private GroupEditorForm(string camFolder, string productsFolder, CamOrder existing,
            string defaultFileName = null, string defaultOrderId = null)
        {
            _camFolder       = camFolder;
            _productsFolder  = productsFolder;
            _existingPath    = existing?.FilePath;
            _defaultFileName = defaultFileName;
            _defaultOrderId  = defaultOrderId;

            Text            = existing == null ? "New Product Group" : "Edit Product Group";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BorderCol;   // 1px border is just the form background peeking around docked panels
            Size            = new Size(900, 660);
            ShowInTaskbar   = false;
            KeyPreview      = true;
            DoubleBuffered  = true;
            KeyDown        += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            // Reload products from file after the handle is created so rows survive CreateHandle().
            Load += (s, e) =>
            {
                if (string.IsNullOrEmpty(_existingPath) || !File.Exists(_existingPath)) return;
                try
                {
                    var order = new CamOrder(_existingPath);
                    foreach (var p in order.Products)
                        AddProductRow(p.ListId, p.ProductId, p.Quantity, p.OperatorHint);
                }
                catch { /* malformed file — skip population */ }
            };

            BuildUI(existing);
        }

        // ── UI layout ──────────────────────────────────────────────────────────
        private void BuildUI(CamOrder existing)
        {
            const int pad = 16, formW = 900, rightPad = 16;

            // ── title bar ──────────────────────────────────────────────────────
            var titleBar = new Panel
            {
                Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(20, 20, 20)
            };
            var titleLbl = new Label
            {
                Text = Text, Dock = DockStyle.Fill, ForeColor = HeaderFg,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(pad, 0, 0, 0)
            };
            titleBar.Controls.Add(titleLbl);

            // ── button bar (bottom) ────────────────────────────────────────────
            var btnBar = new Panel
            {
                Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(24, 24, 24)
            };
            var topLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = BorderCol };
            btnBar.Controls.Add(topLine);

            var saveBtn = MakeButton("Save", Accent, 150, 38);
            saveBtn.Location = new Point(formW - rightPad - 150, 9);
            saveBtn.Click += SaveButton_Click;

            var cancelBtn = MakeButton("Cancel", Color.FromArgb(70, 70, 70), 120, 38);
            cancelBtn.Location = new Point(formW - rightPad - 150 - 8 - 120, 9);
            cancelBtn.Click += (s, e) => Close();

            btnBar.Controls.Add(saveBtn);
            btnBar.Controls.Add(cancelBtn);

            // ── group info panel ───────────────────────────────────────────────
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top, Height = 180, BackColor = PanelBg,
                Padding = new Padding(pad, pad, pad, pad)
            };
            var infoLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderCol };
            infoPanel.Controls.Add(infoLine);

            // Two-column layout: left col (file name, order id, customer) right col (qty, info text)
            int col1x = pad, col2x = 440;
            int rowH = 36, labelW = 100, fieldH = 28;

            int y = pad + 4;
            _fileNameBox = AddField(infoPanel, "File name:", col1x, y, labelW, fieldH, existing?.FileName ?? _defaultFileName ?? "");
            _orderIdBox  = AddField(infoPanel, "Order ID:",  col2x, y, labelW, fieldH, existing?.OrderId ?? _defaultOrderId ?? "");
            y += rowH + 4;
            _customerBox = AddField(infoPanel, "Customer:",  col1x, y, labelW, fieldH, existing?.CustomerName ?? "");
            _qtyBox      = AddField(infoPanel, "Quantity:",  col2x, y, labelW, fieldH, existing?.Quantity.ToString() ?? "1");
            y += rowH + 4;
            _infoBox     = AddWideField(infoPanel, "Info:", col1x, y, labelW, fieldH * 2,
                existing?.InfoText ?? "", formW - col1x * 2 - labelW - 8);

            // ── products header bar ────────────────────────────────────────────
            var prodHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 42, BackColor = PanelBg,
                Padding = new Padding(pad, 0, pad, 0)
            };
            var prodLabel = new Label
            {
                Text = "Products", ForeColor = HeaderFg,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = false, Size = new Size(200, 42),
                Location = new Point(pad, 0), TextAlign = ContentAlignment.MiddleLeft
            };
            prodHeader.Controls.Add(prodLabel);

            var addBtn = MakeButton("+ Add Products", NavyBtn, 160, 32);
            addBtn.Location = new Point(formW - pad - 160 - 8 - 170, 5);
            addBtn.Click += AddProducts_Click;

            var removeBtn = MakeButton("Remove Selected", DangerCol, 170, 32);
            removeBtn.Location = new Point(formW - pad - 170, 5);
            removeBtn.Click += RemoveProduct_Click;

            prodHeader.Controls.Add(addBtn);
            prodHeader.Controls.Add(removeBtn);
            var headerLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderCol };
            prodHeader.Controls.Add(headerLine);

            // ── products grid (fills remaining space) ──────────────────────────
            _grid = BuildGrid();
            _grid.Dock = DockStyle.Fill;

            // DockStyle is processed in reverse Controls-collection order (last added = first to
            // claim space).  Bottom must claim space BEFORE Fill takes the remainder, so _grid
            // must be at index [0] (processed last) and btnBar at index [1].
            Controls.Add(_grid);        // [0] Fill   → processed very last, fills leftover
            Controls.Add(btnBar);       // [1] Bottom → processed before Fill, claims bottom
            Controls.Add(prodHeader);   // [2] Top    → 3rd from top
            Controls.Add(infoPanel);    // [3] Top    → 2nd from top
            Controls.Add(titleBar);     // [4] Top    → processed first → very top

            // Hook onscreen keyboard to text fields
            HookAllTextBoxes(infoPanel);
        }

        // ── helper: labelled field ─────────────────────────────────────────────
        private TextBox AddField(Panel parent, string label, int x, int y,
            int labelW, int h, string value)
        {
            var lbl = new Label
            {
                Text = label, ForeColor = LabelFg, AutoSize = false,
                Font = new Font("Segoe UI", 10F),
                Size = new Size(labelW, h), Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var tb = new TextBox
            {
                Text = value, BackColor = FieldBg, ForeColor = FieldFg,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, h), Location = new Point(x + labelW + 4, y)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(tb);
            return tb;
        }

        private TextBox AddWideField(Panel parent, string label, int x, int y,
            int labelW, int h, string value, int width)
        {
            var lbl = new Label
            {
                Text = label, ForeColor = LabelFg, AutoSize = false,
                Font = new Font("Segoe UI", 10F),
                Size = new Size(labelW, h), Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var tb = new TextBox
            {
                Text = value, BackColor = FieldBg, ForeColor = FieldFg,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = false,
                Size = new Size(width, h / 2), Location = new Point(x + labelW + 4, y)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(tb);
            return tb;
        }

        // ── dark DataGridView ──────────────────────────────────────────────────
        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                BackgroundColor = GridBg,
                BorderStyle     = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false,
                GridColor        = GridLine,
                ReadOnly         = false,
                RowHeadersVisible = false,
                RowTemplate      = { Height = 40 },
                SelectionMode    = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect      = true
            };
            g.AllowUserToResizeRows = false;

            var hdrStyle = new DataGridViewCellStyle
            {
                BackColor = GridHeader, ForeColor = HeaderFg,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = GridHeader, SelectionForeColor = HeaderFg
            };
            g.ColumnHeadersDefaultCellStyle = hdrStyle;

            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = GridBg, ForeColor = FieldFg,
                Font = new Font("Segoe UI", 10.5F),
                SelectionBackColor = Accent, SelectionForeColor = Color.White
            };
            g.DefaultCellStyle = cellStyle;

            // Editable Qty column
            var qtyStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 55, 45), ForeColor = Color.FromArgb(180, 255, 180),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                SelectionBackColor = Accent, SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewTextBoxColumn Col(string name, string header, int w, bool fill = false)
            {
                var c = new DataGridViewTextBoxColumn
                {
                    Name = name, HeaderText = header, ReadOnly = false,
                    MinimumWidth = w
                };
                if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                else       c.Width = w;
                return c;
            }

            var qtyCol = Col("editQty", "Qty", 60);
            qtyCol.DefaultCellStyle = qtyStyle;

            g.Columns.AddRange(
                Col("editListId",   "List ID",    110),
                Col("editName",     "Product",    220),
                qtyCol,
                Col("editHint",     "Operator Hint", 180, fill: true),
                Col("editProdId",   "Product Path",  200)
            );

            // Show keyboard for qty cell
            g.CellBeginEdit += Grid_CellBeginEdit;

            return g;
        }

        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!Properties.Settings.Default.KeyboardEnabled) return;

            // Cancel in-grid editing and show touch keyboard instead.
            // BeginInvoke defers the keyboard until all pending touch/mouse events from the
            // tap that triggered CellBeginEdit have been fully processed, preventing the
            // keyboard from getting a spurious Deactivate and closing immediately.
            e.Cancel = true;
            int row = e.RowIndex, col = e.ColumnIndex;
            BeginInvoke(new Action(() =>
            {
                var cell    = _grid.Rows[row].Cells[col];
                string cur  = cell.Value?.ToString() ?? "";
                string name = _grid.Columns[col].Name;
                if (name == "editQty")
                {
                    string result = TouchKeyboard.Show(this, cur, numericOnly: true);
                    if (result != null && int.TryParse(result, out int v) && v >= 0)
                        cell.Value = v;
                }
                else
                {
                    string result = TouchKeyboard.Show(this, cur);
                    if (result != null)
                        cell.Value = result;
                }
            }));
        }

        // ── add / remove ───────────────────────────────────────────────────────
        private void AddProducts_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_productsFolder) || !Directory.Exists(_productsFolder))
            {
                DarkMessageBox.Show(this,
                    "Please configure the CAM Products Folder in Settings first.",
                    "Add Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title      = "Select product .zip files",
                InitialDirectory = _productsFolder,
                Filter     = "CAM Product files (*.zip)|*.zip",
                Multiselect = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Build relative path: $(ProductDir)\..\Products.CAM\filename.zip
            // The ProductDir variable resolves at machine runtime; we just store the pattern.
            string relBase = @"$(ProductDir)\..\Products.CAM\";

            foreach (string path in dlg.FileNames)
            {
                string filename = Path.GetFileName(path);
                string listId   = Path.GetFileNameWithoutExtension(path);
                string prodId   = relBase + filename;

                // Skip duplicates
                bool dup = _grid.Rows.Cast<DataGridViewRow>()
                    .Any(r => r.Cells["editListId"].Value?.ToString() == listId);
                if (!dup)
                    AddProductRow(listId, prodId, 1, "");
            }
        }

        private void RemoveProduct_Click(object sender, EventArgs e)
        {
            var selected = _grid.SelectedRows.Cast<DataGridViewRow>()
                .OrderByDescending(r => r.Index).ToList();
            foreach (var row in selected)
                _grid.Rows.Remove(row);
        }

        private void AddProductRow(string listId, string productId, int qty, string hint)
        {
            string displayName = Path.GetFileNameWithoutExtension(Path.GetFileName(productId));
            _grid.Rows.Add(listId, displayName, qty, hint, productId);
        }

        // ── save ───────────────────────────────────────────────────────────────
        private void SaveButton_Click(object sender, EventArgs e)
        {
            string fileName = _fileNameBox.Text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                DarkMessageBox.Show(this, "Please enter a file name.", "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_camFolder) || !Directory.Exists(_camFolder))
            {
                DarkMessageBox.Show(this,
                    "CAM folder not set or does not exist. Configure it in Settings.",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(_qtyBox.Text.Trim(), out int qty) || qty < 0) qty = 1;

            // Build XML
            XNamespace ns = "http://eu.schroedergroup.de/xml-schemas/P3CAMSchema.xsd";
            var root = new XElement(ns + "POS3000CAMData",
                new XAttribute("InfoText",     _infoBox.Text.Trim()),
                new XAttribute("OrderId",      _orderIdBox.Text.Trim()),
                new XAttribute("CustomerName", _customerBox.Text.Trim()),
                new XAttribute("Quantity",     qty),
                new XAttribute("Completed",    0));

            foreach (DataGridViewRow row in _grid.Rows)
            {
                string listId  = row.Cells["editListId"].Value?.ToString() ?? "";
                string prodId  = row.Cells["editProdId"].Value?.ToString() ?? "";
                string hint    = row.Cells["editHint"].Value?.ToString() ?? "";
                int    rowQty  = 1;
                int.TryParse(row.Cells["editQty"].Value?.ToString(), out rowQty);

                string infoText = "Auto-generated product";
                string detailsHtml = "<b>Material:</b> TBD<br><b>Thickness:</b> TBD<br><b>Parameters:</b> TBD";

                root.Add(new XElement(ns + "Product",
                    new XAttribute("ListId",       listId),
                    new XAttribute("ProductId",    prodId),
                    new XAttribute("Quantity",     rowQty),
                    new XAttribute("Completed",    0),
                    new XAttribute("LoadError",    0),
                    new XAttribute("InfoText",     infoText),
                    new XAttribute("DetailsHtml",  detailsHtml),
                    new XAttribute("OperatorHint", hint),
                    new XAttribute("UserData",     "Imported from ListId batch")));
            }

            // Ensure .p3cam extension
            if (!fileName.EndsWith(".p3cam", StringComparison.OrdinalIgnoreCase))
                fileName += ".p3cam";

            string savePath = _existingPath ?? Path.Combine(_camFolder, fileName);

            // If filename changed from original, use new name but warn on overwrite
            if (_existingPath != null &&
                !string.Equals(Path.GetFileNameWithoutExtension(_existingPath),
                               Path.GetFileNameWithoutExtension(fileName),
                               StringComparison.OrdinalIgnoreCase))
            {
                savePath = Path.Combine(_camFolder, fileName);
            }

            if (File.Exists(savePath) && savePath != _existingPath)
            {
                var confirm = DarkMessageBox.Show(this,
                    $"'{fileName}' already exists. Overwrite?",
                    "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;
            }

            try
            {
                new XDocument(new XDeclaration("1.0", "utf-8", null), root)
                    .Save(savePath);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this, "Failed to save:\n\n" + ex.Message, "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── keyboard hookup ────────────────────────────────────────────────────
        private void HookAllTextBoxes(Panel panel)
        {
            foreach (Control c in panel.Controls)
            {
                if (c is TextBox tb) HookKb(tb);
            }
        }

        private void HookKb(TextBox tb)
        {
            DateTime focusedAt = DateTime.MinValue;
            tb.Enter += (s, e) => focusedAt = DateTime.UtcNow;
            tb.Click += (s, e) =>
            {
                if ((DateTime.UtcNow - focusedAt).TotalMilliseconds < 300) return;
                if (!Properties.Settings.Default.KeyboardEnabled) return;
                string result = TouchKeyboard.Show(this, tb.Text);
                if (result != null) tb.Text = result;
            };
        }

        // ── button factory ─────────────────────────────────────────────────────
        private static Button MakeButton(string text, Color bg, int w, int h)
        {
            var b = new Button
            {
                Text = text, BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Size = new Size(w, h)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Lighten(bg, 18);
            return b;
        }

        private static Color Lighten(Color c, int amt) => Color.FromArgb(
            Math.Min(255, c.R + amt),
            Math.Min(255, c.G + amt),
            Math.Min(255, c.B + amt));
    }
}
