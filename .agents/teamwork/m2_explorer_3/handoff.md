# Báo Cáo Khảo Sát & Giải Pháp: Kích Thước Vùng Chạm (Touch Targets) & Ngắt Dòng Cú Pháp Ngân Hàng (Bank Memo break-all)

**Người thực hiện**: Teamwork Explorer (`m2_explorer_3`)  
**Nhiệm vụ**: Touch Targets & Bank Memo Line Break Explorer  
**Phạm vi**: 
- `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml` (Bank memo break-all, card width/padding, sponsor buttons, modal close touch targets)
- `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml` (Facebook icon buttons Cấp 1, Cấp 2, Cấp 3, CTA and member expansion buttons)
- `WEB_TinhNguyen/GiaoDien/Home/` (`Index.cshtml`, `TinTuc.cshtml`, `ChuongTrinh.cshtml`, `KyUc.cshtml`, `LoiChuc.cshtml`, `ComingSoon.cshtml`)
- Giải quyết bài kiểm thử tự động đang Pending: `M4_F06_T01` trong `tests/E2ETests/Tier1/Tier1_MobileViewportTests.cs`
- Ranh giới bảo toàn: Tuyệt đối 0 sửa đổi đối với khu vực quản trị (`/admin`)

---

## 1. Observation (Dữ Liệu Khảo Sát Thực Tế & Bằng Chứng)

### 1.1. Hiện Trạng Test E2E `M4_F06_T01` (Feature 6: Bank Memo Break-All)
- **Tập tin**: `tests/E2ETests/Tier1/Tier1_MobileViewportTests.cs` (Dòng 204-220)
- **Mã nguồn kiểm thử nguyên văn**:
  ```csharp
  // tests/E2ETests/Tier1/Tier1_MobileViewportTests.cs:204-220
  [E2ETest(tier: 1, feature: 6, name: "M4_F06_T01: Donation bank transfer memos enforce break-all or break-words")]
  public void M4_F06_T01_DonationMemoBreakAll()
  {
      var gayQuyPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
      Assert.RequireFile(gayQuyPath, "GayQuy.cshtml");

      var content = File.ReadAllText(gayQuyPath);
      if (!content.Contains("break-all") && !content.Contains("break-words"))
      {
          throw new AssertException("[SKIP] Pending implementation in M2: break-all or break-words on bank memo in GayQuy.cshtml");
      }

      Assert.True(
          content.Contains("break-all") || content.Contains("break-words"),
          "Donation transfer memos must declare break-all or break-words to avoid mobile horizontal scroll"
      );
  }
  ```
- **Kết quả chạy thực tế (`dotnet run --project tests/E2ETests/E2ETests.csproj`)**:
  ```text
  [SKIP] Tier 1 | F06 | M4_F06_T01: Donation bank transfer memos enforce break-all or break-words -> [SKIP] Pending implementation in M2: break-all or break-words on bank memo in GayQuy.cshtml
  OVERALL: 552 Tests | 551 Passed | 0 Failed | 1 Skipped in 13.45s
  ```
  Test này là bài test duy nhất đang ở trạng thái `[SKIP]` trong toàn bộ 552 tests của hệ thống.

---

### 1.2. Hiện Trạng `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`

#### Điểm nóng 1: Cú pháp chuyển khoản ngân hàng thiếu `break-all`
- **Vị trí 1 (Vòng lặp `Model.KenhDongGops`, Dòng 187-191)**:
  ```html
  <div class="flex justify-between">
      <span class="text-slate-400">Cú pháp:</span>
      <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all">@effectiveMemo</span>
  </div>
  ```
- **Vị trí 2 (Vòng lặp dự phòng `donationQrConfigs`, Dòng 246-249)**:
  ```html
  <div class="flex justify-between">
      <span class="text-slate-400">Cú pháp:</span>
      <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all">@effectiveMemo</span>
  </div>
  ```
- **Vị trí 3 (Trong Modal `#qrModal`, Dòng 357)**:
  ```html
  <p><span class="text-slate-500 dark:text-slate-400">Nội dung CK:</span> <strong id="qrModalNote" class="text-amber-700 dark:text-amber-400 select-all"></strong></p>
  ```
