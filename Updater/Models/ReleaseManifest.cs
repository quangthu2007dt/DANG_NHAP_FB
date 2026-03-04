namespace Updater
{
    internal sealed class ReleaseManifest
    {
        public string AppName { get; set; } = string.Empty;
        public string Channel { get; set; } = "stable";
        public string LatestVersion { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public string PackageFileName { get; set; } = string.Empty;
        public string PackageUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
