// =========================================================================
// TIỆN ÍCH BẢO MẬT MẬT KHẨU (PBKDF2 HMAC-SHA256)
// =========================================================================

using System;
using System.Security.Cryptography;

namespace WEB_TinhNguyen.DuLieu
{
    /// <summary>
    /// Cung cấp các phương thức băm và kiểm tra mật khẩu bằng thuật toán chuẩn PBKDF2 (HMAC-SHA256).
    /// Đảm bảo tính ngẫu nhiên của muối bảo mật (Salt) và chống tấn công Timing Attack.
    /// </summary>
    public static class BaoMatMatKhau
    {
        /// <summary>
        /// Kích thước muối ngẫu nhiên (16 bytes = 128-bit entropy).
        /// </summary>
        public const int KichThuocMuoiBytes = 16;

        /// <summary>
        /// Kích thước chuỗi băm đầu ra (32 bytes = 256-bit subkey).
        /// </summary>
        public const int KichThuocHashBytes = 32;

        /// <summary>
        /// Số vòng lặp băm PBKDF2 (tối thiểu 100.000 vòng theo chuẩn an toàn hiện đại).
        /// </summary>
        public const int SoVongLap = 100_000;

        /// <summary>
        /// Tạo chuỗi muối (salt) ngẫu nhiên 16 bytes bằng bộ sinh số ngẫu nhiên mật mã (CSPRNG).
        /// Trả về chuỗi được mã hóa Base64 (24 ký tự).
        /// </summary>
        public static string TaoMuoiNgauNhien()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(KichThuocMuoiBytes);
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Băm mật khẩu cùng với chuỗi muối bằng thuật toán PBKDF2 (HMAC-SHA256, 100.000 vòng lặp).
        /// Trả về chuỗi băm 32 bytes được mã hóa Base64 (44 ký tự).
        /// </summary>
        /// <param name="matKhau">Mật khẩu dạng chuỗi văn bản thuần cần băm.</param>
        /// <param name="muoiBaoMat">Chuỗi muối ngẫu nhiên định dạng Base64.</param>
        /// <returns>Chuỗi hash Base64 của mật khẩu.</returns>
        public static string BamMatKhau(string matKhau, string muoiBaoMat)
        {
            if (string.IsNullOrEmpty(matKhau))
            {
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(matKhau));
            }
            if (string.IsNullOrEmpty(muoiBaoMat))
            {
                throw new ArgumentException("Muối bảo mật không được để trống.", nameof(muoiBaoMat));
            }

            byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                matKhau,
                saltBytes,
                SoVongLap,
                HashAlgorithmName.SHA256,
                KichThuocHashBytes);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Kiểm tra tính khớp nối của mật khẩu người dùng nhập vào so với chuỗi hash đã lưu trong CSDL.
        /// Sử dụng so sánh đẳng thời (FixedTimeEquals) để triệt tiêu hoàn toàn nguy cơ Timing Attack.
        /// </summary>
        /// <param name="matKhauNhap">Mật khẩu do người dùng nhập từ form đăng nhập.</param>
        /// <param name="matKhauHashDaLuu">Chuỗi băm Base64 đã lưu trong cơ sở dữ liệu.</param>
        /// <param name="muoiBaoMat">Chuỗi muối Base64 đã lưu trong cơ sở dữ liệu.</param>
        /// <returns>True nếu mật khẩu chính xác; ngược lại False.</returns>
        public static bool KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat)
        {
            if (string.IsNullOrEmpty(matKhauNhap) || 
                string.IsNullOrEmpty(matKhauHashDaLuu) || 
                string.IsNullOrEmpty(muoiBaoMat))
            {
                return false;
            }

            try
            {
                byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
                byte[] expectedHashBytes = Convert.FromBase64String(matKhauHashDaLuu);
                byte[] actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                    matKhauNhap,
                    saltBytes,
                    SoVongLap,
                    HashAlgorithmName.SHA256,
                    expectedHashBytes.Length > 0 ? expectedHashBytes.Length : KichThuocHashBytes);

                return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
            }
            catch
            {
                // Phòng vệ an toàn khi chuỗi Base64 không hợp lệ
                return false;
            }
        }

        // =========================================================================
        // CONTRACT ALIASES (Tương thích giao diện tiếng Anh theo PROJECT.md)
        // =========================================================================

        /// <summary>
        /// Tạo muối mới và băm mật khẩu, trả về chuỗi hash và chuỗi muối qua tham số out salt.
        /// </summary>
        public static string HashPassword(string password, out string salt)
        {
            salt = TaoMuoiNgauNhien();
            return BamMatKhau(password, salt);
        }

        /// <summary>
        /// Kiểm tra mật khẩu dựa trên hash và salt đã lưu.
        /// </summary>
        public static bool VerifyPassword(string password, string hash, string salt)
        {
            return KiemTraMatKhau(password, hash, salt);
        }
    }

    /// <summary>
    /// Alias tương thích cho PasswordHasher để hỗ trợ các kiểm thử và tài liệu tiếng Anh.
    /// </summary>
    public static class PasswordHasher
    {
        public static string HashPassword(string password, out string salt) => BaoMatMatKhau.HashPassword(password, out salt);
        public static bool VerifyPassword(string password, string hash, string salt) => BaoMatMatKhau.VerifyPassword(password, hash, salt);
    }
}
