namespace DANG_NHAP_FACEBOOK
{
    internal static class AppPaths
    {
        private const string TenBienMoiTruongDataPath = "DANG_NHAP_FB_DATA_DIR";
        private static readonly string DataDirectoryResolved = GiaiQuyetDuongDanData();

        public static string BaseDirectory => AppContext.BaseDirectory;
        public static string DataDirectory => DataDirectoryResolved;
        public static string LogsDirectory => Path.Combine(BaseDirectory, "logs");
        public static string TempDirectory => Path.Combine(BaseDirectory, "temp");
        public static string PackagesDirectory => Path.Combine(BaseDirectory, "packages");
        public static string ProfilesRootPath => Path.Combine(DataDirectory, "profiles");
        public static string SessionsRootPath => Path.Combine(DataDirectory, "sessions");
        public static string SessionRegistryFilePath => Path.Combine(DataDirectory, "session_registry.json");
        public static string GridFilePath => Path.Combine(DataDirectory, "grid.json");

        public static string DsFilePath => Path.Combine(DataDirectory, "ds.txt");
        public static string UserAgentsFilePath => Path.Combine(DataDirectory, "user_agents.txt");
        public static string UserAgentDangDungFilePath => Path.Combine(DataDirectory, "ua_dang_dung.txt");
        public static string ProfileMauPath => Path.Combine(DataDirectory, "profile_mau");
        public static string ProfileRanhPath => Path.Combine(DataDirectory, "profile_ranh");

        public static void EnsureCoreDirectoriesExist()
        {
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(TempDirectory);
            Directory.CreateDirectory(PackagesDirectory);
            Directory.CreateDirectory(ProfilesRootPath);
            Directory.CreateDirectory(SessionsRootPath);
        }

        public static string GetProfilePath(string uid)
        {
            return Path.Combine(ProfilesRootPath, uid);
        }

        public static IEnumerable<string> EnumerateProfileDirectories()
        {
            return Directory.Exists(ProfilesRootPath)
                ? Directory.GetDirectories(ProfilesRootPath)
                : Enumerable.Empty<string>();
        }

        private static string GiaiQuyetDuongDanData()
        {
            string? tuBienMoiTruong = Environment.GetEnvironmentVariable(TenBienMoiTruongDataPath)?.Trim();
            if (!string.IsNullOrWhiteSpace(tuBienMoiTruong))
            {
                return tuBienMoiTruong;
            }

            string duongDanDataNgoaiMacDinh = @"E:\DANG_NHAP_FB_DATA";
            if (OperatingSystem.IsWindows() && Directory.Exists(duongDanDataNgoaiMacDinh))
            {
                return duongDanDataNgoaiMacDinh;
            }

            return Path.Combine(BaseDirectory, "data");
        }
    }
}
