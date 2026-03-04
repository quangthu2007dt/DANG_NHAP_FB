using Updater.Services;

namespace Updater
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                UpdaterArguments thamSo = UpdaterArgumentsParser.Parse(args);                 // Đọc tham số dòng lệnh để updater biết app nằm ở đâu, manifest nào cần đọc và có pid nào cần chờ hay không
                AppShutdownService.ChoAppChinhTat(thamSo.ProcessId);                          // Nếu app chính còn đang chạy thì updater chờ tắt hẳn trước khi đi tiếp
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
                return 1;
            }
        }
    }
}
