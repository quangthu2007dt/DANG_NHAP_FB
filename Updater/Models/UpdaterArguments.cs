namespace Updater
{
    internal sealed class UpdaterArguments
    {
        public string AppDirectory { get; init; } = string.Empty;
        public string ManifestPath { get; init; } = string.Empty;
        public int? ProcessId { get; init; }
    }
}
