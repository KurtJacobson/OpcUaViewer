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
            dataGroupBox = new System.Windows.Forms.GroupBox();
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
            pdfGroupBox = new System.Windows.Forms.GroupBox();
            pdfFolderLabel = new System.Windows.Forms.Label();
            pdfFolderTextBox = new System.Windows.Forms.TextBox();
            pdfBrowseButton = new System.Windows.Forms.Button();
            pdfStatusLabel = new System.Windows.Forms.Label();
            navPanel.SuspendLayout();
            monitoringPanel.SuspendLayout();
            dataGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            documentPanel.SuspendLayout();
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
            settingsNavButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            settingsNavButton.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            settingsNavButton.FlatAppearance.BorderSize = 0;
            settingsNavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            settingsNavButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            settingsNavButton.ForeColor = System.Drawing.Color.FromArgb(170, 170, 170);
            settingsNavButton.Location = new System.Drawing.Point(0, 251);
            settingsNavButton.Name = "settingsNavButton";
            settingsNavButton.Size = new System.Drawing.Size(100, 90);
            settingsNavButton.TabIndex = 2;
            settingsNavButton.Text = "Settings";
            settingsNavButton.UseVisualStyleBackColor = false;
            settingsNavButton.Click += settingsNavButton_Click;
            // 
            // monitoringPanel
            // 
            monitoringPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            monitoringPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            monitoringPanel.Controls.Add(dataGroupBox);
            monitoringPanel.Location = new System.Drawing.Point(100, 0);
            monitoringPanel.Name = "monitoringPanel";
            monitoringPanel.Size = new System.Drawing.Size(1000, 660);
            monitoringPanel.TabIndex = 1;
            // 
            // dataGroupBox
            // 
            dataGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGroupBox.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            dataGroupBox.Controls.Add(dataGridView1);
            dataGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGroupBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            dataGroupBox.Location = new System.Drawing.Point(8, 8);
            dataGroupBox.Name = "dataGroupBox";
            dataGroupBox.Size = new System.Drawing.Size(984, 644);
            dataGroupBox.TabIndex = 0;
            dataGroupBox.TabStop = false;
            dataGroupBox.Text = "Monitored Data";
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
            dataGridView1.Location = new System.Drawing.Point(6, 24);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new System.Drawing.Size(972, 614);
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
            nodeIdColumn.Width = 260;
            // 
            // valueColumn
            // 
            valueColumn.HeaderText = "Value";
            valueColumn.Name = "valueColumn";
            valueColumn.ReadOnly = true;
            valueColumn.Width = 160;
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
            documentPanel.Location = new System.Drawing.Point(100, 0);
            documentPanel.Name = "documentPanel";
            documentPanel.Size = new System.Drawing.Size(1000, 660);
            documentPanel.TabIndex = 2;
            documentPanel.Visible = false;
            // 
            // settingsPanel
            // 
            settingsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            settingsPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            settingsPanel.Controls.Add(connectionGroupBox);
            settingsPanel.Controls.Add(pdfGroupBox);
            settingsPanel.Controls.Add(fullscreenCheckBox);
            settingsPanel.Location = new System.Drawing.Point(100, 0);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new System.Drawing.Size(1000, 660);
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
            // fullscreenCheckBox
            //
            fullscreenCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            fullscreenCheckBox.AutoSize = true;
            fullscreenCheckBox.Font = new System.Drawing.Font("Segoe UI", 11F);
            fullscreenCheckBox.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            fullscreenCheckBox.Location = new System.Drawing.Point(20, 272);
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
            Controls.Add(settingsPanel);
            Font = new System.Drawing.Font("Segoe UI", 11F);
            ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(800, 500);
            Name = "Form1";
            Text = "OPC UA Viewer";
            navPanel.ResumeLayout(false);
            monitoringPanel.ResumeLayout(false);
            dataGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            documentPanel.ResumeLayout(false);
            settingsPanel.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox dataGroupBox;
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
    }
}
