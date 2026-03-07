using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;

namespace DANG_NHAP_FACEBOOK
{
    internal static class UpdateService
    {
        private const string TenAppExe = "DANG NHAP FACEBOOK.exe";
        private const string TenUpdaterExe = "Updater.exe";
        private const string TenUpdaterDll = "Updater.dll";
        private const string TenUpdaterDeps = "Updater.deps.json";
        private const string TenUpdaterRuntimeConfig = "Updater.runtimeconfig.json";
        private const string TenUpdaterPdb = "Updater.pdb";
        private const string TenFileDanhDauCapNhatThanhCong = "update_success_marker.json";
        private const string TenFileDanhDauDangCapNhat = "update_pending_marker.json";

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

        private sealed class UpdateSuccessMarker
        {
            public string Version { get; set; } = string.Empty;
            public string UpdatedAt { get; set; } = string.Empty;
        }

        private sealed class UpdatePendingMarker
        {
            public string TargetVersion { get; set; } = string.Empty;
            public string RequestedAt { get; set; } = string.Empty;
        }

        public static void HienThongBaoCapNhatThanhCongNeuCo()
        {
            _ = ThuDocVaThongBaoTuMarkerThanhCong();                                          // Chi don marker sau update, khong hien thong bao khi app mo lai
            _ = ThuDocVaThongBaoTuMarkerDangCapNhat();                                        // Fallback cho truong hop updater cu chua ghi marker thanh cong
        }

