-- =====================================================
-- FILE SQL: QUẢN LÝ BÁN HOA
-- Phiên bản: 2.0 - Đã được sắp xếp và chuẩn hóa
-- Ngày cập nhật: 2025-12-17
-- =====================================================

-- Kiểm tra và xóa database nếu đã tồn tại
USE master
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QL_BanHoa')
BEGIN
    -- Ngắt tất cả kết nối đến database
    ALTER DATABASE QL_BanHoa SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_BanHoa;
END
GO

-- Tạo database mới
CREATE DATABASE QL_BanHoa;
GO

-- Sử dụng database vừa tạo
USE QL_BanHoa;
GO

-- =====================================================
-- PHẦN 1: TẠO CÁC BẢNG (Theo thứ tự phụ thuộc)
-- =====================================================

-- 1.1 Bảng DanhMuc (Danh mục sản phẩm - có quan hệ đệ quy)
CREATE TABLE DanhMuc
(
    MADM INT IDENTITY(1,1) PRIMARY KEY,
    TENDM NVARCHAR(100) NOT NULL,
    MADM_CHA INT NULL,
    TRANGTHAI NVARCHAR(20) DEFAULT N'Hiển thị',
    FOREIGN KEY (MADM_CHA) REFERENCES DanhMuc(MADM)
);
GO

-- 1.2 Bảng KhuyenMai (Chương trình khuyến mãi)
CREATE TABLE KHUYENMAI (
    MAKM INT IDENTITY(1,1) PRIMARY KEY,
    TENKM NVARCHAR(100),
    MOTA NVARCHAR(255),
    PHANTRAMGIAM INT CHECK (PHANTRAMGIAM BETWEEN 1 AND 100), 
    NGAYBATDAU DATETIME,
    NGAYKETTHUC DATETIME,            
    TRANGTHAI NVARCHAR(30) DEFAULT N'Đang áp dụng'
);
GO

-- 1.3 Bảng SanPham (Sản phẩm - MASP bắt đầu từ 1)
CREATE TABLE SanPham (
    MASP INT IDENTITY(1,1) PRIMARY KEY,
    TENSP NVARCHAR(100) NOT NULL,
    MOTA NVARCHAR(MAX),
    GIA DECIMAL(18,2) DEFAULT 0,
    SOLUONGTON INT DEFAULT 0,
    DONVITINH NVARCHAR(20) DEFAULT N'Cái',
    URL_ANH VARCHAR(255),
    NGAYTHEM DATETIME DEFAULT GETDATE(),
    TRANGTHAI NVARCHAR(30) DEFAULT N'Còn hàng',
    LUOTMUA INT DEFAULT 0,
    MADM INT,
    MAKM INT NULL,
    FOREIGN KEY (MADM) REFERENCES DanhMuc(MADM),
    FOREIGN KEY (MAKM) REFERENCES KhuyenMai(MAKM)
);
GO

-- 1.4 Bảng NguoiDung (Người dùng hệ thống)
CREATE TABLE NguoiDung (
    MAND INT IDENTITY(1, 1) PRIMARY KEY,
    TENDANGNHAP VARCHAR(50) UNIQUE,
    MATKHAU NVARCHAR(255),
    HOTEN NVARCHAR(50),
    SODIENTHOAI VARCHAR(15),
    EMAIL VARCHAR(50) UNIQUE,
    DIACHI NVARCHAR(200),
    VAITRO NVARCHAR(20) NOT NULL DEFAULT N'Khách hàng'
);
GO

