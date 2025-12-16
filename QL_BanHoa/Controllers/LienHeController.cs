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
            if (ModelState.IsValid)
            {
                // Khởi tạo đối tượng DB
                QL_BanHoaEntities db = new QL_BanHoaEntities();

                // Tạo tin nhắn mới
                LienHe lh = new LienHe();
                lh.HOTEN = HoTen;
                lh.EMAIL = Email;
                lh.SDT = SDT; // Đừng quên thêm tham số này
                lh.NOIDUNG = NoiDung;
                lh.NGAYGUI = DateTime.Now;
                lh.TRANGTHAI = "Chưa xem";

                // Lưu vào CSDL
                db.LienHes.Add(lh);
                db.SaveChanges();

                ViewBag.ThongBao = "Cảm ơn " + HoTen + "! Tin nhắn của bạn đã được gửi thành công.";
            }
            return View("Index");
        }
    }
}