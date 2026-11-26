# Báo Cáo Khảo Sát & Giải Pháp Chuẩn Hóa Typography & Tiêu Đề Các Trang Home (375px - 430px)

**Người thực hiện**: Teamwork Explorer (`m2_explorer_1`)  
**Vai trò**: Home Views Typography & Headings Explorer  
**Phạm vi**: Toàn bộ 8 view công khai của Home (`WEB_TinhNguyen/GiaoDien/Home/`)  
**Tập trung**: Điều tra các tiêu đề quá khổ, font-size cứng inline (TinTuc 64px, GayQuy 48px, Index text-5xl/text-7xl), và thiết kế giải pháp responsive typography (Tailwind classes + CSS `clamp()`) tương thích hoàn hảo viewports 375px-430px.  
**Ranh giới hệ thống**: Bảo toàn tuyệt đối khu vực quản trị (`/admin` - 0 files modified).

---

## 1. Observation (Dữ Liệu Khảo Sát Thực Tế)

### 1.1. Danh mục 9 Razor Views Thuộc Thư Mục `WEB_TinhNguyen/GiaoDien/Home/`

| STT | File Razor View | Route tương ứng | Hiện trạng Typography / Tiêu đề chính |
| :--- | :--- | :--- | :--- |
| 1 | `TinTuc.cshtml` | `/Home/TinTuc` | `h1` Hero: Cố định inline `style="font-size: @(Model?.Hero_CoChuTieuDe ?? "64px");"`<br>`h3` Card bài viết: `style="font-size: @(news.CoChuTieuDe ?? "18px");"` |
| 2 | `GayQuy.cshtml` | `/Home/GayQuy` | `h1` Hero: Cố định inline `style="font-size: @(Model?.Hero_CoChuTieuDe ?? "48px");"`<br>`h2` Section: `text-3xl font-black leading-tight sm:text-4xl md:text-5xl`<br>Cú pháp CK: `flex justify-between` thiếu `break-all` |
| 3 | `Index.cshtml` | `/` hoặc `/Home/Index` | `h1` Countdown: `font-display text-5xl md:text-7xl font-bold` (`text-5xl` = 48px trên mobile)<br>`h2` Về dự án: `text-4xl md:text-5xl lg:text-6xl` (36px base)<br>`h2` Các Section khác (Gallery, News, Sponsors, FAQ, Explore): đều dùng `text-3xl font-extrabold sm:text-4xl md:text-5xl` |
| 4 | `ChuongTrinh.cshtml` | `/Home/ChuongTrinh` | `h1` Hero: `text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font` (36px base)<br>`h2` Lộ trình: `text-3xl font-black sm:text-4xl lg:text-5xl` |
| 5 | `BanToChuc.cshtml` | `/Home/BanToChuc` | `h1` Tiêu đề Sơ đồ: `org-heading-font text-3xl sm:text-5xl font-black`<br>`h2` Cấp 1: `text-lg sm:text-2xl`<br>`h3` Cấp 2/3: `text-base` / `text-lg` |
| 6 | `KyUc.cshtml` | `/Home/KyUc` | `h1` Hero: `font-display text-4xl sm:text-6xl md:text-7xl dynamic-font` (36px base)<br>`h2` Tổng kết: `text-3xl font-black leading-tight sm:text-4xl md:text-5xl`<br>`h2` Khoảnh khắc / Nhà tài trợ: `whitespace-pre-line text-3xl sm:text-4xl` |
| 7 | `LoiChuc.cshtml` | `/Home/LoiChuc` | `h1` Hero: `font-display text-3xl sm:text-5xl text-gradient-green`<br>`h2` CTA: `font-display text-2xl font-bold sm:text-3xl`<br>Cards: Container queries `@container` và `@sm:text-xl` |
| 8 | `ComingSoon.cshtml` | `/Home/ComingSoon` | `h1` Tiêu đề bảo trì: `text-4xl md:text-5xl font-extrabold`<br>`h2` Tên dự án: `font-dancing text-3xl md:text-4xl` |
| 9 | `Privacy.cshtml` | `/Home/Privacy` | `h1` tiêu đề chính sách mặc định, nội dung cơ bản |

---

### 1.2. Trích Dẫn Chi Tiết Các Điểm Nóng (Hotspots) Quá Khổ

