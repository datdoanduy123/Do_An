USE AutoTask;
GO

-- 1. Xoá dữ liệu bảng trung gian trước để tránh lỗi khoá ngoại
DELETE FROM VaiTroQuyens;
DELETE FROM Quyens;
DELETE FROM NhomQuyens;

-- 2. Đặt lại khóa tự động tăng (Identity) để bắt đầu lại từ 1
DBCC CHECKIDENT ('Quyens', RESEED, 0);
DBCC CHECKIDENT ('NhomQuyens', RESEED, 0);

-- 3. Insert Nhóm Quyền
SET IDENTITY_INSERT NhomQuyens ON;
INSERT INTO NhomQuyens (Id, TenNhom, MoTa, CreatedAt) VALUES
(1, N'Hệ Thống', N'Quản trị hệ thống, người dùng, vai trò', GETDATE()),
(2, N'Quản Trị Dự Án', N'Quản lý dự án thành phần', GETDATE()),
(3, N'Quản Trị Công Việc', N'Quản lý vòng đời công việc', GETDATE()),
(4, N'Quản Lý Kỹ Năng', N'Quản lý danh mục kỹ năng', GETDATE());
SET IDENTITY_INSERT NhomQuyens OFF;

-- 4. Insert Quyền
SET IDENTITY_INSERT Quyens ON;
INSERT INTO Quyens (Id, TenQuyen, MaQuyen, MoTa, NhomQuyenId, CreatedAt) VALUES
-- Nhóm 1 (Hệ Thống)
(1, N'Xem Người Dùng', 'USER_VIEW', N'Xem danh sách và chi tiết User', 1, GETDATE()),
(2, N'Tạo Mới Người Dùng', 'USER_CREATE', N'Tạo mới User', 1, GETDATE()),
(3, N'Cập Nhật Người Dùng', 'USER_UPDATE', N'Sửa thông tin, gán/gỡ kỹ năng user', 1, GETDATE()),
(4, N'Xoá Người Dùng', 'USER_DELETE', N'Xoá mềm User hệ thống', 1, GETDATE()),
(5, N'Xem Vai Trò', 'ROLE_VIEW', N'Xem danh sách Vai trò', 1, GETDATE()),
(6, N'Tạo Vai Trò', 'ROLE_CREATE', N'Tạo mới Vai Trò', 1, GETDATE()),
(7, N'Cập Nhật Vai Trò', 'ROLE_UPDATE', N'Sửa Vai trò', 1, GETDATE()),
(8, N'Xoá Vai Trò', 'ROLE_DELETE', N'Xoá Vai Trò', 1, GETDATE()),
(9, N'Gán Vai Trò', 'ROLE_ASSIGN', N'Gán/Gỡ Vai Trò cho User', 1, GETDATE()),
(10, N'Xem Quyền', 'PERM_VIEW', N'Xem quyền', 1, GETDATE()),
(11, N'Tạo Quyền', 'PERM_CREATE', N'Tạo quyền mới', 1, GETDATE()),
(12, N'Cập Nhật Quyền', 'PERM_UPDATE', N'Chỉnh sửa quyền', 1, GETDATE()),
(13, N'Xoá Quyền', 'PERM_DELETE', N'Xóa quyền', 1, GETDATE()),
(14, N'Xem Nhóm Quyền', 'PERMGROUP_VIEW', N'Xem nhóm quyền', 1, GETDATE()),
(15, N'Tạo Nhóm Quyền', 'PERMGROUP_CREATE', N'Tạo nhóm quyền', 1, GETDATE()),
(16, N'Cập Nhật Nhóm Quyền', 'PERMGROUP_UPDATE', N'Sửa nhóm quyền', 1, GETDATE()),
(17, N'Xoá Nhóm Quyền', 'PERMGROUP_DELETE', N'Xóa nhóm quyền', 1, GETDATE()),

-- Nhóm 2 (Dự Án)
(18, N'Xem Dự Án', 'PROJECT_VIEW', N'User có thể xem chi tiết', 2, GETDATE()),
(19, N'Tạo Dự Án', 'PROJECT_CREATE', N'User có thể tạo', 2, GETDATE()),
(20, N'Cập Nhật Cấu Hình Dự Án', 'PROJECT_UPDATE', N'Quyền thêm bớt thành viên, config', 2, GETDATE()),
(21, N'Xoá Dự Án', 'PROJECT_DELETE', N'Kết thúc dự án', 2, GETDATE()),
(22, N'Xem Tài Liệu', 'DOC_VIEW', N'Quyền đọc file', 2, GETDATE()),
(23, N'Upload Tài Liệu', 'DOC_UPLOAD', N'Quyền tải lên', 2, GETDATE()),

-- Nhóm 3 (Công Việc)
(24, N'Xem Công Việc', 'TASK_VIEW', N'Trả về danh sách Task', 3, GETDATE()),
(25, N'Tạo Công Việc', 'TASK_CREATE', N'Quyền tạo Task', 3, GETDATE()),
(26, N'Cập Nhật Công Việc', 'TASK_UPDATE', N'Kéo thả tiến độ', 3, GETDATE()),
(27, N'Xoá Công Việc', 'TASK_DELETE', N'Quyền xoá', 3, GETDATE()),
(28, N'Giao Việc', 'TASK_ASSIGN', N'Thay đổi người làm (Assignee)', 3, GETDATE()),
(29, N'Duyệt Công Việc', 'TASK_APPROVAL', N'Approve công việc', 3, GETDATE()),
(30, N'Xem Sprint', 'SPRINT_VIEW', N'Quyền vào trang sprint', 3, GETDATE()),
(31, N'Tạo Sprint', 'SPRINT_CREATE', N'Quyền tạo sprint', 3, GETDATE()),
(32, N'Cập Nhật Sprint', 'SPRINT_UPDATE', N'Quyền sửa sprint', 3, GETDATE()),
(33, N'Xoá Sprint', 'SPRINT_DELETE', N'Quyền xoá', 3, GETDATE()),

-- Nhóm 4 (Kỹ Năng)
(34, N'Xem Kỹ Năng', 'SKILL_VIEW', N'Xem skill dictionary', 4, GETDATE()),
(35, N'Tạo Mới Kỹ Năng', 'SKILL_CREATE', N'Tạo', 4, GETDATE()),
(36, N'Sửa Đổi Kỹ Năng', 'SKILL_UPDATE', N'Cập nhật', 4, GETDATE()),
(37, N'Xoá Kỹ Năng', 'SKILL_DELETE', N'Xóa', 4, GETDATE());
SET IDENTITY_INSERT Quyens OFF;

-- 5. Insert Gán Quyền cho Vai Trò 
-- Giả định theo data của bạn: VaiTroId 3 là Quản Lý, VaiTroId 4 là Nhân Viên

-- A. Gán tất cả mọi quyền cho vai trò Quản lý (ID 3)
INSERT INTO VaiTroQuyens (VaiTroId, QuyenId)
SELECT 3, Id FROM Quyens;

-- B. Gán một số quyền cơ bản cho vai trò Nhân Viên (ID 4)
INSERT INTO VaiTroQuyens (VaiTroId, QuyenId)
SELECT 4, Id FROM Quyens WHERE MaQuyen IN (
    'PROJECT_VIEW', 
    'DOC_VIEW', 
    'TASK_VIEW', 
    'TASK_UPDATE', 
    'SPRINT_VIEW', 
    'SKILL_VIEW',
    'USER_VIEW' -- Nhân viên chỉ có thể danh sách xem (có thể tắt ở UI)
);
GO
