using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Helpers; 
namespace QL_BanHoa.Controllers
{
    public class DangKyController : Controller
    {
        QL_BanHoaEntities ql = new QL_BanHoaEntities();
        // GET: DangKy
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuLyFormDK(NguoiDung model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Kiểm tra email đã tồn tại chưa
                    var existingUser = ql.NguoiDungs.FirstOrDefault(u => u.EMAIL == model.EMAIL);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("EMAIL", "Email này đã được sử dụng!");
                        return View("Index", model);
                    }

                    // 2. Kiểm tra tên đăng nhập đã tồn tại chưa
                    var existingUsername = ql.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == model.TENDANGNHAP);
                    if (existingUsername != null)
                    {
                        ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập này đã được sử dụng!");
                        return View("Index", model);
                    }

                    // 3. Tạo user mới và MÃ HÓA MẬT KHẨU
                    var newUser = new NguoiDung
                    {
                        TENDANGNHAP = model.TENDANGNHAP,

                      
                        // Dùng Crypto.HashPassword để mã hóa mật khẩu người dùng nhập (model.MATKHAU)
                        // Sau đó gán vào database
                        MATKHAU = Crypto.HashPassword(model.MATKHAU),
                        // -----------------------

                        HOTEN = model.HOTEN,
                        SODIENTHOAI = model.SODIENTHOAI,
                        EMAIL = model.EMAIL,
                        DIACHI = model.DIACHI,
                        VAITRO = "Khách hàng" // Mặc định là khách hàng
                    };

                    // 4. Lưu vào database
                    ql.NguoiDungs.Add(newUser);
                    ql.SaveChanges();

                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Index", "Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký: " + ex.Message);
                }
            }
            return View("Index", model);
        }

    }
}