namespace DANG_NHAP_FACEBOOK
{
    public partial class Form1 : Form
    {
        private readonly string dsFilePath;
        private readonly string profileMauPath;
        private readonly string profileRanhPath;

        public Form1()
        {
            InitializeComponent();
            dsFilePath = Path.Combine(AppContext.BaseDirectory, "ds.txt");                    // Đường dẫn đầy đủ tới file ds.txt nằm cạnh file chạy app
            profileMauPath = Path.Combine(AppContext.BaseDirectory, "profile_mau");           // Đường dẫn đầy đủ tới thư mục profile mẫu
            profileRanhPath = Path.Combine(AppContext.BaseDirectory, "profile_ranh");         // Đường dẫn đầy đủ tới thư mục profile rảnh
            LoadDuLieuLenGridKhiMoApp();                                                       // Khi app vừa mở thì nạp lại các profile cũ lên grid để giữ đúng trạng thái hiện có                                                                                              // Dòng này là "chìa khóa" nè bạn
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
