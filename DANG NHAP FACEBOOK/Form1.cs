using System.Text;
using System.Text.Json;

using System.Globalization;

namespace DANG_NHAP_FACEBOOK
{
    public partial class Form1 : Form
    {
        private readonly string dsFilePath;
        private readonly string gridFilePath;
        private readonly string profileMauPath;
        private readonly string profileRanhPath;
        private readonly string userAgentDangDungFilePath;
        private readonly string userAgentsFilePath;
        private readonly Dictionary<string, int> congDebugTheoUid = new(StringComparer.OrdinalIgnoreCase);
        private readonly object dieuKhienPhienSyncRoot = new();
        private readonly Queue<TaiKhoanDuocChon> hangChoTaiKhoanDaTick = new();
        private const string facebookDesktopUserAgentMacDinh = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";
        private const string mobileUserAgentMacDinh = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36";
        private const string metaDesktopUserAgentMacDinh = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";
        private const int soGiayChoMoPhienMoi = 5;
        private const int khoangNghiSauKhiDongChromeMs = 500;
        private const int khoangNghiNhinKetQuaTruocKhiDongChromeMs = 5000;
        private bool dangNapGridTuFile;
        private int maDieuKhienTuDong;
        private string uidPhienDangXuLy = string.Empty;
        private string sessionIdPhienDangXuLy = string.Empty;
        private bool daYeuCauDungThuCong;
        private CheDoDieuPhoiPhien cheDoHangChoTaiKhoanDaTick = CheDoDieuPhoiPhien.KhongCo;

        private enum CheDoDieuPhoiPhien
        {
            KhongCo,
            TuDongLayMoiTuDs,
            DangNhapDanhSachDaTick,
            MoChromeDanhSachDaTickKhongDong
        }

        private sealed class TaiKhoanDuocChon
        {
            public string Uid { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        private sealed class DuLieuCapNhatDong
        {
            public string Uid { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Ten { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string GhiChu { get; set; } = string.Empty;
        }

        private sealed class DuLieuDongGrid
        {
            public string Uid { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string NgayTao { get; set; } = string.Empty;
            public string Ten { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string GhiChu { get; set; } = string.Empty;
            public string TuongTacCuoi { get; set; } = string.Empty;
            public string TrangThai { get; set; } = string.Empty;
            public string Cookie { get; set; } = string.Empty;
        }

        public Form1()
        {
            InitializeComponent();
            CapNhatTrangThai("Đang khởi tạo dữ liệu ứng dụng...", Color.DarkGoldenrod);      // Báo ngay từ đầu để người dùng thấy app đang nạp dữ liệu chứ không bị treo
            GanVersionLenTieuDeForm();                                                        // Hiển thị version ngay trên thanh tiêu đề để dễ nhận biết bản đang chạy
            userAgentsFilePath = AppPaths.UserAgentsFilePath;                                 // Danh sách User-Agent chính thức nằm trong data\
            userAgentDangDungFilePath = AppPaths.UserAgentDangDungFilePath;                   // File log User-Agent đang dùng cũng đi theo data\
            dsFilePath = AppPaths.DsFilePath;                                                 // ds.txt chính thức nằm trong data\
            gridFilePath = AppPaths.GridFilePath;                                             // grid.json dùng để giữ lại đúng danh sách đang có trên bảng
            profileMauPath = AppPaths.ProfileMauPath;                                         // Profile mẫu chính thức nằm trong data\
            profileRanhPath = AppPaths.ProfileRanhPath;                                       // Profile rảnh chính thức nằm trong data\
            LoadDuLieuLenGridKhiMoApp();                                                      // Khi app vừa mở thì khởi tạo grid cho mô hình phiên tạm
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

            TaiDanhSachUserAgentLenCombobox();                                                 // Nạp danh sách User-Agent từ file txt lên combobox ngay khi app khởi động
            GanMenuUserAgentChoCombobox();                                                     // Gắn menu chuột phải để thêm và xóa User-Agent ngay trên app mà không phải sửa txt bằng tay
            CapNhatTrangThai("Sẵn sàng.", Color.RoyalBlue);                                    // Sau khi khởi tạo xong thì đưa app về trạng thái chờ thao tác
            tssTime.Text = DateTime.Now.ToString("HH:mm:ss    dd/MM/yyyy");
        }

        private void GanVersionLenTieuDeForm()
        {
            AppVersionInfo thongTinPhienBan = VersionService.DocThongTinPhienBanHienTai();    // Lấy version hiện tại từ metadata build để gắn lên tiêu đề form
            if (string.IsNullOrWhiteSpace(thongTinPhienBan.Version))
            {
                return;                                                                        // Nếu thiếu version thì giữ nguyên tiêu đề cũ của form
            }

            Text = $"{Text} - v{thongTinPhienBan.Version}";                                    // Hiển thị phiên bản ngay trên thanh tiêu đề cho chuyên nghiệp và dễ kiểm tra
        }

        private void CapNhatTrangThai(string noiDung, Color mauChu)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => CapNhatTrangThai(noiDung, mauChu));                          // Các luồng async vẫn phải quay về UI thread để tránh lỗi cross-thread
                return;
            }

            tssTrangThai.Text = $"{noiDung}";                                      // Gom toàn bộ diễn giải tiến trình về đúng một nhãn trung tâm ở cạnh dưới form
            tssTrangThai.ForeColor = mauChu;
            tssTrangThai.Owner?.Refresh();                                                     // Ép refresh ngay để người dùng thấy trạng thái mới trong lúc app còn đang xử lý
            statusStrip1.Refresh();
            Update();
        }

        private void CapNhatTrangThaiDongTheoUid(string uid, string trangThai)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => CapNhatTrangThaiDongTheoUid(uid, trangThai));                // Luồng theo dõi đăng nhập chạy async nên mọi cập nhật grid phải quay về UI thread
                return;
            }

            if (string.IsNullOrWhiteSpace(uid))
            {
                return;
            }

            string uidCanTim = uid.Trim();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string uidTrenDong = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                if (!string.Equals(uidTrenDong, uidCanTim, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                row.Cells["colTrangThai"].Value = trangThai;
                row.Cells["colTuongTacCuoi"].Value = DateTime.Now.ToString("dd/MM HH:mm:ss");
                CapNhatMauDongGrid(row);
                LuuDuLieuGridRaFile();
                break;
            }
        }
        //
        //   CẬP NHẬT MÀU TRONG GIRD
        //
        private void CapNhatMauDongGrid(DataGridViewRow? dongCanCapNhat = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => CapNhatMauDongGrid(dongCanCapNhat));                         // Mọi thay đổi màu dòng đều phải quay về UI thread để tránh lỗi cross-thread
                return;
            }

            static string ChuanHoaTrangThai(string? trangThai)
            {
                if (string.IsNullOrWhiteSpace(trangThai))
                {
                    return string.Empty;
                }

                string daChuanHoa = trangThai.Trim().Normalize(NormalizationForm.FormD);
                var builder = new StringBuilder(daChuanHoa.Length);
                foreach (char kyTu in daChuanHoa)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(kyTu) != UnicodeCategory.NonSpacingMark)
                    {
                        builder.Append(char.ToLowerInvariant(kyTu));
                    }
                }