-- 1.5 Bảng DonHang (Đơn hàng - MADH bắt đầu từ 1)
CREATE TABLE DonHang
(
    MADH INT IDENTITY(1,1) PRIMARY KEY,
    NGAYDAT DATETIME DEFAULT GETDATE(),
    NGAYGIAOHANGDK DATETIME NULL,
    TONGTIEN DECIMAL(18,0),
    PHIVANCHUYEN DECIMAL(18,0) DEFAULT 0,
    PHUONGTHUCTHANHTOAN NVARCHAR(50),
    DIACHIGIAOHANG NVARCHAR(255),
    TENNGUOINHAN NVARCHAR(100),
    SDTNGUOINHAN VARCHAR(15),
    TRANG_THAI NVARCHAR(50) DEFAULT N'Chờ xử lý',
    GHI_CHU NVARCHAR(255),
    MAND INT,
    MaKM INT,
    FOREIGN KEY (MAND) REFERENCES NGUOIDUNG(MAND),
    FOREIGN KEY (MAKM) REFERENCES KHUYENMAI(MAKM)
);
GO

-- 1.6 Bảng ChiTietDonHang (Chi tiết đơn hàng)
CREATE TABLE ChiTietDonHang (
    MADH INT,
    MASP INT,
    SOLUONG INT CHECK (SOLUONG > 0),
    THANHTIEN DECIMAL(18,2),
    CONSTRAINT PK_CHITIETDH PRIMARY KEY (MADH, MASP),
    FOREIGN KEY (MADH) REFERENCES DonHang(MADH),
    FOREIGN KEY (MASP) REFERENCES SanPham(MASP)
);
GO

-- 1.7 Bảng GioHang (Giỏ hàng - đã gộp các thuộc tính từ ALTER TABLE)
CREATE TABLE GioHang (
    MAGH INT IDENTITY(1,1) PRIMARY KEY,
    MAND INT,
    MASP INT,
    SOLUONG INT DEFAULT 1 CHECK (SOLUONG > 0),
    NGAYTHEM DATETIME DEFAULT GETDATE(),
    -- Các thuộc tính bổ sung (đã gộp từ ALTER TABLE)
    Anh VARCHAR(255),
    Gia DECIMAL(18,2),
    THANHTIEN DECIMAL(18,2),
    TenSP NVARCHAR(100),
    FOREIGN KEY (MAND) REFERENCES NguoiDung(MAND),
    FOREIGN KEY (MASP) REFERENCES SanPham(MASP)
);
GO

-- 1.8 Bảng DanhGia (Đánh giá sản phẩm)
CREATE TABLE DanhGia (
    MADG INT IDENTITY(1,1) PRIMARY KEY,
    MAND INT,                        
    MASP INT,                        
    SOSAO INT CHECK (SOSAO BETWEEN 1 AND 5), 
    NOIDUNG NVARCHAR(500),           
    NGAYDANHGIA DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MAND) REFERENCES NguoiDung(MAND),
    FOREIGN KEY (MASP) REFERENCES SanPham(MASP),
    CONSTRAINT UQ_DanhGia UNIQUE (MAND, MASP)
);
GO

-- 1.9 Bảng PhanLoaiSanPham (Phân loại hiển thị trên trang chủ)
CREATE TABLE PhanLoaiSanPham (
    MAPHANLOAI INT IDENTITY(1,1) PRIMARY KEY,
    TENPHANLOAI NVARCHAR(100) NOT NULL,
    MOTA NVARCHAR(255),
    URL_ANH_BANNER VARCHAR(255),
    TRANGTHAI NVARCHAR(20) DEFAULT N'Hiển thị',
    THUTU INT DEFAULT 0
);
GO

-- 1.10 Bảng SanPham_PhanLoai (Liên kết sản phẩm với phân loại trang chủ)
CREATE TABLE SanPham_PhanLoai (
    MASP INT,
    MAPHANLOAI INT,
    THUTUHIENTHI INT DEFAULT 0,
    NGAYTHEM DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_SanPhamPhanLoai PRIMARY KEY (MASP, MAPHANLOAI),
    FOREIGN KEY (MASP) REFERENCES SanPham(MASP),
    FOREIGN KEY (MAPHANLOAI) REFERENCES PhanLoaiSanPham(MAPHANLOAI)
);
GO

