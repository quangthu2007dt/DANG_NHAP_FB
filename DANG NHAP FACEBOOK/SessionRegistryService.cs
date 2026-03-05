using System.Text;
using System.Text.Json;

namespace DANG_NHAP_FACEBOOK
{
    internal static class SessionRegistryService
    {
        private static readonly object SyncRoot = new();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static List<SessionModel> LoadRegistry()
        {
            lock (SyncRoot)
            {
                return LoadRegistryUnsafe();
            }
        }

        public static SessionModel? FindBySessionId(string sessionId)
        {
            lock (SyncRoot)
            {
                return LoadRegistryUnsafe()
                    .FirstOrDefault(session => string.Equals(session.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static List<SessionModel> FindByUid(string uid)
        {
            lock (SyncRoot)
            {
                return LoadRegistryUnsafe()
                    .Where(session => string.Equals(session.Uid, uid, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        public static void UpsertSession(SessionModel session)
        {
            lock (SyncRoot)
            {
                List<SessionModel> sessions = LoadRegistryUnsafe();
                int index = sessions.FindIndex(item => string.Equals(item.SessionId, session.SessionId, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    sessions[index] = session;
                }
                else
                {
                    sessions.Add(session);
                }

                SaveRegistryUnsafe(sessions);
            }
        }

        public static void MarkClosed(string sessionId)
        {
            lock (SyncRoot)
            {
                List<SessionModel> sessions = LoadRegistryUnsafe();
                int index = sessions.FindIndex(item => string.Equals(item.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    return;
                }

                SessionModel session = sessions[index];
                session.Status = SessionStatus.Closed;
                session.ClosedAtUtc = DateTime.UtcNow;
                sessions[index] = session;
                SaveRegistryUnsafe(sessions);
            }
        }

        public static void RemoveSession(string sessionId)
        {
            lock (SyncRoot)
            {
                List<SessionModel> sessions = LoadRegistryUnsafe();
                sessions.RemoveAll(session => string.Equals(session.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
                SaveRegistryUnsafe(sessions);
            }
        }

        private static List<SessionModel> LoadRegistryUnsafe()
        {
            if (!File.Exists(AppPaths.SessionRegistryFilePath))
            {
                return [];
            }

            try
            {
                string json = File.ReadAllText(AppPaths.SessionRegistryFilePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }

                return JsonSerializer.Deserialize<List<SessionModel>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void SaveRegistryUnsafe(List<SessionModel> sessions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AppPaths.SessionRegistryFilePath) ?? AppPaths.DataDirectory);

            string json = JsonSerializer.Serialize(sessions, JsonOptions);
            string tempPath = $"{AppPaths.SessionRegistryFilePath}.tmp";

            File.WriteAllText(tempPath, json, Encoding.UTF8);
            File.Copy(tempPath, AppPaths.SessionRegistryFilePath, true);
            File.Delete(tempPath);
        }
    }
}
