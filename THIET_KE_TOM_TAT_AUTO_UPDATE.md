# THIẾT KẾ TÓM TẮT AUTO UPDATE CHO APP DESKTOP

## 1. Mục tiêu tài liệu

Tài liệu này dùng để chốt hướng cải tổ app `DANG NHAP FACEBOOK` từ cách cập nhật thủ công sang mô hình `auto-update` an toàn dữ liệu, để sau này có thể áp dụng lại cho các app desktop khác.

Mục tiêu cuối cùng:

- Không còn phải copy tay `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`.
- Mỗi bản clean có thể tự kiểm tra và cập nhật bản mới.
- Dữ liệu đang dùng trên máy clean không bị mất khi cập nhật.
- Có một cấu trúc thư mục ổn định để dùng chung cho các app về sau.

## 2. Hiện trạng của app

App hiện tại là WinForms `.NET`, trong đó dữ liệu đang nằm ngay cạnh file chạy app.

Ví dụ:

- `ds.txt`
- `user_agents.txt`
- `ua_dang_dung.txt`
- `profile_mau`
- `profile_ranh`
- các thư mục profile đặt theo UID

Code hiện tại đang lấy dữ liệu bằng `AppContext.BaseDirectory`, nghĩa là vừa file chương trình vừa file dữ liệu cùng nằm chung một khu vực.

Hệ quả:

- Nếu cập nhật cả thư mục app thì dễ ghi đè nhầm dữ liệu.
- Nếu tiếp tục cập nhật thủ công thì tốn thời gian và dễ sót file.
- Cách copy 3 file form chỉ dùng được cho source phát triển, không phải mô hình phát hành chuyên nghiệp.

## 3. Bài học rút ra từ cách đang làm

Cách copy tay 3 file trước đây là hợp lý trong giai đoạn học và làm cơ bản, vì:

- Bạn mới sửa phần giao diện và logic trong form.
- Dữ liệu clean không mất vì bạn không đụng vào file dữ liệu.
- Bạn chưa có cơ chế build, đóng gói và phát hành bản mới.

Tuy nhiên, về kỹ thuật thì đây không phải cơ chế update thật sự.

Đây chỉ là:

- đồng bộ source để tiếp tục lập trình

Không phải là:

- cập nhật file chạy trên môi trường clean
- kiểm soát version
- phát hành bản mới cho nhiều bản clean

## 4. Nguyên tắc thiết kế mới

Từ nay trở đi, mọi app desktop nên tách thành 2 nhóm:

### 4.1. Nhóm file chương trình

Nhóm này được phép cập nhật:

- `App.exe`
- `Updater.exe`
- `.dll`
- `.json`
- resource
- file cấu hình phát hành

### 4.2. Nhóm file dữ liệu

Nhóm này không được ghi đè khi update:

- file txt người dùng đang dùng
- profile Chrome
- cache nghiệp vụ cần giữ
- log
- setting theo máy
- dữ liệu tạo ra trong quá trình vận hành

## 5. Cấu trúc thư mục mục tiêu

App sau khi cải tổ nên có cấu trúc chuẩn như sau:

```text
TEN_APP\
  DANG NHAP FACEBOOK.exe
  Updater.exe
  version.json
  *.dll
  data\
    ds.txt
    user_agents.txt
    ua_dang_dung.txt
    profile_mau\
    profile_ranh\
    profiles\
      1000...\
      1001...\
  logs\
  temp\
  packages\
```

Ý nghĩa:

