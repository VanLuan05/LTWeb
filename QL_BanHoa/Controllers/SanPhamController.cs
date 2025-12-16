using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BanHoa.Controllers
{
    public class SanPhamController : Controller
    {
        // GET: SanPham
        QL_BanHoaEntities SP = new QL_BanHoaEntities();
        public ActionResult Index()
        {
            List<SanPham> lst = SP.SanPhams.ToList();
            return View(lst);

        }
        // Mở file: Controllers/SanPhamController.cs

        
        // --- THÊM ĐOẠN NÀY VÀO ---
        public ActionResult DanhMuc(int? id)
        {
            if (id == null)
            {
                // Nếu không có ID danh mục thì về trang tất cả sản phẩm
                return RedirectToAction("Index");
            }

            // 1. Lấy danh sách sản phẩm thuộc danh mục đó (kể cả danh mục con nếu muốn)
            var listSP = SP.SanPhams.Where(n => n.MADM == id).OrderByDescending(n => n.GIA).ToList();

            // 2. Lấy tên danh mục để hiển thị tiêu đề
            var danhMuc = SP.DanhMucs.Find(id);
            ViewBag.TieuDe = danhMuc != null ? danhMuc.TENDM : "Danh sách sản phẩm";

            // 3. Tái sử dụng View "Index" để hiển thị (đỡ phải tạo View mới)
            return View("Index", listSP);
        }
        public ActionResult ChiTiet(int id)
        {
            // 1. Tìm sản phẩm theo ID
            var sp = SP.SanPhams.Find(id);

            // 2. Nếu không tìm thấy sản phẩm thì báo lỗi 404
            if (sp == null)
            {
                return HttpNotFound();
            }

            // 3. Lấy các sản phẩm liên quan (cùng danh mục, trừ chính nó)
            ViewBag.SanphamLienQuan = SP.SanPhams
                .Where(n => n.MADM == sp.MADM && n.MASP != sp.MASP)
                .Take(4)
                .ToList();

           
            return View("~/Views/Home/ChiTiet.cshtml", sp);
        }
        // ---------------------------
    }
}
