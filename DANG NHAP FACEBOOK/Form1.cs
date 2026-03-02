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
            row.Cells["colChon"].Value = string.Empty;                                        // Cột chọn hiện tại để trống vì chưa xử lý tick ở bước này
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

        private void btnTiepTuc_Click(object sender, EventArgs e)
        {
            XuLyNutNext();                                                                     // Nút btnTiepTuc hiện tại đang đóng vai trò Next
        }

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

        private void XuLyNutNext()
        {
        }

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
    }
}
