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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            dataGridView1 = new DataGridView();
            colSTT = new DataGridViewTextBoxColumn();
            colChon = new DataGridViewCheckBoxColumn();
            colUID = new DataGridViewTextBoxColumn();
            colPass = new DataGridViewTextBoxColumn();
            colTen = new DataGridViewTextBoxColumn();
            colEmail = new DataGridViewTextBoxColumn();
            colNgayTao = new DataGridViewTextBoxColumn();
            colGhiChu = new DataGridViewTextBoxColumn();
            colTuongTacCuoi = new DataGridViewTextBoxColumn();
            colTrangThai = new DataGridViewTextBoxColumn();
            colCookie = new DataGridViewTextBoxColumn();
            cmsGirdRightClick = new ContextMenuStrip(components);
            điềnUIDPaswordToolStripMenuItem = new ToolStripMenuItem();
            mởToolStripMenuItem = new ToolStripMenuItem();
            làmMớiToolStripMenuItem = new ToolStripMenuItem();
            xóaToolStripMenuItem = new ToolStripMenuItem();
            chọnToolStripMenuItem = new ToolStripMenuItem();
            tToolStripMenuItem = new ToolStripMenuItem();
            cácDòngBôiĐenToolStripMenuItem = new ToolStripMenuItem();
            bỏChọnTấtCảToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            dòngToolStripMenuItem = new ToolStripMenuItem();
            cácDòngBôiĐenToolStripMenuItem1 = new ToolStripMenuItem();
            uIDToolStripMenuItem1 = new ToolStripMenuItem();
            tênToolStripMenuItem1 = new ToolStripMenuItem();
            passToolStripMenuItem = new ToolStripMenuItem();
            emailToolStripMenuItem1 = new ToolStripMenuItem();
            cookieToolStripMenuItem = new ToolStripMenuItem();
            ghiChúToolStripMenuItem1 = new ToolStripMenuItem();
            chứcNăngToolStripMenuItem = new ToolStripMenuItem();
            lọcTheoUIDToolStripMenuItem = new ToolStripMenuItem();
            lọcTàiKhoảnTrùngNhauToolStripMenuItem = new ToolStripMenuItem();
            xóaTàiKhoảnTrùngNhauToolStripMenuItem = new ToolStripMenuItem();
            profileToolStripMenuItem = new ToolStripMenuItem();
            checkProfileToolStripMenuItem = new ToolStripMenuItem();
            xóaProfileToolStripMenuItem = new ToolStripMenuItem();
            dọnDẹpProfileToolStripMenuItem = new ToolStripMenuItem();
            xóaCaToolStripMenuItem = new ToolStripMenuItem();
            cậpNhậtDữLiệuToolStripMenuItem = new ToolStripMenuItem();
            mởChromeMẫuToolStripMenuItem = new ToolStripMenuItem();
            nhậpDanhSáchToolStripMenuItem = new ToolStripMenuItem();
            kiểmTraTàiKhoảnToolStripMenuItem = new ToolStripMenuItem();
            checkWallToolStripMenuItem = new ToolStripMenuItem();
            checkÌnorToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            tssSpace1 = new ToolStripStatusLabel();
            tssTrangThai = new ToolStripStatusLabel();
            tssSpace2 = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tssTong = new ToolStripStatusLabel();
            tssSpace3 = new ToolStripStatusLabel();
            tssTime = new ToolStripStatusLabel();
            lblTieuDe = new Label();
            btnDangNhap = new Button();
            btnTiepTuc = new Button();
            lblUserAgent = new Label();
            cboUserAgent = new ComboBox();
            lblChonGiaoDien = new Label();
            cboUrl = new ComboBox();
            ttMain = new ToolTip(components);
            btnXoa = new Button();
            lblDanhSach = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            btnStop = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            cmsGirdRightClick.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colSTT, colChon, colUID, colPass, colTen, colEmail, colNgayTao, colGhiChu, colTuongTacCuoi, colTrangThai, colCookie });
            dataGridView1.ContextMenuStrip = cmsGirdRightClick;
            resources.ApplyResources(dataGridView1, "dataGridView1");
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            // 
            // colSTT
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSTT.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(colSTT, "colSTT");
            colSTT.Name = "colSTT";
            colSTT.ReadOnly = true;
            colSTT.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colChon
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.ForeColor = Color.Blue;
            dataGridViewCellStyle2.NullValue = false;
            colChon.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(colChon, "colChon");
            colChon.Name = "colChon";
            colChon.ReadOnly = true;
            // 
            // colUID
            // 
            resources.ApplyResources(colUID, "colUID");
            colUID.Name = "colUID";
            colUID.ReadOnly = true;
            colUID.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colPass
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPass.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(colPass, "colPass");
            colPass.Name = "colPass";
            colPass.ReadOnly = true;
            colPass.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colTen
            // 
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTen.DefaultCellStyle = dataGridViewCellStyle4;
            resources.ApplyResources(colTen, "colTen");
            colTen.Name = "colTen";
            colTen.ReadOnly = true;
            colTen.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colEmail
            // 
            resources.ApplyResources(colEmail, "colEmail");
            colEmail.Name = "colEmail";
            colEmail.ReadOnly = true;
            colEmail.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colNgayTao
            // 
            resources.ApplyResources(colNgayTao, "colNgayTao");
            colNgayTao.Name = "colNgayTao";
            colNgayTao.ReadOnly = true;
            colNgayTao.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colGhiChu
            // 
            resources.ApplyResources(colGhiChu, "colGhiChu");
            colGhiChu.Name = "colGhiChu";
            colGhiChu.ReadOnly = true;
            colGhiChu.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colTuongTacCuoi
            // 
            resources.ApplyResources(colTuongTacCuoi, "colTuongTacCuoi");
            colTuongTacCuoi.Name = "colTuongTacCuoi";
            colTuongTacCuoi.ReadOnly = true;
            colTuongTacCuoi.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colTrangThai
            // 
            colTrangThai.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(colTrangThai, "colTrangThai");
            colTrangThai.Name = "colTrangThai";
            colTrangThai.ReadOnly = true;
            colTrangThai.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colCookie
            // 
            resources.ApplyResources(colCookie, "colCookie");
            colCookie.Name = "colCookie";
            colCookie.ReadOnly = true;
            colCookie.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // cmsGirdRightClick
            // 
            cmsGirdRightClick.Items.AddRange(new ToolStripItem[] { điềnUIDPaswordToolStripMenuItem, mởToolStripMenuItem, làmMớiToolStripMenuItem, xóaToolStripMenuItem, chọnToolStripMenuItem, bỏChọnTấtCảToolStripMenuItem, copyToolStripMenuItem, chứcNăngToolStripMenuItem, profileToolStripMenuItem, cậpNhậtDữLiệuToolStripMenuItem, mởChromeMẫuToolStripMenuItem, nhậpDanhSáchToolStripMenuItem, kiểmTraTàiKhoảnToolStripMenuItem });
            cmsGirdRightClick.Name = "cmsGirdRightClick";
            resources.ApplyResources(cmsGirdRightClick, "cmsGirdRightClick");
            // 
            // điềnUIDPaswordToolStripMenuItem
            // 
            điềnUIDPaswordToolStripMenuItem.Name = "điềnUIDPaswordToolStripMenuItem";
            resources.ApplyResources(điềnUIDPaswordToolStripMenuItem, "điềnUIDPaswordToolStripMenuItem");
            điềnUIDPaswordToolStripMenuItem.Click += điềnUIDPaswordToolStripMenuItem_Click;
            // 
            // mởToolStripMenuItem
            // 
            mởToolStripMenuItem.Name = "mởToolStripMenuItem";
            resources.ApplyResources(mởToolStripMenuItem, "mởToolStripMenuItem");
            mởToolStripMenuItem.Click += mởToolStripMenuItem_Click;
            // 
            // làmMớiToolStripMenuItem
            // 
            làmMớiToolStripMenuItem.Name = "làmMớiToolStripMenuItem";
            resources.ApplyResources(làmMớiToolStripMenuItem, "làmMớiToolStripMenuItem");
            làmMớiToolStripMenuItem.Click += làmMớiToolStripMenuItem_Click;
            // 
            // xóaToolStripMenuItem
            // 
            xóaToolStripMenuItem.Name = "xóaToolStripMenuItem";
            resources.ApplyResources(xóaToolStripMenuItem, "xóaToolStripMenuItem");
            xóaToolStripMenuItem.Click += xóaToolStripMenuItem_Click;
            // 
            // chọnToolStripMenuItem
            // 
            chọnToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tToolStripMenuItem, cácDòngBôiĐenToolStripMenuItem });
            chọnToolStripMenuItem.Name = "chọnToolStripMenuItem";
            resources.ApplyResources(chọnToolStripMenuItem, "chọnToolStripMenuItem");
            // 
            // tToolStripMenuItem
            // 
            tToolStripMenuItem.Name = "tToolStripMenuItem";
            resources.ApplyResources(tToolStripMenuItem, "tToolStripMenuItem");
            tToolStripMenuItem.Click += tToolStripMenuItem_Click;
            // 
            // cácDòngBôiĐenToolStripMenuItem
            // 
            cácDòngBôiĐenToolStripMenuItem.Name = "cácDòngBôiĐenToolStripMenuItem";
            resources.ApplyResources(cácDòngBôiĐenToolStripMenuItem, "cácDòngBôiĐenToolStripMenuItem");
            cácDòngBôiĐenToolStripMenuItem.Click += cácDòngBôiĐenToolStripMenuItem_Click;
            // 
            // bỏChọnTấtCảToolStripMenuItem
            // 
            bỏChọnTấtCảToolStripMenuItem.Name = "bỏChọnTấtCảToolStripMenuItem";
            resources.ApplyResources(bỏChọnTấtCảToolStripMenuItem, "bỏChọnTấtCảToolStripMenuItem");
            bỏChọnTấtCảToolStripMenuItem.Click += bỏChọnTấtCảToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dòngToolStripMenuItem, cácDòngBôiĐenToolStripMenuItem1, uIDToolStripMenuItem1, tênToolStripMenuItem1, passToolStripMenuItem, emailToolStripMenuItem1, cookieToolStripMenuItem, ghiChúToolStripMenuItem1 });
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(copyToolStripMenuItem, "copyToolStripMenuItem");
            // 
            // dòngToolStripMenuItem
            // 
            dòngToolStripMenuItem.Name = "dòngToolStripMenuItem";
            resources.ApplyResources(dòngToolStripMenuItem, "dòngToolStripMenuItem");
            dòngToolStripMenuItem.Click += dòngToolStripMenuItem_Click;
            // 
            // cácDòngBôiĐenToolStripMenuItem1
            // 
            cácDòngBôiĐenToolStripMenuItem1.Name = "cácDòngBôiĐenToolStripMenuItem1";
            resources.ApplyResources(cácDòngBôiĐenToolStripMenuItem1, "cácDòngBôiĐenToolStripMenuItem1");
            cácDòngBôiĐenToolStripMenuItem1.Click += cácDòngBôiĐenToolStripMenuItem1_Click;
            // 
            // uIDToolStripMenuItem1
            // 
            uIDToolStripMenuItem1.Name = "uIDToolStripMenuItem1";
            resources.ApplyResources(uIDToolStripMenuItem1, "uIDToolStripMenuItem1");
            uIDToolStripMenuItem1.Click += uIDToolStripMenuItem1_Click;
            // 
            // tênToolStripMenuItem1
            // 
            tênToolStripMenuItem1.Name = "tênToolStripMenuItem1";
            resources.ApplyResources(tênToolStripMenuItem1, "tênToolStripMenuItem1");
            tênToolStripMenuItem1.Click += tênToolStripMenuItem1_Click;
            // 
            // passToolStripMenuItem
            // 
            passToolStripMenuItem.Name = "passToolStripMenuItem";
            resources.ApplyResources(passToolStripMenuItem, "passToolStripMenuItem");
            passToolStripMenuItem.Click += passToolStripMenuItem_Click;
            // 
            // emailToolStripMenuItem1
            // 
            emailToolStripMenuItem1.Name = "emailToolStripMenuItem1";
            resources.ApplyResources(emailToolStripMenuItem1, "emailToolStripMenuItem1");
            emailToolStripMenuItem1.Click += emailToolStripMenuItem1_Click;
            // 
            // cookieToolStripMenuItem
            // 
            cookieToolStripMenuItem.Name = "cookieToolStripMenuItem";
            resources.ApplyResources(cookieToolStripMenuItem, "cookieToolStripMenuItem");
            cookieToolStripMenuItem.Click += cookieToolStripMenuItem_Click;
            // 
            // ghiChúToolStripMenuItem1
            // 
            ghiChúToolStripMenuItem1.Name = "ghiChúToolStripMenuItem1";
            resources.ApplyResources(ghiChúToolStripMenuItem1, "ghiChúToolStripMenuItem1");
            ghiChúToolStripMenuItem1.Click += ghiChúToolStripMenuItem1_Click;
            // 
            // chứcNăngToolStripMenuItem
            // 
            chứcNăngToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { lọcTheoUIDToolStripMenuItem, lọcTàiKhoảnTrùngNhauToolStripMenuItem, xóaTàiKhoảnTrùngNhauToolStripMenuItem });
            chứcNăngToolStripMenuItem.Name = "chứcNăngToolStripMenuItem";
            resources.ApplyResources(chứcNăngToolStripMenuItem, "chứcNăngToolStripMenuItem");
            // 
            // lọcTheoUIDToolStripMenuItem
            // 
            lọcTheoUIDToolStripMenuItem.Name = "lọcTheoUIDToolStripMenuItem";
            resources.ApplyResources(lọcTheoUIDToolStripMenuItem, "lọcTheoUIDToolStripMenuItem");
            // 
            // lọcTàiKhoảnTrùngNhauToolStripMenuItem
            // 
            lọcTàiKhoảnTrùngNhauToolStripMenuItem.Name = "lọcTàiKhoảnTrùngNhauToolStripMenuItem";
            resources.ApplyResources(lọcTàiKhoảnTrùngNhauToolStripMenuItem, "lọcTàiKhoảnTrùngNhauToolStripMenuItem");
            // 
            // xóaTàiKhoảnTrùngNhauToolStripMenuItem
            // 
            xóaTàiKhoảnTrùngNhauToolStripMenuItem.Name = "xóaTàiKhoảnTrùngNhauToolStripMenuItem";
            resources.ApplyResources(xóaTàiKhoảnTrùngNhauToolStripMenuItem, "xóaTàiKhoảnTrùngNhauToolStripMenuItem");
            // 
            // profileToolStripMenuItem
            // 
            profileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { checkProfileToolStripMenuItem, xóaProfileToolStripMenuItem, dọnDẹpProfileToolStripMenuItem, xóaCaToolStripMenuItem });
            profileToolStripMenuItem.Name = "profileToolStripMenuItem";
            resources.ApplyResources(profileToolStripMenuItem, "profileToolStripMenuItem");
            // 
            // checkProfileToolStripMenuItem
            // 
            checkProfileToolStripMenuItem.Name = "checkProfileToolStripMenuItem";
            resources.ApplyResources(checkProfileToolStripMenuItem, "checkProfileToolStripMenuItem");
            // 
            // xóaProfileToolStripMenuItem
            // 
            xóaProfileToolStripMenuItem.Name = "xóaProfileToolStripMenuItem";
            resources.ApplyResources(xóaProfileToolStripMenuItem, "xóaProfileToolStripMenuItem");
            // 
            // dọnDẹpProfileToolStripMenuItem
            // 
            dọnDẹpProfileToolStripMenuItem.Name = "dọnDẹpProfileToolStripMenuItem";
            resources.ApplyResources(dọnDẹpProfileToolStripMenuItem, "dọnDẹpProfileToolStripMenuItem");
            // 
            // xóaCaToolStripMenuItem
            // 
            xóaCaToolStripMenuItem.Name = "xóaCaToolStripMenuItem";
            resources.ApplyResources(xóaCaToolStripMenuItem, "xóaCaToolStripMenuItem");
            // 
            // cậpNhậtDữLiệuToolStripMenuItem
            // 
            cậpNhậtDữLiệuToolStripMenuItem.Name = "cậpNhậtDữLiệuToolStripMenuItem";
            resources.ApplyResources(cậpNhậtDữLiệuToolStripMenuItem, "cậpNhậtDữLiệuToolStripMenuItem");
            cậpNhậtDữLiệuToolStripMenuItem.Click += cậpNhậtDữLiệuToolStripMenuItem_Click;
            // 
            // mởChromeMẫuToolStripMenuItem
            // 
            mởChromeMẫuToolStripMenuItem.Name = "mởChromeMẫuToolStripMenuItem";
            resources.ApplyResources(mởChromeMẫuToolStripMenuItem, "mởChromeMẫuToolStripMenuItem");
            mởChromeMẫuToolStripMenuItem.Click += mởChromeMẫuToolStripMenuItem_Click;
            // 
            // nhậpDanhSáchToolStripMenuItem
            // 
            nhậpDanhSáchToolStripMenuItem.Name = "nhậpDanhSáchToolStripMenuItem";
            resources.ApplyResources(nhậpDanhSáchToolStripMenuItem, "nhậpDanhSáchToolStripMenuItem");
            nhậpDanhSáchToolStripMenuItem.Click += nhậpDanhSáchToolStripMenuItem_Click;
            // 
            // kiểmTraTàiKhoảnToolStripMenuItem
            // 
            kiểmTraTàiKhoảnToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { checkWallToolStripMenuItem, checkÌnorToolStripMenuItem });
            kiểmTraTàiKhoảnToolStripMenuItem.Name = "kiểmTraTàiKhoảnToolStripMenuItem";
            resources.ApplyResources(kiểmTraTàiKhoảnToolStripMenuItem, "kiểmTraTàiKhoảnToolStripMenuItem");
            // 
            // checkWallToolStripMenuItem
            // 
            checkWallToolStripMenuItem.Name = "checkWallToolStripMenuItem";
            resources.ApplyResources(checkWallToolStripMenuItem, "checkWallToolStripMenuItem");
            // 
            // checkÌnorToolStripMenuItem
            // 
            checkÌnorToolStripMenuItem.Name = "checkÌnorToolStripMenuItem";
            resources.ApplyResources(checkÌnorToolStripMenuItem, "checkÌnorToolStripMenuItem");
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { tssSpace1, tssTrangThai, tssSpace2, toolStripStatusLabel1, tssTong, tssSpace3, tssTime });
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Name = "statusStrip1";
            statusStrip1.SizingGrip = false;
            // 
            // tssSpace1
            // 
            resources.ApplyResources(tssSpace1, "tssSpace1");
            tssSpace1.Name = "tssSpace1";
            // 
            // tssTrangThai
            // 
            resources.ApplyResources(tssTrangThai, "tssTrangThai");
            tssTrangThai.ForeColor = Color.Blue;
            tssTrangThai.Name = "tssTrangThai";
            tssTrangThai.Spring = true;
            // 
            // tssSpace2
            // 
            tssSpace2.Name = "tssSpace2";
            resources.ApplyResources(tssSpace2, "tssSpace2");
            // 
            // toolStripStatusLabel1
            // 
            resources.ApplyResources(toolStripStatusLabel1, "toolStripStatusLabel1");
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            // 
            // tssTong
            // 
            resources.ApplyResources(tssTong, "tssTong");
            tssTong.ForeColor = Color.Fuchsia;
            tssTong.Name = "tssTong";
            // 
            // tssSpace3
            // 
            resources.ApplyResources(tssSpace3, "tssSpace3");
            tssSpace3.Name = "tssSpace3";
            // 
            // tssTime
            // 
            resources.ApplyResources(tssTime, "tssTime");
            tssTime.ForeColor = Color.Red;
            tssTime.Name = "tssTime";
            // 
            // lblTieuDe
            // 
            resources.ApplyResources(lblTieuDe, "lblTieuDe");
            lblTieuDe.ForeColor = Color.Blue;
            lblTieuDe.Name = "lblTieuDe";
            // 
            // btnDangNhap
            // 
            btnDangNhap.BackColor = Color.FromArgb(128, 255, 255);
            resources.ApplyResources(btnDangNhap, "btnDangNhap");
            btnDangNhap.ForeColor = Color.Blue;
            btnDangNhap.Name = "btnDangNhap";
            ttMain.SetToolTip(btnDangNhap, resources.GetString("btnDangNhap.ToolTip"));
            btnDangNhap.UseVisualStyleBackColor = false;
            btnDangNhap.Click += btnDangNhap_Click;
            // 
            // btnTiepTuc
            // 
            btnTiepTuc.BackColor = Color.Teal;
            resources.ApplyResources(btnTiepTuc, "btnTiepTuc");
            btnTiepTuc.ForeColor = Color.Yellow;
            btnTiepTuc.Name = "btnTiepTuc";
            ttMain.SetToolTip(btnTiepTuc, resources.GetString("btnTiepTuc.ToolTip"));
            btnTiepTuc.UseVisualStyleBackColor = false;
            btnTiepTuc.Click += btnTiepTuc_Click;
            // 
            // lblUserAgent
            // 
            resources.ApplyResources(lblUserAgent, "lblUserAgent");
            lblUserAgent.Name = "lblUserAgent";
            // 
            // cboUserAgent
            // 
            cboUserAgent.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUserAgent.FormattingEnabled = true;
            resources.ApplyResources(cboUserAgent, "cboUserAgent");
            cboUserAgent.Name = "cboUserAgent";
            ttMain.SetToolTip(cboUserAgent, resources.GetString("cboUserAgent.ToolTip"));
            // 
            // lblChonGiaoDien
            // 
            resources.ApplyResources(lblChonGiaoDien, "lblChonGiaoDien");
            lblChonGiaoDien.Name = "lblChonGiaoDien";
            ttMain.SetToolTip(lblChonGiaoDien, resources.GetString("lblChonGiaoDien.ToolTip"));
            // 
            // cboUrl
            // 
            cboUrl.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUrl.FormattingEnabled = true;
            cboUrl.Items.AddRange(new object[] { resources.GetString("cboUrl.Items"), resources.GetString("cboUrl.Items1"), resources.GetString("cboUrl.Items2") });
            resources.ApplyResources(cboUrl, "cboUrl");
            cboUrl.Name = "cboUrl";
            ttMain.SetToolTip(cboUrl, resources.GetString("cboUrl.ToolTip"));
            // 
            // btnXoa
            // 
            btnXoa.BackColor = Color.FromArgb(255, 192, 192);
            resources.ApplyResources(btnXoa, "btnXoa");
            btnXoa.ForeColor = Color.Red;
            btnXoa.Name = "btnXoa";
            ttMain.SetToolTip(btnXoa, resources.GetString("btnXoa.ToolTip"));
            btnXoa.UseVisualStyleBackColor = false;
            btnXoa.Click += btnXoa_Click;
            // 
            // lblDanhSach
            // 
            resources.ApplyResources(lblDanhSach, "lblDanhSach");
            lblDanhSach.Name = "lblDanhSach";
            ttMain.SetToolTip(lblDanhSach, resources.GetString("lblDanhSach.ToolTip"));
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.FromArgb(192, 192, 255);
            resources.ApplyResources(btnStop, "btnStop");
            btnStop.ForeColor = Color.Fuchsia;
            btnStop.Name = "btnStop";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            ContextMenuStrip = cmsGirdRightClick;
            Controls.Add(btnStop);
            Controls.Add(lblDanhSach);
            Controls.Add(btnXoa);
            Controls.Add(cboUrl);
            Controls.Add(lblChonGiaoDien);
            Controls.Add(cboUserAgent);
            Controls.Add(lblUserAgent);
            Controls.Add(btnTiepTuc);
            Controls.Add(btnDangNhap);
            Controls.Add(lblTieuDe);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            SizeGripStyle = SizeGripStyle.Show;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            cmsGirdRightClick.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private StatusStrip statusStrip1;
        private Label lblTieuDe;
        private Button btnDangNhap;
        private Button btnTiepTuc;
        private Label lblUserAgent;
        private ComboBox cboUserAgent;
        private Label lblChonGiaoDien;
        private ComboBox cboUrl;
        private ContextMenuStrip cmsGirdRightClick;
        private ToolStripMenuItem điềnUIDPaswordToolStripMenuItem;
        private ToolStripMenuItem chọnToolStripMenuItem;
        private ToolStripMenuItem tToolStripMenuItem;
        private ToolStripMenuItem cácDòngBôiĐenToolStripMenuItem;
        private ToolStripMenuItem bỏChọnTấtCảToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem dòngToolStripMenuItem;
        private ToolStripMenuItem cácDòngBôiĐenToolStripMenuItem1;
        private ToolStripMenuItem chứcNăngToolStripMenuItem;
        private ToolStripMenuItem lọcTheoUIDToolStripMenuItem;
        private ToolStripMenuItem lọcTàiKhoảnTrùngNhauToolStripMenuItem;
        private ToolStripMenuItem xóaTàiKhoảnTrùngNhauToolStripMenuItem;
        private ToolStripMenuItem profileToolStripMenuItem;
        private ToolStripMenuItem checkProfileToolStripMenuItem;
        private ToolStripMenuItem xóaProfileToolStripMenuItem;
        private ToolStripMenuItem dọnDẹpProfileToolStripMenuItem;
        private ToolStripMenuItem xóaCaToolStripMenuItem;
        private ToolStripMenuItem xóaToolStripMenuItem;
        private ToolStripMenuItem cậpNhậtDữLiệuToolStripMenuItem;
        private ToolStripMenuItem mởChromeMẫuToolStripMenuItem;
        private ToolStripMenuItem làmMớiToolStripMenuItem;
        private ToolTip ttMain;
        private ToolStripStatusLabel tssSpace1;
        private ToolStripStatusLabel tssTrangThai;
        private ToolStripStatusLabel tssSpace2;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel tssTong;
        private ToolStripStatusLabel tssSpace3;
        private ToolStripStatusLabel tssTime;
        private ToolStripMenuItem nhậpDanhSáchToolStripMenuItem;
        private ToolStripMenuItem uIDToolStripMenuItem1;
        private ToolStripMenuItem tênToolStripMenuItem1;
        private ToolStripMenuItem passToolStripMenuItem;
        private ToolStripMenuItem emailToolStripMenuItem1;
        private ToolStripMenuItem cookieToolStripMenuItem;
        private ToolStripMenuItem ghiChúToolStripMenuItem1;
        private ToolStripMenuItem kiểmTraTàiKhoảnToolStripMenuItem;
        private ToolStripMenuItem checkWallToolStripMenuItem;
        private ToolStripMenuItem checkÌnorToolStripMenuItem;
        private ToolStripMenuItem mởToolStripMenuItem;
        private Button btnXoa;
        private Label lblDanhSach;
        private System.Windows.Forms.Timer timer1;
        private Button btnStop;
        private DataGridViewTextBoxColumn colSTT;
        private DataGridViewCheckBoxColumn colChon;
        private DataGridViewTextBoxColumn colUID;
        private DataGridViewTextBoxColumn colPass;
        private DataGridViewTextBoxColumn colTen;
        private DataGridViewTextBoxColumn colEmail;
        private DataGridViewTextBoxColumn colNgayTao;
        private DataGridViewTextBoxColumn colGhiChu;
        private DataGridViewTextBoxColumn colTuongTacCuoi;
        private DataGridViewTextBoxColumn colTrangThai;
        private DataGridViewTextBoxColumn colCookie;
    }
}
