using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_BanHoa.Models; // Hoặc namespace chứa model của bạn

namespace QL_BanHoa.Controllers
{
    public class ThanhToanController : Controller
    {
        QL_BanHoaEntities db = new QL_BanHoaEntities();

        // Hàm lấy giỏ hàng thống nhất (Lấy từ Database vì vào đây bắt buộc đã đăng nhập)
        public List<GioHang> LayGioHang()
        {
            // Vì vào trang thanh toán bắt buộc đăng nhập, nên ta chỉ cần lấy từ DB
            if (Session["TaiKhoan"] != null)
            {
                NguoiDung user = Session["TaiKhoan"] as NguoiDung;

                // Lấy giỏ hàng của user từ DB
                List<GioHang> lstGioHang = db.GioHangs.Where(n => n.MAND == user.MAND).ToList();

                // Cập nhật thông tin hiển thị từ bảng SanPham (Tên, Ảnh, Giá)
                foreach (var item in lstGioHang)
                {
                    if (item.SanPham != null)
                    {
                        item.TenSP = item.SanPham.TENSP;
                        // Lưu ý: Kiểm tra lại tên thuộc tính ảnh trong Model SanPham (URL_ANH hay Anh?)
                        item.Anh = item.SanPham.URL_ANH;
                        item.Gia = item.SanPham.GIA;
                        item.THANHTIEN = item.SOLUONG * item.SanPham.GIA;
                    }
                }
                return lstGioHang;
            }
            return new List<GioHang>();
        }

        // 1. Trang hiển thị form điền thông tin giao hàng
        [HttpGet]
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy giỏ hàng từ Database
            List<GioHang> gioHang = LayGioHang();

            // Kiểm tra giỏ hàng có trống không
            if (gioHang == null || gioHang.Count == 0)
            {
                // Nếu trống thì về trang sản phẩm -> ĐÂY LÀ CHỖ GÂY RA LỖI CŨ CỦA BẠN
                return RedirectToAction("Index", "SanPham");
            }

            // Tính toán tổng tiền
            ViewBag.TongTien = gioHang.Sum(n => n.THANHTIEN);
            ViewBag.TongSoLuong = gioHang.Sum(n => n.SOLUONG);

            // Lấy thông tin người dùng để điền sẵn vào form
            NguoiDung kh = (NguoiDung)Session["TaiKhoan"];
            ViewBag.HoTen = kh.HOTEN;
            ViewBag.DienThoai = kh.SODIENTHOAI;
            ViewBag.DiaChi = kh.DIACHI;

            return View(gioHang);
        }

        // 2. Xử lý đặt hàng (Lưu vào CSDL)
        [HttpPost]
        public ActionResult DatHang(FormCollection f)
        {
            // Kiểm tra đăng nhập lại cho chắc chắn
            if (Session["TaiKhoan"] == null)
                return RedirectToAction("Index", "Login");

            // Lấy giỏ hàng hiện tại
            List<GioHang> gioHang = LayGioHang();
            if (gioHang == null || gioHang.Count == 0)
                return RedirectToAction("Index", "SanPham");

            // --- A. LƯU ĐƠN HÀNG (Bảng DONHANG) ---
            DonHang ddh = new DonHang();
            NguoiDung kh = (NguoiDung)Session["TaiKhoan"];

            ddh.MAND = kh.MAND;
            ddh.NGAYDAT = DateTime.Now;

            // Xử lý ngày giao
            var ngayGiao = f["NgayGiao"];
            if (!string.IsNullOrEmpty(ngayGiao))
                ddh.NGAYGIAOHANGDK = DateTime.Parse(ngayGiao);
            else
                ddh.NGAYGIAOHANGDK = DateTime.Now.AddDays(3);

            // Các thông tin khác
            ddh.TRANG_THAI = "Đang xử lý";
            ddh.PHUONGTHUCTHANHTOAN = "Thanh toán khi nhận hàng (COD)";
            ddh.TENNGUOINHAN = f["NguoiNhan"];
            ddh.DIACHIGIAOHANG = f["DiaChiNhan"];
            ddh.SDTNGUOINHAN = f["SDT"];
            ddh.GHI_CHU = f["GhiChu"];

            // Tính tổng tiền đơn hàng
            ddh.TONGTIEN = gioHang.Sum(n => n.THANHTIEN);

            db.DonHangs.Add(ddh);
            db.SaveChanges(); // Lưu để lấy MADH

            // --- B. LƯU CHI TIẾT ĐƠN HÀNG (Bảng CHITIETDONHANG) ---
            foreach (var item in gioHang)
            {
                ChiTietDonHang ctdh = new ChiTietDonHang();
                ctdh.MADH = ddh.MADH;
                ctdh.MASP = (int)item.MASP;
                ctdh.SOLUONG = item.SOLUONG;
                ctdh.THANHTIEN = item.THANHTIEN; // Hoặc item.Gia * item.SOLUONG

                db.ChiTietDonHangs.Add(ctdh);
            }
            db.SaveChanges();

            // --- C. XÓA GIỎ HÀNG SAU KHI ĐẶT THÀNH CÔNG ---
            // Lấy danh sách cần xóa và chuyển về List để tránh lỗi DataReader
            var cartItems = db.GioHangs.Where(n => n.MAND == kh.MAND).ToList();

            // Duyệt qua từng món và xóa thủ công (Dành cho Entity Framework 5)
            foreach (var item in cartItems)
            {
                db.GioHangs.Remove(item);
            }

            db.SaveChanges();
            // Reset Session số lượng
            Session["SoLuong"] = 0;

            return RedirectToAction("XacNhanDonHang", "ThanhToan");
        }

        public ActionResult XacNhanDonHang()
        {
            return View();
        }
    }
}