        public static bool ThuKichHoatCapNhatNeuCo()
        {
            UpdateCheckResult ketQuaKiemTra = KiemTraCapNhat();
            if (!ketQuaKiemTra.CoBanMoi || !ketQuaKiemTra.CoTheCapNhat)
            {
                return false;
            }

            DialogResult ketQua = MessageBox.Show(
                $"Phiên bản hiện tại: {ketQuaKiemTra.VersionHienTai}{Environment.NewLine}" +
                $"Phiên bản mới nhất: {ketQuaKiemTra.VersionMoiNhat}{Environment.NewLine}" +
                $"Nguồn manifest: {ketQuaKiemTra.NguonManifest}{Environment.NewLine}{Environment.NewLine}" +
                "Đã có bản mới. Bạn có muốn cập nhật ngay không?",
                "Kiểm tra cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
            {
                return false;
            }

            bool daKhoiDongUpdater = ThuKhoiDongUpdater(ketQuaKiemTra);
            if (!daKhoiDongUpdater)
            {
                MessageBox.Show(
                    "Không thể khởi động updater để cập nhật." + Environment.NewLine +
                    "Vui lòng kiểm tra logs/update_launch.log và logs/updater_error.log.",
                    "Cập nhật thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return daKhoiDongUpdater;
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

            string appExePath = Path.Combine(AppPaths.BaseDirectory, TenAppExe);
            string manifestPathChoUpdater = TaoManifestTamChoUpdater(ketQuaKiemTra);
            string thamSo = TaoThamSoGoiUpdater(AppPaths.BaseDirectory, manifestPathChoUpdater, ketQuaKiemTra.PackagePath);

            try
            {
                GhiDanhDauDangCapNhat(ketQuaKiemTra.VersionMoiNhat);

                string updaterTamExePath = TaoBanSaoUpdaterTam();
                string scriptPath = TaoScriptChayUpdaterVaMoLaiApp(updaterTamExePath, thamSo, appExePath);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{scriptPath}\"",
                    UseShellExecute = true,
                    WorkingDirectory = AppPaths.BaseDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                GhiNhatKyKhoiDongUpdater(ketQuaKiemTra, manifestPathChoUpdater, thamSo, null);
                Process.Start(processStartInfo);                                              // Chay updater qua script de sau khi update xong app duoc mo lai tu dong
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
            if (string.Equals(versionHienTai, versionMoi, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (TryParseReleaseVersion(versionHienTai, out Version? parsedHienTai) &&
                TryParseReleaseVersion(versionMoi, out Version? parsedMoi))
            {
                return parsedMoi > parsedHienTai;
            }

            return false;                                                                     // Khong doan "khac nhau la moi hon" de tranh goi updater sai huong (vd: V2.001 vs V2)
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
            string appDirDaChuanHoa = ChuanHoaDuongDanThamSo(appDirectory, boDauGachCuoi: true);
            string manifestDaChuanHoa = ChuanHoaDuongDanThamSo(manifestPath);

            List<string> danhSachThamSo =
            [
                $"--app-dir \"{appDirDaChuanHoa}\"",
                $"--manifest \"{manifestDaChuanHoa}\"",
                $"--pid {Environment.ProcessId}"
            ];

            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                danhSachThamSo.Add($"--package \"{ChuanHoaDuongDanThamSo(packagePath)}\"");
            }

            return string.Join(" ", danhSachThamSo);
        }

        private static string TaoScriptChayUpdaterVaMoLaiApp(string updaterExePath, string thamSo, string appExePath)
        {
            Directory.CreateDirectory(AppPaths.TempDirectory);
            string scriptPath = Path.Combine(AppPaths.TempDirectory, "run_updater_and_restart.cmd");

            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine($"\"{updaterExePath}\" {thamSo}");

            if (File.Exists(appExePath))
            {
                script.AppendLine($"start \"\" \"{appExePath}\"");
            }

            script.AppendLine("del \"%~f0\"");
            File.WriteAllText(scriptPath, script.ToString(), new UTF8Encoding(false));
            return scriptPath;
        }

        private static string TaoBanSaoUpdaterTam()
        {
            Directory.CreateDirectory(AppPaths.TempDirectory);
            string thuMucUpdaterTam = Path.Combine(AppPaths.TempDirectory, $"updater_runtime_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(thuMucUpdaterTam);

            foreach (string tenFile in LayDanhSachTepUpdaterCanChep())
            {
                string nguon = Path.Combine(AppPaths.BaseDirectory, tenFile);
                if (!File.Exists(nguon))
                {
                    continue;
                }

                string dich = Path.Combine(thuMucUpdaterTam, tenFile);
                File.Copy(nguon, dich, true);
            }

            string updaterTamExePath = Path.Combine(thuMucUpdaterTam, TenUpdaterExe);
            if (!File.Exists(updaterTamExePath))
            {
                throw new InvalidOperationException("Khong tao duoc ban sao tam cua Updater.exe.");
            }

            return updaterTamExePath;
        }

        private static IEnumerable<string> LayDanhSachTepUpdaterCanChep()
        {
            yield return TenUpdaterExe;
            yield return TenUpdaterDll;
            yield return TenUpdaterDeps;
            yield return TenUpdaterRuntimeConfig;
            yield return TenUpdaterPdb;
        }

        private static bool ThuDocVaThongBaoTuMarkerThanhCong()
        {
            string markerPath = Path.Combine(AppPaths.TempDirectory, TenFileDanhDauCapNhatThanhCong);
            if (!File.Exists(markerPath))
            {
                return false;
            }

            try
            {
                File.Delete(markerPath);
            }
            catch
            {
            }

            return true;
        }

        private static bool ThuDocVaThongBaoTuMarkerDangCapNhat()
        {
            string markerPath = Path.Combine(AppPaths.TempDirectory, TenFileDanhDauDangCapNhat);
            if (!File.Exists(markerPath))
            {
                return false;
            }

            string versionMucTieu = string.Empty;

            try
            {
                string json = File.ReadAllText(markerPath);
                UpdatePendingMarker? marker = JsonSerializer.Deserialize<UpdatePendingMarker>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (marker != null && !string.IsNullOrWhiteSpace(marker.TargetVersion))
                {
                    versionMucTieu = marker.TargetVersion.Trim();
                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    File.Delete(markerPath);                                                  // Da xu ly marker pending o lan mo dau sau update thi xoa de tranh lap lai
                }
                catch
                {
                }
            }

            if (string.IsNullOrWhiteSpace(versionMucTieu))
            {
                return false;
            }

            AppVersionInfo thongTinHienTai = VersionService.DocThongTinPhienBanHienTai();
            if (!LaVersionDaDatMucTieu(thongTinHienTai.Version, versionMucTieu))
            {
                return false;
            }

            return true;
        }

        private static void GhiDanhDauDangCapNhat(string versionMoiNhat)
        {
            if (string.IsNullOrWhiteSpace(versionMoiNhat))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(AppPaths.TempDirectory);
                string markerPath = Path.Combine(AppPaths.TempDirectory, TenFileDanhDauDangCapNhat);
                var marker = new UpdatePendingMarker
                {
                    TargetVersion = versionMoiNhat.Trim(),
                    RequestedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
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

        private static bool LaVersionDaDatMucTieu(string versionHienTai, string versionMucTieu)
        {
            if (TryParseReleaseVersion(versionHienTai, out Version? parsedHienTai) &&
                TryParseReleaseVersion(versionMucTieu, out Version? parsedMucTieu))
            {
                return parsedHienTai >= parsedMucTieu;
            }

            return string.Equals(versionHienTai, versionMucTieu, StringComparison.OrdinalIgnoreCase);
        }

        private static string ChuanHoaDuongDanThamSo(string path, bool boDauGachCuoi = false)
        {
            string daChuanHoa = Path.GetFullPath(path).Trim();
            if (boDauGachCuoi)
            {
                daChuanHoa = daChuanHoa.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return daChuanHoa;
        }

        private static bool TryParseReleaseVersion(string value, out Version? version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string daCat = value.Trim();
            if (daCat.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                daCat = daCat[1..];
            }

            if (Version.TryParse(daCat, out Version? parsed))
            {
                version = parsed;
                return true;
            }

            MatchCollection matches = Regex.Matches(daCat, @"\d+");
            if (matches.Count == 0)
            {
                return false;
            }

            List<int> parts = new(matches.Count);
            foreach (Match item in matches)
            {
                if (int.TryParse(item.Value, NumberStyles.None, CultureInfo.InvariantCulture, out int so))
                {
                    parts.Add(so);
                }
            }

            if (parts.Count == 0)
            {
                return false;
            }

            version = parts.Count switch
            {
                1 => new Version(parts[0], 0),
                2 => new Version(parts[0], parts[1]),
                3 => new Version(parts[0], parts[1], parts[2]),
                _ => new Version(parts[0], parts[1], parts[2], parts[3])
            };

            return true;
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
