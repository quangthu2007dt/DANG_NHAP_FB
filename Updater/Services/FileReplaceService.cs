using System.Diagnostics;

namespace Updater.Services
{
    internal static class FileReplaceService
    {
        private static readonly HashSet<string> ThuMucBoQua = new(StringComparer.OrdinalIgnoreCase)
        {
            "data",
            "logs",
            "temp",
            "packages"
        };

        private static readonly HashSet<string> TepUpdaterDangChayBoQua = new(StringComparer.OrdinalIgnoreCase)
        {
            "Updater.exe",
            "Updater.dll",
            "Updater.deps.json",
            "Updater.runtimeconfig.json",
            "Updater.pdb"
        };

        private static readonly HashSet<string> TepDangDuocUpdaterNap = LayDanhSachTepDangNap();

        public static void ThayTheFileChuongTrinh(string appDirectory, string sourceDirectory)
        {
            foreach (string directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string duongDanTuongDoi = Path.GetRelativePath(sourceDirectory, directory);
                if (CanBoQua(duongDanTuongDoi))
                {
                    continue;
                }

                string duongDanDich = Path.Combine(appDirectory, duongDanTuongDoi);
                Directory.CreateDirectory(duongDanDich);
            }

            foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string duongDanTuongDoi = Path.GetRelativePath(sourceDirectory, filePath);
                if (CanBoQua(duongDanTuongDoi))
                {
                    continue;
                }

                string tenFile = Path.GetFileName(filePath);
                if (TepUpdaterDangChayBoQua.Contains(tenFile))
                {
                    continue;
                }

                // Portable updater may load runtime dlls in app directory, these files are locked.
                if (TepDangDuocUpdaterNap.Contains(tenFile))
                {
                    continue;
                }

                string duongDanDich = Path.Combine(appDirectory, duongDanTuongDoi);
                string? thuMucDich = Path.GetDirectoryName(duongDanDich);
                if (!string.IsNullOrWhiteSpace(thuMucDich))
                {
                    Directory.CreateDirectory(thuMucDich);
                }

                File.Copy(filePath, duongDanDich, true);
            }
        }

        private static bool CanBoQua(string duongDanTuongDoi)
        {
            string[] segments = duongDanTuongDoi
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .ToArray();

            if (segments.Length == 0)
            {
                return false;
            }

            return ThuMucBoQua.Contains(segments[0]);
        }

        private static HashSet<string> LayDanhSachTepDangNap()
        {
            try
            {
                using Process process = Process.GetCurrentProcess();
                var ketQua = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (ProcessModule? module in process.Modules)
                {
                    if (module == null || string.IsNullOrWhiteSpace(module.FileName))
                    {
                        continue;
                    }

                    ketQua.Add(Path.GetFileName(module.FileName));
                }

                return ketQua;
            }
            catch
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
