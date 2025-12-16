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
    }
}