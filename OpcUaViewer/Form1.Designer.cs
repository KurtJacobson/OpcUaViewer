namespace OpcUaViewer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.monitoringTabPage = new System.Windows.Forms.TabPage();
            this.dataGroupBox = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nodeIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.documentTabPage = new System.Windows.Forms.TabPage();
            this.docViewer = new ShellPreviewPanel();
            this.settingsTabPage = new System.Windows.Forms.TabPage();
            this.connectionGroupBox = new System.Windows.Forms.GroupBox();
            this.endpointLabel = new System.Windows.Forms.Label();
            this.endpointTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.pdfGroupBox = new System.Windows.Forms.GroupBox();
            this.pdfFolderLabel = new System.Windows.Forms.Label();
            this.pdfFolderTextBox = new System.Windows.Forms.TextBox();
            this.pdfBrowseButton = new System.Windows.Forms.Button();
            this.pdfStatusLabel = new System.Windows.Forms.Label();
            this.mainTabControl.SuspendLayout();
            this.monitoringTabPage.SuspendLayout();
            this.dataGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.documentTabPage.SuspendLayout();
            this.settingsTabPage.SuspendLayout();
            this.connectionGroupBox.SuspendLayout();
            this.pdfGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // mainTabControl
            //
            this.mainTabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.mainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.mainTabControl.ItemSize = new System.Drawing.Size(88, 36);
            this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabControl.Controls.Add(this.monitoringTabPage);
            this.mainTabControl.Controls.Add(this.documentTabPage);
            this.mainTabControl.Controls.Add(this.settingsTabPage);
            this.mainTabControl.Location = new System.Drawing.Point(12, 12);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(776, 426);
            this.mainTabControl.TabIndex = 0;
            //
            // monitoringTabPage
            //
            this.monitoringTabPage.Controls.Add(this.dataGroupBox);
            this.monitoringTabPage.Location = new System.Drawing.Point(40, 4);
            this.monitoringTabPage.Name = "monitoringTabPage";
            this.monitoringTabPage.Padding = new System.Windows.Forms.Padding(8);
            this.monitoringTabPage.Size = new System.Drawing.Size(732, 418);
            this.monitoringTabPage.TabIndex = 0;
            this.monitoringTabPage.Text = "Monitor";
            this.monitoringTabPage.UseVisualStyleBackColor = true;
            //
            // dataGroupBox
            //
            this.dataGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGroupBox.Controls.Add(this.dataGridView1);
            this.dataGroupBox.Location = new System.Drawing.Point(8, 8);
            this.dataGroupBox.Name = "dataGroupBox";
            this.dataGroupBox.Size = new System.Drawing.Size(716, 402);
            this.dataGroupBox.TabIndex = 0;
            this.dataGroupBox.TabStop = false;
            this.dataGroupBox.Text = "Monitored Data";
            //
            // dataGridView1
            //
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.nodeIdColumn,
            this.valueColumn,
            this.statusColumn,
            this.timestampColumn});
            this.dataGridView1.Location = new System.Drawing.Point(6, 19);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(704, 377);
            this.dataGridView1.TabIndex = 0;
            //
            // nameColumn
            //
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.ReadOnly = true;
            this.nameColumn.Width = 150;
            //
            // nodeIdColumn
            //
            this.nodeIdColumn.HeaderText = "Node Id";
            this.nodeIdColumn.Name = "nodeIdColumn";
            this.nodeIdColumn.ReadOnly = true;
            this.nodeIdColumn.Width = 260;
            //
            // valueColumn
            //
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            this.valueColumn.ReadOnly = true;
            this.valueColumn.Width = 150;
            //
            // statusColumn
            //
            this.statusColumn.HeaderText = "Status";
            this.statusColumn.Name = "statusColumn";
            this.statusColumn.ReadOnly = true;
            this.statusColumn.Width = 90;
            //
            // timestampColumn
            //
            this.timestampColumn.HeaderText = "Last Updated";
            this.timestampColumn.Name = "timestampColumn";
            this.timestampColumn.ReadOnly = true;
            this.timestampColumn.Width = 110;
            //
            // documentTabPage
            //
            this.documentTabPage.Controls.Add(this.docViewer);
            this.documentTabPage.Location = new System.Drawing.Point(40, 4);
            this.documentTabPage.Name = "documentTabPage";
            this.documentTabPage.Padding = new System.Windows.Forms.Padding(0);
            this.documentTabPage.Size = new System.Drawing.Size(732, 418);
            this.documentTabPage.TabIndex = 1;
            this.documentTabPage.Text = "Document";
            this.documentTabPage.UseVisualStyleBackColor = true;
            //
            // docViewer
            //
            this.docViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.docViewer.BackColor = System.Drawing.Color.White;
            this.docViewer.Location = new System.Drawing.Point(0, 0);
            this.docViewer.Name = "docViewer";
            this.docViewer.Size = new System.Drawing.Size(732, 418);
            this.docViewer.TabIndex = 0;
            //
            // settingsTabPage
            //
            this.settingsTabPage.Controls.Add(this.connectionGroupBox);
            this.settingsTabPage.Controls.Add(this.pdfGroupBox);
            this.settingsTabPage.Location = new System.Drawing.Point(40, 4);
            this.settingsTabPage.Name = "settingsTabPage";
            this.settingsTabPage.Padding = new System.Windows.Forms.Padding(8);
            this.settingsTabPage.Size = new System.Drawing.Size(732, 418);
            this.settingsTabPage.TabIndex = 2;
            this.settingsTabPage.Text = "Settings";
            this.settingsTabPage.UseVisualStyleBackColor = true;
            //
            // connectionGroupBox
            //
            this.connectionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionGroupBox.Controls.Add(this.endpointLabel);
            this.connectionGroupBox.Controls.Add(this.endpointTextBox);
            this.connectionGroupBox.Controls.Add(this.connectButton);
            this.connectionGroupBox.Controls.Add(this.statusLabel);
            this.connectionGroupBox.Location = new System.Drawing.Point(8, 8);
            this.connectionGroupBox.Name = "connectionGroupBox";
            this.connectionGroupBox.Size = new System.Drawing.Size(716, 90);
            this.connectionGroupBox.TabIndex = 0;
            this.connectionGroupBox.TabStop = false;
            this.connectionGroupBox.Text = "OPC UA Connection";
            //
            // endpointLabel
            //
            this.endpointLabel.AutoSize = true;
            this.endpointLabel.Location = new System.Drawing.Point(13, 28);
            this.endpointLabel.Name = "endpointLabel";
            this.endpointLabel.Size = new System.Drawing.Size(52, 13);
            this.endpointLabel.TabIndex = 0;
            this.endpointLabel.Text = "Endpoint:";
            //
            // endpointTextBox
            //
            this.endpointTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.endpointTextBox.Location = new System.Drawing.Point(72, 25);
            this.endpointTextBox.Name = "endpointTextBox";
            this.endpointTextBox.Size = new System.Drawing.Size(532, 20);
            this.endpointTextBox.TabIndex = 1;
            this.endpointTextBox.Text = "opc.tcp://10.10.0.102:4840";
            //
            // connectButton
            //
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connectButton.Location = new System.Drawing.Point(610, 23);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 25);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            //
            // statusLabel
            //
            this.statusLabel.AutoSize = true;
            this.statusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.statusLabel.Location = new System.Drawing.Point(13, 58);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(78, 13);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "Not connected";
            //
            // pdfGroupBox
            //
            this.pdfGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pdfGroupBox.Controls.Add(this.pdfFolderLabel);
            this.pdfGroupBox.Controls.Add(this.pdfFolderTextBox);
            this.pdfGroupBox.Controls.Add(this.pdfBrowseButton);
            this.pdfGroupBox.Controls.Add(this.pdfStatusLabel);
            this.pdfGroupBox.Location = new System.Drawing.Point(8, 110);
            this.pdfGroupBox.Name = "pdfGroupBox";
            this.pdfGroupBox.Size = new System.Drawing.Size(716, 70);
            this.pdfGroupBox.TabIndex = 1;
            this.pdfGroupBox.TabStop = false;
            this.pdfGroupBox.Text = "PDF Documents";
            //
            // pdfFolderLabel
            //
            this.pdfFolderLabel.AutoSize = true;
            this.pdfFolderLabel.Location = new System.Drawing.Point(13, 24);
            this.pdfFolderLabel.Name = "pdfFolderLabel";
            this.pdfFolderLabel.Size = new System.Drawing.Size(43, 13);
            this.pdfFolderLabel.TabIndex = 0;
            this.pdfFolderLabel.Text = "Folder:";
            //
            // pdfFolderTextBox
            //
            this.pdfFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pdfFolderTextBox.Location = new System.Drawing.Point(62, 21);
            this.pdfFolderTextBox.Name = "pdfFolderTextBox";
            this.pdfFolderTextBox.Size = new System.Drawing.Size(542, 20);
            this.pdfFolderTextBox.TabIndex = 1;
            this.pdfFolderTextBox.Text = "C:\\ProductDocs";
            //
            // pdfBrowseButton
            //
            this.pdfBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pdfBrowseButton.Location = new System.Drawing.Point(610, 19);
            this.pdfBrowseButton.Name = "pdfBrowseButton";
            this.pdfBrowseButton.Size = new System.Drawing.Size(100, 25);
            this.pdfBrowseButton.TabIndex = 2;
            this.pdfBrowseButton.Text = "Browse...";
            this.pdfBrowseButton.UseVisualStyleBackColor = true;
            this.pdfBrowseButton.Click += new System.EventHandler(this.pdfBrowseButton_Click);
            //
            // pdfStatusLabel
            //
            this.pdfStatusLabel.AutoSize = true;
            this.pdfStatusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.pdfStatusLabel.Location = new System.Drawing.Point(13, 48);
            this.pdfStatusLabel.Name = "pdfStatusLabel";
            this.pdfStatusLabel.Size = new System.Drawing.Size(126, 13);
            this.pdfStatusLabel.TabIndex = 3;
            this.pdfStatusLabel.Text = "Waiting for a product id...";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "Form1";
            this.Text = "OPC UA Viewer";
            this.mainTabControl.ResumeLayout(false);
            this.monitoringTabPage.ResumeLayout(false);
            this.dataGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.documentTabPage.ResumeLayout(false);
            this.settingsTabPage.ResumeLayout(false);
            this.connectionGroupBox.ResumeLayout(false);
            this.connectionGroupBox.PerformLayout();
            this.pdfGroupBox.ResumeLayout(false);
            this.pdfGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage monitoringTabPage;
        private System.Windows.Forms.GroupBox dataGroupBox;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nodeIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn timestampColumn;
        private System.Windows.Forms.TabPage documentTabPage;
        private ShellPreviewPanel docViewer;
        private System.Windows.Forms.TabPage settingsTabPage;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.Label endpointLabel;
        private System.Windows.Forms.TextBox endpointTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.GroupBox pdfGroupBox;
        private System.Windows.Forms.Label pdfFolderLabel;
        private System.Windows.Forms.TextBox pdfFolderTextBox;
        private System.Windows.Forms.Button pdfBrowseButton;
        private System.Windows.Forms.Label pdfStatusLabel;
    }
}