-- 1.11 Bảng LienHe (Liên hệ từ khách hàng)
CREATE TABLE LienHe (
    MALH INT IDENTITY(1,1) PRIMARY KEY,
    HOTEN NVARCHAR(100),
    EMAIL VARCHAR(100),
    SDT VARCHAR(20),
    NOIDUNG NVARCHAR(MAX),
    NGAYGUI DATETIME DEFAULT GETDATE(),
    TRANGTHAI NVARCHAR(50) DEFAULT N'Chưa xem'
);
GO

-- =====================================================
-- PHẦN 2: CHÈN DỮ LIỆU MẪU
-- =====================================================

-- 2.1 Chèn dữ liệu PhanLoaiSanPham (Phân loại hiển thị trang chủ)
INSERT INTO PhanLoaiSanPham (TENPHANLOAI, MOTA, THUTU) VALUES
(N'BÓ HOA RẺ HÔM NAY', N'Các bó hoa giá tốt nhất trong ngày', 1),
(N'HỘP HOA TƯƠI MICA XINH', N'Hộp hoa mica sang trọng, đẹp mắt', 2),
(N'HOA TƯƠI ƯA CHUỘNG', N'Các loại hoa được yêu thích nhất', 3),
(N'HOA TULIP SANG TRỌNG', N'Hoa tulip cao cấp, quý phái', 4),
(N'GARDEN STYLE', N'Phong cách vườn tự nhiên, tươi mới', 5),
(N'BÓ HOA TƯƠI HÔM NAY', N'Sản phẩm hoa mới nhất', 6);
GO

-- 2.2 Chèn dữ liệu DanhMuc - Danh mục cha (cấp 1)
INSERT INTO DanhMuc (TENDM) VALUES
(N'Hoa Sinh Nhật'),      -- MADM = 1
(N'Hoa Khai Trương'),    -- MADM = 2
(N'Hoa Tốt Nghiệp'),     -- MADM = 3
(N'Dịp Tặng'),           -- MADM = 4
(N'Hoa Bó'),             -- MADM = 5
(N'Bó Hoa Hồng');        -- MADM = 6
GO

-- 2.3 Chèn dữ liệu DanhMuc - Danh mục con cho Hoa Sinh Nhật (MADM_CHA = 1)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Bó Hoa Sinh Nhật', 1),           -- MADM = 7
(N'Lẵng Hoa Sinh Nhật', 1),         -- MADM = 8
(N'Giỏ Hoa Sinh Nhật', 1),          -- MADM = 9
(N'Hoa Sinh Nhật Tặng Mẹ', 1),      -- MADM = 10
(N'Hoa Sinh Nhật Tặng Vợ', 1),      -- MADM = 11
(N'Hoa Sinh Nhật Tặng Người Yêu', 1), -- MADM = 12
(N'Hoa Sinh Nhật Tặng Nam', 1),     -- MADM = 13
(N'Hoa Sinh Nhật Tặng Bạn Thân', 1); -- MADM = 14
GO

-- 2.4 Chèn dữ liệu DanhMuc - Danh mục con cho Hoa Khai Trương (MADM_CHA = 2)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Kệ Hoa Khai Trương', 2),         -- MADM = 15
(N'Lẵng Hoa Khai Trương', 2),       -- MADM = 16
(N'Giỏ Hoa Khai Trương', 2),        -- MADM = 17
(N'Kệ Mini Khai Trương', 2);        -- MADM = 18
GO

-- 2.5 Chèn dữ liệu DanhMuc - Danh mục con cho Hoa Tốt Nghiệp (MADM_CHA = 3)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Hướng Dương Tốt Nghiệp', 3), -- MADM = 19
(N'Hoa Tốt Nghiệp Cho Nữ', 3),      -- MADM = 20
(N'Hoa Tốt Nghiệp Cho Nam', 3),     -- MADM = 21
(N'Hoa Tốt Nghiệp Cho Bạn Thân', 3); -- MADM = 22
GO

