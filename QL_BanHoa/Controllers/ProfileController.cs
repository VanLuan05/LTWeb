using System;
using System.Linq;
using System.Web.Mvc;
using QL_BanHoa.Models; // Thay bằng namespace model thực tế của bạn

namespace QL_BanHoa.Controllers
{
    public class ProfileController : Controller
    {
        // Khai báo context database (Thay tên class context của bạn vào đây, ví dụ QL_BanHoaEntities)
        QL_BanHoaEntities db = new QL_BanHoaEntities();

        // TRANG HIỂN THỊ THÔNG TIN
        public ActionResult Index()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // 2. Lấy thông tin user mới nhất từ Database (tránh dùng session cũ)
            NguoiDung khSession = (NguoiDung)Session["TaiKhoan"];
            var kh = db.NguoiDungs.SingleOrDefault(n => n.MAND == khSession.MAND); // Thay MAND bằng tên khóa chính của bạn

            return View(kh);
        }

        // XỬ LÝ CẬP NHẬT THÔNG TIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(NguoiDung model)
        {
            if (Session["TaiKhoan"] == null) return RedirectToAction("Index", "Login");

            try
            {
                // Lấy user từ DB để update
                var user = db.NguoiDungs.Find(model.MAND);

                if (user != null)
                {
                    // Chỉ cập nhật các trường cho phép
                    user.HOTEN = model.HOTEN;
                    user.SODIENTHOAI = model.SODIENTHOAI;
                    user.DIACHI = model.DIACHI;
                    // Email và Tên đăng nhập thường không cho sửa hoặc cần quy trình riêng

                    db.SaveChanges();

                    // Cập nhật lại Session tên hiển thị
                    Session["TenKH"] = user.HOTEN;
                    Session["TaiKhoan"] = user;

                    TempData["Success"] = "Cập nhật thông tin thành công!";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra, vui lòng thử lại!";
            }

            return RedirectToAction("Index");
        }
    }
}