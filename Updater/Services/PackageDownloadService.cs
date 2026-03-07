using System.IO.Compression;

namespace Updater.Services
{
    internal static class PackageDownloadService
    {
        private static readonly TimeSpan ThoiGianToiDaTaiGoi = TimeSpan.FromMinutes(30);

        public static string LayHoacTaiGoiCapNhat(UpdaterArguments thamSo, ReleaseManifest manifest, Action<UpdateProgressInfo>? baoTienTrinh = null)
        {
            if (!string.IsNullOrWhiteSpace(thamSo.PackagePath))
            {
                baoTienTrinh?.Invoke(new UpdateProgressInfo
                {
                    Message = "Dang dung goi cap nhat da duoc chi dinh san."
                });

                return thamSo.PackagePath;
            }

            string packagesDirectory = Path.Combine(thamSo.AppDirectory, "packages");
            Directory.CreateDirectory(packagesDirectory);

            string packageFileName = LayTenGoiCapNhat(manifest);
            string packagePath = Path.Combine(packagesDirectory, packageFileName);

            if (File.Exists(packagePath))
            {
                baoTienTrinh?.Invoke(new UpdateProgressInfo
                {
                    Message = "Da tim thay goi cap nhat trong thu muc packages.",
                    Percent = 100
                });

                return packagePath;
            }

            if (string.IsNullOrWhiteSpace(manifest.PackageUrl))
            {
                throw new InvalidOperationException($"Khong tim thay goi cap nhat tai {packagePath} va manifest chua co packageUrl.");
            }

            Uri packageUri = TaoUriGoiCapNhat(thamSo, manifest);
            using HttpClient httpClient = new()
            {
                Timeout = ThoiGianToiDaTaiGoi
            };

            baoTienTrinh?.Invoke(new UpdateProgressInfo
            {
                Message = "Dang tai goi cap nhat tu GitHub..."
            });

            using HttpResponseMessage response = httpClient.GetAsync(packageUri, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            using Stream remoteStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using FileStream fileStream = File.Create(packagePath);
            SaoChepStreamCoTienTrinh(remoteStream, fileStream, response.Content.Headers.ContentLength, baoTienTrinh);
            return packagePath;
        }

        public static string GiaiNenGoiCapNhat(string appDirectory, string packagePath, string version, Action<UpdateProgressInfo>? baoTienTrinh = null)
        {
            string tempDirectory = Path.Combine(appDirectory, "temp");
            Directory.CreateDirectory(tempDirectory);

            string tenThuMucTam = $"update_{version}_{DateTime.Now:yyyyMMdd_HHmmss}";
            string duongDanGiaiNen = Path.Combine(tempDirectory, tenThuMucTam);

            if (Directory.Exists(duongDanGiaiNen))
            {
                Directory.Delete(duongDanGiaiNen, true);
            }

            Directory.CreateDirectory(duongDanGiaiNen);
            baoTienTrinh?.Invoke(new UpdateProgressInfo
            {
                Message = "Dang giai nen goi cap nhat..."
            });

            ZipFile.ExtractToDirectory(packagePath, duongDanGiaiNen, true);

            string[] thuMucCon = Directory.GetDirectories(duongDanGiaiNen);
            string[] fileCon = Directory.GetFiles(duongDanGiaiNen);

            if (thuMucCon.Length == 1 && fileCon.Length == 0)
            {
                return thuMucCon[0];
            }

            return duongDanGiaiNen;
        }

        private static void SaoChepStreamCoTienTrinh(Stream remoteStream, Stream fileStream, long? tongSoByte, Action<UpdateProgressInfo>? baoTienTrinh)
        {
            byte[] buffer = new byte[256 * 1024];
            long daTai = 0;
            int? phanTramCuoi = null;

            while (true)
            {
                int soByteDoc = remoteStream.Read(buffer, 0, buffer.Length);
                if (soByteDoc <= 0)
                {
                    break;
                }

                fileStream.Write(buffer, 0, soByteDoc);
                daTai += soByteDoc;

                if (tongSoByte.HasValue && tongSoByte.Value > 0)
                {
                    int phanTram = (int)Math.Min(100, daTai * 100L / tongSoByte.Value);
                    if (phanTramCuoi != phanTram)
                    {
                        phanTramCuoi = phanTram;
                        baoTienTrinh?.Invoke(new UpdateProgressInfo
                        {
                            Message = $"Dang tai goi cap nhat... {DinhDangDungLuong(daTai)} / {DinhDangDungLuong(tongSoByte.Value)}",
                            Percent = phanTram
                        });
                    }
                }
                else
                {
                    baoTienTrinh?.Invoke(new UpdateProgressInfo
                    {
                        Message = $"Dang tai goi cap nhat... {DinhDangDungLuong(daTai)}"
                    });
                }
            }
        }

        private static string DinhDangDungLuong(long soByte)
        {
            string[] donVi = ["B", "KB", "MB", "GB"];
            double giaTri = soByte;
            int chiSo = 0;

            while (giaTri >= 1024 && chiSo < donVi.Length - 1)
            {
                giaTri /= 1024;
                chiSo++;
            }

            return $"{giaTri:0.##} {donVi[chiSo]}";
        }

        private static string LayTenGoiCapNhat(ReleaseManifest manifest)
        {
            if (!string.IsNullOrWhiteSpace(manifest.PackageFileName))
            {
                return manifest.PackageFileName.Trim();
            }

            throw new InvalidOperationException("Manifest chua co packageFileName hop le.");
        }

        private static Uri TaoUriGoiCapNhat(UpdaterArguments thamSo, ReleaseManifest manifest)
        {
            if (Uri.TryCreate(manifest.PackageUrl, UriKind.Absolute, out Uri? uriTuyetDoi))
            {
                return uriTuyetDoi;
            }

            string duongDanLocal = Path.Combine(thamSo.AppDirectory, manifest.PackageUrl);
            return new Uri(duongDanLocal);
        }
    }
}