- **Quan sát số liệu**:
  - Giá trị mặc định của `effectiveMemo` là `"[Họ và tên]_DongGopLangbiang"`. Khi người dùng hoặc quản trị viên nhập cú pháp dài hơn (ví dụ: `[NguyenVanA]_DongGop_ChienDich_TrangSangLangbiang2026`), chuỗi không có khoảng trắng thông thường (chỉ có dấu gạch dưới `_`).
  - Trong bố cục `flex justify-between`, phần tử `<span>` bên phải có `min-width: auto`. Nếu không có `break-all` và không có `shrink-0` ở label bên trái, chuỗi text đơn khối này sẽ không xuống dòng, đẩy khung thẻ phình to ngang vượt quá kích thước màn hình 375px.

#### Điểm nóng 2: Chiều rộng và đệm của Thẻ Kênh Quyên Góp (Donation Cards)
- **Vị trí (Dòng 153 và Dòng 225)**:
  ```html
  <article class="flex flex-col border border-emerald-100 dark:border-white/10 rounded-3xl p-7 gap-5 shadow-sm hover:shadow-xl transition-all duration-300 w-full max-w-[360px] bg-white dark:bg-volunteer-card transform hover:-translate-y-1">
  ```
- **Quan sát số liệu**:
  - `p-7` = `1.75rem = 28px` mỗi bên (tổng padding ngang là **56px**).
  - Vỏ bọc section (Dòng 102): `class="... max-w-7xl mx-auto px-6 w-full"` (`px-6` = 24px mỗi bên, tổng padding là **48px**).
  - Trên màn hình chuẩn 375px (iPhone SE):
    - Bề rộng khả dụng trong section: `375px - 48px = 327px`.
    - Bề rộng tối đa của thẻ là `max-w-[360px]` (> 327px).
    - Lòng thẻ sau khi trừ padding `p-7` (56px): `327px - 56px = 271px`.
    - Khoảng không gian 271px là quá chật chội để hiển thị 2 cột ("Cú pháp: [Họ và tên]_DongGopLangbiang"), gây áp lực ép tràn lề.

#### Điểm nóng 3: Nút Đối Tác Tài Trợ gây tràn màn hình trong Browser Test Runner
- **Vị trí (Dòng 323 và Dòng 329)**:
  ```html
  <div class="flex flex-wrap justify-center gap-5 sm:gap-6">
      <button type="button" class="group relative flex h-28 w-44 items-center justify-center rounded-2xl ... sm:h-32 sm:w-56 ...">
  ```
- **Bằng chứng từ kết quả chạy thực tế Headless Chrome (`node tests/run_mobile_viewport_browser_tests.js`)**:
  ```text
  Testing Page: Home / GayQuy (/Home/GayQuy)
    [FAIL] [375px] Overflow detected! scrollWidth=440px > viewport=375px (excess: +65px)
           Overflow culprits:
             - <button> rect.right=424px (+51px) class="group relative flex h-28 w-44 items-center justify-center ro"
             - <img> rect.right=416px (+43px) class="object-contain p-3 transition duration-300 group-hover:scale"
             - <button> rect.right=424px (+51px) class="group relative flex h-28 w-44 items-center justify-center ro"
  ```
- **Phân tích số liệu**: `w-44` = 176px. Hai nút đứng cạnh nhau: `176 + 20 (gap-5) + 176 = 372px`. Cộng thêm đệm `px-5` (40px) = `412px > 375px`.

#### Điểm nóng 4: Kích thước Vùng chạm (Touch Targets) trong `GayQuy.cshtml`
- **Nút đóng Modal QR (Dòng 348)**:
  ```html
  <button type="button" onclick="closeQrModal()" class="absolute top-4 right-4 h-9 w-9 rounded-full bg-slate-100 dark:bg-white/10 hover:bg-slate-200 dark:hover:bg-white/20 flex items-center justify-center font-bold text-slate-600 dark:text-slate-300">✕</button>
  ```
  `h-9 w-9` = **36px x 36px** (< 40px tiêu chuẩn chạm di động).
- **Nút đóng Modal Nhà tài trợ (Dòng 365)**:
  ```html
  <button type="button" onclick="closeSponsorModal()" class="absolute top-4 right-4 h-9 w-9 rounded-full bg-slate-100 dark:bg-white/10 hover:bg-slate-200 dark:hover:bg-white/20 flex items-center justify-center font-bold text-slate-600 dark:text-slate-300">✕</button>
  ```
  `h-9 w-9` = **36px x 36px** (< 40px).
