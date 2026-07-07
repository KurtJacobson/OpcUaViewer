namespace OpcUaViewer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            navPanel = new System.Windows.Forms.Panel();
            titleLabel = new System.Windows.Forms.Label();
            monitorNavButton = new System.Windows.Forms.Button();
            documentNavButton = new System.Windows.Forms.Button();
            settingsNavButton = new System.Windows.Forms.Button();
            monitoringPanel = new System.Windows.Forms.Panel();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            nodeIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            timestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            documentPanel = new System.Windows.Forms.Panel();
            docViewer = new ShellPreviewPanel();
            settingsPanel = new System.Windows.Forms.Panel();
            connectionGroupBox = new System.Windows.Forms.GroupBox();
            endpointLabel = new System.Windows.Forms.Label();
            endpointTextBox = new System.Windows.Forms.TextBox();
            connectButton = new System.Windows.Forms.Button();
            statusLabel = new System.Windows.Forms.Label();
            fullscreenCheckBox = new System.Windows.Forms.CheckBox();
            camFolderGroupBox = new System.Windows.Forms.GroupBox();
            camFolderLabel = new System.Windows.Forms.Label();
            camFolderTextBox = new System.Windows.Forms.TextBox();
            camFolderBrowseButton = new System.Windows.Forms.Button();
            camOutputGroupBox = new System.Windows.Forms.GroupBox();
            camOutputLabel = new System.Windows.Forms.Label();
            camOutputTextBox = new System.Windows.Forms.TextBox();
            camOutputBrowseButton = new System.Windows.Forms.Button();
            groupsNavButton = new System.Windows.Forms.Button();
            groupsButtonPanel = new System.Windows.Forms.Panel();
            runGroupButton = new System.Windows.Forms.Button();
            cancelGroupButton = new System.Windows.Forms.Button();
            ordersDataGridView = new System.Windows.Forms.DataGridView();
            ordersOrderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ordersCustomerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ordersQtyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            groupsPanel = new System.Windows.Forms.Panel();
            groupsSplitContainer = new System.Windows.Forms.SplitContainer();
            productsDataGridView = new System.Windows.Forms.DataGridView();
            prodListIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodParametersColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodMaterialColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodThicknessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodQtyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodRunQtyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            prodHintColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            pdfGroupBox = new System.Windows.Forms.GroupBox();
            pdfFolderLabel = new System.Windows.Forms.Label();
            pdfFolderTextBox = new System.Windows.Forms.TextBox();
            pdfBrowseButton = new System.Windows.Forms.Button();
            pdfStatusLabel = new System.Windows.Forms.Label();
            navPanel.SuspendLayout();
            monitoringPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            documentPanel.SuspendLayout();
            groupsPanel.SuspendLayout();
            groupsButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupsSplitContainer).BeginInit();
            groupsSplitContainer.Panel1.SuspendLayout();
            groupsSplitContainer.Panel2.SuspendLayout();
            groupsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ordersDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)productsDataGridView).BeginInit();
            camFolderGroupBox.SuspendLayout();
            camOutputGroupBox.SuspendLayout();
            settingsPanel.SuspendLayout();
            connectionGroupBox.SuspendLayout();
            pdfGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // navPanel
            // 
            navPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            navPanel.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            navPanel.Controls.Add(titleLabel);
            navPanel.Controls.Add(monitorNavButton);
            navPanel.Controls.Add(documentNavButton);
            navPanel.Controls.Add(settingsNavButton);
            navPanel.Controls.Add(groupsNavButton);
            navPanel.Location = new System.Drawing.Point(0, 0);
            navPanel.Name = "navPanel";
            navPanel.Size = new System.Drawing.Size(100, 660);
            navPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            titleLabel.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.FromArgb(180, 180, 180);
            titleLabel.Location = new System.Drawing.Point(0, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(100, 65);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "OPC UA\r\nViewer";
            titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // monitorNavButton
            // 
            monitorNavButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            monitorNavButton.BackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            monitorNavButton.FlatAppearance.BorderSize = 0;
            monitorNavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            monitorNavButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            monitorNavButton.ForeColor = System.Drawing.Color.White;
            monitorNavButton.Location = new System.Drawing.Point(0, 65);
            monitorNavButton.Name = "monitorNavButton";
            monitorNavButton.Size = new System.Drawing.Size(100, 90);
            monitorNavButton.TabIndex = 0;
            monitorNavButton.Text = "Status Monitor";
            monitorNavButton.UseVisualStyleBackColor = false;
            monitorNavButton.Click += monitorNavButton_Click;
            // 
            // documentNavButton
            // 
            documentNavButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            documentNavButton.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            documentNavButton.FlatAppearance.BorderSize = 0;
            documentNavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            documentNavButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            documentNavButton.ForeColor = System.Drawing.Color.FromArgb(170, 170, 170);
            documentNavButton.Location = new System.Drawing.Point(0, 158);
            documentNavButton.Name = "documentNavButton";
            documentNavButton.Size = new System.Drawing.Size(100, 90);
            documentNavButton.TabIndex = 1;
            documentNavButton.Text = "Document";
            documentNavButton.UseVisualStyleBackColor = false;
            documentNavButton.Click += documentNavButton_Click;
            // 
            // settingsNavButton
            // 
            settingsNavButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            settingsNavButton.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            settingsNavButton.FlatAppearance.BorderSize = 0;
            settingsNavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            settingsNavButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            settingsNavButton.ForeColor = System.Drawing.Color.FromArgb(170, 170, 170);
            settingsNavButton.Location = new System.Drawing.Point(0, 570);
            settingsNavButton.Name = "settingsNavButton";
            settingsNavButton.Size = new System.Drawing.Size(100, 90);
            settingsNavButton.TabIndex = 2;
            settingsNavButton.Text = "Settings";
            settingsNavButton.UseVisualStyleBackColor = false;
            settingsNavButton.Click += settingsNavButton_Click;
            //
            // groupsNavButton
            //
            groupsNavButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupsNavButton.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            groupsNavButton.FlatAppearance.BorderSize = 0;
            groupsNavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            groupsNavButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            groupsNavButton.ForeColor = System.Drawing.Color.FromArgb(170, 170, 170);
            groupsNavButton.Location = new System.Drawing.Point(0, 251);
            groupsNavButton.Name = "groupsNavButton";
            groupsNavButton.Size = new System.Drawing.Size(100, 90);
            groupsNavButton.TabIndex = 3;
            groupsNavButton.Text = "Product\r\nGroups";
            groupsNavButton.UseVisualStyleBackColor = false;
            groupsNavButton.Click += groupsNavButton_Click;
            //
            // monitoringPanel
            // 
            monitoringPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            monitoringPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            monitoringPanel.Controls.Add(dataGridView1);
            monitoringPanel.Location = new System.Drawing.Point(106, 0);
            monitoringPanel.Name = "monitoringPanel";
            monitoringPanel.Size = new System.Drawing.Size(994, 660);
            monitoringPanel.TabIndex = 1;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(28, 28, 28);
            dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.ColumnHeadersHeight = 40;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { nameColumn, nodeIdColumn, valueColumn, statusColumn, timestampColumn });
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = System.Drawing.Color.FromArgb(55, 55, 55);
            dataGridView1.Location = new System.Drawing.Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView1.Size = new System.Drawing.Size(994, 660);
            dataGridView1.TabIndex = 0;
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Name";
            nameColumn.Name = "nameColumn";
            nameColumn.ReadOnly = true;
            nameColumn.Width = 160;
            // 
            // nodeIdColumn
            // 
            nodeIdColumn.HeaderText = "Node Id";
            nodeIdColumn.Name = "nodeIdColumn";
            nodeIdColumn.ReadOnly = true;
            nodeIdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            nodeIdColumn.MinimumWidth = 120;
            //
            // valueColumn
            //
            valueColumn.HeaderText = "Value";
            valueColumn.Name = "valueColumn";
            valueColumn.ReadOnly = true;
            valueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            valueColumn.MinimumWidth = 80;
            // 
            // statusColumn
            // 
            statusColumn.HeaderText = "Status";
            statusColumn.Name = "statusColumn";
            statusColumn.ReadOnly = true;
            // 
            // timestampColumn
            // 
            timestampColumn.HeaderText = "Last Updated";
            timestampColumn.Name = "timestampColumn";
            timestampColumn.ReadOnly = true;
            timestampColumn.Width = 120;
            //
            // docViewer
            //
            docViewer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            docViewer.BackColor = System.Drawing.Color.White;
            docViewer.Location = new System.Drawing.Point(0, 0);
            docViewer.Name = "docViewer";
            docViewer.Size = new System.Drawing.Size(1000, 660);
            docViewer.TabIndex = 0;
            //
            // documentPanel
            //
            documentPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            documentPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            documentPanel.Controls.Add(docViewer);
            documentPanel.Location = new System.Drawing.Point(106, 0);
            documentPanel.Name = "documentPanel";
            documentPanel.Size = new System.Drawing.Size(994, 660);
            documentPanel.TabIndex = 2;
            documentPanel.Visible = false;
            //
            // groupsPanel
            //
            groupsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupsPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            groupsPanel.Controls.Add(groupsSplitContainer);
            groupsPanel.Controls.Add(groupsButtonPanel);
            groupsPanel.Location = new System.Drawing.Point(106, 0);
            groupsPanel.Name = "groupsPanel";
            groupsPanel.Size = new System.Drawing.Size(994, 660);
            groupsPanel.TabIndex = 4;
            groupsPanel.Visible = false;
            //
            // groupsSplitContainer
            //
            //
            // groupsButtonPanel
            //
            groupsButtonPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupsButtonPanel.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            groupsButtonPanel.Controls.Add(runGroupButton);
            groupsButtonPanel.Controls.Add(cancelGroupButton);
            groupsButtonPanel.Location = new System.Drawing.Point(0, 604);
            groupsButtonPanel.Name = "groupsButtonPanel";
            groupsButtonPanel.Size = new System.Drawing.Size(1000, 56);
            groupsButtonPanel.TabIndex = 1;
            //
            // runGroupButton
            //
            runGroupButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            runGroupButton.BackColor = System.Drawing.Color.FromArgb(34, 139, 34);
            runGroupButton.FlatAppearance.BorderSize = 0;
            runGroupButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            runGroupButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            runGroupButton.ForeColor = System.Drawing.Color.White;
            runGroupButton.Location = new System.Drawing.Point(576, 8);
            runGroupButton.Name = "runGroupButton";
            runGroupButton.Size = new System.Drawing.Size(200, 40);
            runGroupButton.TabIndex = 0;
            runGroupButton.Text = "Run Group";
            runGroupButton.UseVisualStyleBackColor = false;
            runGroupButton.Click += runGroupButton_Click;
            //
            // cancelGroupButton
            //
            cancelGroupButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            cancelGroupButton.BackColor = System.Drawing.Color.FromArgb(180, 50, 50);
            cancelGroupButton.FlatAppearance.BorderSize = 0;
            cancelGroupButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cancelGroupButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            cancelGroupButton.ForeColor = System.Drawing.Color.White;
            cancelGroupButton.Location = new System.Drawing.Point(784, 8);
            cancelGroupButton.Name = "cancelGroupButton";
            cancelGroupButton.Size = new System.Drawing.Size(200, 40);
            cancelGroupButton.TabIndex = 1;
            cancelGroupButton.Text = "Cancel Group";
            cancelGroupButton.UseVisualStyleBackColor = false;
            cancelGroupButton.Click += cancelGroupButton_Click;
            //
            // groupsSplitContainer
            //
            groupsSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupsSplitContainer.BackColor = System.Drawing.Color.FromArgb(55, 55, 55);
            groupsSplitContainer.Location = new System.Drawing.Point(0, 0);
            groupsSplitContainer.Name = "groupsSplitContainer";
            groupsSplitContainer.Size = new System.Drawing.Size(1000, 604);
            groupsSplitContainer.SplitterDistance = 280;
            groupsSplitContainer.SplitterWidth = 3;
            groupsSplitContainer.TabIndex = 0;
            groupsSplitContainer.Panel1.Controls.Add(ordersDataGridView);
            groupsSplitContainer.Panel2.Controls.Add(productsDataGridView);
            //
            // ordersDataGridView
            //
            ordersDataGridView.AllowUserToAddRows = false;
            ordersDataGridView.AllowUserToDeleteRows = false;
            ordersDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ordersDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(28, 28, 28);
            ordersDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ordersDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            ordersDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            ordersDataGridView.ColumnHeadersHeight = 40;
            ordersDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ordersDataGridView.EnableHeadersVisualStyles = false;
            ordersDataGridView.GridColor = System.Drawing.Color.FromArgb(55, 55, 55);
            ordersDataGridView.Location = new System.Drawing.Point(0, 0);
            ordersDataGridView.MultiSelect = false;
            ordersDataGridView.Name = "ordersDataGridView";
            ordersDataGridView.ReadOnly = true;
            ordersDataGridView.RowHeadersVisible = false;
            ordersDataGridView.RowTemplate.Height = 44;
            ordersDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            ordersDataGridView.Size = new System.Drawing.Size(277, 604);
            ordersDataGridView.TabIndex = 0;
            ordersDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                ordersOrderColumn, ordersCustomerColumn, ordersQtyColumn });

            var ordersHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            ordersHeaderStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            ordersHeaderStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            ordersHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            ordersHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            ordersHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            ordersHeaderStyle.SelectionForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            ordersDataGridView.ColumnHeadersDefaultCellStyle = ordersHeaderStyle;

            var ordersCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            ordersCellStyle.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            ordersCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            ordersCellStyle.Font = new System.Drawing.Font("Segoe UI", 11F);
            ordersCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            ordersCellStyle.SelectionForeColor = System.Drawing.Color.White;
            ordersDataGridView.DefaultCellStyle = ordersCellStyle;

            ordersOrderColumn.HeaderText = "Order";        ordersOrderColumn.Name = "ordersOrderColumn";       ordersOrderColumn.Width = 90;
            ordersCustomerColumn.HeaderText = "Customer";  ordersCustomerColumn.Name = "ordersCustomerColumn";  ordersCustomerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            ordersQtyColumn.HeaderText = "Qty";            ordersQtyColumn.Name = "ordersQtyColumn";            ordersQtyColumn.Width = 45;

            ordersDataGridView.SelectionChanged += ordersDataGridView_SelectionChanged;
            //
            // productsDataGridView
            //
            productsDataGridView.AllowUserToAddRows = false;
            productsDataGridView.AllowUserToDeleteRows = false;
            productsDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            productsDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(28, 28, 28);
            productsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            productsDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            productsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            productsDataGridView.ColumnHeadersHeight = 40;
            productsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            productsDataGridView.EnableHeadersVisualStyles = false;
            productsDataGridView.GridColor = System.Drawing.Color.FromArgb(55, 55, 55);
            productsDataGridView.Location = new System.Drawing.Point(0, 0);
            productsDataGridView.Name = "productsDataGridView";
            productsDataGridView.ReadOnly = false;
            productsDataGridView.RowHeadersVisible = false;
            productsDataGridView.RowTemplate.Height = 40;
            productsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            productsDataGridView.Size = new System.Drawing.Size(717, 660);
            productsDataGridView.TabIndex = 0;
            productsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                prodListIdColumn, prodNameColumn, prodParametersColumn,
                prodMaterialColumn, prodThicknessColumn, prodQtyColumn,
                prodRunQtyColumn, prodHintColumn });

            var prodHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            prodHeaderStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            prodHeaderStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            prodHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            prodHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            prodHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            prodHeaderStyle.SelectionForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            productsDataGridView.ColumnHeadersDefaultCellStyle = prodHeaderStyle;

            var prodCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            prodCellStyle.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            prodCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            prodCellStyle.Font = new System.Drawing.Font("Segoe UI", 11F);
            prodCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            prodCellStyle.SelectionForeColor = System.Drawing.Color.White;
            productsDataGridView.DefaultCellStyle = prodCellStyle;

            var prodRunQtyStyle = new System.Windows.Forms.DataGridViewCellStyle();
            prodRunQtyStyle.BackColor = System.Drawing.Color.FromArgb(45, 55, 45);
            prodRunQtyStyle.ForeColor = System.Drawing.Color.FromArgb(180, 255, 180);
            prodRunQtyStyle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            prodRunQtyStyle.SelectionBackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            prodRunQtyStyle.SelectionForeColor = System.Drawing.Color.White;
            prodRunQtyStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            prodRunQtyColumn.DefaultCellStyle = prodRunQtyStyle;

            prodListIdColumn.HeaderText = "List ID";         prodListIdColumn.Name = "prodListIdColumn";     prodListIdColumn.ReadOnly = true;  prodListIdColumn.Width = 110;
            prodNameColumn.HeaderText = "Product";           prodNameColumn.Name = "prodNameColumn";         prodNameColumn.ReadOnly = true;  prodNameColumn.Width = 200;
            prodParametersColumn.HeaderText = "Parameters";  prodParametersColumn.Name = "prodParametersColumn"; prodParametersColumn.ReadOnly = true;  prodParametersColumn.Width = 160;
            prodMaterialColumn.HeaderText = "Material";      prodMaterialColumn.Name = "prodMaterialColumn"; prodMaterialColumn.ReadOnly = true;  prodMaterialColumn.Width = 90;
            prodThicknessColumn.HeaderText = "Thickness";    prodThicknessColumn.Name = "prodThicknessColumn"; prodThicknessColumn.ReadOnly = true; prodThicknessColumn.Width = 80;
            prodQtyColumn.HeaderText = "Order Qty";          prodQtyColumn.Name = "prodQtyColumn";           prodQtyColumn.ReadOnly = true;  prodQtyColumn.Width = 80;
            prodRunQtyColumn.HeaderText = "Run Qty";         prodRunQtyColumn.Name = "prodRunQtyColumn";     prodRunQtyColumn.ReadOnly = false; prodRunQtyColumn.Width = 80;
            prodHintColumn.HeaderText = "Hint";              prodHintColumn.Name = "prodHintColumn";         prodHintColumn.ReadOnly = true;  prodHintColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            productsDataGridView.CellValidating += productsDataGridView_CellValidating;
            //
            // settingsPanel
            // 
            settingsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            settingsPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            settingsPanel.Controls.Add(connectionGroupBox);
            settingsPanel.Controls.Add(pdfGroupBox);
            settingsPanel.Controls.Add(camFolderGroupBox);
            settingsPanel.Controls.Add(camOutputGroupBox);
            settingsPanel.Controls.Add(fullscreenCheckBox);
            settingsPanel.Location = new System.Drawing.Point(106, 0);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new System.Drawing.Size(994, 660);
            settingsPanel.TabIndex = 3;
            settingsPanel.Visible = false;
            // 
            // connectionGroupBox
            // 
            connectionGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            connectionGroupBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            connectionGroupBox.Controls.Add(endpointLabel);
            connectionGroupBox.Controls.Add(endpointTextBox);
            connectionGroupBox.Controls.Add(connectButton);
            connectionGroupBox.Controls.Add(statusLabel);
            connectionGroupBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            connectionGroupBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            connectionGroupBox.Location = new System.Drawing.Point(20, 20);
            connectionGroupBox.Name = "connectionGroupBox";
            connectionGroupBox.Size = new System.Drawing.Size(960, 120);
            connectionGroupBox.TabIndex = 0;
            connectionGroupBox.TabStop = false;
            connectionGroupBox.Text = "OPC UA Connection";
            // 
            // endpointLabel
            // 
            endpointLabel.AutoSize = true;
            endpointLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            endpointLabel.ForeColor = System.Drawing.Color.FromArgb(190, 190, 190);
            endpointLabel.Location = new System.Drawing.Point(16, 40);
            endpointLabel.Name = "endpointLabel";
            endpointLabel.Size = new System.Drawing.Size(72, 20);
            endpointLabel.TabIndex = 0;
            endpointLabel.Text = "Endpoint:";
            // 
            // endpointTextBox
            // 
            endpointTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            endpointTextBox.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            endpointTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            endpointTextBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            endpointTextBox.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            endpointTextBox.Location = new System.Drawing.Point(100, 36);
            endpointTextBox.Name = "endpointTextBox";
            endpointTextBox.Size = new System.Drawing.Size(710, 27);
            endpointTextBox.TabIndex = 1;
            endpointTextBox.Text = "opc.tcp://10.10.0.102:4840";
            // 
            // connectButton
            // 
            connectButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            connectButton.BackColor = System.Drawing.Color.FromArgb(255, 140, 0);
            connectButton.FlatAppearance.BorderSize = 0;
            connectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            connectButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            connectButton.ForeColor = System.Drawing.Color.White;
            connectButton.Location = new System.Drawing.Point(820, 32);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(130, 38);
            connectButton.TabIndex = 2;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = false;
            connectButton.Click += connectButton_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            statusLabel.Location = new System.Drawing.Point(16, 82);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(99, 19);
            statusLabel.TabIndex = 3;
            statusLabel.Text = "Not connected";
            // 
            // pdfGroupBox
            // 
            pdfGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pdfGroupBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            pdfGroupBox.Controls.Add(pdfFolderLabel);
            pdfGroupBox.Controls.Add(pdfFolderTextBox);
            pdfGroupBox.Controls.Add(pdfBrowseButton);
            pdfGroupBox.Controls.Add(pdfStatusLabel);
            pdfGroupBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            pdfGroupBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            pdfGroupBox.Location = new System.Drawing.Point(20, 156);
            pdfGroupBox.Name = "pdfGroupBox";
            pdfGroupBox.Size = new System.Drawing.Size(960, 100);
            pdfGroupBox.TabIndex = 1;
            pdfGroupBox.TabStop = false;
            pdfGroupBox.Text = "PDF Documents";
            // 
            // pdfFolderLabel
            // 
            pdfFolderLabel.AutoSize = true;
            pdfFolderLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            pdfFolderLabel.ForeColor = System.Drawing.Color.FromArgb(190, 190, 190);
            pdfFolderLabel.Location = new System.Drawing.Point(16, 40);
            pdfFolderLabel.Name = "pdfFolderLabel";
            pdfFolderLabel.Size = new System.Drawing.Size(54, 20);
            pdfFolderLabel.TabIndex = 0;
            pdfFolderLabel.Text = "Folder:";
            // 
            // pdfFolderTextBox
            // 
            pdfFolderTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pdfFolderTextBox.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            pdfFolderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pdfFolderTextBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            pdfFolderTextBox.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            pdfFolderTextBox.Location = new System.Drawing.Point(72, 36);
            pdfFolderTextBox.Name = "pdfFolderTextBox";
            pdfFolderTextBox.Size = new System.Drawing.Size(738, 27);
            pdfFolderTextBox.TabIndex = 1;
            pdfFolderTextBox.Text = "C:\\ProductDocs";
            // 
            // pdfBrowseButton
            // 
            pdfBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            pdfBrowseButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            pdfBrowseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(90, 90, 90);
            pdfBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            pdfBrowseButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            pdfBrowseButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            pdfBrowseButton.Location = new System.Drawing.Point(820, 32);
            pdfBrowseButton.Name = "pdfBrowseButton";
            pdfBrowseButton.Size = new System.Drawing.Size(130, 38);
            pdfBrowseButton.TabIndex = 2;
            pdfBrowseButton.Text = "Browse...";
            pdfBrowseButton.UseVisualStyleBackColor = false;
            pdfBrowseButton.Click += pdfBrowseButton_Click;
            // 
            // pdfStatusLabel
            // 
            pdfStatusLabel.AutoSize = true;
            pdfStatusLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            pdfStatusLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            pdfStatusLabel.Location = new System.Drawing.Point(16, 74);
            pdfStatusLabel.Name = "pdfStatusLabel";
            pdfStatusLabel.Size = new System.Drawing.Size(163, 19);
            pdfStatusLabel.TabIndex = 3;
            pdfStatusLabel.Text = "Waiting for a product id...";
            //
            // camOutputGroupBox  (destination for Run/Cancel — contains "in" and "canceled" subfolders)
            //
            camOutputGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            camOutputGroupBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            camOutputGroupBox.Controls.Add(camOutputLabel);
            camOutputGroupBox.Controls.Add(camOutputTextBox);
            camOutputGroupBox.Controls.Add(camOutputBrowseButton);
            camOutputGroupBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            camOutputGroupBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            camOutputGroupBox.Location = new System.Drawing.Point(20, 373);
            camOutputGroupBox.Name = "camOutputGroupBox";
            camOutputGroupBox.Size = new System.Drawing.Size(960, 85);
            camOutputGroupBox.TabIndex = 5;
            camOutputGroupBox.TabStop = false;
            camOutputGroupBox.Text = "CAM Output Directory (Run / Cancel destination)";
            //
            // camOutputLabel
            //
            camOutputLabel.AutoSize = true;
            camOutputLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            camOutputLabel.ForeColor = System.Drawing.Color.FromArgb(190, 190, 190);
            camOutputLabel.Location = new System.Drawing.Point(16, 40);
            camOutputLabel.Name = "camOutputLabel";
            camOutputLabel.TabIndex = 0;
            camOutputLabel.Text = "Folder:";
            //
            // camOutputTextBox
            //
            camOutputTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            camOutputTextBox.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            camOutputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            camOutputTextBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            camOutputTextBox.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            camOutputTextBox.Location = new System.Drawing.Point(72, 36);
            camOutputTextBox.Name = "camOutputTextBox";
            camOutputTextBox.Size = new System.Drawing.Size(738, 27);
            camOutputTextBox.TabIndex = 1;
            //
            // camOutputBrowseButton
            //
            camOutputBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            camOutputBrowseButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            camOutputBrowseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(90, 90, 90);
            camOutputBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            camOutputBrowseButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            camOutputBrowseButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            camOutputBrowseButton.Location = new System.Drawing.Point(820, 32);
            camOutputBrowseButton.Name = "camOutputBrowseButton";
            camOutputBrowseButton.Size = new System.Drawing.Size(130, 38);
            camOutputBrowseButton.TabIndex = 2;
            camOutputBrowseButton.Text = "Browse...";
            camOutputBrowseButton.UseVisualStyleBackColor = false;
            camOutputBrowseButton.Click += camOutputBrowseButton_Click;
            //
            // camFolderGroupBox
            //
            camFolderGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            camFolderGroupBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 36);
            camFolderGroupBox.Controls.Add(camFolderLabel);
            camFolderGroupBox.Controls.Add(camFolderTextBox);
            camFolderGroupBox.Controls.Add(camFolderBrowseButton);
            camFolderGroupBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            camFolderGroupBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            camFolderGroupBox.Location = new System.Drawing.Point(20, 272);
            camFolderGroupBox.Name = "camFolderGroupBox";
            camFolderGroupBox.Size = new System.Drawing.Size(960, 85);
            camFolderGroupBox.TabIndex = 4;
            camFolderGroupBox.TabStop = false;
            camFolderGroupBox.Text = "Product Groups (.p3cam Files)";
            //
            // camFolderLabel
            //
            camFolderLabel.AutoSize = true;
            camFolderLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            camFolderLabel.ForeColor = System.Drawing.Color.FromArgb(190, 190, 190);
            camFolderLabel.Location = new System.Drawing.Point(16, 40);
            camFolderLabel.Name = "camFolderLabel";
            camFolderLabel.Size = new System.Drawing.Size(54, 20);
            camFolderLabel.TabIndex = 0;
            camFolderLabel.Text = "Folder:";
            //
            // camFolderTextBox
            //
            camFolderTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            camFolderTextBox.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            camFolderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            camFolderTextBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            camFolderTextBox.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            camFolderTextBox.Location = new System.Drawing.Point(72, 36);
            camFolderTextBox.Name = "camFolderTextBox";
            camFolderTextBox.Size = new System.Drawing.Size(738, 27);
            camFolderTextBox.TabIndex = 1;
            //
            // camFolderBrowseButton
            //
            camFolderBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            camFolderBrowseButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            camFolderBrowseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(90, 90, 90);
            camFolderBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            camFolderBrowseButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            camFolderBrowseButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            camFolderBrowseButton.Location = new System.Drawing.Point(820, 32);
            camFolderBrowseButton.Name = "camFolderBrowseButton";
            camFolderBrowseButton.Size = new System.Drawing.Size(130, 38);
            camFolderBrowseButton.TabIndex = 2;
            camFolderBrowseButton.Text = "Browse...";
            camFolderBrowseButton.UseVisualStyleBackColor = false;
            camFolderBrowseButton.Click += camFolderBrowseButton_Click;
            //
            // fullscreenCheckBox
            //
            fullscreenCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            fullscreenCheckBox.AutoSize = true;
            fullscreenCheckBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            fullscreenCheckBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            fullscreenCheckBox.Location = new System.Drawing.Point(20, 474);
            fullscreenCheckBox.Name = "fullscreenCheckBox";
            fullscreenCheckBox.Size = new System.Drawing.Size(160, 24);
            fullscreenCheckBox.TabIndex = 2;
            fullscreenCheckBox.Text = "Full screen on startup";
            fullscreenCheckBox.UseVisualStyleBackColor = false;
            fullscreenCheckBox.BackColor = System.Drawing.Color.Transparent;
            fullscreenCheckBox.CheckedChanged += new System.EventHandler(this.fullscreenCheckBox_CheckedChanged);
            //
            // Form1
            //
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            ClientSize = new System.Drawing.Size(1100, 660);
            Controls.Add(navPanel);
            Controls.Add(monitoringPanel);
            Controls.Add(documentPanel);
            Controls.Add(groupsPanel);
            Controls.Add(settingsPanel);
            Font = new System.Drawing.Font("Segoe UI", 11F);
            ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(800, 500);
            Name = "Form1";
            Text = "OPC UA Viewer";
            navPanel.ResumeLayout(false);
            monitoringPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            documentPanel.ResumeLayout(false);
            groupsPanel.ResumeLayout(false);
            groupsButtonPanel.ResumeLayout(false);
            groupsSplitContainer.Panel1.ResumeLayout(false);
            groupsSplitContainer.Panel2.ResumeLayout(false);
            groupsSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ordersDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)productsDataGridView).EndInit();
            camFolderGroupBox.ResumeLayout(false);
            camFolderGroupBox.PerformLayout();
            camOutputGroupBox.ResumeLayout(false);
            camOutputGroupBox.PerformLayout();
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            connectionGroupBox.ResumeLayout(false);
            connectionGroupBox.PerformLayout();
            pdfGroupBox.ResumeLayout(false);
            pdfGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel navPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Button monitorNavButton;
        private System.Windows.Forms.Button documentNavButton;
        private System.Windows.Forms.Button settingsNavButton;

        private System.Windows.Forms.Panel monitoringPanel;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nodeIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn timestampColumn;

        private System.Windows.Forms.Panel documentPanel;
        private ShellPreviewPanel docViewer;

        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.Label endpointLabel;
        private System.Windows.Forms.TextBox endpointTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.CheckBox fullscreenCheckBox;
        private System.Windows.Forms.GroupBox pdfGroupBox;
        private System.Windows.Forms.Label pdfFolderLabel;
        private System.Windows.Forms.TextBox pdfFolderTextBox;
        private System.Windows.Forms.Button pdfBrowseButton;
        private System.Windows.Forms.Label pdfStatusLabel;
        private System.Windows.Forms.GroupBox camFolderGroupBox;
        private System.Windows.Forms.Label camFolderLabel;
        private System.Windows.Forms.TextBox camFolderTextBox;
        private System.Windows.Forms.Button camFolderBrowseButton;
        private System.Windows.Forms.GroupBox camOutputGroupBox;
        private System.Windows.Forms.Label camOutputLabel;
        private System.Windows.Forms.TextBox camOutputTextBox;
        private System.Windows.Forms.Button camOutputBrowseButton;

        private System.Windows.Forms.Button groupsNavButton;
        private System.Windows.Forms.Panel groupsButtonPanel;
        private System.Windows.Forms.Button runGroupButton;
        private System.Windows.Forms.Button cancelGroupButton;
        private System.Windows.Forms.Panel groupsPanel;
        private System.Windows.Forms.SplitContainer groupsSplitContainer;
        private System.Windows.Forms.DataGridView ordersDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn ordersOrderColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ordersCustomerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ordersQtyColumn;
        private System.Windows.Forms.DataGridView productsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodListIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodParametersColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodMaterialColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodThicknessColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodQtyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodRunQtyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn prodHintColumn;
    }
}
