using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Helpers;
namespace QL_BanHoa.Controllers
{
    public class LoginController : Controller
    {
        QL_BanHoaEntities ql = new QL_BanHoaEntities();

        // GET: Login
        public ActionResult Index()
        {
            string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "/";
            ViewBag.url = referrer;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuLyFormDN(FormCollection f, string duongdan)
        {
            string useroremail = f["email"];
            string pass = f["password"];
            var user = ql.NguoiDungs.SingleOrDefault(k => k.EMAIL == useroremail || k.TENDANGNHAP == useroremail);

            if (user != null && Crypto.VerifyHashedPassword(user.MATKHAU, pass))
            {
                FormsAuthentication.SetAuthCookie(user.TENDANGNHAP, true);

                // Lưu Session 
                Session["TaiKhoan"] = user;
                Session["TenKH"] = user.HOTEN;
                Session["MaKH"] = user.MAND; 
                Session["Email"] = user.EMAIL;
                Session["VaiTro"] = user.VAITRO;

                if (user.VAITRO == "Quản trị")
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (!string.IsNullOrEmpty(duongdan) && duongdan != "/")
                    return Redirect(duongdan);
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng";
                ViewBag.url = duongdan;
                ViewBag.Email = useroremail;

                return View("Index");
            }
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return Redirect("/");
        }
    }
}