- **Link CTA Modal Nhà tài trợ (Dòng 369)**:
  `px-6 py-2.5 text-sm` (~36px chiều cao).

---

### 1.3. Hiện Trạng `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`

#### Điểm nóng 1: Nút Icon Facebook vi phạm nghiêm trọng chuẩn kích thước vùng chạm (< 40px)
- **Vị trí 1 (Cấp 1 - Người đứng đầu, Dòng 305-309)**:
  ```html
  <a href="@root.LinkFacebook" target="_blank" title="Facebook của @root.HoTen"
     class="absolute bottom-0 right-0 h-8 w-8 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow-md hover:scale-110 transition">
      <svg class="h-4 w-4 fill-current" viewBox="0 0 24 24">...</svg>
  </a>
  ```
  Kích thước: `h-8 w-8` = **32px x 32px** (< 40px).
- **Vị trí 2 (Cấp 2 - Ban Thường trực / Trưởng ban, Dòng 413-416)**:
  ```html
  <a href="@item.LinkFacebook" target="_blank" title="Facebook của @item.HoTen"
     class="absolute bottom-0 right-0 h-6 w-6 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow hover:scale-110 transition">
      <svg class="h-3 w-3 fill-current" viewBox="0 0 24 24">...</svg>
  </a>
  ```
  Kích thước: `h-6 w-6` = **24px x 24px** (< 40px, đặc biệt cực kỳ khó bấm trúng trên ngón tay người dùng di động).
- **Vị trí 3 (Cấp 3 - Thành viên Ban tổ chức, Dòng 527-531)**:
  ```html
  <a href="@mem.LinkFacebook" target="_blank" title="Facebook của @mem.HoTen"
     class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full bg-[#1877F2]/10 text-[#1877F2] hover:bg-[#1877F2] hover:text-white transition">
      <svg class="h-3.5 w-3.5 fill-current" viewBox="0 0 24 24">...</svg>
  </a>
  ```
  Kích thước: `h-7 w-7` = **28px x 28px** (< 40px).

#### Điểm nóng 2: Các nút tương tác khác trong `BanToChuc.cshtml`
- **Nút "Xem thêm thành viên khác" (Dòng 540-545)**:
  ```html
  <button type="button" id="btnShowMoreMembers" onclick="toggleMoreMembers()"
          class="rounded-full bg-white px-7 py-2.5 text-xs sm:text-sm font-bold shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
          style="color: @lineColor; border: 1.5px solid @lineColor;">
      Xem thêm @(cap3Members.Count - initialShow) thành viên khác ▼
  </button>
  ```
  `py-2.5` + `text-xs` chỉ đạt ~36px chiều cao.

---

### 1.4. Khảo Sát Vùng Chạm Trên Các View Home Khác

| File View | Dòng Code & Phần Tử | Hiện Trạng Kích Thước | Đánh Giá & Rủi Ro |
| :--- | :--- | :--- | :--- |
| `Index.cshtml` | Dòng 349-352: `<button onclick="openNewsModal(...)"> Đọc tiếp &rarr; </button>` | Không padding (`cursor-pointer hover:underline`), chỉ cao ~20px (bằng font text) | Khó bấm trúng trên màn hình cảm ứng |
| `Index.cshtml` | Dòng 354-357: `<button onclick="likeNews(this)" class="flex items-center gap-1 text-gray-400...">` | Không padding, chiều cao đúng bằng icon 20px | Dễ bấm trượt hoặc bấm nhầm vào thẻ cha |
| `Index.cshtml` | Dòng 483: Nút đóng `#newsModal` `<button onclick="closeNewsModal()" class="... p-2">` | `p-2` + icon 24px = 40px | Đạt 40px, nên bổ sung `w-10 h-10` cố định để đảm bảo hình tròn chuẩn |
| `TinTuc.cshtml` | Dòng 184-191: Nút Like bài viết `<button onclick="likePost(...)">` | Không padding, chiều cao chỉ ~16px | Quá nhỏ, vi phạm chuẩn tiếp cận WCAG |
| `TinTuc.cshtml` | Dòng 225-230: Nút đóng `#newsReaderModal` `<button class="h-9 w-9...">` | `h-9 w-9` = 36px x 36px | Dưới 40px, cần nâng lên 40px |
| `TinTuc.cshtml` | Dòng 243-246: Link Fanpage `<a id="modalPostFbLink">` | Chỉ có padding chữ, không rõ dạng nút | Cần nâng `min-h-[44px]` dạng pill button |
| `ChuongTrinh.cshtml` | Dòng 41-48: 2 nút CTA Hero | `px-6 py-3` (~44px) | Tốt, cần thêm `min-h-[44px]` để cố định |
| `LoiChuc.cshtml` | Dòng 47 (CTA gửi lời chúc) & Dòng 109 (Nút nổi) | `min-h-12` (48px) và `min-h-14` (56px) | Hoàn hảo (>= 48px) |
| `ComingSoon.cshtml` | Dòng 92-99: Nút Fanpage và Tải lại trang | `px-8 py-3.5` (~48px) | Hoàn hảo |

