# Báo Cáo Khảo Sát & Phân Tích Responsive Giao Diện Người Dùng Bên Ngoài (Hotspot Analysis)

**Tác giả**: teamwork_explorer instance 2  
**Nhiệm vụ**: Public Views & Responsive Hotspot Explorer  
**Thời gian hoàn thành**: 2026-11-07T09:22:00Z  
**Phạm vi**: Toàn bộ Controller, Razor View, Stylesheet và Layout bên ngoài khu vực quản trị (`/admin`)

---

## 1. Observation (Dữ Liệu Khảo Sát Thực Tế)

### 1.1. Danh mục Tuyến đường Công Khai (Public Routes) và File Razor View Tương Ứng

| Tuyến Đường (Route Path) | Controller & Action | File Razor View & Layout | Vai Trò & Chức Năng |
| :--- | :--- | :--- | :--- |
| `/` hoặc `/Home/Index` | `HomeController.Index` | `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Trang chủ: Hero slider, sự kiện đếm ngược, về dự án, tivi khoảnh khắc, bản tin, nhà tài trợ, FAQ, khám phá |
| `/Home/ChuongTrinh` | `HomeController.ChuongTrinh` | `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Lộ trình và các chặng hoạt động tình nguyện theo năm |
| `/Home/GayQuy` | `HomeController.GayQuy` | `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Kênh đóng góp, mã VietQR quyên góp động, báo cáo thu chi, danh sách đơn vị tài trợ |
| `/Home/BanToChuc` | `HomeController.BanToChuc` | `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Sơ đồ cây tổ chức 3 cấp: Ban sáng lập (Cấp 1), Ban thường trực (Cấp 2), Thành viên BTC (Cấp 3) |
| `/Home/TinTuc` | `HomeController.TinTuc` | `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Danh sách bài viết bản tin hành trình, modal đọc tin, nút like bài viết |
| `/Home/KyUc` | `HomeController.KyUc` | `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Hồi ức theo năm chiến dịch, tổng kết hành trình, gallery ảnh dạng lưới, nhà tài trợ |
| `/Home/LoiChuc` | `HomeController.LoiChuc` | `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Lời chúc tình nguyện viên (Masonry column grid), nút nổi gửi lời chúc |
| `/Home/ComingSoon` | Fallback view khi tắt chiến dịch | `WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml`<br>*(Không dùng layout, Layout = null)* | Trang thông báo hệ thống đang bảo trì/cập nhật |
| `/Home/Privacy` | `HomeController.Privacy` | `WEB_TinhNguyen/GiaoDien/Home/Privacy.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Chính sách bảo mật |
| `/Home/Error` | `HomeController.Error` | `WEB_TinhNguyen/GiaoDien/Shared/Error.cshtml`<br>*(Dùng Layout `_Layout.cshtml`)* | Trang hiển thị lỗi hệ thống |
| `/CuaHang` hoặc `/CuaHang/Index` | `CuaHangController.Index` (`[Area("CuaHang")]`) | `WEB_TinhNguyen/Areas/CuaHang/Views/CuaHang/Index.cshtml`<br>*(Dùng Layout `_CuaHangLayout.cshtml`)* | Danh mục sản phẩm gây quỹ, tìm kiếm live, lọc danh mục, lọc khoảng giá, phân loại 100% quỹ |
| `/CuaHang/ChiTiet/{id}` | `CuaHangController.ChiTiet` (`[Area("CuaHang")]`) | `WEB_TinhNguyen/Areas/CuaHang/Views/CuaHang/ChiTiet.cshtml`<br>*(Dùng Layout `_CuaHangLayout.cshtml`)* | Chi tiết vật phẩm gây quỹ, chọn số lượng, thêm giỏ/mua ngay, thông điệp gây quỹ, sản phẩm liên quan |
| `/CuaHang/GioHang` hoặc `/CuaHang/GioHang/Index` | `GioHangController.Index` (`[Area("CuaHang")]`) | `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/Index.cshtml`<br>*(Dùng Layout `_CuaHangLayout.cshtml`)* | Giỏ hàng, cập nhật số lượng AJAX, form đặt hàng thanh toán (Họ tên, SĐT, Địa chỉ, Ủng hộ thêm, VietQR/COD) |
| `/CuaHang/GioHang/DatHangThanhCong/{id}` | `GioHangController.DatHangThanhCong` (`[Area("CuaHang")]`) | `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml`<br>*(Dùng Layout `_CuaHangLayout.cshtml`)* | Xác nhận đặt hàng thành công, tiến độ 4 bước, mã VietQR thanh toán tự động, thông tin nhận hàng, in biên lai |
| Layout Chung Hệ Thống | Dùng cho tất cả view ngoài trừ CuaHang | `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` | Header, menu desktop, liquid bottom nav, cart button nổi, footer |
| Layout Cửa Hàng Riêng Biệt | Dùng cho phân hệ storefront Cửa hàng & Giỏ hàng | `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` | Header cửa hàng, menu desktop, liquid bottom nav, cart button nổi, footer cửa hàng |

