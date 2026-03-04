# CHECKLIST CẢI TỔ APP ĐĂNG NHẬP FACEBOOK ĐỂ HƯỚNG TỚI AUTO UPDATE

## 1. Mục đích của checklist

Checklist này dùng để tách công việc cải tổ app `DANG NHAP FACEBOOK` thành từng bước rõ ràng, để có thể làm lần lượt, kiểm tra lần lượt, và không bị sửa tràn lan.

Checklist này áp dụng trực tiếp cho source hiện tại trong:

- `E:\BT_Python_Tiem\DANG NHAP FACEBOOK`

## 2. Hiện trạng kỹ thuật cần nhớ

### Đã xác nhận trong code hiện tại

- App là WinForms `.NET 9` trong `DANG NHAP FACEBOOK.csproj`.
- Entry point hiện tại rất gọn, chỉ mở `Form1`.
- Dữ liệu đang dùng `AppContext.BaseDirectory`.
- `Form1.cs` đang trỏ trực tiếp tới:
  - `ds.txt`
  - `user_agents.txt`
  - `ua_dang_dung.txt`
  - `profile_mau`
  - `profile_ranh`
- Các thư mục profile theo UID cũng đang nằm cạnh file chạy app.

### Kết luận

App chưa sẵn sàng cho auto-update an toàn dữ liệu.

Lý do:

- file chương trình và file dữ liệu đang bị trộn cùng chỗ
- chưa có version phát hành
- chưa có manifest
- chưa có updater riêng

## 3. Thứ tự triển khai khuyến nghị

Không làm tất cả cùng lúc. Làm theo 5 giai đoạn.

---

## GIAI ĐOẠN 1 - CHUẨN HÓA DỮ LIỆU VÀ ĐƯỜNG DẪN

### Mục tiêu

Tách dữ liệu ra khỏi khu vực file chương trình để tạo nền tảng cho auto-update.

### Việc cần làm

- Tạo một lớp mới để quản lý đường dẫn.
- Chốt cấu trúc thư mục chuẩn:
  - `data\`
  - `logs\`
  - `temp\`
  - `packages\`
- Chuyển các đường dẫn hiện tại từ `AppContext.BaseDirectory + tên file` thành đường dẫn theo `data\`.

### Danh sách đổi cụ thể

- `ds.txt` -> `data\ds.txt`
- `user_agents.txt` -> `data\user_agents.txt`
- `ua_dang_dung.txt` -> `data\ua_dang_dung.txt`
- `profile_mau` -> `data\profile_mau`
- `profile_ranh` -> `data\profile_ranh`
- các profile UID -> `data\profiles\<UID>`

### Đầu việc code

- Tạo file mới, dự kiến:
  - `DANG NHAP FACEBOOK\AppPaths.cs`
- Trong file này khai báo:
  - `BaseDirectory`
  - `DataDirectory`
  - `LogsDirectory`
  - `TempDirectory`
  - `PackagesDirectory`
  - `DsFilePath`
  - `UserAgentsFilePath`
  - `UserAgentDangDungFilePath`
  - `ProfileMauPath`
  - `ProfileRanhPath`
  - `ProfilesRootPath`

### Kiểm tra đạt

- App vẫn chạy được.
- App vẫn đọc và ghi được `ds.txt`.
- App vẫn tạo profile mới.
- App vẫn mở lại profile cũ.
- Không còn hardcode `ds.txt`, `user_agents.txt`, `profile_mau`, `profile_ranh` trực tiếp trong `Form1.cs`.

---

## GIAI ĐOẠN 2 - MIGRATION TỪ BẢN CŨ SANG CẤU TRÚC MỚI

### Mục tiêu

Cho phép bản clean đang có dữ liệu ở cạnh exe được tự động chuyển sang cấu trúc mới mà không mất dữ liệu.

### Việc cần làm

- Thêm bước migration khi app khởi động.
- Kiểm tra dữ liệu cũ ở cạnh exe.
- Nếu `data\` chưa có thì chuyển dữ liệu cũ vào.
- Nếu `data\` đã có rồi thì không ghi đè.

### Danh sách cần migrate

- `ds.txt`
- `user_agents.txt`
- `ua_dang_dung.txt`
- `profile_mau`
- `profile_ranh`
- các thư mục profile cũ nằm cạnh exe

### Quy tắc migration

- Chỉ chạy migration 1 lần khi cần.
- Không xóa file cũ trước khi copy xong.
- Nếu copy lỗi thì dừng và báo người dùng.
- Ghi log migration.

### Đầu việc code

- Tạo file mới, dự kiến:
  - `DANG NHAP FACEBOOK\DataMigrationService.cs`
- Gọi migration ngay đầu chương trình, trước khi mở `Form1`.

### Kiểm tra đạt

- Bản cũ nâng cấp lên bản mới không mất dữ liệu.
- Nếu thư mục `data\` đã tồn tại thì app không migrate lặp lại.
- Các profile cũ vẫn đọc được sau migration.

---

## GIAI ĐOẠN 3 - CHUẨN HÓA VERSION VÀ RELEASE

### Mục tiêu

Tạo khả năng phát hành bản mới có version rõ ràng.

### Việc cần làm

- Chọn cách gắn version cho app.
- Thêm thông tin version vào project.
- Tạo file version/manifest local.

### Khuyến nghị

- Dùng version theo dạng:
  - `2026.03.04.1`
  - hoặc `1.0.0`
- Thêm vào `csproj`:
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`
  - `InformationalVersion`

