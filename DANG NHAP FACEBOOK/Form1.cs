namespace DANG_NHAP_FACEBOOK
{
    public partial class Form1 : Form
    {
        private readonly string dsFilePath;
        private readonly string profileMauPath;
        private readonly string profileRanhPath;
        private const string mobileUserAgentMacDinh = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36";

        public Form1()
        {
            InitializeComponent();
            dsFilePath = Path.Combine(AppContext.BaseDirectory, "ds.txt");                    // Đường dẫn đầy đủ tới file ds.txt nằm cạnh file chạy app
            profileMauPath = Path.Combine(AppContext.BaseDirectory, "profile_mau");           // Đường dẫn đầy đủ tới thư mục profile mẫu
            profileRanhPath = Path.Combine(AppContext.BaseDirectory, "profile_ranh");         // Đường dẫn đầy đủ tới thư mục profile rảnh
            LoadDuLieuLenGridKhiMoApp();                                                       // Khi app vừa mở thì nạp lại các profile cũ lên grid để giữ đúng trạng thái hiện có                                                                                              // Dòng này là "chìa khóa" nè bạn
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (!cboUrl.Items.Contains("m.facebook.com"))
            {
                cboUrl.Items.Add("m.facebook.com");                                            // Bổ sung thêm lựa chọn mobile để app không bị giới hạn chỉ 2 giao diện
            }

            if (cboUrl.Items.Count > 0 && cboUrl.SelectedIndex < 0)
            {
                cboUrl.SelectedIndex = 0;                                                     // Mặc định chọn giao diện đầu tiên để khi bấm Mở dòng không bị thiếu URL
            }
        }

        //
        //  HÀM LÁY DÒNG TÀI KHOẢN TỪ DS.TXT
        //
        private bool TryLayTaiKhoanMoiTuDs(out string uid, out string password)
        {
            uid = string.Empty;                                                               // Giá trị UID trả ra ngoài nếu tìm thấy dòng hợp lệ
            password = string.Empty;                                                          // Giá trị mật khẩu trả ra ngoài nếu tìm thấy dòng hợp lệ

            if (!File.Exists(dsFilePath))                                                     // Nếu chưa có file ds.txt thì chưa thể đọc dữ liệu
            {
                MessageBox.Show("Không tìm thấy file ds.txt.");
                return false;                                                                 // Thoát hàm vì chưa lấy được tài khoản nào
            }

            string[] lines = File.ReadAllLines(dsFilePath);                                   // Đọc toàn bộ các dòng trong ds.txt vào mảng

            if (lines.Length == 0)                                                            // Nếu file không có dòng nào
            {
                MessageBox.Show("ds.txt đang rỗng, vui lòng nhập thêm tài khoản.");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",                                                 // Mở bằng Notepad để người dùng tự nhập thêm
                    Arguments = $"\"{dsFilePath}\"",
                    UseShellExecute = true
                });
                return false;                                                                 // Thoát hàm vì chưa có tài khoản để lấy
            }

            for (int i = 0; i < lines.Length; i++)                                            // Duyệt lần lượt từng dòng trong ds.txt
            {
                string line = lines[i].Trim();                                                // Loại khoảng trắng thừa ở đầu và cuối dòng

                if (string.IsNullOrWhiteSpace(line))                                          // Nếu là dòng trống thì bỏ qua
                {
                    continue;
                }

                string[] parts = line.Split('|');                                             // Tách dòng thành UID và Password bằng dấu |

                if (parts.Length != 2)                                                        // Chỉ chấp nhận đúng 1 dấu | để ra 2 phần tử
                {
                    MessageBox.Show($"Dòng {i + 1} lỗi.");
                    continue;
                }

                string uidTam = parts[0].Trim();                                              // Lấy UID tạm từ phần tử đầu tiên
                string passwordTam = parts[1].Trim();                                         // Lấy Password tạm từ phần tử thứ hai

                if (string.IsNullOrWhiteSpace(uidTam) || string.IsNullOrWhiteSpace(passwordTam)) // Nếu UID hoặc Password bị trống thì xem là dòng lỗi
                {
                    MessageBox.Show($"Dòng {i + 1} lỗi.");
                    continue;
                }

                bool daCoTrenGrid = false;                                                    // Dùng để đánh dấu UID này đã tồn tại trên grid hay chưa

                foreach (DataGridViewRow row in dataGridView1.Rows)                           // Duyệt các dòng hiện có trên grid để kiểm tra trùng UID
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    string uidTrenGrid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty; // Lấy UID đang có trên từng dòng của grid

                    if (string.Equals(uidTrenGrid, uidTam, StringComparison.OrdinalIgnoreCase))          // Nếu UID đã có trên grid thì bỏ qua, lấy dòng kế tiếp
                    {
                        daCoTrenGrid = true;
                        break;
                    }
                }

                if (daCoTrenGrid)
                {
                    continue;
                }

                uid = uidTam;                                                                 // Gán UID hợp lệ đầu tiên chưa có trên grid ra ngoài
                password = passwordTam;                                                       // Gán Password tương ứng ra ngoài
                return true;                                                                  // Báo lấy tài khoản mới thành công
            }

            MessageBox.Show("Không còn tài khoản mới hợp lệ trong ds.txt.");
            return false;                                                                     // Duyệt hết file nhưng không còn dòng nào dùng được
        }
        //
        //  HÀM THÊM DÒNG MỚI TRONG BẢNG (GIRD)
        //
        private void ThemDongMoiLenGrid(string uid, string password, string profileName)
        {
            int rowIndex = dataGridView1.Rows.Add();                                          // Tạo một dòng mới và lấy ra vị trí của dòng vừa thêm
            DataGridViewRow row = dataGridView1.Rows[rowIndex];                               // Lấy đối tượng dòng để đổ dữ liệu vào các cột

            row.Cells["colSTT"].Value = rowIndex + 1;                                         // Đổ số thứ tự theo vị trí hiện tại trên grid
            row.Cells["colChon"].Value = false;                                               // Cột chọn là checkbox nên phải gán giá trị bool để tránh lỗi kiểu dữ liệu
            row.Cells["colUID"].Value = uid;                                                  // Đổ UID vừa lấy được từ ds.txt
            row.Cells["colPass"].Value = password;                                            // Đổ mật khẩu tương ứng với UID
            row.Cells["colNgayTao"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");     // Đổ thời gian tạo dòng theo thời điểm hiện tại
            row.Cells["colTen"].Value = string.Empty;                                         // Tên để trống, sau này người dùng hoặc code sẽ cập nhật
            row.Cells["colEmail"].Value = string.Empty;                                       // Email để trống ở bước hiện tại
            row.Cells["colGhiChu"].Value = string.Empty;                                      // Ghi chú để trống ở bước hiện tại
            row.Cells["colTuongTacCuoi"].Value = string.Empty;                                // Tương tác cuối để trống ở bước hiện tại
            row.Cells["colTrangThai"].Value = string.Empty;                                   // Trạng thái để trống, ưu tiên hiện ở label phía dưới
            row.Cells["colCookie"].Value = string.Empty;                                      // Cookie để trống, sau này mới tính tới

            if (dataGridView1.Columns.Contains("colProfileName"))                             // Nếu sau này có cột profile riêng thì đổ thêm ProfileName vào đó
            {
                row.Cells["colProfileName"].Value = profileName;
            }

            dataGridView1.ClearSelection();                                                   // Bỏ toàn bộ lựa chọn cũ để chỉ giữ đúng dòng mới
            row.Selected = true;                                                              // Tự chọn ngay dòng vừa được thêm
            dataGridView1.CurrentCell = row.Cells["colUID"];                                  // Đưa con trỏ hiện tại về cột UID của dòng mới
        }
        //
        //   HÀM XỬ LÝ SỰ KIỆN NÚT TIẾP TỤC
        //
        private void btnTiepTuc_Click(object sender, EventArgs e)
        {
            XuLyNutNext();                                                                     // Nút btnTiepTuc hiện tại đang đóng vai trò Next
        }

        private void mởChromeMẫuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoChromeMau();                                                                     // Menu "Mở Chrome mẫu" chỉ gọi đúng hàm mở profile_mau
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            MoProfileTheoDongDangChon();                                                       // Nút Đăng Nhập hiện tại đóng vai trò mở lại profile của dòng đang chọn
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)                                                                // Bỏ qua khi người dùng bấm vào phần tiêu đề hoặc vùng không phải dữ liệu
            {
                return;
            }

            DataGridViewColumn? cotChon = dataGridView1.Columns["colChon"];                   // Lấy cột checkbox Chọn để so đúng vị trí cột cần xử lý
            if (cotChon == null || e.ColumnIndex != cotChon.Index)                            // Chỉ xử lý khi người dùng bấm đúng vào cột checkbox Chọn
            {
                return;
            }

            DaoTrangThaiChonCuaDong(dataGridView1.Rows[e.RowIndex]);                           // Click vào checkbox sẽ được coi là thao tác chọn thật của app
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)                                                                // Bỏ qua khi double click không rơi vào một dòng dữ liệu hợp lệ
            {
                return;
            }

            DataGridViewColumn? cotChon = dataGridView1.Columns["colChon"];                   // Lấy cột checkbox Chọn để tránh double click bị xử lý đè lên click thường
            if (cotChon != null && e.ColumnIndex == cotChon.Index)                            // Nếu double click ngay trên checkbox thì giữ cho CellClick tự xử lý, tránh đổi trạng thái hai lần
            {
                return;
            }

            DaoTrangThaiChonCuaDong(dataGridView1.Rows[e.RowIndex]);                           // Double click vào dòng cũng được coi là thao tác chọn thật
        }

        private void mởToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoProfileTheoDongDangChon();                                                       // Menu Mở cũng dùng chung luồng mở profile của dòng đang chọn
        }

        private void dòngToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            XoaMotDongDaTick();                                                                // Menu xóa dòng chỉ xử lý khi có đúng 1 dòng đang được tick thật
        }

        private void cácDòngĐãChọnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XoaNhieuDongDaTick();                                                              // Menu xóa nhiều dòng dùng danh sách checkbox đang được tick
        }
        // 
        //  HÀM TẠO PRORILE RẢNH NẾU CHƯA CÓ KHI NÚT CHỌN DC GỌI
        //
        private bool TimProfileRanhHoacTaoMoi(out string duongDanProfileSuDung)
        {
            duongDanProfileSuDung = string.Empty;                                              // Đường dẫn profile sẽ trả ra ngoài nếu tìm hoặc tạo được

            if (Directory.Exists(profileRanhPath))                                             // Nếu đã có sẵn profile_ranh thì dùng luôn profile này
            {
                duongDanProfileSuDung = profileRanhPath;
                return true;                                                                   // Báo đã lấy được profile sẵn sàng để dùng
            }

            if (!Directory.Exists(profileMauPath))                                             // Nếu không có profile_mau thì không thể tạo profile mới
            {
                MessageBox.Show("Không tìm thấy thư mục profile_mau.");
                return false;                                                                  // Thoát hàm vì thiếu profile gốc để sao chép
            }

            CopyDirectory(profileMauPath, profileRanhPath);                                    // Sao chép toàn bộ dữ liệu từ profile_mau sang profile_ranh
            duongDanProfileSuDung = profileRanhPath;                                           // Sau khi sao chép xong thì profile_ranh chính là profile dùng được
            return true;                                                                       // Báo đã tạo thành công profile để dùng cho bước tiếp theo
        }
        //
        //  HÀM XỬ LÝ KHI CHỌN NÚT TIẾP TỤC
        //
        private void XuLyNutNext()
        {
            if (!TryLayTaiKhoanMoiTuDs(out string uid, out string password))                  // Nếu chưa lấy được tài khoản mới từ ds.txt thì dừng tại đây
            {
                return;
            }

            if (!TimProfileRanhHoacTaoMoi(out string duongDanProfileSuDung))                  // Nếu chưa lấy hoặc tạo được profile sẵn sàng thì dừng tại đây
            {
                return;
            }

            string duongDanProfileTheoUid = Path.Combine(AppContext.BaseDirectory, uid);      // Tạo đường dẫn profile mới theo đúng tên UID của tài khoản

            if (string.Equals(duongDanProfileSuDung, duongDanProfileTheoUid, StringComparison.OrdinalIgnoreCase)) // Nếu profile hiện tại đã mang đúng tên UID thì không cần đổi tên
            {
                ThemDongMoiLenGrid(uid, password, uid);
                return;
            }

            if (Directory.Exists(duongDanProfileTheoUid))                                     // Nếu thư mục profile theo UID đã tồn tại sẵn thì báo để tránh ghi đè nhầm
            {
                MessageBox.Show($"Profile {uid} đã tồn tại.");
                return;
            }

            Directory.Move(duongDanProfileSuDung, duongDanProfileTheoUid);                    // Đổi tên thư mục profile đang dùng sang đúng UID của tài khoản mới
            ThemDongMoiLenGrid(uid, password, uid);                                           // Sau khi đã có profile đúng tên, thêm ngay dòng mới lên grid
        }
        //
        //   HÀM MỞ CHROME MẪU
        //
        private void MoChromeMau()
        {
            Directory.CreateDirectory(profileMauPath);                                         // Nếu chưa có profile_mau thì tạo mới để luôn có nơi chuẩn bị profile gốc

            string chromeExe = TimChromeExe();                                                 // Tìm đường dẫn chrome.exe từ các vị trí cài đặt thông dụng
            if (string.IsNullOrWhiteSpace(chromeExe))
            {
                MessageBox.Show("Không tìm thấy chrome.exe.");
                return;                                                                        // Không mở tiếp nếu máy chưa tìm thấy Chrome
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = chromeExe,
                Arguments = $"--user-data-dir=\"{profileMauPath}\"",                          // Mở Chrome bằng đúng thư mục profile_mau để người dùng cài extension và chuẩn bị môi trường gốc
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(psi);                                             // Mở Chrome mẫu để người dùng cấu hình các thành phần cần thiết
        }
        //
        //  HÀM MỞ CHROM THEO DÒNG
        //
        private void MoProfileTheoDongDangChon()
        {
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy danh sách dòng được chọn thật theo checkbox, không dùng bôi đen
            if (dsDongDaTick.Count != 1)                                                       // Chỉ cho phép mở khi đang tick đúng 1 dòng
            {
                MessageBox.Show("Vui lòng tick đúng 1 dòng để mở profile.");
                return;
            }

            DataGridViewRow row = dsDongDaTick[0];                                             // Lấy đúng dòng đang được tick để xác định profile cần mở
            string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;        // UID của dòng đang chọn cũng chính là tên thư mục profile

            if (string.IsNullOrWhiteSpace(uid))
            {
                MessageBox.Show("Dòng đang chọn không có UID hợp lệ.");
                return;                                                                        // Không thể mở profile nếu dòng chưa có UID
            }

            string duongDanProfile = Path.Combine(AppContext.BaseDirectory, uid);              // Tạo đường dẫn đầy đủ tới profile mang tên UID
            if (!Directory.Exists(duongDanProfile))
            {
                MessageBox.Show($"Không tìm thấy profile {uid}.");
                return;                                                                        // Thoát nếu profile tương ứng đã bị thiếu hoặc chưa được tạo
            }

            string chromeExe = TimChromeExe();                                                 // Tìm đường dẫn chrome.exe để mở profile bằng Chrome thật
            if (string.IsNullOrWhiteSpace(chromeExe))
            {
                MessageBox.Show("Không tìm thấy chrome.exe.");
                return;                                                                        // Không mở tiếp nếu máy chưa tìm thấy Chrome
            }

            string urlCanMo = LayUrlFacebookDaChon();                                          // Lấy đúng URL Facebook theo giao diện đang chọn trên combobox
            string thamSoUserAgent = LayThamSoUserAgentTheoGiaoDien(urlCanMo);                 // Nếu là giao diện mobile thì ghép thêm User-Agent mobile cố định

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = chromeExe,
                Arguments = $"--user-data-dir=\"{duongDanProfile}\" {thamSoUserAgent} {urlCanMo}".Trim(), // Mở Chrome bằng đúng profile của UID đang chọn, kèm User-Agent nếu cần và đi tới URL đã chọn
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(psi);                                             // Mở lại profile cũ đúng với dòng đang chọn trên grid
        }
        //
        //  HÀM LẤY URL FACEBOOK ĐANG CHỌN
        //
        private string LayUrlFacebookDaChon()
        {
            string luaChon = cboUrl.SelectedItem?.ToString()?.Trim() ?? string.Empty;         // Lấy nội dung người dùng đang chọn trong combobox giao diện

            if (luaChon.Contains("m.facebook", StringComparison.OrdinalIgnoreCase))
            {
                return "https://m.facebook.com/";                                              // Nếu chọn giao diện mobile thì mở đúng URL mobile
            }

            if (luaChon.Contains("meta", StringComparison.OrdinalIgnoreCase))
            {
                return "https://facebook.com/meta";                                            // Nếu chọn giao diện meta thì mở đúng URL meta
            }

            return "https://facebook.com/";                                                    // Mặc định còn lại sẽ mở giao diện Facebook thông thường
        }
        //
        //  HÀM LẤY THAM SỐ USER-AGENT THEO GIAO DIỆN
        //
        private string LayThamSoUserAgentTheoGiaoDien(string urlCanMo)
        {
            if (urlCanMo.Contains("m.facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                return $"--user-agent=\"{mobileUserAgentMacDinh}\"";                           // Nếu mở giao diện mobile thì ép luôn User-Agent mobile mặc định
            }

            return string.Empty;                                                               // Các giao diện còn lại tạm thời chưa cần ép User-Agent
        }
        //
        //  HÀM ĐẢO TRẠNG THÁI TICK CỦA DÒNG
        //
        private void DaoTrangThaiChonCuaDong(DataGridViewRow row)
        {
            bool dangDuocTick = row.Cells["colChon"].Value is bool giaTriTick && giaTriTick;   // Đọc trạng thái hiện tại của checkbox để biết dòng đang được chọn thật hay chưa
            row.Cells["colChon"].Value = !dangDuocTick;                                        // Đảo lại trạng thái tick khi người dùng click hoặc double click
            dataGridView1.ClearSelection();                                                    // Bỏ bôi đen để app chỉ coi checkbox là trạng thái chọn thật
        }
        //
        //  HÀM LẤY DANH SÁCH DÒNG ĐÃ TICK
        //
        private List<DataGridViewRow> LayDanhSachDongDaTick()
        {
            List<DataGridViewRow> ketQua = new();                                              // Gom các dòng đang được tick thật để dùng chung cho Mở dòng và Xóa dòng

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                if (row.Cells["colChon"].Value is bool daTick && daTick)
                {
                    ketQua.Add(row);                                                           // Chỉ thêm các dòng có checkbox đang bật
                }
            }

            return ketQua;                                                                     // Trả lại toàn bộ danh sách dòng đã chọn thật theo cột Chọn
        }
        //
        //  HÀM XÓA MỘT DÒNG ĐÃ TICK
        //
        private void XoaMotDongDaTick()
        {
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy danh sách tick thật để bảo đảm xóa đúng nghiệp vụ đã chốt
            if (dsDongDaTick.Count != 1)
            {
                MessageBox.Show("Vui lòng tick đúng 1 dòng để xóa.");                          // Xóa một dòng chỉ chấp nhận đúng 1 checkbox đang bật
                return;
            }

            DataGridViewRow row = dsDongDaTick[0];                                             // Lấy dòng duy nhất đang được tick để xử lý xóa
            string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;        // UID dùng để xóa khỏi ds.txt và xử lý profile tương ứng

            if (string.IsNullOrWhiteSpace(uid))
            {
                MessageBox.Show("Dòng đang tick không có UID hợp lệ.");
                return;
            }

            XoaUidKhoiDsTxt(uid);                                                              // Xóa UID tương ứng ra khỏi ds.txt để dữ liệu file và grid luôn đồng bộ
            XuLyProfileKhiXoaMotDong(uid);                                                     // Xóa profile theo UID và giữ lại tối đa 1 profile_ranh sạch để dùng cho lần sau
            dataGridView1.Rows.Remove(row);                                                    // Bỏ dòng đã xóa ra khỏi grid
            CapNhatLaiSTT();                                                                   // Đánh lại số thứ tự để bảng không bị lệch sau khi xóa
        }
        //
        //  HÀM XÓA NHIỀU DÒNG ĐÃ TICK
        //
        private void XoaNhieuDongDaTick()
        {
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy toàn bộ dòng đang được tick thật để xử lý xóa hàng loạt
            if (dsDongDaTick.Count < 2)
            {
                MessageBox.Show("Vui lòng tick từ 2 dòng trở lên để xóa nhiều.");             // Nhánh này chỉ dành cho xóa nhiều dòng cùng lúc
                return;
            }

            foreach (DataGridViewRow row in dsDongDaTick)
            {
                string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;    // Lấy UID từng dòng để xóa khỏi ds.txt và xóa profile tương ứng

                if (string.IsNullOrWhiteSpace(uid))
                {
                    continue;
                }

                XoaUidKhoiDsTxt(uid);                                                          // Xóa toàn bộ UID đang tick ra khỏi file nguồn
                XoaProfileTheoUid(uid);                                                        // Xóa hẳn profile của các dòng bị xóa hàng loạt để tránh phình số lượng profile
            }

            for (int i = dsDongDaTick.Count - 1; i >= 0; i--)
            {
                dataGridView1.Rows.Remove(dsDongDaTick[i]);                                    // Xóa từ cuối về đầu để tránh lệch chỉ số khi bỏ nhiều dòng
            }

            CapNhatLaiSTT();                                                                   // Đánh lại STT sau khi xóa hàng loạt
        }
        //
        //  HÀM XÓA UID KHỎI DS.TXT
        //
        private void XoaUidKhoiDsTxt(string uidCanXoa)
        {
            if (!File.Exists(dsFilePath))
            {
                return;                                                                        // Nếu chưa có ds.txt thì không có gì để đồng bộ xóa
            }

            string[] lines = File.ReadAllLines(dsFilePath);                                    // Đọc lại toàn bộ file để lọc bỏ đúng UID cần xóa
            List<string> dsConLai = new();                                                     // Giữ lại các dòng không thuộc UID bị xóa để ghi đè lại file

            foreach (string line in lines)
            {
                string lineDaCat = line.Trim();                                                // Chuẩn hóa từng dòng trước khi kiểm tra định dạng UID|Password

                if (string.IsNullOrWhiteSpace(lineDaCat))
                {
                    continue;
                }

                string[] parts = lineDaCat.Split('|');                                         // Tách UID và Password để so đúng UID cần xóa

                if (parts.Length == 2 && string.Equals(parts[0].Trim(), uidCanXoa, StringComparison.OrdinalIgnoreCase))
                {
                    continue;                                                                   // Bỏ qua đúng dòng thuộc UID cần xóa để nó không còn trong ds.txt
                }

                dsConLai.Add(line);                                                            // Giữ nguyên các dòng còn lại để không làm mất dữ liệu khác
            }

            File.WriteAllLines(dsFilePath, dsConLai);                                          // Ghi lại file sau khi đã bỏ các dòng thuộc UID bị xóa
        }
        //
        //  HÀM XỬ LÝ PROFILE KHI XÓA MỘT DÒNG
        //
        private void XuLyProfileKhiXoaMotDong(string uid)
        {
            string duongDanProfileTheoUid = Path.Combine(AppContext.BaseDirectory, uid);       // Xác định đúng thư mục profile theo UID của dòng bị xóa
            if (!Directory.Exists(duongDanProfileTheoUid))
            {
                return;                                                                        // Nếu không còn profile thì chỉ cần xóa dữ liệu grid và ds.txt
            }

            if (Directory.Exists(profileRanhPath))
            {
                Directory.Delete(duongDanProfileTheoUid, true);                                // Nếu đã có sẵn profile_ranh thì chỉ xóa profile theo UID để giữ đúng một profile rảnh duy nhất
                return;
            }

            if (Directory.Exists(profileMauPath))
            {
                Directory.Delete(duongDanProfileTheoUid, true);                                // Xóa profile cũ của UID để tránh giữ lại dữ liệu phiên cũ
                CopyDirectory(profileMauPath, profileRanhPath);                                // Tạo lại profile_ranh sạch từ profile_mau để dùng cho lần Next sau
                return;
            }

            Directory.Move(duongDanProfileTheoUid, profileRanhPath);                           // Nếu thiếu profile_mau thì giữ lại tối thiểu một profile_ranh bằng cách đổi tên profile vừa xóa
        }
        //
        //  HÀM XÓA HẲN PROFILE THEO UID
        //
        private void XoaProfileTheoUid(string uid)
        {
            string duongDanProfileTheoUid = Path.Combine(AppContext.BaseDirectory, uid);       // Xác định thư mục profile cần xóa hẳn khỏi ổ đĩa
            if (!Directory.Exists(duongDanProfileTheoUid))
            {
                return;                                                                        // Nếu profile không tồn tại thì bỏ qua để không làm phát sinh lỗi
            }

            Directory.Delete(duongDanProfileTheoUid, true);                                    // Xóa sạch profile của các dòng bị loại khỏi app
        }
        //
        //  HÀM CẬP NHẬT LẠI STT
        //
        private void CapNhatLaiSTT()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["colSTT"].Value = i + 1;                           // Sau khi xóa dòng thì đánh lại STT để bảng luôn liên tục và dễ nhìn
            }
        }
        //
        //   HÀM LOAD DỮ LIỆU KHI MỞ APP
        //
        private void LoadDuLieuLenGridKhiMoApp()
        {
            Dictionary<string, string> matKhauTheoUid = new(StringComparer.OrdinalIgnoreCase); // Lưu tạm mật khẩu theo UID để đồng bộ lại dữ liệu khi nạp grid lúc mở app
            if (File.Exists(dsFilePath))
            {
                string[] lines = File.ReadAllLines(dsFilePath);                                // Đọc ds.txt để đối chiếu UID và mật khẩu tương ứng

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();                                             // Loại khoảng trắng thừa trước khi kiểm tra định dạng từng dòng

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split('|');                                          // Tách UID và mật khẩu từ từng dòng trong ds.txt

                    if (parts.Length != 2)
                    {
                        continue;                                                               // Bỏ qua dòng lỗi để việc nạp dữ liệu cũ không bị dừng
                    }

                    string uid = parts[0].Trim();                                              // Lấy UID từ phần tử đầu tiên của dòng hợp lệ
                    string password = parts[1].Trim();                                         // Lấy mật khẩu từ phần tử thứ hai của dòng hợp lệ

                    if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
                    {
                        continue;                                                               // Bỏ qua dòng lỗi hoặc thiếu dữ liệu
                    }

                    if (!matKhauTheoUid.ContainsKey(uid))
                    {
                        matKhauTheoUid.Add(uid, password);                                      // Chỉ lấy giá trị đầu tiên của UID để tránh ghi đè không rõ nguyên nhân
                    }
                }
            }

            string[] thuMucProfiles = Directory.GetDirectories(AppContext.BaseDirectory);     // Lấy toàn bộ thư mục đang nằm cạnh file chạy app

            foreach (string thuMucProfile in thuMucProfiles)                                   // Duyệt lần lượt từng thư mục để tìm các profile đã có
            {
                string tenThuMuc = Path.GetFileName(thuMucProfile);                            // Lấy tên thư mục để phân biệt profile nào cần bỏ qua, profile nào cần nạp

                if (string.Equals(tenThuMuc, "profile_mau", StringComparison.OrdinalIgnoreCase)) // Bỏ qua thư mục profile mẫu vì đây không phải tài khoản thật
                {
                    continue;
                }

                if (string.Equals(tenThuMuc, "profile_ranh", StringComparison.OrdinalIgnoreCase)) // Bỏ qua thư mục profile rảnh vì đây là profile chờ tái sử dụng
                {
                    continue;
                }

                int rowIndex = dataGridView1.Rows.Add();                                       // Tạo dòng mới trên grid để đổ dữ liệu profile cũ lên
                DataGridViewRow row = dataGridView1.Rows[rowIndex];                            // Lấy đối tượng dòng vừa tạo để gán dữ liệu theo từng cột

                row.Cells["colSTT"].Value = rowIndex + 1;                                      // Đánh số thứ tự theo vị trí hiện tại trên grid
                row.Cells["colChon"].Value = false;                                            // Cột chọn là checkbox nên phải gán giá trị bool để tránh lỗi kiểu dữ liệu
                row.Cells["colUID"].Value = tenThuMuc;                                         // Tên thư mục hiện tại chính là UID và cũng là profileName của tài khoản
                row.Cells["colPass"].Value = matKhauTheoUid.TryGetValue(tenThuMuc, out string? password) ? password ?? string.Empty : string.Empty; // Nếu UID còn trong ds.txt thì đổ lại đúng mật khẩu, còn null thì ép về chuỗi rỗng để tránh warning nullable
                row.Cells["colTen"].Value = string.Empty;                                      // Tên để trống, sau này có thể cập nhật tay hoặc bằng code
                row.Cells["colEmail"].Value = string.Empty;                                    // Email để trống ở bước hiện tại
                row.Cells["colNgayTao"].Value = Directory.GetCreationTime(thuMucProfile).ToString("dd/MM/yyyy HH:mm:ss"); // Lấy ngày tạo thư mục làm ngày tạo dòng hiển thị
                row.Cells["colGhiChu"].Value = string.Empty;                                   // Ghi chú để trống ở bước hiện tại
                row.Cells["colTuongTacCuoi"].Value = string.Empty;                             // Tương tác cuối để trống ở bước hiện tại
                row.Cells["colTrangThai"].Value = string.Empty;                                // Trạng thái để trống, sau này có thể dùng label/status hoặc cập nhật riêng
                row.Cells["colCookie"].Value = string.Empty;                                   // Cookie để trống vì chưa có bước đọc dữ liệu phiên
            }
        }
        //
        //  HÀM TẠO PROFILE RẢNH NẾU CHƯA CÓ
        //
        private void CopyDirectory(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);                                        // Tạo thư mục đích nếu chưa có để sẵn sàng nhận dữ liệu được sao chép

            foreach (string filePath in Directory.GetFiles(sourcePath))                        // Duyệt toàn bộ file trực tiếp trong thư mục nguồn
            {
                string fileName = Path.GetFileName(filePath);                                  // Lấy riêng tên file để ghép sang thư mục đích
                string destinationFilePath = Path.Combine(destinationPath, fileName);          // Tạo đường dẫn đầy đủ cho file đích tương ứng
                File.Copy(filePath, destinationFilePath, true);                                // Sao chép file và cho phép ghi đè nếu file đích đã tồn tại
            }

            foreach (string directoryPath in Directory.GetDirectories(sourcePath))             // Duyệt toàn bộ thư mục con trong thư mục nguồn
            {
                string directoryName = Path.GetFileName(directoryPath);                        // Lấy tên thư mục con để tái tạo bên thư mục đích
                string destinationDirectoryPath = Path.Combine(destinationPath, directoryName);// Tạo đường dẫn đầy đủ cho thư mục con đích
                CopyDirectory(directoryPath, destinationDirectoryPath);                         // Gọi đệ quy để sao chép tiếp toàn bộ nội dung thư mục con
            }
        }

        private string TimChromeExe()
        {
            string chrome1 = @"C:\Program Files\Google\Chrome\Application\chrome.exe";        // Vị trí cài đặt Chrome phổ biến trên Windows 64-bit
            string chrome2 = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";  // Vị trí cài đặt Chrome phổ biến trên một số máy khác

            if (File.Exists(chrome1)) return chrome1;
            if (File.Exists(chrome2)) return chrome2;

            return string.Empty;                                                                // Trả rỗng nếu chưa tìm thấy chrome.exe ở các vị trí đang hỗ trợ
        }

    }
}