-- 2.6 Chèn dữ liệu DanhMuc - Danh mục con cho Dịp Tặng (MADM_CHA = 4)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Sự Kiện', 4),                -- MADM = 23
(N'Hoa Tình Yêu', 4),               -- MADM = 24
(N'Hoa Sinh Viên', 4),              -- MADM = 25
(N'Hoa Cưới', 4),                   -- MADM = 26
(N'Hoa 14/02', 4),                  -- MADM = 27
(N'Hoa 8/3', 4),                    -- MADM = 28
(N'BST Hoa 20 Tháng 10', 4),        -- MADM = 29
(N'Hoa 20/11', 4),                  -- MADM = 30
(N'Hoa Chia Buồn', 4);              -- MADM = 31
GO

-- 2.7 Chèn dữ liệu DanhMuc - Danh mục con cho Hoa Bó (MADM_CHA = 5)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Bó Hoa Best Seller', 5),         -- MADM = 32
(N'Hoa Tulip', 5),                  -- MADM = 33
(N'Style: Garden Mix', 5),          -- MADM = 34
(N'Hoa Cẩm Tú Cầu', 5),             -- MADM = 35
(N'Hoa Hướng Dương', 5),            -- MADM = 36
(N'Hoa Cao Cấp', 5),                -- MADM = 37
(N'Hoa Baby', 5),                   -- MADM = 38
(N'Hoa Sen', 5),                    -- MADM = 39
(N'Hoa Cúc Mẫu Đơn', 5);            -- MADM = 40
GO

-- 2.8 Chèn dữ liệu DanhMuc - Danh mục con cho Bó Hoa Hồng (MADM_CHA = 6)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Hồng', 6),                   -- MADM = 41
(N'Hoa Hồng Ohara', 6),             -- MADM = 42
(N'Hoa Hồng Sophia', 6),            -- MADM = 43
(N'Hoa Hồng Đỏ', 6),                -- MADM = 44
(N'Hoa Hồng Cam', 6),               -- MADM = 45
(N'Hoa Hồng Trắng', 6),             -- MADM = 46
(N'Hoa Hồng Tím', 6);               -- MADM = 47
GO

-- 2.9 Chèn dữ liệu KhuyenMai
INSERT INTO KhuyenMai (TENKM, MOTA, PHANTRAMGIAM, NGAYBATDAU, NGAYKETTHUC, TRANGTHAI) VALUES
(N'Khuyến mãi mùa hè', N'Giảm giá các sản phẩm hoa hồng', 15, '2024-06-01', '2024-08-31', N'Đang áp dụng'),
(N'Khuyến mãi đặc biệt', N'Giảm giá các sản phẩm mới', 10, '2024-07-01', '2024-07-31', N'Đang áp dụng'),
(N'Khuyến mãi sinh nhật', N'Giảm giá hoa sinh nhật', 20, '2024-01-01', '2024-12-31', N'Đang áp dụng');
GO

-- 2.10 Chèn dữ liệu NguoiDung
INSERT INTO NguoiDung (TENDANGNHAP, MATKHAU, HOTEN, SODIENTHOAI, EMAIL, DIACHI, VAITRO) VALUES
('admin', '123456', N'Quản trị viên', '0909123456', 'admin@flowerstore.com', N'Hà Nội', N'Quản trị'),
('customer1', '123456', N'Nguyễn Văn A', '0918123456', 'customer1@gmail.com', N'TP.HCM', N'Khách hàng'),
('customer2', '123456', N'Trần Thị B', '0987123456', 'customer2@gmail.com', N'Đà Nẵng', N'Khách hàng');
GO

-- =====================================================
-- PHẦN 3: CHÈN DỮ LIỆU SẢN PHẨM (MASP bắt đầu từ 1)
-- =====================================================

