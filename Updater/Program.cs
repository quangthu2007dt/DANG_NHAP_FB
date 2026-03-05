using System.Text;
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
    }
}
