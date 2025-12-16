use QL_BanHoa 

-- Tạo bảng DanhMuc với cấu trúc đầy đủ
CREATE TABLE DanhMuc
(
    MADM INT IDENTITY(1,1) PRIMARY KEY,
    TENDM NVARCHAR(100) NOT NULL,
    MADM_CHA INT NULL,
    TRANGTHAI NVARCHAR(20) DEFAULT N'Hiển thị',
    FOREIGN KEY (MADM_CHA) REFERENCES DanhMuc(MADM)
);
GO

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

CREATE TABLE SanPham (
    MASP INT IDENTITY(1001,1) PRIMARY KEY,
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

CREATE TABLE DonHang
(
    MADH INT IDENTITY(100001,1) PRIMARY KEY,
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

CREATE TABLE GioHang (
    MAGH INT IDENTITY(1,1) PRIMARY KEY,
    MAND INT,
    MASP INT,
    SOLUONG INT DEFAULT 1 CHECK (SOLUONG > 0),
    NGAYTHEM DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MAND) REFERENCES NguoiDung(MAND),
    FOREIGN KEY (MASP) REFERENCES SanPham(MASP)
);
GO
--Bổ sung thuộc tính cho giỏ hàng
--Thêm thuộc tính ảnh
ALTER TABLE GIOHANG
ADD Anh VARCHAR(255);

--Thêm thuộc tính giá cho giỏ hàng
ALTER TABLE GIOHANG
ADD Gia DECIMAL(18,2);
--Thêm thuộc tính Thành tiền cho giỏ hàng
ALTER TABLE GIOHANG
ADD THANHTIEN DECIMAL(18,2);
--Thêm thuộc tính Tên sản phẩm cho giỏ hàng
ALTER TABLE GIOHANG
ADD TenSP NVARCHAR(100);
--============================================
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

CREATE TABLE PhanLoaiSanPham (
    MAPHANLOAI INT IDENTITY(1,1) PRIMARY KEY,
    TENPHANLOAI NVARCHAR(100) NOT NULL,
    MOTA NVARCHAR(255),
    URL_ANH_BANNER VARCHAR(255),
    TRANGTHAI NVARCHAR(20) DEFAULT N'Hiển thị',
    THUTU INT DEFAULT 0
);
GO

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



-- 3. Chèn dữ liệu phân loại phần trang chủ
INSERT INTO PhanLoaiSanPham (TENPHANLOAI, MOTA, THUTU) VALUES
(N'BÓ HOA RẺ HÔM NAY', N'Các bó hoa giá tốt nhất trong ngày', 1),
(N'HỘP HOA TƯƠI MICA XINH', N'Hộp hoa mica sang trọng, đẹp mắt', 2),
(N'HOA TƯƠI ƯA CHUỘNG', N'Các loại hoa được yêu thích nhất', 3),
(N'HOA TULIP SANG TRỌNG', N'Hoa tulip cao cấp, quý phái', 4),
(N'GARDEN STYLE', N'Phong cách vườn tự nhiên, tươi mới', 5),
(N'BÓ HOA TƯƠI HÔM NAY', N'Sản phẩm hoa mới nhất', 6);
GO
-- Chèn dữ liệu DanhMuc theo cấu trúc menu
-- Danh mục cha (cấp 1)
INSERT INTO DanhMuc (TENDM) VALUES
(N'Hoa Sinh Nhật'),
(N'Hoa Khai Trương'),
(N'Hoa Tốt Nghiệp'),
(N'Dịp Tặng'),
(N'Hoa Bó'),
(N'Bó Hoa Hồng');
GO

-- Danh mục con cho Hoa Sinh Nhật (MADM_CHA = 1)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Bó Hoa Sinh Nhật', 1),
(N'Lẵng Hoa Sinh Nhật', 1),
(N'Giỏ Hoa Sinh Nhật', 1),
(N'Hoa Sinh Nhật Tặng Mẹ', 1),
(N'Hoa Sinh Nhật Tặng Vợ', 1),
(N'Hoa Sinh Nhật Tặng Người Yêu', 1),
(N'Hoa Sinh Nhật Tặng Nam', 1),
(N'Hoa Sinh Nhật Tặng Bạn Thân', 1);
GO

-- Danh mục con cho Hoa Khai Trương (MADM_CHA = 2)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Kệ Hoa Khai Trương', 2),
(N'Lẵng Hoa Khai Trương', 2),
(N'Giỏ Hoa Khai Trương', 2),
(N'Kệ Mini Khai Trương', 2);
GO

-- Danh mục con cho Hoa Tốt Nghiệp (MADM_CHA = 3)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Hướng Dương Tốt Nghiệp', 3),
(N'Hoa Tốt Nghiệp Cho Nữ', 3),
(N'Hoa Tốt Nghiệp Cho Nam', 3),
(N'Hoa Tốt Nghiệp Cho Bạn Thân', 3);
GO

-- Danh mục con cho Dịp Tặng (MADM_CHA = 4)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Sự Kiện', 4),
(N'Hoa Tình Yêu', 4),
(N'Hoa Sinh Viên', 4),
(N'Hoa Cưới', 4),
(N'Hoa 14/02', 4),
(N'Hoa 8/3', 4),
(N'BST Hoa 20 Tháng 10', 4),
(N'Hoa 20/11', 4),
(N'Hoa Chia Buồn', 4);
GO

-- Danh mục con cho Hoa Bó (MADM_CHA = 5)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Bó Hoa Best Seller', 5),
(N'Hoa Tulip', 5),
(N'Style: Garden Mix', 5),
(N'Hoa Cẩm Tú Cầu', 5),
(N'Hoa Hướng Dương', 5),
(N'Hoa Cao Cấp', 5),
(N'Hoa Baby', 5),
(N'Hoa Sen', 5),
(N'Hoa Cúc Mẫu Đơn', 5);
GO

-- Danh mục con cho Bó Hoa Hồng (MADM_CHA = 6)
INSERT INTO DanhMuc (TENDM, MADM_CHA) VALUES
(N'Hoa Hồng', 6),
(N'Hoa Hồng Ohara', 6),
(N'Hoa Hồng Sophia', 6),
(N'Hoa Hồng Đỏ', 6),
(N'Hoa Hồng Cam', 6),
(N'Hoa Hồng Trắng', 6),
(N'Hoa Hồng Tím', 6);
GO

-- Chèn dữ liệu KhuyenMai
INSERT INTO KhuyenMai (TENKM, MOTA, PHANTRAMGIAM, NGAYBATDAU, NGAYKETTHUC, TRANGTHAI) VALUES
(N'Khuyến mãi mùa hè', N'Giảm giá các sản phẩm hoa hồng', 15, '2024-06-01', '2024-08-31', N'Đang áp dụng'),
(N'Khuyến mãi đặc biệt', N'Giảm giá các sản phẩm mới', 10, '2024-07-01', '2024-07-31', N'Đang áp dụng'),
(N'Khuyến mãi sinh nhật', N'Giảm giá hoa sinh nhật', 20, '2024-01-01', '2024-12-31', N'Đang áp dụng');
GO


-- Chèn dữ liệu người dùng mẫu
INSERT INTO NguoiDung (TENDANGNHAP, MATKHAU, HOTEN, SODIENTHOAI, EMAIL, DIACHI, VAITRO) VALUES
('admin', '123456', N'Quản trị viên', '0909123456', 'admin@flowerstore.com', N'Hà Nội', N'Quản trị'),
('customer1', '123456', N'Nguyễn Văn A', '0918123456', 'customer1@gmail.com', N'TP.HCM', N'Khách hàng'),
('customer2', '123456', N'Trần Thị B', '0987123456', 'customer2@gmail.com', N'Đà Nẵng', N'Khách hàng');
GO

-- Insert sản phẩm
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
-- Nhóm HOA TƯƠI HÔM NAY
(N'Rose Pink Tweedia', N'Bó hoa hồng tweedia màu hồng pastel dịu dàng, tinh tế và lãng mạn', 1300000, 15, 'hoa1.jpg', 15),
(N'Garden Style Hoa Tặng Người Yêu Bó Hoa Nhẹ Nhàng', N'Bó hoa phong cách vườn nhẹ nhàng, hoàn hảo để tặng người yêu', 1350000, 12, 'hoa19.jpg', 13),
(N'Sophie Pink Rose', N'Bó hoa hồng Sophie màu hồng sang trọng, thể hiện sự ngọt ngào và lãng mạn', 800000, 20, 'hoa2.jpg', 17),
(N'Flower Rose of Hope', N'Đóa hoa hồng của hy vọng, mang thông điệp tích cực và lạc quan', 1500000, 10, 'hoa21.jpg', 15),
(N'Bó Hoa Hồng Sophie LoveHeart', N'Bó hoa hồng Sophie hình trái tim, biểu tượng của tình yêu vĩnh cửu', 650000, 18, 'hoa22.jpg', 15),
(N'Bó Hoa Hồng Sophie Gửi Tặng Sinh Nhật', N'Bó hoa hồng Sophie tuyệt đẹp, lựa chọn hoàn hảo cho quà tặng sinh nhật', 1300000, 15, 'hoa3.jpg', 7),
(N'Bó Hoa Hồng David Austin Beauty', N'Bó hoa hồng David Austin với vẻ đẹp cổ điển và hương thơm quyến rũ', 1350000, 12, 'hoa24.jpg', 15),
(N'Hydrangea Muse Bouquet', N'Bó hoa cẩm tú cầu Muse thanh lịch, phù hợp cho mọi dịp đặc biệt', 800000, 20, 'hoa25.jpg', 12),
(N'Peach Serenity Bouquet', N'Bó hoa đào serenity mang đến sự bình yên và thư giãn tinh thần', 1500000, 10, 'hoa27.jpg', 9),
(N'Bó Hoa Hồng Sweet Harmony', N'Bó hoa hồng Sweet Harmony với sự kết hợp hài hòa màu sắc và hương thơm', 650000, 18, 'hoa26.jpg', 15),
(N'Sunny Rose Pink', N'Bừng sáng ngày mới với sắc hồng rực rỡ, mang theo năng lượng tích cực và niềm vui trọn vẹn.', 260000, 18, 'hoa5.jpg', 15),
(N'Bó Hoa Giá Rẻ Cẩm Tú Cầu Hàn Quốc', N'Cẩm Tú Cầu Hàn Quốc thanh lịch, sang trọng mà vẫn vô cùng tiết kiệm. Vẻ đẹp đẳng cấp, giá trị không ngờ!', 345000, 18, 'hoa4.jpg', 15),
(N'Bó Hoa Hồng Sinh Nhật Giá Rẻ Đẹp', N'Sinh nhật rực rỡ, ngập tràn yêu thương với bó hồng tươi thắm. Đẹp xuất sắc - Giá siêu sốc!', 345000, 18, 'hoa6.jpg', 15),
(N'Little Lamb Hydrangea', N'Ngọt ngào và thuần khiết như chú cừu non. Đáng yêu cho mọi khoảnh khắc.', 330000, 18, 'hoa12.jpg', 15),
(N'Bó Hoa Tươi Yêu Nàng Hẹn Hò Ngày Đầu', N'Chinh phục "trái tim" ngay từ lần đầu gặp gỡ. Khởi đầu một câu chuyện thật ngọt ngào.', 320000, 18, 'hoa13.jpg', 15),
(N'Hoa Bó Sinh Nhật Đồng Tiền Hồng', N'Tặng hoa Đồng Tiền Hồng, trao gửi lời chúc tài lộc và hạnh phúc viên mãn.', 300000, 18, 'hoa14.jpg', 15),
(N'Bó Hoa Tươi Tặng Sinh Nhật Đẹp', N'Món quà sinh nhật hoàn hảo! Một bó hoa tươi đẹp rạng ngời, thay ngàn lời yêu thương.', 330000, 18, 'hoa15.jpg', 15),
(N'Bó Hoa Sinh Nhật Giá Rẻ Hồng Cam Đồng Tiền', N'Sắc Hồng - Cam rực rỡ, mang đến sinh nhật vui tươi và ấm áp. Rực rỡ bất ngờ, giá cực kỳ hấp dẫn!', 265000, 18, 'hoa16.jpg', 15),
(N'Serenity in Bloom Flowers', N'Cảm nhận sự bình yên giữa muôn hoa khoe sắc. Khoảnh khắc tĩnh lặng và thư thái cho tâm hồn.', 250000, 18, 'hoa17.jpg', 15);

INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
-- Nhóm HOA TƯƠI xinh mica
(N'Tulip Silli Pink', N'Bó tulip Silli Pink mang sắc hồng pastel dịu dàng, tượng trưng cho tình yêu nhẹ nhàng và lãng mạn', 1500000, 5, 'hoa38.jpg', 20),
(N'True Love Moment', N'Khoảnh khắc tình yêu đích thực được lưu giữ trọn vẹn trong hộp hoa mica sang trọng. Món quà hoàn hảo để thay lời muốn nói: "Anh yêu Em", phù hợp cho ngày kỷ niệm, Valentine hay đề nghị kết hôn.', 950000, 12, 'hoa39.jpg', 21),
(N'Hộp Hoa Mica Lucent Pastel', N'Tỏa sáng với vẻ đẹp trong trẻo và dịu dàng từ những sắc màu pastel. Hộp hoa mica Lucent Pastel như một viên ngọc trai, mang đến sự ngọt ngào và tinh tế, xóa tan mọi khoảng cách.', 260000, 20, 'hoa40.jpg', 22),
(N'Vali Hoa Tươi Mica Ấm Áp Tặng Nữ', N'Hãy cùng "xách vali yêu thương" đến trao cho cô ấy! Thiết kế vali độc đáo chứa đựng trọn vẹn hơi ấm và tình cảm chân thành, thay lời hỏi thăm và quan tâm đến người bạn đặc biệt.', 550000, 10, 'hoa41.jpg', 20),
(N'Hộp Hoa Mica Sắc Hồng Êm Ái', N'Chạm đến trái tim bằng sự êm ái khó cưỡng. Sắc hồng ngọt ngào trong lớp vỏ mica tinh tế là món quà xoa dịu mọi tổn thương và thắp lên niềm hy vọng mới.', 845000, 18, 'hoa42.jpg', 21),
(N'Vali Hoa Tươi Mica Shimmer Delphinium', N'Lấp lánh và quyến rũ như chính nàng! Vali hoa Mica Shimmer kết hợp cùng loài Delphinium cao quý, tôn vinh vẻ đẹp sang trọng, thanh lịch và đầy mạnh mẽ của người phụ nữ hiện đại.', 950000, 15, 'hoa43.jpg', 22);

-- Chèn sản phẩm mới
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
-- Nhóm HOA TULIP SANG TRỌNG
(N'Tulip Treasure', N'Bó hoa tulip quý giá, sang trọng', 1300000, 15, 'hoa33.jpg', 4),
(N'Ohara Tulips White', N'Bó hoa tulip Ohara trắng tinh khôi', 1350000, 12, 'hoa34.jpg', 4),
(N'Tulip Flower Sweet Blossom', N'Bó hoa tulip nở ngọt ngào', 800000, 20, 'hoa35.jpg', 4),
(N'Bó Hoa Tulip Mãi Mãi Yêu', N'Bó hoa tulip thể hiện tình yêu vĩnh cửu', 1500000, 10, 'hoa36.jpg', 4),
(N'Bó Hoa Tulip White Love', N'Bó hoa tulip trắng tình yêu thuần khiết', 650000, 18, 'hoa37.jpg', 4);


INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
-- Nhóm GARDEN STYLE
(N'Blushing Meadow', N'Bó hoa đồng cỏ ửng hồng', 680000, 25, 'hoa28.jpg', 6),
(N'Morning Blossom', N'Bó hoa nở rộ buổi sáng', 755000, 22, 'hoa29.jpg', 6),
(N'Love in Full Bloom', N'Tình yêu nở rộ - bó hoa đặc biệt', 625000, 18, 'hoa30.jpg', 6),
(N'Bó Hoa Tươi Garden Feeling', N'Bó hoa cảm giác vườn tược', 750000, 20, 'hoa31.jpg', 6),
(N'Bó Hoa Tặng Mẹ Yêu Thương', N'Bó hoa ý nghĩa tặng mẹ', 480000, 30, 'hoa32.jpg', 6);

-- Nhóm HOA GIÁ RẺ
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, URL_ANH, MADM) VALUES
(N'Bó Hoa Sinh Nhật Cẩm Tú Cầu Moomi', N'Bó hoa sinh nhật giá rẻ', 245000, 40, 'hoa7.jpg', 10),
(N'Bó Hoa Giá Rẻ Litte Rose Tana', N'Bó hoa hồng nhỏ giá tốt', 280000, 35, 'hoa18.jpg', 10),
(N'Bó Hoa Tú Cầu Kem Dâu', N'Bó hoa cẩm tú cầu màu kem', 330000, 28, 'hoa8.jpg', 10),
(N'Bó hoa cẩm tú cầu Nàng Thơ', N'Bó hoa cẩm tú cầu tươi sáng', 330000, 25, 'hoa9.jpg', 10),
(N'Bó Hoa The Ivory Dream', N'Bó hoa giấc mơ ngà trắng', 330000, 20, 'hoa10.jpg', 10),
(N'Shimmer IZI', N'Bó hoa lấp lánh IZI', 355000, 32, 'hoa11.jpg', 10);
GO


