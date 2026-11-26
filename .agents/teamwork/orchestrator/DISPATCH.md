## 2026-11-07T02:15:22Z

Xây dựng và tích hợp hệ thống phân quyền quản trị (Admin Authentication & Authorization) cho ứng dụng: yêu cầu đăng nhập bằng tài khoản quản trị để truy cập vào toàn bộ khu vực quản trị `/admin` và thực hiện các chức năng thêm, xóa, sửa. Tự động thiết lập cơ sở dữ liệu SQL Server, lưu trữ tài khoản admin mặc định với mật khẩu được băm bảo mật, và xây dựng giao diện đăng nhập quản trị dành riêng cho hệ thống vận hành thực tế.

Working directory: D:\web_tinh_nguyen
Integrity mode: development

## Requirements

### R1. Thiết lập Cơ sở dữ liệu và Khởi tạo Tài khoản Admin trong SQL Server
Tạo cấu trúc bảng dữ liệu người dùng / quản trị trong SQL Server phù hợp với kiến trúc dự án hiện tại. Tự động khởi tạo (seed) sẵn một tài khoản quản trị mặc định (tên đăng nhập: `admin`, mật khẩu: `Admin@123` hoặc tương đương) với mật khẩu được băm (hash) bảo mật trong cơ sở dữ liệu, đảm bảo chuỗi kết nối và cơ sở dữ liệu sẵn sàng hoạt động.

### R2. Giao diện và Cơ chế Đăng nhập Quản trị
Cung cấp giao diện đăng nhập dành riêng cho quản trị viên (form nhập tài khoản và mật khẩu có giao diện thân thiện, hiển thị thông báo lỗi rõ ràng khi thông tin không chính xác). Khi người dùng nhập đúng thông tin tài khoản admin, hệ thống xác thực từ SQL Server, thiết lập phiên đăng nhập an toàn (cookie/session) và chuyển hướng vào trang quản trị `/admin`. Cung cấp chức năng đăng xuất để xóa phiên đăng nhập.

### R3. Phân quyền và Bảo vệ Tuyến đường Quản trị (/admin) cùng các Tác vụ Thêm, Xóa, Sửa
Áp dụng cơ chế kiểm soát truy cập (Authorization) cho toàn bộ các route thuộc khu vực quản trị `/admin` và tất cả các action/endpoint thực hiện thao tác thêm, xóa, sửa dữ liệu. Khi người dùng chưa đăng nhập hoặc không có vai trò quản trị truy cập vào bất kỳ trang nào thuộc `/admin` hoặc gửi yêu cầu thêm/xóa/sửa, hệ thống phải chặn truy cập và tự động chuyển hướng về trang đăng nhập `/admin/login` (kèm thông báo yêu cầu đăng nhập). Sau khi đăng nhập thành công bằng tài khoản admin, người dùng có toàn quyền truy cập và thao tác đầy đủ các chức năng quản trị.

### R4. Kịch bản Kiểm thử Tự động Xác minh Toàn diện
Xây dựng và thực thi bộ kịch bản kiểm thử tự động (integration / E2E / HTTP test suite) để xác minh khách quan: kiểm tra kết nối SQL Server và tài khoản admin, kiểm tra đăng nhập thành công và đăng nhập thất bại, kiểm tra cơ chế chặn và chuyển hướng đối với tất cả các route `/admin` khi chưa xác thực, và xác nhận truy cập thành công sau khi xác thực.

## Acceptance Criteria

### Cơ sở dữ liệu và Dữ liệu mẫu (Database & Seed Data)
- [ ] Bảng dữ liệu người dùng / quản trị được tạo thành công trong SQL Server và kết nối hoạt động bình thường từ ứng dụng.
- [ ] Tài khoản admin mặc định tồn tại trong cơ sở dữ liệu với mật khẩu được lưu trữ dưới dạng băm (hashed password, không lưu plain-text), sẵn sàng dùng để đăng nhập.

### Giao diện và Xác thực Đăng nhập (Admin Login UI & Auth)
- [ ] Truy cập đường dẫn đăng nhập quản trị (ví dụ: `/admin/login` hoặc `/Account/Login`) hiển thị đầy đủ giao diện form đăng nhập tài khoản và mật khẩu.
- [ ] Nhập thông tin đăng nhập admin hợp lệ: hệ thống xác thực thành công, tạo phiên đăng nhập (session/cookie) và chuyển hướng người dùng vào giao diện quản trị `/admin`.
- [ ] Nhập sai tài khoản hoặc mật khẩu: hệ thống từ chối đăng nhập và hiển thị thông báo lỗi rõ ràng trên giao diện, không tạo phiên đăng nhập.
- [ ] Thực hiện chức năng đăng xuất: hệ thống hủy phiên đăng nhập an toàn và chuyển hướng ra khỏi khu vực quản trị.

### Phân quyền và Kiểm soát truy cập (Authorization & Route Protection)
- [ ] Khi chưa đăng nhập tài khoản admin, mọi yêu cầu truy cập (GET/POST) đến bất kỳ route nào thuộc `/admin` hoặc các endpoint thêm, xóa, sửa đều bị từ chối và tự động chuyển hướng (HTTP redirect 302) về trang đăng nhập `/admin/login`.
- [ ] Sau khi đăng nhập bằng tài khoản admin, người dùng có thể truy cập thành công vào tất cả các trang quản trị `/admin` (HTTP status code 200 OK) và thực hiện đầy đủ các chức năng thêm, xóa, sửa dữ liệu.

