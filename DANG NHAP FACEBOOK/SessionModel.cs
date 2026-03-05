namespace DANG_NHAP_FACEBOOK
{
    internal sealed class SessionModel
    {
        public string SessionId { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public string SessionPath { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAtUtc { get; set; }
        public string Status { get; set; } = SessionStatus.Pending;
    }

    internal static class SessionStatus
    {
        public const string Pending = "Pending";
        public const string Running = "Running";
        public const string Closed = "Closed";
    }
}
