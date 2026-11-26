# Báo Cáo Điều Tra & Phân Tích Kỹ Thuật: Tối Ưu Hóa Responsive Các View Home (Grids, Cards & Scaling Explorer)

**Tác giả**: teamwork_explorer instance 2 (`m2_explorer_2`)  
**Nhiệm vụ**: Home Views Grids, Cards & Scaling Explorer  
**Thời gian hoàn thành**: 2026-11-07T10:35:00Z  
**Phạm vi**: 4 Razor Views thuộc `WEB_TinhNguyen/GiaoDien/Home/` (`Index.cshtml`, `BanToChuc.cshtml`, `KyUc.cshtml`, `ChuongTrinh.cshtml`) và tương tác bố cục với `_Layout.cshtml`, đảm bảo `document.scrollingElement.scrollWidth <= window.innerWidth` trên các viewport di động từ 375px đến 430px.

---

## 1. Observation (Dữ Liệu Khảo Sát & Đo Đạc Thực Tế)

### 1.1. Khảo Sát Chi Tiết 4 Razor Views Của Home

#### A. `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`
- **Khối Countdown Timer (dòng 142, 150, 152)**:
  - Khung bao ngoài: `class="relative w-full py-16 px-2 md:px-8 z-10 block bg-gray-50/50 dark:bg-night-2"`
  - Swiper slide: `class="swiper-slide px-4 md:px-12 bg-transparent"`
  - Card đếm ngược: `class="countdown-card keep-theme mx-auto text-center backdrop-blur shadow-2xl rounded-[2.5rem] p-8 md:p-14 border border-white/50 w-full"`
  - Tiêu đề đếm ngược (dòng 167): `class="font-display text-5xl md:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color"`
  - Hộp đếm ngược (dòng 187-204): `class="countdown-boxes flex flex-wrap justify-center gap-3 md:gap-5"`
  - Mỗi ô đếm: `class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-2xl p-4 w-20 md:w-28 keep-color"`
  - Số đếm: `class="d-val text-3xl md:text-5xl font-black keep-color"`
  - Nhãn: `class="text-[10px] md:text-xs text-gray-500 font-bold mt-2 uppercase tracking-wider keep-color"`
  - Nút CTA (dòng 209-215): `class="flex flex-col sm:flex-row justify-center gap-4"` với các thẻ `<a class="px-8 py-3.5 rounded-full ...">`
- **Khối Nhà Tài Trợ & Đơn Vị Tổ Chức (dòng 388-398)**:
  - Lưới logo: `class="flex flex-wrap justify-center gap-4 sm:gap-6"`
  - Thẻ logo: `class="group relative flex items-center justify-center w-36 h-36 sm:w-44 sm:h-44 rounded-3xl bg-white dark:bg-gray-800 p-3 sm:p-5 shadow-sm ..."`
  - Tooltip hover (dòng 393): `class="absolute -bottom-10 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none whitespace-nowrap bg-gray-900 ..."` (`whitespace-nowrap` không giới hạn chiều dài ký tự).

#### B. `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`
- **Thẻ Cấp 1 (Người đứng đầu - dòng 294-337)**:
  - Khung Card: `class="relative bg-white rounded-2xl px-6 py-6 sm:px-10 sm:py-7 shadow-md max-w-2xl w-full flex flex-col sm:flex-row items-center gap-6 ..."`
  - Avatar: `class="w-28 h-28 sm:w-32 sm:h-32 rounded-full overflow-hidden bg-white p-1 shadow-md"`
  - Nút Facebook: `class="absolute bottom-0 right-0 h-8 w-8 rounded-full bg-[#1877F2] ..."`
  - Nhãn Pill Họ Tên: `class="inline-block px-6 py-1.5 rounded-full text-white font-bold text-sm sm:text-base uppercase tracking-wide shadow-sm"`
  - Chức vụ chính: `class="mt-2 text-lg sm:text-2xl font-extrabold uppercase tracking-wide"`
- **Thẻ Cấp 2 (Ban Thường trực - dòng 379-465)**:
  - Lưới: `class="level2-row grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-@colCount gap-x-6 gap-y-16"`
  - Card: `class="relative bg-white rounded-2xl pt-14 pb-6 px-5 shadow-md flex-1 flex flex-col justify-between ..."`
  - Avatar nhô lên: `class="w-[88px] h-[88px] rounded-full overflow-hidden bg-white p-1 shadow-md"`
  - Nút Facebook (dòng 414): `class="absolute bottom-0 right-0 h-6 w-6 rounded-full bg-[#1877F2] ..."` (`h-6 w-6` = 24x24px, dưới chuẩn touch target).
- **Thẻ Cấp 3 (Thành viên Ban Tổ Chức - dòng 495-548)**:
  - Khung bao ngoài: `class="mt-4 lg:mt-0 bg-white/70 backdrop-blur-md rounded-3xl p-6 sm:p-8 shadow-sm"`
  - Lưới: `class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5"`
  - Thẻ thành viên: `class="btc-member-card ... bg-white rounded-2xl p-4 shadow-sm ..."`
  - Avatar: `class="w-14 h-14 rounded-full object-cover p-0.5 bg-white shadow-sm"`
  - Nút Facebook (dòng 528): `class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full ..."` (`h-7 w-7` = 28x28px, dưới chuẩn touch target).

