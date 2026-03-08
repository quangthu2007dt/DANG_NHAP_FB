using System.Diagnostics;

namespace Updater.Services
{
    internal static class AppShutdownService
    {
        public static void ChoAppChinhTat(int? processId, Action<UpdateProgressInfo>? baoTienTrinh = null)
        {
            if (processId == null)
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

                baoTienTrinh?.Invoke(new UpdateProgressInfo
                {
                    Message = "Đang chờ app chính tắt..."
                });

                process.WaitForExit(30000);
                if (!process.HasExited)
                {
                    throw new InvalidOperationException("App chinh van chua tat sau 30 giay.");
                }
            }
            catch (ArgumentException)
            {
                return;
            }
        }
    }
}
