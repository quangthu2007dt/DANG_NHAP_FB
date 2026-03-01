namespace DANG_NHAP_FACEBOOK
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dataGridView1 = new DataGridView();
            statusStrip1 = new StatusStrip();
            lblTieuDe = new Label();
            lblUid = new Label();
            lblPass = new Label();
            txtUid = new TextBox();
            txtPass = new TextBox();
            btnDangNhap = new Button();
            btnTiepTuc = new Button();
            lblUserAgent = new Label();
            cboUserAgent = new ComboBox();
            lblChonGiaoDien = new Label();
            comboBox1 = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            resources.ApplyResources(dataGridView1, "dataGridView1");
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 
            // statusStrip1
            // 
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Name = "statusStrip1";
            statusStrip1.SizingGrip = false;
            // 
            // lblTieuDe
            // 
            resources.ApplyResources(lblTieuDe, "lblTieuDe");
            lblTieuDe.ForeColor = Color.Blue;
            lblTieuDe.Name = "lblTieuDe";
            // 
            // lblUid
            // 
            resources.ApplyResources(lblUid, "lblUid");
            lblUid.Name = "lblUid";
            lblUid.Click += lblUid_Click;
            // 
            // lblPass
            // 
            resources.ApplyResources(lblPass, "lblPass");
            lblPass.Name = "lblPass";
            // 
            // txtUid
            // 
            resources.ApplyResources(txtUid, "txtUid");
            txtUid.Name = "txtUid";
            // 
            // txtPass
            // 
            resources.ApplyResources(txtPass, "txtPass");
            txtPass.Name = "txtPass";
            // 
            // btnDangNhap
            // 
            resources.ApplyResources(btnDangNhap, "btnDangNhap");
            btnDangNhap.Name = "btnDangNhap";
            btnDangNhap.UseVisualStyleBackColor = true;
            // 
            // btnTiepTuc
            // 
            resources.ApplyResources(btnTiepTuc, "btnTiepTuc");
            btnTiepTuc.Name = "btnTiepTuc";
            btnTiepTuc.UseVisualStyleBackColor = true;
            // 
            // lblUserAgent
            // 
            resources.ApplyResources(lblUserAgent, "lblUserAgent");
            lblUserAgent.Name = "lblUserAgent";
            // 
            // cboUserAgent
            // 
            resources.ApplyResources(cboUserAgent, "cboUserAgent");
            cboUserAgent.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUserAgent.FormattingEnabled = true;
            cboUserAgent.Name = "cboUserAgent";
            // 
            // lblChonGiaoDien
            // 
            resources.ApplyResources(lblChonGiaoDien, "lblChonGiaoDien");
            lblChonGiaoDien.Name = "lblChonGiaoDien";
            // 
            // comboBox1
            // 
            resources.ApplyResources(comboBox1, "comboBox1");
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { resources.GetString("comboBox1.Items"), resources.GetString("comboBox1.Items1") });
            comboBox1.Name = "comboBox1";
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(comboBox1);
            Controls.Add(lblChonGiaoDien);
            Controls.Add(cboUserAgent);
            Controls.Add(lblUserAgent);
            Controls.Add(btnTiepTuc);
            Controls.Add(btnDangNhap);
            Controls.Add(txtPass);
            Controls.Add(txtUid);
            Controls.Add(lblPass);
            Controls.Add(lblUid);
            Controls.Add(lblTieuDe);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private StatusStrip statusStrip1;
        private Label lblTieuDe;
        private Label lblUid;
        private Label lblPass;
        private TextBox txtUid;
        private TextBox txtPass;
        private Button btnDangNhap;
        private Button btnTiepTuc;
        private Label lblUserAgent;
        private ComboBox cboUserAgent;
        private Label lblChonGiaoDien;
        private ComboBox comboBox1;
    }
}
