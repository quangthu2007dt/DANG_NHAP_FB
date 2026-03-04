using System.Diagnostics;

namespace DANG_NHAP_FACEBOOK
{
    internal static class UpdateService
    {
        private const string TenUpdaterExe = "Updater.exe";

        internal sealed class UpdateCheckResult
        {
            public string VersionHienTai { get; set; } = string.Empty;
            public string VersionMoiNhat { get; set; } = string.Empty;
            public bool CoBanMoi { get; set; }
            public bool CoTheCapNhat { get; set; }
            public string NguonManifest { get; set; } = "Local";
            public string? PackagePath { get; set; }
            public AppReleaseManifest Manifest { get; set; } = new();
        }

        public static bool ThuKichHoatCapNhatNeuCo()
        {
            UpdateCheckResult ketQuaKiemTra = KiemTraCapNhat();
            if (!ketQuaKiemTra.CoBanMoi || !ketQuaKiemTra.CoTheCapNhat)
            {
                return false;                                                                 // Không có bản mới hoặc chưa đủ điều kiện updater thì app mở bình thường
            }

            DialogResult ketQua = MessageBox.Show(
                $"Đã có bản mới {ketQuaKiemTra.VersionMoiNhat}. Bạn có muốn cập nhật ngay không?",
                "Kiểm tra cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
            {
                return false;                                                                 // Người dùng từ chối thì app chạy tiếp bản hiện tại
            }

            return ThuKhoiDongUpdater(ketQuaKiemTra);
        }

        public static UpdateCheckResult KiemTraCapNhat()
        {
            AppVersionInfo thongTinHienTai = VersionService.DocThongTinPhienBanHienTai();
            ManifestReadResult ketQuaDocManifest = ManifestService.DocManifestCapNhatChiTiet();
            AppReleaseManifest manifest = ketQuaDocManifest.Manifest;
            string? packagePath = LayPackagePathNeuCo(manifest);
            bool coUpdater = File.Exists(Path.Combine(AppPaths.BaseDirectory, TenUpdaterExe));
            bool coNguonCapNhat = !string.IsNullOrWhiteSpace(packagePath) ||
                                  !string.IsNullOrWhiteSpace(manifest.PackageUrl);

            return new UpdateCheckResult
            {
                VersionHienTai = thongTinHienTai.Version,
                VersionMoiNhat = manifest.LatestVersion,
                CoBanMoi = CoBanMoiHon(thongTinHienTai.Version, manifest.LatestVersion),
                CoTheCapNhat = coUpdater && coNguonCapNhat,
                NguonManifest = ketQuaDocManifest.NguonHienThi,
                PackagePath = packagePath,
                Manifest = manifest
            };
        }

        public static bool ThuKhoiDongUpdater(UpdateCheckResult ketQuaKiemTra)
        {
            string updaterExePath = Path.Combine(AppPaths.BaseDirectory, TenUpdaterExe);
            if (!ketQuaKiemTra.CoTheCapNhat || !File.Exists(updaterExePath))
            {
                return false;                                                                 // Chưa đủ điều kiện chạy updater thì dừng ở đây
            }

            string thamSo = TaoThamSoGoiUpdater(AppPaths.BaseDirectory, ManifestService.ManifestFilePath, ketQuaKiemTra.PackagePath);
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
