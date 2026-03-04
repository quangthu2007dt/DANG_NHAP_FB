namespace DANG_NHAP_FACEBOOK
{
    internal static class AppPaths
    {
        public static string BaseDirectory => AppContext.BaseDirectory;
        public static string DataDirectory => Path.Combine(BaseDirectory, "data");
        public static string LogsDirectory => Path.Combine(BaseDirectory, "logs");
        public static string TempDirectory => Path.Combine(BaseDirectory, "temp");
        public static string PackagesDirectory => Path.Combine(BaseDirectory, "packages");
        public static string ProfilesRootPath => Path.Combine(DataDirectory, "profiles");

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
    }
}
