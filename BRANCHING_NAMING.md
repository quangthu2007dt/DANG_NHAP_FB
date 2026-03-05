# Branching and Naming Standard

## 1) Long-lived branches

- `v2/session-runtime`: nhanh phat trien va release chinh.
- `main`: nhanh legacy/tai lieu de hoc tap, doi chieu.

Neu can tach ro legacy, tao them branch snapshot:

- `legacy/v1.0.6`: branch chi doc, khong phat trien tinh nang moi.

## 2) Ten nhanh lam viec

- `feat/v2-<topic>`
- `fix/v2-<topic>`
- `docs/<topic>`

Vi du:

- `feat/v2-auto-login-dom`
- `fix/v2-session-cleanup`

## 3) Version naming

- Luong V2 dung nhan version: `V2`.
- Luong legacy dung dang: `1.0.x`.

Dong bo 3 diem sau trong moi lan release:

- `DANG NHAP FACEBOOK/version.json` -> `version`
- `DANG NHAP FACEBOOK/manifest.json` -> `latestVersion`
- `release/stable/manifest.json` -> `latestVersion`

## 4) Package naming

- Goi updater o kenh stable (giu co dinh): `DANG_NHAP_FACEBOOK_V2.zip`
- Goi luu tru lich su: `DANG_NHAP_FACEBOOK_<version>.zip`

Vi du:

- `DANG_NHAP_FACEBOOK_1.0.6.zip`
- `DANG_NHAP_FACEBOOK_1.0.5.zip`

## 5) Tag naming

- V2 release tag: `v2-YYYYMMDD-rN` (vi du: `v2-20260305-r1`)
- Legacy reference tag: `legacy-1.0.6`

## 6) Commit prefix de de loc log

- `[v2][feat] ...`
- `[v2][fix] ...`
- `[docs] ...`