### Kiểm thử và Biên dịch (Build & Automated Verification)
- [ ] Dự án biên dịch thành công (`dotnet build`) không phát sinh bất kỳ lỗi nào (0 errors).
- [ ] Bộ kịch bản kiểm thử tự động được thực thi thành công 100%, tự động kiểm tra và xác nhận: chặn truy cập khi chưa đăng nhập, đăng nhập admin thành công, và truy cập hợp lệ sau khi đăng nhập.


## 2026-11-07T05:08:43Z

Xây dựng chức năng quản lý tài khoản quản trị và hệ thống phân quyền chi tiết cho từng tài khoản trong khu vực admin (`/admin`), đảm bảo mỗi tài khoản chỉ có thể truy cập và thao tác đúng các phân hệ chức năng được admin chỉ định.

Working directory: D:\web_tinh_nguyen
Integrity mode: development

## Requirements

### R1. Quản lý Tài khoản Quản trị và Phân quyền (Admin Account & Role Management)
Trong khu vực quản trị, cung cấp giao diện và tính năng cho phép quản trị viên thêm mới tài khoản quản trị, thiết lập thông tin đăng nhập và linh hoạt lựa chọn phân quyền các phân hệ chức năng mà tài khoản đó được phép quản lý (bao gồm quyền toàn bộ cấu hình/hệ thống hoặc 1 hay nhiều phân hệ tùy chọn). Danh sách phân quyền bao gồm tất cả các phân hệ quản trị hiện có trong hệ thống (Trang chủ, Chương trình, Gây quỹ, Cấu hình QR, Ký ức, Ban tổ chức, Tin tức, Hiệu ứng, Quản lý sản phẩm, Đơn hàng, Quản lý tài khoản quản trị). Thông tin tài khoản và phân quyền phải được lưu trữ bền vững trong cơ sở dữ liệu với mật khẩu được băm bảo mật.

### R2. Kiểm soát Quyền Truy cập và Thực thi theo Phân hệ (Granular Authorization Enforcement)
Khi người dùng đăng nhập bằng tài khoản quản trị, hệ thống kiểm tra phân quyền của tài khoản đó đối với từng yêu cầu HTTP (GET, POST và các thao tác thêm, sửa, xóa dữ liệu). Tài khoản chỉ được phép truy cập và thực thi đúng các phân hệ chức năng đã được cấp quyền. Mọi yêu cầu truy cập trực tiếp vào các tuyến đường (routes/endpoints) của phân hệ không được phân quyền đều phải bị chặn (trả về mã trạng thái từ chối truy cập HTTP 403 Forbidden hoặc chuyển hướng kèm thông báo không có quyền).

### R3. Hiển thị Giao diện và Điều hướng theo Phân quyền (Dynamic Navigation Menu)
Thanh điều hướng/menu của khu vực quản trị tự động cập nhật động theo quyền hạn của tài khoản đang đăng nhập: chỉ hiển thị các liên kết dẫn đến những phân hệ chức năng mà tài khoản đó được phép quản lý, đồng thời ẩn toàn bộ các liên kết đến các phân hệ không được cấp quyền.

### R4. Kịch bản Kiểm thử Tự động Xác minh (Automated Verification)
Tận dụng hạ tầng kiểm thử hiện có của dự án (`tests/`) để xây dựng và thực thi bộ kịch bản kiểm thử tự động (integration/E2E test suite) nhằm xác minh khách quan tính năng tạo tài khoản, gán quyền, đăng nhập và kiểm soát quyền truy cập: xác nhận tài khoản truy cập thành công vào các phân hệ được cấp quyền và bị chặn truy cập đối với các phân hệ không được cấp quyền.

## Acceptance Criteria

### Quản lý Tài khoản và Gán quyền
- [ ] Giao diện admin cho phép thêm mới tài khoản quản trị với đầy đủ thông tin xác thực và danh sách chọn các quyền/phân hệ quản lý cụ thể.
- [ ] Mật khẩu của tài khoản mới được mã hóa/băm bằng thuật toán an toàn trước khi lưu vào cơ sở dữ liệu, không lưu trữ plain-text.
- [ ] Tài khoản mới tạo cùng danh sách quyền hạn được lưu trữ và truy vấn chính xác từ cơ sở dữ liệu.

### Kiểm soát Truy cập theo Quyền hạn
- [ ] Tài khoản được cấp quyền cho phân hệ X: đăng nhập và gửi yêu cầu (GET/POST) đến các endpoint của phân hệ X nhận được phản hồi thành công (HTTP 200 OK).
- [ ] Tài khoản không được cấp quyền cho phân hệ Y: gửi yêu cầu (GET/POST) đến bất kỳ endpoint nào của phân hệ Y bị hệ thống chặn truy cập (HTTP 403 Forbidden hoặc HTTP 302 chuyển hướng kèm thông báo từ chối truy cập), không cho phép thực hiện thao tác dữ liệu.
- [ ] Tài khoản được phân quyền quản trị toàn bộ hệ thống (toàn quyền) có thể truy cập và thao tác trên tất cả các phân hệ admin.

### Giao diện Điều hướng Menu
- [ ] Khi đăng nhập bằng tài khoản chỉ có quyền trên một số phân hệ nhất định, thanh menu quản trị chỉ hiển thị các mục tương ứng với những phân hệ đó, ẩn các mục phân hệ không có quyền.

### Kiểm thử và Biên dịch
- [ ] Dự án biên dịch thành công (`dotnet build`) không phát sinh bất kỳ lỗi nào (0 errors).
- [ ] Toàn bộ kịch bản kiểm thử tự động xác minh phân quyền và các kịch bản kiểm thử hiện có của dự án được thực thi thành công 100%.


## 2026-11-07T09:11:05Z

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