#### C. `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`
- **Hero Section (dòng 20-48)**:
  - Section: `class="relative flex items-center overflow-hidden px-6 text-center min-h-[90svh] py-24 sm:py-32 ..."`
  - Tiêu đề: `class="font-display pb-3 font-bold leading-[1.18] sm:pb-5 text-4xl sm:text-6xl md:text-7xl dynamic-font ..."`
- **Tổng Kết Hành Trình (dòng 51-69)**:
  - Khung bao ngoài: `class="mx-auto max-w-6xl px-6"`
  - Card nội dung: `class="relative rounded-3xl bg-emerald-50/50 dark:bg-volunteer-card p-6 sm:p-10 border border-emerald-200/70 ..."`
  - Nội dung HTML rich text: `class="tsl-prose text-base leading-relaxed text-volunteer-text/85 dark:text-slate-200 sm:text-lg"`
- **Thư Viện Ảnh (Gallery - dòng 72-97)**:
  - Lưới: `class="grid grid-cols-2 gap-4 sm:grid-cols-3 sm:gap-5 lg:grid-cols-5" id="lightgallery"`
  - Thẻ ảnh: `class="... rounded-3xl outline-none overflow-hidden @(img.LaAnhDoc ? "row-span-2" : "") ..."`
  - Div con (dòng 85): `class="group relative overflow-hidden rounded-3xl bg-emerald-50 dark:bg-white/5 h-full w-full aspect-square ..."` (chứa cả `h-full` và `aspect-square` khi ảnh dọc có `row-span-2`).
- **Khối Nhà Tài Trợ Ký Ức (dòng 123-142)**:
  - Khung bọc: `class="flex flex-wrap justify-center gap-5 sm:gap-6"`
  - Thẻ nhà tài trợ: `class="group relative flex h-28 w-44 items-center justify-center rounded-2xl bg-white dark:bg-volunteer-card p-5 ring-1 ring-emerald-100 dark:ring-white/10 transition hover:-translate-y-1.5 sm:h-32 sm:w-56 sm:p-6 shadow-sm hover:shadow-md cursor-pointer"`

#### D. `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`
- **Hero Section & Cụm Ảnh Hero (dòng 16-64)**:
  - Section: `class="relative overflow-hidden pt-28 pb-16 sm:pt-36 lg:pt-40 lg:pb-24 ... px-6"`
  - Tiêu đề: `class="text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font text-volunteer-text dark:text-white leading-tight"`
  - Ảnh Hero 1 (dòng 56): `class="absolute top-0 right-0 h-[70%] w-[65%] rounded-3xl overflow-hidden shadow-2xl z-20 hover:z-30 border-4 border-white dark:border-slate-800 transform translate-x-3 -translate-y-2 transition duration-500 hover:scale-105"` (`translate-x-3` dịch chuyển +12px sang phải).
- **Lộ Trình Các Chặng Hoạt Động (dòng 81-146)**:
  - Cột nội dung và ảnh: `flex flex-col lg:flex-row gap-12 items-center`
  - Khung box lộ trình chi tiết: `class="bg-white dark:bg-volunteer-card p-6 rounded-3xl border border-emerald-100 dark:border-white/10 shadow-lg backdrop-blur-sm"`
  - Đoạn đường nối timeline dọc (dòng 123): `class="w-0.5 h-8 my-1 bg-emerald-200 dark:bg-emerald-900"` (chiều cao cố định `h-8` = 32px, bị đứt đoạn khi nội dung hoạt động dài hơn 2 dòng).

---

### 1.2. Phát Hiện Khiếm Khuyết Nghiêm Trọng Trong Bộ Test `tests/run_mobile_viewport_browser_tests.js`

Khi chạy lệnh kiểm thử tự động trình duyệt:
```powershell
node tests/run_mobile_viewport_browser_tests.js
```
Kết quả ghi nhận lỗi tràn màn hình giả lập trên hàng loạt trang:
- `BanToChuc`: scrollWidth = 659px (excess: +284px)
- `Index`: scrollWidth = 400px (excess: +25px)
- `KyUc`: scrollWidth = 416px (excess: +41px)

**Truy nguyên mã nguồn file `tests/run_mobile_viewport_browser_tests.js` (dòng 222-225)**:
```javascript
const cssUrl = 'file:///' + SITE_CSS_PATH.replace(/\\/g, '/');
layout = layout.replace('~/css/site.css', cssUrl);
layout = layout.replace('/css/site.css', cssUrl);
```
- Dòng 223: Thay thế `~/css/site.css` thành `file:///D:/web_tinh_nguyen/WEB_TinhNguyen/wwwroot/css/site.css`.
- Dòng 224: Tìm chuỗi `/css/site.css` trong `layout`. Do chuỗi `cssUrl` vừa được chèn vào ở dòng 223 có chứa `/css/site.css` ở đuôi, lệnh `replace` ở dòng 224 tiếp tục thay thế chuỗi con này bằng `cssUrl` lần thứ 2!
- Kết quả URL file CSS được sinh ra trong mã HTML tạm:
  ```html
  <link rel="stylesheet" href="file:///D:/web_tinh_nguyen/WEB_TinhNguyen/wwwrootfile:///D:/web_tinh_nguyen/WEB_TinhNguyen/wwwroot/css/site.css" asp-append-version="true" />
  ```
- **Hệ quả**: Trình duyệt Headless Chrome không thể nạp file `site.css` (báo lỗi đường dẫn không hợp lệ). Khi đó toàn bộ Tailwind CSS bị vô hiệu hóa trong môi trường test, ảnh SVG mẫu 400x300 hiển thị ở kích cỡ gốc 400px khiến các trang bị kết luận sai là tràn màn hình 400px - 659px.

