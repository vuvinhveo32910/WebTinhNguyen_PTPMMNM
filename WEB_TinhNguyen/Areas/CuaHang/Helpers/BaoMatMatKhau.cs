// =========================================================================
// LỚP CHUYỂN TIẾP CHO KHU VỰC CỬA HÀNG VÀ CÁC MODULE NGOẠI VI
// =========================================================================

namespace WEB_TinhNguyen.Areas.CuaHang.Helpers
{
    /// <summary>
    /// Chuyển tiếp tới WEB_TinhNguyen.DuLieu.BaoMatMatKhau để đảm bảo mã gọi từ bất kỳ namespace nào cũng tương thích 100%.
    /// </summary>
    public static class BaoMatMatKhau
    {
        public static string TaoMuoiNgauNhien() => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.TaoMuoiNgauNhien();
        public static string BamMatKhau(string matKhau, string muoiBaoMat) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.BamMatKhau(matKhau, muoiBaoMat);
        public static bool KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.KiemTraMatKhau(matKhauNhap, matKhauHashDaLuu, muoiBaoMat);
        public static string HashPassword(string password, out string salt) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.HashPassword(password, out salt);
        public static bool VerifyPassword(string password, string hash, string salt) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.VerifyPassword(password, hash, salt);
    }
}
