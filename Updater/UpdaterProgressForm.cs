using System.Drawing;

namespace Updater
{
    internal sealed class UpdaterProgressForm : Form
    {
        private readonly Label lblTrangThai;
        private readonly Label lblPhanTram;
        private readonly ProgressBar progressBar;

        public UpdaterProgressForm()
        {
            Text = "Đang cập nhật ứng dụng";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(520, 140);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            lblTrangThai = new Label
            {
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(480, 44),
                Text = "Đang khởi tạo update..."
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 78),
                Size = new Size(480, 24),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            lblPhanTram = new Label
            {
                AutoSize = false,
                Location = new Point(20, 108),
                Size = new Size(480, 20),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.DimGray,
                Text = string.Empty
            };

            Controls.Add(lblTrangThai);
            Controls.Add(progressBar);
            Controls.Add(lblPhanTram);
        }

        public void CapNhatTienTrinh(UpdateProgressInfo thongTin)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => CapNhatTienTrinh(thongTin));
                return;
            }

            lblTrangThai.Text = thongTin.Message;
            lblTrangThai.ForeColor = thongTin.IsError ? Color.Firebrick : SystemColors.ControlText;

            if (thongTin.Percent.HasValue)
            {
                if (progressBar.Style != ProgressBarStyle.Continuous)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

                int giaTri = Math.Max(0, Math.Min(100, thongTin.Percent.Value));
                progressBar.Value = giaTri;
                lblPhanTram.Text = $"{giaTri}%";
            }
            else
            {
                if (progressBar.Style != ProgressBarStyle.Marquee)
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                }

                lblPhanTram.Text = string.Empty;
            }
        }
    }
}