#### Hotspot 1: `TinTuc.cshtml` (Dòng 93-96) — Cỡ chữ Hero 64px Cứng
```html
<!-- TinTuc.cshtml:93-96 -->
<h1 class="tintuc-display pb-3 font-bold leading-[1.18] sm:pb-5 text-volunteer-text dark:text-white"
    style="font-size: @(Model?.Hero_CoChuTieuDe ?? "64px");">
    @(Model?.Hero_TieuDe ?? "Câu chuyện từ hành trình")
</h1>
```
- **Hiện tượng**: Style inline `font-size: 64px` đè hoàn toàn mọi class CSS của trình duyệt. Tiêu đề tiếng Việt viết bằng font uốn lượn `Dancing Script` với 25 ký tự.
- **Không gian hiển thị**: Trên màn hình iPhone SE (375px), trừ padding `px-6` (48px) hai bên, chiều rộng nội dung khả dụng chỉ còn **327px**.
- **Hậu quả**: Chữ 64px có độ rộng từ 30px đến 45px mỗi chữ cái. Từ "chuyện" chiếm ~195px, từ "hành trình" chiếm ~320px. Tiêu đề bị vỡ nát thành 4-5 dòng chữ khổng lồ ("Câu" / "chuyện từ" / "hành" / "trình"), chiều cao khối tiêu đề vượt quá 300px, chiếm trọn 50% chiều cao màn hình di động, hoặc làm tràn mép nếu admin nhập tiêu đề dài hơn.

#### Hotspot 2: `GayQuy.cshtml` (Dòng 68-71) — Cỡ chữ Hero 48px Cứng
```html
<!-- GayQuy.cshtml:68-71 -->
<h1 class="gayquy-heading font-extrabold tracking-tight leading-tight text-volunteer-text dark:text-white"
    style="font-size: @(Model?.Hero_CoChuTieuDe ?? "48px");">
    @(Model?.Hero_TieuDe ?? "HÌNH THỨC TRAO GỬI YÊU THƯƠNG")
</h1>
```
- **Hiện tượng**: Cỡ chữ cứng 48px cho tiêu đề in hoa toàn bộ 29 ký tự `HÌNH THỨC TRAO GỬI YÊU THƯƠNG`.
- **Hậu quả**: Ký tự in hoa in đậm của font `Be Vietnam Pro` có chiều rộng ~33.6px mỗi ký tự. Cụm từ "YÊU THƯƠNG" (10 ký tự) có độ rộng lý thuyết lên đến **336px**, lớn hơn bề rộng khả dụng **327px** của màn hình 375px. Điều này ép trình duyệt phải bẻ chữ ngay giữa từ hoặc gây trồi khung ra mép phải.

#### Hotspot 3: `Index.cshtml` (Dòng 167) — Tiêu đề Hero Countdown `text-5xl md:text-7xl`
```html
<!-- Index.cshtml:167 -->
<h1 class="font-display text-5xl md:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color" style="color: #1b5e20;">
    @Html.Raw(ev.TieuDeChinh.Replace("\n", "<br/>"))
</h1>
```
- **Hiện tượng**: Lớp cơ sở (mobile) là `text-5xl` tương đương `3rem = 48px`.
- **Không gian hiển thị**: Thẻ này nằm trong `.countdown-card` (dòng 152) có padding `p-8` (64px) kết hợp với padding của swiper-slide `px-4` (32px). Tổng khoảng đệm ngang là 96px. Bề rộng lòng thẻ trên viewport 375px chỉ còn **279px**.
- **Hậu quả**: Cỡ chữ 48px trong không gian 279px chỉ chứa được tối đa 5-6 ký tự trên một dòng. Khi tiêu đề có dấu ngắt dòng `<br/>` sẵn, giao diện bị vỡ vụn thành các đoạn cụt lủn mất mỹ quan.

#### Hotspot 4: `Index.cshtml` (Dòng 244) — Tiêu đề "Về dự án" `text-4xl md:text-5xl lg:text-6xl`
```html
<!-- Index.cshtml:244 -->
<h2 class="text-4xl md:text-5xl lg:text-6xl font-black leading-tight font-display mb-8" style="color: #1b5e20;">
    @(Model?.VeDuAn_TieuDe ?? "Mang ánh trăng ấm áp đến với núi rừng cao nguyên")
</h2>
```
- **Hiện tượng**: Lớp cơ sở là `text-4xl` (36px).
- **Hậu quả**: Tiêu đề dài 53 ký tự tiếng Việt. Ở kích cỡ 36px trên di động, nó bị ép thành 6-7 dòng chữ to dày, đẩy toàn bộ nội dung TinyMCE và ảnh xuống dưới nếp gấp màn hình (fold).