---

### 1.2. Phân Định Ranh Giới Khu Vực Quản Trị (Admin Isolation Boundary)

Đối chiếu chặt chẽ với Yêu cầu **R2** ("Tuyệt đối không thay đổi mã nguồn, bố cục, kiểu dáng (CSS) hoặc hành vi của các trang và route thuộc khu vực quản trị `/admin`"):

**Toàn bộ các file SAU ĐÂY LÀ KHU VỰC ADMIN - CẤM CHỈNH SỬA**:
- Controllers Quản trị:
  - `WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminTaiKhoanController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs`
  - `WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs`
  - `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs`
  - `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs`
- Razor Views Quản trị:
  - `WEB_TinhNguyen/GiaoDien/Admin/**` (Index.cshtml, Login.cshtml)
  - `WEB_TinhNguyen/GiaoDien/AdminBanToChuc/**`
  - `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/**`
  - `WEB_TinhNguyen/GiaoDien/AdminChuongTrinh/**`
  - `WEB_TinhNguyen/GiaoDien/AdminGayQuy/**`
  - `WEB_TinhNguyen/GiaoDien/AdminHieuUng/**`
  - `WEB_TinhNguyen/GiaoDien/AdminKyUc/**`
  - `WEB_TinhNguyen/GiaoDien/AdminTaiKhoan/**`
  - `WEB_TinhNguyen/GiaoDien/AdminTinTuc/**`
  - `WEB_TinhNguyen/GiaoDien/AdminTrangChu/**`
  - `WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/**`
  - `WEB_TinhNguyen/Areas/CuaHang/Views/AdminSanPham/**`
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`

*Ghi chú thêm về cô lập CuaHang*: Bộ test `adversarial_final_stress_harness.js` (201 test) và `adversarial_store_isolation_test.js` (109 test) nghiêm ngặt kiểm tra tính cô lập của phân hệ `/cuahang`: Tuyệt đối không được kích hoạt Dark Mode trong `/cuahang`, không được hiển thị nút gạt Theme trên layout CuaHang, và phải bảo toàn giá trị theme của người dùng khi chuyển hướng qua lại.

---

### 1.3. Khảo Sát Bố Cục & Các Thành Phần UI (Forms, Tables, Lists, Cards, Buttons)

| View | Thành Phần Layout & Bố Cục | Forms & Biểu Mẫu Nhập Liệu | Bảng Dữ Liệu / Lists | Cards & Banners | Buttons & Interactive Elements |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **_Layout.cshtml** | `header fixed`, `main min-h-screen pt-[60px] md:pt-[72px]`, `footer`, `navigation fixed bottom-[15px] md:hidden` | Không | Menu `ul li` (7 items trong bottom nav), Dropdown Mùa ký ức | Floating cart badge, Theme toggle buttons | Nút toggle theme (desktop `w-10 h-10`, mobile `w-9 h-9`), nút giỏ hàng nổi |
| **_CuaHangLayout.cshtml** | `header fixed`, `main min-h-screen pt-[60px] pb-16`, `footer`, `navigation fixed bottom-[15px] md:hidden` | Không | Menu `ul li` (7 items), Dropdown Ký ức | Floating cart badge | Nút giỏ hàng nổi (`bottom-24 md:bottom-8`) |
| **Home/Index.cshtml** | 8 sections dọc: Banner slider, Countdown, Về dự án, TV Khoảnh khắc, Bản tin, Đơn vị tài trợ, FAQ Accordion, Khám phá | Không | FAQ accordion list (`.faq-item`), Explore links list | Countdown card (`rounded-[2.5rem] p-8`), News cards (`aspect-[16/10]`), Sponsor logos (`w-36 h-36`) | Nút CTA đếm ngược, nút next/prev swiper, nút Like tin tức, nút mở Modal tin tức |
| **Home/ChuongTrinh.cshtml** | Hero section 2 cột, Timeline các chặng hành trình | Không | Danh sách lộ trình hoạt động chi tiết (`ChiTietLoTrinhs`) | Card chặng chương trình (`rounded-3xl border-4`), Box hoạt động | Nút CTA cuộn chặng, nút đi tới Gây quỹ |
| **Home/GayQuy.cshtml** | Hero section, Grid kênh đóng góp, Section báo cáo thu chi, Section nhà tài trợ | Form QR Modal, Sponsor Modal | Danh sách thông tin tài khoản ngân hàng (flex justify-between) | Card kênh ủng hộ (`max-w-[360px] p-7`), Card nhà tài trợ (`w-44 sm:w-56`) | Nút "QR Tài khoản" / "Mở QR Modal", nút link ngoài, nút xem báo cáo chi tiết, nút đóng modal |
| **Home/BanToChuc.cshtml** | Sơ đồ phân cấp 3 tầng: Cấp 1 (Đứng đầu), Cấp 2 (Ban thường trực), Cấp 3 (Thành viên BTC) | Không | Danh sách chức vụ gạch đầu dòng (`ul.space-y-2.5`), Grid thành viên Cấp 3 (`grid-cols-1 sm:grid-cols-2 lg:grid-cols-4`) | Card Cấp 1 pill avatar tròn, Card Cấp 2 avatar `-top-11`, Card Cấp 3 member card | Nút Facebook link (`h-6 w-6` hoặc `h-7 w-7`), nút "Xem thêm thành viên" |
| **Home/TinTuc.cshtml** | Hero banner, Grid bài viết bản tin (`md:grid-cols-3`), Modal đọc chi tiết | Modal popup bài viết (`#newsReaderModal`) | Danh sách ảnh đính kèm bài viết | Card bài viết (`rounded-3xl`, ảnh `16:10`) | Nút đọc tiếp, nút thả tim (`likePost`), nút xem bài gốc Fanpage, nút đóng modal |
| **Home/KyUc.cshtml** | Hero banner, Tổng kết hành trình (HTML rich text), Gallery ảnh lưới (`grid-cols-2 sm:grid-cols-3 lg:grid-cols-5`), Đơn vị đồng hành | Không | Thư viện ảnh khoảnh khắc | Card ảnh khoảnh khắc (`aspect-square`), Card nhà tài trợ (`w-44 sm:w-56`) | Nút mở xem ảnh, nút xem tài trợ |
| **Home/LoiChuc.cshtml** | Hero banner, Call-to-action gửi lời chúc, Ô tìm kiếm TNV, Masonry grid lời chúc (`columns-1 sm:columns-2 md:columns-3 lg:columns-4`) | Input tìm kiếm TNV | Danh sách lời chúc dạng thiệp bưu thiếp | Card bưu thiếp có dán tem trang trí, ảnh Polaroid | Nút "Gửi lời chúc" CTA, Nút Floating button gửi lời chúc ở góc dưới phải |
| **Home/ComingSoon.cshtml** | Layout độc lập căn giữa màn hình (`min-h-screen flex items-center justify-center`) | Không | Không | Card thông báo trung tâm (`max-w-2xl px-6`) | Nút cập nhật Fanpage, nút Tải lại trang (`w-full sm:w-auto`) |
| **CuaHang/Index.cshtml** | Hero banner với 4 chỉ số tác động, Thanh công cụ tìm kiếm và lọc, Grid sản phẩm | Form tìm kiếm `filterForm` (input `q`, select `sort`) | Thanh trượt danh mục (`overflow-x-auto`), Thanh trượt khoảng giá (`overflow-x-auto`) | Product cards (`grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4`) | Nút tìm kiếm, nút lọc nhanh giá, nút xem chi tiết, nút thêm nhanh vào giỏ AJAX (`w-9 h-9`) |
| **CuaHang/ChiTiet.cshtml** | Breadcrumbs, Card chi tiết sản phẩm 2 cột (`lg:grid-cols-12`), Khung mô tả chi tiết, Sản phẩm liên quan | Form thêm vào giỏ (`#detailAddToCartForm`) | Danh sách cam kết niềm tin (3 cột) | Card chi tiết lớn, Card thông điệp gây quỹ nhân văn, Card sản phẩm liên quan | Bộ tăng giảm số lượng (`-` / `+`), Nút "Thêm vào giỏ", Nút "Mua ngay" (`w-full` trên mobile) |
| **GioHang/Index.cshtml** | Breadcrumbs, 2 cột: Cột trái (7 cols) giỏ hàng, Cột phải (5 cols) checkout form | **Biểu mẫu đặt hàng CheckoutForm**: Họ tên, SĐT, Email, Địa chỉ, Ghi chú lời chúc, Tiền ủng hộ thêm, Radio PT thanh toán (VietQR/COD) | Danh sách sản phẩm trong giỏ (ảnh, tên, giá, % quỹ, bộ số lượng, thành tiền, nút xóa), Bảng tổng kết tài chính | Khung giỏ hàng, Khung checkout form sticky | Bộ nút tăng/giảm số lượng AJAX (`w-8 h-8`), Nút xóa dòng, Nút xóa tất cả giỏ, Nút gợi ý ủng hộ nhanh (+20k, +50k...), Nút "XÁC NHẬN ĐẶT HÀNG & ỦNG HỘ QUỸ" |
| **GioHang/DatHangThanhCong.cshtml** | Card thành công, Tiến độ đơn hàng 4 bước (`grid-cols-2 md:grid-cols-4`), Khung thanh toán VietQR, Thông tin người nhận, Danh sách sản phẩm đã đặt | Không (chỉ hiển thị) | Danh sách sản phẩm chi tiết trong đơn, Bảng thanh toán | Khung mã VietQR động, Khung thông tin ngân hàng kèm nút sao chép | Nút sao chép mã đơn/STK/nội dung, Nút "In biên lai đóng góp", Nút "Tiếp tục mua sắm", Nút "Về Trang chủ" |

