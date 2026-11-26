# BÁO CÁO BÀN GIAO KHẢO SÁT HỆ THỐNG PHÂN QUYỀN ADMIN (R1, R2, R3)
**Agent**: `teamwork_explorer_survey_1`  
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1`  
**Ngày thực hiện**: 2026-11-07  

---

## 1. OBSERVATION (QUAN SÁT THỰC NGHIỆM)

1. **Khảo sát 10 Controller hiện tại của 10 phân hệ chức năng**:
   - `AdminController.cs` (`WEB_TinhNguyen/BoDieuKhien/AdminController.cs` dòng 20-48): `[Authorize]` trên lớp, action `Index()` dòng 43-48 trả về View Dashboard `/Admin`. Action `Login` dòng 58-142 có `[AllowAnonymous]` và `[ValidateAntiForgeryToken]` trên POST. Action `Logout` dòng 153-175 hỗ trợ cả GET và POST.
   - 9 Controller còn lại trong `WEB_TinhNguyen/BoDieuKhien/`:
     - `AdminTrangChuController.cs` (dòng 16, 5 Action POST CRUD: `CreateChienDich`, `ToggleStatus`, `DeleteChienDich`, `Edit`, `UploadHieuUngNhanh`).
     - `AdminChuongTrinhController.cs` (dòng 17, 1 Action POST: `Edit`).
     - `AdminGayQuyController.cs` (dòng 17, 1 Action POST: `Edit`).
     - `AdminCauHinhQRController.cs` (dòng 23, 4 Action POST: `Create`, `Edit`, `ToggleActive`, `Delete`).
     - `AdminKyUcController.cs` (dòng 17, 1 Action POST: `Edit`).
     - `AdminBanToChucController.cs` (dòng 17, 1 Action POST: `Edit`).
     - `AdminTinTucController.cs` (dòng 17, 1 Action POST: `Edit`).
     - `AdminHieuUngController.cs` (dòng 16, 2 Action POST: `Save`, `Delete`).
   - 2 Controller trong `WEB_TinhNguyen/Areas/CuaHang/Controllers/`:
     - `AdminSanPhamController.cs` (dòng 22-24, `[Area("CuaHang")]`, `[Authorize]`, 3 Action POST: `Create`, `Edit`, `Delete`).
     - `AdminDonHangController.cs` (dòng 20-22, `[Area("CuaHang")]`, `[Authorize]`, 2 Action POST: `CapNhatTrangThai`, `XoaDonHang`).
   - Tổng cộng: Đúng 21 Action POST CRUD phân bố trên 10 controller nghiệp vụ.

2. **Cấu trúc điều hướng `_AdminLayout.cshtml` (`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`)**:
   - Menu hiện có liên kết cho 9 controller: `Admin`, `AdminTrangChu`, `AdminChuongTrinh`, `AdminGayQuy`, `AdminSanPham` (`/CuaHang/AdminSanPham`), `AdminDonHang` (`/CuaHang/AdminDonHang`), `AdminCauHinhQR`, `AdminKyUc`, `AdminBanToChuc`, `AdminTinTuc`.
   - Tất cả các liên kết `@Url.Action(...)` ở root đều có `new { area = "" }`.
   - Logic active: `ViewContext.RouteData.Values["Controller"]?.ToString() == "<ControllerName>"`.
   - **Mục còn thiếu trong sidebar**: `AdminHieuUng` (phân hệ 8) và `AdminTaiKhoan` (phân hệ 11 chưa xây dựng).

3. **Cơ chế dữ liệu `TaiKhoanAdmin` và Dual-Mode Store**:
   - Thực thể `TaiKhoanAdmin.cs` (`WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs` dòng 13-79) hiện có: `IDTaiKhoan`, `TenDangNhap`, `MatKhauHash`, `MuoiBaoMat`, `HoTen`, `Email`, `VaiTro`, `TrangThaiHoatDong`, `NgayTao`, `LanDangNhapCuoi`. Chưa có trường lưu danh sách phân quyền chi tiết.
   - `DbInitializer.cs` (`WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` dòng 21, 59-79, 413-428, 1172-1250) quản lý `_inMemoryAdmins` và khởi tạo bảng SQL Server `[dbo].[TaiKhoanAdmin]`. Tài khoản mặc định là `admin` / `Admin@123` băm PBKDF2.

4. **Kiểm thử tự động hiện tại**:
   - Thực thi lệnh: `dotnet run --project tests/E2ETests/E2ETests.csproj`.
   - Kết quả: **518 Tests | 518 Passed | 0 Failed | 0 Skipped in 13.05s**.
   - Bài test `Tier5_ChallengerM2_2_RouteProtectionHarness` (Test `M2_2_01` & `M2_2_03`) và `Tier5_ChallengerM2_3_AuthPipelineHarness` (Test `M2_3_01` & `M2_3_02`) xác minh danh sách 11 controller và chính xác 21 Action POST CRUD.
   - Bài test `Tier5_ChallengerM2_2_RouteProtectionHarness` (Test `M2_2_06_CuaHangViewsPreserved`) khóa bất biến 12 tệp View trong `Areas/CuaHang/Views/`.

---

## 2. LOGIC CHAIN (CHUỖI SUY LUẬN TỪ QUAN SÁT ĐẾN KẾT LUẬN)

1. Từ **Observation 1 & 4**: Dự án có các bài test tự động phản chiếu (Reflection tests) kiểm tra danh sách 11 controller hiện có và 21 action POST CRUD. Mọi thay đổi không được làm mất thuộc tính `[Authorize]` trên các controller hiện có, không được thay đổi chữ ký 21 action POST, và không được vi phạm `[ValidateAntiForgeryToken]`.
2. Từ **Observation 2 & 4**: Các bài test giao diện (`CH2_04`, `CH2_05`, `CH2_06`, `F19_T01`, `F19_T02`) đọc mã nguồn tệp `_AdminLayout.cshtml` bằng `File.ReadAllText` và kiểm tra tĩnh các chuỗi `@Url.Action("Index", "...", new { area = "" })` cũng như `ViewContext.RouteData.Values["Controller"]?.ToString() == "..."`. Vì vậy:
   - Các liên kết hiện có không được sửa đổi URL hoặc xóa bỏ.
   - Khi áp dụng phân quyền động R3 qua `@if (User.CoQuyen("..."))`, các chuỗi mã nguồn trên vẫn tồn tại đầy đủ trong tệp nguồn, nên 100% test tĩnh vẫn đạt (PASS).
   - Bổ sung `AdminHieuUng` và `AdminTaiKhoan` vào `_AdminLayout.cshtml` phải tuân thủ nghiêm ngặt quy tắc `new { area = "" }` để không vi phạm `CH2_05`.
3. Từ **Observation 3**: Để thực hiện R1 bền vững trong cả môi trường SQL Server và Sandbox In-Memory:
   - Bổ sung trường `public string? PhanQuyen { get; set; } = "ALL";` vào `TaiKhoanAdmin`.
   - Cập nhật DDL trong `DbInitializer.cs` (`ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(1000) NULL`).
   - Cập nhật các hàm `CloneAdmin`, `AddInMemoryAdmin`, `UpdateInMemoryAdmin`, và gán `PhanQuyen = "ALL"` cho tài khoản `admin` mặc định.
4. Từ **Observation 1 & 3**: Phân hệ thứ 11 nên được đặt tại `AdminTaiKhoanController` trong `WEB_TinhNguyen/BoDieuKhien/AdminTaiKhoanController.cs` theo đúng quy ước đặt tên và cấu trúc MVC của toàn bộ dự án (`Admin*Controller`), với các view tương ứng trong `WEB_TinhNguyen/GiaoDien/AdminTaiKhoan/`.
5. Từ **Observation 4**: Khu vực `Areas/CuaHang/Views/` bị khóa SHA-256 bất biến. Do đó, kiểm soát phân quyền cho `AdminSanPham` và `AdminDonHang` chỉ được thực hiện ở Controller và Menu sidebar, không được can thiệp vào các tệp view này.

---

## 3. CAVEATS (CÁC ĐIỂM CẦN LƯU Ý VÀ GIẢ ĐỊNH)

1. **Tài khoản `admin` gốc**: Tài khoản `admin` mặc định (`Admin@123`) phải luôn mang quyền `"ALL"`. Không được phép chỉnh sửa xóa bỏ tài khoản này để các test suite hiện tại không bị ảnh hưởng.
2. **Không sửa đổi `Areas/CuaHang/Views/`**: Tuyệt đối không chỉnh sửa bất kỳ tệp Razor nào trong thư mục này.
3. **Thứ tự Middleware `Program.cs`**: Phải giữ nguyên: `UseRouting -> UseSession -> UseAuthentication -> UseAuthorization -> UseStaticFiles`.
4. **Mã phản hồi từ chối quyền (R2)**: Trả về HTTP 403 Forbidden hoặc HTTP 302 chuyển hướng kèm thông báo không có quyền đều hợp lệ theo ORIGINAL_REQUEST §R2 và Acceptance Criteria. Khuyến nghị trả về HTTP 403 Forbidden với View `AccessDenied.cshtml`.

---

## 4. CONCLUSION (KẾT LUẬN)

1. **Khảo sát hoàn thành 100%**: Đã định vị, lập danh mục và phân tích toàn bộ 10 phân hệ hiện có, cùng thiết kế chi tiết cho phân hệ 11 (`AdminTaiKhoanController`).
2. **Giải pháp kiến trúc khả thi**:
   - **R1**: `AdminTaiKhoanController` + `TaiKhoanAdmin.PhanQuyen` + Dual-mode SQL/In-Memory.
   - **R2**: `[PhanQuyenAdmin("Code")]` + Claims-based authorization + HTTP 403 Forbidden response.
   - **R3**: `User.CoQuyen(...)` extension method trong `_AdminLayout.cshtml`, tương thích 100% với các bài test tĩnh hiện tại.
   - **R4**: Bộ kịch bản kiểm thử tự động xác minh phân quyền độc lập, đảm bảo bảo toàn 518/518 test hiện có.
3. Toàn bộ thiết kế chi tiết đã được ghi chép đầy đủ tại `analysis.md`.

---

## 5. VERIFICATION METHOD (PHƯƠNG PHÁP XÁC MINH ĐỘC LẬP)

1. **Biên dịch dự án**:
   ```powershell
   dotnet build
   ```
   *Yêu cầu*: Build succeeded, 0 errors.

2. **Chạy toàn bộ 518 kiểm thử tự động hiện tại**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj
   ```
   *Yêu cầu*: 518 Tests | 518 Passed | 0 Failed.

3. **Kiểm tra các tệp tài liệu khảo sát**:
   - `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\analysis.md`
   - `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\handoff.md`