#### Hotspot 5: `Index.cshtml` (Dòng 280, 329, 374, 418, 460) — Toàn Bộ `h2` Các Section Đang Dùng `text-3xl` Cơ Sở
- Dòng 280 (Tivi khoảnh khắc): `<h2 class="text-3xl font-extrabold sm:text-4xl md:text-5xl font-display drop-shadow-sm"...>`
- Dòng 329 (Bản tin): `<h2 class="text-3xl font-extrabold sm:text-4xl md:text-5xl font-display"...>`
- Dòng 374 (Đồng hành): `<h2 class="text-3xl font-extrabold sm:text-4xl md:text-5xl font-display"...>`
- Dòng 418 (FAQ): `<h2 class="text-3xl font-extrabold sm:text-4xl md:text-5xl font-display mt-4"...>`
- Dòng 460 (Khám phá): `<h2 class="text-3xl font-extrabold sm:text-4xl"...>`
- **Hiện tượng**: Tất cả các đề mục section đều gán `text-3xl` (30px) cho mobile. Đối với các tiêu đề dài như "Đơn vị tổ chức & Tài trợ" hay "Tìm hiểu sâu hơn về dự án", 30px chiếm không gian quá lớn trên mobile, làm mất cân đối tỷ lệ phân cấp thị giác (visual hierarchy) giữa H1 và H2.

#### Hotspot 6: `ChuongTrinh.cshtml` (Dòng 26, 75) — Hero và Section Tiêu Đề
```html
<!-- ChuongTrinh.cshtml:26 -->
<h1 class="text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font text-volunteer-text dark:text-white leading-tight">
    @conf.TieuDeChinh <br class="hidden md:block">
    <span class="bg-gradient-to-r from-emerald-600 via-green-600 to-teal-700 bg-clip-text text-transparent dark:from-emerald-400 dark:to-green-300">
        @conf.TieuDeNhanManh
    </span>
</h1>

<!-- ChuongTrinh.cshtml:75 -->
<h2 class="text-3xl font-black sm:text-4xl lg:text-5xl text-volunteer-text dark:text-white mt-2">
    LỘ TRÌNH TRIỂN KHAI CHIẾN DỊCH
</h2>
```
- **Hậu quả**: H1 với `text-4xl` (36px) ghép 2 đoạn văn bản khiến tiêu đề dài bị gập dòng lộn xộn. H2 với 29 ký tự in hoa `LỘ TRÌNH TRIỂN KHAI CHIẾN DỊCH` ở cỡ `text-3xl` (30px) chiếm hơn 500px chiều rộng lý thuyết, bị ép gãy thành 3 dòng cụt.

#### Hotspot 7: `KyUc.cshtml` (Dòng 40, 56, 76, 104)
```html
<!-- KyUc.cshtml:40 -->
<h1 class="font-display pb-3 font-bold leading-[1.18] sm:pb-5 text-4xl sm:text-6xl md:text-7xl dynamic-font text-volunteer-text dark:text-white drop-shadow-sm">
    @conf.TieuDeChinh
</h1>
<!-- KyUc.cshtml:56 -->
<h2 class="text-3xl font-black leading-tight sm:text-4xl md:text-5xl dynamic-font-tongket text-volunteer-text dark:text-white mt-2">
    @conf.TieuDeTongKet
</h2>
```
- **Hậu quả**: `text-4xl` (36px) ở H1 và `text-3xl` (30px) ở H2 làm tiêu đề các mùa chiến dịch bị quá khổ trên màn hình 375px.

#### Hotspot 8: `BanToChuc.cshtml` (Dòng 264)
```html
<h1 class="org-heading-font text-3xl sm:text-5xl font-black tracking-tight mb-5 text-volunteer-text dark:text-white">
```
- `text-3xl` (30px) cho tiêu đề cơ cấu tổ chức trên màn hình nhỏ nên hạ về `text-2xl` để thanh thoát và dành không gian cho sơ đồ cây phía dưới.

#### Hotspot 9: `ComingSoon.cshtml` (Dòng 74, 78)
```html
<h1 class="text-4xl md:text-5xl font-extrabold text-gray-900 tracking-tight mb-4">
<h2 class="font-dancing text-3xl md:text-4xl text-[#4caf50] mb-6">
```
- Cỡ chữ `text-4xl` và `text-3xl` khiến trang bảo trì bị chật chội trên điện thoại mini 375px.

