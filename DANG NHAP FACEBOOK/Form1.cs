using System.Text;
using System.Text.Json;

namespace DANG_NHAP_FACEBOOK
{
    public partial class Form1 : Form
    {
        private readonly string dsFilePath;
        private readonly string profileMauPath;
        private readonly string profileRanhPath;
        private readonly string userAgentDangDungFilePath;
        private readonly string userAgentsFilePath;
        private readonly Dictionary<string, int> congDebugTheoUid = new(StringComparer.OrdinalIgnoreCase);
        private const string facebookDesktopUserAgentMacDinh = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";
        private const string safariIphoneUserAgentMacDinh = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 Safari/604.1";
        private const string mobileUserAgentMacDinh = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36";
        private const string metaDesktopUserAgentMacDinh = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";

        public Form1()
        {
            InitializeComponent();
            userAgentsFilePath = Path.Combine(AppContext.BaseDirectory, "user_agents.txt");         // File txt l?u danh s?ch User-Agent ?? app c? th? n?p l?n combobox
            userAgentDangDungFilePath = Path.Combine(AppContext.BaseDirectory, "ua_dang_dung.txt"); // File txt lưu lại URL và User-Agent đang dùng để sau này còn lần theo vết
            dsFilePath = Path.Combine(AppContext.BaseDirectory, "ds.txt");                    // Đường dẫn đầy đủ tới file ds.txt nằm cạnh file chạy app
            profileMauPath = Path.Combine(AppContext.BaseDirectory, "profile_mau");           // Đường dẫn đầy đủ tới thư mục profile mẫu
            profileRanhPath = Path.Combine(AppContext.BaseDirectory, "profile_ranh");         // Đường dẫn đầy đủ tới thư mục profile rảnh
            LoadDuLieuLenGridKhiMoApp();                                                       // Khi app vừa mở thì nạp lại các profile cũ lên grid để giữ đúng trạng thái hiện có                                                                                              // Dòng này là "chìa khóa" nè bạn
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.RowsAdded += (_, _) => CapNhatThongTinSoLuong();                    // Chạy sau cùng để lblDanhSach đọc đúng số dòng còn lại trong ds.txt
            dataGridView1.RowsRemoved += (_, _) => CapNhatThongTinSoLuong();                  // Chạy sau cùng để Tổng luôn bám đúng số dòng hiện còn trên grid
            CapNhatThongTinSoLuong();                                                         // Ghi lại đúng ý nghĩa mới của lblDanhSach và Tổng sau khi đã nạp xong dữ liệu

            if (!cboUrl.Items.Contains("m.facebook.com"))
            {
                cboUrl.Items.Add("m.facebook.com");                                            // Bổ sung thêm lựa chọn mobile để app không bị giới hạn chỉ 2 giao diện
            }

            if (cboUrl.Items.Count > 0 && cboUrl.SelectedIndex < 0)
            {
                cboUrl.SelectedIndex = 0;                                                     // Mặc định chọn giao diện đầu tiên để khi bấm Mở dòng không bị thiếu URL
            }

            DamBaoTonTaiUserAgentsTxt();                                                       // ??m b?o lu?n c? file user_agents.txt ?? kh?ng hardcode danh s?ch User-Agent tr?n UI
            TaiDanhSachUserAgentLenCombobox();                                                 // N?p danh s?ch User-Agent t? file txt l?n combobox ngay khi app kh?i ??ng
        }

        //
        //  HÀM CẬP NHẬT SỐ LƯỢNG
        //
        private void CapNhatThongTinSoLuong()
        {
            int soLuongDsTxtConLai = 0;                                                       // Đếm số dòng còn lại trong ds.txt để đổ đúng lên label Danh Sách
            if (File.Exists(dsFilePath))
            {
                soLuongDsTxtConLai = File.ReadAllLines(dsFilePath)
                    .Count(line => !string.IsNullOrWhiteSpace(line));                         // Chỉ tính các dòng còn dữ liệu thực trong ds.txt
            }

            lblDanhSach.Text = $"Số lượng DS.txt : {soLuongDsTxtConLai}";                     // Label Danh Sách phản ánh số dòng còn lại trong ds.txt
            tssTong.Text = $"Tổng : {dataGridView1.Rows.Count}";                               // Thanh trạng thái Tổng phản ánh số dòng hiện đang có trên grid
        }

