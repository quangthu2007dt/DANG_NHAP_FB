using System.Reflection;
using System.Text.Json;

namespace DANG_NHAP_FACEBOOK
{
    internal sealed class AppVersionInfo
    {
        public string AppName { get; set; } = "DANG NHAP FACEBOOK";
        public string Version { get; set; } = "1.0.0";
        public string ReleaseDate { get; set; } = string.Empty;
        public string Channel { get; set; } = "stable";
    }

    internal static class VersionService
    {
        public static string VersionFilePath => Path.Combine(AppPaths.BaseDirectory, "version.json");

        public static AppVersionInfo DocThongTinPhienBanHienTai()
        {
            try
            {
                if (File.Exists(VersionFilePath))
                {
                    string json = File.ReadAllText(VersionFilePath);                          // Đọc version.json đi kèm bản build để lấy đúng metadata phát hành hiện tại
                    AppVersionInfo? thongTin = JsonSerializer.Deserialize<AppVersionInfo>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (thongTin != null &&
                        !string.IsNullOrWhiteSpace(thongTin.AppName) &&
                        !string.IsNullOrWhiteSpace(thongTin.Version))
                    {
                        return thongTin;                                                      // Có file version hợp lệ thì ưu tiên dùng luôn để sau này dễ nối với manifest/update
                    }
                }
            }
            catch
            {
            }

            Version? assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;     // Fallback an toàn khi version.json bị thiếu hoặc lỗi

            return new AppVersionInfo
            {
                AppName = "DANG NHAP FACEBOOK",
                Version = assemblyVersion?.ToString(3) ?? "1.0.0",
                ReleaseDate = DateTime.Today.ToString("yyyy-MM-dd"),
                Channel = "stable"
            };
        }

        public static void KiemTraThongTinPhienBan()
        {
            _ = DocThongTinPhienBanHienTai();                                                 // Ép app đọc thử version ngay lúc startup để phát hiện sớm file version lỗi
        }
    }
}
