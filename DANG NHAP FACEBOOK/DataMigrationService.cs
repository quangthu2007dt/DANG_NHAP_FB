using System.Text;

namespace DANG_NHAP_FACEBOOK
{
    internal static class DataMigrationService
    {
        private static readonly string[] UserAgentsMacDinh =
        [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 Safari/604.1"
        ];

        public static void KhoiTaoCauTrucDuLieu()
        {
            AppPaths.EnsureCoreDirectoriesExist();                                            // Tạo sẵn toàn bộ khung thư mục chuẩn trước khi app chạy
            Directory.CreateDirectory(AppPaths.ProfileMauPath);                               // Luôn tạo sẵn profile_mau để máy mới có thể mở Chrome mẫu cấu hình ngay

            DamBaoTapTinRongTonTai(AppPaths.DsFilePath);                                      // ds.txt phải luôn có để các chức năng đọc danh sách không bị lỗi file thiếu
            DamBaoTapTinRongTonTai(AppPaths.UserAgentDangDungFilePath);                       // Tạo sẵn file lưu UA đang dùng để các lần ghi log sau không bị vấp đường dẫn
            DamBaoDanhSachUserAgentMacDinh();                                                 // Nếu user_agents.txt chưa có hoặc đang rỗng thì nạp sẵn các UA mặc định
        }

        private static void DamBaoTapTinRongTonTai(string duongDanFile)
        {
            if (File.Exists(duongDanFile))
            {
                return;                                                                       // File đã có sẵn thì giữ nguyên dữ liệu hiện tại
            }

            File.WriteAllText(duongDanFile, string.Empty, Encoding.UTF8);                     // Máy mới chưa có file thì tạo rỗng theo chuẩn UTF-8
        }

        private static void DamBaoDanhSachUserAgentMacDinh()
        {
            if (!File.Exists(AppPaths.UserAgentsFilePath))
            {
                File.WriteAllLines(AppPaths.UserAgentsFilePath, UserAgentsMacDinh, Encoding.UTF8);
                return;                                                                       // Chưa có file thì tạo mới luôn với danh sách mặc định
            }

            bool fileDangRong = File.ReadAllLines(AppPaths.UserAgentsFilePath)
                .All(line => string.IsNullOrWhiteSpace(line));                                // Chỉ coi là cần khởi tạo lại khi file không còn UA hợp lệ nào

            if (fileDangRong)
            {
                File.WriteAllLines(AppPaths.UserAgentsFilePath, UserAgentsMacDinh, Encoding.UTF8);
            }
        }
    }
}