#### Hotspot 10: Rủi Ro Tiêu Đề Bài Viết Cấu Hình Động Có Cỡ Chữ Tùy Biến
- `TinTuc.cshtml` (dòng 167): `style="color: #1b5e20; font-size: @(news.CoChuTieuDe ?? "18px");"`
- `Index.cshtml` (dòng 343): `style="color: #2e7d32; font-size: @(n.KichCoTieuDe ?? "18px"); line-height: 1.3;"`
- **Hậu quả**: Nếu người quản trị trong Admin vô tình cấu hình cỡ chữ bài viết là 24px hay 28px, tiêu đề card trên mobile sẽ phình to quá mức, đẩy xô lệch toàn bộ layout thẻ card.

---

## 2. Logic Chain (Chuỗi Suy Luận & Thiết Kế Giải Pháp)

### 2.1. Phân Tích Cơ Sở Toán Học Cho Typography Màn Hình 375px - 430px

1. **Chiều rộng màn hình và container hữu dụng**:
   - iPhone SE / mini: `375px` $\rightarrow$ Padding hai lề thông dụng `px-4` (32px) hoặc `px-6` (48px) $\rightarrow$ Lòng chứa còn **327px đến 343px**.
   - iPhone 14/15/16: `390px` $\rightarrow$ Lòng chứa còn **342px đến 358px**.
   - iPhone Pro Max: `430px` $\rightarrow$ Lòng chứa còn **382px đến 398px**.

2. **Kích thước font và mật độ hiển thị tiếng Việt**:
   - Chữ in hoa (Uppercase bold): Bề rộng trung bình mỗi ký tự là $0.7em - 0.8em$.
     - Ở cỡ chữ **48px**: 1 ký tự rộng $\approx 35px \rightarrow 10$ ký tự đã chiếm $350px > 327px$ (GÂY TRÀN NGAY LẬP TỨC).
     - Ở cỡ chữ **24px** (`text-2xl`): 1 ký tự rộng $\approx 17.5px \rightarrow 18$ ký tự vừa vặn 1 dòng $315px \le 327px$.
   - Chữ viết tay/nghệ thuật (`Dancing Script` / `font-display`):
     - Ở cỡ chữ **64px**: Ký tự hoa/chữ cái có dấu phụ (ễ, ẫ, ợ) chiếm diện tích đứng và ngang rất lớn. Chiều cao dòng `leading-[1.18]` ở 64px là $75.5px$/dòng $\rightarrow 4$ dòng là $302px$.
     - Ở cỡ chữ **30px - 32px** (`clamp(1.875rem, 6vw, ...)`): Chiều cao dòng chỉ còn $\approx 38px$/dòng $\rightarrow 2$ dòng hoàn hảo là $76px$. Tiết kiệm hơn $220px$ chiều cao dọc, nội dung phía dưới được đẩy lên ngay tầm mắt người dùng.

### 2.2. Chiến Lược Hai Tầng: CSS `clamp()` Linh Hoạt + Tailwind Responsive Scale

Để giải quyết triệt để vấn đề mà vẫn **bảo toàn 100% khả năng cấu hình của Admin trong Cơ sở Dữ liệu**:

1. **Đối với các thuộc tính font-size động gắn với Model (`Hero_CoChuTieuDe`, `CoChuTieuDe`)**:
   - **Không được** xóa bỏ giá trị từ DB vì Admin có quyền chỉnh sửa trên desktop.
   - **Giải pháp**: Dùng hàm chuẩn CSS `clamp(min, preferred_vw, max)`.
     - `min`: Giá trị an toàn tuyệt đối cho màn hình 375px (ví dụ `1.875rem` = 30px cho TinTuc, `1.625rem` = 26px cho GayQuy).
     - `preferred_vw`: Tỷ lệ phần trăm co giãn theo chiều rộng khung nhìn (ví dụ `6vw` hoặc `5.5vw`).
     - `max`: Giá trị do Admin cấu hình từ DB (`Model?.Hero_CoChuTieuDe ?? "64px"`).
   - **Kết quả**:
     - Trên mobile 375px: `6vw = 22.5px < 1.875rem (30px)` $\rightarrow$ Trình duyệt áp dụng đúng **30px**.
     - Trên tablet 768px: `6vw = 46px` $\rightarrow$ Trình duyệt co giãn mượt mà tại **46px**.
     - Trên desktop 1200px: `6vw = 72px > 64px` $\rightarrow$ Trình duyệt ghim chuẩn xác tại **64px** (giữ nguyên thiết kế desktop ban đầu).