-- 3.1 Nhóm BÓ HOA TƯƠI HÔM NAY (MASP 1-19)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Rose Pink Tweedia', N'Bó hoa hồng tweedia màu hồng pastel dịu dàng, tinh tế và lãng mạn', 1300000, 15, 'hoa1.jpg', 41),                                    -- MASP = 1
(N'Garden Style Hoa Tặng Người Yêu Bó Hoa Nhẹ Nhàng', N'Bó hoa phong cách vườn nhẹ nhàng, hoàn hảo để tặng người yêu', 1350000, 12, 'hoa19.jpg', 34),        -- MASP = 2
(N'Sophie Pink Rose', N'Bó hoa hồng Sophie màu hồng sang trọng, thể hiện sự ngọt ngào và lãng mạn', 800000, 20, 'hoa2.jpg', 43),                             -- MASP = 3
(N'Flower Rose of Hope', N'Đóa hoa hồng của hy vọng, mang thông điệp tích cực và lạc quan', 1500000, 10, 'hoa21.jpg', 41),                                   -- MASP = 4
(N'Bó Hoa Hồng Sophie LoveHeart', N'Bó hoa hồng Sophie hình trái tim, biểu tượng của tình yêu vĩnh cửu', 650000, 18, 'hoa22.jpg', 41),                       -- MASP = 5
(N'Bó Hoa Hồng Sophie Gửi Tặng Sinh Nhật', N'Bó hoa hồng Sophie tuyệt đẹp, lựa chọn hoàn hảo cho quà tặng sinh nhật', 1300000, 15, 'hoa3.jpg', 7),           -- MASP = 6
(N'Bó Hoa Hồng David Austin Beauty', N'Bó hoa hồng David Austin với vẻ đẹp cổ điển và hương thơm quyến rũ', 1350000, 12, 'hoa24.jpg', 41),                   -- MASP = 7
(N'Hydrangea Muse Bouquet', N'Bó hoa cẩm tú cầu Muse thanh lịch, phù hợp cho mọi dịp đặc biệt', 800000, 20, 'hoa25.jpg', 35),                                -- MASP = 8
(N'Peach Serenity Bouquet', N'Bó hoa đào serenity mang đến sự bình yên và thư giãn tinh thần', 1500000, 10, 'hoa27.jpg', 9),                                 -- MASP = 9
(N'Bó Hoa Hồng Sweet Harmony', N'Bó hoa hồng Sweet Harmony với sự kết hợp hài hòa màu sắc và hương thơm', 650000, 18, 'hoa26.jpg', 41),                      -- MASP = 10
(N'Sunny Rose Pink', N'Bừng sáng ngày mới với sắc hồng rực rỡ, mang theo năng lượng tích cực và niềm vui trọn vẹn.', 260000, 18, 'hoa5.jpg', 41),            -- MASP = 11
(N'Bó Hoa Giá Rẻ Cẩm Tú Cầu Hàn Quốc', N'Cẩm Tú Cầu Hàn Quốc thanh lịch, sang trọng mà vẫn vô cùng tiết kiệm.', 345000, 18, 'hoa4.jpg', 35),                -- MASP = 12
(N'Bó Hoa Hồng Sinh Nhật Giá Rẻ Đẹp', N'Sinh nhật rực rỡ, ngập tràn yêu thương với bó hồng tươi thắm.', 345000, 18, 'hoa6.jpg', 7),                          -- MASP = 13
(N'Little Lamb Hydrangea', N'Ngọt ngào và thuần khiết như chú cừu non. Đáng yêu cho mọi khoảnh khắc.', 330000, 18, 'hoa12.jpg', 35),                         -- MASP = 14
(N'Bó Hoa Tươi Yêu Nàng Hẹn Hò Ngày Đầu', N'Chinh phục trái tim ngay từ lần đầu gặp gỡ.', 320000, 18, 'hoa13.jpg', 24),                                     -- MASP = 15
(N'Hoa Bó Sinh Nhật Đồng Tiền Hồng', N'Tặng hoa Đồng Tiền Hồng, trao gửi lời chúc tài lộc và hạnh phúc viên mãn.', 300000, 18, 'hoa14.jpg', 7),              -- MASP = 16
(N'Bó Hoa Tươi Tặng Sinh Nhật Đẹp', N'Món quà sinh nhật hoàn hảo! Một bó hoa tươi đẹp rạng ngời.', 330000, 18, 'hoa15.jpg', 7),                              -- MASP = 17
(N'Bó Hoa Sinh Nhật Giá Rẻ Hồng Cam Đồng Tiền', N'Sắc Hồng - Cam rực rỡ, mang đến sinh nhật vui tươi và ấm áp.', 265000, 18, 'hoa16.jpg', 7),                -- MASP = 18
(N'Serenity in Bloom Flowers', N'Cảm nhận sự bình yên giữa muôn hoa khoe sắc.', 250000, 18, 'hoa17.jpg', 41);                                                -- MASP = 19
GO