---

## 2. Logic Chain (Chuỗi Suy Luận Phân Tích & Xác Định Hotspot)

### 2.1. Hotspot 1: Thiếu Menu Hamburger / Drawer Trên Header Di Động
- **Quan sát thực tế**:
  - `_Layout.cshtml` (dòng 454) và `_CuaHangLayout.cshtml` (dòng 199): Thanh điều hướng chính được đặt lớp `class="hidden md:flex items-center gap-1 lg:flex"`. Khi màn hình `< 768px` (đặc biệt là 375px - 430px), toàn bộ menu header bị ẩn hoàn toàn.
  - Header không hề có nút bấm hamburger (`fa-bars`) cũng như không có drawer/slide-out menu trượt ra.
  - Website chỉ phụ thuộc vào thanh liquid bottom navigation gắn cố định ở đáy màn hình (`fixed bottom-[15px]`).
- **Đối chiếu Tiêu chí chấp nhận (Acceptance Criteria)**:
  - Yêu cầu ghi rõ: *"Menu điều hướng trên di động hiển thị gọn gàng (dạng hamburger hoặc drawer di động) và đóng/mở hoạt động chính xác khi tương tác trên màn hình nhỏ."*
- **Hệ quả & Rủi ro**:
  - Người dùng truy cập trên di động không tìm thấy menu điều hướng ở góc trên quen thuộc.
  - Không đạt tiêu chí kiểm thử tự động của bài test di động chuẩn mực.
  - Cần bổ sung nút Hamburger trên Header di động và Sidebar/Drawer Menu trượt mở mượt mà với backdrop overlay.

