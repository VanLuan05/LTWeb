using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BanHoa.Controllers
{
    public class GioHangController : Controller
    {
        // 1. Khởi tạo Database
        QL_BanHoaEntities db = new QL_BanHoaEntities();

        // Lấy giỏ hàng (Hỗ trợ cả Database và Session)
        public List<GioHang> LayGioHang()
        {
            // CÁCH 1: Nếu đã đăng nhập -> Lấy từ Database
            if (Session["TaiKhoan"] != null)
            {
                NguoiDung user = Session["TaiKhoan"] as NguoiDung;

                // Lấy các sản phẩm trong bảng GioHang của user này
                List<GioHang> lstGioHang = db.GioHangs.Where(n => n.MAND == user.MAND).ToList();

                // --- QUAN TRỌNG: Cập nhật thông tin hiển thị (Tên, Ảnh, Giá) ---
                // Vì Database thường chỉ lưu ID, ta cần lấy thông tin chi tiết từ bảng SanPham
                foreach (var item in lstGioHang)
                {
                    // Giả sử bạn có quan hệ (Navigation Property) tên là SanPham
                    // Nếu không có, bạn phải query: var sp = db.SanPhams.Find(item.MASP);
                    if (item.SanPham != null)
                    {
                        item.TenSP = item.SanPham.TENSP;
                        item.Anh = item.SanPham.URL_ANH;
                        item.Gia = item.SanPham.GIA;
                        item.THANHTIEN = item.SOLUONG * item.SanPham.GIA;
                    }
                }
                return lstGioHang;
            }

            // CÁCH 2: Chưa đăng nhập -> Lấy từ Session (Code cũ)
            else
            {
                List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
                if (lstGioHang == null)
                {
                    lstGioHang = new List<GioHang>();
                    Session["GioHang"] = lstGioHang;
                }
                return lstGioHang;
            }
        }

        // GET: GioHang/Index
        public ActionResult Index()
        {
            // 1. Lấy giỏ hàng
            List<GioHang> lstGioHang = LayGioHang();

            // 2. Nếu giỏ hàng trống -> Trả về View cùng list rỗng (để View xử lý hiện thông báo trống)
            if (lstGioHang.Count == 0)
            {
                return View(lstGioHang); // <--- QUAN TRỌNG: Phải truyền lstGioHang vào
            }

            // 3. Tính toán tổng tiền
            ViewBag.TongSoLuong = lstGioHang.Sum(n => n.SOLUONG);
            ViewBag.TongTT = lstGioHang.Sum(n => n.THANHTIEN);

            // 4. Trả về View kèm danh sách
            // LỖI CỦA BẠN CÓ THỂ NẰM Ở ĐÂY: Bạn có thể đang viết là return View();
            return View(lstGioHang); // <--- BẮT BUỘC PHẢI CÓ BIẾN lstGioHang Ở TRONG NGOẶC
        }

        // Thêm giỏ hàng (Xử lý lưu Database)
        public ActionResult ThemGioHang(int MaSP, string url, int SoLuong = 1)
        {
            // TRƯỜNG HỢP 1: ĐÃ ĐĂNG NHẬP -> LƯU VÀO DATABASE
            if (Session["TaiKhoan"] != null)
            {
                NguoiDung user = Session["TaiKhoan"] as NguoiDung;

                // Kiểm tra sản phẩm này đã có trong giỏ hàng DB của user chưa
                GioHang sp = db.GioHangs.FirstOrDefault(n => n.MASP == MaSP && n.MAND == user.MAND);

                if (sp == null) // Chưa có -> Thêm mới
                {
                    sp = new GioHang();
                    sp.MASP = MaSP;
                    sp.MAND = user.MAND; // Lưu mã người dùng
                    sp.SOLUONG = SoLuong;
                    sp.NGAYTHEM = DateTime.Now;
                    db.GioHangs.Add(sp);
                }
                else 
                {
                    sp.SOLUONG += SoLuong;
                }

                // Lưu thay đổi xuống SQL Server
                db.SaveChanges();

                // Cập nhật số lượng hiển thị trên icon
                List<GioHang> ghHienTai = db.GioHangs.Where(n => n.MAND == user.MAND).ToList();
                Session["SoLuong"] = ghHienTai.Sum(n => n.SOLUONG);
            }
            // TRƯỜNG HỢP 2: CHƯA ĐĂNG NHẬP -> LƯU VÀO SESSION (Code cũ)
            else
            {
                List<GioHang> dsGioHang = LayGioHang();
                GioHang sanpham = dsGioHang.FirstOrDefault(sp => sp.MASP == MaSP);
                if (sanpham == null)
                {
                    sanpham = new GioHang(MaSP, SoLuong); // Dùng Constructor của bạn
                    dsGioHang.Add(sanpham);
                }
                else
                {
                    sanpham.SOLUONG += SoLuong;
                    sanpham.THANHTIEN += SoLuong * sanpham.Gia;
                }
                Session["SoLuong"] = dsGioHang.Sum(s => s.SOLUONG);
            }

            return Redirect(url);
        }

        // Xóa giỏ hàng
        public ActionResult XoaGioHang(int MaSP)
        {
            // XỬ LÝ DB
            if (Session["TaiKhoan"] != null)
            {
                NguoiDung user = Session["TaiKhoan"] as NguoiDung;
                GioHang sp = db.GioHangs.FirstOrDefault(n => n.MASP == MaSP && n.MAND == user.MAND);
                if (sp != null)
                {
                    db.GioHangs.Remove(sp);
                    db.SaveChanges();

                    // Cập nhật lại session số lượng
                    var listHienTai = db.GioHangs.Where(n => n.MAND == user.MAND).ToList();
                    Session["SoLuong"] = listHienTai.Sum(n => n.SOLUONG);
                }
            }
            // XỬ LÝ SESSION
            else
            {
                List<GioHang> ds = LayGioHang();
                GioHang sp = ds.FirstOrDefault(s => s.MASP == MaSP);
                if (sp != null)
                {
                    ds.RemoveAll(s => s.MASP == MaSP);
                    Session["SoLuong"] = ds.Sum(s => s.SOLUONG);
                }
            }
            return RedirectToAction("Index");
        }

        // Cập nhật giỏ hàng
        public ActionResult CapNhatGioHang(int MaSP, FormCollection f)
        {
            string action = f["action"];
            int soluongMoi = int.Parse(f["SoLuong"].ToString()); // Số lượng hiện tại ở ô input

            // XỬ LÝ DB
            if (Session["TaiKhoan"] != null)
            {
                NguoiDung user = Session["TaiKhoan"] as NguoiDung;
                GioHang item = db.GioHangs.FirstOrDefault(n => n.MASP == MaSP && n.MAND == user.MAND);

                if (item != null)
                {
                    if (action == "plus") item.SOLUONG = soluongMoi + 1;
                    else if (action == "minus" && soluongMoi > 1) item.SOLUONG = soluongMoi - 1;
                    else if (action == "minus" && soluongMoi <= 1) db.GioHangs.Remove(item); // Xóa nếu về 0

                    db.SaveChanges(); // Lưu vào DB

                    // Cập nhật Session số lượng
                    var list = db.GioHangs.Where(n => n.MAND == user.MAND).ToList();
                    Session["SoLuong"] = list.Sum(s => s.SOLUONG);
                }
            }
            // XỬ LÝ SESSION
            else
            {
                var giohang = LayGioHang();
                var item = giohang.FirstOrDefault(s => s.MASP == MaSP);
                if (item != null)
                {
                    if (action == "plus") item.SOLUONG = soluongMoi + 1;
                    else if (action == "minus" && soluongMoi > 1) item.SOLUONG = soluongMoi - 1;
                    else if (action == "minus" && soluongMoi <= 1) giohang.Remove(item);

                    // Tính lại thành tiền (cho Session)
                    if (item.SOLUONG > 0) item.THANHTIEN = item.SOLUONG * item.Gia;

                    Session["SoLuong"] = giohang.Sum(s => s.SOLUONG);
                }
            }
            return RedirectToAction("Index");
        }
    }
}