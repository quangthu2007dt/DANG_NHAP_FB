using System.Diagnostics;
using System.Text;
using System.Text.Json;

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
                return false;
            }

            DialogResult ketQua = MessageBox.Show(
                $"Đã có bản mới {ketQuaKiemTra.VersionMoiNhat}. Bạn có muốn cập nhật ngay không?",
                "Kiểm tra cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
            {
                return false;
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
                return false;
            }

            string manifestPathChoUpdater = TaoManifestTamChoUpdater(ketQuaKiemTra);
            string thamSo = TaoThamSoGoiUpdater(AppPaths.BaseDirectory, manifestPathChoUpdater, ketQuaKiemTra.PackagePath);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = updaterExePath,
                Arguments = thamSo,
                UseShellExecute = true,
                WorkingDirectory = AppPaths.BaseDirectory
            };

            try
            {
                GhiNhatKyKhoiDongUpdater(ketQuaKiemTra, manifestPathChoUpdater, thamSo, null);
                Process.Start(processStartInfo);
                return true;
            }
            catch (Exception ex)
            {
                GhiNhatKyKhoiDongUpdater(ketQuaKiemTra, manifestPathChoUpdater, thamSo, ex.Message);
                return false;
            }
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
            return File.Exists(packagePath) ? packagePath : null;
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

        private static string TaoManifestTamChoUpdater(UpdateCheckResult ketQuaKiemTra)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.TempDirectory);
                string manifestTamPath = Path.Combine(AppPaths.TempDirectory, "manifest_runtime_update.json");
                string json = JsonSerializer.Serialize(ketQuaKiemTra.Manifest, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(manifestTamPath, json, new UTF8Encoding(false));
                return manifestTamPath;
            }
            catch
            {
                return ManifestService.ManifestFilePath;
            }
        }

        private static void GhiNhatKyKhoiDongUpdater(UpdateCheckResult ketQuaKiemTra, string manifestPath, string thamSo, string? loi)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.LogsDirectory);
                string duongDanNhatKy = Path.Combine(AppPaths.LogsDirectory, "update_launch.log");
                var noiDung = new StringBuilder();
                noiDung.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
                noiDung.AppendLine($"VersionHienTai: {ketQuaKiemTra.VersionHienTai}");
                noiDung.AppendLine($"VersionMoiNhat: {ketQuaKiemTra.VersionMoiNhat}");
                noiDung.AppendLine($"NguonManifest: {ketQuaKiemTra.NguonManifest}");
                noiDung.AppendLine($"ManifestPath: {manifestPath}");
                noiDung.AppendLine($"PackagePath: {ketQuaKiemTra.PackagePath ?? "(null)"}");
                noiDung.AppendLine($"ThamSo: {thamSo}");

                if (!string.IsNullOrWhiteSpace(loi))
                {
                    noiDung.AppendLine($"Loi: {loi}");
                }

                noiDung.AppendLine();
                File.AppendAllText(duongDanNhatKy, noiDung.ToString(), new UTF8Encoding(false));
            }
            catch
            {
            }
        }
    }
}