---

## 2. Logic Chain (Chuỗi Suy Luận Từ Quan Sát Đến Giải Pháp)

1. **Khớp nối Test `M4_F06_T01`**:
   - Test kiểm tra sự xuất hiện của từ khóa `"break-all"` hoặc `"break-words"` trong `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`.
   - Cú pháp chuyển khoản ngân hàng trong thực tế có thể rất dài và không có khoảng trắng.
   - Khi không có `break-all`, trình duyệt giữ nguyên khối chuỗi, đẩy `flex justify-between` nở rộng sang phải, làm nứt vỡ giới hạn 375px của màn hình di động.
   - Thêm lớp `break-all` trên thẻ hiển thị cú pháp và tài khoản, đồng thời thêm `shrink-0` vào tiêu đề nhãn sẽ cho phép văn bản tự động xuống dòng an toàn, lập tức chuyển đổi trạng thái test `M4_F06_T01` từ `[SKIP]` sang `[PASS]`, nâng tỷ lệ vượt qua E2E Tests lên 552/552 (100%).

2. **Khớp nối Khoảng đệm Thẻ Quyên góp và Bề rộng 375px**:
   - `max-w-[360px]` lớn hơn bề rộng khả dụng `327px` trên màn hình 375px (khi container có `px-6`).
   - Padding bên trong `p-7` (28px mỗi bên = 56px) chiếm tới 17% bề rộng thẻ.
   - Giảm padding trên mobile xuống `p-5` (20px mỗi bên) và giữ `sm:p-7` trên tablet/desktop, đồng thời đổi `max-w-[360px]` thành `w-full max-w-full sm:max-w-[360px]` sẽ giải phóng thêm 32px không gian ngang, giúp toàn bộ nội dung ngân hàng hiển thị thoáng đãng và không bị dồn ép.

3. **Khắc phục Lỗi Tràn Màn Hình Của Các Nút Tài Trợ (`<button class="w-44">`)**:
   - Nút đối tác tài trợ dùng `w-44` (176px) cố định. Khi 2 nút đứng cạnh nhau trong `flex flex-wrap gap-5`, tổng độ rộng cần là 372px + 40px padding = 412px, vượt quá 375px/390px và bị trình duyệt Headless Chrome bắt lỗi trực tiếp.
   - Chuyển sang kích thước co giãn `w-[calc(50%-0.75rem)] max-w-[176px] sm:w-56` hoặc `w-36 sm:w-56` với `h-24 sm:h-32` vừa bảo đảm touch target rộng rãi (> 96px x 144px), vừa đảm bảo hai logo luôn vừa khít 100% trong màn hình 375px không sinh thanh cuộn ngang.

4. **Nâng cấp Toàn diện Kích thước Vùng chạm (Touch Target >= 40px)**:
   - Theo tiêu chuẩn công thái học di động (WCAG 2.5.5 / 2.5.8 và Apple/Google Design Guidelines), vùng ngón tay chạm tối thiểu phải đạt 40px x 40px (hoặc 44px x 44px) để tránh hiện tượng bấm trượt hoặc bấm nhầm.
   - Toàn bộ các icon Facebook trong `BanToChuc.cshtml` (24px, 28px, 32px) phải được nâng lên `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Toàn bộ các nút đóng modal (36px `h-9 w-9`) phải được nâng lên `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Các nút Like và nút "Đọc tiếp" trong `Index.cshtml` và `TinTuc.cshtml` phải được bổ sung padding âm/dương để mở rộng hộp chạm (hitbox) đạt tối thiểu `min-h-[40px]`.