2. **Đối với các thẻ tiêu đề dùng lớp Tailwind thuần**:
   - Áp dụng thang đo chuẩn hóa thống nhất theo phân cấp:
     - **H1 (Hero chính)**: `text-2xl sm:text-4xl md:text-5xl lg:text-7xl` (Mobile 24px $\rightarrow$ Desktop 72px).
     - **H2 (Section chính)**: `text-2xl sm:text-3xl md:text-4xl lg:text-5xl` (Mobile 24px $\rightarrow$ Desktop 48px).
     - **H3 (Sub-heading/Card)**: `text-lg sm:text-xl md:text-2xl` (Mobile 18px $\rightarrow$ Desktop 24px).
     - **H4/H5**: `text-sm sm:text-base`.

---

### 2.3. Quy Cách Thay Thế Mã Nguồn Cụ Thể (Before $\rightarrow$ After)

#### View 1: `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml`

*Điểm 1.1 — Hero H1 Heading (Dòng 93-96):*
- **Trước (Before)**:
```html
<h1 class="tintuc-display pb-3 font-bold leading-[1.18] sm:pb-5 text-volunteer-text dark:text-white"
    style="font-size: @(Model?.Hero_CoChuTieuDe ?? "64px");">
    @(Model?.Hero_TieuDe ?? "Câu chuyện từ hành trình")
</h1>
```
- **Sau (After)**:
```html
<h1 class="tintuc-display pb-3 font-bold leading-[1.2] sm:pb-5 text-volunteer-text dark:text-white"
    style="font-size: clamp(1.875rem, 6vw, @(Model?.Hero_CoChuTieuDe ?? "64px"));">
    @(Model?.Hero_TieuDe ?? "Câu chuyện từ hành trình")
</h1>
```
*Tác dụng*: Trên 375px, font-size giảm từ 64px xuống 30px (1.875rem), tiêu đề dài hiển thị cân xứng trong 2 dòng ngắn, không chiếm hết chiều cao màn hình. Trên desktop >= 1067px, giữ nguyên đúng giá trị cấu hình `Hero_CoChuTieuDe` (mặc định 64px).

*Điểm 1.2 — Tiêu đề Card bài viết (Dòng 166-169):*
- **Trước (Before)**:
```html
<h3 class="mt-1 font-bold leading-snug"
    style="color: #1b5e20; font-size: @(news.CoChuTieuDe ?? "18px");">
    @news.TieuDe
</h3>
```
- **Sau (After)**:
```html
<h3 class="mt-1 font-bold leading-snug line-clamp-2 break-words"
    style="color: #1b5e20; font-size: clamp(1rem, 2.5vw, @(news.CoChuTieuDe ?? "18px"));">
    @news.TieuDe
</h3>
```

---

#### View 2: `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`

*Điểm 2.1 — Hero H1 Heading (Dòng 68-71):*
- **Trước (Before)**:
```html
<h1 class="gayquy-heading font-extrabold tracking-tight leading-tight text-volunteer-text dark:text-white"
    style="font-size: @(Model?.Hero_CoChuTieuDe ?? "48px");">
    @(Model?.Hero_TieuDe ?? "HÌNH THỨC TRAO GỬI YÊU THƯƠNG")
</h1>
```
- **Sau (After)**:
```html
<h1 class="gayquy-heading font-extrabold tracking-tight leading-tight text-volunteer-text dark:text-white"
    style="font-size: clamp(1.625rem, 5.5vw, @(Model?.Hero_CoChuTieuDe ?? "48px"));">
    @(Model?.Hero_TieuDe ?? "HÌNH THỨC TRAO GỬI YÊU THƯƠNG")
</h1>
```
*Tác dụng*: Trên 375px, font-size giảm từ 48px xuống 26px (1.625rem). Cụm từ in hoa "YÊU THƯƠNG" thu nhỏ từ 336px xuống ~180px, hoàn toàn nằm trọn trong lề 327px mà không tràn màn hình.

*Điểm 2.2 — Các đề mục Section H2 (Dòng 107, 278, 303):*
- **Trước (Before)** (Dòng 107):
```html
<h2 class="gayquy-heading text-3xl font-black leading-tight sm:text-4xl md:text-5xl text-volunteer-text dark:text-white">
```
- **Sau (After)**:
```html
<h2 class="gayquy-heading text-2xl font-black leading-tight sm:text-3xl md:text-4xl lg:text-5xl text-volunteer-text dark:text-white">
```
(Áp dụng đồng bộ cho Dòng 278 "Báo cáo thu - chi" và Dòng 303 "ĐƠN VỊ TỔ CHỨC - TÀI TRỢ").