---

### 1.3. Khảo Sát Header Trên `_Layout.cshtml` Gây Tràn Ngang Khắp Các View Home

Khi sửa lỗi đường dẫn CSS ở trên, chạy lại kiểm thử phát hiện lỗi tràn màn hình thực tế từ Header chung của `_Layout.cshtml`:
```
scrollWidth=601px > viewport=375px (excess: +226px)
Overflow culprits:
  - <div> rect.right=492px (+119px) class="flex items-center gap-2 md:hidden"
  - <button id="themeToggleBtnMobile"> rect.right=437px (+64px)
```
- **Nguyên nhân tại `_Layout.cshtml` (dòng 475-492 & dòng 543)**:
  - Thẻ logo bên trái `<a href="/" class="flex min-w-0 items-center gap-2 sm:gap-2.5">` không có giới hạn `max-width` hoặc `flex-1`.
  - Thẻ `span` bọc tiêu đề (dòng 482) là thẻ `inline` (`class="min-w-0 leading-tight"`), không kích hoạt được thuộc tính `truncate` của thẻ con (`logo-font-main`).
  - Thẻ cụm điều khiển mobile bên phải (dòng 543) `class="flex items-center gap-2 md:hidden"` thiếu lớp `shrink-0`.
  - Khi tiêu đề website dài (hoặc chứa `<br/>`), khối logo bên trái nở rộng và đẩy toàn bộ nút chuyển theme và nút hamburger di động tràn ra ngoài mép phải màn hình 375px (vị trí `rect.right = 492px`).

---

## 2. Logic Chain (Chuỗi Suy Luận Đo Đạc & Phân Tích Kích Thước)

### 2.1. Phân Tích Toán Học Chi Tiết Cho Countdown Timer Trong `Index.cshtml`
- **Không gian khả dụng trên viewport 375px**:
  - Chiều rộng màn hình: `375px`.
  - Padding section: `px-2` (2 x 8px = 16px). Còn: `375 - 16 = 359px`.
  - Padding slide swiper: `px-4` (2 x 16px = 32px). Còn: `359 - 32 = 327px`.
  - Padding thẻ `countdown-card`: `p-8` (2 x 32px = 64px). Không gian thực tế còn: `327 - 64 = 263px`.
- **Kích thước 4 khối đếm ngược hiện tại**:
  - Mỗi ô có lớp `w-20` (= 80px). 4 ô = 4 x 80px = `320px`.
  - Khoảng cách `gap-3` (= 12px) giữa 4 ô (3 khoảng) = 3 x 12px = `36px`.
  - Tổng chiều rộng cần thiết để hiển thị 1 hàng: `320px + 36px = 356px`.
  - **So sánh**: `356px > 263px` (vượt quá 93px!).
  - **Hệ quả**: Thuộc tính `flex-wrap` buộc 4 ô phải bẻ thành 2 hàng (mỗi hàng 2 ô). Cách bố trí 2x2 làm mất tính thẩm mỹ của thanh đếm ngược sự kiện.
- **Giải pháp tối ưu hóa responsive**:
  - Giảm padding thẻ: `p-4 sm:p-8 md:p-14` (tiết kiệm 32px padding).
  - Giảm padding slide: `px-2 sm:px-6 md:px-12` (tiết kiệm 16px padding).
  - Không gian khả dụng mới tại 375px: `375 - 16 (section px-2) - 16 (slide px-2) - 32 (card p-4) = 311px`.
  - Thu nhỏ kích thước ô: `w-16 sm:w-20 md:w-28` (`w-16` = 64px, hoặc co giãn `flex-1 min-w-[62px] max-w-[76px]`).
  - Thu nhỏ khoảng cách: `gap-2 sm:gap-3 md:gap-5` (`gap-2` = 8px).
  - Thu nhỏ số đếm: `text-2xl sm:text-3xl md:text-5xl`.
  - Tính toán lại chiều rộng 1 hàng mới: `4 x 64px (ô) + 3 x 8px (gap) = 280px`.
  - **Kết quả**: `280px <= 311px`. Toàn bộ 4 khối Ngày - Giờ - Phút - Giây nằm vừa vặn, thẳng hàng trên 1 dòng duy nhất trên tất cả màn hình di động từ 375px trở lên!

---

### 2.2. Phân Tích Toán Học Cho Nhà Tài Trợ Trong `KyUc.cshtml`
- **Không gian khả dụng trên viewport 375px & 390px**:
  - Viewport 375px: Section `px-5` (40px) -> Khả dụng: `375 - 40 = 335px`.
  - Viewport 390px: Section `px-5` (40px) -> Khả dụng: `390 - 40 = 350px`.
- **Kích thước thẻ tài trợ hiện tại**:
  - Mỗi thẻ có lớp cố định `w-44` (= 176px).
  - Khoảng cách `gap-5` (= 20px).
  - Nếu xếp 2 thẻ 1 hàng: `176px + 176px + 20px = 372px`.
  - **So sánh tại 375px**: `372px > 335px` (vượt 37px).
  - **So sánh tại 390px**: `372px > 350px` (vượt 22px).
  - **Hệ quả**: Trên iPhone SE (375px) và iPhone 14/15 (390px), các thẻ tài trợ bị rớt dòng thành **1 thẻ đơn lẻ/hàng**, nằm trơ trọi giữa trang và chiếm chiều dài cuộn trang rất lớn.
