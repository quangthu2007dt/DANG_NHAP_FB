# THIẾT KẾ NÂNG CẤP: PHIÊN TẠM SẠCH, KHÔNG LƯU PROFILE UID

## 1. Mục tiêu

Nâng cấp app theo hướng:

- Mỗi lần mở Chrome là một phiên mới sạch.
- Vẫn giữ extension đã cài sẵn trong `profile_mau`.
- Hỗ trợ mở nhiều phiên cùng lúc.
- Khi Chrome thoát thì xóa nhanh phiên cũ.
- Không còn phụ thuộc mô hình lưu profile theo UID.

## 2. Vấn đề hiện tại

Mô hình cũ đang lưu profile theo UID trong `data\profiles\<uid>`, dẫn tới:

- Dữ liệu profile phình lớn theo thời gian.
- Xóa dòng chậm vì phải đóng Chrome + xóa thư mục profile nặng.
- Dễ gặp lỗi lock file khi profile chưa nhả hết.
- Phụ thuộc vào clone profile và thao tác dọn thủ công.

## 3. Nguyên tắc thiết kế mới

1. Không dùng profile hệ thống của Chrome.
2. Không lưu profile cố định theo UID.
3. Dùng một profile mẫu duy nhất để nhân bản phiên.
4. Mỗi phiên chạy dùng thư mục session tạm riêng.
5. Session phải được dọn khi process thoát hoặc khi app khởi động lại.

## 4. Cấu trúc dữ liệu mới

Trong `data\`:

- `profile_mau\` : profile gốc đã cài extension.
- `sessions\` : thư mục chứa phiên tạm.
- `sessions\<sessionId>\` : user-data-dir của từng phiên.
- `session_registry.json` : theo dõi session đang chạy để dọn đúng.
- `ds.txt`, `user_agents.txt`, `ua_dang_dung.txt` giữ nguyên vai trò.

Bỏ phụ thuộc:

- `data\profiles\<uid>` (chuyển sang legacy backup).
- logic bắt buộc `profileName = UID`.

## 5. Luồng chạy mới

## 5.1. Mở phiên mới

1. Người dùng chọn dòng tài khoản.
2. Tạo `sessionId` (ví dụ: `yyyyMMdd_HHmmss_uid_random`).
3. Tạo thư mục `data\sessions\<sessionId>`.
4. Copy từ `data\profile_mau` sang thư mục session.
5. Mở Chrome với:
   - `--user-data-dir=<data\sessions\<sessionId>>`
   - URL theo lựa chọn giao diện.
6. Ghi registry session:
   - `sessionId`
   - `uid`
   - `processId`
   - `path`
   - `startedAt`

## 5.2. Đóng phiên

1. Theo dõi process Chrome đã mở.
2. Khi process thoát:
   - đánh dấu session đóng.
   - xóa thư mục `data\sessions\<sessionId>`.
3. Nếu xóa lỗi do lock:
   - retry nhiều lần, tăng delay.
   - fallback bằng lệnh hệ thống.
   - ghi log chi tiết.

## 5.3. Khi app khởi động

1. Đọc `session_registry.json`.
2. Với session không còn process sống:
   - dọn session mồ côi.
3. Dọn thêm thư mục cũ trong `data\sessions` không còn trong registry.

## 6. Xử lý nút Xóa dòng

Mục tiêu mới của xóa dòng:

- Chỉ xóa dữ liệu dòng trong grid/ds.
- Không xóa profile UID (vì không còn dùng).

Nếu dòng đang có phiên chạy:

- Hỏi xác nhận đóng phiên trước.
- Đóng process theo sessionId.
- Dọn session tạm.
- Sau đó xóa dòng dữ liệu.

Kết quả: không còn cảnh bấm xóa nhiều lần vì phải xử lý thư mục profile lớn.

## 7. Hỗ trợ đa phiên

Thiết kế cho nhiều phiên cùng lúc:

- Mỗi phiên có `sessionId` riêng.
- Mỗi phiên có `processId` riêng.
- Không dùng chung `user-data-dir`.
- Registry là nguồn sự thật cho việc quản lý phiên.

## 8. Migration từ mô hình cũ

Khi nâng cấp:

1. Nếu tồn tại `data\profiles\`:
   - đổi tên sang `data\_legacy_profiles_backup_<timestamp>`.
2. Giữ lại để an toàn, không dùng trong luồng mới.
3. Tạo mới `data\sessions\` và `session_registry.json`.

Không xóa legacy ngay trong bản đầu để tránh mất dữ liệu ngoài ý muốn.

## 9. Các điểm kỹ thuật cần chú ý

1. `profile_mau` phải luôn sạch (không cache rác).
2. Không ghi dữ liệu nghiệp vụ vào session tạm.
3. Dọn session phải chạy nền, không khóa UI lâu.
4. Thao tác file phải có timeout + retry + log.
5. Không dùng `Thread.Sleep` cứng kéo dài trên UI thread.

## 10. API/lớp cần bổ sung

Đề xuất thêm:

- `SessionRuntimeService`
  - `CreateSessionFromTemplate(uid)`
  - `LaunchChromeForSession(session, url, ua)`
  - `TryCloseAndCleanupSession(sessionId)`
  - `CleanupOrphanSessions()`

- `SessionRegistryService`
  - `LoadRegistry()`
  - `SaveRegistry()`
  - `UpsertSession()`
  - `MarkClosed()`
  - `FindByUid()`

- `SessionModel`
  - `SessionId`
  - `Uid`
  - `SessionPath`
  - `ProcessId`
  - `StartedAt`
  - `ClosedAt`
  - `Status`

## 11. Tiêu chí nghiệm thu

Đạt khi:

1. Mở 1 phiên:
   - có extension.
   - profile sạch.
   - đóng xong thư mục session bị xóa.
2. Mở 3-5 phiên cùng lúc:
   - không đụng nhau.
   - đóng phiên nào dọn phiên đó.
3. Bấm xóa dòng:
   - không phải bấm 2-3 lần.
   - không báo lỗi lock kéo dài như mô hình cũ.
4. Khởi động lại app sau crash:
   - session mồ côi được dọn.
5. Không còn phụ thuộc `data\profiles\<uid>`.

## 12. Lộ trình triển khai

Giai đoạn 1:

- Thêm `sessions` + `session_registry`.
- Viết service tạo/mở/dọn session.

Giai đoạn 2:

- Đổi luồng `Next/Chạy/Mở dòng` sang session tạm.
- Đổi luồng `Xóa dòng` theo quy tắc mới.

Giai đoạn 3:

- Migration legacy profiles.
- Tối ưu hiệu năng copy và dọn session.
- Hoàn thiện logging + test case.

## 13. Kết luận

Mô hình phiên tạm là hướng phù hợp nhất với mục tiêu hiện tại:

- nhanh hơn,
- sạch hơn,
- ít lỗi lock hơn,
- không còn phụ thuộc profile clone theo UID,
- vẫn giữ được extension từ `profile_mau`.
