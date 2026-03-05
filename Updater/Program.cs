using System.Text;
using System.Text.Json;
using Updater.Services;

namespace Updater
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                UpdaterArguments thamSo = UpdaterArgumentsParser.Parse(args);
                AppShutdownService.ChoAppChinhTat(thamSo.ProcessId);
                ReleaseManifest manifest = UpdaterManifestService.DocManifest(thamSo.ManifestPath);
                string packagePath = PackageDownloadService.LayHoacTaiGoiCapNhat(thamSo, manifest);
                string sourceDirectory = PackageDownloadService.GiaiNenGoiCapNhat(thamSo.AppDirectory, packagePath, manifest.LatestVersion);
                FileReplaceService.ThayTheFileChuongTrinh(thamSo.AppDirectory, sourceDirectory);
                GhiDanhDauCapNhatThanhCong(thamSo.AppDirectory, manifest.LatestVersion);

                Console.WriteLine($"App       : {manifest.AppName}");
                Console.WriteLine($"Channel   : {manifest.Channel}");
                Console.WriteLine($"Version   : {manifest.LatestVersion}");
                Console.WriteLine($"Release   : {manifest.ReleaseDate}");
                Console.WriteLine($"Package   : {packagePath}");
                Console.WriteLine($"App Dir   : {thamSo.AppDirectory}");
                Console.WriteLine("Updater đã giải nén gói và thay file chương trình thành công.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                GhiNhatKyLoi(ex, args);
                return 1;
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
