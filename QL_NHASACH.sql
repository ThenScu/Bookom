USE master;
GO

-- 1. XÓA DB CŨ NẾU TỒN TẠI (ĐỂ LÀM MỚI TỪ ĐẦU)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QL_NHASACH')
BEGIN
    ALTER DATABASE QL_NHASACH SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_NHASACH;
END
GO

CREATE DATABASE QL_NHASACH;
GO
USE QL_NHASACH;
GO

-- 2. TẠO CÁC BẢNG (TABLES)

-- Bảng ACCOUNT
CREATE TABLE ACCOUNT (
    MAACCOUNT INT IDENTITY(1,1),
    PASSWORD VARCHAR(100) NOT NULL,
    NAME NVARCHAR(500),
    EMAIL VARCHAR(100) NOT NULL,
    SDT VARCHAR(15) NOT NULL,
    DIACHI NVARCHAR(255),
    TYPEACCOUNT VARCHAR(20) NOT NULL, -- Admin, NhanVien, KhachHang
    CONSTRAINT PK_ACCOUNT PRIMARY KEY (MAACCOUNT),
    CONSTRAINT UQ_ACCOUNT_EMAIL UNIQUE (EMAIL),
    CONSTRAINT UQ_ACCOUNT_SDT UNIQUE (SDT),
    CONSTRAINT CK_TYPEACCOUNT CHECK (TYPEACCOUNT IN ('Admin', 'NhanVien', 'KhachHang')),
    CONSTRAINT CK_PASSWORD_LEN CHECK (LEN(PASSWORD) >= 6)
);

-- Bảng NHÀ SẢN XUẤT
CREATE TABLE NHASANXUAT (
    MANSX INT IDENTITY(1,1),
    TENNSX NVARCHAR(255) NOT NULL,
    DIACHI NVARCHAR(255),
    SDT VARCHAR(15),
    CONSTRAINT PK_NHASANXUAT PRIMARY KEY (MANSX)
);

-- Bảng THỂ LOẠI
CREATE TABLE THELOAI (
    MATL INT IDENTITY(1,1),
    TENTL NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_THELOAI PRIMARY KEY (MATL)
);

-- Bảng SÁCH (Đã có cột ANHBIA)
CREATE TABLE SACH (
    MASACH INT IDENTITY(1,1),
    TENSACH NVARCHAR(255) NOT NULL,
    TACGIA NVARCHAR(100),
    GIA FLOAT,
    SOLUONG INT DEFAULT 0,
    MANSX INT,
    MATL INT,
    ANHBIA VARCHAR(255), -- Thêm trực tiếp
    CONSTRAINT PK_SACH PRIMARY KEY (MASACH),
    CONSTRAINT FK_SACH_NHASANXUAT FOREIGN KEY (MANSX) REFERENCES NHASANXUAT(MANSX),
    CONSTRAINT FK_SACH_THELOAI FOREIGN KEY (MATL) REFERENCES THELOAI(MATL)
);

-- Bảng KHÁCH HÀNG (Liên kết với Account)
CREATE TABLE KHACHHANG (
    MAKH INT IDENTITY(1,1),
    TENKH NVARCHAR(100) NOT NULL,
    DIACHI NVARCHAR(255),
    SDT VARCHAR(15),
    EMAIL VARCHAR(100),
    MAACCOUNT INT, -- Liên kết tài khoản
    CONSTRAINT PK_KHACHHANG PRIMARY KEY (MAKH),
    CONSTRAINT UQ_KHACHHANG_SDT UNIQUE (SDT),
    CONSTRAINT FK_KHACHHANG_ACCOUNT FOREIGN KEY (MAACCOUNT) REFERENCES ACCOUNT(MAACCOUNT)
);

-- Bảng HÓA ĐƠN (Đã sửa lại cấu trúc chuẩn)
CREATE TABLE HOADON (
    MAHD INT IDENTITY(1,1),
    NGAYLAP DATE NOT NULL,
    MAKH INT, -- Chỉ lưu mã khách hàng
    MAACCOUNT INT, -- (Tùy chọn) Lưu nhân viên lập đơn nếu cần
    CONSTRAINT PK_HOADON PRIMARY KEY (MAHD),
    CONSTRAINT FK_HOADON_KHACHHANG FOREIGN KEY (MAKH) REFERENCES KHACHHANG(MAKH),
    CONSTRAINT FK_HOADON_ACCOUNT FOREIGN KEY (MAACCOUNT) REFERENCES ACCOUNT(MAACCOUNT)
);

