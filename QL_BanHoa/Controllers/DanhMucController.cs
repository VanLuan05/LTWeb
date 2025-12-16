// Controllers/DanhMucController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BanHoa.Controllers
{
    public class DanhMucController : Controller
    {
        QL_BanHoaEntities db = new QL_BanHoaEntities();

        // GET: /DanhMuc/Index/{id}
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy danh mục theo ID
            var danhMuc = db.DanhMucs.Find(id);
            if (danhMuc == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhMucCha = danhMuc;
            ViewBag.DanhMucCon = LayDanhMucCon(danhMuc.MADM);

            // Lấy sản phẩm theo danh mục
            List<SanPham> sanPhams;

            if (danhMuc.MADM_CHA == null)
            {
                // Nếu là danh mục CHA: lấy tất cả sản phẩm của các danh mục con
                var idsDanhMucCon = LayTatCaDanhMucConIds(danhMuc.MADM);
                sanPhams = db.SanPhams
                    .Where(sp => idsDanhMucCon.Contains((int)sp.MADM) && sp.TRANGTHAI == "Còn hàng")
                    .OrderByDescending(sp => sp.NGAYTHEM)
                    .ToList();
            }
            else
            {
                // Nếu là danh mục CON: chỉ lấy sản phẩm thuộc danh mục đó
                sanPhams = db.SanPhams
                    .Where(sp => sp.MADM == id && sp.TRANGTHAI == "Còn hàng")
                    .OrderByDescending(sp => sp.NGAYTHEM)
                    .ToList();
            }

            ViewBag.SanPhams = sanPhams;
            return View();
        }

        // Lấy danh mục con của một danh mục
        private List<DanhMuc> LayDanhMucCon(int? madmCha)
        {
            return db.DanhMucs
                .Where(dm => dm.MADM_CHA == madmCha && dm.TRANGTHAI == "Hiển thị")
                .OrderBy(dm => dm.TENDM)
                .ToList();
        }

        // Lấy tất cả ID danh mục con (bao gồm cả con của con)
        private List<int> LayTatCaDanhMucConIds(int madmCha)
        {
            var result = new List<int>();
            LayDanhMucConIdsRecursive(madmCha, result);
            return result;
        }

        private void LayDanhMucConIdsRecursive(int madmCha, List<int> result)
        {
            var danhMucCons = db.DanhMucs
                .Where(dm => dm.MADM_CHA == madmCha)
                .Select(dm => dm.MADM)
                .ToList();

            result.AddRange(danhMucCons);

            foreach (var dmId in danhMucCons)
            {
                LayDanhMucConIdsRecursive(dmId, result);
            }
        }
        // PARTIAL VIEW: Menu danh mục chính (cho layout)
        [ChildActionOnly]
        public ActionResult MenuDanhMuc()
        {
            var danhMucCha = db.DanhMucs
                .Where(dm => dm.MADM_CHA == null && dm.TRANGTHAI == "Hiển thị")
                .OrderBy(dm => dm.TENDM)
                .ToList();

           

            return PartialView("_MenuDanhMuc", danhMucCha);
        }

        // PARTIAL VIEW: Danh mục con (cho sidebar)
        [ChildActionOnly]
        public ActionResult DanhMucConSidebar(int? madmCha)
        {
            var danhMucCons = db.DanhMucs
                .Where(dm => dm.MADM_CHA == madmCha && dm.TRANGTHAI == "Hiển thị")
                .OrderBy(dm => dm.TENDM)
                .ToList();

          

            return PartialView("_DanhMucConSidebar", danhMucCons);
        }
    }
}