                return builder.ToString().Normalize(NormalizationForm.FormC);
            }

            static bool ChuaBatKyCumTuNao(string trangThaiDaChuanHoa, params string[] cumTu)
            {
                foreach (string item in cumTu)
                {
                    if (trangThaiDaChuanHoa.Contains(item, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            static (Color MauNen, Color MauChu) LayMauTheoTrangThai(string trangThai)
            {
                string trangThaiDaChuanHoa = ChuanHoaTrangThai(trangThai);
                if (string.IsNullOrWhiteSpace(trangThaiDaChuanHoa))
                {
                    return (Color.White, Color.Black);
                }

                if (ChuaBatKyCumTuNao(trangThaiDaChuanHoa, "dang nhap thanh cong", "thanh cong"))
                {
                    return (Color.FromArgb(212, 237, 182), Color.FromArgb(33, 87, 50));       // Xanh nhạt kiểu MaxCare cho kết quả tốt
                }

                if (ChuaBatKyCumTuNao(trangThaiDaChuanHoa, "da dung thu cong"))
                {
                    return (Color.FromArgb(238, 242, 247), Color.FromArgb(71, 85, 105));      // Dừng thủ công là trạng thái trung tính, không nên nhuộm đỏ như lỗi thật
                }

                if (ChuaBatKyCumTuNao(trangThaiDaChuanHoa, "dang ", "cho ", "mo phien moi sau", "da gui dang nhap", "da thu lai lan 2"))
                {
                    return (Color.FromArgb(219, 234, 254), Color.FromArgb(30, 64, 175));      // Màu xanh dương nhạt cho luồng đang xử lý/chờ
                }

                if (ChuaBatKyCumTuNao(trangThaiDaChuanHoa, "captcha", "checkpoint", "can nhap", "can xac minh", "mat khau da thay doi", "956"))
                {
                    return (Color.FromArgb(255, 236, 179), Color.FromArgb(146, 64, 14));      // Vàng cam nhạt cho trạng thái cần chú ý nhưng chưa coi là lỗi đứt hẳn
                }

                if (ChuaBatKyCumTuNao(trangThaiDaChuanHoa, "dung:", "sai ", "khong ", "loi", "het thoi gian cho", "bi khoa", "vo hieu hoa", "thu lai sau"))
                {
                    return (Color.FromArgb(255, 182, 193), Color.FromArgb(127, 29, 29));      // Hồng đỏ nhạt kiểu MaxCare cho kết quả xấu/lỗi
                }

                return (Color.White, Color.Black);
            }

            static void ApMauChoDong(DataGridViewRow row)
            {
                if (row.IsNewRow)
                {
                    return;
                }

                string trangThai = row.Cells["colTrangThai"].Value?.ToString() ?? string.Empty;
                (Color mauNen, Color mauChu) = LayMauTheoTrangThai(trangThai);
                row.DefaultCellStyle.BackColor = mauNen;
                row.DefaultCellStyle.ForeColor = mauChu;
                row.DefaultCellStyle.SelectionBackColor = mauNen;
                row.DefaultCellStyle.SelectionForeColor = mauChu;
            }

            if (dongCanCapNhat != null)
            {
                ApMauChoDong(dongCanCapNhat);
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ApMauChoDong(row);
            }
        }

        private void DatLaiYeuCauDungThuCong()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                daYeuCauDungThuCong = false;                                                   // Khi người dùng chủ động bấm Next/Mở lại thì bỏ cờ Stop cũ để app được phép chạy tiếp
                maDieuKhienTuDong++;                                                           // Lệnh thủ công mới phải vô hiệu mọi countdown cũ còn sót lại
            }
        }

        private bool CoYeuCauDungThuCong()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                return daYeuCauDungThuCong;                                                    // Mọi luồng async đều đọc chung một cờ Stop để dừng đúng thời điểm người dùng yêu cầu
            }
        }

        private void DanhDauDungThuCong()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                daYeuCauDungThuCong = true;                                                    // Stop phải chặn luôn cả countdown lẫn việc mở phiên kế tiếp
                maDieuKhienTuDong++;                                                           // Mỗi lần Stop là một mốc điều khiển mới để countdown cũ tự vô hiệu
            }
        }

        private int TaoMaDieuKhienTuDongMoi()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                maDieuKhienTuDong++;
                return maDieuKhienTuDong;
            }
        }

        private bool MaDieuKhienTuDongConHieuLuc(int maDieuKhien)
        {
            lock (dieuKhienPhienSyncRoot)
            {
                return !daYeuCauDungThuCong && maDieuKhienTuDong == maDieuKhien;              // Countdown chỉ được chạy tiếp nếu chưa bị Stop và chưa có lệnh mới ghi đè
            }
        }

        private void GhiNhanPhienDangXuLy(SessionModel session, string uid)
        {
            lock (dieuKhienPhienSyncRoot)
            {
                uidPhienDangXuLy = uid.Trim();
                sessionIdPhienDangXuLy = session.SessionId?.Trim() ?? string.Empty;            // Lưu đúng phiên đang chạy để Stop đóng đúng Chrome hiện tại, không đụng nhầm dòng khác
            }
        }

        private (string Uid, string SessionId) LayThongTinPhienDangXuLy()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                return (uidPhienDangXuLy, sessionIdPhienDangXuLy);                            // Gộp UID và SessionId hiện tại để Stop/cleanup không đóng nhầm dòng khác
            }
        }

        private void XoaThongTinPhienDangXuLy(string uid, string sessionId)
        {
            lock (dieuKhienPhienSyncRoot)
            {
                bool khopUid = string.IsNullOrWhiteSpace(uid) || string.Equals(uidPhienDangXuLy, uid, StringComparison.OrdinalIgnoreCase);
                bool khopSession = string.IsNullOrWhiteSpace(sessionId) || string.Equals(sessionIdPhienDangXuLy, sessionId, StringComparison.OrdinalIgnoreCase);

                if (!khopUid || !khopSession)
                {
                    return;
                }

                uidPhienDangXuLy = string.Empty;
                sessionIdPhienDangXuLy = string.Empty;
            }
        }

        private void XoaHangChoTaiKhoanDaTick()
        {
            lock (dieuKhienPhienSyncRoot)
            {
                hangChoTaiKhoanDaTick.Clear();
                cheDoHangChoTaiKhoanDaTick = CheDoDieuPhoiPhien.KhongCo;
            }
        }

        private void NapHangChoTaiKhoanDaTick(IEnumerable<TaiKhoanDuocChon> danhSachTaiKhoan, CheDoDieuPhoiPhien cheDo)
        {
            lock (dieuKhienPhienSyncRoot)
            {
                hangChoTaiKhoanDaTick.Clear();
                foreach (TaiKhoanDuocChon taiKhoan in danhSachTaiKhoan)
                {
                    hangChoTaiKhoanDaTick.Enqueue(taiKhoan);
                }
                cheDoHangChoTaiKhoanDaTick = hangChoTaiKhoanDaTick.Count == 0 ? CheDoDieuPhoiPhien.KhongCo : cheDo;
            }
        }

        private bool TryLayTaiKhoanDaTickTiepTheo(out TaiKhoanDuocChon taiKhoan, out CheDoDieuPhoiPhien cheDo)
        {
            lock (dieuKhienPhienSyncRoot)
            {
                if (hangChoTaiKhoanDaTick.Count == 0 || cheDoHangChoTaiKhoanDaTick == CheDoDieuPhoiPhien.KhongCo)
                {
                    taiKhoan = new TaiKhoanDuocChon();
                    cheDo = CheDoDieuPhoiPhien.KhongCo;
                    cheDoHangChoTaiKhoanDaTick = CheDoDieuPhoiPhien.KhongCo;
                    return false;
                }

                taiKhoan = hangChoTaiKhoanDaTick.Dequeue();
                cheDo = cheDoHangChoTaiKhoanDaTick;
                if (hangChoTaiKhoanDaTick.Count == 0)
                {
                    cheDoHangChoTaiKhoanDaTick = CheDoDieuPhoiPhien.KhongCo;
                }
                return true;
            }
        }

        private bool LaCheDoChayTiepTheoDanhSachDaTick(CheDoDieuPhoiPhien cheDo)
        {
            return cheDo == CheDoDieuPhoiPhien.DangNhapDanhSachDaTick ||
                   cheDo == CheDoDieuPhoiPhien.MoChromeDanhSachDaTickKhongDong;
        }

        private bool LaCheDoCanDongChromeSauKetQua(CheDoDieuPhoiPhien cheDo)
        {
            return cheDo == CheDoDieuPhoiPhien.TuDongLayMoiTuDs ||
                   cheDo == CheDoDieuPhoiPhien.DangNhapDanhSachDaTick;
        }

        private bool LaCheDoCanChayTiepSauKetQua(CheDoDieuPhoiPhien cheDo)
        {
            return cheDo == CheDoDieuPhoiPhien.TuDongLayMoiTuDs ||
                   LaCheDoChayTiepTheoDanhSachDaTick(cheDo);
        }

        private void DieuPhoiBuocTiepTheoSauKetQua(CheDoDieuPhoiPhien cheDo)
        {
            if (!LaCheDoCanChayTiepSauKetQua(cheDo))
            {
                return;
            }

            int maDieuKhien = TaoMaDieuKhienTuDongMoi();
            if (cheDo == CheDoDieuPhoiPhien.TuDongLayMoiTuDs)
            {
                _ = ChoMoPhienMoiSau5GiayAsync(maDieuKhien, XuLyNutNext);
                return;
            }

            _ = ChoMoPhienMoiSau5GiayAsync(maDieuKhien, XuLyTaiKhoanDaTickTiepTheo);
        }

        private void DongVaDonDepPhienDangXuLy(string uidUuTien = "")
        {
            (string uidDangXuLy, string sessionIdDangXuLy) = LayThongTinPhienDangXuLy();
            string uidCanDong = string.IsNullOrWhiteSpace(uidUuTien) ? uidDangXuLy : uidUuTien.Trim();

            if (!string.IsNullOrWhiteSpace(sessionIdDangXuLy) &&
                (string.IsNullOrWhiteSpace(uidCanDong) || string.Equals(uidDangXuLy, uidCanDong, StringComparison.OrdinalIgnoreCase)))
            {
                SessionRuntimeService.TryCloseAndCleanupSession(sessionIdDangXuLy);
                XoaThongTinPhienDangXuLy(uidDangXuLy, sessionIdDangXuLy);
            }
            else if (!string.IsNullOrWhiteSpace(uidCanDong))
            {
                SessionRuntimeService.TryCloseAndCleanupSessionsByUid(uidCanDong);
                XoaThongTinPhienDangXuLy(uidCanDong, string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(uidCanDong))
            {
                congDebugTheoUid.Remove(uidCanDong);
            }
        }

        private async Task ChoMoPhienMoiSau5GiayAsync(int maDieuKhien, Action buocTiepTheo)
        {
            for (int soGiayConLai = soGiayChoMoPhienMoi; soGiayConLai >= 1; soGiayConLai--)
            {
                if (!MaDieuKhienTuDongConHieuLuc(maDieuKhien) || IsDisposed || !IsHandleCreated)
                {
                    return;
                }

                CapNhatTrangThai($"Đang mở phiên mới sau {soGiayConLai} giây...", Color.DarkGoldenrod); // Bắt đầu phiên mới thì chỉ báo đúng luồng mở phiên mới, không nhắc lại kết quả phiên cũ
                await Task.Delay(1000);
            }

            if (!MaDieuKhienTuDongConHieuLuc(maDieuKhien) || IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(buocTiepTheo);                                                        // Mỗi chế độ sẽ tự quyết định bước tiếp theo sau countdown, không còn cố định chỉ gọi Next
        }

        private async void HoanTatPhienVaChoMoTiep(string uid, string thongBao, Color mauChu, CheDoDieuPhoiPhien cheDo)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => HoanTatPhienVaChoMoTiep(uid, thongBao, mauChu, cheDo));
                return;
            }

            CapNhatTrangThai(thongBao, mauChu);                                               // Đẩy trạng thái kết quả ra UI ngay trước khi bắt đầu đóng Chrome
            await Task.Delay(khoangNghiNhinKetQuaTruocKhiDongChromeMs);                       // Mọi kết quả auto đều phải dừng 5 giây để người dùng kịp nhìn trước khi app tự đóng Chrome

            if (LaCheDoCanDongChromeSauKetQua(cheDo))
            {
                await Task.Run(() => DongVaDonDepPhienDangXuLy(uid));
                await Task.Delay(khoangNghiSauKhiDongChromeMs);                               // Chỉ các chế độ auto đóng Chrome mới cần nhịp tách phiên cũ và phiên mới
            }

            if (CoYeuCauDungThuCong())
            {
                return;                                                                        // Stop được bấm trong lúc kết thúc phiên thì chỉ dọn Chrome, không tự Next nữa
            }

            DieuPhoiBuocTiepTheoSauKetQua(cheDo);
        }

        private void CapNhatKetQuaTheoCheDo(string uid, string trangThaiDong, string thongBao, Color mauChu, CheDoDieuPhoiPhien cheDo)
        {
            if (!string.IsNullOrWhiteSpace(trangThaiDong))
            {
                CapNhatTrangThaiDongTheoUid(uid, trangThaiDong);                               // Grid luôn phải được cập nhật trước để người dùng nhìn thấy kết quả ngay trên dòng đang xử lý
            }

            DanhDauDongDaXuLyTrongBatch(uid, cheDo);                                           // Batch tick phải bỏ check ngay khi dòng đã có kết quả cuối để lần sau không chạy lại dòng cũ

            if (LaCheDoCanChayTiepSauKetQua(cheDo) || LaCheDoCanDongChromeSauKetQua(cheDo))
            {
                HoanTatPhienVaChoMoTiep(uid, thongBao, mauChu, cheDo);                         // Mọi chế độ batch đều đi qua một nơi điều phối chung để không lệch hành vi
                return;
            }

            CapNhatTrangThai(thongBao, mauChu);                                                 // Luồng mở thủ công chỉ cập nhật trạng thái rồi dừng lại, không đóng Chrome và không gọi Next
        }

        private async void XoaUidMatKhauDaThayDoiVaChayTiep(string uid, CheDoDieuPhoiPhien cheDo)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => XoaUidMatKhauDaThayDoiVaChayTiep(uid, cheDo));              // Nhánh này thường được gọi từ luồng theo dõi async nên cần quay về UI thread
                return;
            }

            if (string.IsNullOrWhiteSpace(uid))
            {
                return;
            }

            string uidCanXuLy = uid.Trim();
            string thongBao = $"UID {uidCanXuLy} có mật khẩu đã thay đổi.";

            if (!LaCheDoCanDongChromeSauKetQua(cheDo))
            {
                CapNhatKetQuaTheoCheDo(uidCanXuLy, "Dừng: mật khẩu đã thay đổi", thongBao, Color.DarkOrange, cheDo);
                return;                                                                        // Các chế độ giữ Chrome mở chỉ ghi nhận trạng thái rồi tự điều phối theo mode, không xóa dòng
            }

            DanhDauDongDaXuLyTrongBatch(uidCanXuLy, cheDo);                                    // Nhánh tự xóa dòng cũng nên bỏ tick trước để người dùng không thấy dòng cũ còn được chọn
            CapNhatTrangThai(thongBao, Color.DarkOrange);                                     // Giữ nguyên kết quả mật khẩu đổi trên màn hình để người dùng còn nhìn thấy trước khi app đóng Chrome
            await Task.Delay(khoangNghiNhinKetQuaTruocKhiDongChromeMs);                       // Giữ nguyên quy ước chung: mọi kết quả auto đều phải cho người dùng nhìn 5 giây trước khi đóng Chrome
            CapNhatTrangThai($"{thongBao} Đang đóng phiên hiện tại...", Color.DarkOrange);
            await Task.Run(() => DongVaDonDepPhienDangXuLy(uidCanXuLy));
            await Task.Delay(khoangNghiSauKhiDongChromeMs);                                   // Tách nhịp giữa lúc đóng xong phiên cũ và lúc app đẩy Next

            XoaUidKhoiDsTxt(uidCanXuLy);

            DataGridViewRow? rowCanXoa = null;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string uidTrenDong = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                if (string.Equals(uidTrenDong, uidCanXuLy, StringComparison.OrdinalIgnoreCase))
                {
                    rowCanXoa = row;
                    break;
                }
            }

            if (rowCanXoa != null)
            {
                dataGridView1.Rows.Remove(rowCanXoa);
                CapNhatLaiSTT();
            }

            LuuDuLieuGridRaFile();

            if (CoYeuCauDungThuCong())
            {
                return;
            }

            DieuPhoiBuocTiepTheoSauKetQua(cheDo);                                             // Sau khi xóa xong dòng thì tiếp tục đúng luồng của mode hiện tại
        }

        private void DanhDauUidCanCaptchaVaChayTiep(string uid, CheDoDieuPhoiPhien cheDo)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => DanhDauUidCanCaptchaVaChayTiep(uid, cheDo));                // Nhánh captcha cũng thường đi ra từ luồng theo dõi async nên phải quay về UI thread
                return;
            }

            if (string.IsNullOrWhiteSpace(uid))
            {
                return;
            }

            string uidCanXuLy = uid.Trim();
            CapNhatKetQuaTheoCheDo(uidCanXuLy, "Dừng: cần nhập captcha", $"UID {uidCanXuLy} gặp captcha.", Color.DarkOrange, cheDo);
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

            lblDanhSach.Text = $"Số lượng DS : {soLuongDsTxtConLai}";                     // Label Danh Sách phản ánh số dòng còn lại trong ds.txt
            tssTong.Text = $"Tổng : {dataGridView1.Rows.Count}";                               // Thanh trạng thái Tổng phản ánh số dòng hiện đang có trên grid
        }

        private void TaiDanhSachUserAgentLenCombobox()
        {
            cboUserAgent.Items.Clear();                                                         // Mỗi lần nạp lại thì xóa danh sách cũ để tránh bị trùng lặp
            if (!File.Exists(userAgentsFilePath))
            {
                return;                                                                         // Thiếu file thì dừng, vì hàm đảm bảo sẽ tạo lại trước đó rồi
            }
            List<string> danhSachUserAgent = File.ReadAllLines(userAgentsFilePath)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();                                                                      // Chỉ lấy các dòng UA có giá trị và loại bỏ trùng lặp
            foreach (string userAgent in danhSachUserAgent)
            {
                cboUserAgent.Items.Add(userAgent);                                              // Đổ từng User-Agent từ file txt lên combobox để người dùng chọn
            }
            if (cboUserAgent.Items.Count == 0)
            {
                return;                                                                         // Không có UA nào thì dừng, tránh gán SelectedItem khi combobox đang trống
            }
            if (danhSachUserAgent.Contains(facebookDesktopUserAgentMacDinh, StringComparer.OrdinalIgnoreCase))
            {
                cboUserAgent.SelectedItem = facebookDesktopUserAgentMacDinh;                   // Facebook thường mặc định quay về UA desktop đang test ổn này
                return;
            }
            cboUserAgent.SelectedIndex = 0;                                                     // Nếu không có UA desktop mặc định thì chọn dòng đầu tiên trong file
        }

        private void GanMenuUserAgentChoCombobox()
        {
            ContextMenuStrip cmsUserAgent = new();                                              // Tạo menu chuột phải riêng cho combobox User-Agent để không đụng layout hiện có
            ToolStripMenuItem themUserAgentToolStripMenuItem = new("Thêm User-Agent");         // Mục thêm mới để người dùng tự bổ sung UA cần test
            ToolStripMenuItem xoaUserAgentToolStripMenuItem = new("Xóa User-Agent đang chọn"); // Mục xóa đúng UA đang chọn khỏi danh sách

            themUserAgentToolStripMenuItem.Click += (_, _) => ThemUserAgentMoi();              // Bấm menu là mở hộp thoại nhập UA mới rồi lưu vào file txt
            xoaUserAgentToolStripMenuItem.Click += (_, _) => XoaUserAgentDangChon();           // Bấm menu là xóa đúng UA đang chọn khỏi combobox và file txt

            cmsUserAgent.Items.Add(themUserAgentToolStripMenuItem);
            cmsUserAgent.Items.Add(xoaUserAgentToolStripMenuItem);
            cboUserAgent.ContextMenuStrip = cmsUserAgent;                                       // Gắn menu thẳng vào combobox để dùng ngay trên app
        }

        private void ThemUserAgentMoi()
        {
            string? userAgentMoi = HienHopThoaiNhapUserAgent();                                 // Lấy chuỗi UA người dùng nhập từ hộp thoại nhỏ trên app
            if (string.IsNullOrWhiteSpace(userAgentMoi))
            {
                return;                                                                         // Hủy hoặc để trống thì dừng, không ghi gì vào danh sách
            }

            userAgentMoi = userAgentMoi.Trim();                                                 // Cắt khoảng trắng thừa để tránh lưu cùng một UA thành hai dòng khác nhau

            foreach (object item in cboUserAgent.Items)
            {
                string userAgentDangCo = item?.ToString()?.Trim() ?? string.Empty;
                if (string.Equals(userAgentDangCo, userAgentMoi, StringComparison.OrdinalIgnoreCase))
                {
                    cboUserAgent.SelectedItem = item;                                           // Nếu UA đã có thì chỉ chọn lại để khỏi tạo bản trùng
                    MessageBox.Show("User-Agent này đã có sẵn.");
                    return;
                }
            }

            cboUserAgent.Items.Add(userAgentMoi);                                               // Thêm UA mới vào combobox để người dùng thấy ngay trên app
            cboUserAgent.SelectedItem = userAgentMoi;                                           // Chọn luôn UA vừa thêm để tiện test facebook.com ngay
            LuuDanhSachUserAgentTuCombobox();                                                   // Ghi lại toàn bộ danh sách xuống user_agents.txt để giữ đồng bộ với UI
        }

        private void XoaUserAgentDangChon()
        {
            if (cboUserAgent.SelectedItem == null)
            {
                MessageBox.Show("Hãy chọn User-Agent cần xóa trước.");                          // Không có dòng nào đang chọn thì chưa xác định được sẽ xóa UA nào
                return;
            }

            if (cboUserAgent.Items.Count <= 1)
            {
                MessageBox.Show("Cần giữ lại ít nhất 1 User-Agent.");                           // Chặn xóa trắng danh sách để facebook.com không bị mất UA đang dùng
                return;
            }

            object userAgentDangChon = cboUserAgent.SelectedItem;                               // Giữ lại giá trị đang chọn để xóa đúng item khỏi danh sách hiện tại
            cboUserAgent.Items.Remove(userAgentDangChon);                                       // Xóa UA đang chọn khỏi combobox

            if (cboUserAgent.Items.Count > 0)
            {
                cboUserAgent.SelectedIndex = 0;                                                 // Sau khi xóa thì chọn lại một UA hợp lệ để app vẫn chạy tiếp được ngay
            }

            LuuDanhSachUserAgentTuCombobox();                                                   // Ghi lại danh sách còn lại xuống file txt để UI và file luôn đồng bộ
        }

        private void LuuDanhSachUserAgentTuCombobox()
        {
            List<string> danhSachUserAgent = [];

            foreach (object item in cboUserAgent.Items)
            {
                string userAgent = item?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(userAgent))
                {
                    danhSachUserAgent.Add(userAgent);                                           // Chỉ lưu các UA còn dữ liệu thật để file txt luôn sạch
                }
            }

            File.WriteAllLines(userAgentsFilePath, danhSachUserAgent.Distinct(StringComparer.OrdinalIgnoreCase), Encoding.UTF8); // Lưu lại danh sách hiện có trên app xuống user_agents.txt
        }

        private string? HienHopThoaiNhapUserAgent()
        {
            using Form formNhap = new();                                                        // Hộp thoại nhỏ tạo bằng code để không phải thêm control mới vào Designer
            using Label lblNoiDung = new();
            using TextBox txtUserAgent = new();
            using Button btnDongY = new();
            using Button btnHuy = new();

            formNhap.Text = "Thêm User-Agent";
            formNhap.StartPosition = FormStartPosition.CenterParent;
            formNhap.FormBorderStyle = FormBorderStyle.FixedDialog;
            formNhap.MinimizeBox = false;
            formNhap.MaximizeBox = false;
            formNhap.ClientSize = new Size(640, 150);

            lblNoiDung.Text = "Nhập User-Agent mới:";
            lblNoiDung.AutoSize = true;
            lblNoiDung.Location = new Point(12, 15);

            txtUserAgent.Location = new Point(12, 40);
            txtUserAgent.Size = new Size(610, 23);
            txtUserAgent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            btnDongY.Text = "Lưu";
            btnDongY.Location = new Point(466, 95);
            btnDongY.Size = new Size(75, 30);
            btnDongY.DialogResult = DialogResult.OK;

            btnHuy.Text = "Hủy";
            btnHuy.Location = new Point(547, 95);
            btnHuy.Size = new Size(75, 30);
            btnHuy.DialogResult = DialogResult.Cancel;

            formNhap.Controls.Add(lblNoiDung);
            formNhap.Controls.Add(txtUserAgent);
            formNhap.Controls.Add(btnDongY);
            formNhap.Controls.Add(btnHuy);
            formNhap.AcceptButton = btnDongY;
            formNhap.CancelButton = btnHuy;

            return formNhap.ShowDialog(this) == DialogResult.OK ? txtUserAgent.Text : null;    // Trả chuỗi đã nhập nếu người dùng bấm Lưu, ngược lại trả null
        }

        private void cậpNhậtDữLiệuToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && (menuItem.Text ?? string.Empty).Contains("danh sách", StringComparison.CurrentCultureIgnoreCase))
            {
                MoDsTxtBangNotepadDeNhapThem();                                               // Nếu menu này đang được dùng như "Cập nhật danh sách" thì mở thẳng ds.txt như nghiệp vụ mới
                return;
            }

            CapNhatDuLieuDongDaTick();                                                         // Menu Cập nhật dữ liệu giờ chỉ còn một dòng duy nhất và mở thẳng hộp sửa của dòng đang tick
        }

        private void CapNhatDuLieuDongDaTick()
        {
            CapNhatTrangThai("Đang chuẩn bị cập nhật dữ liệu dòng đang chọn...", Color.DarkGoldenrod); // Báo trước để người dùng biết app đang mở luồng sửa dữ liệu của dòng
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();
            if (dsDongDaTick.Count != 1)
            {
                CapNhatTrangThai("Cập nhật dữ liệu thất bại: chưa tick đúng 1 dòng.", Color.Firebrick);
                MessageBox.Show("Vui lòng tick đúng 1 dòng để cập nhật dữ liệu.");
                return;
            }

            DataGridViewRow row = dsDongDaTick[0];
            string uidCu = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;

            DuLieuCapNhatDong duLieuHienTai = new()
            {
                Uid = uidCu,
                Password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty,
                Ten = row.Cells["colTen"].Value?.ToString()?.Trim() ?? string.Empty,
                Email = row.Cells["colEmail"].Value?.ToString()?.Trim() ?? string.Empty,
                GhiChu = row.Cells["colGhiChu"].Value?.ToString() ?? string.Empty
            };

            DuLieuCapNhatDong? duLieuMoi = HienHopThoaiCapNhatDuLieu(duLieuHienTai);
            if (duLieuMoi == null)
            {
                CapNhatTrangThai("Đã hủy cập nhật dữ liệu dòng.", Color.RoyalBlue);
                return;
            }

            duLieuMoi.Uid = duLieuMoi.Uid.Trim();
            duLieuMoi.Password = duLieuMoi.Password.Trim();
            duLieuMoi.Ten = duLieuMoi.Ten.Trim();
            duLieuMoi.Email = duLieuMoi.Email.Trim();
            duLieuMoi.GhiChu = duLieuMoi.GhiChu.Trim();

            if (string.IsNullOrWhiteSpace(duLieuMoi.Uid) || string.IsNullOrWhiteSpace(duLieuMoi.Password))
            {
                CapNhatTrangThai("Cập nhật dữ liệu thất bại: thiếu UID hoặc Password.", Color.Firebrick);
                MessageBox.Show("UID và Password không được để trống.");
                return;
            }

            if (CoDongKhacTrungUid(row, duLieuMoi.Uid))
            {
                CapNhatTrangThai("Cập nhật dữ liệu thất bại: UID đã tồn tại trên grid.", Color.Firebrick);
                MessageBox.Show("UID này đã có trên grid.");
                return;
            }

            if (!XuLyDongBoUidVaProfileKhiCapNhat(uidCu, duLieuMoi.Uid))
            {
                return;
            }

            CapNhatTaiKhoanTrongDsTxt(uidCu, duLieuMoi.Uid, duLieuMoi.Password);

            row.Cells["colUID"].Value = duLieuMoi.Uid;
            row.Cells["colPass"].Value = duLieuMoi.Password;
            row.Cells["colTen"].Value = duLieuMoi.Ten;
            row.Cells["colEmail"].Value = duLieuMoi.Email;
            row.Cells["colGhiChu"].Value = duLieuMoi.GhiChu;

            LuuDuLieuGridRaFile();
            CapNhatThongTinSoLuong();
            CapNhatTrangThai($"Đã cập nhật dữ liệu dòng {duLieuMoi.Uid}.", Color.ForestGreen);
        }

        private bool CoDongKhacTrungUid(DataGridViewRow dongDangSua, string uidMoi)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow || ReferenceEquals(row, dongDangSua))
                {
                    continue;
                }

                string uidTrenDong = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                if (string.Equals(uidTrenDong, uidMoi, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool XuLyDongBoUidVaProfileKhiCapNhat(string uidCu, string uidMoi)
        {
            if (string.Equals(uidCu, uidMoi, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            SessionRuntimeService.TryCloseAndCleanupSessionsByUid(uidCu);
            congDebugTheoUid.Remove(uidCu);
            congDebugTheoUid.Remove(uidMoi);
            return true;
        }

        private static bool TryTachUidVaPasswordTuDongDs(string line, out string uid, out string password, out string phanDuSauPassword)
        {
            uid = string.Empty;
            password = string.Empty;
            phanDuSauPassword = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string lineDaCat = line.Trim();
            int viTriGachDauTien = lineDaCat.IndexOf('|');
            if (viTriGachDauTien <= 0)
            {
                return false;                                                                 // Dong khong co UID hop le truoc dau | dau tien
            }

            int viTriGachThuHai = lineDaCat.IndexOf('|', viTriGachDauTien + 1);
            uid = lineDaCat[..viTriGachDauTien].Trim();

            if (viTriGachThuHai < 0)
            {
                password = lineDaCat[(viTriGachDauTien + 1)..].Trim();                       // Truong hop chi co 1 dau | thi toan bo phia sau la password
                return !string.IsNullOrWhiteSpace(uid) && !string.IsNullOrWhiteSpace(password);
            }

            password = lineDaCat[(viTriGachDauTien + 1)..viTriGachThuHai].Trim();            // Co nhieu cot phu thi chi lay password tai giua 2 dau | dau tien
            phanDuSauPassword = lineDaCat[viTriGachThuHai..];                                 // Giu nguyen phan du de khi cap nhat co the bao toan du lieu bo sung
            return !string.IsNullOrWhiteSpace(uid) && !string.IsNullOrWhiteSpace(password);
        }

        private void CapNhatTaiKhoanTrongDsTxt(string uidCu, string uidMoi, string passwordMoi)
        {
            List<string> dsSauCapNhat = new();
            bool daCapNhat = false;

            if (File.Exists(dsFilePath))
            {
                foreach (string line in File.ReadAllLines(dsFilePath))
                {
                    string lineDaCat = line.Trim();
                    if (string.IsNullOrWhiteSpace(lineDaCat))
                    {
                        continue;
                    }

                    if (TryTachUidVaPasswordTuDongDs(lineDaCat, out string uidTrenDong, out _, out string phanDuSauPassword) &&
                        string.Equals(uidTrenDong, uidCu, StringComparison.OrdinalIgnoreCase))
                    {
                        dsSauCapNhat.Add($"{uidMoi}|{passwordMoi}{phanDuSauPassword}");
                        daCapNhat = true;
                        continue;
                    }

                    dsSauCapNhat.Add(lineDaCat);
                }
            }

            if (!daCapNhat)
            {
                dsSauCapNhat.Add($"{uidMoi}|{passwordMoi}");                                   // Nếu profile cũ chưa còn trong ds.txt thì thêm lại một dòng mới cho đồng bộ
            }

            File.WriteAllLines(dsFilePath, dsSauCapNhat, Encoding.UTF8);
        }

        private DuLieuCapNhatDong? HienHopThoaiCapNhatDuLieu(DuLieuCapNhatDong duLieuHienTai)
        {
            using Form formNhap = new();
            using Label lblUid = new();
            using Label lblPassword = new();
            using Label lblTen = new();
            using Label lblEmail = new();
            using Label lblGhiChu = new();
            using TextBox txtUid = new();
            using TextBox txtPassword = new();
            using TextBox txtTen = new();
            using TextBox txtEmail = new();
            using TextBox txtGhiChu = new();
            using Button btnLuu = new();
            using Button btnHuy = new();

            formNhap.Text = "Cập nhật dữ liệu";
            formNhap.StartPosition = FormStartPosition.CenterParent;
            formNhap.FormBorderStyle = FormBorderStyle.FixedDialog;
            formNhap.MinimizeBox = false;
            formNhap.MaximizeBox = false;
            formNhap.ClientSize = new Size(520, 310);

            lblUid.Text = "UID";
            lblUid.Location = new Point(12, 15);
            lblUid.AutoSize = true;
            txtUid.Location = new Point(110, 12);
            txtUid.Size = new Size(390, 23);
            txtUid.Text = duLieuHienTai.Uid;

            lblPassword.Text = "Password";
            lblPassword.Location = new Point(12, 50);
            lblPassword.AutoSize = true;
            txtPassword.Location = new Point(110, 47);
            txtPassword.Size = new Size(390, 23);
            txtPassword.Text = duLieuHienTai.Password;

            lblTen.Text = "Tên";
            lblTen.Location = new Point(12, 85);
            lblTen.AutoSize = true;
            txtTen.Location = new Point(110, 82);
            txtTen.Size = new Size(390, 23);
            txtTen.Text = duLieuHienTai.Ten;

            lblEmail.Text = "Mail";
            lblEmail.Location = new Point(12, 120);
            lblEmail.AutoSize = true;
            txtEmail.Location = new Point(110, 117);
            txtEmail.Size = new Size(390, 23);
            txtEmail.Text = duLieuHienTai.Email;

            lblGhiChu.Text = "Ghi chú";
            lblGhiChu.Location = new Point(12, 155);
            lblGhiChu.AutoSize = true;
            txtGhiChu.Location = new Point(110, 152);
            txtGhiChu.Size = new Size(390, 100);
            txtGhiChu.Multiline = true;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;
            txtGhiChu.Text = duLieuHienTai.GhiChu;

            btnLuu.Text = "Lưu";
            btnLuu.Location = new Point(344, 268);
            btnLuu.Size = new Size(75, 30);
            btnLuu.DialogResult = DialogResult.OK;

            btnHuy.Text = "Hủy";
            btnHuy.Location = new Point(425, 268);
            btnHuy.Size = new Size(75, 30);
            btnHuy.DialogResult = DialogResult.Cancel;

            formNhap.Controls.Add(lblUid);
            formNhap.Controls.Add(txtUid);
            formNhap.Controls.Add(lblPassword);
            formNhap.Controls.Add(txtPassword);
            formNhap.Controls.Add(lblTen);
            formNhap.Controls.Add(txtTen);
            formNhap.Controls.Add(lblEmail);
            formNhap.Controls.Add(txtEmail);
            formNhap.Controls.Add(lblGhiChu);
            formNhap.Controls.Add(txtGhiChu);
            formNhap.Controls.Add(btnLuu);
            formNhap.Controls.Add(btnHuy);
            formNhap.AcceptButton = btnLuu;
            formNhap.CancelButton = btnHuy;

            if (formNhap.ShowDialog(this) != DialogResult.OK)
            {
                return null;
            }

            return new DuLieuCapNhatDong
            {
                Uid = txtUid.Text,
                Password = txtPassword.Text,
                Ten = txtTen.Text,
                Email = txtEmail.Text,
                GhiChu = txtGhiChu.Text
            };
        }

        private string LayUserAgentFacebookThuongDangChon()
        {
            string userAgentDangChon = cboUserAgent.SelectedItem?.ToString()?.Trim() ?? string.Empty; // Lấy User-Agent đang chọn trên combobox để dùng cho facebook.com, nếu có
            if (!string.IsNullOrWhiteSpace(userAgentDangChon))                                // Nếu có User-Agent nào đang chọn thì dùng luôn để test facebook.com
            {
                return userAgentDangChon;                                                     // Nếu có User-Agent nào đang chọn thì dùng luôn để test facebook.com
            }
            return facebookDesktopUserAgentMacDinh;                                           // Nếu chưa có User-Agent nào được chọn thì dùng UA desktop mặc định để đảm bảo facebook.com luôn có UA hợp lệ để test
        }

        private void MoDsTxtBangNotepadDeNhapThem()
        {
            try
            {
                string? thuMucDs = Path.GetDirectoryName(dsFilePath);
                if (!string.IsNullOrWhiteSpace(thuMucDs))
                {
                    Directory.CreateDirectory(thuMucDs);                                      // Đảm bảo thư mục data luôn tồn tại trước khi mở ds.txt để nhập thêm
                }

                if (!File.Exists(dsFilePath))
                {
                    File.WriteAllText(dsFilePath, string.Empty, Encoding.UTF8);               // Nếu ds.txt chưa có thì tạo sẵn file rỗng để người dùng nhập ngay
                }

                System.Diagnostics.Process? tienTrinhNotepad = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{dsFilePath}\"",
                    UseShellExecute = true
                });

                CapNhatTrangThai("Đã mở ds.txt để nhập thêm danh sách.", Color.SteelBlue);

                if (tienTrinhNotepad == null)
                {
                    return;
                }

                try
                {
                    tienTrinhNotepad.EnableRaisingEvents = true;
                    tienTrinhNotepad.Exited += (_, _) =>
                    {
                        try
                        {
                            if (IsDisposed || !IsHandleCreated)
                            {
                                return;                                                       // Form đã đóng thì không cần cố cập nhật lại UI
                            }

                            BeginInvoke(() =>
                            {
                                CapNhatThongTinSoLuong();                                     // Khi đóng Notepad thì đọc lại ds.txt để label Danh Sách khớp ngay
                                CapNhatTrangThai("Đã đọc lại ds.txt sau khi cập nhật danh sách.", Color.SeaGreen);
                            });
                        }
                        catch
                        {
                            // Đóng form đúng lúc Notepad thoát thì bỏ qua, không ảnh hưởng nghiệp vụ chính.
                        }
                    };
                }
                catch
                {
                    // Không gắn được sự kiện Exited thì vẫn giữ hành vi chính là mở Notepad cho người dùng nhập ds.txt.
                }
            }
            catch (Exception ex)
            {
                CapNhatTrangThai($"Mở ds.txt thất bại: {ex.Message}", Color.Firebrick);
                MessageBox.Show($"Không thể mở ds.txt để nhập thêm danh sách.\r\n{ex.Message}");
            }
        }

        private bool TryLayTaiKhoanMoiTuDs(out string uid, out string password)              // Hàm này sẽ tìm và trả ra một dòng hợp lệ đầu tiên trong ds.txt mà UID chưa có trên grid, nếu không còn dòng nào hợp lệ thì trả về false
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
                MoDsTxtBangNotepadDeNhapThem();                                               // Dùng chung đúng luồng mở ds.txt để người dùng nhập thêm danh sách
                return false;                                                                 // Thoát hàm vì chưa có tài khoản để lấy
            }

            for (int i = 0; i < lines.Length; i++)                                            // Duyệt lần lượt từng dòng trong ds.txt
            {
                string line = lines[i].Trim();                                                // Loại khoảng trắng thừa ở đầu và cuối dòng

                if (string.IsNullOrWhiteSpace(line))                                          // Nếu là dòng trống thì bỏ qua
                {
                    continue;
                }

                if (!TryTachUidVaPasswordTuDongDs(line, out string uidTam, out string passwordTam, out _))
                {
                    MessageBox.Show($"Dòng {i + 1} lỗi.");
                    continue;
                }

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
            BoTickTatCaDong();                                                                 // Next đi tài khoản mới thì phải giải phóng toàn bộ dòng đang tick trước đó
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

            CapNhatMauDongGrid(row);
            dataGridView1.ClearSelection();                                                   // Bỏ toàn bộ lựa chọn cũ để chỉ giữ đúng dòng mới
            row.Selected = true;                                                              // Tự chọn ngay dòng vừa được thêm
            dataGridView1.CurrentCell = row.Cells["colUID"];                                  // Đưa con trỏ hiện tại về cột UID của dòng mới
            LuuDuLieuGridRaFile();
        }
        //
        //   HÀM XỬ LÝ SỰ KIỆN NÚT TIẾP TỤC
        //
        private void btnTiepTuc_Click(object sender, EventArgs e)
        {
            XoaHangChoTaiKhoanDaTick();                                                        // Người dùng chuyển sang Next thủ công thì hủy batch tick cũ để app quay lại đúng luồng ds.txt
            DatLaiYeuCauDungThuCong();                                                         // Next do người dùng bấm là tín hiệu tiếp tục chạy lại sau Stop
            XuLyNutNext();                                                                     // Nút btnTiepTuc hiện tại đang đóng vai trò Next
        }

        private void mởChromeMẫuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoChromeMau();                                                                     // Menu "Mở Chrome mẫu" chỉ gọi đúng hàm mở profile_mau
        }

        private void nhậpDanhSáchToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            MoDsTxtBangNotepadDeNhapThem();                                                   // Menu chuột phải Nhập/Cập nhật danh sách dùng lại đúng luồng mở ds.txt sẵn có
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            BatDauXuLyDanhSachDongDaTick(CheDoDieuPhoiPhien.DangNhapDanhSachDaTick);          // Nút Đăng nhập sẽ xử lý lần lượt các dòng đã tick trên grid
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
            BatDauXuLyDanhSachDongDaTick(CheDoDieuPhoiPhien.MoChromeDanhSachDaTickKhongDong); // Menu Mở Chrome cũng đi lần lượt theo các dòng đã tick nhưng giữ Chrome mở
        }

        private void làmMớiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CapNhatTrangThai("Đang kiểm tra cập nhật từ GitHub...", Color.DarkGoldenrod);     // Bấm Làm mới sẽ chủ động kiểm tra manifest online để người dùng nhìn rõ app không bị chạy offline
            Refresh();

            UpdateService.UpdateCheckResult ketQuaKiemTra = UpdateService.KiemTraCapNhat();
            if (!ketQuaKiemTra.CoBanMoi)
            {
                CapNhatTrangThai($"Đang ở bản mới nhất {ketQuaKiemTra.VersionHienTai} ({ketQuaKiemTra.NguonManifest}).", Color.RoyalBlue);
                MessageBox.Show(
                    $"Phiên bản hiện tại: {ketQuaKiemTra.VersionHienTai}{Environment.NewLine}" +
                    $"Phiên bản mới nhất: {ketQuaKiemTra.VersionMoiNhat}{Environment.NewLine}" +
                    $"Nguồn manifest: {ketQuaKiemTra.NguonManifest}{Environment.NewLine}{Environment.NewLine}" +
                    "Hiện chưa có bản mới hơn để cập nhật.",
                    "Kiểm tra cập nhật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!ketQuaKiemTra.CoTheCapNhat)
            {
                CapNhatTrangThai($"Đã thấy bản mới {ketQuaKiemTra.VersionMoiNhat} nhưng chưa đủ điều kiện cập nhật.", Color.Firebrick);
                MessageBox.Show(
                    $"Đã thấy bản mới {ketQuaKiemTra.VersionMoiNhat} từ {ketQuaKiemTra.NguonManifest}, nhưng app chưa đủ điều kiện chạy updater.{Environment.NewLine}" +
                    "Hãy kiểm tra lại Updater.exe hoặc gói cập nhật.",
                    "Kiểm tra cập nhật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            CapNhatTrangThai($"Đã thấy bản mới {ketQuaKiemTra.VersionMoiNhat} từ {ketQuaKiemTra.NguonManifest}.", Color.ForestGreen);
            DialogResult xacNhan = MessageBox.Show(
                $"Phiên bản hiện tại: {ketQuaKiemTra.VersionHienTai}{Environment.NewLine}" +
                $"Phiên bản mới nhất: {ketQuaKiemTra.VersionMoiNhat}{Environment.NewLine}" +
                $"Nguồn manifest: {ketQuaKiemTra.NguonManifest}{Environment.NewLine}{Environment.NewLine}" +
                "Bạn có muốn cập nhật ngay không?",
                "Kiểm tra cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
            {
                CapNhatTrangThai("Đã hủy cập nhật thủ công.", Color.RoyalBlue);
                return;
            }

            CapNhatTrangThai($"Đang khởi chạy updater để lên {ketQuaKiemTra.VersionMoiNhat}...", Color.DarkGoldenrod);
            if (!UpdateService.ThuKhoiDongUpdater(ketQuaKiemTra))
            {
                CapNhatTrangThai("Không thể khởi chạy updater.", Color.Firebrick);
                MessageBox.Show("Không thể khởi chạy updater để cập nhật bản mới.");
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            XoaCacDongDaTick();                                                                // Nút Xóa trên form dùng chung một luồng xóa theo các dòng đang tick
        }

        private void xóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XoaCacDongDaTick();                                                                // Menu chuột phải giờ chỉ còn một nhánh Xóa chung
        }

        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                row.Cells["colChon"].Value = true;                                             // Chọn tất cả nghĩa là tick toàn bộ dòng trên grid
            }

            dataGridView1.ClearSelection();                                                    // Bỏ bôi đen vì checkbox mới là trạng thái chọn thật
        }

        private void cácDòngBôiĐenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> dsDongBoiDen = LayDanhSachDongBoiDen();
            if (dsDongBoiDen.Count == 0)
            {
                MessageBox.Show("Hãy bôi đen ít nhất 1 dòng để chuyển sang trạng thái chọn.");
                return;
            }

            BoTickTatCaDong();                                                                 // Bôi đen chỉ là bước trung gian, khi áp dụng thì đưa hệ thống về đúng trạng thái tick thật
            foreach (DataGridViewRow row in dsDongBoiDen)
            {
                row.Cells["colChon"].Value = true;
            }

            dataGridView1.ClearSelection();
        }

        private void bỏChọnTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BoTickTatCaDong();                                                                 // Bỏ chọn tất cả là đưa mọi checkbox về false
            dataGridView1.ClearSelection();
        }

        private void dòngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(TaoNoiDungDayDuCuaDong);
        }

        private void cácDòngBôiĐenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(TaoNoiDungDayDuCuaDong);                                  // Menu copy này cũng bám theo các dòng đã tick để giữ đúng quy ước chọn thật
        }

        private void uIDToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colUID"));
        }

        private void tênToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colTen"));
        }

        private void passToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colPass"));
        }

        private void emailToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colEmail"));
        }

        private void cookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colCookie"));
        }

        private void ghiChúToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDuLieuTheoDongDaTick(row => LayGiaTriCell(row, "colGhiChu"));
        }
        // 
        //  HÀM TẠO PRORILE RẢNH NẾU CHƯA CÓ KHI NÚT CHỌN DC GỌI
        //
        private bool TaoSessionTuProfileMau(string uid, out SessionModel session)
        {
            session = new SessionModel();

            if (!Directory.Exists(profileMauPath))
            {
                MessageBox.Show("Không tìm thấy thư mục profile_mau.");
                return false;
            }

            try
            {
                session = SessionRuntimeService.CreateSessionFromTemplate(uid);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tạo phiên tạm mới.{Environment.NewLine}{ex.Message}");
                return false;
            }
        }
        //
        //  HÀM XỬ LÝ KHI CHỌN NÚT TIẾP TỤC
        //
        private void XuLyNutNext()
        {
            XoaHangChoTaiKhoanDaTick();                                                        // Next lấy tài khoản mới từ ds.txt nên phải tắt mọi batch đang chờ trên grid
            if (CoYeuCauDungThuCong())
            {
                CapNhatTrangThai("Đã dừng thủ công. App sẽ không mở phiên mới cho đến khi bạn bấm Next lại.", Color.RoyalBlue);
                return;                                                                        // Stop phải chặn cả luồng Next nếu nó chưa kịp mở Chrome mới
            }

            CapNhatTrangThai("Đang lấy tài khoản mới từ ds.txt...", Color.DarkGoldenrod);    // Đây là bước đầu của luồng Next nên cần báo rõ để người dùng biết app chưa bị đứng
            if (!TryLayTaiKhoanMoiTuDs(out string uid, out string password))                  // Nếu chưa lấy được tài khoản mới từ ds.txt thì dừng tại đây
            {
                CapNhatTrangThai("Không lấy được tài khoản mới từ ds.txt.", Color.Firebrick);
                return;
            }

            CapNhatTrangThai($"Đang tạo phiên tạm mới cho UID {uid}...", Color.DarkGoldenrod);
            if (!TaoSessionTuProfileMau(uid, out SessionModel session))
            {
                CapNhatTrangThai("Không tạo được phiên tạm từ profile_mau.", Color.Firebrick);
                return;
            }

            if (CoYeuCauDungThuCong())
            {
                SessionRuntimeService.TryCloseAndCleanupSession(session.SessionId);
                CapNhatTrangThaiDongTheoUid(uid, "Đã dừng thủ công");
                CapNhatTrangThai("Đã dừng thủ công trước khi mở phiên mới.", Color.RoyalBlue);
                return;
            }

            CapNhatTrangThai($"Đang thêm dòng mới {uid} lên grid...", Color.DarkGoldenrod);
            ThemDongMoiLenGrid(uid, password);
            CapNhatTrangThai($"Đang mở phiên mới cho UID {uid}...", Color.DarkGoldenrod);
            MoChromeTheoSession(session, uid, password, CheDoDieuPhoiPhien.TuDongLayMoiTuDs);
        }
        //
        //   HÀM MỞ CHROME MẪU
        //
        private void MoChromeMau()
        {
            CapNhatTrangThai("Đang mở Chrome mẫu...", Color.DarkGoldenrod);                   // Báo trạng thái riêng cho profile mẫu để người dùng biết app vẫn đang chạy bình thường
            Directory.CreateDirectory(profileMauPath);                                         // Nếu chưa có profile_mau thì tạo mới để luôn có nơi chuẩn bị profile gốc

            string chromeExe = TimChromeExe();                                                 // Tìm đường dẫn chrome.exe từ các vị trí cài đặt thông dụng
            if (string.IsNullOrWhiteSpace(chromeExe))
            {
                CapNhatTrangThai("Mở Chrome mẫu thất bại: không tìm thấy chrome.exe.", Color.Firebrick);
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
            CapNhatTrangThai("Đã mở Chrome mẫu.", Color.ForestGreen);
        }
        //
        //  HÀM MỞ CHROM THEO DÒNG
        //
        private void BatDauXuLyDanhSachDongDaTick(CheDoDieuPhoiPhien cheDo)
        {
            DatLaiYeuCauDungThuCong();                                                         // Mọi batch mới đều phải vô hiệu trạng thái Stop cũ trước khi chạy
            XoaHangChoTaiKhoanDaTick();
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy danh sách dòng được chọn thật theo checkbox, không dùng bôi đen
            if (dsDongDaTick.Count == 0)
            {
                CapNhatTrangThai("Chưa tick dòng nào để xử lý.", Color.Firebrick);
                MessageBox.Show("Vui lòng tick ít nhất 1 dòng để xử lý.");
                return;
            }

            List<TaiKhoanDuocChon> danhSachTaiKhoan = new();
            foreach (DataGridViewRow row in dsDongDaTick)
            {
                string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                string password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(uid))
                {
                    continue;                                                                   // Bỏ qua các dòng không có UID để batch vẫn chạy được cho các dòng hợp lệ còn lại
                }

                danhSachTaiKhoan.Add(new TaiKhoanDuocChon
                {
                    Uid = uid,
                    Password = password
                });
            }

            if (danhSachTaiKhoan.Count == 0)
            {
                CapNhatTrangThai("Không có dòng hợp lệ để xử lý.", Color.Firebrick);
                MessageBox.Show("Các dòng đã tick không có UID hợp lệ.");
                return;
            }

            NapHangChoTaiKhoanDaTick(danhSachTaiKhoan, cheDo);
            string moTaCheDo = cheDo == CheDoDieuPhoiPhien.DangNhapDanhSachDaTick
                ? "đăng nhập"
                : "mở Chrome";
            CapNhatTrangThai($"Đang chuẩn bị {moTaCheDo} {danhSachTaiKhoan.Count} dòng đã chọn...", Color.DarkGoldenrod);
            XuLyTaiKhoanDaTickTiepTheo();
        }

        private void XuLyTaiKhoanDaTickTiepTheo()
        {
            if (CoYeuCauDungThuCong())
            {
                CapNhatTrangThai("Đã dừng thủ công. Batch các dòng đã chọn sẽ không chạy tiếp.", Color.RoyalBlue);
                return;
            }

            if (!TryLayTaiKhoanDaTickTiepTheo(out TaiKhoanDuocChon taiKhoan, out CheDoDieuPhoiPhien cheDo))
            {
                CapNhatTrangThai("Đã xử lý xong các dòng đã tick.", Color.ForestGreen);
                return;
            }

            string uid = taiKhoan.Uid.Trim();
            string password = taiKhoan.Password;
            if (string.IsNullOrWhiteSpace(uid))
            {
                XuLyTaiKhoanDaTickTiepTheo();                                                  // Nếu queue còn sót mục rỗng thì bỏ qua để tới ngay dòng hợp lệ tiếp theo
                return;
            }

            DataGridViewRow? rowDangXuLy = TimDongTheoUid(uid);
            if (rowDangXuLy != null)
            {
                dataGridView1.ClearSelection();
                rowDangXuLy.Selected = true;
                rowDangXuLy.Cells["colChon"].Value = true;
                dataGridView1.CurrentCell = rowDangXuLy.Cells["colUID"];
            }

            if (!TaoSessionTuProfileMau(uid, out SessionModel session))
            {
                CapNhatTrangThaiDongTheoUid(uid, "Lỗi tạo phiên");
                DanhDauDongDaXuLyTrongBatch(uid, cheDo);                                      // Không tạo được phiên thì coi như dòng này đã xử lý xong và bỏ tick để batch đi tiếp
                CapNhatTrangThai($"Bỏ qua UID {uid}: không tạo được phiên tạm.", Color.Firebrick);
                DieuPhoiBuocTiepTheoSauKetQua(cheDo);
                return;
            }

            string moTaCheDo = cheDo == CheDoDieuPhoiPhien.DangNhapDanhSachDaTick
                ? "đăng nhập"
                : "mở Chrome";
            CapNhatTrangThai($"Đang {moTaCheDo} cho UID {uid}...", Color.DarkGoldenrod);
            MoChromeTheoSession(session, uid, password, cheDo);
        }
        //
        //  HÀM MỞ CHROME THEO PROFILE
        //
        private void MoChromeTheoSession(SessionModel session, string uid, string password, CheDoDieuPhoiPhien cheDo)
        {
            if (CoYeuCauDungThuCong())
            {
                SessionRuntimeService.TryCloseAndCleanupSession(session.SessionId);
                CapNhatTrangThaiDongTheoUid(uid, "Đã dừng thủ công");
                CapNhatTrangThai("Đã dừng thủ công trước khi khởi chạy Chrome.", Color.RoyalBlue);
                return;
            }

            CapNhatTrangThai($"Đang khởi chạy Chrome cho UID {uid}...", Color.DarkGoldenrod);
            CapNhatTrangThaiDongTheoUid(uid, "Đang mở Chrome phiên mới");
            string chromeExe = TimChromeExe();
            if (string.IsNullOrWhiteSpace(chromeExe))
            {
                CapNhatTrangThai("Mở Chrome thất bại: không tìm thấy chrome.exe.", Color.Firebrick);
                MessageBox.Show("Không tìm thấy chrome.exe.");
                SessionRuntimeService.TryCloseAndCleanupSession(session.SessionId);
                return;
            }

            string urlCanMo = LayUrlFacebookDaChon();
            string userAgentDangDung = LayGiaTriUserAgentTheoGiaoDien(urlCanMo);
            string thamSoUserAgent = LayThamSoUserAgentTheoGiaoDien(urlCanMo);
            int congDebugChrome = LayCongDebugChromeTrong();

            Rectangle vungLamViec = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1200, 900);
            int chieuRongCuaSo = Math.Min(vungLamViec.Width, Math.Max(1000, (int)(vungLamViec.Width * 0.75))); // Mở lớn hơn 2/3 màn hình để form Meta không bị bó quá hẹp
            int chieuCaoCuaSo = Math.Min(vungLamViec.Height, Math.Max(760, (int)(vungLamViec.Height * 0.88)));
            int viTriX = vungLamViec.Left;                                                    // Neo sát mép trái màn hình để chừa khoảng trống bên phải cho người dùng theo dõi app
            int viTriY = Math.Max(0, (vungLamViec.Height - chieuCaoCuaSo) / 2);

            string thamSoChanPopupChrome = "--disable-notifications --disable-save-password-bubble --disable-session-crashed-bubble --disable-features=PasswordManagerOnboarding,Translate";
            string thamSoKichThuocCuaSo = $"--window-size={chieuRongCuaSo},{chieuCaoCuaSo} --window-position={viTriX},{viTriY}";
            string arguments = $"--new-window {thamSoKichThuocCuaSo} --remote-debugging-port={congDebugChrome} --user-data-dir=\"{session.SessionPath}\" {thamSoUserAgent} {thamSoChanPopupChrome} {urlCanMo}".Trim();

            try
            {
                GhiNhanPhienDangXuLy(session, uid);
                GhiLaiUserAgentDangDung(urlCanMo, userAgentDangDung);
                SessionRuntimeService.LaunchChromeForSession(session, chromeExe, arguments);
                congDebugTheoUid[uid] = congDebugChrome;
                CapNhatTrangThai($"Đang tự điền UID/Password cho {uid}...", Color.DarkGoldenrod);
                CapNhatTrangThaiDongTheoUid(uid, "Đang tự điền UID/Password");
                _ = TuDongDienThongTinDangNhapAsync(congDebugChrome, urlCanMo, uid, password, cheDo);
            }
            catch (Exception ex)
            {
                XoaThongTinPhienDangXuLy(uid, session.SessionId);
                SessionRuntimeService.TryCloseAndCleanupSession(session.SessionId);
                CapNhatTrangThai($"Mở Chrome thất bại cho UID {uid}.", Color.Firebrick);
                CapNhatTrangThaiDongTheoUid(uid, "Lỗi mở Chrome");
                MessageBox.Show($"Không thể mở Chrome cho UID {uid}.{Environment.NewLine}{ex.Message}");
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

            return LayUserAgentFacebookThuongDangChon();                                       // Facebook thường phải lấy đúng UA đang chọn trên combobox thì mới đúng ý đồ test của app
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
        private async Task TuDongDienThongTinDangNhapAsync(int congDebugChrome, string urlCanMo, string uid, string password, CheDoDieuPhoiPhien cheDo)
        {
            if (CoYeuCauDungThuCong())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
            {
                CapNhatKetQuaTheoCheDo(uid, "Thiếu UID/Password", $"Bỏ qua tự điền cho {uid}: thiếu UID hoặc Password.", Color.Firebrick, cheDo);
                return;                                                                        // Nếu thiếu UID hoặc Password thì không tự điền được, tránh đổ nhầm dữ liệu rỗng lên form đăng nhập
            }

            string script = TaoScriptTuDongDienDangNhap(uid, password);                        // Tạo script JavaScript để tìm ô email/password và đổ giá trị vào đúng cách của trình duyệt
            int soLanThuToiDa = urlCanMo.Contains("meta", StringComparison.OrdinalIgnoreCase) ? 40 : 25; // Giao diện meta thường render chậm hơn nên cho phép thử nhiều lần hơn

            for (int i = 0; i < soLanThuToiDa; i++)
            {
                if (CoYeuCauDungThuCong())
                {
                    return;
                }

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
                        CapNhatTrangThai($"Đã tự điền và gửi đăng nhập cho {uid}.", Color.ForestGreen);
                        CapNhatTrangThaiDongTheoUid(uid, "Đã gửi đăng nhập, đang chờ kết quả");
                        _ = TheoDoiKetQuaDangNhapSauKhiSubmitAsync(congDebugChrome, uid, password, urlCanMo, cheDo);
                        return;
                    }
                }
                catch
                {
                }

                await Task.Delay(1000);                                                        // Chờ trang tải thêm rồi thử lại vì Facebook có thể render form chậm
            }

            if (CoYeuCauDungThuCong())
            {
                return;
            }

            CapNhatKetQuaTheoCheDo(uid, "Không tìm thấy ô đăng nhập", $"Không tìm thấy ô đăng nhập để tự điền cho {uid}.", Color.Firebrick, cheDo);
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
        //  HÀM THEO DÕI KẾT QUẢ ĐĂNG NHẬP SAU KHI APP ĐÃ TỰ GỬI ĐĂNG NHẬP
        //
        private async Task TheoDoiKetQuaDangNhapSauKhiSubmitAsync(int congDebugChrome, string uid, string password, string urlCanMo, CheDoDieuPhoiPhien cheDo)
        {
            const int soLanTheoDoiToiDa = 180;                                                 // Chờ tối đa 3 phút để Facebook phản hồi sau khi app đã tự gửi đăng nhập
            bool daThuLaiLanHai = false;
            string scriptTuDongDienVaSubmit = TaoScriptTuDongDienDangNhap(uid, password);
            string chiTietCuoi = string.Empty;
            string urlCuoi = string.Empty;
            bool laFacebookThuong = urlCanMo.Equals("https://facebook.com/", StringComparison.OrdinalIgnoreCase);

            for (int i = 0; i < soLanTheoDoiToiDa; i++)
            {
                if (CoYeuCauDungThuCong())
                {
                    return;
                }

                try
                {
                    string? webSocketDebuggerUrl = await LayWebSocketDebuggerUrlFacebookDangMoAsync(congDebugChrome);
                    if (string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    (string trangThai, string urlHienTai, string lyDo, string chiTiet) = await LayTrangThaiDangNhapSauSubmitAsync(webSocketDebuggerUrl);
                    urlCuoi = urlHienTai;
                    chiTietCuoi = chiTiet;
                    if (string.Equals(trangThai, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        CapNhatKetQuaTheoCheDo(uid, "Đăng nhập thành công", $"UID {uid} đã đăng nhập thành công.", Color.ForestGreen, cheDo);
                        return;
                    }

                    if (string.Equals(trangThai, "checkpoint", StringComparison.OrdinalIgnoreCase))
                    {
                        string dienGiaiCheckpoint = lyDo switch
                        {
                            "two_factor" => "cần nhập mã 2FA",
                            "verify_identity" => "cần xác minh danh tính",
                            "checkpoint" => "cần xác minh checkpoint",
                            _ => "cần xác minh checkpoint/2FA"
                        };

                        CapNhatKetQuaTheoCheDo(uid, $"Dừng: {dienGiaiCheckpoint}", $"UID {uid} {dienGiaiCheckpoint}.", Color.DarkOrange, cheDo);
                        return;
                    }

                    if (string.Equals(trangThai, "blocked", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(lyDo, "password_changed", StringComparison.OrdinalIgnoreCase))
                        {
                            XoaUidMatKhauDaThayDoiVaChayTiep(uid, cheDo);
                            return;
                        }

                        if (string.Equals(lyDo, "captcha", StringComparison.OrdinalIgnoreCase))
                        {
                            DanhDauUidCanCaptchaVaChayTiep(uid, cheDo);
                            return;
                        }

                        string dienGiaiLyDo = lyDo switch
                        {
                            "password_changed" => "mật khẩu đã thay đổi",
                            "captcha" => "cần nhập captcha",
                            "956" => "mã 956",
                            "login_not_allowed" => "không cho phép đăng nhập",
                            "wrong_password" => "sai mật khẩu",
                            "account_locked" => "tài khoản bị khóa/tạm khóa",
                            "account_disabled" => "tài khoản đã bị vô hiệu hóa",
                            "suspicious_activity" => "hoạt động bất thường, cần xác minh",
                            "rate_limited" => "bị giới hạn tần suất, thử lại sau",
                            "no_account" => "không tìm thấy tài khoản",
                            _ => "lỗi chặn đăng nhập"
                        };

                        CapNhatKetQuaTheoCheDo(uid, $"{dienGiaiLyDo}", $"UID {uid} dừng xử lý vì {dienGiaiLyDo}.", Color.Firebrick, cheDo);
                        return;
                    }

                    if (string.Equals(trangThai, "pending", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(lyDo, "login_identify", StringComparison.OrdinalIgnoreCase) &&
                        laFacebookThuong &&
                        daThuLaiLanHai)
                    {
                        CapNhatKetQuaTheoCheDo(uid, "Sai tài khoản/mật khẩu", $"UID {uid} vẫn rơi vào trang tìm tài khoản sau 2 lần đăng nhập.  Sai tài khoản/mật khẩu.", Color.Firebrick, cheDo);
                        return;
                    }

                    if (string.Equals(trangThai, "pending", StringComparison.OrdinalIgnoreCase) && !daThuLaiLanHai && i >= 8)
                    {
                        bool daGuiLanHai = await ThuChayScriptTuDongDienAsync(webSocketDebuggerUrl, scriptTuDongDienVaSubmit);
                        daThuLaiLanHai = true;

                        if (daGuiLanHai)
                        {
                            CapNhatTrangThaiDongTheoUid(uid, "Đã thử lại lần 2, chờ kết quả");
                            CapNhatTrangThai($"UID {uid}: đã tự điền + gửi đăng nhập lần 2.", Color.DarkGoldenrod);
                        }
                        else
                        {
                            CapNhatTrangThaiDongTheoUid(uid, "Lần 2 không thấy form đăng nhập");
                            CapNhatTrangThai($"UID {uid}: thử lần 2 nhưng không tìm thấy form đăng nhập.", Color.Firebrick);
                        }
                    }
                }
                catch
                {
                }

                await Task.Delay(1000);
            }

            if (CoYeuCauDungThuCong())
            {
                return;
            }

            string thongTinBoSung = string.IsNullOrWhiteSpace(chiTietCuoi)
                ? urlCuoi
                : chiTietCuoi;
            if (!string.IsNullOrWhiteSpace(thongTinBoSung) && thongTinBoSung.Length > 140)
            {
                thongTinBoSung = thongTinBoSung[..140] + "...";
            }

            string noiDungTimeout = string.IsNullOrWhiteSpace(thongTinBoSung)
                ? $"UID {uid} chưa có kết quả sau khi chờ 3 phút."
                : $"UID {uid} chưa có kết quả sau khi chờ 3 phút. Dấu hiệu cuối: {thongTinBoSung}";
            CapNhatKetQuaTheoCheDo(uid, "Hết thời gian chờ xác nhận", noiDungTimeout, Color.Firebrick, cheDo);
        }

        private async Task<(string TrangThai, string UrlHienTai, string LyDo, string ChiTiet)> LayTrangThaiDangNhapSauSubmitAsync(string webSocketDebuggerUrl)
        {
            using var webSocket = new System.Net.WebSockets.ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(webSocketDebuggerUrl), CancellationToken.None);

            using JsonDocument ketQua = await GuiLenhCdpAsync(webSocket, "Runtime.evaluate", new
            {
                expression = TaoScriptKiemTraTrangThaiDangNhapSauSubmit(),
                returnByValue = true
            });

            if (!ketQua.RootElement.TryGetProperty("result", out JsonElement resultElement))
            {
                return ("unknown", string.Empty, string.Empty, string.Empty);
            }

            if (!resultElement.TryGetProperty("result", out JsonElement evaluateResult))
            {
                return ("unknown", string.Empty, string.Empty, string.Empty);
            }

            if (!evaluateResult.TryGetProperty("value", out JsonElement valueElement) || valueElement.ValueKind != JsonValueKind.Object)
            {
                return ("unknown", string.Empty, string.Empty, string.Empty);
            }

            string trangThai = valueElement.TryGetProperty("state", out JsonElement stateElement) ? stateElement.GetString() ?? "unknown" : "unknown";
            string urlHienTai = valueElement.TryGetProperty("href", out JsonElement hrefElement) ? hrefElement.GetString() ?? string.Empty : string.Empty;
            string lyDo = valueElement.TryGetProperty("reason", out JsonElement reasonElement) ? reasonElement.GetString() ?? string.Empty : string.Empty;
            string chiTiet = valueElement.TryGetProperty("detail", out JsonElement detailElement) ? detailElement.GetString() ?? string.Empty : string.Empty;
            return (trangThai, urlHienTai, lyDo, chiTiet);
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
  const firstVisibleInRoot = (root, selectors) => {
    for (const selector of selectors) {
      const element = root.querySelector(selector);
      if (isVisible(element)) return element;
    }
    return null;
  };

  const normalizeText = (value) => (value || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');

  const laHopDangNhap = (container) => {
    if (!container || !(container instanceof Element)) return false;

    const coInputMatKhau = !!container.querySelector('input[type="password"], input[name="pass"], input[name="password"]');
    if (!coInputMatKhau) return false;

    const coInputTaiKhoan = !!container.querySelector('input[name="email"], input[id="email"], input[type="email"], input[autocomplete="username"], input[inputmode="email"], input[type="text"]');
    const coNutDangNhap = !!container.querySelector('button[name="login"], input[name="login"], button[type="submit"], input[type="submit"], [aria-label*="Đăng nhập"], [aria-label*="Log in"]');
    return coInputTaiKhoan || coNutDangNhap;
  };

  const namTrongHopDangNhap = (node) => {
    let current = node;
    for (let i = 0; current && i < 8; i++) {
      if (laHopDangNhap(current)) {
        return true;
      }

      current = current.parentElement;
    }

    return false;
  };

  const clickByKeywords = (root, keywords) => {
    const nodes = Array.from(root.querySelectorAll('button, [role="button"], a, div[role="button"]'));
    for (const node of nodes) {
      if (!isVisible(node)) continue;
      if (namTrongHopDangNhap(node)) continue;
      const text = normalizeText((node.innerText || node.textContent || node.getAttribute('aria-label') || '').trim());
      if (!text) continue;
      if (keywords.some((keyword) => text.includes(keyword))) {
        node.click();
        return true;
      }
    }

    return false;
  };

  const dongPopupCanTro = (root) => {
    let daXuLy = false;
    const nutDong = [
      '[aria-label="Close"]',
      '[aria-label="Đóng"]',
      'button[title="Close"]',
      '[data-testid="cookie-policy-banner-close-button"]'
    ];

    for (const selector of nutDong) {
      const node = root.querySelector(selector);
      if (isVisible(node)) {
        if (namTrongHopDangNhap(node)) {
          continue;                                                                   // Không được đóng popup chứa form đăng nhập (Meta/Facebook modal login)
        }

        node.click();
        daXuLy = true;
      }
    }

    const keywordDong = [
      'not now',
      'close',
      'dong',
      'bo qua',
      'de sau',
      'khong luc nay',
      'chua bay gio',
      'allow all cookies',
      'accept all',
      'allow essential',
      'chi cho phep cookie thiet yeu',
      'cho phep tat ca cookie'
    ];

    return clickByKeywords(root, keywordDong) || daXuLy;
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

    const loginButton = queryVisible([
      'button[name="login"]',
      'button[id="loginbutton"]',
      '#loginbutton',
      'input[name="login"]',
      'button[type="submit"]',
      'input[type="submit"]',
      '[data-testid*="royal_login_button"]',
      '[aria-label*="Đăng nhập"]',
      '[aria-label*="Log in"]'
    ]);

    return { emailInput, passwordInput, loginButton, root };
  };

  let emailInput = null;
  let passwordInput = null;
  let loginButtonTheoRoot = null;
  let rootChuaForm = document;

  for (const root of contexts) {
    const found = findInputsInContext(root);
    if (found.emailInput && found.passwordInput) {
      emailInput = found.emailInput;
      passwordInput = found.passwordInput;
      loginButtonTheoRoot = found.loginButton;
      rootChuaForm = found.root || document;
      break;
    }
  }

  if (!emailInput || !passwordInput) {
    return 'wait';
  }

  dongPopupCanTro(document);
  if (rootChuaForm && rootChuaForm !== document) {
    dongPopupCanTro(rootChuaForm);
  }

  const submitLogin = () => {
    const loginButton = (loginButtonTheoRoot && isVisible(loginButtonTheoRoot))
      ? loginButtonTheoRoot
      : firstVisibleInRoot(rootChuaForm || document, [
      'button[name="login"]',
      'button[id="loginbutton"]',
      '#loginbutton',
      'input[name="login"]',
      'button[type="submit"]',
      'input[type="submit"]',
      '[data-testid*="royal_login_button"]',
      '[aria-label*="Đăng nhập"]',
      '[aria-label*="Log in"]'
    ]) || firstVisibleInRoot(document, [
      'button[name="login"]',
      'button[id="loginbutton"]',
      '#loginbutton',
      'input[name="login"]',
      'button[type="submit"]',
      'input[type="submit"]',
      '[data-testid*="royal_login_button"]',
      '[aria-label*="Đăng nhập"]',
      '[aria-label*="Log in"]'
    ]);

    if (loginButton && !loginButton.disabled) {
      loginButton.scrollIntoView({ block: 'center', inline: 'center' });
      loginButton.click();
      return true;
    }

    const enterEvent = {
      key: 'Enter',
      code: 'Enter',
      keyCode: 13,
      which: 13,
      bubbles: true
    };

    passwordInput.focus();
    passwordInput.dispatchEvent(new KeyboardEvent('keydown', enterEvent));
    passwordInput.dispatchEvent(new KeyboardEvent('keypress', enterEvent));
    passwordInput.dispatchEvent(new KeyboardEvent('keyup', enterEvent));

    const form = passwordInput.form || emailInput.form;
    if (form && typeof form.requestSubmit === 'function') {
      form.requestSubmit();
      return true;
    }

    if (form) {
      form.submit();
      return true;
    }

    return true;
  };

  setNativeValue(emailInput, uid);
  setNativeValue(passwordInput, password);
  dongPopupCanTro(document);
  if (rootChuaForm && rootChuaForm !== document) {
    dongPopupCanTro(rootChuaForm);
  }
  submitLogin();
  return 'ok';
})();
""";
        }

        private string TaoScriptKiemTraTrangThaiDangNhapSauSubmit()
        {
            return """
(() => {
  const normalizeText = (value) => (value || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');

  const documents = [document];
  for (const frame of Array.from(document.querySelectorAll('iframe'))) {
    try {
      if (frame.contentDocument) {
        documents.push(frame.contentDocument);
      }
    } catch {
    }
  }

  const hrefs = documents.map((doc) => {
    try {
      return String(doc.location?.href || '');
    } catch {
      return '';
    }
  }).filter(Boolean);

  const titles = documents.map((doc) => {
    try {
      return String(doc.title || '');
    } catch {
      return '';
    }
  }).filter(Boolean);

  const rawTexts = documents.map((doc) => {
    try {
      return [
        String(doc.title || ''),
        String(doc.body?.innerText || ''),
        String(doc.documentElement?.innerText || '')
      ].join('\n');
    } catch {
      return '';
    }
  }).filter(Boolean);

  const href = hrefs[0] || String(window.location.href || '');
  const allLowerHrefs = hrefs.join('\n').toLowerCase();
  const compactRawText = rawTexts.join('\n').replace(/\s+/g, ' ').trim();
  const normalizedBodyText = normalizeText(compactRawText);
  const titleText = titles.join(' | ').replace(/\s+/g, ' ').trim();

  const includesAny = (needles) => needles.some((needle) => normalizedBodyText.includes(needle) || allLowerHrefs.includes(needle));
  const firstMatch = (needles) => needles.find((needle) => normalizedBodyText.includes(needle) || allLowerHrefs.includes(needle)) || '';
  const makeResult = (state, reason = '', detail = '') => ({
    state,
    href,
    reason,
    detail: (detail || titleText || compactRawText || href).replace(/\s+/g, ' ').trim().slice(0, 220)
  });

  const hasPasswordInput = documents.some((doc) => {
    try {
      return !!doc.querySelector('input[type="password"], input[name="pass"], input[name="password"]');
    } catch {
      return false;
    }
  });

  const hasLoginButton = documents.some((doc) => {
    try {
      return !!doc.querySelector('button[name="login"], input[name="login"], button[type="submit"], input[type="submit"]');
    } catch {
      return false;
    }
  });

  const actionHints = documents.flatMap((doc) => {
    try {
      return Array.from(doc.querySelectorAll('a[href], a[role="link"], a[role="button"], button, [role="button"], [role="link"], form[action]')).map((node) => ({
        text: normalizeText(String(node.innerText || node.textContent || node.getAttribute('aria-label') || node.getAttribute('value') || '')),
        href: normalizeText(String(node.getAttribute('href') || node.getAttribute('action') || ''))
      }));
    } catch {
      return [];
    }
  });

  const passwordChangedNeedles = [
    'mat khau cua ban da bi thay doi',
    'mat khau cua ban da duoc thay doi',
    'mat khau cua ban da thay doi',
    'ban da thay doi mat khau',
    'do mat khau cua ban da thay doi',
    'mat khau cua ban vua duoc thay doi',
    'ban da nhap mat khau cu',
    'mat khau ban vua nhap la mat khau cu',
    'mat khau cu',
    'your password was changed',
    'your password has been changed',
    'because your password was changed',
    'you changed your password',
    'password changed',
    'you entered an old password',
    'you entered your old password',
    'the password you entered is old'
  ];

  const passwordChangedUrlHints = [
    'update-password',
    'change-password',
    'change_password',
    'password/reset',
    'recover/password',
    'login/device-based/update-password',
    'hacked=1',
    '/recover/initiate/?s=14&hacked=1'
  ];

  const passwordChangedHelpUrlHints = [
    '/login/help.php?st=opw',
    'login/help.php?st=opw',
    'st=opw'
  ];

  const passwordChangedActionTexts = [
    'bao ve tai khoan cua toi',
    'dat lai mat khau cua toi',
    'secure my account',
    'reset my password'
  ];

  const hasNewPasswordFlow = documents.some((doc) => {
    try {
      const text = normalizeText([
        String(doc.title || ''),
        String(doc.body?.innerText || ''),
        String(doc.documentElement?.innerText || '')
      ].join(' '));
      return text.includes('mat khau moi') ||
        text.includes('tao mat khau moi') ||
        text.includes('new password') ||
        text.includes('create a new password');
    } catch {
      return false;
    }
  });

  const matchedPasswordChangedAction = actionHints.find((action) =>
    passwordChangedUrlHints.some((hint) => action.href.includes(hint)) ||
    (action.href.includes('/recover/initiate/') && passwordChangedActionTexts.some((text) => action.text.includes(text))) ||
    passwordChangedActionTexts.some((text) => action.text.includes(text))
  );

  const matchedPasswordChangedHelp = actionHints.find((action) =>
    passwordChangedHelpUrlHints.some((hint) => action.href.includes(hint))
  );

  if (includesAny(passwordChangedNeedles) ||
      passwordChangedUrlHints.some((hint) => allLowerHrefs.includes(hint)) ||
      passwordChangedHelpUrlHints.some((hint) => allLowerHrefs.includes(hint)) ||
      hasNewPasswordFlow ||
      matchedPasswordChangedAction ||
      matchedPasswordChangedHelp) {
    const chiTietPasswordChanged =
      matchedPasswordChangedAction?.text ||
      matchedPasswordChangedAction?.href ||
      matchedPasswordChangedHelp?.text ||
      matchedPasswordChangedHelp?.href ||
      firstMatch(passwordChangedNeedles) ||
      titleText ||
      href;
    return makeResult('blocked', 'password_changed', chiTietPasswordChanged);
  }

  const captchaNeedles = [
    'captcha',
    'security check',
    'kiem tra bao mat',
    'nhap cac ky tu ban nhin thay',
    'nhap cac ky tu ma ban nhin thay',
    'nhap ma ban thay trong anh',
    'enter the characters you see',
    'enter the characters shown',
    'type the text you see in the image',
    'prove you are not a robot',
    'are you a real person',
    'toi khong phai la nguoi may',
    "i'm not a robot",
    'i am not a robot',
    'recaptcha'
  ];

  const captchaAudioNeedles = [
    'phat am thanh',
    'nghe am thanh',
    'play audio',
    'listen to the audio',
    'audio challenge'
  ];

  const hasCaptchaImage = documents.some((doc) => {
    try {
      return !!doc.querySelector(
        'img[src*="captcha/tfbimage" i], img[src*="/captcha/" i], img[src*="captcha" i], img[alt*="captcha" i], iframe[src*="captcha" i], iframe[src*="recaptcha" i], iframe[src*="hcaptcha" i], [id*="recaptcha" i], [class*="recaptcha" i], [id*="hcaptcha" i], [class*="hcaptcha" i], .g-recaptcha, .h-captcha'
      );
    } catch {
      return false;
    }
  });

  const hasCaptchaInput = documents.some((doc) => {
    try {
      return !!doc.querySelector(
        'input[name*="captcha" i], input[id*="captcha" i], input[aria-label*="captcha" i], input[id*="recaptcha" i], textarea[id*="g-recaptcha-response" i], textarea[name*="g-recaptcha-response" i], textarea[id*="h-captcha-response" i], textarea[name*="h-captcha-response" i], [id="recaptcha-anchor"], [aria-labelledby*="recaptcha" i], input[type="text"], input[inputmode="text"]'
      );
    } catch {
      return false;
    }
  });

  const hasRecaptchaAnchor = documents.some((doc) => {
    try {
      return !!doc.querySelector(
        '#recaptcha-anchor, #recaptcha-anchor-label, [id*="recaptcha-anchor" i], [aria-labelledby*="recaptcha-anchor-label" i], [class*="rc-anchor" i]'
      );
    } catch {
      return false;
    }
  });

  const matchedCaptchaAudioAction = actionHints.find((action) =>
    captchaAudioNeedles.some((needle) => action.text.includes(needle))
  );

  const hasContinueAction = actionHints.some((action) =>
    action.text === 'tiep tuc' ||
    action.text.includes('tiep tuc') ||
    action.text === 'continue' ||
    action.text.includes('continue')
  );

  const hasStrongCaptchaText = includesAny(captchaNeedles);
  const hasStrongCaptchaHref = allLowerHrefs.includes('captcha/tfbimage') || allLowerHrefs.includes('/captcha/');
  const duDauHieuCaptcha =
    hasCaptchaImage ||
    hasRecaptchaAnchor ||
    !!matchedCaptchaAudioAction ||
    hasStrongCaptchaText ||
    hasStrongCaptchaHref;

  const duDauHieuNhapCaptcha =
    hasCaptchaInput ||
    hasRecaptchaAnchor ||
    hasContinueAction ||
    hasCaptchaImage;

  if (duDauHieuCaptcha && duDauHieuNhapCaptcha) {
    const chiTietCaptcha =
      (hasRecaptchaAnchor ? 'recaptcha-anchor' : '') ||
      matchedCaptchaAudioAction?.text ||
      (hasCaptchaImage ? 'captcha/tfbimage' : '') ||
      firstMatch(captchaNeedles) ||
      (hasContinueAction ? 'tiep tuc' : '') ||
      firstMatch(captchaNeedles) ||
      titleText ||
      href;
    return makeResult('blocked', 'captcha', chiTietCaptcha);
  }

  if (/(^|\D)956(\D|$)/.test(normalizedBodyText)) {
    return makeResult('blocked', '956', '956');
  }

  const wrongPasswordNeedles = [
    'sai mat khau',
    'ten nguoi dung hoac mat khau khong hop le',
    'ten dang nhap hoac mat khau khong hop le',
    'mat khau ban nhap khong chinh xac',
    'mat khau ban da nhap khong chinh xac',
    'mat khau ma ban nhap khong chinh xac',
    'invalid username or password',
    'username or password is invalid',
    'the password you entered is incorrect',
    "the password that you've entered is incorrect",
    'the password you entered was incorrect',
    'incorrect password'
  ];

  const forgotPasswordNeedles = [
    'quen mat khau',
    'forgot password'
  ];

  const matchedForgotPasswordAction = actionHints.find((action) =>
    forgotPasswordNeedles.some((needle) => action.text.includes(needle))
  );

  if (includesAny(wrongPasswordNeedles) || (matchedForgotPasswordAction && includesAny(wrongPasswordNeedles))) {
    const chiTietWrongPassword =
      firstMatch(wrongPasswordNeedles) ||
      matchedForgotPasswordAction?.text ||
      titleText ||
      href;
    return makeResult('blocked', 'wrong_password', chiTietWrongPassword);
  }

  const noAccountNeedles = [
    'khong tim thay tai khoan',
    'tai khoan khong ton tai',
    "isn't connected to an account",
    "is not connected to an account",
    "we couldn't find your account"
  ];
  if (includesAny(noAccountNeedles)) {
    return makeResult('blocked', 'no_account', firstMatch(noAccountNeedles));
  }

  const loginIdentifyNeedles = [
    'hay tim tai khoan cua ban va dang nhap',
    'hay tim tai khoan cua ban',
    'tim tai khoan cua ban va dang nhap',
    'find your account and log in',
    'find your account and login',
    'find your account'
  ];
  const loginIdentifyAction = actionHints.find((action) =>
    action.href.includes('facebook.com/login/identify') ||
    action.href.includes('https://facebook.com/login/identify/') ||
    action.href.includes('https://www.facebook.com/login/identify/') ||
    action.href.includes('/login/identify') ||
    action.href.includes('/login/identify/') ||
    loginIdentifyNeedles.some((needle) => action.text.includes(needle))
  );
  if (allLowerHrefs.includes('facebook.com/login/identify') ||
      allLowerHrefs.includes('https://facebook.com/login/identify/') ||
      allLowerHrefs.includes('https://www.facebook.com/login/identify/') ||
      allLowerHrefs.includes('/login/identify') ||
      allLowerHrefs.includes('/login/identify/') ||
      includesAny(loginIdentifyNeedles) ||
      loginIdentifyAction) {
    const chiTietLoginIdentify =
      loginIdentifyAction?.text ||
      loginIdentifyAction?.href ||
      firstMatch(loginIdentifyNeedles) ||
      href;
    return makeResult('pending', 'login_identify', chiTietLoginIdentify);
  }

  const disabledNeedles = [
    'tai khoan cua ban da bi vo hieu hoa',
    'tai khoan da bi vo hieu hoa',
    'account disabled',
    'disabled for violating'
  ];
  if (includesAny(disabledNeedles)) {
    return makeResult('blocked', 'account_disabled', firstMatch(disabledNeedles));
  }

  const lockedNeedles = [
    'tai khoan cua ban da bi khoa',
    'tai khoan cua ban tam thoi bi khoa',
    'your account has been locked',
    'account locked',
    'temporarily blocked'
  ];
  if (includesAny(lockedNeedles)) {
    return makeResult('blocked', 'account_locked', firstMatch(lockedNeedles));
  }

  const rateLimitedNeedles = [
    'we limit how often',
    'try again later',
    'ban da thao tac qua nhanh',
    'thu lai sau',
    'qua nhieu lan'
  ];
  if (includesAny(rateLimitedNeedles)) {
    return makeResult('blocked', 'rate_limited', firstMatch(rateLimitedNeedles));
  }

  const suspiciousNeedles = [
    'hoat dong dang ngo',
    'dang nhap bat thuong',
    'suspicious activity',
    'unusual login attempt',
    'help us confirm'
  ];
  if (includesAny(suspiciousNeedles)) {
    return makeResult('blocked', 'suspicious_activity', firstMatch(suspiciousNeedles));
  }

  const notAllowedNeedles = [
    'khong cho phep dang nhap',
    'not allowed to log in',
    'ban bi han che dang nhap'
  ];
  if (includesAny(notAllowedNeedles)) {
    return makeResult('blocked', 'login_not_allowed', firstMatch(notAllowedNeedles));
  }

  const twoFactorNeedles = [
    'ma xac thuc',
    'security code',
    'authentication code',
    'two-factor'
  ];
  if (allLowerHrefs.includes('two_factor') || includesAny(twoFactorNeedles)) {
    return makeResult('checkpoint', 'two_factor', firstMatch(twoFactorNeedles) || titleText);
  }

  const verifyIdentityNeedles = [
    'xac nhan danh tinh',
    'confirm your identity',
    'verify your identity',
    'upload your id',
    'chung minh danh tinh'
  ];
  if (includesAny(verifyIdentityNeedles)) {
    return makeResult('checkpoint', 'verify_identity', firstMatch(verifyIdentityNeedles));
  }

  if (allLowerHrefs.includes('checkpoint') || allLowerHrefs.includes('approvals') || allLowerHrefs.includes('challenge')) {
    return makeResult('checkpoint', 'checkpoint', titleText || href);
  }

  const isLoginLikePage = allLowerHrefs.includes('/login') ||
                          allLowerHrefs.includes('recover') ||
                          allLowerHrefs.includes('device-based') ||
                          allLowerHrefs.includes('facebook.com/?sk=welcome');

  const successTextNeedles = [
    "what's on your mind",
    'ban dang nghi gi',
    'create post',
    'tao bai viet',
    'news feed',
    'messenger',
    'marketplace',
    'watch',
    'groups',
    'thong bao',
    'notifications',
    'ban be',
    'friends'
  ];

  const successUrlHints = [
    '/home.php',
    '/friends/',
    '/messages/',
    '/marketplace/',
    '/watch/',
    '/groups/',
    '/notifications',
    '/bookmarks/',
    '/profile.php',
    '/me/',
    '/settings'
  ];

  const hasNavigationShell = documents.some((doc) => {
    try {
      return !!doc.querySelector('nav, [role="navigation"], [role="banner"]');
    } catch {
      return false;
    }
  });

  const hasProfileOrAccountSurface = documents.some((doc) => {
    try {
      return !!doc.querySelector(
        'a[href*="/logout" i], form[action*="logout" i], a[href*="/me/" i], a[href*="/profile.php" i], a[href*="/settings" i], [aria-label*="Profile" i], [aria-label*="Trang cá nhân" i], [aria-label*="Your profile" i]'
      );
    } catch {
      return false;
    }
  });

  const hasLoggedInFeatureSurface = documents.some((doc) => {
    try {
      return !!doc.querySelector(
        'a[href*="/friends/" i], a[href*="/messages/" i], a[href*="/marketplace/" i], a[href*="/watch/" i], a[href*="/groups/" i], a[href*="/notifications" i], a[href*="/bookmarks/" i]'
      );
    } catch {
      return false;
    }
  });

  const hasSuccessText = includesAny(successTextNeedles);
  const hasSuccessUrl = successUrlHints.some((hint) => allLowerHrefs.includes(hint));
  const hasStrongSuccessSignal =
    hasProfileOrAccountSurface ||
    (hasNavigationShell && hasLoggedInFeatureSurface) ||
    (hasNavigationShell && hasSuccessText) ||
    (hasSuccessUrl && (hasNavigationShell || hasLoggedInFeatureSurface || hasSuccessText));

  if (hasPasswordInput || hasLoginButton || isLoginLikePage) {
    return makeResult('pending', '', titleText || href);
  }

  if (hasStrongSuccessSignal) {
    return makeResult('success', '', titleText || href);
  }

  return makeResult('pending', '', titleText || href);
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

        private void BoTickTatCaDong()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                row.Cells["colChon"].Value = false;                                            // Đưa toàn bộ checkbox về trạng thái không chọn để luồng chọn của app luôn nhất quán
            }
        }

        private DataGridViewRow? TimDongTheoUid(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return null;
            }

            string uidCanTim = uid.Trim();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string uidTrenDong = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                if (string.Equals(uidTrenDong, uidCanTim, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        private void BoTickDongTheoUid(string uid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => BoTickDongTheoUid(uid));                                    // Luồng theo dõi đăng nhập chạy async nên mọi thay đổi checkbox phải quay về UI thread
                return;
            }

            DataGridViewRow? row = TimDongTheoUid(uid);
            if (row == null)
            {
                return;
            }

            row.Cells["colChon"].Value = false;                                               // Dòng đã có kết quả cuối thì bỏ tick để batch không lẫn với các dòng chưa xử lý
            LuuDuLieuGridRaFile();
        }

        private void DanhDauDongDaXuLyTrongBatch(string uid, CheDoDieuPhoiPhien cheDo)
        {
            if (!LaCheDoChayTiepTheoDanhSachDaTick(cheDo) || string.IsNullOrWhiteSpace(uid))
            {
                return;                                                                        // Chỉ batch các dòng đã tick mới cần tự bỏ check khi hoàn tất một dòng
            }

            BoTickDongTheoUid(uid);
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

        private List<DataGridViewRow> LayDanhSachDongBoiDen()
        {
            return dataGridView1.SelectedRows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .OrderBy(row => row.Index)
                .ToList();                                                                     // Bôi đen chỉ dùng như nguồn để menu Chọn những dòng bôi đen chuyển thành tick thật
        }

        private void CopyDuLieuTheoDongDaTick(Func<DataGridViewRow, string> cachLayNoiDung)
        {
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();
            if (dsDongDaTick.Count == 0)
            {
                MessageBox.Show("Vui lòng tick ít nhất 1 dòng để copy.");
                return;
            }

            string noiDungCanCopy = string.Join(Environment.NewLine,
                dsDongDaTick
                    .Select(cachLayNoiDung)
                    .Where(value => !string.IsNullOrWhiteSpace(value)));                       // Copy luôn theo các dòng đã tick, kể cả khi đang copy lẻ từng cột

            if (string.IsNullOrWhiteSpace(noiDungCanCopy))
            {
                MessageBox.Show("Không có dữ liệu để copy.");
                return;
            }

            Clipboard.SetText(noiDungCanCopy);
        }

        private string TaoNoiDungDayDuCuaDong(DataGridViewRow row)
        {
            return string.Join("|",
                LayGiaTriCell(row, "colUID"),
                LayGiaTriCell(row, "colPass"),
                LayGiaTriCell(row, "colTen"),
                LayGiaTriCell(row, "colEmail"),
                LayGiaTriCell(row, "colCookie"),
                LayGiaTriCell(row, "colGhiChu"));                                              // Copy cả dòng theo các trường chính đang cần quản lý
        }

        private string LayGiaTriCell(DataGridViewRow row, string tenCot)
        {
            return row.Cells[tenCot].Value?.ToString()?.Trim() ?? string.Empty;
        }
        //
        //  HÀM XÓA MỘT DÒNG ĐÃ TICK
        //
        private void XoaMotDongDaTick()
        {
            CapNhatTrangThai("Đang chuẩn bị xóa 1 dòng đang tick...", Color.DarkGoldenrod);
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy danh sách tick thật để bảo đảm xóa đúng nghiệp vụ đã chốt
            if (dsDongDaTick.Count != 1)
            {
                CapNhatTrangThai("Xóa thất bại: chưa tick đúng 1 dòng.", Color.Firebrick);
                MessageBox.Show("Vui lòng tick đúng 1 dòng để xóa.");                          // Xóa một dòng chỉ chấp nhận đúng 1 checkbox đang bật
                return;
            }

            DataGridViewRow row = dsDongDaTick[0];                                             // Lấy dòng duy nhất đang được tick để xử lý xóa
            string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;        // UID dùng để xóa khỏi ds.txt và xử lý profile tương ứng

            if (string.IsNullOrWhiteSpace(uid))
            {
                CapNhatTrangThai("Xóa thất bại: dòng đang tick không có UID hợp lệ.", Color.Firebrick);
                MessageBox.Show("Dòng đang tick không có UID hợp lệ.");
                return;
            }

            CapNhatTrangThai($"Đang dọn các phiên tạm của UID {uid}...", Color.DarkGoldenrod);
            SessionRuntimeService.TryCloseAndCleanupSessionsByUid(uid);
            congDebugTheoUid.Remove(uid);

            XoaUidKhoiDsTxt(uid);                                                              // Xóa UID tương ứng ra khỏi ds.txt để dữ liệu file và grid luôn đồng bộ
            dataGridView1.Rows.Remove(row);                                                    // Bỏ dòng đã xóa ra khỏi grid
            CapNhatLaiSTT();                                                                   // Đánh lại số thứ tự để bảng không bị lệch sau khi xóa
            LuuDuLieuGridRaFile();
            CapNhatTrangThai($"Đã xóa xong dòng {uid}.", Color.ForestGreen);
        }
        //
        //  HÀM XÓA NHIỀU DÒNG ĐÃ TICK
        //
        private void XoaNhieuDongDaTick()
        {
            CapNhatTrangThai("Đang chuẩn bị xóa nhiều dòng...", Color.DarkGoldenrod);
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Lấy toàn bộ dòng đang được tick thật để xử lý xóa hàng loạt
            if (dsDongDaTick.Count < 2)
            {
                CapNhatTrangThai("Xóa nhiều thất bại: cần tick từ 2 dòng trở lên.", Color.Firebrick);
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

                dsUidCanXoa.Add(uid);                                                          // Gom UID hợp lệ để xóa profile và dữ liệu đồng bộ theo cùng một danh sách
            }

            CapNhatTrangThai("Đang dọn các phiên tạm đang chạy...", Color.DarkGoldenrod);

            foreach (string uid in dsUidCanXoa)
            {
                SessionRuntimeService.TryCloseAndCleanupSessionsByUid(uid);
                congDebugTheoUid.Remove(uid);
                XoaUidKhoiDsTxt(uid);
            }

            for (int i = dsDongDaTick.Count - 1; i >= 0; i--)
            {
                dataGridView1.Rows.Remove(dsDongDaTick[i]);                                    // Xóa từ cuối về đầu để tránh lệch chỉ số khi bỏ nhiều dòng
            }

            CapNhatLaiSTT();                                                                   // Đánh lại STT sau khi xóa hàng loạt
            LuuDuLieuGridRaFile();
            CapNhatTrangThai($"Đã xóa xong {dsUidCanXoa.Count} dòng đã tick.", Color.ForestGreen);
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

                if (TryTachUidVaPasswordTuDongDs(lineDaCat, out string uidTrenDong, out _, out _) &&
                    string.Equals(uidTrenDong, uidCanXoa, StringComparison.OrdinalIgnoreCase))
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
            string duongDanProfileTheoUid = AppPaths.GetProfilePath(uid);                      // Xác định đúng thư mục profile theo UID của dòng bị xóa trong data\profiles
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
            string duongDanProfileTheoUid = AppPaths.GetProfilePath(uid);                      // Xác định thư mục profile cần xóa hẳn khỏi ổ đĩa trong data\profiles
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
            dataGridView1.Rows.Clear();
            dangNapGridTuFile = true;

            try
            {
                if (!File.Exists(gridFilePath))
                {
                    CapNhatTrangThai("Chưa có grid.json, bảng sẽ bắt đầu rỗng.", Color.ForestGreen);
                    return;
                }

                string json = File.ReadAllText(gridFilePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    CapNhatTrangThai("grid.json đang rỗng, bảng sẽ bắt đầu rỗng.", Color.ForestGreen);
                    return;
                }

                List<DuLieuDongGrid> dsDongGrid = JsonSerializer.Deserialize<List<DuLieuDongGrid>>(json) ?? [];
                int stt = 1;

                foreach (DuLieuDongGrid dong in dsDongGrid)
                {
                    if (string.IsNullOrWhiteSpace(dong.Uid))
                    {
                        continue;                                                               // Bỏ qua dòng lỗi để không nạp UID rỗng lên grid
                    }

                    int rowIndex = dataGridView1.Rows.Add();
                    DataGridViewRow row = dataGridView1.Rows[rowIndex];
                    row.Cells["colSTT"].Value = stt++;
                    row.Cells["colChon"].Value = false;
                    row.Cells["colUID"].Value = dong.Uid;
                    row.Cells["colPass"].Value = dong.Password;
                    row.Cells["colNgayTao"].Value = dong.NgayTao;
                    row.Cells["colTen"].Value = dong.Ten;
                    row.Cells["colEmail"].Value = dong.Email;
                    row.Cells["colGhiChu"].Value = dong.GhiChu;
                    row.Cells["colTuongTacCuoi"].Value = dong.TuongTacCuoi;
                    row.Cells["colTrangThai"].Value = dong.TrangThai;
                    row.Cells["colCookie"].Value = dong.Cookie;
                }

                CapNhatMauDongGrid();
                dataGridView1.ClearSelection();
                CapNhatTrangThai($"Đã nạp {dataGridView1.Rows.Count} dòng từ grid.json.", Color.ForestGreen);
            }
            catch
            {
                CapNhatTrangThai("Không đọc được grid.json, bảng sẽ bắt đầu rỗng.", Color.Firebrick);
            }
            finally
            {
                dangNapGridTuFile = false;
            }
        }

        private void LuuDuLieuGridRaFile()
        {
            if (dangNapGridTuFile)
            {
                return;                                                                        // Đang nạp lại từ file thì không ghi ngược vào grid.json
            }

            try
            {
                List<DuLieuDongGrid> dsDongGrid = new();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(uid))
                    {
                        continue;                                                               // Chỉ lưu các dòng có UID thật để file grid luôn gọn và đúng nghiệp vụ
                    }

                    dsDongGrid.Add(new DuLieuDongGrid
                    {
                        Uid = uid,
                        Password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty,
                        NgayTao = row.Cells["colNgayTao"].Value?.ToString()?.Trim() ?? string.Empty,
                        Ten = row.Cells["colTen"].Value?.ToString()?.Trim() ?? string.Empty,
                        Email = row.Cells["colEmail"].Value?.ToString()?.Trim() ?? string.Empty,
                        GhiChu = row.Cells["colGhiChu"].Value?.ToString()?.Trim() ?? string.Empty,
                        TuongTacCuoi = row.Cells["colTuongTacCuoi"].Value?.ToString()?.Trim() ?? string.Empty,
                        TrangThai = row.Cells["colTrangThai"].Value?.ToString()?.Trim() ?? string.Empty,
                        Cookie = row.Cells["colCookie"].Value?.ToString()?.Trim() ?? string.Empty
                    });
                }

                string json = JsonSerializer.Serialize(dsDongGrid, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                string tempPath = $"{gridFilePath}.tmp";
                File.WriteAllText(tempPath, json, new UTF8Encoding(false));
                File.Copy(tempPath, gridFilePath, true);
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                CapNhatTrangThai($"Không thể lưu grid.json: {ex.Message}", Color.Firebrick);
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
            CapNhatTrangThai("Đang chuẩn bị xóa các dòng đã tick...", Color.DarkGoldenrod);  // Menu xóa chung cũng cần báo rõ trước khi điều phối sang xóa 1 hay xóa nhiều
            int soDongDaTick = LayDanhSachDongDaTick().Count;                                  // Đếm số dòng được tick để điều phối đúng nhánh xóa bên trong

            if (soDongDaTick == 0)
            {
                CapNhatTrangThai("Xóa thất bại: chưa tick dòng nào.", Color.Firebrick);
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
            CapNhatTrangThai("Đang chuẩn bị điền lại UID/Password...", Color.DarkGoldenrod); // Đây là luồng phụ nhưng người dùng vẫn cần biết app có đang kết nối vào Chrome hay không
            List<DataGridViewRow> dsDongDaTick = LayDanhSachDongDaTick();                      // Chỉ lấy những dòng được chọn thật bằng checkbox để tránh điền nhầm theo bôi đen
            if (dsDongDaTick.Count != 1)
            {
                CapNhatTrangThai("Điền UID/Password thất bại: chưa tick đúng 1 dòng.", Color.Firebrick);
                MessageBox.Show("Vui lòng tick đúng 1 dòng để điền lại UID và Password.");
                return;                                                                        // Menu này chỉ có ý nghĩa khi đang làm việc với đúng 1 tài khoản đang mở
            }

            DataGridViewRow row = dsDongDaTick[0];                                             // Lấy đúng dòng đang muốn điền lại thông tin vào phiên Chrome đang mở
            string uid = row.Cells["colUID"].Value?.ToString()?.Trim() ?? string.Empty;        // UID cũng là khóa để tìm lại đúng cổng debug của phiên Chrome đã mở trước đó
            string password = row.Cells["colPass"].Value?.ToString()?.Trim() ?? string.Empty;  // Mật khẩu lấy trực tiếp từ grid để điền lại vào form đăng nhập đang mở

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
            {
                CapNhatTrangThai("Điền UID/Password thất bại: thiếu UID hoặc Password.", Color.Firebrick);
                MessageBox.Show("Dòng đang chọn chưa có đủ UID hoặc Password.");
                return;                                                                        // Nếu thiếu dữ liệu thì dừng để tránh điền sai hoặc điền chuỗi rỗng
            }

            if (!congDebugTheoUid.TryGetValue(uid, out int congDebugChrome))
            {
                CapNhatTrangThai("Điền UID/Password thất bại: chưa có Chrome đang mở.", Color.Firebrick);
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
                CapNhatTrangThai($"Đang điền lại UID/Password trên Chrome đang mở của {uid}...", Color.DarkGoldenrod);
                string? webSocketDebuggerUrl = await LayWebSocketDebuggerUrlFacebookDangMoAsync(congDebugChrome); // Lấy tab Facebook đang mở thực tế của phiên Chrome này, bất kể đang ở URL nào
                if (string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
                {
                    CapNhatTrangThai("Điền UID/Password thất bại: không tìm thấy tab Facebook đang mở.", Color.Firebrick);
                    MessageBox.Show("Không tìm thấy tab Facebook đang mở để điền lại UID và Password.");
                    return;                                                                    // Nếu Chrome đang mở nhưng không còn tab Facebook nào thì menu này không còn mục tiêu để điền
                }

                string script = TaoScriptTuDongDienDangNhap(uid, password);                    // Dùng lại cùng một script tự điền đã ổn định ở các luồng khác để tránh tách logic rời rạc
                bool daDienThanhCong = await ThuChayScriptTuDongDienAsync(webSocketDebuggerUrl, script);
                if (!daDienThanhCong)
                {
                    CapNhatTrangThai("Điền UID/Password thất bại: không tìm thấy ô đăng nhập.", Color.Firebrick);
                    MessageBox.Show("Không tìm thấy ô đăng nhập trên tab Facebook đang mở.");
                    return;
                }

                CapNhatTrangThai($"Đã điền lại UID/Password cho {uid}.", Color.ForestGreen);
            }
            catch
            {
                CapNhatTrangThai("Điền UID/Password thất bại: không kết nối được phiên Chrome.", Color.Firebrick);
                MessageBox.Show("Không thể kết nối lại phiên Chrome đang mở của dòng này.");
            }
        }
        // HÀM SỰ KIỆN HIỂN THỊ TIMES
        private void timer1_Tick(object sender, EventArgs e)
        {
            tssTime.Text = DateTime.Now.ToString("HH:mm:ss    dd/MM/yyyy");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            (string uidDangXuLy, _) = LayThongTinPhienDangXuLy();
            DanhDauDungThuCong();
            XoaHangChoTaiKhoanDaTick();                                                        // Stop phải hủy luôn batch các dòng đã tick để app không tự mở lại dòng kế tiếp

            if (!string.IsNullOrWhiteSpace(uidDangXuLy))
            {
                CapNhatTrangThai($"Đang dừng thủ công phiên của UID {uidDangXuLy}...", Color.DarkOrange);
                CapNhatTrangThaiDongTheoUid(uidDangXuLy, "Đã dừng thủ công");
                DongVaDonDepPhienDangXuLy(uidDangXuLy);
                CapNhatTrangThai($"Đã dừng thủ công UID {uidDangXuLy}. Chrome đã đóng, dòng hiện tại vẫn được giữ nguyên.", Color.RoyalBlue);
                return;
            }

            CapNhatTrangThai("Đã dừng thủ công. App sẽ không mở phiên mới cho đến khi bạn bấm Next lại.", Color.RoyalBlue);
        }
    }
}

