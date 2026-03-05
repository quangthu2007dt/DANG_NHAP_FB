# Changelog

Tat ca thay doi quan trong cua du an duoc ghi tai day.

## [V2.05] - 2026-03-06

### Changed

- Bo sung marker `update_pending_marker.json` duoc ghi truoc khi app giao updater.
- Lan mo dau sau update: app fallback doc marker pending de hien thong bao thanh cong ngay ca khi updater cu khong ghi marker success.
- Giu hanh vi xoa marker sau khi xu ly de thong bao chi hien 1 lan.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2_05.zip`

## [V2.04] - 2026-03-06

### Changed

- Updater ghi marker `update_success_marker.json` sau khi thay file thanh cong.
- App doc marker o lan mo dau sau update va hien thong bao: da cap nhat thanh cong len phien ban moi.
- Marker tu dong duoc xoa sau khi thong bao de khong lap lai.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2_04.zip`

## [V2.03] - 2026-03-05

### Changed

- Bo sung hop thoai startup update hien thi ro: version hien tai, version moi nhat, nguon manifest.
- Neu khoi dong updater that bai, app hien thong bao va huong dan kiem tra log thay vi im lang.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2_03.zip`

## [V2.002] - 2026-03-05

### Changed

- Sua loi updater parse sai `--app-dir` khi duong dan co dau gach cuoi.
- Sua so sanh version de khong coi moi chuoi "khac nhau" la ban moi.
- Tang do ben cho ban portable: updater bo qua cac runtime DLL dang duoc no nap.
- Bo sung script build portable on dinh cho thu muc clean va goi zip release.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2_002.zip`

## [V2.001] - 2026-03-05

### Changed

- Cap nhat ban phat hanh `V2.001` de kiem thu luong auto-update.
- Bo sung su kien hien thi times o canh duoi (theo cap nhat moi trong giao dien).
- Chot goi update moi: `DANG_NHAP_FACEBOOK_V2_001.zip`.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2_001.zip`

## [V2] - 2026-03-05

### Changed

- Chot dinh huong V2 theo mo hinh session tam: khong luu profile theo UID.
- Dong bo nhan version `V2` trong `version.json` va cac manifest.
- Chuan hoa goi release chinh cho updater: `DANG_NHAP_FACEBOOK_V2.zip`.

### Artifact

- `release/stable/DANG_NHAP_FACEBOOK_V2.zip`

## [1.0.6] - Legacy baseline

### Notes

- Ban da nang duoc giu lai de hoc tap va doi chieu.
- Khong phai nhanh release chinh cho muc tieu kiem tra dang nhap.

## [1.0.5] [1.0.4] [1.0.3] [1.0.2] [1.0.1] - Legacy archive

### Notes

- Cac ban nay duoc luu trong `release/stable/` de tham khao lich su.
- Chi dung de tra cuu, khong tiep tuc mo rong tinh nang.
