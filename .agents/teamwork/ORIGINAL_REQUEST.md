# Original User Request

## Initial Request — 2026-11-07T09:10:13Z

Đồng bộ và tối ưu hóa toàn bộ giao diện người dùng bên ngoài của dự án web hiện tại để hiển thị trực quan, dễ nhìn và thuận tiện thao tác trên trình duyệt di động theo chuẩn responsive web, giữ nguyên hoàn toàn khu vực quản trị (admin).

Working directory: D:\web_tinh_nguyen
Integrity mode: development

## Requirements

### R1. Tối ưu hóa Responsive Web cho toàn bộ giao diện người dùng bên ngoài
Điều chỉnh bố cục, thanh điều hướng (navigation/menu), phông chữ, khoảng cách, các biểu mẫu (forms) và bảng/danh sách trên toàn bộ các trang công khai phía người dùng để tương thích tối đa với màn hình điện thoại di động. Giao diện phải đảm bảo tính dễ đọc, các thành phần tương tác (nút bấm, liên kết) dễ chạm bấm bằng ngón tay và nội dung vừa vặn với kích thước màn hình di động mà không phát sinh hiện tượng tràn màn hình theo chiều ngang.

### R2. Bảo toàn khu vực quản trị (Admin)
Tuyệt đối không thay đổi mã nguồn, bố cục, kiểu dáng (CSS) hoặc hành vi của các trang và route thuộc khu vực quản trị (`/admin`).

## Acceptance Criteria

### Khả năng tương thích và hiển thị trên màn hình di động
- [ ] Bộ kiểm thử tự động trên trình duyệt (automated browser tests) kiểm tra trên các kích thước viewport di động tiêu chuẩn (chiều rộng từ 375px đến 430px) vượt qua 100% cho toàn bộ các trang công khai bên ngoài.
- [ ] Không có trang công khai nào bị thanh cuộn ngang ngoài ý muốn (`document.scrollingElement.scrollWidth <= window.innerWidth`) trên các viewport di động từ 375px trở lên.
- [ ] Menu điều hướng trên di động hiển thị gọn gàng (dạng hamburger hoặc drawer di động) và đóng/mở hoạt động chính xác khi tương tác trên màn hình nhỏ.
- [ ] Văn bản, hình ảnh, bảng dữ liệu và form nhập liệu trên các trang người dùng co giãn phù hợp, không bị che khuất, méo mó hoặc tràn lề màn hình di động.

### Tính toàn vẹn khu vực Admin và hệ thống
- [ ] Toàn bộ các view, controller và style thuộc khu vực Admin không bị chỉnh sửa hoặc phát sinh lỗi hiển thị/chức năng.
- [ ] Dự án biên dịch (`dotnet build`) và toàn bộ các bộ kiểm thử tự động hiện có cũng như kiểm thử mới đều vượt qua thành công.
