using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BanHoa.Controllers
{
    public class LienHeController : Controller
    {
        // GET: LienHe
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GuiLienHe(string HoTen, string Email, string SDT, string NoiDung)
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            // Nếu chưa có session tài khoản, chuyển hướng sang trang đăng nhập
            if (Session["TaiKhoan"] == null)
            {
                TempData["Loi"] = "Bạn cần đăng nhập để gửi liên hệ!";
                // Truyền duongdan để sau khi đăng nhập xong nó quay lại trang Liên hệ
                return RedirectToAction("Index", "Login", new { duongdan = Url.Action("Index", "LienHe") });
            }

            // 2. XỬ LÝ GỬI TIN (Code cũ giữ nguyên)
            if (ModelState.IsValid)
            {
                QL_BanHoaEntities db = new QL_BanHoaEntities();

                LienHe lh = new LienHe();
                lh.HOTEN = HoTen;
                lh.EMAIL = Email;
                lh.SDT = SDT;
                lh.NOIDUNG = NoiDung;
                lh.NGAYGUI = DateTime.Now;
                lh.TRANGTHAI = "Chưa xem";

                db.LienHes.Add(lh);
                db.SaveChanges();

                ViewBag.ThongBao = "Cảm ơn " + HoTen + "! Tin nhắn của bạn đã được gửi thành công.";
            }
            return View("Index");
        }
    }
}