- **Giải pháp tối ưu hóa responsive**:
  - Section padding: `px-4 sm:px-6` (khả dụng tại 375px: `375 - 32 = 343px`).
  - Kích thước thẻ tài trợ: `h-24 w-[150px] sm:h-32 sm:w-56 p-3 sm:p-6 rounded-xl sm:rounded-2xl`.
  - Khoảng cách: `gap-3 sm:gap-5 md:gap-6` (`gap-3` = 12px).
  - Chiều rộng 2 thẻ: `2 x 150px + 12px = 312px`.
  - **Kết quả**: `312px <= 343px`. Thẻ tài trợ hiển thị cân xứng 2 cột/hàng trên mọi màn hình di động 375px - 430px và tự động mở rộng lên `w-56 h-32` trên máy tính bảng/màn hình lớn.

---

### 2.3. Phân Tích Cấu Trúc Khung Lưới Thẻ Ban Tổ Chức Trong `BanToChuc.cshtml`
- **Tầng 3 (Cấp 3 - Thành viên BTC)**:
  - Container bao ngoài: `bg-white/70 rounded-3xl p-6 sm:p-8`.
  - Thẻ thành viên: `btc-member-card ... p-4`.
  - Tổng padding ngang dồn lại trên di động 375px: `2 x 24px (p-6) + 2 x 16px (p-4) = 80px`.
  - Không gian chiều rộng thẻ thực tế: `343px - 48px = 295px`.
  - Bố cục dòng thành viên: Avatar tròn (`w-14` = 56px) + gap (14px) + nút FB (`w-7` = 28px) + gap (14px) = 112px không gian cố định.
  - Không gian còn lại cho khối Họ tên & Chức vụ: `295px - 112px - 32px (card padding) = 151px`.
  - Với Họ tên tiếng Việt trung bình 18-25 ký tự, không gian 151px khiến tên bị cắt ngắn (`truncate`) cụt ngủn chỉ sau 10-12 ký tự.
- **Giải pháp tối ưu hóa responsive**:
  - Giảm padding container tầng 3: `p-3 sm:p-6 md:p-8 rounded-2xl sm:rounded-3xl` (tiết kiệm 24px).
  - Giảm padding thẻ thành viên: `p-3 sm:p-4 rounded-xl sm:rounded-2xl` (tiết kiệm 8px).
  - Không gian tên thành viên tăng từ 151px lên `183px` (+32px, tăng 21.2%), hiển thị trọn vẹn họ tên thành viên.
  - Nâng kích thước nút Facebook Cấp 2 và Cấp 3 từ `h-6 w-6` / `h-7 w-7` lên kích thước chạm chuẩn: `h-8 w-8 sm:h-7 sm:w-7` (32x32px) để ngón tay chạm bấm dễ dàng.

---

### 2.4. Phân Tích Lộ Trình & Hiệu Ứng Hero Trong `ChuongTrinh.cshtml`
- **Ảnh Hero Floating**:
  - Lớp `transform translate-x-3` dịch ảnh +12px sang mép phải, kết hợp cùng `hover:scale-105` trên màn hình 375px có thể chạm sát viền màn hình hoặc làm mất viền trắng trang trí.
  - Giải pháp: Chuyển thành `translate-x-0 sm:translate-x-3` và `border-2 sm:border-4`.
- **Khối Timeline Chặng & Đường Kẻ Dọc**:
  - Đoạn kẻ nối giữa 2 hoạt động hiện tại dùng chiều cao cứng: `h-8` (32px).
  - Khi nội dung hoạt động có 3-4 dòng chữ (khoảng 70-80px), đường kẻ 32px bị đứt quãng giữa chừng, tạo khoảng trống rời rạc trước khi chạm tới dấu chấm tiếp theo.
  - Giải pháp: Sử dụng `flex-1 min-h-[1.5rem] w-0.5 my-1 bg-emerald-200 dark:bg-emerald-900` kết hợp `self-stretch` ở cột mốc để đường timeline co giãn liên tục theo đúng chiều cao nội dung.
  - Card lộ trình: Đổi từ `p-6 rounded-3xl` thành `p-4 sm:p-6 rounded-2xl sm:rounded-3xl` để tăng không gian đọc văn bản thêm 32px trên màn hình 375px.

---

## 3. Caveats (Các Điểm Giới Hạn & Ràng Buộc Kỹ Thuật)

1. **Ràng buộc Quyền Hạn Agent (Read-Only Explorer)**:
   - Agent `m2_explorer_2` tuân thủ nguyên tắc điều tra đọc, không chỉnh sửa trực tiếp mã nguồn trong thư mục dự án `WEB_TinhNguyen/`.
   - Các giải pháp điều chỉnh mã nguồn được đóng gói thành đặc tả kỹ thuật chi tiết (mục 4) và file patch mẫu để agent thực thi (implementer) áp dụng chính xác.
2. **Ràng buộc Tuyệt Đối Về Khu Vực Quản Trị (Admin Boundary)**:
   - Toàn bộ các view, controller và style thuộc thư mục `WEB_TinhNguyen/GiaoDien/Admin*` và `WEB_TinhNguyen/BoDieuKhien/Admin*` giữ nguyên 100%, không bị tác động.
3. **Ràng buộc Tương Thích Với Bộ Test E2E Tier 1 - Tier 5**:
   - Tất cả các thẻ ID và class phục vụ kiểm thử (như `#main-header`, `#mobileNavToggle`, `#mobileDrawer`, `.countdown-card`, `.countdown-boxes`, `.d-val`, `.h-val`, `.m-val`, `.s-val`, `.btc-member-card`, `#btnShowMoreMembers`) được bảo toàn nguyên vẹn tên định danh.

