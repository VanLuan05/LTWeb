using QL_BanHoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
namespace QL_BanHoa.Controllers
{
    public class HomeController : Controller
    {
        QL_BanHoaEntities db = new QL_BanHoaEntities();
        public ActionResult Index()
        {
            var model = new TrangChu
            {
                BoHoaReHomNay = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "BÓ HOA RẺ HÔM NAY"))
                    .Take(18).ToList(),

                BoHoaTuoiHomNay = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "BÓ HOA TƯƠI HÔM NAY"))
                    .Take(10).ToList(),

                GardenStyle = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "GARDEN STYLE"))
                    .Take(6).ToList(),

                HoaTulipSangTrong = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "HOA TULIP SANG TRỌNG"))
                    .Take(6).ToList(),

                HoaTuoiUaChuong = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "HOA TƯƠI ƯA CHUỘNG"))
                    .Take(18).ToList(),

                HopHoaMicaXinh = db.SanPhams
                    .Where(sp => sp.SanPham_PhanLoai
                    .Any(spl => spl.PhanLoaiSanPham.TENPHANLOAI == "HỘP HOA TƯƠI MICA XINH"))
                    .Take(18).ToList(),

            };

            return View(model);
        }

        public ActionResult ChiTiet(int id)
        {
            // 1. Lấy sản phẩm hiện tại
            var sanpham = db.SanPhams.FirstOrDefault(s => s.MASP == id);

            if (sanpham == null)
            {
                return RedirectToAction("Index");
            }

            // 2. Lấy danh sách sản phẩm liên quan (Cùng Danh Mục, trừ chính nó ra)
            // Lấy khoảng 4-5 sản phẩm để hiển thị
            var sanphamLienQuan = db.SanPhams
                .Where(s => s.MADM == sanpham.MADM && s.MASP != sanpham.MASP && s.TRANGTHAI == "Còn hàng")
                .Take(10) // Lấy 4 sản phẩm
                .ToList();

            // 3. Truyền danh sách qua ViewBag
            ViewBag.SanphamLienQuan = sanphamLienQuan;

            return View(sanpham);
        }

        public ActionResult TimKiemSP(string keyword) // Đổi từ "kw" thành "keyword"
        {
            // Kiểm tra nếu keyword null hoặc rỗng
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index");
            }

            // Tìm kiếm sản phẩm theo tên (có chứa keyword)
            var ketQuaTimKiem = db.SanPhams
                .Where(sp => sp.TENSP.Contains(keyword) ||
                            sp.MOTA.Contains(keyword) ||
                            sp.TRANGTHAI.Contains(keyword))
                .ToList();

            // Lưu keyword để hiển thị lại trên view
            ViewBag.Keyword = keyword;
            ViewBag.SoLuongTimThay = ketQuaTimKiem.Count;

            return View(ketQuaTimKiem);
        }
    }
}