-- 3.2 Nhóm HỘP HOA TƯƠI MICA XINH (MASP 20-25)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Tulip Silli Pink', N'Bó tulip Silli Pink mang sắc hồng pastel dịu dàng, tượng trưng cho tình yêu nhẹ nhàng và lãng mạn', 1500000, 5, 'hoa38.jpg', 33),     -- MASP = 20
(N'True Love Moment', N'Khoảnh khắc tình yêu đích thực được lưu giữ trọn vẹn trong hộp hoa mica sang trọng.', 950000, 12, 'hoa39.jpg', 24),                  -- MASP = 21
(N'Hộp Hoa Mica Lucent Pastel', N'Tỏa sáng với vẻ đẹp trong trẻo và dịu dàng từ những sắc màu pastel.', 260000, 20, 'hoa40.jpg', 41),                        -- MASP = 22
(N'Vali Hoa Tươi Mica Ấm Áp Tặng Nữ', N'Thiết kế vali độc đáo chứa đựng trọn vẹn hơi ấm và tình cảm chân thành.', 550000, 10, 'hoa41.jpg', 20),              -- MASP = 23
(N'Hộp Hoa Mica Sắc Hồng Êm Ái', N'Chạm đến trái tim bằng sự êm ái khó cưỡng.', 845000, 18, 'hoa42.jpg', 24),                                                -- MASP = 24
(N'Vali Hoa Tươi Mica Shimmer Delphinium', N'Lấp lánh và quyến rũ như chính nàng!', 950000, 15, 'hoa43.jpg', 37);                                            -- MASP = 25
GO

-- 3.3 Nhóm HOA TULIP SANG TRỌNG (MASP 26-30)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Tulip Treasure', N'Bó hoa tulip quý giá, sang trọng', 1300000, 15, 'hoa33.jpg', 33),                   -- MASP = 26
(N'Ohara Tulips White', N'Bó hoa tulip Ohara trắng tinh khôi', 1350000, 12, 'hoa34.jpg', 33),             -- MASP = 27
(N'Tulip Flower Sweet Blossom', N'Bó hoa tulip nở ngọt ngào', 800000, 20, 'hoa35.jpg', 33),               -- MASP = 28
(N'Bó Hoa Tulip Mãi Mãi Yêu', N'Bó hoa tulip thể hiện tình yêu vĩnh cửu', 1500000, 10, 'hoa36.jpg', 33),  -- MASP = 29
(N'Bó Hoa Tulip White Love', N'Bó hoa tulip trắng tình yêu thuần khiết', 650000, 18, 'hoa37.jpg', 33);    -- MASP = 30
GO