---

## 4. Conclusion (Đặc Tả Đề Xuất Chỉnh Sửa Mã Nguồn Chi Tiết)

Dưới đây là danh mục thay đổi cụ thể cần thực hiện trên 4 file Razor View và 2 file liên quan:

### 4.1. File `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`

#### Thay đổi 1: Tối ưu khối Countdown Section & Card Padding
- **Vị trí**: Dòng 142, 150, 152
- **Hiện tại**:
  ```html
  <div class="relative w-full py-16 px-2 md:px-8 z-10 block bg-gray-50/50 dark:bg-night-2">
      <div class="swiper event-swiper max-w-5xl mx-auto">
          <div class="swiper-wrapper">
              ...
                  <div class="swiper-slide px-4 md:px-12 bg-transparent">
                      <div class="countdown-card keep-theme mx-auto text-center backdrop-blur shadow-2xl rounded-[2.5rem] p-8 md:p-14 border border-white/50 w-full"
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="relative w-full py-10 sm:py-16 px-2 sm:px-4 md:px-8 z-10 block bg-gray-50/50 dark:bg-night-2">
      <div class="swiper event-swiper max-w-5xl mx-auto">
          <div class="swiper-wrapper">
              ...
                  <div class="swiper-slide px-2 sm:px-6 md:px-12 bg-transparent">
                      <div class="countdown-card keep-theme mx-auto text-center backdrop-blur shadow-2xl rounded-3xl sm:rounded-[2.5rem] p-4 sm:p-8 md:p-14 border border-white/50 w-full"
  ```

#### Thay đổi 2: Tối ưu cỡ chữ tiêu đề Countdown & Hộp đếm ngược 1 hàng
- **Vị trí**: Dòng 167 & 187-204
- **Hiện tại**:
  ```html
  <h1 class="font-display text-5xl md:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color" style="color: #1b5e20;">
  ...
  <div class="countdown-boxes flex flex-wrap justify-center gap-3 md:gap-5">
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-2xl p-4 w-20 md:w-28 keep-color">
          <span class="d-val text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[10px] md:text-xs text-gray-500 font-bold mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Ngày</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-2xl p-4 w-20 md:w-28 keep-color">
          <span class="h-val text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[10px] md:text-xs text-gray-500 font-bold mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Giờ</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-2xl p-4 w-20 md:w-28 keep-color">
          <span class="m-val text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[10px] md:text-xs text-gray-500 font-bold mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Phút</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-2xl p-4 w-20 md:w-28 keep-color">
          <span class="s-val text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[10px] md:text-xs text-gray-500 font-bold mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Giây</span>
      </div>
  </div>
  ```
- **Đề xuất thay thế**:
  ```html
  <h1 class="font-display text-3xl sm:text-5xl md:text-7xl font-bold mb-3 sm:mb-4 drop-shadow-sm leading-tight keep-color" style="color: #1b5e20;">
  ...
  <div class="countdown-boxes flex flex-nowrap sm:flex-wrap justify-center gap-2 sm:gap-3 md:gap-5 max-w-full">
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-xl sm:rounded-2xl py-3 px-2 sm:p-4 w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none keep-color">
          <span class="d-val text-2xl sm:text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[9px] sm:text-[10px] md:text-xs text-gray-500 font-bold mt-1 sm:mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Ngày</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-xl sm:rounded-2xl py-3 px-2 sm:p-4 w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none keep-color">
          <span class="h-val text-2xl sm:text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[9px] sm:text-[10px] md:text-xs text-gray-500 font-bold mt-1 sm:mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Giờ</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-xl sm:rounded-2xl py-3 px-2 sm:p-4 w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none keep-color">
          <span class="m-val text-2xl sm:text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[9px] sm:text-[10px] md:text-xs text-gray-500 font-bold mt-1 sm:mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Phút</span>
      </div>
      <div class="flex flex-col items-center bg-white/80 shadow border border-gray-100 rounded-xl sm:rounded-2xl py-3 px-2 sm:p-4 w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none keep-color">
          <span class="s-val text-2xl sm:text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
          <span class="text-[9px] sm:text-[10px] md:text-xs text-gray-500 font-bold mt-1 sm:mt-2 uppercase tracking-wider keep-color" style="color: #6b7280;">Giây</span>
      </div>
  </div>
  ```

#### Thay đổi 3: Nút CTA đếm ngược co giãn toàn chiều ngang trên di động
- **Vị trí**: Dòng 209-215
- **Hiện tại**:
  ```html
  <div class="flex flex-col sm:flex-row justify-center gap-4">
      <a href="#register" class="px-8 py-3.5 rounded-full text-white font-bold shadow-md transition hover:-translate-y-1 hover:shadow-xl text-sm md:text-base keep-color" style="background-color: #1b5e20; color: #ffffff !important;">
          Đăng ký đồng hành
      </a>
      <a href="#about" class="px-8 py-3.5 rounded-full font-bold shadow-sm bg-white/50 border-2 transition hover:-translate-y-1 hover:bg-white text-sm md:text-base keep-color" style="color: #1b5e20; border-color: #1b5e20;">
          Tìm hiểu dự án
      </a>
  </div>
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="flex flex-col sm:flex-row justify-center gap-3 sm:gap-4 w-full max-w-md mx-auto">
      <a href="#register" class="w-full sm:w-auto text-center px-6 sm:px-8 py-3 sm:py-3.5 rounded-full text-white font-bold shadow-md transition hover:-translate-y-1 hover:shadow-xl text-sm md:text-base keep-color" style="background-color: #1b5e20; color: #ffffff !important;">
          Đăng ký đồng hành
      </a>
      <a href="#about" class="w-full sm:w-auto text-center px-6 sm:px-8 py-3 sm:py-3.5 rounded-full font-bold shadow-sm bg-white/50 border-2 transition hover:-translate-y-1 hover:bg-white text-sm md:text-base keep-color" style="color: #1b5e20; border-color: #1b5e20;">
          Tìm hiểu dự án
      </a>
  </div>
  ```

