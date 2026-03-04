using System.Text.Json;

namespace Updater.Services
{
    internal static class UpdaterManifestService
    {
        public static ReleaseManifest DocManifest(string manifestPath)
        {
            try
            {
                string json = File.ReadAllText(manifestPath);                                 // Đọc manifest được truyền vào để updater biết bản phát hành mới nhất là gì
                ReleaseManifest? manifest = JsonSerializer.Deserialize<ReleaseManifest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest != null &&
                    !string.IsNullOrWhiteSpace(manifest.AppName) &&
                    !string.IsNullOrWhiteSpace(manifest.LatestVersion))
                {
                    return manifest;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không đọc được manifest: {ex.Message}");
            }

            throw new InvalidOperationException("Manifest không hợp lệ.");
        }
    }
}