### 2.2. Hotspot 2: Thanh Liquid Bottom Navigation Bị Chật Chội Trên Màn Hình 375px
- **Quan sát thực tế**:
  - `_Layout.cshtml` (dòng 314-328) và `_CuaHangLayout.cshtml` (dòng 79-93):
    ```css
    .navigation { width: 96%; height: 70px; ... }
    ```
  - Trong thanh điều hướng có tới **7 mục menu**: Trang chủ, Cửa hàng, Sự kiện, Ký ức, Đóng góp, Tổ chức, Tin tức (dòng 597-640).
  - Trên màn hình chuẩn 375px (iPhone SE/12 mini):
    - Chiều rộng 96% của 375px là `360px`.
    - `360px / 7 items = 51.4px` cho mỗi nút.
    - Nhãn chữ bên dưới icon (`.navigation ul li a .text`) có `font-size: 10px`, `white-space: nowrap`. Các từ tiếng Việt như "Trang chủ", "Cửa hàng", "Đóng góp", "Tổ chức", "Tin tức" có độ dài từ 40px đến 60px.
    - Con trỏ chuyển động (`.navigation .indicator`) có kích thước cố định `width: 60px; height: 60px`. Do 60px > 51.4px, con trỏ tròn to hơn slot của từng item, gây đè lấn sang hai nút bên cạnh khi kích hoạt.
- **Hệ quả & Rủi ro**:
  - Chữ bị đè lấn hoặc sát mép nhau, khó nhìn và mất cân đối thị giác.
  - Khi đã có Drawer Menu đầy đủ các mục, Bottom Navigation nên tinh chỉnh lại gọn hơn (ví dụ 4-5 mục thiết yếu nhất: Trang chủ, Cửa hàng, Đóng góp, Giỏ hàng, Menu mở thêm) hoặc cấu hình kích thước font/indicator co giãn linh hoạt theo tỷ lệ phần trăm thay vì cố định 60px.