5. **Bảo toàn Ranh giới Quản trị (`/admin`)**:
   - Tất cả các chỉnh sửa nói trên chỉ nằm cục bộ trong các file Razor view thuộc thư mục `WEB_TinhNguyen/GiaoDien/Home/`.
   - Tuyệt đối không chạm vào bất kỳ file nào thuộc `GiaoDien/Admin*`, `BoDieuKhien/Admin*`, `Areas/CuaHang/**/Admin*`, hay `_AdminLayout.cshtml`.

---

## 3. Caveats (Các Giả Định & Vấn Đề Cần Lưu Ý)

1. **Giả định về hiển thị Icon Facebook**: Khi nâng kích thước nút Facebook trên avatar Cấp 2 từ `h-6 w-6` (24px) lên `h-10 w-10` (40px), việc đặt vị trí lớp `class="absolute -bottom-1 -right-1 ..."` trên khung avatar `w-[88px] h-[88px]` đảm bảo nút bám khéo léo vào góc phần tư dưới phải của ảnh đại diện mà không che mất khuôn mặt thành viên.
2. **Tương thích với các Explorer khác trong Milestone 2**:
   - Explorer `m2_explorer_1` tập trung vào kích cỡ chữ tiêu đề (typography `clamp()` và Tailwind responsive classes).
   - Explorer `m2_explorer_2` tập trung vào bố cục khung lưới countdown và container chung.
   - Các đề xuất của `m2_explorer_3` về `break-all` và touch targets hoàn toàn độc lập, tương thích và bổ trợ trực tiếp cho 2 agent trên.
3. **Không chỉnh sửa CSS biên dịch bằng tay**: Toàn bộ tinh chỉnh sử dụng utility classes tiêu chuẩn của Tailwind CSS (`break-all`, `min-w-[40px]`, `min-h-[40px]`, `p-5 sm:p-7`, `shrink-0`).

---

## 4. Conclusion & Actionable Code Replacement Proposals

### 4.1. File `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`

#### [Sửa 1] Thẻ Kênh Quyên Góp: Padding, Max-Width và `break-all` cho Cú pháp CK
- **Áp dụng tại**: Dòng 153-193 (Vòng lặp `Model.KenhDongGops`) và Dòng 225-251 (Vòng lặp dự phòng `donationQrConfigs`).
- **Before (Dòng 153)**:
  ```html
  <article class="flex flex-col border border-emerald-100 dark:border-white/10 rounded-3xl p-7 gap-5 shadow-sm hover:shadow-xl transition-all duration-300 w-full max-w-[360px] bg-white dark:bg-volunteer-card transform hover:-translate-y-1">
  ```
- **After (Dòng 153)**:
  ```html
  <article class="flex flex-col border border-emerald-100 dark:border-white/10 rounded-3xl p-5 sm:p-7 gap-5 shadow-sm hover:shadow-xl transition-all duration-300 w-full max-w-full sm:max-w-[360px] bg-white dark:bg-volunteer-card transform hover:-translate-y-1">
  ```

- **Before (Dòng 174-192)**:
  ```html
  <div class="mt-2 text-xs text-slate-600 dark:text-slate-300 space-y-1 bg-slate-50 dark:bg-white/5 p-3 rounded-xl border border-slate-200 dark:border-white/10">
      <div class="flex justify-between">
          <span class="text-slate-400">Ngân hàng:</span>
          <span class="font-bold text-slate-800 dark:text-white">@effectiveBank</span>
      </div>
      <div class="flex justify-between">
          <span class="text-slate-400">Số tài khoản:</span>
          <span class="font-mono font-bold text-emerald-600 dark:text-emerald-400 select-all">@effectiveAccNum</span>
      </div>
      <div class="flex justify-between">
          <span class="text-slate-400">Chủ TK:</span>
          <span class="font-semibold text-slate-800 dark:text-white uppercase">@effectiveAccName</span>
      </div>
      <div class="flex justify-between">
          <span class="text-slate-400">Cú pháp:</span>
          <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all">@effectiveMemo</span>
      </div>
  </div>
  ```
