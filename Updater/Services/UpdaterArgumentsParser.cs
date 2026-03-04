namespace Updater.Services
{
    internal static class UpdaterArgumentsParser
    {
        public static UpdaterArguments Parse(string[] args)
        {
            Dictionary<string, string> thamSo = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                string key = args[i];
                if (!key.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;                                                                 // Bỏ qua đối số lạ để parser chỉ bám theo các khóa updater hỗ trợ
                }

                string value = i + 1 < args.Length ? args[i + 1] : string.Empty;
                if (string.IsNullOrWhiteSpace(value) || value.StartsWith("--", StringComparison.Ordinal))
                {
                    thamSo[key] = string.Empty;
                    continue;
                }

                thamSo[key] = value;
                i++;
            }

            string appDirectory = LayGiaTriBatBuoc(thamSo, "--app-dir");
            string manifestPath = LayManifestPath(thamSo, appDirectory);
            string? packagePath = LayPackagePathNeuCo(thamSo);
            int? processId = LayProcessIdNeuCo(thamSo);

            if (!Directory.Exists(appDirectory))
            {
                throw new InvalidOperationException($"Không tìm thấy thư mục app: {appDirectory}");
            }

            if (!File.Exists(manifestPath))
            {
                throw new InvalidOperationException($"Không tìm thấy manifest: {manifestPath}");
            }

            if (!string.IsNullOrWhiteSpace(packagePath) && !File.Exists(packagePath))
            {
                throw new InvalidOperationException($"Không tìm thấy gói cập nhật: {packagePath}");
            }

            return new UpdaterArguments
            {
                AppDirectory = appDirectory,
                ManifestPath = manifestPath,
                PackagePath = packagePath,
                ProcessId = processId
            };
        }

        private static string LayGiaTriBatBuoc(Dictionary<string, string> thamSo, string key)
        {
            if (thamSo.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            throw new InvalidOperationException($"Thiếu tham số bắt buộc: {key}");
        }

        private static string LayManifestPath(Dictionary<string, string> thamSo, string appDirectory)
        {
            if (thamSo.TryGetValue("--manifest", out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            return Path.Combine(appDirectory, "manifest.json");                               // Nếu không truyền manifest thì updater tự lấy manifest nằm cạnh app để dễ test local
        }

        private static string? LayPackagePathNeuCo(Dictionary<string, string> thamSo)
        {
            if (thamSo.TryGetValue("--package", out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            return null;
        }

        private static int? LayProcessIdNeuCo(Dictionary<string, string> thamSo)
        {
            if (!thamSo.TryGetValue("--pid", out string? value) || string.IsNullOrWhiteSpace(value))
            {
                return null;                                                                  // Không truyền pid thì updater coi như app chính đã tắt sẵn
            }

            if (int.TryParse(value, out int pid) && pid > 0)
            {
                return pid;
            }

            throw new InvalidOperationException("Tham số --pid không hợp lệ.");
        }
    }
}