        private void DamBaoTonTaiUserAgentsTxt()
        {
            string[] danhSachMacDinh =
            [
                facebookDesktopUserAgentMacDinh,                                               // Gi? s?n UA desktop ?ang test ?n cho facebook.com
                safariIphoneUserAgentMacDinh                                                   // Th?m s?n m?t UA Safari iPhone ?? sau n?y c?n test giao di?n c?
            ];
            if (!File.Exists(userAgentsFilePath))
            {
                File.WriteAllLines(userAgentsFilePath, danhSachMacDinh, Encoding.UTF8);        // N?u ch?a c? file th? t?o m?i v? ghi s?n 2 UA m?c ??nh
                return;
            }
            bool fileDangRong = File.ReadAllLines(userAgentsFilePath)
                .All(line => string.IsNullOrWhiteSpace(line));                                 // Neu file dang co nhung khong chua UA nao thi coi nhu rong
            if (fileDangRong)
            {
                File.WriteAllLines(userAgentsFilePath, danhSachMacDinh, Encoding.UTF8);        // Ghi l?i danh s?ch m?c ??nh ?? combobox kh?ng b? tr?ng
            }
        }
        private void TaiDanhSachUserAgentLenCombobox()
        {
            cboUserAgent.Items.Clear();                                                         // M?i l?n n?p l?i th? x?a danh s?ch c? ?? tr?nh b? tr?ng l?p
            if (!File.Exists(userAgentsFilePath))
            {
                return;                                                                         // Thi?u file th? d?ng, v? h?m ??m b?o s? t?o l?i tr??c ?? r?i
            }
            List<string> danhSachUserAgent = File.ReadAllLines(userAgentsFilePath)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();                                                                      // Ch? l?y c?c d?ng UA c? gi? tr? v? lo?i b? tr?ng l?p
            foreach (string userAgent in danhSachUserAgent)
            {
                cboUserAgent.Items.Add(userAgent);                                              // ?? t?ng User-Agent t? file txt l?n combobox ?? ng??i d?ng ch?n
            }
            if (cboUserAgent.Items.Count == 0)
            {
                return;                                                                         // Kh?ng c? UA n?o th? d?ng, tr?nh g?t SelectedItem khi combobox ?ang tr?ng
            }
            if (danhSachUserAgent.Contains(facebookDesktopUserAgentMacDinh, StringComparer.OrdinalIgnoreCase))
            {
                cboUserAgent.SelectedItem = facebookDesktopUserAgentMacDinh;                   // Facebook th??ng m?c ??nh quay v? UA desktop ?ang test ?n n?y
                return;
            }
            cboUserAgent.SelectedIndex = 0;                                                     // N?u kh?ng c? UA desktop m?c ??nh th? ch?n d?ng ??u ti?n trong file
        }
        private string LayUserAgentFacebookThuongDangChon()
        {
            string userAgentDangChon = cboUserAgent.SelectedItem?.ToString()?.Trim() ?? string.Empty; // ??c ??ng User-Agent ng??i d?ng ?ang ch?n tr?n combobox
            if (!string.IsNullOrWhiteSpace(userAgentDangChon))
            {
                return userAgentDangChon;                                                      // N?u combobox ?ang c? gi? tr? th? facebook.com s? d?ng ch?nh UA n?y
            }
            return facebookDesktopUserAgentMacDinh;                                            // Fall back v? UA desktop m?c ??nh n?u combobox ch?a ch?n g?
        }        private bool TryLayTaiKhoanMoiTuDs(out string uid, out string password)
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
        private void ThemDongMoiLenGrid(string uid, string password)
        {
            int rowIndex = dataGridView1.Rows.Add();                                          // Tạo một dòng mới và lấy ra vị trí của dòng vừa thêm
            DataGridViewRow row = dataGridView1.Rows[rowIndex];                               // Lấy đối tượng dòng để đổ dữ liệu vào các cột

            row.Cells["colSTT"].Value = rowIndex + 1;                                         // Đổ số thứ tự theo vị trí hiện tại trên grid
            row.Cells["colChon"].Value = true;                                                // Dòng mới do Next tạo ra phải được tick ngay để app bám đúng dòng đang xử lý
            row.Cells["colUID"].Value = uid;                                                  // Đổ UID vừa lấy được từ ds.txt
            row.Cells["colPass"].Value = password;                                            // Đổ mật khẩu tương ứng với UID
            row.Cells["colNgayTao"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");     // Đổ thời gian tạo dòng theo thời điểm hiện tại
            row.Cells["colTen"].Value = string.Empty;                                         // Tên để trống, sau này người dùng hoặc code sẽ cập nhật
            row.Cells["colEmail"].Value = string.Empty;                                       // Email để trống ở bước hiện tại
            row.Cells["colGhiChu"].Value = string.Empty;                                      // Ghi chú để trống ở bước hiện tại
            row.Cells["colTuongTacCuoi"].Value = string.Empty;                                // Tương tác cuối để trống ở bước hiện tại
            row.Cells["colTrangThai"].Value = string.Empty;                                   // Trạng thái để trống, ưu tiên hiện ở label phía dưới
            row.Cells["colCookie"].Value = string.Empty;                                      // Cookie để trống, sau này mới tính tới

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

        private void btnXoa_Click(object sender, EventArgs e)
        {
            XoaCacDongDaTick();                                                                // Nút Xóa trên form dùng chung một luồng xóa theo các dòng đang tick
        }

        private void xóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XoaCacDongDaTick();                                                                // Menu chuột phải giờ chỉ còn một nhánh Xóa chung
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
                ThemDongMoiLenGrid(uid, password);
                MoChromeTheoProfile(duongDanProfileTheoUid, uid, password);                    // Sau khi đã có dòng mới thì mở luôn Chrome theo profile vừa tạo để hoàn chỉnh luồng Next
                return;
            }

            if (Directory.Exists(duongDanProfileTheoUid))                                     // Nếu thư mục profile theo UID đã tồn tại sẵn thì báo để tránh ghi đè nhầm
            {
                MessageBox.Show($"Profile {uid} đã tồn tại.");
                return;
            }

            if (!ThuDongTatCaChromeDeXuLyProfile())
            {
                return;                                                                        // Trước khi đổi tên profile_ranh thành UID phải đóng toàn bộ Chrome để tránh khóa thư mục
            }

            if (!ThuDoiTenThuMucProfile(duongDanProfileSuDung, duongDanProfileTheoUid, uid))
            {
                return;                                                                        // Nếu vẫn chưa đổi tên được thì dừng lại để tránh thêm dòng grid khi profile chưa sẵn sàng
            }

            ThemDongMoiLenGrid(uid, password);                                                // Sau khi đã có profile đúng tên, thêm ngay dòng mới lên grid
            MoChromeTheoProfile(duongDanProfileTheoUid, uid, password);                        // Mở ngay profile mới theo giao diện đã chọn để người dùng tiếp tục thao tác
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
            string password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty;  // Lấy lại mật khẩu trên grid để khi mở profile cũ vẫn tự điền lại đúng tài khoản

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

            MoChromeTheoProfile(duongDanProfile, uid, password);                               // Dùng chung một hàm mở Chrome để đồng bộ logic URL, User-Agent và tự điền với nút Next
        }
        //
        //  HÀM MỞ CHROME THEO PROFILE
        //
        private void MoChromeTheoProfile(string duongDanProfile, string tenProfile, string password)
        {
            string chromeExe = TimChromeExe();                                                 // Tìm đường dẫn chrome.exe để mở profile bằng Chrome thật
            if (string.IsNullOrWhiteSpace(chromeExe))
            {
                MessageBox.Show("Không tìm thấy chrome.exe.");
                return;                                                                        // Không mở tiếp nếu máy chưa tìm thấy Chrome
            }

            string urlCanMo = LayUrlFacebookDaChon();                                          // Lấy đúng URL Facebook theo giao diện đang chọn trên combobox
            string userAgentDangDung = LayGiaTriUserAgentTheoGiaoDien(urlCanMo);               // Xác định đúng UA sẽ dùng cho giao diện hiện tại để vừa mở Chrome vừa lưu lại làm mốc test
            string thamSoUserAgent = LayThamSoUserAgentTheoGiaoDien(urlCanMo);                 // Nếu là giao diện mobile hoặc meta thì ghép thêm User-Agent cố định
            int congDebugChrome = LayCongDebugChromeTrong();                                   // Mỗi lần mở Chrome cấp một cổng debug riêng để app có thể tự điền UID và Password chính xác

            Rectangle vungLamViec = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1200, 900); // Lấy vùng làm việc hiện tại để mở Chrome nhỏ hơn, không full màn hình
            int chieuRongCuaSo = Math.Max(900, vungLamViec.Width / 2);                          // Giữ chiều rộng khoảng nửa màn hình để đủ chỗ thao tác
            int chieuCaoCuaSo = Math.Max(700, (int)(vungLamViec.Height * 0.85));                // Giữ chiều cao thoải mái nhưng vẫn nhỏ hơn full màn hình

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = chromeExe,
                Arguments = $"--new-window --window-size={chieuRongCuaSo},{chieuCaoCuaSo} --window-position=0,0 --remote-debugging-port={congDebugChrome} --user-data-dir=\"{duongDanProfile}\" {thamSoUserAgent} {urlCanMo}".Trim(), // Mở Chrome theo cửa sổ vừa phải để dễ quan sát, không chiếm full màn hình
                UseShellExecute = true
            };