### Thêm file release metadata

- Tạo file:
  - `version.json`

### Nội dung tối thiểu của `version.json`

- `appName`
- `version`
- `releaseDate`
- `channel`

### Kiểm tra đạt

- File build ra hiện đúng version.
- App đọc được version hiện tại.
- Có thể dùng version này để so sánh với bản mới trên server.

---

## GIAI ĐOẠN 4 - TẠO UPDATER RIÊNG

### Mục tiêu

Tạo `Updater.exe` riêng để cập nhật file chương trình mà không đụng vào dữ liệu.

### Việc cần làm

- Tạo project mới trong solution:
  - `Updater`
- `Updater.exe` nhận tham số:
  - đường dẫn app
  - process id hoặc tên exe cần chờ thoát
  - url manifest
- Tải manifest
- So sánh version
- Tải file zip release
- Giải nén vào thư mục tạm
- Ghi đè file chương trình
- Bỏ qua `data\`, `logs\`, `temp\`, `packages\`
- Mở lại app

### Quy tắc updater

- Không được ghi đè `data\`
- Không được xóa `data\`
- Không được xóa log đang cần cho debug nếu không cần
- Nếu update lỗi phải báo rõ và không làm mất dữ liệu

### File dự kiến thêm mới

- `Updater\Updater.csproj`
- `Updater\Program.cs`
- `Updater\Services\ManifestService.cs`
- `Updater\Services\PackageDownloadService.cs`
- `Updater\Services\FileReplaceService.cs`

### Kiểm tra đạt

- Updater có thể chạy độc lập.
- Updater đợi app tắt rồi mới thay file.
- Updater cập nhật xong app mở lại được.
- Dữ liệu trong `data\` còn nguyên.

---

## GIAI ĐOẠN 5 - NỐI APP CHÍNH VỚI HỆ THỐNG AUTO UPDATE

### Mục tiêu

Cho app tự kiểm tra và kích hoạt updater.

### Việc cần làm

- Thêm `UpdateService` vào app chính.
- Khi app mở:
  - đọc version hiện tại
  - tải manifest trên server
  - nếu có bản mới thì hỏi người dùng hoặc tự động gọi updater
- Thêm nút menu:
  - `Kiểm tra cập nhật`

### Hành vi tối thiểu

- Hiện version hiện tại
- Báo khi có bản mới
- Cho phép cập nhật ngay
- Đóng app an toàn trước khi updater thay file

### Kiểm tra đạt

- App báo đúng khi có bản mới
- App không báo sai khi đang ở bản mới nhất
- Gọi updater thành công

---

## 4. Quy trình phát hành mục tiêu sau khi hoàn thành

Quy trình sau này sẽ là:

1. Sửa code trong thư mục gốc phát triển.
2. Test xong thì tăng version.
3. Build release.
4. Tạo zip release.
5. Cập nhật `manifest.json`.
6. Đẩy release lên nơi phát hành.
7. Mỗi bản clean tự nhận bản mới và auto-update.

Không còn quy trình:

- copy tay `Form1.cs`
- copy tay `Form1.Designer.cs`
- copy tay `Form1.resx`

## 5. Chốt danh sách file hiện tại cần đổi sớm nhất

Trong app hiện tại, những nơi cần ưu tiên đổi đầu tiên là:

- `DANG NHAP FACEBOOK\Form1.cs`
- `DANG NHAP FACEBOOK\Program.cs`
- thêm mới:
  - `DANG NHAP FACEBOOK\AppPaths.cs`
  - `DANG NHAP FACEBOOK\DataMigrationService.cs`

## 6. Tiêu chí nghiệm thu cho bản cải tổ đầu tiên

Bản cải tổ đầu tiên được xem là đạt khi:

- App chạy được trên cấu trúc `data\`
- Bản cũ được migrate sang `data\`
- Không mất `ds.txt`
- Không mất user-agent
- Không mất profile
- Logic hiện tại của nút `Next`, `Chạy`, `Xóa` vẫn hoạt động

## 7. Cách làm tiếp theo

Thứ tự đúng cho các bước tiếp theo:

1. Làm xong Giai đoạn 1
2. Test ổn định
3. Làm Giai đoạn 2
4. Test trên một bản clean thật
5. Mới bắt đầu version và updater

## 8. Ghi chú quan trọng

Auto-update chuyên nghiệp không bắt đầu từ `Updater.exe`.

Nó bắt đầu từ:

- cấu trúc thư mục đúng
- tách dữ liệu đúng
- migration đúng

Nếu 3 phần này chưa xong mà đã viết updater thì updater sẽ dễ phá dữ liệu.
