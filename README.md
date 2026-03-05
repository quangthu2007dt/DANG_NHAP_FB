# DANG NHAP FACEBOOK - V2 Focus

## Muc tieu

Du an uu tien mot muc tieu chinh: kiem tra nhanh tai khoan co dang nhap duoc hay khong.

## Dinh huong san pham

- V2 (thuc dung): khong luu folder profile theo UID, uu tien toc do va do on dinh.
- V1.0.6 (legacy): giu lai de hoc tap, doi chieu va tham khao.

## Nhanh va vai tro

- `v2/session-runtime`: nhanh phat trien chinh, dung de release.
- `main`: nhanh legacy/tai lieu tham khao (v1.0.6).

## Cau truc thu muc chinh

- `DANG NHAP FACEBOOK/`: app WinForms chinh.
- `Updater/`: trinh auto update.
- `release/stable/`: manifest va goi phat hanh.
- `artifacts/`: output build tam.

## Quy trinh release nhanh

1. Cap nhat `DANG NHAP FACEBOOK/version.json`.
2. Cap nhat `DANG NHAP FACEBOOK/manifest.json`.
3. Cap nhat `release/stable/manifest.json`.
4. Chay `./Build-Release.ps1` (build nhanh theo kieu framework-dependent).
5. Neu can mang sang may khac de chay ngay, chay them: `powershell -ExecutionPolicy Bypass -File .\Build-Portable.ps1`.
6. Day goi zip moi vao `release/stable/`.

## Tai lieu lien quan

- `CHANGELOG.md`
- `BRANCHING_NAMING.md`
- `HUONG_DAN_CAU_TRUC_DU_AN.md`