            try
            {
                GhiLaiUserAgentDangDung(urlCanMo, userAgentDangDung);                          // Ghi lại ngay URL và UA đang dùng để nếu Facebook đổi giao diện còn có dữ liệu đối chiếu
                System.Diagnostics.Process.Start(psi);                                         // Mở Chrome theo đúng profile, dùng chung cho cả Mở dòng và Next
                congDebugTheoUid[tenProfile] = congDebugChrome;                                // Lưu lại cổng debug theo UID để các chức năng như Điền UID Password còn bám lại đúng phiên Chrome đang mở
                _ = TuDongDienThongTinDangNhapAsync(congDebugChrome, urlCanMo, tenProfile, password); // Chạy nền bước tự điền để người dùng đỡ phải nhập tay lại UID và Password
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở Chrome cho profile {tenProfile}.{Environment.NewLine}{ex.Message}");
            }
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
            string userAgentDangDung = LayGiaTriUserAgentTheoGiaoDien(urlCanMo);               // Tách riêng giá trị UA để cùng một nguồn dữ liệu được dùng cho cả lệnh mở Chrome lẫn file ghi log

            if (!string.IsNullOrWhiteSpace(userAgentDangDung))
            {
                return $"--user-agent=\"{userAgentDangDung}\"";                                // Nếu giao diện này cần ép UA thì ghép đúng tham số --user-agent để Chrome mở theo ý app
            }