-- KIỂM TRA ID thực tế của các phân loại để gán sản phẩm cho bảng phân loại sản phẩm
SELECT MAPHANLOAI, TENPHANLOAI FROM PhanLoaiSanPham;
GO

-- Gán sản phẩm vào phân loại BÓ HOA RẺ HÔM NAY (MAPHANLOAI = 1)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 1, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP IN (1097, 1099, 1107, 1102, 1108, 1109, 1132, 1133, 1134, 1135, 1136, 1137, 1110, 1111, 1112, 1113, 1114, 1115);
-- -- Gán sản phẩm vào phân loại BÓ HOA TƯƠI HÔM NAY (MAPHANLOAI = 6)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 6, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP BETWEEN 1097 AND 1106;

-- Gán sản phẩm vào phân loại HỘP HOA TƯƠI MICA XINH (MAPHANLOAI = 2)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 2, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP BETWEEN 1138 AND 1143;
--==============
select * from sanpham 
--==============

-- Gán sản phẩm vào phân loại HOA TƯƠI ƯA CHUỘNG (MAPHANLOAI = 3)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 3, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP IN(1097, 1099, 1107, 1102, 1108, 1109, 1132, 1133, 1134, 1135, 1136, 1137, 1110, 1111, 1112, 1113, 1114, 1115);