### 2.3. Hotspot 3: Xung Đột & Đè Chồng Các Nút Nổi (Floating Action Elements) Ở Đáy Màn Hình
- **Quan sát thực tế**:
  - `_Layout.cshtml` (dòng 596): `.navigation` cố định tại `bottom: 15px`, chiều cao `70px` (chiếm từ y = 15px đến 85px tính từ đáy).
  - `_Layout.cshtml` (dòng 645): `#floatingCartBtn` cố định tại `bottom-24` (tương đương `bottom: 96px`), `right-5`.
  - `LoiChuc.cshtml` (dòng 109): Nút "Gửi lời chúc" nổi đặt lớp `class="fixed bottom-[max(1.25rem,env(safe-area-inset-bottom))] right-5 z-40"`. Vị trí `1.25rem = 20px`!
- **Hệ quả & Rủi ro**:
  - Trên trang `/Home/LoiChuc`, nút "Gửi lời chúc" nằm ngay ở vị trí `bottom: 20px, right-5`, đè trực tiếp lên thanh Bottom Navigation hoặc bị che lấp hoàn toàn phía sau thanh điều hướng, khiến người dùng không thể bấm được.
  - Cần nâng `bottom` của nút nổi trang Lời chúc lên trên thanh bottom nav (tối thiểu `bottom: 100px` trên mobile) hoặc tích hợp vào giao diện trang thay vì thả nổi chồng chéo.

### 2.4. Hotspot 4: Cỡ Chữ Tiêu Đề Cố Định Quá Lớn Gây Vỡ Dòng Trên Màn Hình 375px
- **Quan sát thực tế**:
  - `TinTuc.cshtml` (dòng 94):
    ```html
    <h1 class="tintuc-display pb-3 font-bold leading-[1.18] sm:pb-5 text-volunteer-text dark:text-white"
        style="font-size: @(Model?.Hero_CoChuTieuDe ?? "64px");">
    ```
    Font cursive viết tay `Dancing Script` với kích cỡ cố định `64px` trên điện thoại 375px. Tiêu đề "Câu chuyện từ hành trình" dài 25 ký tự, chữ 64px sẽ chiếm hơn 400px nếu không gãy dòng, hoặc phải gãy thành 4 dòng chữ khổng lồ choán hết màn hình.
  - `GayQuy.cshtml` (dòng 69):
    ```html
    <h1 ... style="font-size: @(Model?.Hero_CoChuTieuDe ?? "48px");">
    ```
    Font in hoa "HÌNH THỨC TRAO GỬI YÊU THƯƠNG" cỡ `48px` trên màn hình 375px.
  - `Index.cshtml` (dòng 167):
    ```html
    <h1 class="font-display text-5xl md:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color">
    ```
    `text-5xl` tương đương `48px` trên mobile.
- **Hệ quả & Rủi ro**:
  - Chữ bị tràn viền, hoặc gãy chữ từng từ đơn lẻ, gây mất thẩm mỹ và chiếm toàn bộ chiều cao màn hình trước khi người dùng kịp cuộn thấy nội dung.
  - Cần áp dụng media query hoặc responsive classes: ví dụ `text-3xl sm:text-5xl md:text-6xl`, dùng CSS `clamp(1.75rem, 5vw, ...)` hoặc ghi đè inline font-size trên màn hình nhỏ.

### 2.5. Hotspot 5: Khối Dòng Sản Phẩm Trong Giỏ Hàng (`GioHang/Index.cshtml`) Nguy Cơ Tràn Ngang
- **Quan sát thực tế**:
  - `GioHang/Index.cshtml` (dòng 86, 110-142):
    ```html
    <div class="p-6 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4" id="cartRow_@item.IDSanPham">
        <!-- Trái: Ảnh (80px) + Tên sản phẩm + Giá -->
        <div class="flex items-center space-x-4 flex-1 min-w-0">...</div>
        <!-- Phải: Bộ tăng giảm số lượng + Thành tiền + Nút xóa -->
        <div class="flex items-center space-x-4 self-end sm:self-center">
            <div class="flex items-center ..."> <!-- Bộ nút +/-: 32 + 48 + 32 = 112px --> </div>
            <div class="text-right min-w-[90px]"> <!-- min-w 90px --> </div>
            <button ...> <!-- Nút xóa: 32px --> </button>
        </div>
    </div>
    ```
  - Chiều rộng màn hình di động: `375px`.
  - Padding ngoài container: `px-4` (trái 16px + phải 16px = 32px) -> còn `343px`.
  - Padding trong khung thẻ giỏ hàng: `p-6` (trái 24px + phải 24px = 48px) -> không gian thực tế còn `295px`.
  - Tổng chiều rộng tối thiểu của cụm điều khiển bên phải: `112px (bộ nút) + 16px (gap) + 90px (thành tiền) + 16px (gap) + 32px (nút xóa) = 266px`.
  - Nếu số tiền lớn (ví dụ 1.500.000₫ hay 12.000.000₫), `min-w-[90px]` sẽ nở rộng lên 110-120px. Khi đó tổng chiều rộng cụm này đạt gần `290px - 300px`, sát nút với 295px, có thể đẩy mép phải gây xuất hiện thanh cuộn ngang cục bộ.