*Điểm 2.3 — Cú pháp Chuyển khoản chống tràn ngang (Dòng 188-190 và 246-249):*
- **Trước (Before)**:
```html
<div class="flex justify-between">
    <span class="text-slate-400">Cú pháp:</span>
    <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all">@effectiveMemo</span>
</div>
```
- **Sau (After)**:
```html
<div class="flex justify-between items-start gap-2">
    <span class="text-slate-400 shrink-0">Cú pháp:</span>
    <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all break-all text-right">@effectiveMemo</span>
</div>
```

---

#### View 3: `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`

*Điểm 3.1 — Tiêu đề Hero Countdown Slider (Dòng 167):*
- **Trước (Before)**:
```html
<h1 class="font-display text-5xl md:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color" style="color: #1b5e20;">
```
- **Sau (After)**:
```html
<h1 class="font-display text-2xl sm:text-4xl md:text-6xl lg:text-7xl font-bold mb-4 drop-shadow-sm leading-tight keep-color" style="color: #1b5e20;">
```
*Tác dụng*: Trên 375px, tiêu đề chuyển từ 48px xuống 24px (`text-2xl`), hoàn toàn nằm gọn trong lòng card 279px, các chữ có dấu gắt dòng `<br/>` hiển thị đẹp mắt, không bị xé vụn.

*Điểm 3.2 — Đệm thẻ Countdown Card (Dòng 152):*
- **Trước (Before)**:
```html
<div class="countdown-card keep-theme mx-auto text-center backdrop-blur shadow-2xl rounded-[2.5rem] p-8 md:p-14 border border-white/50 w-full"
```
- **Sau (After)**:
```html
<div class="countdown-card keep-theme mx-auto text-center backdrop-blur shadow-2xl rounded-[2.5rem] p-5 sm:p-8 md:p-14 border border-white/50 w-full"
```
*Tác dụng*: Giảm padding từ 32px xuống 20px trên mobile, tăng bề rộng khả dụng từ 279px lên 303px.

*Điểm 3.3 — Các số đếm Countdown (Dòng 189, 193, 197, 201):*
- **Trước (Before)**:
```html
<span class="d-val text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
```
- **Sau (After)**:
```html
<span class="d-val text-2xl sm:text-3xl md:text-5xl font-black keep-color" style="color: #1b5e20;">00</span>
```

*Điểm 3.4 — Tiêu đề "Về dự án" (Dòng 244):*
- **Trước (Before)**:
```html
<h2 class="text-4xl md:text-5xl lg:text-6xl font-black leading-tight font-display mb-8" style="color: #1b5e20;">
```
- **Sau (After)**:
```html
<h2 class="text-2xl sm:text-3xl md:text-4xl lg:text-6xl font-black leading-tight font-display mb-6 sm:mb-8" style="color: #1b5e20;">
```

*Điểm 3.5 — Toàn bộ H2 các Section còn lại (Dòng 280, 329, 374, 418, 460):*
- **Trước (Before)**:
```html
<h2 class="text-3xl font-extrabold sm:text-4xl md:text-5xl font-display...
```
- **Sau (After)**:
```html
<h2 class="text-2xl sm:text-3xl md:text-4xl lg:text-5xl font-display...
```

*Điểm 3.6 — Tiêu đề Card Tin tức trong Index (Dòng 343):*
- **Trước (Before)**:
```html
<h3 class="font-bold font-display" style="color: #2e7d32; font-size: @(n.KichCoTieuDe ?? "18px"); line-height: 1.3;">
```
- **Sau (After)**:
```html
<h3 class="font-bold font-display line-clamp-2 break-words" style="color: #2e7d32; font-size: clamp(1rem, 2.5vw, @(n.KichCoTieuDe ?? "18px")); line-height: 1.3;">
```

---

#### View 4: `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`

*Điểm 4.1 — Hero H1 Heading (Dòng 26):*
- **Trước (Before)**:
```html
<h1 class="text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl dynamic-font text-volunteer-text dark:text-white leading-tight">
```
- **Sau (After)**:
```html
<h1 class="text-2xl sm:text-4xl lg:text-6xl font-extrabold tracking-tight dynamic-font text-volunteer-text dark:text-white leading-tight">
```

