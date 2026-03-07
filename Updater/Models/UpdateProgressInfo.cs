namespace Updater
{
    internal sealed class UpdateProgressInfo
    {
        public string Message { get; init; } = string.Empty;
        public int? Percent { get; init; }
        public bool IsError { get; init; }
    }
}