-- 3.4 Nhóm GARDEN STYLE (MASP 31-35)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Blushing Meadow', N'Bó hoa đồng cỏ ửng hồng', 680000, 25, 'hoa28.jpg', 34),                            -- MASP = 31
(N'Morning Blossom', N'Bó hoa nở rộ buổi sáng', 755000, 22, 'hoa29.jpg', 34),                             -- MASP = 32
(N'Love in Full Bloom', N'Tình yêu nở rộ - bó hoa đặc biệt', 625000, 18, 'hoa30.jpg', 34),                -- MASP = 33
(N'Bó Hoa Tươi Garden Feeling', N'Bó hoa cảm giác vườn tược', 750000, 20, 'hoa31.jpg', 34),               -- MASP = 34
(N'Bó Hoa Tặng Mẹ Yêu Thương', N'Bó hoa ý nghĩa tặng mẹ', 480000, 30, 'hoa32.jpg', 10);                   -- MASP = 35
GO

-- 3.5 Nhóm HOA GIÁ RẺ (MASP 36-41)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Bó Hoa Sinh Nhật Cẩm Tú Cầu Moomi', N'Bó hoa sinh nhật giá rẻ', 245000, 40, 'hoa7.jpg', 35),           -- MASP = 36
(N'Bó Hoa Giá Rẻ Litte Rose Tana', N'Bó hoa hồng nhỏ giá tốt', 280000, 35, 'hoa18.jpg', 41),              -- MASP = 37
(N'Bó Hoa Tú Cầu Kem Dâu', N'Bó hoa cẩm tú cầu màu kem', 330000, 28, 'hoa8.jpg', 35),                     -- MASP = 38
(N'Bó hoa cẩm tú cầu Nàng Thơ', N'Bó hoa cẩm tú cầu tươi sáng', 330000, 25, 'hoa9.jpg', 35),              -- MASP = 39
(N'Bó Hoa The Ivory Dream', N'Bó hoa giấc mơ ngà trắng', 330000, 20, 'hoa10.jpg', 41),                    -- MASP = 40
(N'Shimmer IZI', N'Bó hoa lấp lánh IZI', 355000, 32, 'hoa11.jpg', 41);                                    -- MASP = 41
GO

-- =====================================================
-- PHẦN 4: GÁN SẢN PHẨM VÀO PHÂN LOẠI TRANG CHỦ
-- =====================================================

-- 4.1 Gán sản phẩm vào phân loại "BÓ HOA RẺ HÔM NAY" (MAPHANLOAI = 1)
-- Gồm các sản phẩm giá rẻ: Little Lamb, Đồng Tiền, các bó hoa giá dưới 400k
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(11, 1, 1),  -- Sunny Rose Pink - 260k
(18, 1, 2),  -- Bó Hoa Sinh Nhật Giá Rẻ Hồng Cam Đồng Tiền - 265k
(19, 1, 3),  -- Serenity in Bloom Flowers - 250k
(12, 1, 4),  -- Bó Hoa Giá Rẻ Cẩm Tú Cầu Hàn Quốc - 345k
(13, 1, 5),  -- Bó Hoa Hồng Sinh Nhật Giá Rẻ Đẹp - 345k
(14, 1, 6),  -- Little Lamb Hydrangea - 330k
(15, 1, 7),  -- Bó Hoa Tươi Yêu Nàng Hẹn Hò Ngày Đầu - 320k
(16, 1, 8),  -- Hoa Bó Sinh Nhật Đồng Tiền Hồng - 300k
(17, 1, 9),  -- Bó Hoa Tươi Tặng Sinh Nhật Đẹp - 330k
(36, 1, 10), -- Bó Hoa Sinh Nhật Cẩm Tú Cầu Moomi - 245k
(37, 1, 11), -- Bó Hoa Giá Rẻ Litte Rose Tana - 280k
(38, 1, 12), -- Bó Hoa Tú Cầu Kem Dâu - 330k
(39, 1, 13), -- Bó hoa cẩm tú cầu Nàng Thơ - 330k
(40, 1, 14), -- Bó Hoa The Ivory Dream - 330k
(41, 1, 15); -- Shimmer IZI - 355k
GO