-- Bảng CHI TIẾT HÓA ĐƠN
CREATE TABLE CT_HOADON (
    MAHD INT,
    MASACH INT,
    SOLUONG INT,
    DONGIA MONEY,
    CONSTRAINT PK_CT_HOADON PRIMARY KEY (MAHD, MASACH),
    CONSTRAINT FK_CT_HOADON_HOADON FOREIGN KEY (MAHD) REFERENCES HOADON(MAHD),
    CONSTRAINT FK_CT_HOADON_SACH FOREIGN KEY (MASACH) REFERENCES SACH(MASACH)
);

-- Bảng NHẬP HÀNG
CREATE TABLE NHAPHANG (
    MANHAP INT IDENTITY(1,1),
    NGAYNHAP DATE NOT NULL,
    MAACCOUNT INT,
    NHACUNGCAP NVARCHAR(255),
    CONSTRAINT PK_NHAPHANG PRIMARY KEY (MANHAP),
    CONSTRAINT FK_NHAPHANG_ACCOUNT FOREIGN KEY (MAACCOUNT) REFERENCES ACCOUNT(MAACCOUNT)
);

-- Bảng CHI TIẾT NHẬP HÀNG
CREATE TABLE CT_NHAPHANG (
    MANHAP INT,
    MASACH INT,
    SOLUONG INT,
    GIANHAP MONEY,
    CONSTRAINT PK_CT_NHAPHANG PRIMARY KEY (MANHAP, MASACH),
    CONSTRAINT FK_CT_NHAPHANG_NHAPHANG FOREIGN KEY (MANHAP) REFERENCES NHAPHANG(MANHAP),
    CONSTRAINT FK_CT_NHAPHANG_SACH FOREIGN KEY (MASACH) REFERENCES SACH(MASACH)
);
GO

-- 3. INSERT DỮ LIỆU MẪU

-- ACCOUNT
INSERT INTO ACCOUNT (PASSWORD, NAME, EMAIL, SDT, DIACHI, TYPEACCOUNT)
VALUES
('admin123', N'Quản trị viên', 'admin@example.com', '0909000000', N'Văn phòng chính', 'Admin'),
('user1234', N'Nhà sách Nguyễn Văn Cừ', 'user01@gmail.com', '0909111111', N'805–809 Hồng Bàng, Phường 9, Quận 6, Tp. HCM', 'NhanVien'),
('123456', N'Lê Văn C', 'levanc@example.com', '0987654321', N'300 Hai Bà Trưng, Q3, Tp. HCM', 'KhachHang');

-- KHÁCH HÀNG (Liên kết với Account số 3)
INSERT INTO KHACHHANG (TENKH, DIACHI, SDT, EMAIL, MAACCOUNT)
VALUES
(N'Lê Văn C', N'300 Hai Bà Trưng, Q3, Tp. HCM', '0987654321', 'levanc@example.com', 3),
(N'Phạm Thị D', N'10 Võ Văn Tần, Q3, Tp. HCM', '0912345678', 'phamthid@example.com', NULL); -- Khách vãng lai

-- NHÀ SẢN XUẤT
INSERT INTO NHASANXUAT (TENNSX, DIACHI, SDT)
VALUES 
(N'NXB Kim Đồng', N'123 Trần Hưng Đạo, Q1, TP.HCM', '0909123456'), -- Mã 1
(N'NXB Trẻ', N'25 Nguyễn Du, Q1, TP.HCM', '0911222333'),      -- Mã 2
(N'Nhã Nam', N'59 Đỗ Quang, Hà Nội', '02435146875'),    -- Mã 3
(N'First News - Trí Việt', N'11H Nguyễn Thị Minh Khai', '02838227979'), -- Mã 4
(N'NXB Văn Học', N'18 Nguyễn Trường Tộ', '02437161518'); -- Mã 5

-- THỂ LOẠI
INSERT INTO THELOAI (TENTL)
VALUES 
(N'Văn học'),       -- 1
(N'Thiếu nhi'),     -- 2
(N'Kinh tế'),       -- 3
(N'Tiểu thuyết'),   -- 4
(N'CNTT'),          -- 5
(N'Ngoại ngữ'),     -- 6
(N'Kỹ năng sống'),  -- 7
(N'Truyện tranh');  -- 8