- **After (Dòng 174-192)**:
  ```html
  <div class="mt-2 text-xs text-slate-600 dark:text-slate-300 space-y-1 bg-slate-50 dark:bg-white/5 p-3 rounded-xl border border-slate-200 dark:border-white/10">
      <div class="flex items-start justify-between gap-2">
          <span class="text-slate-400 shrink-0">Ngân hàng:</span>
          <span class="font-bold text-slate-800 dark:text-white text-right">@effectiveBank</span>
      </div>
      <div class="flex items-start justify-between gap-2">
          <span class="text-slate-400 shrink-0">Số tài khoản:</span>
          <span class="font-mono font-bold text-emerald-600 dark:text-emerald-400 select-all text-right break-all">@effectiveAccNum</span>
      </div>
      <div class="flex items-start justify-between gap-2">
          <span class="text-slate-400 shrink-0">Chủ TK:</span>
          <span class="font-semibold text-slate-800 dark:text-white uppercase text-right">@effectiveAccName</span>
      </div>
      <div class="flex items-start justify-between gap-2">
          <span class="text-slate-400 shrink-0">Cú pháp:</span>
          <span class="font-mono font-semibold text-amber-600 dark:text-amber-400 select-all text-right break-all">@effectiveMemo</span>
      </div>
  </div>
  ```
*(Tương tự áp dụng cho thẻ dự phòng tại Dòng 225-251)*.

#### [Sửa 2] Nút bấm hành động trong Card & Nút đóng Modals
- **Dòng 199 & 255 (Nút mở QR Modal)**: Thêm `min-h-[44px]`
  ```html
  class="inline-flex items-center justify-center gap-2 text-white font-bold text-sm px-5 py-3 rounded-2xl bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 shadow-md hover:shadow-lg transition-all w-full mt-auto cursor-pointer min-h-[44px]"
  ```
- **Dòng 207 (Nút Link ngoài)**: Thêm `min-h-[44px]`
  ```html
  class="inline-flex items-center justify-center gap-2 font-bold text-sm px-5 py-3 rounded-2xl text-white bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 shadow-md hover:shadow-lg transition-all w-full mt-auto cursor-pointer min-h-[44px]"
  ```
- **Dòng 348 (Nút đóng QR Modal)**:
  - **Before**: `class="absolute top-4 right-4 h-9 w-9 rounded-full ..."`
  - **After**: `class="absolute top-4 right-4 h-10 w-10 min-w-[40px] min-h-[40px] rounded-full bg-slate-100 dark:bg-white/10 hover:bg-slate-200 dark:hover:bg-white/20 flex items-center justify-center font-bold text-slate-600 dark:text-slate-300 transition focus:outline-none"`
- **Dòng 357 (Nội dung CK trong QR Modal)**:
  - **Before**: `<p><span class="text-slate-500 dark:text-slate-400">Nội dung CK:</span> <strong id="qrModalNote" class="text-amber-700 dark:text-amber-400 select-all"></strong></p>`
  - **After**: `<p class="flex items-start justify-between gap-2"><span class="text-slate-500 dark:text-slate-400 shrink-0">Nội dung CK:</span> <strong id="qrModalNote" class="text-amber-700 dark:text-amber-400 select-all break-all text-right"></strong></p>`
- **Dòng 365 (Nút đóng Sponsor Modal)**:
  - **Before**: `class="absolute top-4 right-4 h-9 w-9 rounded-full ..."`
  - **After**: `class="absolute top-4 right-4 h-10 w-10 min-w-[40px] min-h-[40px] rounded-full bg-slate-100 dark:bg-white/10 hover:bg-slate-200 dark:hover:bg-white/20 flex items-center justify-center font-bold text-slate-600 dark:text-slate-300 transition focus:outline-none"`
- **Dòng 369 (CTA Sponsor Modal)**: Thêm `min-h-[44px]`
  - `class="inline-flex items-center justify-center gap-2 rounded-full bg-emerald-600 hover:bg-emerald-700 text-white font-bold px-6 py-3 min-h-[44px] text-sm transition shadow-md"`

#### [Sửa 3] Nút Nhà Tài Trợ trong `GayQuy.cshtml` (Dòng 329)
- **Before (Dòng 329)**:
  ```html
  <button type="button"
          title="@sp.TenDonVi"
          onclick="openSponsorModal(...)"
          class="group relative flex h-28 w-44 items-center justify-center rounded-2xl bg-white dark:bg-volunteer-card p-5 ring-1 ring-emerald-100 dark:ring-white/10 transition hover:-translate-y-1.5 sm:h-32 sm:w-56 sm:p-6 shadow-sm hover:shadow-md cursor-pointer">
  ```
