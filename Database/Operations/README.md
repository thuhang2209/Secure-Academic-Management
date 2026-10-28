# Vận hành cơ sở dữ liệu

Các script trong thư mục này là runbook có placeholder, không phải migration
ứng dụng. DBA phải thay token `$(...)`, kiểm tra quyền NTFS/certificate và chạy
trên môi trường được phê duyệt.

## Backup và khôi phục

[`backup_restore.sql`](./backup_restore.sql) thực hiện full backup có checksum,
`RESTORE VERIFYONLY` và mô tả restore drill cô lập. Tạo lịch SQL Agent riêng cho
full/differential/transaction log theo RPO/RTO đã được phê duyệt; không chạy
backup từ request web.

Mỗi lần backup cần kiểm tra checksum, thời gian lưu giữ, quyền đọc thư mục và
khả năng khôi phục. Restore drill tối thiểu phải chạy `DBCC CHECKDB` và ghi nhận
thời gian phục hồi. TDE certificate/private key phải được backup và lưu độc lập
với `.bak`; thiếu certificate phải được coi là kiểm thử thất bại.

## Audit, RLS và masking

Chạy các script trong [`../Security`](../Security) sau khi thay placeholder.
`002_row_level_masking.sql` yêu cầu ứng dụng hoặc connection initializer đặt
`SESSION_CONTEXT(N'IdentityUserId')` trước khi truy vấn; nếu chưa có cơ chế đó,
không bật policy trên production.
