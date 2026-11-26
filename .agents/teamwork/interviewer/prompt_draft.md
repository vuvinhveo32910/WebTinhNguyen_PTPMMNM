Thực thi đồng bộ giao diện hiển thị danh sách sản phẩm và giỏ hàng của dự án hiện tại, đảm bảo trên mobile hiển thị theo bố cục lưới sản phẩm thu gọn như trong Ảnh 1 và trên PC hiển thị đồng bộ theo bố cục như trong Ảnh 2.

Working directory: D:\web_tinh_nguyen
Integrity mode: development

Reference material:
- Ảnh 1 (Mobile product grid view): C:/Users/Admin/.gemini/antigravity/brain/01119d34-8fe7-47fe-a854-a89d3eab242c/.user_uploaded/media_1794051580635_c727aca5.png
- Ảnh 2 (PC product grid view): C:/Users/Admin/.gemini/antigravity/brain/01119d34-8fe7-47fe-a854-a89d3eab242c/.user_uploaded/media_1794051652186_47e3a517.jpg
- View danh sách sản phẩm: D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Views\CuaHang\Index.cshtml
- View giỏ hàng: D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Views\GioHang\Index.cshtml
- Layout cửa hàng: D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Views\Shared\_CuaHangLayout.cshtml

## Requirements

### R1. Giao diện danh sách sản phẩm trên PC và Mobile
Cập nhật giao diện danh sách sản phẩm trong `CuaHang/Index.cshtml` đồng bộ với ảnh tham chiếu:
- Trên màn hình di động (mobile viewport): hiển thị lưới sản phẩm thu gọn (compact grid) tương ứng bố cục Ảnh 1, với thẻ sản phẩm bo góc, ảnh sản phẩm vuông ở trên, tiêu đề sản phẩm gọn gàng, thông tin phụ/đánh giá, và hàng đáy gồm giá tiền màu nổi bật bên trái cùng nút icon thêm giỏ hàng hình vuông bo góc bên phải.
- Trên màn hình máy tính (PC viewport): hiển thị lưới sản phẩm nhiều cột theo bố cục Ảnh 2, đồng bộ kiểu dáng thẻ sản phẩm với phiên bản di động, giữ thanh tìm kiếm và bộ lọc danh mục trực quan.

### R2. Giao diện giỏ hàng đồng bộ phong cách và tương thích Responsive
Cập nhật giao diện trang giỏ hàng `GioHang/Index.cshtml` để có kiểu dáng hiện đại, sạch sẽ và đồng bộ phong cách thiết kế với giao diện sản phẩm mới. Giao diện giỏ hàng phải hiển thị rõ ràng thông tin sản phẩm, số lượng, đơn giá, thành tiền và các thao tác cập nhật/xóa, tự động thích ứng mượt mà trên cả màn hình di động lẫn máy tính mà không bị tràn màn hình theo chiều ngang.

### R3. Tương thích hệ thống và giữ nguyên logic nghiệp vụ
Kế thừa cấu hình Tailwind CSS / CSS của dự án, biên dịch CSS đúng quy trình (`npm run build:css`). Giữ nguyên toàn bộ logic gửi nhận dữ liệu MVC, xử lý giỏ hàng, tìm kiếm, lọc và phân trang hiện có; không làm thay đổi các khu vực khác ngoài cửa hàng (đặc biệt khu vực Admin).

## Acceptance Criteria

### Giao diện và Trải nghiệm hiển thị
- [ ] Giao diện danh sách sản phẩm trên màn hình PC hiển thị đúng bố cục nhiều cột và kiểu thẻ sản phẩm như trong Ảnh 2.
- [ ] Giao diện danh sách sản phẩm trên màn hình Mobile hiển thị lưới sản phẩm thu gọn (compact grid) với nút giỏ hàng và giá tiền tương ứng bố cục Ảnh 1.
- [ ] Giao diện trang giỏ hàng (`GioHang/Index.cshtml`) đồng bộ thiết kế, hiển thị tối ưu trên cả kích thước mobile và PC, không phát sinh lỗi tràn ngang (`scrollWidth <= window.innerWidth`).
- [ ] Thao tác thêm vào giỏ từ nút icon trên thẻ sản phẩm và các thao tác điều chỉnh số lượng, xóa trong giỏ hàng hoạt động chính xác.

### Kiểm thử và Tích hợp hệ thống
- [ ] Biên dịch Tailwind CSS (`npm run build:css`) thành công, không có xung đột kiểu dáng giao diện.
- [ ] Dự án biên dịch (`dotnet build`) không phát sinh lỗi hoặc cảnh báo gãy liên kết.
- [ ] Các bộ kiểm thử hiện có (`tests/run_mobile_viewport_browser_tests.js` và `dotnet run --project tests/E2ETests/E2ETests.csproj --no-restore -- --all`) chạy thành công.
