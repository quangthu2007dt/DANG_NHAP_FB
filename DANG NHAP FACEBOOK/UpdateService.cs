using System.Diagnostics;

namespace DANG_NHAP_FACEBOOK
{
    internal static class UpdateService
    {
        private const string TenUpdaterExe = "Updater.exe";

        public static bool ThuKichHoatCapNhatNeuCo()
        {
            AppVersionInfo thongTinHienTai = VersionService.DocThongTinPhienBanHienTai();
            AppReleaseManifest manifest = ManifestService.DocManifestCucBo();

            if (!CoBanMoiHon(thongTinHienTai.Version, manifest.LatestVersion))
            {
                return false;                                                                 // Không có bản mới thì tiếp tục mở app bình thường
            }

            string updaterExePath = Path.Combine(AppPaths.BaseDirectory, TenUpdaterExe);
            if (!File.Exists(updaterExePath))
            {
                return false;                                                                 // Chưa có Updater.exe cạnh app thì chưa thể kích hoạt luồng update
            }

            string? packagePath = LayPackagePathNeuCo(manifest);
            bool coNguonCapNhat = !string.IsNullOrWhiteSpace(packagePath) ||
                                  !string.IsNullOrWhiteSpace(manifest.PackageUrl);

            if (!coNguonCapNhat)
            {
                return false;                                                                 // Chưa có package local hay packageUrl thì chỉ coi như metadata phát hành, chưa chạy updater
            }

            DialogResult ketQua = MessageBox.Show(
                $"Đã có bản mới {manifest.LatestVersion}. Bạn có muốn cập nhật ngay không?",
                "Kiểm tra cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
            {
                return false;                                                                 // Người dùng từ chối thì app chạy tiếp bản hiện tại
            }

            string thamSo = TaoThamSoGoiUpdater(AppPaths.BaseDirectory, ManifestService.ManifestFilePath, packagePath);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = updaterExePath,
                Arguments = thamSo,
                UseShellExecute = true,
                WorkingDirectory = AppPaths.BaseDirectory
            };

            Process.Start(processStartInfo);                                                  // Gọi Updater.exe riêng rồi thoát app chính để updater thay file
            return true;
        }

        private static bool CoBanMoiHon(string versionHienTai, string versionMoi)
        {
            if (Version.TryParse(versionHienTai, out Version? parsedHienTai) &&
                Version.TryParse(versionMoi, out Version? parsedMoi))
            {
                return parsedMoi > parsedHienTai;
            }

            return !string.Equals(versionHienTai, versionMoi, StringComparison.OrdinalIgnoreCase);
        }

        private static string? LayPackagePathNeuCo(AppReleaseManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.PackageFileName))
            {
                return null;
            }

            string packagePath = Path.Combine(AppPaths.PackagesDirectory, manifest.PackageFileName);
            return File.Exists(packagePath) ? packagePath : null;                             // Ưu tiên package local trong packages để dễ test cập nhật trước khi có server thật
        }

        private static string TaoThamSoGoiUpdater(string appDirectory, string manifestPath, string? packagePath)
        {
            List<string> danhSachThamSo =
            [
                $"--app-dir \"{appDirectory}\"",
                $"--manifest \"{manifestPath}\"",
                $"--pid {Environment.ProcessId}"
            ];

            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                danhSachThamSo.Add($"--package \"{packagePath}\"");
            }

            return string.Join(" ", danhSachThamSo);
        }
    }
}