- **After (Dòng 329)**:
  ```html
  <button type="button"
          title="@sp.TenDonVi"
          onclick="openSponsorModal(...)"
          class="group relative flex h-24 w-[calc(50%-0.75rem)] max-w-[176px] sm:w-56 sm:h-32 items-center justify-center rounded-2xl bg-white dark:bg-volunteer-card p-4 sm:p-6 ring-1 ring-emerald-100 dark:ring-white/10 transition hover:-translate-y-1.5 shadow-sm hover:shadow-md cursor-pointer">
  ```

---

### 4.2. File `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`

#### [Sửa 1] Nút Facebook Cấp 1 (Dòng 305-309)
- **Before**:
  ```html
  <a href="@root.LinkFacebook" target="_blank" title="Facebook của @root.HoTen"
     class="absolute bottom-0 right-0 h-8 w-8 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow-md hover:scale-110 transition">
      <svg class="h-4 w-4 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```
- **After**:
  ```html
  <a href="@root.LinkFacebook" target="_blank" title="Facebook của @root.HoTen"
     class="absolute bottom-0 right-0 h-10 w-10 min-w-[40px] min-h-[40px] rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow-md hover:scale-110 transition focus:outline-none focus:ring-2 focus:ring-[#1877F2]">
      <svg class="h-4.5 w-4.5 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```

#### [Sửa 2] Nút Facebook Cấp 2 (Dòng 413-416)
- **Before**:
  ```html
  <a href="@item.LinkFacebook" target="_blank" title="Facebook của @item.HoTen"
     class="absolute bottom-0 right-0 h-6 w-6 rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow hover:scale-110 transition">
      <svg class="h-3 w-3 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```
- **After**:
  ```html
  <a href="@item.LinkFacebook" target="_blank" title="Facebook của @item.HoTen"
     class="absolute -bottom-1 -right-1 h-10 w-10 min-w-[40px] min-h-[40px] rounded-full bg-[#1877F2] text-white flex items-center justify-center shadow-md hover:scale-110 transition focus:outline-none focus:ring-2 focus:ring-[#1877F2]">
      <svg class="h-4 w-4 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```

#### [Sửa 3] Nút Facebook Cấp 3 (Dòng 527-531)
- **Before**:
  ```html
  <a href="@mem.LinkFacebook" target="_blank" title="Facebook của @mem.HoTen"
     class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full bg-[#1877F2]/10 text-[#1877F2] hover:bg-[#1877F2] hover:text-white transition">
      <svg class="h-3.5 w-3.5 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```
- **After**:
  ```html
  <a href="@mem.LinkFacebook" target="_blank" title="Facebook của @mem.HoTen"
     class="flex h-10 w-10 min-w-[40px] min-h-[40px] flex-shrink-0 items-center justify-center rounded-full bg-[#1877F2]/10 text-[#1877F2] hover:bg-[#1877F2] hover:text-white transition focus:outline-none focus:ring-2 focus:ring-[#1877F2]">
      <svg class="h-4 w-4 fill-current" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"></path></svg>
  </a>
  ```

#### [Sửa 4] Nút Mở rộng Thành viên Cấp 3 & Nút CTA (Dòng 540 & Dòng 559)
- **Before (Dòng 540)**:
  ```html
  <button type="button" id="btnShowMoreMembers" onclick="toggleMoreMembers()"
          class="rounded-full bg-white px-7 py-2.5 text-xs sm:text-sm font-bold shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
          style="color: @lineColor; border: 1.5px solid @lineColor;">
  ```
- **After (Dòng 540)**:
  ```html
  <button type="button" id="btnShowMoreMembers" onclick="toggleMoreMembers()"
          class="inline-flex items-center justify-center min-h-[44px] rounded-full bg-white px-7 py-3 text-xs sm:text-sm font-bold shadow-sm transition hover:-translate-y-0.5 hover:shadow-md focus:outline-none focus:ring-2"
          style="color: @lineColor; border: 1.5px solid @lineColor;">
  ```
- **After (Dòng 559 - CTA TNV)**:
  ```html
  <a href="@(Model?.CTA_LinkNut ?? "/#register")"
     class="inline-flex items-center justify-center min-h-[44px] rounded-full px-8 py-3.5 font-bold text-white shadow-md transition hover:-translate-y-0.5 hover:brightness-110"
     style="background-color: @lineColor;">
  ```

---

