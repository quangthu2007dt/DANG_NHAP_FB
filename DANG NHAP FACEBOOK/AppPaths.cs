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

        public static string LegacyDsFilePath => Path.Combine(BaseDirectory, "ds.txt");
        public static string LegacyUserAgentsFilePath => Path.Combine(BaseDirectory, "user_agents.txt");
        public static string LegacyUserAgentDangDungFilePath => Path.Combine(BaseDirectory, "ua_dang_dung.txt");
        public static string LegacyProfileMauPath => Path.Combine(BaseDirectory, "profile_mau");
        public static string LegacyProfileRanhPath => Path.Combine(BaseDirectory, "profile_ranh");

        public static void EnsureCoreDirectoriesExist()
        {
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(TempDirectory);
            Directory.CreateDirectory(PackagesDirectory);
            Directory.CreateDirectory(ProfilesRootPath);
        }

        public static string ResolveDsFilePath()
        {
            return File.Exists(DsFilePath) ? DsFilePath : LegacyDsFilePath;
        }

        public static string ResolveUserAgentsFilePath()
        {
            return File.Exists(UserAgentsFilePath) ? UserAgentsFilePath : LegacyUserAgentsFilePath;
        }

        public static string ResolveUserAgentDangDungFilePath()
        {
            return File.Exists(UserAgentDangDungFilePath) ? UserAgentDangDungFilePath : UserAgentDangDungFilePath;
        }

        public static string ResolveProfileMauPath()
        {
            if (Directory.Exists(ProfileMauPath))
            {
                return ProfileMauPath;
            }

            return Directory.Exists(LegacyProfileMauPath) ? LegacyProfileMauPath : ProfileMauPath;
        }

        public static string ResolveProfileRanhPath()
        {
            if (Directory.Exists(ProfileRanhPath))
            {
                return ProfileRanhPath;
            }

            return Directory.Exists(LegacyProfileRanhPath) ? LegacyProfileRanhPath : ProfileRanhPath;
        }

        public static string ResolveProfilePath(string uid)
        {
            string profilePathMoi = Path.Combine(ProfilesRootPath, uid);
            if (Directory.Exists(profilePathMoi))
            {
                return profilePathMoi;
            }

            string profilePathCu = Path.Combine(BaseDirectory, uid);
            if (Directory.Exists(profilePathCu))
            {
                return profilePathCu;
            }

            return profilePathMoi;
        }

        public static IEnumerable<string> EnumerateProfileDirectories()
        {
            HashSet<string> daGap = new(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(ProfilesRootPath))
            {
                foreach (string directory in Directory.GetDirectories(ProfilesRootPath))
                {
                    if (daGap.Add(Path.GetFullPath(directory)))
                    {
                        yield return directory;
                    }
                }
            }

            foreach (string directory in Directory.GetDirectories(BaseDirectory))
            {
                string tenThuMuc = Path.GetFileName(directory);
                if (string.Equals(tenThuMuc, "data", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tenThuMuc, "logs", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tenThuMuc, "temp", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tenThuMuc, "packages", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (daGap.Add(Path.GetFullPath(directory)))
                {
                    yield return directory;
                }
            }
        }
    }
}
