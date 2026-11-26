using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.Areas.CuaHang.Models;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.Areas.CuaHang.Helpers
{
    /// <summary>
    /// Bộ khởi tạo dữ liệu mẫu và cung cấp cơ chế In-Memory Fallback khi database offline.
    /// Đảm bảo hệ thống hoạt động trơn tru trong mọi môi trường (Production có SQL Server hoặc Demo Offline).
    /// </summary>
    public static class DbInitializer
    {
        private static readonly object _syncLock = new object();
        private static readonly List<SanPhamGayQuy> _inMemoryProducts = new List<SanPhamGayQuy>();
        private static readonly List<DonHangGayQuy> _inMemoryOrders = new List<DonHangGayQuy>();
        private static readonly List<CauHinhThanhToanQR> _inMemoryQrConfigs = new List<CauHinhThanhToanQR>();
        private static readonly List<TaiKhoanAdmin> _inMemoryAdmins = new List<TaiKhoanAdmin>();
        private static int _nextProductId = 5;
        private static int _nextOrderId = 1;
        private static int _nextQrConfigId = 2;
        private static int _nextAdminId = 2;
        private static bool _sampleOrdersSeeded = false;

        // Cache chuỗi muối và hash của admin mặc định để tối ưu hóa hiệu năng trong kiểm thử song song
        private static string? _defaultAdminSalt;
        private static string? _defaultAdminHash;
        private static readonly object _adminHashLock = new object();

        static DbInitializer()
        {
            ResetInMemoryStore();
            SeedSampleOrders();
        }

        /// <summary>
        /// Lấy hoặc tạo chuỗi muối và mã băm PBKDF2 của tài khoản admin mặc định ("Admin@123").
        /// Sử dụng bộ nhớ đệm an toàn theo luồng để tối ưu hóa hiệu năng trong các kiểm thử đa luồng.
        /// </summary>
        private static (string salt, string hash) GetOrCreateDefaultAdminCredentials()
        {
            lock (_adminHashLock)
            {
                if (string.IsNullOrEmpty(_defaultAdminSalt) || string.IsNullOrEmpty(_defaultAdminHash))
                {
                    _defaultAdminSalt = BaoMatMatKhau.TaoMuoiNgauNhien();
                    _defaultAdminHash = BaoMatMatKhau.BamMatKhau("Admin@123", _defaultAdminSalt);
                }
                return (_defaultAdminSalt, _defaultAdminHash);
            }
        }

        /// <summary>
        /// Danh sách tài khoản quản trị viên mặc định ("admin" / "Admin@123") với mật khẩu được băm bảo mật bằng PBKDF2.
        /// </summary>
        public static List<TaiKhoanAdmin> GetDefaultAdmins()
        {
            var (salt, hash) = GetOrCreateDefaultAdminCredentials();

            return new List<TaiKhoanAdmin>
            {
                new TaiKhoanAdmin
                {
                    IDTaiKhoan = 1,
                    TenDangNhap = "admin",
                    MatKhauHash = hash,
                    MuoiBaoMat = salt,
                    HoTen = "Quản trị viên Hệ thống",
                    Email = "admin@tinhnguyen.vn",
                    VaiTro = "Admin",
                    PhanQuyen = QuyenAdmin.All,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now,
                    LanDangNhapCuoi = null
                }
            };
        }

        /// <summary>
        /// Tạo bản sao dữ liệu TaiKhoanAdmin để đảm bảo an toàn truy cập (Clone Isolation).
        /// </summary>
        private static TaiKhoanAdmin CloneAdmin(TaiKhoanAdmin a)
        {
            return new TaiKhoanAdmin
            {
                IDTaiKhoan = a.IDTaiKhoan,
                TenDangNhap = a.TenDangNhap,
                MatKhauHash = a.MatKhauHash,
                MuoiBaoMat = a.MuoiBaoMat,
                HoTen = a.HoTen,
                Email = a.Email,
                VaiTro = a.VaiTro,
                PhanQuyen = string.IsNullOrWhiteSpace(a.PhanQuyen) ? QuyenAdmin.All : a.PhanQuyen,
                TrangThaiHoatDong = a.TrangThaiHoatDong,
                NgayTao = a.NgayTao,
                LanDangNhapCuoi = a.LanDangNhapCuoi
            };
        }

        /// <summary>
        /// Danh sách 4 sản phẩm gây quỹ ban đầu mang bản sắc "Trăng Sáng Langbiang 2026".
        /// Sử dụng hình ảnh cục bộ có sẵn trong wwwroot/assets/.
        /// </summary>
        public static List<SanPhamGayQuy> GetDefaultProducts()
        {
            return new List<SanPhamGayQuy>
            {
                new SanPhamGayQuy
                {
                    IDSanPham = 1,
                    TenSanPham = "Áo Thun Tình Nguyện \"Trăng Sáng Langbiang 2026\"",
                    GiaBan = 129000m,
                    GiaGoc = 150000m,
                    MoTaNgan = "Áo thun cotton 100% 4 chiều thoáng mát, in logo chính thức của chiến dịch Trăng Sáng Langbiang 2026.",
                    MoTaChiTiet = "Sản phẩm áo thun đồng phục chính thức của chiến dịch thiện nguyện 'Trăng Sáng Langbiang 2026'. Vải cotton 100% co giãn 4 chiều mềm mịn, thoáng khí, thấm hút mồ hôi tối ưu, form áo trẻ trung năng động phù hợp cả nam và nữ. Mỗi chiếc áo được trao đi là sự chung tay đóng góp trực tiếp 50.000 VNĐ vào quỹ quà tặng Trung thu cho 200 em nhỏ đồng bào K'Ho tại chân núi Langbiang huyền thoại.",
                    ThongDiepGayQuy = "Đóng góp 50.000₫/áo vào quỹ quà tặng Trung thu cho 200 em nhỏ đồng bào K'Ho tại Langbiang.",
                    PhanTramDongGop = 100,
                    SoLuongTon = 100,
                    DaBan = 35,
                    DanhMuc = "Trang phục",
                    LinkHinhAnh = "/assets/journey.jpg",
                    TrangThai = true,
                    ThuTu = 1,
                    NgayTao = DateTime.Now.AddDays(-10)
                },
                new SanPhamGayQuy
                {
                    IDSanPham = 2,
                    TenSanPham = "Túi Tote Canvas \"Yêu Thương Gửi Về Núi Rừng\"",
                    GiaBan = 79000m,
                    GiaGoc = 99000m,
                    MoTaNgan = "Túi vải canvas mộc tự nhiên, thân thiện môi trường, quai đeo chịu lực tốt đựng vừa laptop 14 inch.",
                    MoTaChiTiet = "Túi vải canvas mộc mạc dệt từ sợi tự nhiên, thân thiện môi trường và an toàn cho sức khỏe. Thiết kế tối giản tinh tế với họa tiết rừng thông Langbiang cùng thông điệp 'Yêu thương gửi về núi rừng'. Có khóa kéo miệng an toàn và ngăn phụ bên trong. 100% lợi nhuận từ sản phẩm được chuyển vào chương trình 'Nấu ăn cho em' mang tới 250 suất phở bò nóng sốt ấm lòng trẻ thơ vùng cao.",
                    ThongDiepGayQuy = "100% lợi nhuận dành cho chương trình 'Nấu ăn cho em' với 250 suất phở bò nóng hổi nơi vùng cao.",
                    PhanTramDongGop = 100,
                    SoLuongTon = 85,
                    DaBan = 52,
                    DanhMuc = "Phụ kiện",
                    LinkHinhAnh = "/assets/cooking.jpg",
                    TrangThai = true,
                    ThuTu = 2,
                    NgayTao = DateTime.Now.AddDays(-8)
                },
                new SanPhamGayQuy
                {
                    IDSanPham = 3,
                    TenSanPham = "Bình Giữ Nhiệt Vỏ Tre Khắc Tên 500ml",
                    GiaBan = 159000m,
                    GiaGoc = 199000m,
                    MoTaNgan = "Ruột inox 304 giữ nhiệt 12h, vỏ gỗ tre tự nhiên khắc laser biểu tượng đỉnh Langbiang hùng vĩ.",
                    MoTaChiTiet = "Bình giữ nhiệt phong cách Eco-friendly kết hợp giữa nghệ thuật mộc truyền thống và công nghệ hiện đại. Vỏ ngoài gia công từ thân tre tự nhiên phủ sáp mờ chống ẩm mốc, khắc laser tinh xảo. Ruột bình làm từ thép không gỉ SUS 304 tiêu chuẩn quốc tế giúp giữ nóng và giữ lạnh liên tục suốt 12 tiếng. Mỗi bình bán ra là một viên gạch xây dựng công trình máy lọc nước uống tinh khiết cho điểm trường tiểu học vùng sâu.",
                    ThongDiepGayQuy = "Đóng góp quỹ máy lọc nước sạch học đường cho điểm trường khó khăn tại vùng cao Lâm Đồng.",
                    PhanTramDongGop = 100,
                    SoLuongTon = 50,
                    DaBan = 28,
                    DanhMuc = "Quà lưu niệm",
                    LinkHinhAnh = "/assets/children.jpg",
                    TrangThai = true,
                    ThuTu = 3,
                    NgayTao = DateTime.Now.AddDays(-5)
                },
                new SanPhamGayQuy
                {
                    IDSanPham = 4,
                    TenSanPham = "Móc Khóa Linh Vật Thỏ Trăng Langbiang Đan Len",
                    GiaBan = 35000m,
                    GiaGoc = 45000m,
                    MoTaNgan = "Linh vật Thỏ Trăng đan len thủ công tinh xảo do chính tay các bạn sinh viên tình nguyện HUTECH thực hiện.",
                    MoTaChiTiet = "Móc chìa khóa đan len thủ công hình tượng chú thỏ ngắm trăng - biểu tượng của sự thuần khiết, niềm hy vọng và tuổi thơ ấm áp. Từng đường móc, mũi len đều được các bạn sinh viên tình nguyện tự tay đan gởi gắm tình cảm chân thành. Mua 1 móc khóa là bạn đã góp thêm 1 chiếc lồng đèn truyền thống và 2 tập vở trắng mới tinh cho các bé trong Đêm hội Trăng rằm.",
                    ThongDiepGayQuy = "Góp 1 lồng đèn truyền thống và tập vở mới cho trẻ em nghèo trong đêm hội Trăng Rằm.",
                    PhanTramDongGop = 100,
                    SoLuongTon = 120,
                    DaBan = 94,
                    DanhMuc = "Quà lưu niệm",
                    LinkHinhAnh = "/assets/lanterns.jpg",
                    TrangThai = true,
                    ThuTu = 4,
                    NgayTao = DateTime.Now.AddDays(-3)
                }
            };
        }

        /// <summary>
        /// Danh sách 2 đơn hàng mẫu gây quỹ thực tế (1 "MoiDat", 1 "DaXacNhan") phục vụ kiểm thử và phân hệ quản trị Admin.
        /// </summary>
        public static List<DonHangGayQuy> GetDefaultOrders()
        {
            var dateStamp = DateTime.Now.ToString("yyyyMMdd");
            return new List<DonHangGayQuy>
            {
                new DonHangGayQuy
                {
                    IDDonHang = 1,
                    MaDonHang = $"GQ{dateStamp}-8812",
                    HoTenNguoiMua = "Nguyễn Thị Mai Anh",
                    SoDienThoai = "0987654321",
                    Email = "maianh.nguyen@gmail.com",
                    DiaChiNhanHang = "Số 45 Lê Duẩn, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    GhiChu = "Giao giờ hành chính giúp em, em cảm ơn các bạn tình nguyện viên!",
                    TongTienHang = 258000m,
                    TienUngHoThem = 50000m,
                    PhiVanChuyen = 0m,
                    TongThanhToan = 308000m,
                    PhuongThucThanhToan = "ChuyenKhoanQR",
                    TrangThaiDonHang = "MoiDat",
                    TrangThaiThanhToan = "ChuaThanhToan",
                    NgayDatHang = DateTime.Now.AddHours(-3),
                    ChiTietDonHangs = new List<ChiTietDonHangGayQuy>
                    {
                        new ChiTietDonHangGayQuy
                        {
                            IDChiTiet = 1,
                            IDDonHang = 1,
                            IDSanPham = 1,
                            TenSanPham = "Áo Thun Tình Nguyện \"Trăng Sáng Langbiang 2026\"",
                            LinkHinhAnh = "/assets/journey.jpg",
                            DonGia = 129000m,
                            SoLuong = 2,
                            ThanhTien = 258000m,
                            ThongDiepGayQuy = "Đóng góp 50.000₫/áo vào quỹ quà tặng Trung thu cho 200 em nhỏ đồng bào K'Ho tại Langbiang.",
                            PhanTramDongGop = 100
                        }
                    }
                },
                new DonHangGayQuy
                {
                    IDDonHang = 2,
                    MaDonHang = $"GQ{dateStamp}-9431",
                    HoTenNguoiMua = "Trần Bảo Long",
                    SoDienThoai = "0912345678",
                    Email = "baolong.tran@outlook.com",
                    DiaChiNhanHang = "Ký túc xá Đại học HUTECH, Khu Công nghệ cao, TP. Thủ Đức",
                    GhiChu = "Ủng hộ các bạn chiến dịch Trăng Sáng Langbiang 2026!",
                    TongTienHang = 149000m,
                    TienUngHoThem = 21000m,
                    PhiVanChuyen = 0m,
                    TongThanhToan = 170000m,
                    PhuongThucThanhToan = "ChuyenKhoanQR",
                    TrangThaiDonHang = "DaXacNhan",
                    TrangThaiThanhToan = "DaThanhToan",
                    NgayDatHang = DateTime.Now.AddDays(-1),
                    ChiTietDonHangs = new List<ChiTietDonHangGayQuy>
                    {
                        new ChiTietDonHangGayQuy
                        {
                            IDChiTiet = 2,
                            IDDonHang = 2,
                            IDSanPham = 2,
                            TenSanPham = "Túi Tote Canvas \"Yêu Thương Gửi Về Núi Rừng\"",
                            LinkHinhAnh = "/assets/cooking.jpg",
                            DonGia = 79000m,
                            SoLuong = 1,
                            ThanhTien = 79000m,
                            ThongDiepGayQuy = "100% lợi nhuận dành cho chương trình 'Nấu ăn cho em' với 250 suất phở bò nóng hổi nơi vùng cao.",
                            PhanTramDongGop = 100
                        },
                        new ChiTietDonHangGayQuy
                        {
                            IDChiTiet = 3,
                            IDDonHang = 2,
                            IDSanPham = 4,
                            TenSanPham = "Móc Khóa Linh Vật Thỏ Trăng Langbiang Đan Len",
                            LinkHinhAnh = "/assets/lanterns.jpg",
                            DonGia = 35000m,
                            SoLuong = 2,
                            ThanhTien = 70000m,
                            ThongDiepGayQuy = "Góp 1 lồng đèn truyền thống và tập vở mới cho trẻ em nghèo trong đêm hội Trăng Rằm.",
                            PhanTramDongGop = 100
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Cấu hình mã QR thanh toán / ủng hộ mặc định ban đầu mang thương hiệu MBBank chiến dịch Trăng Sáng Langbiang 2026.
        /// </summary>
        public static CauHinhThanhToanQR GetDefaultQrConfig()
        {
            return new CauHinhThanhToanQR
            {
                IDCauHinhQR = 1,
                TenCauHinh = "Tài khoản MBBank - Cửa hàng & Quyên góp",
                MaNganHang = "MB",
                TenNganHang = "Quân Đội (MBBank)",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                CuPhapChuyenKhoan = "{MADON}",
                MauVietQR = "compact2",
                DuongDanAnhQRTuyChon = null,
                SuDungVietQR = true,
                HuongDanThanhToan = "Mở ứng dụng ngân hàng hoặc ví điện tử bất kỳ (Vietcombank, MBBank, Techcombank, MoMo...) và Quét mã để thanh toán tự động",
                GhiChuThem = "Tài khoản thụ hưởng chính thức của chiến dịch thiện nguyện Trăng Sáng Langbiang 2026.",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 1,
                NgayTao = DateTime.Now
            };
        }

        /// <summary>
        /// Bí danh cho Seed() để hỗ trợ các cách gọi khác nhau.
        /// </summary>
        public static void Initialize(ApplicationDbContext context)
        {
            Seed(context);
        }

        /// <summary>
        /// Khởi tạo dữ liệu vào cơ sở dữ liệu nếu có kết nối.
        /// Bọc toàn bộ trong try/catch để đảm bảo ứng dụng không bao giờ bị gián đoạn nếu SQL Server offline.
        /// </summary>
        public static void Seed(ApplicationDbContext context)
        {
            if (context == null) return;

            try
            {
                // Kiểm tra xem database có phản hồi không
                if (context.Database.CanConnect())
                {
                    // Đảm bảo 3 bảng Cửa hàng tồn tại trong SQL Server nếu database cũ chưa có
                    context.Database.ExecuteSqlRaw(@"
                        IF OBJECT_ID(N'[dbo].[SanPhamGayQuy]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[SanPhamGayQuy] (
                                [IDSanPham] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [TenSanPham] NVARCHAR(255) NOT NULL,
                                [GiaBan] DECIMAL(18,2) NOT NULL,
                                [GiaGoc] DECIMAL(18,2) NULL,
                                [LinkHinhAnh] NVARCHAR(1000) NULL,
                                [MoTaNgan] NVARCHAR(500) NULL,
                                [MoTaChiTiet] NVARCHAR(MAX) NULL,
                                [ThongDiepGayQuy] NVARCHAR(500) NULL,
                                [PhanTramDongGop] INT NOT NULL DEFAULT 100,
                                [SoLuongTon] INT NOT NULL DEFAULT 100,
                                [DaBan] INT NOT NULL DEFAULT 0,
                                [DanhMuc] NVARCHAR(100) NOT NULL DEFAULT N'Quà lưu niệm',
                                [TrangThai] BIT NOT NULL DEFAULT 1,
                                [ThuTu] INT NOT NULL DEFAULT 0,
                                [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
                                [NgayCapNhat] DATETIME2 NULL,
                                [IDChienDich] INT NULL
                            );
                        END;

                        IF OBJECT_ID(N'[dbo].[DonHangGayQuy]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[DonHangGayQuy] (
                                [IDDonHang] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [MaDonHang] NVARCHAR(50) NOT NULL,
                                [HoTenNguoiMua] NVARCHAR(255) NOT NULL,
                                [SoDienThoai] NVARCHAR(20) NOT NULL,
                                [Email] NVARCHAR(255) NULL,
                                [DiaChiNhanHang] NVARCHAR(500) NOT NULL,
                                [GhiChu] NVARCHAR(1000) NULL,
                                [TongTienHang] DECIMAL(18,2) NOT NULL DEFAULT 0,
                                [TienUngHoThem] DECIMAL(18,2) NOT NULL DEFAULT 0,
                                [PhiVanChuyen] DECIMAL(18,2) NOT NULL DEFAULT 0,
                                [TongThanhToan] DECIMAL(18,2) NOT NULL DEFAULT 0,
                                [PhuongThucThanhToan] NVARCHAR(50) NOT NULL DEFAULT N'ChuyenKhoanQR',
                                [TrangThaiDonHang] NVARCHAR(50) NOT NULL DEFAULT N'MoiDat',
                                [TrangThaiThanhToan] NVARCHAR(50) NOT NULL DEFAULT N'ChuaThanhToan',
                                [NgayDatHang] DATETIME2 NOT NULL DEFAULT GETDATE(),
                                [NgayCapNhat] DATETIME2 NULL
                            );
                        END;

                        IF OBJECT_ID(N'[dbo].[ChiTietDonHangGayQuy]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[ChiTietDonHangGayQuy] (
                                [IDChiTiet] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [IDDonHang] INT NOT NULL,
                                [IDSanPham] INT NULL,
                                [TenSanPham] NVARCHAR(255) NOT NULL,
                                [LinkHinhAnh] NVARCHAR(1000) NULL,
                                [DonGia] DECIMAL(18,2) NOT NULL,
                                [SoLuong] INT NOT NULL,
                                [ThanhTien] DECIMAL(18,2) NOT NULL,
                                [ThongDiepGayQuy] NVARCHAR(500) NULL,
                                [PhanTramDongGop] INT NOT NULL DEFAULT 100,
                                CONSTRAINT [FK_ChiTietDonHangGayQuy_DonHangGayQuy_IDDonHang] FOREIGN KEY ([IDDonHang]) REFERENCES [dbo].[DonHangGayQuy]([IDDonHang]) ON DELETE CASCADE,
                                CONSTRAINT [FK_ChiTietDonHangGayQuy_SanPhamGayQuy_IDSanPham] FOREIGN KEY ([IDSanPham]) REFERENCES [dbo].[SanPhamGayQuy]([IDSanPham]) ON DELETE SET NULL
                            );
                        END;

                        IF OBJECT_ID(N'[dbo].[CauHinhThanhToanQR]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[CauHinhThanhToanQR] (
                                [IDCauHinhQR] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [TenCauHinh] NVARCHAR(150) NOT NULL,
                                [MaNganHang] NVARCHAR(50) NOT NULL,
                                [TenNganHang] NVARCHAR(150) NOT NULL,
                                [SoTaiKhoan] NVARCHAR(50) NOT NULL,
                                [TenChuTaiKhoan] NVARCHAR(150) NOT NULL,
                                [CuPhapChuyenKhoan] NVARCHAR(255) NULL,
                                [MauVietQR] NVARCHAR(50) NOT NULL DEFAULT N'compact2',
                                [DuongDanAnhQRTuyChon] NVARCHAR(1000) NULL,
                                [SuDungVietQR] BIT NOT NULL DEFAULT 1,
                                [HuongDanThanhToan] NVARCHAR(MAX) NULL,
                                [GhiChuThem] NVARCHAR(MAX) NULL,
                                [ApDungChoCuaHang] BIT NOT NULL DEFAULT 1,
                                [ApDungChoQuyenGop] BIT NOT NULL DEFAULT 1,
                                [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
                                [ThuTu] INT NOT NULL DEFAULT 1,
                                [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
                                [NgayCapNhat] DATETIME2 NULL
                            );
                        END;

                        IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[TaiKhoanAdmin] (
                                [IDTaiKhoan] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [TenDangNhap] NVARCHAR(100) NOT NULL UNIQUE,
                                [MatKhauHash] NVARCHAR(500) NOT NULL,
                                [MuoiBaoMat] NVARCHAR(100) NOT NULL,
                                [HoTen] NVARCHAR(150) NULL,
                                [Email] NVARCHAR(150) NULL,
                                [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
                                [PhanQuyen] NVARCHAR(MAX) NOT NULL DEFAULT N'ALL',
                                [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
                                [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
                                [LanDangNhapCuoi] DATETIME2 NULL
                            );
                        END;

                        IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'PhanQuyen') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(MAX) NOT NULL DEFAULT N'ALL';
                        END;
                    ");

                    if (!context.SanPhamGayQuys.Any())
                    {
                        var defaultItems = GetDefaultProducts();
                        foreach (var item in defaultItems)
                        {
                            // Reset ID để SQL Server tự sinh identity nếu có
                            item.IDSanPham = 0;
                            context.SanPhamGayQuys.Add(item);
                        }
                        context.SaveChanges();
                    }

                    if (!context.DonHangGayQuys.Any())
                    {
                        var defaultOrders = GetDefaultOrders();
                        foreach (var ord in defaultOrders)
                        {
                            ord.IDDonHang = 0;
                            if (ord.ChiTietDonHangs != null)
                            {
                                foreach (var d in ord.ChiTietDonHangs)
                                {
                                    d.IDChiTiet = 0;
                                    d.IDDonHang = 0;
                                }
                            }
                            context.DonHangGayQuys.Add(ord);
                        }
                        context.SaveChanges();
                    }

                    if (!context.CauHinhThanhToanQRs.Any())
                    {
                        var defaultQr = GetDefaultQrConfig();
                        defaultQr.IDCauHinhQR = 0;
                        context.CauHinhThanhToanQRs.Add(defaultQr);
                        context.SaveChanges();
                    }

                    SeedAdmin(context);
                }
            }
            catch
            {
                // Bắt toàn bộ lỗi (SQL Server không chạy, chưa migrate bảng, timeout...)
                // Chuyển sang sử dụng In-Memory Store
            }
        }

        #region In-Memory Store Methods (Phục vụ Demo & Offline Fallback)

        public static void ResetInMemoryStore()
        {
            lock (_syncLock)
            {
                _inMemoryProducts.Clear();
                _inMemoryProducts.AddRange(GetDefaultProducts());
                _inMemoryOrders.Clear();
                _nextProductId = 5;
                _nextOrderId = 1;
                SeedSampleOrders();
                _sampleOrdersSeeded = true;

                _inMemoryQrConfigs.Clear();
                _inMemoryQrConfigs.Add(GetDefaultQrConfig());
                _nextQrConfigId = 2;

                _inMemoryAdmins.Clear();
                _inMemoryAdmins.AddRange(GetDefaultAdmins());
                _nextAdminId = 2;
            }
        }

        public static void SeedSampleOrders()
        {
            lock (_syncLock)
            {
                if (!_sampleOrdersSeeded || _inMemoryOrders.Count == 0)
                {
                    var defaultOrders = GetDefaultOrders();
                    int seedId = 1001;
                    foreach (var ord in defaultOrders)
                    {
                        ord.IDDonHang = seedId++;
                        if (ord.ChiTietDonHangs != null)
                        {
                            foreach (var d in ord.ChiTietDonHangs)
                            {
                                d.IDDonHang = ord.IDDonHang;
                            }
                        }
                        _inMemoryOrders.Add(CloneOrder(ord));
                    }
                    _sampleOrdersSeeded = true;
                }
            }
        }

        public static List<SanPhamGayQuy> GetInMemoryProducts()
        {
            lock (_syncLock)
            {
                // Trả về bản sao danh sách để đảm bảo an toàn truy cập
                return _inMemoryProducts.Select(p => CloneProduct(p)).ToList();
            }
        }

        public static SanPhamGayQuy? GetInMemoryProductById(int id)
        {
            lock (_syncLock)
            {
                var p = _inMemoryProducts.FirstOrDefault(x => x.IDSanPham == id);
                return p == null ? null : CloneProduct(p);
            }
        }

        public static SanPhamGayQuy AddInMemoryProduct(SanPhamGayQuy item)
        {
            lock (_syncLock)
            {
                item.IDSanPham = _nextProductId++;
                item.NgayTao = DateTime.Now;
                _inMemoryProducts.Add(CloneProduct(item));
                return item;
            }
        }

        public static bool UpdateInMemoryProduct(SanPhamGayQuy item)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryProducts.FirstOrDefault(x => x.IDSanPham == item.IDSanPham);
                if (existing == null) return false;

                existing.TenSanPham = item.TenSanPham;
                existing.GiaBan = item.GiaBan;
                existing.GiaGoc = item.GiaGoc;
                existing.SoLuongTon = item.SoLuongTon;
                existing.DaBan = item.DaBan;
                existing.DanhMuc = item.DanhMuc;
                existing.MoTaNgan = item.MoTaNgan;
                existing.MoTaChiTiet = item.MoTaChiTiet;
                existing.ThongDiepGayQuy = item.ThongDiepGayQuy;
                existing.PhanTramDongGop = item.PhanTramDongGop;
                existing.TrangThai = item.TrangThai;
                existing.ThuTu = item.ThuTu;
                if (!string.IsNullOrEmpty(item.LinkHinhAnh))
                {
                    existing.LinkHinhAnh = item.LinkHinhAnh;
                }
                return true;
            }
        }

        public static bool DeleteInMemoryProduct(int id)
        {
            lock (_syncLock)
            {
                var item = _inMemoryProducts.FirstOrDefault(x => x.IDSanPham == id);
                if (item == null) return false;
                _inMemoryProducts.Remove(item);
                return true;
            }
        }

        public static DonHangGayQuy AddInMemoryOrder(DonHangGayQuy order)
        {
            lock (_syncLock)
            {
                while (_inMemoryOrders.Any(o => o.IDDonHang == _nextOrderId))
                {
                    _nextOrderId++;
                }
                order.IDDonHang = _nextOrderId++;
                if (string.IsNullOrWhiteSpace(order.MaDonHang))
                {
                    order.MaDonHang = $"GQ2026-{DateTime.Now:yyyyMMdd}-{order.IDDonHang:D4}";
                }
                order.NgayDatHang = DateTime.Now;

                var clonedOrder = CloneOrder(order);
                _inMemoryOrders.Add(clonedOrder);

                // Cập nhật số lượng tồn kho và đã bán tương ứng
                if (clonedOrder.ChiTietDonHangs != null)
                {
                    foreach (var detail in clonedOrder.ChiTietDonHangs)
                    {
                        if (detail.IDSanPham.HasValue)
                        {
                            var prod = _inMemoryProducts.FirstOrDefault(p => p.IDSanPham == detail.IDSanPham.Value);
                            if (prod != null)
                            {
                                prod.SoLuongTon = Math.Max(0, prod.SoLuongTon - detail.SoLuong);
                                prod.DaBan += detail.SoLuong;
                            }
                        }
                    }
                }

                return CloneOrder(clonedOrder);
            }
        }

        public static List<DonHangGayQuy> GetInMemoryOrders()
        {
            lock (_syncLock)
            {
                if (!_sampleOrdersSeeded && _inMemoryOrders.Count == 0)
                {
                    SeedSampleOrders();
                }

                return _inMemoryOrders
                    .OrderByDescending(o => o.NgayDatHang)
                    .Select(o => CloneOrder(o))
                    .ToList();
            }
        }

        public static DonHangGayQuy? GetInMemoryOrderById(int id)
        {
            lock (_syncLock)
            {
                if (!_sampleOrdersSeeded && _inMemoryOrders.Count == 0)
                {
                    SeedSampleOrders();
                }
                var order = _inMemoryOrders.FirstOrDefault(o => o.IDDonHang == id);
                return order == null ? null : CloneOrder(order);
            }
        }

        public static DonHangGayQuy? GetInMemoryOrderByCode(string code)
        {
            lock (_syncLock)
            {
                if (!_sampleOrdersSeeded && _inMemoryOrders.Count == 0)
                {
                    SeedSampleOrders();
                }
                var order = _inMemoryOrders.FirstOrDefault(o => string.Equals(o.MaDonHang, code, StringComparison.OrdinalIgnoreCase));
                return order == null ? null : CloneOrder(order);
            }
        }

        public static bool UpdateInMemoryOrderStatus(int id, string trangThaiDonHang, string? trangThaiThanhToan = null)
        {
            lock (_syncLock)
            {
                var order = _inMemoryOrders.FirstOrDefault(o => o.IDDonHang == id);
                if (order == null) return false;

                bool wasCancelled = string.Equals(order.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase);
                bool isCancelled = string.Equals(trangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase);

                // Terminal state: không thể kích hoạt lại đơn hàng đã hủy
                if (wasCancelled && !isCancelled)
                {
                    return false;
                }

                bool wasNotCancelled = !wasCancelled;

                order.TrangThaiDonHang = trangThaiDonHang;
                if (!string.IsNullOrWhiteSpace(trangThaiThanhToan))
                {
                    order.TrangThaiThanhToan = trangThaiThanhToan;
                }
                order.NgayCapNhat = DateTime.Now;

                // Restock logic: If trangThaiDonHang == "DaHuy", restore the inventory of each product in ChiTietDonHangs
                if (wasNotCancelled && isCancelled && order.ChiTietDonHangs != null)
                {
                    foreach (var detail in order.ChiTietDonHangs)
                    {
                        if (detail.IDSanPham.HasValue)
                        {
                            var prod = _inMemoryProducts.FirstOrDefault(p => p.IDSanPham == detail.IDSanPham.Value);
                            if (prod != null)
                            {
                                prod.SoLuongTon += detail.SoLuong;
                                prod.DaBan = Math.Max(0, prod.DaBan - detail.SoLuong);
                            }
                        }
                    }
                }

                return true;
            }
        }

        public static bool DeleteInMemoryOrder(int id)
        {
            lock (_syncLock)
            {
                var order = _inMemoryOrders.FirstOrDefault(o => o.IDDonHang == id);
                if (order == null) return false;

                // Hoàn lại số lượng tồn kho nếu đơn hàng chưa hủy trước khi xóa
                if (!string.Equals(order.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase) && order.ChiTietDonHangs != null)
                {
                    foreach (var detail in order.ChiTietDonHangs)
                    {
                        if (detail.IDSanPham.HasValue)
                        {
                            var prod = _inMemoryProducts.FirstOrDefault(p => p.IDSanPham == detail.IDSanPham.Value);
                            if (prod != null)
                            {
                                prod.SoLuongTon += detail.SoLuong;
                                prod.DaBan = Math.Max(0, prod.DaBan - detail.SoLuong);
                            }
                        }
                    }
                }

                _inMemoryOrders.Remove(order);
                return true;
            }
        }

        private static DonHangGayQuy CloneOrder(DonHangGayQuy o)
        {
            var clone = new DonHangGayQuy
            {
                IDDonHang = o.IDDonHang,
                MaDonHang = o.MaDonHang,
                HoTenNguoiMua = o.HoTenNguoiMua,
                SoDienThoai = o.SoDienThoai,
                Email = o.Email,
                DiaChiNhanHang = o.DiaChiNhanHang,
                GhiChu = o.GhiChu,
                TongTienHang = o.TongTienHang,
                TienUngHoThem = o.TienUngHoThem,
                PhiVanChuyen = o.PhiVanChuyen,
                TongThanhToan = o.TongThanhToan,
                PhuongThucThanhToan = o.PhuongThucThanhToan,
                TrangThaiDonHang = o.TrangThaiDonHang,
                TrangThaiThanhToan = o.TrangThaiThanhToan,
                NgayDatHang = o.NgayDatHang,
                NgayCapNhat = o.NgayCapNhat,
                ChiTietDonHangs = new List<ChiTietDonHangGayQuy>()
            };

            if (o.ChiTietDonHangs != null)
            {
                foreach (var d in o.ChiTietDonHangs)
                {
                    var detailClone = new ChiTietDonHangGayQuy
                    {
                        IDChiTiet = d.IDChiTiet,
                        IDDonHang = d.IDDonHang,
                        IDSanPham = d.IDSanPham,
                        TenSanPham = d.TenSanPham,
                        LinkHinhAnh = d.LinkHinhAnh,
                        DonGia = d.DonGia,
                        SoLuong = d.SoLuong,
                        ThanhTien = d.ThanhTien,
                        ThongDiepGayQuy = d.ThongDiepGayQuy,
                        PhanTramDongGop = d.PhanTramDongGop
                    };

                    if (d.IDSanPham.HasValue)
                    {
                        var prod = _inMemoryProducts.FirstOrDefault(p => p.IDSanPham == d.IDSanPham.Value);
                        if (prod != null)
                        {
                            detailClone.SanPham = CloneProduct(prod);
                        }
                    }

                    clone.ChiTietDonHangs.Add(detailClone);
                }
            }

            return clone;
        }

        private static SanPhamGayQuy CloneProduct(SanPhamGayQuy p)
        {
            return new SanPhamGayQuy
            {
                IDSanPham = p.IDSanPham,
                TenSanPham = p.TenSanPham,
                GiaBan = p.GiaBan,
                GiaGoc = p.GiaGoc,
                LinkHinhAnh = p.LinkHinhAnh,
                MoTaNgan = p.MoTaNgan,
                MoTaChiTiet = p.MoTaChiTiet,
                ThongDiepGayQuy = p.ThongDiepGayQuy,
                PhanTramDongGop = p.PhanTramDongGop,
                SoLuongTon = p.SoLuongTon,
                DaBan = p.DaBan,
                DanhMuc = p.DanhMuc,
                TrangThai = p.TrangThai,
                ThuTu = p.ThuTu,
                NgayTao = p.NgayTao,
                IDChienDich = p.IDChienDich
            };
        }

        #region In-Memory QR Config Store Methods (Phục vụ Demo & Offline Fallback)

        public static List<CauHinhThanhToanQR> GetInMemoryQrConfigs()
        {
            lock (_syncLock)
            {
                if (_inMemoryQrConfigs.Count == 0)
                {
                    _inMemoryQrConfigs.Add(GetDefaultQrConfig());
                }
                return _inMemoryQrConfigs
                    .OrderBy(q => q.ThuTu)
                    .ThenBy(q => q.IDCauHinhQR)
                    .Select(q => CloneQrConfig(q))
                    .ToList();
            }
        }

        public static CauHinhThanhToanQR? GetInMemoryQrConfigById(int id)
        {
            lock (_syncLock)
            {
                if (_inMemoryQrConfigs.Count == 0)
                {
                    _inMemoryQrConfigs.Add(GetDefaultQrConfig());
                }
                var q = _inMemoryQrConfigs.FirstOrDefault(x => x.IDCauHinhQR == id);
                return q == null ? null : CloneQrConfig(q);
            }
        }

        public static CauHinhThanhToanQR? GetActiveStoreQrConfig()
        {
            lock (_syncLock)
            {
                if (_inMemoryQrConfigs.Count == 0)
                {
                    _inMemoryQrConfigs.Add(GetDefaultQrConfig());
                }
                var active = _inMemoryQrConfigs
                    .Where(q => q.ApDungChoCuaHang && q.TrangThaiHoatDong)
                    .OrderBy(q => q.ThuTu)
                    .ThenBy(q => q.IDCauHinhQR)
                    .FirstOrDefault();

                return active != null ? CloneQrConfig(active) : null;
            }
        }

        public static List<CauHinhThanhToanQR> GetActiveDonationQrConfigs()
        {
            lock (_syncLock)
            {
                if (_inMemoryQrConfigs.Count == 0)
                {
                    _inMemoryQrConfigs.Add(GetDefaultQrConfig());
                }
                return _inMemoryQrConfigs
                    .Where(q => q.ApDungChoQuyenGop && q.TrangThaiHoatDong)
                    .OrderBy(q => q.ThuTu)
                    .ThenBy(q => q.IDCauHinhQR)
                    .Select(q => CloneQrConfig(q))
                    .ToList();
            }
        }

        public static CauHinhThanhToanQR AddInMemoryQrConfig(CauHinhThanhToanQR item)
        {
            lock (_syncLock)
            {
                while (_inMemoryQrConfigs.Any(q => q.IDCauHinhQR == _nextQrConfigId))
                {
                    _nextQrConfigId++;
                }
                item.IDCauHinhQR = _nextQrConfigId++;
                item.NgayTao = DateTime.Now;
                var clone = CloneQrConfig(item);
                _inMemoryQrConfigs.Add(clone);
                return CloneQrConfig(clone);
            }
        }

        public static bool UpdateInMemoryQrConfig(CauHinhThanhToanQR item)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryQrConfigs.FirstOrDefault(x => x.IDCauHinhQR == item.IDCauHinhQR);
                if (existing == null) return false;

                existing.TenCauHinh = item.TenCauHinh;
                existing.MaNganHang = item.MaNganHang;
                existing.TenNganHang = item.TenNganHang;
                existing.SoTaiKhoan = item.SoTaiKhoan;
                existing.TenChuTaiKhoan = item.TenChuTaiKhoan;
                existing.CuPhapChuyenKhoan = item.CuPhapChuyenKhoan;
                existing.MauVietQR = item.MauVietQR;
                existing.SuDungVietQR = item.SuDungVietQR;
                existing.HuongDanThanhToan = item.HuongDanThanhToan;
                existing.GhiChuThem = item.GhiChuThem;
                existing.ApDungChoCuaHang = item.ApDungChoCuaHang;
                existing.ApDungChoQuyenGop = item.ApDungChoQuyenGop;
                existing.TrangThaiHoatDong = item.TrangThaiHoatDong;
                existing.ThuTu = item.ThuTu;
                existing.NgayCapNhat = DateTime.Now;

                if (!string.IsNullOrEmpty(item.DuongDanAnhQRTuyChon))
                {
                    existing.DuongDanAnhQRTuyChon = item.DuongDanAnhQRTuyChon;
                }

                return true;
            }
        }

        public static bool DeleteInMemoryQrConfig(int id)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryQrConfigs.FirstOrDefault(x => x.IDCauHinhQR == id);
                if (existing == null) return false;
                _inMemoryQrConfigs.Remove(existing);
                return true;
            }
        }

        public static bool ToggleActiveInMemoryQrConfig(int id)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryQrConfigs.FirstOrDefault(x => x.IDCauHinhQR == id);
                if (existing == null) return false;
                existing.TrangThaiHoatDong = !existing.TrangThaiHoatDong;
                existing.NgayCapNhat = DateTime.Now;
                return true;
            }
        }

        private static CauHinhThanhToanQR CloneQrConfig(CauHinhThanhToanQR q)
        {
            return new CauHinhThanhToanQR
            {
                IDCauHinhQR = q.IDCauHinhQR,
                TenCauHinh = q.TenCauHinh,
                MaNganHang = q.MaNganHang,
                TenNganHang = q.TenNganHang,
                SoTaiKhoan = q.SoTaiKhoan,
                TenChuTaiKhoan = q.TenChuTaiKhoan,
                CuPhapChuyenKhoan = q.CuPhapChuyenKhoan,
                MauVietQR = q.MauVietQR,
                DuongDanAnhQRTuyChon = q.DuongDanAnhQRTuyChon,
                SuDungVietQR = q.SuDungVietQR,
                HuongDanThanhToan = q.HuongDanThanhToan,
                GhiChuThem = q.GhiChuThem,
                ApDungChoCuaHang = q.ApDungChoCuaHang,
                ApDungChoQuyenGop = q.ApDungChoQuyenGop,
                TrangThaiHoatDong = q.TrangThaiHoatDong,
                ThuTu = q.ThuTu,
                NgayTao = q.NgayTao,
                NgayCapNhat = q.NgayCapNhat
            };
        }

        #endregion

        #region Quản trị viên & Phân quyền (Admin Authentication & Authorization)

        /// <summary>
        /// Khởi tạo tài khoản quản trị viên mặc định vào SQL Server nếu chưa tồn tại.
        /// Nếu database offline hoặc không truyền context, đồng bộ đảm bảo bộ nhớ in-memory có sẵn admin.
        /// </summary>
        public static void SeedAdmin(ApplicationDbContext? context = null)
        {
            if (context != null)
            {
                try
                {
                    if (context.Database.CanConnect())
                    {
                        // Đảm bảo bảng TaiKhoanAdmin tồn tại (Idempotent DDL)
                        context.Database.ExecuteSqlRaw(@"
                            IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL
                            BEGIN
                                CREATE TABLE [dbo].[TaiKhoanAdmin] (
                                    [IDTaiKhoan] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    [TenDangNhap] NVARCHAR(100) NOT NULL UNIQUE,
                                    [MatKhauHash] NVARCHAR(500) NOT NULL,
                                    [MuoiBaoMat] NVARCHAR(100) NOT NULL,
                                    [HoTen] NVARCHAR(150) NULL,
                                    [Email] NVARCHAR(150) NULL,
                                    [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
                                    [PhanQuyen] NVARCHAR(MAX) NOT NULL DEFAULT N'ALL',
                                    [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
                                    [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
                                    [LanDangNhapCuoi] DATETIME2 NULL
                                );
                            END;

                            IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'PhanQuyen') IS NULL
                            BEGIN
                                ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(MAX) NOT NULL DEFAULT N'ALL';
                            END;
                        ");

                        // Kiểm tra tài khoản admin mặc định
                        var existingDefaultAdmin = context.TaiKhoanAdmins.FirstOrDefault(a => a.TenDangNhap == "admin");
                        if (existingDefaultAdmin == null)
                        {
                            var defaultAdmins = GetDefaultAdmins();
                            foreach (var adm in defaultAdmins)
                            {
                                var dbAdmin = CloneAdmin(adm);
                                dbAdmin.IDTaiKhoan = 0; // Để SQL Server tự sinh ID Identity
                                context.TaiKhoanAdmins.Add(dbAdmin);
                            }
                            context.SaveChanges();
                        }
                        else if (string.IsNullOrWhiteSpace(existingDefaultAdmin.PhanQuyen))
                        {
                            existingDefaultAdmin.PhanQuyen = QuyenAdmin.All;
                            context.SaveChanges();
                        }
                    }
                }
                catch
                {
                    // SQL Server offline: bắt lỗi để không làm gián đoạn ứng dụng
                }
            }

            // Đảm bảo bộ nhớ In-Memory luôn có tài khoản admin
            lock (_syncLock)
            {
                if (_inMemoryAdmins.Count == 0)
                {
                    _inMemoryAdmins.AddRange(GetDefaultAdmins());
                }
            }
        }

        /// <summary>
        /// Lấy thông tin tài khoản admin theo tên đăng nhập (không phân biệt chữ hoa thường).
        /// Hỗ trợ dual-mode: truy vấn SQL Server trước, nếu offline hoặc không có context thì truy vấn từ In-Memory store.
        /// Trả về bản sao đối tượng (clone) để tránh bị đột biến dữ liệu ngoài ý muốn.
        /// </summary>
        public static TaiKhoanAdmin? GetAdminByUsername(string username, ApplicationDbContext? context = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            string normalized = username.Trim();

            if (context != null)
            {
                try
                {
                    if (context.Database.CanConnect())
                    {
                        var dbAdmin = context.TaiKhoanAdmins
                            .AsNoTracking()
                            .FirstOrDefault(a => a.TenDangNhap.ToLower() == normalized.ToLower());

                        if (dbAdmin != null)
                        {
                            return CloneAdmin(dbAdmin);
                        }
                    }
                }
                catch
                {
                    // Database gặp sự cố: chuyển xuống In-Memory fallback
                }
            }

            lock (_syncLock)
            {
                if (_inMemoryAdmins.Count == 0)
                {
                    _inMemoryAdmins.AddRange(GetDefaultAdmins());
                }

                var inMemAdmin = _inMemoryAdmins.FirstOrDefault(a =>
                    string.Equals(a.TenDangNhap, normalized, StringComparison.OrdinalIgnoreCase));

                return inMemAdmin != null ? CloneAdmin(inMemAdmin) : null;
            }
        }

        /// <summary>
        /// Xác thực thông tin đăng nhập admin (tên đăng nhập và mật khẩu).
        /// Kiểm tra mật khẩu bằng thuật toán PBKDF2 HMAC-SHA256 chống timing attack.
        /// Nếu thông tin hợp lệ, tự động cập nhật thời điểm đăng nhập cuối (LanDangNhapCuoi).
        /// </summary>
        public static bool ValidateAdminCredentials(string username, string password, ApplicationDbContext? context = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var admin = GetAdminByUsername(username, context);
            if (admin == null || !admin.TrangThaiHoatDong)
            {
                return false;
            }

            bool isValid = BaoMatMatKhau.KiemTraMatKhau(password, admin.MatKhauHash, admin.MuoiBaoMat);
            if (!isValid)
            {
                return false;
            }

            // Cập nhật LanDangNhapCuoi khi đăng nhập thành công
            var now = DateTime.Now;

            // Cập nhật trong In-Memory Store
            lock (_syncLock)
            {
                var inMem = _inMemoryAdmins.FirstOrDefault(a =>
                    string.Equals(a.TenDangNhap, username.Trim(), StringComparison.OrdinalIgnoreCase));
                if (inMem != null)
                {
                    inMem.LanDangNhapCuoi = now;
                }
            }

            // Cập nhật trong CSDL nếu có context
            if (context != null)
            {
                try
                {
                    if (context.Database.CanConnect())
                    {
                        var dbAdmin = context.TaiKhoanAdmins.FirstOrDefault(a =>
                            a.TenDangNhap.ToLower() == username.Trim().ToLower());
                        if (dbAdmin != null)
                        {
                            dbAdmin.LanDangNhapCuoi = now;
                            context.SaveChanges();
                        }
                    }
                }
                catch
                {
                    // Bỏ qua lỗi ghi nhật ký đăng nhập nếu CSDL tạm thời mất kết nối
                }
            }

            return true;
        }

        #region In-Memory Admin Store Methods (Phục vụ Demo & Offline Fallback)

        public static List<TaiKhoanAdmin> GetInMemoryAdmins()
        {
            lock (_syncLock)
            {
                if (_inMemoryAdmins.Count == 0)
                {
                    _inMemoryAdmins.AddRange(GetDefaultAdmins());
                }
                return _inMemoryAdmins.Select(a => CloneAdmin(a)).ToList();
            }
        }

        public static TaiKhoanAdmin? GetInMemoryAdminById(int id)
        {
            lock (_syncLock)
            {
                if (_inMemoryAdmins.Count == 0)
                {
                    _inMemoryAdmins.AddRange(GetDefaultAdmins());
                }
                var a = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == id);
                return a == null ? null : CloneAdmin(a);
            }
        }

        public static TaiKhoanAdmin? GetInMemoryAdminByUsername(string username)
        {
            return GetAdminByUsername(username);
        }

        public static TaiKhoanAdmin AddInMemoryAdmin(TaiKhoanAdmin item)
        {
            lock (_syncLock)
            {
                while (_inMemoryAdmins.Any(a => a.IDTaiKhoan == _nextAdminId))
                {
                    _nextAdminId++;
                }
                item.IDTaiKhoan = _nextAdminId++;
                item.NgayTao = DateTime.Now;
                var clone = CloneAdmin(item);
                _inMemoryAdmins.Add(clone);
                return CloneAdmin(clone);
            }
        }

        public static bool UpdateInMemoryAdmin(TaiKhoanAdmin item)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == item.IDTaiKhoan);
                if (existing == null) return false;

                existing.TenDangNhap = item.TenDangNhap;
                if (!string.IsNullOrEmpty(item.MatKhauHash))
                {
                    existing.MatKhauHash = item.MatKhauHash;
                }
                if (!string.IsNullOrEmpty(item.MuoiBaoMat))
                {
                    existing.MuoiBaoMat = item.MuoiBaoMat;
                }
                existing.HoTen = item.HoTen;
                existing.Email = item.Email;
                existing.VaiTro = item.VaiTro;
                if (!string.IsNullOrWhiteSpace(item.PhanQuyen))
                {
                    existing.PhanQuyen = item.PhanQuyen;
                }
                existing.TrangThaiHoatDong = item.TrangThaiHoatDong;
                existing.LanDangNhapCuoi = item.LanDangNhapCuoi;
                return true;
            }
        }

        public static bool ToggleActiveInMemoryAdmin(int id)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == id);
                if (existing == null) return false;
                if (string.Equals(existing.TenDangNhap, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Không cho phép vô hiệu hóa tài khoản quản trị gốc
                    existing.TrangThaiHoatDong = true;
                    return false;
                }
                existing.TrangThaiHoatDong = !existing.TrangThaiHoatDong;
                return true;
            }
        }

        public static bool DeleteInMemoryAdmin(int id)
        {
            lock (_syncLock)
            {
                var existing = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == id);
                if (existing == null) return false;
                _inMemoryAdmins.Remove(existing);
                return true;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
