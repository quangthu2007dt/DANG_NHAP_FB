namespace DANG_NHAP_FACEBOOK
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            DataMigrationService.KhoiTaoCauTrucDuLieu();
            SessionRuntimeService.CleanupOrphanSessions();

            ApplicationConfiguration.Initialize();
            VersionService.KiemTraThongTinPhienBan();
            ManifestService.KiemTraManifestCucBo();

            if (UpdateService.ThuKichHoatCapNhatNeuCo())
            {
                return;
            }

            Application.Run(new Form1());
        }
    }
}