- **Hệ quả & Rủi ro**:
  - Trên màn hình 375px, khi giỏ hàng có số tiền hàng triệu đồng, khung dòng có thể bị tràn.
  - Cần tinh chỉnh trên màn hình nhỏ: giảm padding thẻ từ `p-6` thành `p-4 sm:p-6`, thu hẹp `space-x-4` thành `space-x-2 sm:space-x-4`, và cho phép thành tiền hiển thị linh hoạt không bị gò bó.

### 2.6. Hotspot 6: Cú Pháp Chuyển Khoản Ngân Hàng Không Tự Ngắt Dòng (`GayQuy.cshtml`)
- **Quan sát thực tế**:
  - `GayQuy.cshtml` (dòng 188-191 và 246-249):
    ```html
    <div class="flex justify-between">
        <span class="text-slate-400">Cú pháp:</span>
        <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all">@effectiveMemo</span>
    </div>
    ```
  - Cú pháp mặc định: `[Họ và tên]_DongGopLangbiang` (26 ký tự liền nhau không có dấu cách).
  - Thẻ `div` sử dụng `flex justify-between` nhưng không có `break-all` hay `min-w-0`. Nếu người dùng cấu hình một cú pháp dài hơn (ví dụ `HOVATEN_UNGHO_TRANGBIANG_2026`), chuỗi chữ không có dấu cách sẽ đẩy thẻ phình to hơn chiều rộng `max-w-[360px]` của card, làm card tràn ra ngoài màn hình 375px.
- **Hệ quả & Rủi ro**:
  - Gây ra `document.scrollingElement.scrollWidth > window.innerWidth`.
  - Cần thêm `break-all` hoặc `text-right truncate` vào thẻ hiển thị cú pháp và tài khoản.

### 2.7. Hotspot 7: Kích Thước Vùng Chạm (Touch Target Sizing) Dưới Chuẩn 44x44px
- **Quan sát thực tế**:
  - `BanToChuc.cshtml` (dòng 414 & 527):
    - Nút icon Facebook Cấp 2: `h-6 w-6` (24x24px).
    - Nút icon Facebook Cấp 3: `h-7 w-7` (28x28px).
  - `CuaHang/Index.cshtml` (dòng 274):
    - Nút thêm nhanh vào giỏ: `w-9 h-9` (36x36px).
  - `GioHang/Index.cshtml` (dòng 113, 123, 138, 244):
    - Nút giảm/tăng số lượng: `w-8 h-8` (32x32px).
    - Nút xóa sản phẩm: `p-1.5` (~28x28px).
    - Các nút tag ủng hộ nhanh (+20k, +50k...): `px-2.5 py-1 text-xs` (chiều cao ~26px).
  - `_Layout.cshtml` (dòng 505):
    - Nút toggle theme mobile: `w-9 h-9` (36x36px).
- **Hệ quả & Rủi ro**:
  - Ngón tay người dùng trên điện thoại rất khó chạm chính xác vào các nút 24px-32px, dễ dẫn đến bấm trượt hoặc ấn nhầm.
  - Cần tăng kích thước vùng chạm (padding vô hình hoặc kích thước tối thiểu `min-w-[40px] min-h-[40px]`).

### 2.8. Hotspot 8: Ô Nhập Liệu Input Có Cỡ Chữ Nhỏ Hơn 16px Gây Phóng To (Auto-zoom) Trên iOS
- **Quan sát thực tế**:
  - `GioHang/Index.cshtml` (dòng 188, 199, 207, 218, 228, 240): Toàn bộ các thẻ `input` và `textarea` đều dùng lớp `text-sm` (14px).
- **Cơ chế trình duyệt Safari iOS**:
  - Trên iPhone (kể cả Safari và Chrome/Firefox trên iOS do dùng chung WebKit engine), khi người dùng nhấp vào một ô `<input>` có `font-size < 16px`, trình duyệt sẽ **tự động zoom màn hình vào ô nhập liệu đó**. Sau khi zoom, toàn bộ giao diện bị phóng to quá khổ và xuất hiện thanh cuộn ngang ngoài ý muốn!
- **Hệ quả & Rủi ro**:
  - Vi phạm trực tiếp Acceptance Criteria: *"Không có trang công khai nào bị thanh cuộn ngang ngoài ý muốn (`document.scrollingElement.scrollWidth <= window.innerWidth`) trên các viewport di động từ 375px trở lên."*
  - Cần nâng cấp các thẻ input/textarea thành `text-base sm:text-sm` (16px trên mobile, 14px trên desktop).

