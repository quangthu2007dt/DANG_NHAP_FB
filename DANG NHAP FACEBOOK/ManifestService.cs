using System.Text.Json;

namespace DANG_NHAP_FACEBOOK
{
    internal sealed class AppReleaseManifest
    {
        public string AppName { get; set; } = "DANG NHAP FACEBOOK";
        public string Channel { get; set; } = "stable";
        public string LatestVersion { get; set; } = "1.0.0";
        public string ReleaseDate { get; set; } = string.Empty;
        public string PackageFileName { get; set; } = string.Empty;
        public string PackageUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    internal static class ManifestService
    {
        private const string RemoteManifestUrl = "https://raw.githubusercontent.com/quangthu2007dt/DANG_NHAP_FB/main/release/stable/manifest.json";

        public static string ManifestFilePath => Path.Combine(AppPaths.BaseDirectory, "manifest.json");

        public static AppReleaseManifest DocManifestCucBo()
        {
            try
            {
                if (File.Exists(ManifestFilePath))
                {
                    string json = File.ReadAllText(ManifestFilePath);                         // Đọc manifest.json đi kèm bản build để chuẩn bị cho bước auto-update sau này
                    AppReleaseManifest? manifest = JsonSerializer.Deserialize<AppReleaseManifest>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (manifest != null &&
                        !string.IsNullOrWhiteSpace(manifest.AppName) &&
                        !string.IsNullOrWhiteSpace(manifest.LatestVersion))
                    {
                        return manifest;                                                       // Có manifest hợp lệ thì dùng luôn để updater tương lai không phải đoán cấu trúc dữ liệu
                    }
                }
            }
            catch
            {
            }

            AppVersionInfo thongTinPhienBan = VersionService.DocThongTinPhienBanHienTai();    // Fallback theo version hiện tại để app vẫn có metadata release tối thiểu

            return new AppReleaseManifest
            {
                AppName = thongTinPhienBan.AppName,
                Channel = thongTinPhienBan.Channel,
                LatestVersion = thongTinPhienBan.Version,
                ReleaseDate = thongTinPhienBan.ReleaseDate,
                PackageFileName = $"DANG_NHAP_FACEBOOK_{thongTinPhienBan.Version}.zip",
                PackageUrl = string.Empty,
                Notes = string.Empty
            };
        }

        public static AppReleaseManifest DocManifestCapNhat()
        {
            AppReleaseManifest? manifestTuXa = DocManifestTuXa();
            return manifestTuXa ?? DocManifestCucBo();
        }

        private static AppReleaseManifest? DocManifestTuXa()
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(8)
                };

                string json = httpClient.GetStringAsync(RemoteManifestUrl).GetAwaiter().GetResult(); // Ưu tiên manifest phát hành trên GitHub để app biết có bản mới thật hay không
                AppReleaseManifest? manifest = JsonSerializer.Deserialize<AppReleaseManifest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest != null &&
                    !string.IsNullOrWhiteSpace(manifest.AppName) &&
                    !string.IsNullOrWhiteSpace(manifest.LatestVersion) &&
                    !string.IsNullOrWhiteSpace(manifest.PackageFileName))
                {
                    return manifest;
                }
            }
            catch
            {
            }

            return null;                                                                      // Nếu không đọc được manifest từ xa thì fallback về manifest local
        }

        public static void KiemTraManifestCucBo()
        {
            _ = DocManifestCucBo();                                                           // Ép app đọc thử manifest ngay lúc startup để phát hiện sớm file manifest lỗi
        }
    }
}