### 4.3. Các Tinh Chỉnh Trên `Index.cshtml` & `TinTuc.cshtml`

#### `Index.cshtml`:
- **Dòng 349-352 (Nút Đọc tiếp)**:
  - **After**: `<button type="button" onclick="openNewsModal(...)" class="inline-flex items-center min-h-[40px] py-2 pr-3 -ml-1 cursor-pointer hover:underline font-semibold" style="color: #4caf50;"> Đọc tiếp &rarr; </button>`
- **Dòng 354-357 (Nút Thả tim)**:
  - **After**: `<button type="button" onclick="likeNews(this)" class="inline-flex items-center justify-center min-h-[40px] min-w-[40px] p-2 -mr-2 gap-1.5 text-gray-400 hover:text-red-500 transition-colors cursor-pointer">`
- **Dòng 483 (Nút đóng Modal Tin tức)**:
  - **After**: `<button onclick="closeNewsModal()" class="w-10 h-10 min-w-[40px] min-h-[40px] flex items-center justify-center text-gray-400 hover:text-red-500 bg-gray-100 dark:bg-gray-800 hover:bg-red-50 rounded-full transition focus:outline-none">`

#### `TinTuc.cshtml`:
- **Dòng 184-191 (Nút Like bài viết)**:
  - **After**: `<button type="button" onclick="likePost(@news.IDBaiViet, this)" aria-label="Thích bài viết" class="inline-flex items-center justify-center min-h-[40px] min-w-[40px] p-2 -mr-2 gap-1.5 text-xs font-semibold transition-all hover:scale-105 text-gray-700 hover:text-rose-500 cursor-pointer">`
- **Dòng 225-230 (Nút đóng Modal đọc tin)**:
  - **After**: `<button type="button" onclick="closeNewsModal()" class="h-10 w-10 min-w-[40px] min-h-[40px] shrink-0 rounded-full bg-gray-100 hover:bg-gray-200 text-gray-500 hover:text-gray-800 flex items-center justify-center transition focus:outline-none">`
- **Dòng 243-246 (Link Fanpage trong Modal)**:
  - **After**: `<a id="modalPostFbLink" href="#" target="_blank" rel="noopener noreferrer" class="inline-flex items-center justify-center min-h-[44px] px-4 py-2.5 rounded-xl bg-emerald-50 hover:bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:hover:bg-emerald-900 dark:text-emerald-300 font-bold text-xs sm:text-sm transition">`

---

## 5. Verification Method (Quy Trình Kiểm Tra & Xác Minh Độc Lập)

Khi agent thực thi (Worker) áp dụng các mã đề xuất trên, tiến hành kiểm tra theo quy trình 4 bước sau:

### Bước 1: Biên dịch dự án và toàn bộ test suite
```powershell
dotnet build WEB_TinhNguyen.sln
```
*Điều kiện thành công*: Biên dịch 0 lỗi (0 Errors).

### Bước 2: Kiểm tra test hợp đồng `M4_F06_T01` trong `E2ETests`
```powershell
dotnet run --project tests/E2ETests/E2ETests.csproj
```
*Điều kiện thành công*: 
- Test `[SKIP] Tier 1 | F06 | M4_F06_T01` chuyển thành:
  ```text
  [PASS] Tier 1 | F06 | M4_F06_T01: Donation bank transfer memos enforce break-all or break-words
  ```
- Kết quả tổng thể: **552/552 Tests PASSED (100%)**, 0 Failed, 0 Skipped!

### Bước 3: Kiểm tra giao diện thực tế trên Headless Chrome Browser Runner
```powershell
node tests/run_mobile_viewport_browser_tests.js
```
*Điều kiện thành công*:
- Trang `Home / GayQuy` không còn bị lỗi overflow bởi các nút `<button class="w-44">`.
- Kích thước các phần tử form input và nút bấm đạt chuẩn không bị co ngắn dưới 34px/40px.

### Bước 4: Kiểm tra tính bất biến của khu vực Admin (/admin)
```powershell
git -c safe.directory=D:/web_tinh_nguyen status --porcelain
```
*Điều kiện thành công*:
- Tuyệt đối KHÔNG có bất kỳ file nào có tiền tố `WEB_TinhNguyen/GiaoDien/Admin`, `WEB_TinhNguyen/BoDieuKhien/Admin`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin`, hoặc `_AdminLayout.cshtml` bị sửa đổi (`M`).