-- Gán sản phẩm vào phân loại HOA TULIP SANG TRỌNG (MAPHANLOAI = 4)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 4, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP BETWEEN 1154 AND 1158;

-- Gán sản phẩm vào phân loại GARDEN STYLE (MAPHANLOAI = 5)
INSERT INTO SanPham_PhanLoai (MASP, MAPHANLOAI, THUTUHIENTHI) 
SELECT MASP, 5, ROW_NUMBER() OVER(ORDER BY MASP) 
FROM SanPham 
WHERE MASP BETWEEN 1149 AND 1153;
GO

--=====================Thêm bảng liên hệ để kết nối được với khách hàng
CREATE TABLE LienHe (
    MALH INT IDENTITY(1,1) PRIMARY KEY,
    HOTEN NVARCHAR(100),
    EMAIL VARCHAR(100),
    SDT VARCHAR(20),
    NOIDUNG NVARCHAR(MAX),
    NGAYGUI DATETIME DEFAULT GETDATE(),
    TRANGTHAI NVARCHAR(50) DEFAULT N'Chưa xem' -- 'Chưa xem' hoặc 'Đã xem'
);
GO
--===================
select * from donhang
select * from PhanLoaiSanPham
select * from SanPham_PhanLoai
select * from danhmuc 
select * from sanpham
select * from Lienhe 
select * from NGUOIDUNG 
select * from GIOHANG 
select * from CHITIETDONHANG 