            return string.Empty;                                                               // Các giao diện còn lại tạm thời chưa cần ép User-Agent
        }
        //
        //  HÀM LẤY GIÁ TRỊ USER-AGENT THEO GIAO DIỆN
        //
        private string LayGiaTriUserAgentTheoGiaoDien(string urlCanMo)
        {
            if (urlCanMo.Contains("m.facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                return mobileUserAgentMacDinh;                                                 // Giao diện mobile luôn phải ép UA mobile thì Facebook mới chịu giữ giao diện m.facebook
            }

            if (urlCanMo.Contains("facebook.com/meta", StringComparison.OrdinalIgnoreCase))
            {
                return metaDesktopUserAgentMacDinh;                                            // Meta sẽ dùng một UA desktop cố định để mình còn lưu lại và test ổn định hơn
            }

            return facebookDesktopUserAgentMacDinh;                                            // Facebook thường giờ cũng ép theo UA desktop đã test ổn thay vì thả theo mặc định của Chrome
        }
        //
        //  HÀM GHI LẠI USER-AGENT ĐANG DÙNG
        //
        private void GhiLaiUserAgentDangDung(string urlCanMo, string userAgentDangDung)
        {
            string noiDung = $"""
Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
URL: {urlCanMo}
User-Agent: {(string.IsNullOrWhiteSpace(userAgentDangDung) ? "Dùng User-Agent mặc định của Chrome" : userAgentDangDung)}
""";

            File.WriteAllText(userAgentDangDungFilePath, noiDung, Encoding.UTF8);              // Mỗi lần mở Chrome ghi đè mốc UA mới nhất để người dùng dễ nhìn và dễ lưu lại
        }
        //
        //  HÀM LẤY CỔNG DEBUG CHROME TRỐNG
        //
        private int LayCongDebugChromeTrong()
        {
            using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();                                                                  // Mượn tạm một cổng trống của hệ điều hành để dùng cho remote debugging của Chrome
            int congTrong = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return congTrong;
        }
        //
        //  HÀM TỰ ĐIỀN THÔNG TIN ĐĂNG NHẬP
        //
        private async Task TuDongDienThongTinDangNhapAsync(int congDebugChrome, string urlCanMo, string uid, string password)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
            {
                return;                                                                        // Nếu thiếu UID hoặc Password thì không tự điền được, tránh đổ nhầm dữ liệu rỗng lên form đăng nhập
            }

            string script = TaoScriptTuDongDienDangNhap(uid, password);                        // Tạo script JavaScript để tìm ô email/password và đổ giá trị vào đúng cách của trình duyệt
            int soLanThuToiDa = urlCanMo.Contains("meta", StringComparison.OrdinalIgnoreCase) ? 40 : 25; // Giao diện meta thường render chậm hơn nên cho phép thử nhiều lần hơn

            for (int i = 0; i < soLanThuToiDa; i++)
            {
                try
                {
                    string? webSocketDebuggerUrl = await LayWebSocketDebuggerUrlFacebookAsync(congDebugChrome, urlCanMo); // Tìm đúng tab Facebook theo giao diện đang mở để kết nối vào CDP
                    if (string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    bool daDienThanhCong = await ThuChayScriptTuDongDienAsync(webSocketDebuggerUrl, script); // Chạy script bằng CDP, nếu ô đã hiện thì điền ngay UID và Password
                    if (daDienThanhCong)
                    {
                        return;
                    }
                }
                catch
                {
                }

                await Task.Delay(1000);                                                        // Chờ trang tải thêm rồi thử lại vì Facebook có thể render form chậm
            }
        }
        //
        //  HÀM LẤY WEBSOCKET DEBUGGER URL CỦA TAB FACEBOOK
        //
        private async Task<string?> LayWebSocketDebuggerUrlFacebookAsync(int congDebugChrome, string urlCanMo)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            string json = await httpClient.GetStringAsync($"http://127.0.0.1:{congDebugChrome}/json"); // Đọc danh sách tab debug mà Chrome đang mở trên cổng remote-debugging-port
            using JsonDocument document = JsonDocument.Parse(json);
            string? webSocketDebuggerUrlPhuHopNhat = null;                                     // Lưu tạm một tab Facebook hợp lệ nếu chưa bắt được đúng URL cần mở

            foreach (JsonElement tab in document.RootElement.EnumerateArray())
            {
                string type = tab.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
                string url = tab.TryGetProperty("url", out JsonElement urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                string webSocketDebuggerUrl = tab.TryGetProperty("webSocketDebuggerUrl", out JsonElement wsElement) ? wsElement.GetString() ?? string.Empty : string.Empty;

                if (!string.Equals(type, "page", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (url.Contains(urlCanMo, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                {
                    return webSocketDebuggerUrl;                                               // Ưu tiên đúng tab của giao diện đang chọn, đặc biệt quan trọng với facebook.com/meta
                }

                if (!string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                {
                    webSocketDebuggerUrlPhuHopNhat ??= webSocketDebuggerUrl;                   // Nếu chưa gặp đúng URL thì tạm giữ lại một tab Facebook hợp lệ làm phương án dự phòng
                }
            }

            return webSocketDebuggerUrlPhuHopNhat;
        }
        //
        //  HÀM LẤY WEBSOCKET DEBUGGER URL CỦA TAB FACEBOOK ĐANG MỞ
        //
        private async Task<string?> LayWebSocketDebuggerUrlFacebookDangMoAsync(int congDebugChrome)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            string json = await httpClient.GetStringAsync($"http://127.0.0.1:{congDebugChrome}/json"); // Đọc danh sách tab hiện có của phiên Chrome đang mở để bám vào tab Facebook hiện tại
            using JsonDocument document = JsonDocument.Parse(json);

            foreach (JsonElement tab in document.RootElement.EnumerateArray())
            {
                string type = tab.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
                string url = tab.TryGetProperty("url", out JsonElement urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                string webSocketDebuggerUrl = tab.TryGetProperty("webSocketDebuggerUrl", out JsonElement wsElement) ? wsElement.GetString() ?? string.Empty : string.Empty;

                if (!string.Equals(type, "page", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                {
                    return webSocketDebuggerUrl;                                               // Chỉ cần tab Facebook nào đang mở được là menu Điền UID Password có thể bám vào để điền lại
                }
            }

            return null;
        }
        //
        //  HÀM THỬ CHẠY SCRIPT TỰ ĐIỀN
        //
        private async Task<bool> ThuChayScriptTuDongDienAsync(string webSocketDebuggerUrl, string script)
        {
            using var webSocket = new System.Net.WebSockets.ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(webSocketDebuggerUrl), CancellationToken.None); // Kết nối thẳng vào tab Facebook bằng giao thức DevTools Protocol

            using JsonDocument ketQua = await GuiLenhCdpAsync(webSocket, "Runtime.evaluate", new
            {
                expression = script,
                returnByValue = true
            });

            if (!ketQua.RootElement.TryGetProperty("result", out JsonElement resultElement))
            {
                return false;
            }

            if (!resultElement.TryGetProperty("result", out JsonElement evaluateResult))
            {
                return false;
            }

            if (!evaluateResult.TryGetProperty("value", out JsonElement valueElement))
            {
                return false;
            }

            string trangThai = valueElement.GetString() ?? string.Empty;                       // Script trả về 'ok' khi đã tìm thấy đủ ô và điền xong, còn lại là 'wait'
            return string.Equals(trangThai, "ok", StringComparison.OrdinalIgnoreCase);
        }
        //
        //  HÀM GỬI LỆNH CDP
        //
        private async Task<JsonDocument> GuiLenhCdpAsync(System.Net.WebSockets.ClientWebSocket webSocket, string method, object thamSo)
        {
            string jsonRequest = JsonSerializer.Serialize(new
            {
                id = 1,
                method,
                @params = thamSo
            });

            byte[] requestBytes = Encoding.UTF8.GetBytes(jsonRequest);
            await webSocket.SendAsync(new ArraySegment<byte>(requestBytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

            byte[] buffer = new byte[32768];
            using var memoryStream = new MemoryStream();

            while (true)
            {
                System.Net.WebSockets.WebSocketReceiveResult ketQuaNhan = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                memoryStream.Write(buffer, 0, ketQuaNhan.Count);

                if (ketQuaNhan.EndOfMessage)
                {
                    break;
                }
            }

            memoryStream.Position = 0;
            return await JsonDocument.ParseAsync(memoryStream);                                 // Trả lại JSON gốc để đọc kết quả evaluate từ tab Chrome
        }
        //
        //  HÀM TẠO SCRIPT TỰ ĐIỀN ĐĂNG NHẬP
        //
        private string TaoScriptTuDongDienDangNhap(string uid, string password)
        {
            string uidJson = JsonSerializer.Serialize(uid);
            string passwordJson = JsonSerializer.Serialize(password);

            return $$"""
(() => {
  const uid = {{uidJson}};
  const password = {{passwordJson}};

  const isVisible = (el) => !!(el && (el.offsetWidth || el.offsetHeight || el.getClientRects().length));
  const firstVisible = (selectors) => {
    for (const selector of selectors) {
      const element = document.querySelector(selector);
      if (isVisible(element)) return element;
    }
    return null;
  };

  const setNativeValue = (element, value) => {
    const prototype = Object.getPrototypeOf(element);
    const descriptor = Object.getOwnPropertyDescriptor(prototype, 'value') || Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value');
    const oldValue = element.value;
    element.focus();
    element.click();
    descriptor.set.call(element, value);
    if (element._valueTracker) {
      element._valueTracker.setValue(oldValue);
    }
    element.dispatchEvent(new Event('input', { bubbles: true }));
    element.dispatchEvent(new Event('change', { bubbles: true }));
    element.dispatchEvent(new Event('blur', { bubbles: true }));
  };

  const contexts = [document, ...Array.from(document.querySelectorAll('iframe')).map((frame) => {
    try {
      return frame.contentDocument;
    } catch {
      return null;
    }
  }).filter(Boolean)];

  const findInputsInContext = (root) => {
    const queryVisible = (selectors) => {
      for (const selector of selectors) {
        const element = root.querySelector(selector);
        if (isVisible(element)) return element;
      }
      return null;
    };

    const emailInput = queryVisible([
      'input[name="email"]',
      'input[id="email"]',
      'input[name="username"]',
      'input[name="login"]',
      'input[autocomplete="username"]',
      'input[type="email"]',
      'input[inputmode="email"]',
      'input[placeholder*="Email"]',
      'input[placeholder*="email"]',
      'input[placeholder*="Số điện thoại"]',
      'input[placeholder*="điện thoại"]',
      'input[placeholder*="phone"]',
      'input[aria-label*="Email"]',
      'input[aria-label*="email"]',
      'input[aria-label*="username"]'
    ]) || Array.from(root.querySelectorAll('input')).find((input) => {
      const type = (input.getAttribute('type') || 'text').toLowerCase();
      const name = (input.getAttribute('name') || '').toLowerCase();
      const id = (input.getAttribute('id') || '').toLowerCase();
      const placeholder = (input.getAttribute('placeholder') || '').toLowerCase();
      const ariaLabel = (input.getAttribute('aria-label') || '').toLowerCase();
      return isVisible(input) &&
        type !== 'password' &&
        type !== 'hidden' &&
        (
          type === 'text' ||
          type === 'email' ||
          name.includes('email') ||
          name.includes('user') ||
          name.includes('login') ||
          id.includes('email') ||
          id.includes('user') ||
          placeholder.includes('email') ||
          placeholder.includes('điện thoại') ||
          placeholder.includes('phone') ||
          ariaLabel.includes('email') ||
          ariaLabel.includes('user')
        );
    });

    const passwordInput = queryVisible([
      'input[name="pass"]',
      'input[name="password"]',
      'input[id="pass"]',
      'input[id="password"]',
      'input[type="password"]',
      'input[autocomplete="current-password"]',
      'input[aria-label*="Password"]',
      'input[aria-label*="password"]',
      'input[placeholder*="Password"]',
      'input[placeholder*="password"]',
      'input[placeholder*="Mật khẩu"]',
      'input[placeholder*="mật khẩu"]'
    ]);

    return { emailInput, passwordInput };
  };

  let emailInput = null;
  let passwordInput = null;

  for (const root of contexts) {
    const found = findInputsInContext(root);
    if (found.emailInput && found.passwordInput) {
      emailInput = found.emailInput;
      passwordInput = found.passwordInput;
      break;
    }
  }

  if (!emailInput || !passwordInput) {
    return 'wait';
  }

  setNativeValue(emailInput, uid);
  setNativeValue(passwordInput, password);
  return 'ok';
})();
""";
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

            if (!LaTenProfileUidHopLe(uid))
            {
                MessageBox.Show("Dòng đang tick không phải profile UID hợp lệ.");
                return;                                                                        // Chặn xóa nhầm các thư mục hệ thống như runtimes nếu chúng từng bị nạp sai lên grid
            }

            if (!ThuDongTatCaChromeDeXuLyProfile())
            {
                return;                                                                        // Khi đã xác định xóa thì app phải đóng toàn bộ Chrome trước để tránh lỗi khóa file profile
            }

            if (!XuLyProfileKhiXoaMotDong(uid))                                                // Chỉ tiếp tục xóa dữ liệu khi profile đã được xử lý xong an toàn
            {
                return;
            }

            XoaUidKhoiDsTxt(uid);                                                              // Xóa UID tương ứng ra khỏi ds.txt để dữ liệu file và grid luôn đồng bộ
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

            List<string> dsUidCanXoa = new();                                                  // Tách riêng UID cần xóa để xử lý profile trước, rồi mới đồng bộ file và grid

            foreach (DataGridViewRow row in dsDongDaTick)
            {
                string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;    // Lấy UID từng dòng để xóa khỏi ds.txt và xóa profile tương ứng

                if (string.IsNullOrWhiteSpace(uid))
                {
                    continue;
                }

                if (!LaTenProfileUidHopLe(uid))
                {
                    MessageBox.Show("Có dòng được tick không phải profile UID hợp lệ. Vui lòng mở lại app rồi thử lại.");
                    return;                                                                    // Dừng lại để không đụng nhầm thư mục hệ thống hoặc thư mục phụ của app
                }

                dsUidCanXoa.Add(uid);                                                          // Gom UID hợp lệ để xóa profile và dữ liệu đồng bộ theo cùng một danh sách
            }

            if (!ThuDongTatCaChromeDeXuLyProfile())
            {
                return;                                                                        // Xóa nhiều dòng cũng phải dừng toàn bộ Chrome trước để xóa profile dứt điểm
            }

            if (!XoaToanBoProfileKhiXoaNhieuDong(dsUidCanXoa))                                 // Xóa nhiều dòng phải xóa sạch luôn cả profile_ranh sau khi đóng phiên Chrome liên quan
            {
                return;
            }

            foreach (string uid in dsUidCanXoa)
            {
                XoaUidKhoiDsTxt(uid);                                                          // Chỉ xóa ds.txt sau khi phần profile đã được xóa an toàn
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
        private bool XuLyProfileKhiXoaMotDong(string uid)
        {
            string duongDanProfileTheoUid = Path.Combine(AppContext.BaseDirectory, uid);       // Xác định đúng thư mục profile theo UID của dòng bị xóa
            if (!Directory.Exists(duongDanProfileTheoUid))
            {
                return true;                                                                   // Nếu không còn profile thì chỉ cần xóa dữ liệu grid và ds.txt
            }

            if (Directory.Exists(profileRanhPath))
            {
                return ThuXoaThuMucProfile(duongDanProfileTheoUid, uid);                       // Nếu đã có sẵn profile_ranh thì chỉ xóa profile theo UID để giữ đúng một profile rảnh duy nhất
            }

            if (Directory.Exists(profileMauPath))
            {
                if (!ThuXoaThuMucProfile(duongDanProfileTheoUid, uid))                         // Xóa profile cũ của UID sau khi đã đóng phiên Chrome đang giữ profile đó
                {
                    return false;
                }

                CopyDirectory(profileMauPath, profileRanhPath);                                // Tạo lại profile_ranh sạch từ profile_mau để dùng cho lần Next sau
                return true;
            }

            return ThuDoiTenThuMucProfile(duongDanProfileTheoUid, profileRanhPath, uid);      // Nếu thiếu profile_mau thì giữ lại tối thiểu một profile_ranh bằng cách đổi tên profile vừa xóa
        }
        //
        //  HÀM XÓA HẲN PROFILE THEO UID
        //
        private bool XoaProfileTheoUid(string uid)
        {
            string duongDanProfileTheoUid = Path.Combine(AppContext.BaseDirectory, uid);       // Xác định thư mục profile cần xóa hẳn khỏi ổ đĩa
            return ThuXoaThuMucProfile(duongDanProfileTheoUid, uid);                           // Xóa sạch profile của các dòng bị loại khỏi app sau khi đã đóng phiên Chrome tương ứng
        }
        //
        //  HÀM XÓA TOÀN BỘ PROFILE KHI XÓA NHIỀU DÒNG
        //
        private bool XoaToanBoProfileKhiXoaNhieuDong(List<string> dsUidCanXoa)
        {
            foreach (string uid in dsUidCanXoa)
            {
                if (!XoaProfileTheoUid(uid))
                {
                    return false;                                                              // Dừng ngay nếu có profile nào chưa xóa được để tránh lệch dữ liệu
                }
            }

            return ThuXoaThuMucProfile(profileRanhPath, "profile_ranh");                       // Xóa nhiều dòng thì xóa luôn cả profile_ranh như logic đã chốt
        }
        //
        //  HÀM ĐÓNG TOÀN BỘ CHROME ĐỂ XỬ LÝ PROFILE
        //
        private bool ThuDongTatCaChromeDeXuLyProfile()
        {
            try
            {
                System.Diagnostics.Process[] dsChrome = System.Diagnostics.Process.GetProcessesByName("chrome"); // Khi người dùng đã quyết tâm xóa thì đóng toàn bộ Chrome để không còn phiên nào giữ file
                foreach (System.Diagnostics.Process process in dsChrome)
                {
                    try
                    {
                        if (process.HasExited)
                        {
                            continue;
                        }

                        process.CloseMainWindow();                                             // Ưu tiên đóng nhẹ nhàng trước để Chrome tự nhả file
                        if (!process.WaitForExit(1500))
                        {
                            process.Kill(true);                                                // Nếu Chrome không tự đóng thì cưỡng bức đóng cả cây tiến trình của profile đó
                            process.WaitForExit(5000);
                        }
                    }
                    catch (ArgumentException)
                    {
                        continue;                                                              // Tiến trình có thể đã tự thoát trước khi app kịp xử lý, khi đó chỉ cần bỏ qua
                    }
                }

                if (dsChrome.Length > 0)
                {
                    System.Threading.Thread.Sleep(1200);                                       // Chờ thêm một nhịp để hệ thống nhả hẳn file profile sau khi mọi phiên Chrome đã đóng
                }

                ThuDongChromeBangTaskkill();

                for (int i = 0; i < 10; i++)
                {
                    if (System.Diagnostics.Process.GetProcessesByName("chrome").Length == 0)
                    {
                        return true;
                    }

                    ThuDongChromeBangTaskkill();
                    System.Threading.Thread.Sleep(500);
                }

                MessageBox.Show("Chrome vẫn chưa tắt hết, không thể xử lý xóa profile.");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể đóng Chrome trước khi xóa.{Environment.NewLine}{ex.Message}");
                return false;
            }
        }
        //
        //  HÀM THỬ XÓA THƯ MỤC PROFILE
        //
        private bool ThuXoaThuMucProfile(string duongDanProfile, string tenProfile)
        {
            if (!Directory.Exists(duongDanProfile))
            {
                return true;                                                                   // Nếu thư mục không còn thì coi như đã xóa xong
            }

            string chiTietLoiCuoi = string.Empty;

            for (int i = 0; i < 15; i++)
            {
                try
                {
                    BoThuocTinhThuMucVeBinhThuong(duongDanProfile);                            // Gỡ ReadOnly và các attribute đặc biệt trước khi xóa để tránh Access Denied trên Windows
                    Directory.Delete(duongDanProfile, true);                                   // Thử xóa toàn bộ thư mục profile sau khi đã đóng phiên Chrome tương ứng
                    return true;
                }
                catch (IOException)
                {
                    chiTietLoiCuoi = "IOException";
                    System.Threading.Thread.Sleep(500);                                        // Chờ ngắn để hệ thống nhả file rồi thử lại
                }
                catch (UnauthorizedAccessException)
                {
                    chiTietLoiCuoi = "UnauthorizedAccessException";
                    System.Threading.Thread.Sleep(500);                                        // Chờ ngắn để xử lý nốt các file còn đang khóa rồi thử lại
                }
            }

            if (ThuXoaThuMucProfileBangLenhHeThong(duongDanProfile))
            {
                return true;
            }

            MessageBox.Show($"Không thể xóa profile {tenProfile}. {chiTietLoiCuoi}");
            return false;
        }

        private void ThuDongChromeBangTaskkill()
        {
            using var process = new System.Diagnostics.Process();                               // Dùng taskkill để dọn nốt các tiến trình con của Chrome mà Kill(true) đôi khi vẫn để sót
            process.StartInfo.FileName = "taskkill";
            process.StartInfo.Arguments = "/F /T /IM chrome.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit(5000);
        }

        private bool ThuXoaThuMucProfileBangLenhHeThong(string duongDanProfile)
        {
            try
            {
                using var process = new System.Diagnostics.Process();                           // Fallback cuối cùng dùng lệnh hệ thống để xóa khi Directory.Delete vẫn vướng trên Windows
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c attrib -r -s -h \"{duongDanProfile}\" /s /d & rmdir /s /q \"{duongDanProfile}\"";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(5000);
                return !Directory.Exists(duongDanProfile);
            }
            catch
            {
                return false;                                                                   // Nếu fallback này cũng thất bại thì để hàm ngoài báo lỗi ra cho người dùng
            }
        }

        //
        //  HÀM ĐƯA THUỘC TÍNH THƯ MỤC VỀ BÌNH THƯỜNG
        //
        private void BoThuocTinhThuMucVeBinhThuong(string duongDanProfile)
        {
            if (!Directory.Exists(duongDanProfile))
            {
                return;                                                                        // Nếu thư mục không còn thì không cần xử lý attribute nữa
            }

            foreach (string filePath in Directory.GetFiles(duongDanProfile, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);                           // Xóa cờ ReadOnly/Hidden/System của file để cho phép xóa dứt điểm
            }

            foreach (string dirPath in Directory.GetDirectories(duongDanProfile, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(dirPath, FileAttributes.Normal);                            // Xóa cờ ReadOnly/Hidden/System của thư mục con để tránh lỗi Access Denied
            }

            File.SetAttributes(duongDanProfile, FileAttributes.Normal);                        // Đưa cả thư mục gốc của profile về trạng thái bình thường trước khi xóa
        }
        //
        //  HÀM THỬ ĐỔI TÊN THƯ MỤC PROFILE
        //
        private bool ThuDoiTenThuMucProfile(string duongDanNguon, string duongDanDich, string tenProfile)
        {
            if (!Directory.Exists(duongDanNguon))
            {
                return true;                                                                   // Nếu thư mục nguồn không còn thì không cần đổi tên nữa
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    BoThuocTinhThuMucVeBinhThuong(duongDanNguon);                              // Gỡ ReadOnly và attribute đặc biệt trước khi đổi tên để tránh Access Denied trên Windows
                    Directory.Move(duongDanNguon, duongDanDich);                               // Đổi tên profile sau khi chắc chắn Chrome đã nhả thư mục
                    return true;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(300);                                        // Chờ hệ thống nhả file rồi thử lại
                }
                catch (UnauthorizedAccessException)
                {
                    System.Threading.Thread.Sleep(300);                                        // Chờ quyền truy cập được giải phóng rồi thử lại
                }
            }

            MessageBox.Show($"Không thể đổi tên profile {tenProfile}. Vui lòng thử lại.");
            return false;
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
        //  HÀM KIỂM TRA TÊN PROFILE UID HỢP LỆ
        //
        private bool LaTenProfileUidHopLe(string tenThuMuc)
        {
            return !string.IsNullOrWhiteSpace(tenThuMuc) && tenThuMuc.All(char.IsDigit);      // App hiện tại chỉ coi tên thư mục toàn số là profile UID hợp lệ để tránh nạp nhầm runtimes hay thư mục hệ thống
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

                if (!LaTenProfileUidHopLe(tenThuMuc))
                {
                    continue;                                                                   // Chỉ nạp các thư mục tên UID hợp lệ, bỏ qua runtimes và các thư mục hệ thống khác
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
        //
        //   HÀM TÌM CHROME
        //
        private string TimChromeExe()
        {
            string chrome1 = @"C:\Program Files\Google\Chrome\Application\chrome.exe";        // Vị trí cài đặt Chrome phổ biến trên Windows 64-bit
            string chrome2 = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";  // Vị trí cài đặt Chrome phổ biến trên một số máy khác

            if (File.Exists(chrome1)) return chrome1;
            if (File.Exists(chrome2)) return chrome2;

            return string.Empty;                                                                // Trả rỗng nếu chưa tìm thấy chrome.exe ở các vị trí đang hỗ trợ
        }
        //
        //  SỰ ĐIỆN CHUỘT PHẢI ĐIỀN UID/PASSWORD
        //
        private void XoaCacDongDaTick()
        {
            int soDongDaTick = LayDanhSachDongDaTick().Count;                                  // Đếm số dòng được tick để điều phối đúng nhánh xóa bên trong

            if (soDongDaTick == 0)
            {
                MessageBox.Show("Vui lòng tick ít nhất 1 dòng để xóa.");
                return;                                                                        // Không có dòng nào được tick thì không có mục tiêu để xóa
            }

            if (soDongDaTick == 1)
            {
                XoaMotDongDaTick();                                                            // Giữ lại logic xóa 1 dòng đang chạy ổn, chỉ gom đầu vào về một chỗ
                return;
            }

            XoaNhieuDongDaTick();                                                              // Từ 2 dòng trở lên thì đi theo nhánh xóa nhiều dòng như nghiệp vụ đã chốt
        }

        private void điềnUIDPaswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Chỉ lấy những dòng được chọn thật bằng checkbox để tránh điền nhầm theo bôi đen
            if (dsDongDaTick.Count != 1)
            {
                MessageBox.Show("Vui lòng tick đúng 1 dòng để điền lại UID và Password.");
                return;                                                                        // Menu này chỉ có ý nghĩa khi đang làm việc với đúng 1 tài khoản đang mở
            }

            DataGridViewRow row = dsDongDaTick[0];                                             // Lấy đúng dòng đang muốn điền lại thông tin vào phiên Chrome đang mở
            string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;        // UID cũng là khóa để tìm lại đúng cổng debug của phiên Chrome đã mở trước đó
            string password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty;  // Mật khẩu lấy trực tiếp từ grid để điền lại vào form đăng nhập đang mở

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Dòng đang chọn chưa có đủ UID hoặc Password.");
                return;                                                                        // Nếu thiếu dữ liệu thì dừng để tránh điền sai hoặc điền chuỗi rỗng
            }

            if (!congDebugTheoUid.TryGetValue(uid, out int congDebugChrome))
            {
                MessageBox.Show("Dòng này chưa có Chrome đang mở. Hãy mở dòng này bằng app trước rồi thử lại.");
                return;                                                                        // Chỉ điền lại được khi profile này đã được app mở trước đó và còn giữ cổng debug
            }

            _ = DienLaiUidVaPasswordLenChromeDangMoAsync(congDebugChrome, uid, password);      // Chỉ điền lại trên tab Facebook hiện có của phiên Chrome đang mở, không phụ thuộc combobox URL
        }

        //
        //  HÀM ĐIỀN LẠI UID/PASSWORD LÊN CHROME ĐANG MỞ
        //
        private async Task DienLaiUidVaPasswordLenChromeDangMoAsync(int congDebugChrome, string uid, string password)
        {
            try
            {
                string? webSocketDebuggerUrl = await LayWebSocketDebuggerUrlFacebookDangMoAsync(congDebugChrome); // Lấy tab Facebook đang mở thực tế của phiên Chrome này, bất kể đang ở URL nào
                if (string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                {
                    MessageBox.Show("Không tìm thấy tab Facebook đang mở để điền lại UID và Password.");
                    return;                                                                    // Nếu Chrome đang mở nhưng không còn tab Facebook nào thì menu này không còn mục tiêu để điền
                }

                string script = TaoScriptTuDongDienDangNhap(uid, password);                    // Dùng lại cùng một script tự điền đã ổn định ở các luồng khác để tránh tách logic rời rạc
                bool daDienThanhCong = await ThuChayScriptTuDongDienAsync(webSocketDebuggerUrl, script);
                if (!daDienThanhCong)
                {
                    MessageBox.Show("Không tìm thấy ô đăng nhập trên tab Facebook đang mở.");
                }
            }
            catch
            {
                MessageBox.Show("Không thể kết nối lại phiên Chrome đang mở của dòng này.");
            }
        }
    }
}

