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
            UpdateService.HienThongBaoCapNhatThanhCongNeuCo();                               // Lan mo dau sau update se thong bao da len ban moi, sau do marker tu xoa

            if (UpdateService.ThuKichHoatCapNhatNeuCo())
            {
                return;
            }

            Application.Run(new Form1());
        }
    }
}