#### Thay đổi 4: Tinh chỉnh thẻ Sponsor Logos và Tooltip chống tràn
- **Vị trí**: Dòng 391-397
- **Hiện tại**:
  ```html
  <div class="group relative flex items-center justify-center w-36 h-36 sm:w-44 sm:h-44 rounded-3xl bg-white dark:bg-gray-800 p-3 sm:p-5 shadow-sm ring-1 ring-gray-200 dark:ring-gray-700 transition-all duration-300 hover:-translate-y-2 hover:shadow-xl hover:ring-2" style="--tw-ring-color: #2e7d32;">
      <img alt="@sp.TenDonVi" class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110" src="@sp.LinkLogo">
      <div class="absolute -bottom-10 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none whitespace-nowrap bg-gray-900 dark:bg-white text-white dark:text-gray-900 text-xs px-3 py-1.5 rounded-lg shadow-lg z-20 font-semibold">
          @sp.TenDonVi
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="group relative flex items-center justify-center w-32 h-32 sm:w-40 sm:h-40 md:w-44 md:h-44 rounded-2xl sm:rounded-3xl bg-white dark:bg-gray-800 p-3 sm:p-5 shadow-sm ring-1 ring-gray-200 dark:ring-gray-700 transition-all duration-300 hover:-translate-y-2 hover:shadow-xl hover:ring-2" style="--tw-ring-color: #2e7d32;">
      <img alt="@sp.TenDonVi" class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110" src="@sp.LinkLogo">
      <div class="absolute -bottom-10 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none max-w-[200px] truncate bg-gray-900 dark:bg-white text-white dark:text-gray-900 text-xs px-3 py-1.5 rounded-lg shadow-lg z-20 font-semibold">
          @sp.TenDonVi
  ```

---

### 4.2. File `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`

#### Thay đổi 1: Tối ưu Card Cấp 1 trên màn hình nhỏ
- **Vị trí**: Dòng 294, 315, 327
- **Hiện tại**:
  ```html
  <div class="relative bg-white rounded-2xl px-6 py-6 sm:px-10 sm:py-7 shadow-md max-w-2xl w-full flex flex-col sm:flex-row items-center gap-6 transition duration-300 hover:shadow-xl"
  ...
  <div class="inline-block px-6 py-1.5 rounded-full text-white font-bold text-sm sm:text-base uppercase tracking-wide shadow-sm"
  ...
  <h2 class="mt-2 text-lg sm:text-2xl font-extrabold uppercase tracking-wide"
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="relative bg-white rounded-2xl p-5 sm:px-10 sm:py-7 shadow-md max-w-2xl w-full flex flex-col sm:flex-row items-center gap-4 sm:gap-6 transition duration-300 hover:shadow-xl"
  ...
  <div class="inline-block px-4 sm:px-6 py-1 sm:py-1.5 rounded-full text-white font-bold text-xs sm:text-base uppercase tracking-wide shadow-sm max-w-full truncate"
  ...
  <h2 class="mt-2 text-base sm:text-2xl font-extrabold uppercase tracking-wide break-words"
  ```

#### Thay đổi 2: Tăng kích thước nút Facebook Cấp 2 đạt chuẩn Touch Target
- **Vị trí**: Dòng 414-416
- **Hiện tại**:
  ```html
  <a href="@item.LinkFacebook" target="_blank" title="Facebook của @item.HoTen"
     class="absolute bottom-0 right-0 h-6 w-6 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow hover:scale-110 transition">
      <svg class="h-3 w-3 fill-current" viewBox="0 0 24 24"><path d="..."></path></svg>
  </a>
  ```
- **Đề xuất thay thế**:
  ```html
  <a href="@item.LinkFacebook" target="_blank" title="Facebook của @item.HoTen"
     class="absolute bottom-0 right-0 h-8 w-8 sm:h-7 sm:w-7 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow hover:scale-110 transition focus:outline-none focus:ring-2 focus:ring-blue-400">
      <svg class="h-4 w-4 sm:h-3.5 sm:w-3.5 fill-current" viewBox="0 0 24 24"><path d="..."></path></svg>
  </a>
  ```

