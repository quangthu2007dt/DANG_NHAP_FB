using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DANG_NHAP_FACEBOOK
{
    internal static class SessionRuntimeService
    {
        public static SessionModel CreateSessionFromTemplate(string uid)
        {
            string sessionId = TaoSessionId(uid);
            string sessionPath = Path.Combine(AppPaths.SessionsRootPath, sessionId);

            if (!Directory.Exists(AppPaths.ProfileMauPath))
            {
                throw new DirectoryNotFoundException("Không tìm thấy profile_mau.");
            }

            Directory.CreateDirectory(AppPaths.SessionsRootPath);
            SaoChepThuMuc(AppPaths.ProfileMauPath, sessionPath);

            SessionModel session = new()
            {
                SessionId = sessionId,
                Uid = uid,
                SessionPath = sessionPath,
                CreatedAtUtc = DateTime.UtcNow,
                Status = SessionStatus.Pending
            };

            SessionRegistryService.UpsertSession(session);
            return session;
        }

        public static Process LaunchChromeForSession(SessionModel session, string chromeExe, string arguments)
        {
            ProcessStartInfo psi = new()
            {
                FileName = chromeExe,
                Arguments = arguments,
                UseShellExecute = true
            };

            Process? process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Không thể khởi chạy Chrome.");
            }

            session.ProcessId = process.Id;
            session.Status = SessionStatus.Running;
            SessionRegistryService.UpsertSession(session);
            _ = TheoDoiVaDonDepPhienKhiChromeThoatAsync(session.SessionId, process.Id);
            return process;
        }

        public static int TryCloseAndCleanupSessionsByUid(string uid)
        {
            List<SessionModel> sessions = SessionRegistryService.FindByUid(uid);
            int soPhienDaXuLy = 0;

            foreach (SessionModel session in sessions)
            {
                if (TryCloseAndCleanupSession(session.SessionId))
                {
                    soPhienDaXuLy++;
                }
            }

            return soPhienDaXuLy;
        }

        public static bool TryCloseAndCleanupSession(string sessionId)
        {
            SessionModel? session = SessionRegistryService.FindBySessionId(sessionId);
            if (session == null)
            {
                return true;
            }

            ThuDongChromeTheoProcessId(session.ProcessId);
            bool daXoa = ThuXoaSessionPath(session.SessionPath);
            SessionRegistryService.MarkClosed(sessionId);
            SessionRegistryService.RemoveSession(sessionId);
            return daXoa;
        }

        public static void CleanupOrphanSessions()
        {
            Directory.CreateDirectory(AppPaths.SessionsRootPath);
            List<SessionModel> sessions = SessionRegistryService.LoadRegistry();
            HashSet<string> idsTrongRegistry = new(StringComparer.OrdinalIgnoreCase);

            foreach (SessionModel session in sessions)
            {
                idsTrongRegistry.Add(session.SessionId);
                if (KiemTraProcessDangSong(session.ProcessId))
                {
                    continue;
                }

                ThuXoaSessionPath(session.SessionPath);
                SessionRegistryService.MarkClosed(session.SessionId);
                SessionRegistryService.RemoveSession(session.SessionId);
            }

            foreach (string sessionDir in Directory.GetDirectories(AppPaths.SessionsRootPath))
            {
                string sessionId = Path.GetFileName(sessionDir);
                if (idsTrongRegistry.Contains(sessionId))
                {
                    continue;
                }

                ThuXoaSessionPath(sessionDir);
            }
        }

        private static async Task TheoDoiVaDonDepPhienKhiChromeThoatAsync(string sessionId, int processId)
        {
            await Task.Run(() =>
            {
                try
                {
                    using Process process = Process.GetProcessById(processId);
                    process.WaitForExit();
                }
                catch
                {
                }

                SessionModel? session = SessionRegistryService.FindBySessionId(sessionId);
                if (session == null)
                {
                    return;
                }

                ThuXoaSessionPath(session.SessionPath);
                SessionRegistryService.MarkClosed(sessionId);
                SessionRegistryService.RemoveSession(sessionId);
            });
        }

        private static void ThuDongChromeTheoProcessId(int? processId)
        {
            if (!processId.HasValue)
            {
                return;
            }

            try
            {
                using Process process = Process.GetProcessById(processId.Value);
                if (process.HasExited)
                {
                    return;
                }

                process.CloseMainWindow();
                if (!process.WaitForExit(1500))
                {
                    process.Kill(true);
                    process.WaitForExit(5000);
                }
            }
            catch
            {
            }
        }

        private static bool KiemTraProcessDangSong(int? processId)
        {
            if (!processId.HasValue)
            {
                return false;
            }

            try
            {
                using Process process = Process.GetProcessById(processId.Value);
                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private static bool ThuXoaSessionPath(string sessionPath)
        {
            if (string.IsNullOrWhiteSpace(sessionPath) || !Directory.Exists(sessionPath))
            {
                return true;
            }

            for (int i = 0; i < 12; i++)
            {
                try
                {
                    DatThuocTinhThuMucVeBinhThuong(sessionPath);
                    Directory.Delete(sessionPath, true);
                    return true;
                }
                catch
                {
                    Thread.Sleep(350);
                }
            }

            try
            {
                using Process process = new();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c attrib -r -s -h \"{sessionPath}\" /s /d & rmdir /s /q \"{sessionPath}\"";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(5000);
                return !Directory.Exists(sessionPath);
            }
            catch
            {
                return false;
            }
        }

        private static void DatThuocTinhThuMucVeBinhThuong(string directoryPath)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            foreach (string subDir in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(subDir, FileAttributes.Normal);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
        }

        private static void SaoChepThuMuc(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);

            foreach (string filePath in Directory.GetFiles(sourcePath))
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFile = Path.Combine(destinationPath, fileName);
                File.Copy(filePath, destinationFile, true);
            }

            foreach (string directoryPath in Directory.GetDirectories(sourcePath))
            {
                string directoryName = Path.GetFileName(directoryPath);
                string destinationSubDir = Path.Combine(destinationPath, directoryName);
                SaoChepThuMuc(directoryPath, destinationSubDir);
            }
        }

        private static string TaoSessionId(string uid)
        {
            string uidRutGon = new string(uid.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(uidRutGon))
            {
                uidRutGon = "uid";
            }

            uidRutGon = uidRutGon.Length > 18 ? uidRutGon[..18] : uidRutGon;
            string randomPart = Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant();
            return $"{DateTime.Now:yyyyMMdd_HHmmss}_{uidRutGon}_{randomPart}";
        }
    }
}
