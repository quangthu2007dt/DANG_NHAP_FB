using System.Diagnostics;

namespace Updater.Services
{
    internal static class AppShutdownService
    {
        public static void ChoAppChinhTat(int? processId)
        {
            if (processId == null)
            {
                return;                                                                       // Không có pid thì updater không cần chờ app chính
            }

            try
            {
                using Process process = Process.GetProcessById(processId.Value);
                if (process.HasExited)
                {
                    return;                                                                   // App chính đã tắt rồi thì đi tiếp ngay
                }

                process.WaitForExit(30000);                                                   // Chờ tối đa 30 giây để app chính đóng hẳn trước khi updater đi tiếp
                if (!process.HasExited)
                {
                    throw new InvalidOperationException("App chính vẫn chưa tắt sau 30 giây.");
                }
            }
            catch (ArgumentException)
            {
                return;                                                                       // Không còn tìm thấy pid thì coi như app chính đã tắt
            }
        }
    }
}
