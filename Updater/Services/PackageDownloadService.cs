using System.IO.Compression;

namespace Updater.Services
{
    internal static class PackageDownloadService
    {
        public static string LayHoacTaiGoiCapNhat(UpdaterArguments thamSo, ReleaseManifest manifest)
        {
            if (!string.IsNullOrWhiteSpace(thamSo.PackagePath))
            {
                return thamSo.PackagePath;                                                    // Khi test local có thể truyền thẳng zip vào updater mà không cần tải
            }

            string packagesDirectory = Path.Combine(thamSo.AppDirectory, "packages");
            Directory.CreateDirectory(packagesDirectory);

            string packageFileName = LayTenGoiCapNhat(manifest);
            string packagePath = Path.Combine(packagesDirectory, packageFileName);

            if (File.Exists(packagePath))
            {
                return packagePath;                                                           // Nếu gói đã có sẵn trong packages thì dùng luôn để tiết kiệm thời gian
            }

            if (string.IsNullOrWhiteSpace(manifest.PackageUrl))
            {
                throw new InvalidOperationException($"Không tìm thấy gói cập nhật tại {packagePath} và manifest chưa có packageUrl.");
            }

            Uri packageUri = TaoUriGoiCapNhat(thamSo, manifest);
            using HttpClient httpClient = new();
            using HttpResponseMessage response = httpClient.GetAsync(packageUri).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            using Stream remoteStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using FileStream fileStream = File.Create(packagePath);
            remoteStream.CopyTo(fileStream);                                                  // Tải gói update về packages để updater xử lý theo luồng thống nhất
            return packagePath;
        }

        public static string GiaiNenGoiCapNhat(string appDirectory, string packagePath, string version)
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
            ZipFile.ExtractToDirectory(packagePath, duongDanGiaiNen, true);                   // Giải nén toàn bộ package ra thư mục tạm để chuẩn bị thay file

            string[] thuMucCon = Directory.GetDirectories(duongDanGiaiNen);
            string[] fileCon = Directory.GetFiles(duongDanGiaiNen);

            if (thuMucCon.Length == 1 && fileCon.Length == 0)
            {
                return thuMucCon[0];                                                          // Nếu zip bọc thêm một thư mục gốc thì dùng luôn thư mục con đó làm nguồn thay file
            }

            return duongDanGiaiNen;
        }

        private static string LayTenGoiCapNhat(ReleaseManifest manifest)
        {
            if (!string.IsNullOrWhiteSpace(manifest.PackageFileName))
            {
                return manifest.PackageFileName.Trim();
            }

            throw new InvalidOperationException("Manifest chưa có packageFileName hợp lệ.");
        }

        private static Uri TaoUriGoiCapNhat(UpdaterArguments thamSo, ReleaseManifest manifest)
        {
            if (Uri.TryCreate(manifest.PackageUrl, UriKind.Absolute, out Uri? uriTuyetDoi))
            {
                return uriTuyetDoi;
            }

            string duongDanLocal = Path.Combine(thamSo.AppDirectory, manifest.PackageUrl);    // Cho phép manifest local dùng đường dẫn tương đối để test trước khi có server thật
            return new Uri(duongDanLocal);
        }
    }
}