*Điểm 4.2 — Tiêu đề Lộ trình H2 (Dòng 75):*
- **Trước (Before)**:
```html
<h2 class="text-3xl font-black sm:text-4xl lg:text-5xl text-volunteer-text dark:text-white mt-2">
    LỘ TRÌNH TRIỂN KHAI CHIẾN DỊCH
</h2>
```
- **Sau (After)**:
```html
<h2 class="text-2xl sm:text-3xl lg:text-5xl font-black text-volunteer-text dark:text-white mt-2">
    LỘ TRÌNH TRIỂN KHAI CHIẾN DỊCH
</h2>
```

---

#### View 5: `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`

*Điểm 5.1 — Hero H1 Heading (Dòng 40):*
- **Trước (Before)**:
```html
<h1 class="font-display pb-3 font-bold leading-[1.18] sm:pb-5 text-4xl sm:text-6xl md:text-7xl dynamic-font text-volunteer-text dark:text-white drop-shadow-sm">
```
- **Sau (After)**:
```html
<h1 class="font-display pb-3 font-bold leading-[1.2] sm:pb-5 text-2xl sm:text-4xl md:text-6xl lg:text-7xl dynamic-font text-volunteer-text dark:text-white drop-shadow-sm">
```

*Điểm 5.2 — Đề mục Tổng kết & Khoảnh khắc H2 (Dòng 56, 76, 104):*
- **Trước (Before)** (Dòng 56):
```html
<h2 class="text-3xl font-black leading-tight sm:text-4xl md:text-5xl dynamic-font-tongket text-volunteer-text dark:text-white mt-2">
```
- **Sau (After)**:
```html
<h2 class="text-2xl sm:text-3xl md:text-4xl lg:text-5xl font-black leading-tight dynamic-font-tongket text-volunteer-text dark:text-white mt-2">
```
(Dòng 76 & 104: Thay `text-3xl` bằng `text-2xl sm:text-3xl md:text-4xl`).

---

#### View 6: `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`

*Điểm 6.1 — Tiêu đề chính Sơ đồ H1 (Dòng 264):*
- **Trước (Before)**:
```html
<h1 class="org-heading-font text-3xl sm:text-5xl font-black tracking-tight mb-5 text-volunteer-text dark:text-white">
```
- **Sau (After)**:
```html
<h1 class="org-heading-font text-2xl sm:text-4xl md:text-5xl font-black tracking-tight mb-5 text-volunteer-text dark:text-white">
```

---

#### View 7: `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`

*Điểm 7.1 — Hero H1 Heading (Dòng 35):*
- **Trước (Before)**:
```html
<h1 class="font-display pb-3 font-bold leading-[1.18] text-gradient-green sm:pb-5 text-3xl sm:text-5xl">Gửi yêu thương từ trái tim</h1>
```
- **Sau (After)**:
```html
<h1 class="font-display pb-3 font-bold leading-[1.2] text-gradient-green sm:pb-5 text-2xl sm:text-4xl md:text-5xl">Gửi yêu thương từ trái tim</h1>
```

---

#### View 8: `WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml`

*Điểm 8.1 — Tiêu đề Bảo trì H1 & H2 (Dòng 74, 78):*
- **Trước (Before)**:
```html
<h1 class="text-4xl md:text-5xl font-extrabold text-gray-900 tracking-tight mb-4">
<h2 class="font-dancing text-3xl md:text-4xl text-[#4caf50] mb-6">
```
- **Sau (After)**:
```html
<h1 class="text-2xl sm:text-3xl md:text-5xl font-extrabold text-gray-900 tracking-tight mb-4">
<h2 class="font-dancing text-2xl md:text-4xl text-[#4caf50] mb-6">
```

---

## 3. Caveats (Các Điểm Lưu Ý & Ranh Giới Khảo Sát)

1. **Bảo toàn khả năng cấu hình của Quản trị viên (Admin Flexibility)**:
   - Việc dùng CSS `clamp(min, preferred_vw, max)` đảm bảo khi người quản trị vào trang quản trị (`/AdminTinTuc`, `/AdminGayQuy`) đổi cỡ chữ tiêu đề thành 72px hay 56px, giá trị này **vẫn hiển thị trọn vẹn trên màn hình Desktop** (vì nó đóng vai trò chặn trên `max`). Nó chỉ bảo vệ người dùng trên điện thoại không bị tràn viền (chặn dưới `min`).
2. **Cô lập tuyệt đối khu vực Admin**:
   - Tất cả 8 file view được phân tích đều nằm trong `WEB_TinhNguyen/GiaoDien/Home/`.
   - Tuyệt đối không chỉnh sửa bất kỳ controller nào trong `WEB_TinhNguyen/BoDieuKhien/Admin*` hay view nào trong `WEB_TinhNguyen/GiaoDien/Admin*` và `Areas/CuaHang/Views/Admin*`.
