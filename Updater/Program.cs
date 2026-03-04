using Updater.Services;

namespace Updater
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                UpdaterArguments thamSo = UpdaterArgumentsParser.Parse(args);                 // Đọc tham số dòng lệnh để updater biết app nằm ở đâu và manifest nào cần đọc
                AppShutdownService.ChoAppChinhTat(thamSo.ProcessId);                          // Nếu app chính còn đang chạy thì updater chờ tắt hẳn trước khi đi tiếp
                ReleaseManifest manifest = UpdaterManifestService.DocManifest(thamSo.ManifestPath);

                Console.WriteLine($"App       : {manifest.AppName}");
                Console.WriteLine($"Channel   : {manifest.Channel}");
                Console.WriteLine($"Version   : {manifest.LatestVersion}");
                Console.WriteLine($"Release   : {manifest.ReleaseDate}");
                Console.WriteLine($"Package   : {manifest.PackageFileName}");
                Console.WriteLine($"App Dir   : {thamSo.AppDirectory}");
                Console.WriteLine("Updater đã sẵn sàng cho bước tải gói và thay file ở nhịp tiếp theo.");
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