---

## 3. Caveats (Các Điểm Lưu Ý & Ràng Buộc Kỹ Thuật)

1. **Ràng buộc kiểm thử tự động hiện hữu (Tier 1 F06 & Challenger Tests)**:
   - File `tests/E2ETests/Tier1/Tier1_StorefrontTests.cs` (kiểm tra F06_T01 đến F06_T05) kiểm tra sự tồn tại của các thuộc tính:
     - `fixed bottom` hoặc `bottom-0` hoặc `fixed inset-x-0`
     - Lớp `md:hidden`
     - Liên kết tới `CuaHang/Index`
   - Vì vậy, **KHÔNG ĐƯỢC XÓA BỎ HOÀN TOÀN** thanh bottom nav, mà cần giữ lại hoặc bổ sung thêm Drawer Menu song song để vừa đáp ứng tiêu chí Hamburger/Drawer mới, vừa vượt qua 100% 536 bài test E2E hiện có.
2. **Ràng buộc cô lập Cửa Hàng (Store Theme Isolation)**:
   - Bộ test `adversarial_store_isolation_test.js` (109 bài test) kiểm tra việc cấm triệt để nút bấm theme toggle trên các view của `Areas/CuaHang/Views` và layout `_CuaHangLayout.cshtml`.
   - Khi chỉnh sửa Drawer Menu hoặc Header của Cửa hàng, **tuyệt đối không đưa nút chuyển đổi Dark/Light mode vào phân hệ Cửa Hàng**.
3. **Môi trường Database Offline**:
   - Website được thiết kế chạy kép (Dual-mode): Tự động kết nối SQL Server và fallback In-Memory khi offline. Bất kỳ view nào cũng phải kiểm tra `null` an toàn với Model và ViewBag (ví dụ `Model?.Items ?? new List<CartItem>()`).

---

## 4. Conclusion (Đánh Giá Tổng Kết & Danh Sách Đề Xuất Ưu Tiên)

Giao diện người dùng hiện tại đã có nền tảng Tailwind CSS tốt, tuy nhiên trên màn hình di động 375px - 430px vẫn tồn tại **8 điểm nghẽn (hotspots)** chính cần được cải tiến để đạt chuẩn Responsive Web toàn diện và vượt qua bộ kiểm thử tự động.

### Danh Sách Cải Tiến View Cần Thực Hiện Theo Mức Độ Ưu Tiên:

#### Mức Ưu Tiên Cao Nhất (P0 - Cần Thực Hiện Ngay):
1. **Bổ sung Mobile Drawer Navigation Menu trên Header (`_Layout.cshtml` & `_CuaHangLayout.cshtml`)**:
   - Thêm nút Hamburger (`<button id="mobileMenuBtn" aria-label="Mở menu điều hướng">`) trên header cho màn hình `md:hidden`.
   - Xây dựng Drawer / Slide-over Menu trượt từ cạnh phải hoặc trái màn hình với hiệu ứng mượt mà, hỗ trợ đóng/mở bằng nút X, nhấn phím ESC hoặc nhấn ra backdrop mờ.
   - Hiển thị đầy đủ cây liên kết điều hướng: Trang chủ, Cửa hàng gây quỹ, Giỏ quà, Chương trình, Đóng góp, Ban tổ chức, Tin tức, Ký ức các mùa.
2. **Ngăn chặn triệt để hiện tượng cuộn ngang trên toàn bộ trang công khai (`overflow-x-hidden`)**:
   - Bổ sung `overflow-x-hidden` trên thẻ `body` và thẻ bao bọc chính của `_Layout.cshtml` và `_CuaHangLayout.cshtml`.
   - Khắc phục các cỡ chữ tiêu đề khổng lồ cố định:
     - `TinTuc.cshtml` (dòng 94): Thay `font-size: 64px` bằng `text-3xl sm:text-5xl md:text-6xl`.
     - `GayQuy.cshtml` (dòng 69): Thay `font-size: 48px` bằng `text-2xl sm:text-4xl md:text-5xl`.
     - `Index.cshtml` (dòng 167): Đổi `text-5xl` thành `text-3xl sm:text-5xl md:text-6xl`.
3. **Khắc phục ô nhập liệu gây Zoom trên iOS Safari (`GioHang/Index.cshtml`)**:
   - Đổi toàn bộ các trường `input` và `textarea` trong form đặt hàng từ `text-sm` thành `text-base sm:text-sm` để đảm bảo cỡ chữ tối thiểu 16px trên mobile, ngăn chặn triệt để lỗi Safari tự động phóng to làm tràn màn hình.