-- 4.2 Gán sản phẩm vào phân loại "HỘP HOA TƯƠI MICA XINH" (MAPHANLOAI = 2)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(20, 2, 1),  -- Tulip Silli Pink
(21, 2, 2),  -- True Love Moment
(22, 2, 3),  -- Hộp Hoa Mica Lucent Pastel
(23, 2, 4),  -- Vali Hoa Tươi Mica Ấm Áp Tặng Nữ
(24, 2, 5),  -- Hộp Hoa Mica Sắc Hồng Êm Ái
(25, 2, 6); -- Vali Hoa Tươi Mica Shimmer Delphinium
GO

-- 4.3 Gán sản phẩm vào phân loại "HOA TƯƠI ƯA CHUỘNG" (MAPHANLOAI = 3)
-- Các sản phẩm bán chạy, được yêu thích
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(1, 3, 1),   -- Rose Pink Tweedia
(3, 3, 2),   -- Sophie Pink Rose
(4, 3, 3),   -- Flower Rose of Hope
(6, 3, 4),   -- Bó Hoa Hồng Sophie Gửi Tặng Sinh Nhật
(7, 3, 5),   -- Bó Hoa Hồng David Austin Beauty
(8, 3, 6),   -- Hydrangea Muse Bouquet
(10, 3, 7),  -- Bó Hoa Hồng Sweet Harmony
(21, 3, 8),  -- True Love Moment
(29, 3, 9);  -- Bó Hoa Tulip Mãi Mãi Yêu
GO

-- 4.4 Gán sản phẩm vào phân loại "HOA TULIP SANG TRỌNG" (MAPHANLOAI = 4)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(20, 4, 1),  -- Tulip Silli Pink
(26, 4, 2),  -- Tulip Treasure
(27, 4, 3),  -- Ohara Tulips White
(28, 4, 4),  -- Tulip Flower Sweet Blossom
(29, 4, 5),  -- Bó Hoa Tulip Mãi Mãi Yêu
(30, 4, 6); -- Bó Hoa Tulip White Love
GO

-- 4.5 Gán sản phẩm vào phân loại "GARDEN STYLE" (MAPHANLOAI = 5)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(2, 5, 1),   -- Garden Style Hoa Tặng Người Yêu Bó Hoa Nhẹ Nhàng
(31, 5, 2),  -- Blushing Meadow
(32, 5, 3),  -- Morning Blossom
(33, 5, 4),  -- Love in Full Bloom
(34, 5, 5),  -- Bó Hoa Tươi Garden Feeling
(35, 5, 6); -- Bó Hoa Tặng Mẹ Yêu Thương
GO

-- 4.6 Gán sản phẩm vào phân loại "BÓ HOA TƯƠI HÔM NAY" (MAPHANLOAI = 6)
-- Các sản phẩm mới nhất, nổi bật
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) VALUES
(1, 6, 1),   -- Rose Pink Tweedia
(2, 6, 2),   -- Garden Style Hoa Tặng Người Yêu Bó Hoa Nhẹ Nhàng
(3, 6, 3),   -- Sophie Pink Rose
(4, 6, 4),   -- Flower Rose of Hope
(5, 6, 5),   -- Bó Hoa Hồng Sophie LoveHeart
(6, 6, 6),   -- Bó Hoa Hồng Sophie Gửi Tặng Sinh Nhật
(7, 6, 7),   -- Bó Hoa Hồng David Austin Beauty
(8, 6, 8),   -- Hydrangea Muse Bouquet
(9, 6, 9),   -- Peach Serenity Bouquet
(10, 6, 10); -- Bó Hoa Hồng Sweet Harmony
GO

-- =====================================================
-- PHẦN 5: KIỂM TRA DỮ LIỆU (Tùy chọn - có thể bỏ khi deploy)
-- =====================================================
-- SELECT * FROM DanhMuc;
SELECT * FROM SanPham;
-- SELECT * FROM PhanLoaiSanPham;
-- SELECT * FROM SanPham_PhanLoai;
-- SELECT * FROM NguoiDung;
-- SELECT * FROM KhuyenMai;