#### Thay đổi 3: Tối ưu khung bọc & thẻ thành viên Cấp 3
- **Vị trí**: Dòng 495, 504, 528
- **Hiện tại**:
  ```html
  <div class="mt-4 lg:mt-0 bg-white/70 backdrop-blur-md rounded-3xl p-6 sm:p-8 shadow-sm"
  ...
  <div class="btc-member-card @(isHidden ? "hidden" : "flex") flex-col justify-between bg-white rounded-2xl p-4 shadow-sm hover:shadow-md transition duration-300 hover:-translate-y-0.5"
  ...
  <a href="@mem.LinkFacebook" target="_blank" title="Facebook của @mem.HoTen"
     class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full bg-[#1877F2]/10 text-[#1877F2] hover:bg-[#1877F2] hover:text-white transition">
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="mt-4 lg:mt-0 bg-white/70 backdrop-blur-md rounded-2xl sm:rounded-3xl p-3 sm:p-6 md:p-8 shadow-sm"
  ...
  <div class="btc-member-card @(isHidden ? "hidden" : "flex") flex-col justify-between bg-white rounded-xl sm:rounded-2xl p-3 sm:p-4 shadow-sm hover:shadow-md transition duration-300 hover:-translate-y-0.5"
  ...
  <a href="@mem.LinkFacebook" target="_blank" title="Facebook của @mem.HoTen"
     class="flex h-8 w-8 sm:h-7 sm:w-7 flex-shrink-0 items-center justify-center rounded-full bg-[#1877F2]/10 text-[#1877F2] hover:bg-[#1877F2] hover:text-white transition focus:outline-none focus:ring-2 focus:ring-blue-400">
  ```

---

### 4.3. File `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`

#### Thay đổi 1: Tối ưu Hero & Khung Rich Text Tổng Kết
- **Vị trí**: Dòng 20, 40, 52, 62
- **Hiện tại**:
  ```html
  <section class="relative flex items-center overflow-hidden px-6 text-center min-h-[90svh] py-24 sm:py-32 ...">
  ...
  <h1 class="font-display pb-3 font-bold leading-[1.18] sm:pb-5 text-4xl sm:text-6xl md:text-7xl dynamic-font ...">
  ...
  <div class="mx-auto max-w-6xl px-6">
  ...
  <div class="relative rounded-3xl bg-emerald-50/50 dark:bg-volunteer-card p-6 sm:p-10 border border-emerald-200/70 ...">
  ```
- **Đề xuất thay thế**:
  ```html
  <section class="relative flex items-center overflow-hidden px-4 sm:px-6 text-center min-h-[85svh] py-20 sm:py-32 ...">
  ...
  <h1 class="font-display pb-3 font-bold leading-[1.18] sm:pb-5 text-3xl sm:text-5xl md:text-7xl dynamic-font ...">
  ...
  <div class="mx-auto max-w-6xl px-4 sm:px-6">
  ...
  <div class="relative rounded-2xl sm:rounded-3xl bg-emerald-50/50 dark:bg-volunteer-card p-4 sm:p-8 md:p-10 border border-emerald-200/70 ...">
  ```

#### Thay đổi 2: Tối ưu Thư viện ảnh cho ảnh dọc
- **Vị trí**: Dòng 79, 85
- **Hiện tại**:
  ```html
  <div class="grid grid-cols-2 gap-4 sm:grid-cols-3 sm:gap-5 lg:grid-cols-5" id="lightgallery">
  ...
  <div class="group relative overflow-hidden rounded-3xl bg-emerald-50 dark:bg-white/5 h-full w-full aspect-square border border-emerald-100 dark:border-white/10">
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-5 lg:grid-cols-5" id="lightgallery">
  ...
  <div class="group relative overflow-hidden rounded-2xl sm:rounded-3xl bg-emerald-50 dark:bg-white/5 w-full @(img.LaAnhDoc ? "h-full min-h-[220px]" : "aspect-square") border border-emerald-100 dark:border-white/10">
  ```

#### Thay đổi 3: Điều chỉnh thẻ Sponsor hiển thị 2 cột cân đối trên di động
- **Vị trí**: Dòng 123, 126
- **Hiện tại**:
  ```html
  <div class="flex flex-wrap justify-center gap-5 sm:gap-6">
      @foreach (var sponsor in group)
      {
          <button type="button" title="@sponsor.TenDonVi" aria-label="Xem giới thiệu @sponsor.TenDonVi" class="group relative flex h-28 w-44 items-center justify-center rounded-2xl bg-white dark:bg-volunteer-card p-5 ring-1 ring-emerald-100 dark:ring-white/10 transition hover:-translate-y-1.5 sm:h-32 sm:w-56 sm:p-6 shadow-sm hover:shadow-md cursor-pointer">
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="flex flex-wrap justify-center gap-3 sm:gap-5 md:gap-6">
      @foreach (var sponsor in group)
      {
          <button type="button" title="@sponsor.TenDonVi" aria-label="Xem giới thiệu @sponsor.TenDonVi" class="group relative flex h-24 w-[150px] sm:h-32 sm:w-56 items-center justify-center rounded-xl sm:rounded-2xl bg-white dark:bg-volunteer-card p-3 sm:p-6 ring-1 ring-emerald-100 dark:ring-white/10 transition hover:-translate-y-1.5 shadow-sm hover:shadow-md cursor-pointer">
  ```

---

### 4.4. File `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`

#### Thay đổi 1: Tối ưu Hero floating photos chống tràn viền
- **Vị trí**: Dòng 17, 26, 56
- **Hiện tại**:
  ```html
  <div class="relative z-10 mx-auto max-w-7xl px-6">
  ...
  <h1 class="text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font ...">
  ...
  <div class="absolute top-0 right-0 h-[70%] w-[65%] rounded-3xl overflow-hidden shadow-2xl z-20 hover:z-30 border-4 border-white dark:border-slate-800 transform translate-x-3 -translate-y-2 transition duration-500 hover:scale-105">
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="relative z-10 mx-auto max-w-7xl px-4 sm:px-6">
  ...
  <h1 class="text-3xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font ...">
  ...
  <div class="absolute top-0 right-0 h-[70%] w-[65%] rounded-2xl sm:rounded-3xl overflow-hidden shadow-2xl z-20 hover:z-30 border-2 sm:border-4 border-white dark:border-slate-800 transform translate-x-0 sm:translate-x-3 -translate-y-2 transition duration-500 hover:scale-105">
  ```