#### Mức Ưu Tiên Cao (P1 - Cải Thiện Trải Nghiệm & Bố Cục):
4. **Giải tỏa chồng lấn các nút nổi ở góc dưới (`_Layout.cshtml` & `Home/LoiChuc.cshtml`)**:
   - Trên `Home/LoiChuc.cshtml` (dòng 109), điều chỉnh vị trí nút "Gửi lời chúc" nổi lên trên thanh bottom nav (`bottom-24` hoặc `bottom-28`), tránh nằm đè lên thanh điều hướng di động.
   - Điều chỉnh thanh Bottom Navigation: Giảm bớt số lượng tab hoặc thu nhỏ padding/indicator hợp lý để không bị chen chúc trên màn hình 375px.
5. **Tối ưu hóa bảng dòng sản phẩm giỏ hàng trên di động (`GioHang/Index.cshtml`)**:
   - Đổi padding thẻ dòng giỏ hàng từ `p-6` thành `p-4 sm:p-6`.
   - Đổi khoảng cách cụm nút tăng giảm số lượng từ `space-x-4` thành `space-x-2 sm:space-x-4`.
   - Cho phép hiển thị thành tiền dòng gọn gàng, linh hoạt co giãn.
6. **Thêm `break-all` phòng vệ cho chuỗi ký tự chuyển khoản ngân hàng (`GayQuy.cshtml`)**:
   - Bổ sung `break-all` cho `span` hiển thị cú pháp và số tài khoản ngân hàng trong các card quyên góp.

#### Mức Ưu Tiên Trung Bình (P2 - Tối Ưu Vùng Chạm & Độ Tương Phản):
7. **Tăng kích thước vùng chạm (Touch Targets) đạt chuẩn 40x40px - 44x44px**:
   - Tăng nút icon Facebook trong `BanToChuc.cshtml` từ `h-6 w-6` / `h-7 w-7` lên kích thước bấm tối thiểu `h-9 w-9` (36px) hoặc `h-10 w-10` (40px).
   - Tăng kích thước nút thêm giỏ trên card `CuaHang/Index.cshtml` lên `w-10 h-10`.
   - Tăng kích thước nút tăng/giảm số lượng trong `GioHang/Index.cshtml` lên `w-9 h-9`.
   - Tăng kích thước nút gạt Theme mobile trên `_Layout.cshtml` lên `w-10 h-10`.

---

## 5. Verification Method (Phương Pháp & Lệnh Kiểm Tra Độc Lập)

Sau khi agent thực thi hoàn thành việc điều chỉnh mã nguồn, người thẩm định độc lập có thể kiểm tra toàn diện bằng các bước sau:

### 5.1. Biên dịch Dự Án & Build CSS
```powershell
# 1. Biên dịch toàn bộ Solution và dự án WEB_TinhNguyen
dotnet build

# 2. Biên dịch Tailwind CSS sang site.css (đảm bảo không có lỗi cú pháp class mới)
npm run build:css
```

### 5.2. Chạy Toàn Bộ Bộ Test Tự Động Hiện Hữu
```powershell
# Chạy 536 bài test E2E (Tier 1 đến Tier 5)
dotnet run --project tests/E2ETests/E2ETests.csproj -- --all

# Chạy kiểm thử cách ly giao diện và chống rò rỉ theme Cửa hàng
node tests/adversarial_final_stress_harness.js
node tests/adversarial_store_isolation_test.js
```

### 5.3. Kiểm Tra Tính Toàn Vẹn Của Khu Vực Quản Trị (Admin Boundary)
```powershell
# Xác nhận không có bất kỳ file nào trong thư mục Admin bị chỉnh sửa
git status WEB_TinhNguyen/GiaoDien/Admin*
git status WEB_TinhNguyen/BoDieuKhien/Admin*
git status WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*
git status WEB_TinhNguyen/Areas/CuaHang/Views/Admin*
```

### 5.4. Kiểm Tra Tự Động Trên Trình Duyệt Với Các Viewport Di Động (375px, 390px, 414px, 430px)
Sử dụng Playwright hoặc Chrome DevTools Protocol để duyệt qua tất cả 14 trang công khai:
```javascript
// Kiểm tra không có thanh cuộn ngang ngoài ý muốn
const isOverflowing = await page.evaluate(() => {
    return document.scrollingElement.scrollWidth > window.innerWidth;
});
expect(isOverflowing).toBe(false);

// Kiểm tra menu drawer di động
const hamburgerBtn = page.locator('#mobileMenuBtn');
await expect(hamburgerBtn).toBeVisible();
await hamburgerBtn.click();
const drawer = page.locator('#mobileDrawer');
await expect(drawer).toBeVisible();
```
