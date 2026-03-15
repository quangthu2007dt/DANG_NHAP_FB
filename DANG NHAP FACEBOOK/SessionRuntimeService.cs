using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DANG_NHAP_FACEBOOK
{
    internal static class SessionRuntimeService
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;
        private const uint GW_OWNER = 4;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

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

        public static int ThuDatTrangThaiAnHienChoTatCaChromeDangChay(bool hienChrome)
        {
            List<SessionModel> sessions = SessionRegistryService.LoadRegistry();
            HashSet<int> processIdsDaXuLy = new();
            int soCuaSoDaXuLy = 0;

            foreach (SessionModel session in sessions)
            {
                if (!session.ProcessId.HasValue || session.Status != SessionStatus.Running)
                {
                    continue;
                }

                int processId = session.ProcessId.Value;
                if (!processIdsDaXuLy.Add(processId))
                {
                    continue;
                }

                if (ThuDatTrangThaiAnHienChromeTheoProcessId(processId, hienChrome))
                {
                    soCuaSoDaXuLy++;
                }
            }

            return soCuaSoDaXuLy;
        }

        public static bool ThuDatTrangThaiAnHienChromeTheoProcessId(int? processId, bool hienChrome)
        {
            if (!processId.HasValue)
            {
                return false;
            }

            IntPtr cuaSoChrome = TimCuaSoChromeTheoProcessId(processId.Value);
            if (cuaSoChrome == IntPtr.Zero)
            {
                return false;
            }

            if (hienChrome)
            {
                DuaCuaSoChromeVeManHinhChinh(cuaSoChrome);
                ShowWindow(cuaSoChrome, SW_RESTORE);
                ShowWindow(cuaSoChrome, SW_SHOW);
                SetForegroundWindow(cuaSoChrome);
                return true;
            }

            ShowWindow(cuaSoChrome, SW_HIDE);
            return true;
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

        private static IntPtr TimCuaSoChromeTheoProcessId(int processId)
        {
            try
            {
                using Process process = Process.GetProcessById(processId);
                for (int i = 0; i < 8; i++)
                {
                    process.Refresh();
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process.MainWindowHandle;
                    }

                    Thread.Sleep(150);
                }
            }
            catch
            {
            }

            IntPtr ketQua = IntPtr.Zero;
            EnumWindows((hWnd, _) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid != processId)
                {
                    return true;
                }

                if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                {
                    return true;                                                             // Chỉ lấy cửa sổ top-level thật của tiến trình Chrome để tránh đụng popup con
                }

                ketQua = hWnd;
                return false;
            }, IntPtr.Zero);

            return ketQua;
        }

        private static void DuaCuaSoChromeVeManHinhChinh(IntPtr cuaSoChrome)
        {
            Rectangle vungLamViec = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1200, 900);
            int chieuRongCuaSo = Math.Min(vungLamViec.Width, Math.Max(1000, (int)(vungLamViec.Width * 0.75)));
            int chieuCaoCuaSo = Math.Min(vungLamViec.Height, Math.Max(760, (int)(vungLamViec.Height * 0.88)));
            int viTriX = vungLamViec.Left;
            int viTriY = Math.Max(vungLamViec.Top, vungLamViec.Top + (vungLamViec.Height - chieuCaoCuaSo) / 2);

            MoveWindow(cuaSoChrome, viTriX, viTriY, chieuRongCuaSo, chieuCaoCuaSo, true);    // Khi hiện lại từ mode ẩn thì kéo hẳn cửa sổ về vùng nhìn thấy thay vì chỉ ShowWindow trên vị trí off-screen cũ
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
