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

        private static readonly HashSet<string> TepDangDuocUpdaterNap = LayDanhSachTepDangNap();

        public static void ThayTheFileChuongTrinh(string appDirectory, string sourceDirectory, Action<UpdateProgressInfo>? baoTienTrinh = null)
        {
            baoTienTrinh?.Invoke(new UpdateProgressInfo
            {
                Message = "Đang tạo cấu trúc thư mục mới..."
            });

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

                string duongDanDich = Path.Combine(appDirectory, duongDanTuongDoi);
                string duongDanDichChuanHoa = Path.GetFullPath(duongDanDich);
                if (TepDangDuocUpdaterNap.Contains(duongDanDichChuanHoa))
                {
                    continue;
                }

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

                    ketQua.Add(Path.GetFullPath(module.FileName));
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
