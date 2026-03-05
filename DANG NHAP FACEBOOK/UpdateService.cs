using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
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
