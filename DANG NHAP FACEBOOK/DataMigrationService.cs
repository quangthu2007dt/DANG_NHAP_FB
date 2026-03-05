using System.Text;

namespace DANG_NHAP_FACEBOOK
{
    internal static class DataMigrationService
    {
        private static readonly string[] UserAgentsMacDinh =
        [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 Safari/604.1"
        ];

        public static void KhoiTaoCauTrucDuLieu()
        {
            AppPaths.EnsureCoreDirectoriesExist();
            Directory.CreateDirectory(AppPaths.ProfileMauPath);
            Directory.CreateDirectory(AppPaths.SessionsRootPath);

            DiChuyenProfilesCuSangLegacyNeuCan();
            DamBaoSessionRegistryTonTai();

            DamBaoTapTinRongTonTai(AppPaths.DsFilePath);
            DamBaoTapTinRongTonTai(AppPaths.UserAgentDangDungFilePath);
            DamBaoDanhSachUserAgentMacDinh();
        }

        private static void DamBaoTapTinRongTonTai(string duongDanFile)
        {
            if (File.Exists(duongDanFile))
            {
                return;
            }

            File.WriteAllText(duongDanFile, string.Empty, Encoding.UTF8);
        }

        private static void DamBaoDanhSachUserAgentMacDinh()
        {
            if (!File.Exists(AppPaths.UserAgentsFilePath))
            {
                File.WriteAllLines(AppPaths.UserAgentsFilePath, UserAgentsMacDinh, Encoding.UTF8);
                return;
            }

            bool fileDangRong = File.ReadAllLines(AppPaths.UserAgentsFilePath)
                .All(line => string.IsNullOrWhiteSpace(line));

            if (fileDangRong)
            {
                File.WriteAllLines(AppPaths.UserAgentsFilePath, UserAgentsMacDinh, Encoding.UTF8);
            }
        }

        private static void DamBaoSessionRegistryTonTai()
        {
            if (File.Exists(AppPaths.SessionRegistryFilePath))
            {
                return;
            }

            File.WriteAllText(AppPaths.SessionRegistryFilePath, "[]", Encoding.UTF8);
        }

        private static void DiChuyenProfilesCuSangLegacyNeuCan()
        {
            if (!Directory.Exists(AppPaths.ProfilesRootPath))
            {
                return;
            }

            bool coDuLieuTrongProfilesCu =
                Directory.GetDirectories(AppPaths.ProfilesRootPath).Length > 0 ||
                Directory.GetFiles(AppPaths.ProfilesRootPath).Length > 0;

            if (!coDuLieuTrongProfilesCu)
            {
                try
                {
                    Directory.Delete(AppPaths.ProfilesRootPath, true);
                }
                catch
                {
                }

                Directory.CreateDirectory(AppPaths.ProfilesRootPath);
                return;
            }

            string tenBackup = $"_legacy_profiles_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            string duongDanBackup = Path.Combine(AppPaths.DataDirectory, tenBackup);
            int suffix = 1;

            while (Directory.Exists(duongDanBackup))
            {
                duongDanBackup = Path.Combine(AppPaths.DataDirectory, $"{tenBackup}_{suffix++}");
            }

            Directory.Move(AppPaths.ProfilesRootPath, duongDanBackup);
            Directory.CreateDirectory(AppPaths.ProfilesRootPath);
        }
    }
}
