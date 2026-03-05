# HƯỚNG DẪN CẤU TRÚC TÁCH DỰ ÁN

## 1) Thư mục source

- `E:\BT_Python_Tiem\DANG NHAP FACEBOOK_LEGACY`
  - Bản cũ ổn định để học, đối chiếu.
  - Không phát triển tính năng mới tại đây.

- `E:\BT_Python_Tiem\DANG NHAP FACEBOOK_V2`
  - Bản phát triển mới (mô hình phiên tạm + auto update).
  - Nhánh làm việc hiện tại: `v2/session-runtime`.

## 2) Thư mục dữ liệu chạy thật

- `E:\DANG_NHAP_FB_DATA`
  - Chứa `ds.txt`, `user_agents.txt`, `ua_dang_dung.txt`, `profile_mau`.
  - Tách rời khỏi source để build/pull code không làm lệch dữ liệu.

## 3) Quy tắc làm việc

- Chỉnh code: chỉ làm trong `DANG NHAP FACEBOOK_V2`.
- Cần đối chiếu bản cũ: mở `DANG NHAP FACEBOOK_LEGACY`.
- Không copy thủ công source từ legacy sang V2.
- Nếu cần đổi đường dẫn data riêng cho máy khác:
  - Tạo biến môi trường `DANG_NHAP_FB_DATA_DIR`.
