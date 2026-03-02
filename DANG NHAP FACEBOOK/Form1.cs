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
    }
}
