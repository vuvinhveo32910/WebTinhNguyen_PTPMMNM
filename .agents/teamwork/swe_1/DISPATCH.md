## 2026-11-07T07:41:36Z

This is a single self-contained fix; keep it small and focused. Cập nhật giao diện bảng điều khiển quản trị (Admin Dashboard) để hiển thị chính xác các con số thống kê thực tế của dự án lấy từ cơ sở dữ liệu, thay thế toàn bộ các thông tin số liệu ảo và hardcoded.

Working directory: D:\web_tinh_nguyen
Integrity mode: development

## Requirements

### R1. Thay thế số liệu hardcode bằng dữ liệu thực tế trên Admin Dashboard
Tại giao diện Bảng điều khiển quản trị (`/admin`, tương ứng view `Index.cshtml` và controller `AdminController.cs`), loại bỏ toàn bộ các số liệu mẫu cố định (gồm: "12 YÊU CẦU MỚI", "45 BÀI VIẾT", "2 MÙA DỰ ÁN", "125.500K TỔNG QUỸ ĐÓNG GÓP", "150 TÌNH NGUYỆN VIÊN", "342 LỜI CHÚC ĐÃ NHẬN", "1,248 HÌNH ẢNH KHOẢNH KHẮC") và thay thế bằng các giá trị động lấy trực tiếp từ hệ thống.

### R2. Truy vấn và ánh xạ dữ liệu từ cơ sở dữ liệu
Rà soát mô hình cơ sở dữ liệu hiện có của dự án và viết các câu truy vấn thống kê tương ứng cho từng chỉ số (yêu cầu mới/liên hệ, bài viết tin tức, chiến dịch/mùa dự án, tổng tiền quỹ đóng góp, số lượng tình nguyện viên, số lời chúc, số hình ảnh khoảnh khắc). Truyền các kết quả thống kê này từ `AdminController` sang `Index.cshtml` và hiển thị với định dạng số/tiền tệ phù hợp, xử lý an toàn trường hợp số liệu bằng 0.

## Acceptance Criteria

### Tính chính xác và nguồn dữ liệu thống kê
- [ ] Toàn bộ 7 thẻ thống kê trên Bảng điều khiển quản trị (`Index.cshtml`) hiển thị dữ liệu được truyền động từ `AdminController`, không còn thẻ nào chứa giá trị hardcode tĩnh.
- [ ] Từng chỉ số hiển thị khớp chính xác với kết quả truy vấn thực tế từ các bảng dữ liệu tương ứng trong cơ sở dữ liệu.
- [ ] Số liệu được hiển thị đúng định dạng (đặc biệt là định dạng tiền tệ cho tổng quỹ đóng góp) và hiển thị giá trị mặc định an toàn (0 hoặc định dạng 0đ) nếu cơ sở dữ liệu chưa có bản ghi.

### Biên dịch và tính toàn vẹn
- [ ] Dự án biên dịch thành công (`dotnet build`) không phát sinh bất kỳ lỗi nào.
