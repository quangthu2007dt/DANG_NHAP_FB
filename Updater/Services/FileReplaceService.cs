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

        public static void ThayTheFileChuongTrinh(string appDirectory, string sourceDirectory)
        {
            foreach (string directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string duongDanTuongDoi = Path.GetRelativePath(sourceDirectory, directory);
                if (CanBoQua(duongDanTuongDoi))
                {
                    continue;                                                                 // Không đụng vào data, logs, temp, packages trong bất kỳ gói update nào
                }

                string duongDanDich = Path.Combine(appDirectory, duongDanTuongDoi);
                Directory.CreateDirectory(duongDanDich);                                      // Tạo sẵn cây thư mục đích trước khi copy file chương trình
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
                    continue;                                                                 // Updater đang chạy thì không tự ghi đè chính nó ở nhịp đầu này
                }

                string duongDanDich = Path.Combine(appDirectory, duongDanTuongDoi);
                string? thuMucDich = Path.GetDirectoryName(duongDanDich);
                if (!string.IsNullOrWhiteSpace(thuMucDich))
                {
                    Directory.CreateDirectory(thuMucDich);
                }

                File.Copy(filePath, duongDanDich, true);                                      // Ghi đè file chương trình bằng bản mới trong package
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

            return ThuMucBoQua.Contains(segments[0]);                                         // Chỉ cần segment đầu nằm trong nhóm bảo vệ là updater bỏ qua toàn bộ nhánh đó
        }
    }
}