-- SÁCH (Đầy đủ dữ liệu & NXB)
INSERT INTO SACH (TENSACH, TACGIA, GIA, SOLUONG, MANSX, MATL, ANHBIA)
VALUES 
-- Nhã Nam & Văn Học
(N'Mắt Biếc', N'Nguyễn Nhật Ánh', 110000, 100, 3, 1, 'MatBiec.jpg'),
(N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh', N'Nguyễn Nhật Ánh', 125000, 80, 3, 1, 'ToiThay.jpg'),
(N'Rừng Na Uy', N'Haruki Murakami', 145000, 30, 3, 4, 'RungNaUy.jpg'),
(N'Nhà Giả Kim', N'Paulo Coelho', 79000, 150, 3, 4, 'NhaGiaKim.png'),
(N'Số Đỏ', N'Vũ Trọng Phụng', 65000, 50, 5, 1, 'SoDo.jpg'),
(N'Chí Phèo', N'Nam Cao', 55000, 40, 5, 1, 'ChiPheo.jpg'),
(N'Truyện Kiều', N'Nguyễn Du', 50000, 100, 5, 1, 'MatBiec.jpg'), -- Tạm dùng ảnh Mắt biếc

-- Thiếu nhi & Truyện tranh (Kim Đồng)
(N'Doraemon Tập 1', N'Fujiko F. Fujio', 20000, 50, 1, 8, 'DoraemonTap1.png'),
(N'Doraemon Tập 2', N'Fujiko F. Fujio', 20000, 200, 1, 8, 'DoraemonTap2.jpg'),
(N'Conan - Thám Tử Lừng Danh Tập 100', N'Gosho Aoyama', 25000, 100, 1, 8, 'ConanTap100.jpg'),
(N'One Piece - Đảo Hải Tặc Tập 1', N'Eiichiro Oda', 22000, 120, 1, 8, 'OnePiece.jpg'),
(N'Thần Đồng Đất Việt - Tập 1', N'Lê Linh', 15000, 50, 1, 2, 'ThanDong.jpg'),

-- Kinh tế & Kỹ năng (First News / Trẻ)
(N'Đắc Nhân Tâm', N'Dale Carnegie', 80000, 30, 4, 3, 'DacNhanTam.jpg'),
(N'Cha Giàu Cha Nghèo', N'Robert Kiyosaki', 155000, 40, 4, 3, 'ChaGiau.jpg'),
(N'Khởi Nghiệp Tinh Gọn', N'Eric Ries', 130000, 25, 4, 3, 'KhoiNghiep.jpg'),
(N'Tuổi Trẻ Đáng Giá Bao Nhiêu', N'Rosie Nguyễn', 85000, 90, 4, 7, 'TuoiTre.jpg'),
(N'Đời Thay Đổi Khi Chúng Ta Thay Đổi', N'Andrew Matthews', 68000, 45, 4, 7, 'DoiThayDoi.jpg'),

-- CNTT (Trẻ)
(N'Clean Code - Mã Sạch', N'Robert C. Martin', 350000, 15, 2, 5, 'CleanCode.jpg'),
(N'Lập Trình C# Cơ Bản', N'Phạm Huy Hoàng', 120000, 30, 2, 5, 'LapTrinhC.jpg'),
(N'Nhập Môn Cơ Sở Dữ Liệu SQL', N'Nguyễn Văn Ất', 95000, 40, 2, 5, 'Sql.png');

-- HÓA ĐƠN
INSERT INTO HOADON (NGAYLAP, MAACCOUNT, MAKH)
VALUES 
(GETDATE(), 2, 1), -- Khách Lê Văn C
(GETDATE(), 2, 2); -- Khách Phạm Thị D

ALTER TABLE HOADON
ADD TINHTRANG INT DEFAULT 0; -- 0: Chưa duyệt, 1: Đã duyệt
GO

-- CHI TIẾT HÓA ĐƠN
INSERT INTO CT_HOADON (MAHD, MASACH, SOLUONG, DONGIA)
VALUES 
(1, 1, 2, 110000), -- Mua 2 cuốn Mắt biếc
(1, 8, 1, 20000),  -- Mua 1 cuốn Doraemon
(2, 13, 1, 80000); -- Mua 1 cuốn Đắc nhân tâm

-- NHẬP HÀNG
INSERT INTO NHAPHANG (NGAYNHAP, MAACCOUNT, NHACUNGCAP)
VALUES 
(GETDATE(), 1, N'NXB Kim Đồng'),
(GETDATE(), 1, N'NXB Trẻ');

-- CHI TIẾT NHẬP HÀNG
INSERT INTO CT_NHAPHANG (MANHAP, MASACH, SOLUONG, GIANHAP)
VALUES
(1, 8, 50, 15000),
(1, 9, 50, 15000),
(2, 13, 30, 60000);
GO

SELECT * FROM SACH;
SELECT * FROM ACCOUNT;
SELECT * FROM KHACHHANG;