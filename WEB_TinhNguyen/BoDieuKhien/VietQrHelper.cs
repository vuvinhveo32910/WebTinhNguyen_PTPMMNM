using System;
using System.Collections.Generic;
using System.Linq;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Tiện ích hỗ trợ tạo mã VietQR chuẩn Napas247 tự động theo quy chuẩn VietQR API.
    /// Hỗ trợ cả hai phân hệ: Cửa hàng gây quỹ và Kênh đóng góp thiện nguyện.
    /// </summary>
    public static class VietQrHelper
    {
        public const string DefaultBankCode = "MB";
        public const string DefaultAccountNumber = "0924044283";
        public const string DefaultAccountName = "NGUYEN THAI VINH";
        public const string DefaultTemplate = "compact2";

        /// <summary>
        /// Tạo đường dẫn ảnh VietQR chuẩn Napas247 từ dịch vụ img.vietqr.io.
        /// Định dạng: https://img.vietqr.io/image/{bank}-{accountNumber}-{template}.png?amount={amount}&addInfo={memo}&accountName={accountName}
        /// </summary>
        /// <param name="maNganHang">Mã ngân hàng (ví dụ: MB, VCB, TCB, ICB, ACB, BIDV)</param>
        /// <param name="soTaiKhoan">Số tài khoản nhận tiền</param>
        /// <param name="amount">Số tiền chuyển khoản (VND, số nguyên)</param>
        /// <param name="memo">Nội dung / cú pháp chuyển khoản</param>
        /// <param name="tenChuTaiKhoan">Tên chủ tài khoản nhận tiền</param>
        /// <param name="template">Mẫu giao diện VietQR: compact2, compact, qr_only, print</param>
        /// <returns>URL hình ảnh VietQR hoàn chỉnh</returns>
        public static string GenerateUrl(
            string maNganHang,
            string soTaiKhoan,
            decimal? amount = null,
            string? memo = null,
            string? tenChuTaiKhoan = null,
            string template = "compact2")
        {
            var cleanBank = string.IsNullOrWhiteSpace(maNganHang) ? DefaultBankCode : maNganHang.Trim().ToUpperInvariant();
            var cleanAcc = string.IsNullOrWhiteSpace(soTaiKhoan) ? DefaultAccountNumber : soTaiKhoan.Trim();
            var cleanTemplate = string.IsNullOrWhiteSpace(template) ? DefaultTemplate : template.Trim().ToLowerInvariant();

            var queryParams = new List<string>();

            if (amount.HasValue && amount.Value > 0)
            {
                var cleanAmount = (long)Math.Max(0, amount.Value);
                queryParams.Add($"amount={cleanAmount}");
            }

            if (!string.IsNullOrWhiteSpace(memo))
            {
                queryParams.Add($"addInfo={Uri.EscapeDataString(memo.Trim())}");
            }

            if (!string.IsNullOrWhiteSpace(tenChuTaiKhoan))
            {
                queryParams.Add($"accountName={Uri.EscapeDataString(tenChuTaiKhoan.Trim().ToUpperInvariant())}");
            }

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
            return $"https://img.vietqr.io/image/{cleanBank}-{cleanAcc}-{cleanTemplate}.png{queryString}";
        }

        /// <summary>
        /// Danh sách các ngân hàng phổ biến tại Việt Nam hỗ trợ tạo mã VietQR nhanh
        /// </summary>
        public static readonly IReadOnlyList<(string Code, string ShortName, string FullName)> PopularBanks = new List<(string, string, string)>
        {
            ("MB", "MBBank", "Ngân hàng TMCP Quân Đội (MB)"),
            ("VCB", "Vietcombank", "Ngân hàng TMCP Ngoại thương Việt Nam"),
            ("ICB", "VietinBank", "Ngân hàng TMCP Công Thương Việt Nam"),
            ("BIDV", "BIDV", "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam"),
            ("TCB", "Techcombank", "Ngân hàng TMCP Kỹ Thương Việt Nam"),
            ("ACB", "ACB", "Ngân hàng TMCP Á Châu"),
            ("VPB", "VPBank", "Ngân hàng TMCP Việt Nam Thịnh Vượng"),
            ("TPB", "TPBank", "Ngân hàng TMCP Tiên Phong"),
            ("STB", "Sacombank", "Ngân hàng TMCP Sài Gòn Thương Tín"),
            ("HDB", "HDBank", "Ngân hàng TMCP Phát triển TP. Hồ Chí Minh"),
            ("VIB", "VIB", "Ngân hàng TMCP Quốc tế Việt Nam"),
            ("SHB", "SHB", "Ngân hàng TMCP Sài Gòn - Hà Nội"),
            ("MSB", "MSB", "Ngân hàng TMCP Hàng Hải Việt Nam"),
            ("OCB", "OCB", "Ngân hàng TMCP Phương Đông"),
            ("LPB", "LPBank", "Ngân hàng TMCP Lộc Phát Việt Nam"),
            ("SEAB", "SeABank", "Ngân hàng TMCP Đông Nam Á")
        };
    }
}
