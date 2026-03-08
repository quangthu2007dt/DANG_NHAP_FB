using System.Text;
using System.Text.Json;
using Updater.Services;

namespace Updater
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using var form = new UpdaterProgressForm();
            form.Shown += async (_, _) => await ChayCapNhatAsync(form, args);
            Application.Run(form);
        }

        private static async Task ChayCapNhatAsync(UpdaterProgressForm form, string[] args)
        {
            try
            {
                void BaoTienTrinh(UpdateProgressInfo thongTin) => form.CapNhatTienTrinh(thongTin);

                BaoTienTrinh(new UpdateProgressInfo
                {
                    Message = "Dang doc tham so cap nhat..."
                });

                string thongBaoHoanTat = await Task.Run(() =>
                {
                    UpdaterArguments thamSo = UpdaterArgumentsParser.Parse(args);
                    ReleaseManifest manifest = UpdaterManifestService.DocManifest(thamSo.ManifestPath);
                    AppShutdownService.ChoAppChinhTat(thamSo.ProcessId, BaoTienTrinh);
                    string packagePath = PackageDownloadService.LayHoacTaiGoiCapNhat(thamSo, manifest, BaoTienTrinh);
                    string sourceDirectory = PackageDownloadService.GiaiNenGoiCapNhat(thamSo.AppDirectory, packagePath, manifest.LatestVersion, BaoTienTrinh);

                    BaoTienTrinh(new UpdateProgressInfo
                    {
                        Message = "Dang thay file chuong trinh...",
                        Percent = 100
                    });

                    FileReplaceService.ThayTheFileChuongTrinh(thamSo.AppDirectory, sourceDirectory, BaoTienTrinh);
                    GhiDanhDauCapNhatThanhCong(thamSo.AppDirectory, manifest.LatestVersion);
                    return $"Cap nhat xong {manifest.LatestVersion}. Dang mo lai app...";
                });

                BaoTienTrinh(new UpdateProgressInfo
                {
                    Message = thongBaoHoanTat,
                    Percent = 100
                });

                await Task.Delay(1200);
                Environment.ExitCode = 0;
                form.Close();
            }
            catch (Exception ex)
            {
                form.CapNhatTienTrinh(new UpdateProgressInfo
                {
                    Message = $"Cap nhat that bai: {ex.Message}",
                    IsError = true
                });

                GhiNhatKyLoi(ex, args);
                Environment.ExitCode = 1;
                await Task.Delay(2500);
                form.Close();
            }
        }

        private static void GhiNhatKyLoi(Exception ex, string[] args)
        {
            try
            {
                string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDirectory);
                string duongDanNhatKy = Path.Combine(logsDirectory, "updater_error.log");

                var noiDung = new StringBuilder();
                noiDung.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
                noiDung.AppendLine($"Args: {string.Join(" ", args)}");
                noiDung.AppendLine($"Message: {ex.Message}");
                noiDung.AppendLine($"StackTrace: {ex.StackTrace}");
                noiDung.AppendLine();

                File.AppendAllText(duongDanNhatKy, noiDung.ToString(), new UTF8Encoding(false));
            }
            catch
            {
            }
        }

        private static void GhiDanhDauCapNhatThanhCong(string appDirectory, string version)
        {
            try
            {
                string tempDirectory = Path.Combine(appDirectory, "temp");
                Directory.CreateDirectory(tempDirectory);

                string markerPath = Path.Combine(tempDirectory, "update_success_marker.json");
                var marker = new
                {
                    Version = version,
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string json = JsonSerializer.Serialize(marker, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(markerPath, json, new UTF8Encoding(false));
            }
            catch
            {
            }
        }
    }
}