#### Thay đổi 2: Tối ưu Khung lộ trình & Đường kẻ Timeline dọc co giãn linh hoạt
- **Vị trí**: Dòng 106, 118, 123
- **Hiện tại**:
  ```html
  <div class="bg-white dark:bg-volunteer-card p-6 rounded-3xl border border-emerald-100 dark:border-white/10 shadow-lg backdrop-blur-sm">
  ...
  <div class="flex gap-4 items-start group">
      <div class="flex flex-col items-center">
          <div class="w-3.5 h-3.5 rounded-full mt-1 bg-emerald-600 shadow-xs ring-4 ring-emerald-100 dark:ring-emerald-950"></div>
          @if (!isLast)
          {
              <div class="w-0.5 h-8 my-1 bg-emerald-200 dark:bg-emerald-900"></div>
          }
      </div>
  ```
- **Đề xuất thay thế**:
  ```html
  <div class="bg-white dark:bg-volunteer-card p-4 sm:p-6 rounded-2xl sm:rounded-3xl border border-emerald-100 dark:border-white/10 shadow-lg backdrop-blur-sm">
  ...
  <div class="flex gap-3 sm:gap-4 items-start group">
      <div class="flex flex-col items-center self-stretch shrink-0">
          <div class="w-3.5 h-3.5 rounded-full mt-1 bg-emerald-600 shadow-xs ring-4 ring-emerald-100 dark:ring-emerald-950"></div>
          @if (!isLast)
          {
              <div class="w-0.5 flex-1 min-h-[1.5rem] my-1 bg-emerald-200 dark:bg-emerald-900"></div>
          }
      </div>
  ```

---

### 4.5. Khắc Phục Lỗi Hệ Thống Hỗ Trợ (Layout Header & Test Harness)

#### 1. Header trên `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (dòng 475, 482, 543)
- Bổ sung `max-w-[calc(100%-96px)] flex-1` cho thẻ `<a>` logo.
- Đổi `<span class="min-w-0 leading-tight">` thành `<span class="min-w-0 flex-1 block leading-tight overflow-hidden">`.
- Bổ sung `shrink-0` cho `<div class="flex items-center gap-2 md:hidden shrink-0">`.
- Điều này bảo vệ 100% các trang sử dụng `_Layout.cshtml` không bị tràn tiêu đề sang mép phải trên màn hình 375px.

#### 2. Khắc phục lỗi Double Replace trong `tests/run_mobile_viewport_browser_tests.js` (dòng 223-224)
- Thay thế 2 dòng:
  ```javascript
  layout = layout.replace('~/css/site.css', cssUrl);
  layout = layout.replace('/css/site.css', cssUrl);
  ```
  bằng regex an toàn duy nhất:
  ```javascript
  layout = layout.replace(/~?\/css\/site\.css/, cssUrl);
  ```
  giúp `site.css` được nạp đúng vào Headless Chrome, trả về kết quả kiểm thử trung thực tuyệt đối.

---

## 5. Verification Method (Phương Pháp Kiểm Tra & Lệnh Độc Lập)

Người thẩm định độc lập có thể kiểm tra toàn diện đề xuất trên thông qua các bước:

### 5.1. Biên Dịch Dự Án C# & Build CSS
```powershell
# 1. Biên dịch dự án (đảm bảo không có lỗi Razor hoặc cú pháp C#)
dotnet build

# 2. Biên dịch Tailwind CSS (đảm bảo các utility classes mới được sinh vào site.css)
npm run build:css
```

### 5.2. Chạy Toàn Bộ Bộ Kiểm Thử Tự Động Trình Duyệt Mobile Viewport
```powershell
# Chạy kiểm tra 4 kích thước viewport (375px, 390px, 412px, 430px) trên toàn bộ 11 trang công khai
node tests/run_mobile_viewport_browser_tests.js
```
**Tiêu chí đạt (Acceptance Check)**:
1. `Home / Index`: 375px, 390px, 412px, 430px đạt `[PASS] scrollWidth <= viewport` (zero overflow).
2. `Home / BanToChuc`: 375px, 390px, 412px, 430px đạt `[PASS]` (zero overflow).
3. `Home / KyUc`: 375px, 390px, 412px, 430px đạt `[PASS]` (zero overflow).
4. `Home / ChuongTrinh`: 375px, 390px, 412px, 430px đạt `[PASS]` (zero overflow).
5. Không có bất kỳ cảnh báo hình ảnh nào vượt quá kích thước màn hình (`Image overflow: 0`).

### 5.3. Kiểm Tra Tính Toàn Vẹn Của Khu Vực Quản Trị (/admin)
```powershell
# Xác nhận tuyệt đối không có chỉnh sửa nào trên thư mục Admin
git status WEB_TinhNguyen/GiaoDien/Admin*
git status WEB_TinhNguyen/BoDieuKhien/Admin*
git status WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*
git status WEB_TinhNguyen/Areas/CuaHang/Views/Admin*
```
Kết quả phải hoàn toàn sạch sẽ (`clean`), không có bất kỳ file nào bị thay đổi.
