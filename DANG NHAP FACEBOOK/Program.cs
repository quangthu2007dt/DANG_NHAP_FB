namespace DANG_NHAP_FACEBOOK
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            DataMigrationService.KhoiTaoCauTrucDuLieu();                                      // Dựng sẵn cấu trúc data chuẩn trước khi bất kỳ form nào bắt đầu đọc file
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