- Thư mục gốc chỉ chứa file chương trình và file phục vụ update.
- Toàn bộ dữ liệu người dùng đưa vào `data\`.
- Log tách riêng để dễ dọn dẹp và debug.
- Gói cập nhật tải về tạm thời đưa vào `packages\` hoặc `temp\`.

## 6. Quy ước chuẩn cho tất cả app sau này

Để tránh lệch cấu trúc giữa các app, nên thống nhất các quy ước sau:

- `data\` là nơi chứa dữ liệu nghiệp vụ cần giữ.
- `logs\` là nơi chứa log.
- `temp\` là nơi chứa file tạm.
- `packages\` là nơi lưu gói update tải về.
- file version dùng `version.json` hoặc `manifest.json`.
- mọi app desktop đều có `Updater.exe` riêng.

Nếu giữ đúng quy ước này, các app sau này sẽ:

- dễ phát triển hơn
- dễ build release hơn
- dễ auto-update hơn
- an toàn dữ liệu hơn trên mọi máy

## 7. Hướng đổi code trong app hiện tại

Cần đổi dần từ:

```csharp
Path.Combine(AppContext.BaseDirectory, "ds.txt")
```

thành:

```csharp
Path.Combine(AppContext.BaseDirectory, "data", "ds.txt")
```

Tương tự với:

- `user_agents.txt`
- `ua_dang_dung.txt`
- `profile_mau`
- `profile_ranh`
- các profile UID

Nên tạo một lớp trung tâm để quản lý đường dẫn, ví dụ:

- `AppPaths`
- `StoragePaths`

Lớp này sẽ trả về các đường dẫn chuẩn:

- `DataDirectory`
- `LogsDirectory`
- `PackagesDirectory`
- `DsFilePath`
- `UserAgentsFilePath`
- `ProfileMauPath`
- `ProfileRanhPath`
- `ProfilesRootPath`

Lợi ích:

- dễ đổi cấu trúc sau này
- tránh hardcode lặp lại nhiều nơi
- giảm nguy cơ sai đường dẫn

## 8. Cơ chế auto-update mục tiêu

Mô hình khuyến nghị:

### 8.1. App chính

Nhiệm vụ:

- đọc version hiện tại
- kiểm tra bản mới trên nguồn phát hành
- nếu có bản mới thì gọi `Updater.exe`
- thoát app để updater thay file

### 8.2. Updater.exe

Nhiệm vụ:

- chờ app chính tắt hẳn
- tải gói cập nhật mới
- giải nén vào thư mục tạm
- ghi đè file chương trình
- bỏ qua `data\`, `logs\`, `temp\` nếu cần
- mở lại app sau khi cập nhật xong

### 8.3. Nguồn phát hành

Khuyến nghị:

- GitHub giữ source và release
- mỗi bản release có file zip build sẵn
- có `manifest.json` để updater đọc version mới nhất

## 9. Quy trình phát hành chuẩn

Quy trình mục tiêu:

1. Lập trình trên thư mục gốc phát triển.
2. Test xong thì tăng version.
3. Build ra gói release.
4. Tạo `manifest.json` cho bản mới.
5. Đưa file zip và manifest lên nơi phát hành.
6. Mỗi bản clean tự đọc manifest.
7. Nếu thấy version mới thì tự update.

Từ đây về sau, thao tác phát hành sẽ là:

- build
- đăng release
- để app tự cập nhật

Không còn là:

- copy tay từng file form
- nhớ xem thiếu file nào
- chạy đi chạy lại để thay file thủ công

## 10. Migration từ bản cũ sang cấu trúc mới

Đây là phần rất quan trọng.

Vì bản cũ đang lưu dữ liệu ngay cạnh exe, bản mới phải có bước migration lần đầu:

1. Khi app mới chạy lần đầu, kiểm tra xem có `ds.txt` nằm cạnh exe hay không.
2. Nếu có và `data\ds.txt` chưa tồn tại thì tự chuyển vào `data\`.
3. Nếu có `profile_mau`, `profile_ranh`, hoặc các thư mục profile cũ thì chuyển vào `data\`.
4. Nếu dữ liệu đã tồn tại trong `data\` thì không ghi đè.
5. Ghi log migration để dễ kiểm tra.

Mục tiêu:

- người dùng nâng cấp lên bản mới mà không mất dữ liệu cũ
- không cần thao tác tay trên mỗi bản clean

## 11. Quy tắc an toàn dữ liệu

Updater và app mới phải tuân thủ các quy tắc sau:

- Tuyệt đối không xóa `data\` khi cập nhật.
- Không giải nén để ghi đè lên `data\`.
- Không xóa profile người dùng nếu không có lệnh nghiệp vụ rõ ràng.
- Luôn có backup tạm thời trước khi migration lần đầu.
- Nếu update lỗi giữa chừng thì dữ liệu vẫn còn nguyên.

## 12. Lộ trình cải tổ để xây dựng

Nên làm theo thứ tự sau:

### Giai đoạn 1. Chuẩn hóa cấu trúc dữ liệu

- Tạo lớp quản lý đường dẫn.
- Đưa dữ liệu vào `data\`.
- Thêm migration từ cấu trúc cũ sang cấu trúc mới.

### Giai đoạn 2. Thêm version và release package

- Thêm version cho app.
- Tạo output release rõ ràng.
- Chốt danh sách file được phát hành.

### Giai đoạn 3. Xây dựng Updater.exe

- Tạo project updater riêng.
- Cho updater đọc manifest.
- Tải zip và thay file chương trình.

### Giai đoạn 4. Nối app chính với updater

- App kiểm tra version mới.
- Gọi updater khi cần.
- Mở lại app sau cập nhật.

### Giai đoạn 5. Chuẩn hóa để dùng lại cho các app khác

- Tái sử dụng `Updater.exe`.
- Tái sử dụng `AppPaths`.
- Tái sử dụng mẫu cấu trúc thư mục.
- Tái sử dụng quy trình release.

## 13. Định nghĩa thành công

Dự án được xem là đạt mục tiêu khi:

- Bạn không cần copy tay 3 file form nữa.
- Bản clean update lên bản mới mà không mất `ds.txt`, user-agent, profile và dữ liệu đang có.
- Mọi app sau này có thể theo cùng một mẫu cấu trúc.
- Việc phát hành bản mới trở thành quy trình có version, có release, có updater.

## 14. Kết luận

Hướng đi đúng không phải là tiếp tục copy tay file source, mà là:

- tách dữ liệu ra khỏi file chương trình
- chuẩn hóa cấu trúc thư mục
- thêm updater riêng
- phát hành theo version

Đây là bước cải tổ cần thiết để app hiện tại và các app sau này có thể:

- an toàn hơn
- dễ bảo trì hơn
- dễ cập nhật hơn
- dùng được trên nhiều máy mà không lo lệch dữ liệu