3. **Phân định phạm vi giữa các subagent**:
   - Báo cáo này chuyên trách về **Typography & Headings**.
   - Các vấn đề về lưới thẻ (Grid), hình ảnh nhà tài trợ cố định (như nút tài trợ `w-44` trong GayQuy khiến test runner báo tràn ảnh/button) và giỏ hàng thuộc về các subagent phụ trách Layout/Grid/Storefront.

---

## 4. Conclusion (Kết Luận)

1. **Nguyên nhân cốt lõi**: Hiện tượng vỡ dòng, tràn chữ và chiếm nửa màn hình di động trên các trang Home xuất phát từ:
   - Các thuộc tính inline `font-size: 64px` (TinTuc) và `font-size: 48px` (GayQuy).
   - Lớp cơ sở Tailwind quá lớn (`text-5xl` và `text-4xl`) không có tiền tố responsive cho mobile (`text-2xl` trên màn hình `< 640px`).
2. **Tính khả thi của giải pháp**:
   - Giải pháp phối hợp CSS `clamp()` và thang responsive Tailwind (`text-2xl sm:text-4xl md:text-5xl lg:text-7xl`) giải quyết triệt để 100% các điểm nghẽn tiêu đề trên viewports từ 375px đến 430px.
   - Giải pháp không làm gãy layout desktop, không làm vô hiệu hóa cấu hình Admin và hoàn toàn tuân thủ Tailwind CSS có sẵn trong dự án.
3. **Sẵn sàng triển khai**:
   - Đặc tả chi tiết từng file, dòng mã, nội dung Trước và Sau đã sẵn sàng để chuyển giao cho agent thực thi (Builder/Implementer) áp dụng trực tiếp.

---

## 5. Verification Method (Phương Pháp Kiểm Tra & Xác Minh Độc Lập)

Để kiểm chứng tính chính xác của phân tích và hiệu quả của các giải pháp đề xuất, thực hiện theo các bước:

### 5.1. Kiểm Tra Biên Dịch Hệ Thống
```powershell
dotnet build
```
- **Kỳ vọng**: 0 Error. Trình biên dịch C# và Tailwind CLI biên dịch thành công `site.css` mà không phát sinh lỗi cú pháp Razor.

### 5.2. Kiểm Tra Kiểm Thử Đơn Vị & Ranh Giới Admin
```powershell
dotnet test tests/E2ETests/E2ETests.csproj
```
- **Kỳ vọng**: Toàn bộ các bài test về tính bất biến của Admin (`Tier4_AdminImmutabilityTests.cs`) và an toàn chuyển khoản (`M4_F06_T01_DonationMemoBreakAll`) vượt qua 100%.

### 5.3. Kiểm Tra Kiểm Thử Tự Động Trình Duyệt Di Động
```powershell
node tests/run_mobile_viewport_browser_tests.js
```
- **Kỳ vọng**: Trên các trang Home (Index, ChuongTrinh, GayQuy, BanToChuc, TinTuc, KyUc, LoiChuc), các tiêu đề co giãn vừa vặn, không còn đối tượng `h1`, `h2` nào xuất hiện trong danh sách `Overflow culprits` trên các viewport chuẩn:
  - 375px (iPhone SE)
  - 390px (iPhone 14/15/16)
  - 412px (Samsung Galaxy S24)
  - 430px (iPhone 16 Pro Max)

### 5.4. Kiểm Tra Trực Quan Thủ Công (Manual Visual Verification)
1. Mở Chrome DevTools $\rightarrow$ Bật Device Mode (`Ctrl + Shift + M`).
2. Chọn thiết bị **iPhone SE (375 x 667)**.
3. Truy cập `/Home/TinTuc`: Tiêu đề "Câu chuyện từ hành trình" hiển thị gọn gàng trong 2 dòng, chiều cao khối tiêu đề không vượt quá 100px.
4. Truy cập `/Home/GayQuy`: Tiêu đề "HÌNH THỨC TRAO GỬI YÊU THƯƠNG" hiển thị 2-3 dòng, cụm từ "YÊU THƯƠNG" cách lề phải an toàn > 20px.
5. Truy cập `/Home/Index`: Tiêu đề đếm ngược trong thẻ Countdown hiển thị thanh thoát, không chạm vào viền hộp.
