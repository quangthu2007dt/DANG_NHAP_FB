namespace DANG_NHAP_FACEBOOK
{
    public partial class Form1 : Form
    {
        private readonly string dsFilePath;

        public Form1()
        {
            InitializeComponent();

            dsFilePath = Path.Combine(AppContext.BaseDirectory, "ds.txt");
            DamBaoTonTaiDsTxt();
        }

        private void DamBaoTonTaiDsTxt()
        {
            if (File.Exists(dsFilePath))
            {
                return;
            }

            File.WriteAllText(dsFilePath, string.Empty);
        }
    }
}
