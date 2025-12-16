using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BanHoa.Controllers
{
    public class DonHangController : Controller
    {
        QL_BanHoaEntities db = new QL_BanHoaEntities();

        // 1. Xem danh sách tất cả đơn hàng của khách
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["TaiKhoan"] == null)
                return RedirectToAction("Index", "Login");

            // Lấy user từ session
            NguoiDung kh = (NguoiDung)Session["TaiKhoan"];

            // Lấy danh sách đơn hàng của user đó, sắp xếp đơn mới nhất lên đầu
            // Lưu ý: MAKH trong DONHANG phải khớp với MAND của NguoiDung
            var lstDonHang = db.DonHangs.Where(n => n.MAND == kh.MAND)
                                        .OrderByDescending(n => n.NGAYDAT)
                                        .ToList();

            return View(lstDonHang);
        }

        // 2. Xem chi tiết một đơn hàng cụ thể
        public ActionResult ChiTiet(int id)
        {
            if (Session["TaiKhoan"] == null)
                return RedirectToAction("Index", "Login");

            NguoiDung kh = (NguoiDung)Session["TaiKhoan"];

            // Lấy đơn hàng theo Mã ĐH và phải đúng là của Khách hàng đó (tránh xem trộm đơn người khác)
            var dh = db.DonHangs.FirstOrDefault(n => n.MADH == id && n.MAND == kh.MAND);

            if (dh == null)
            {
                // Nếu không tìm thấy hoặc đơn hàng không phải của user này
                return RedirectToAction("Index");
            }

            return View(dh);
        }
        // Action Hủy đơn hàng
        public ActionResult HuyDonHang(int id)
        {
            // 1. Tìm đơn hàng theo ID
            var donHang = db.DonHangs.Find(id);

            // 2. Kiểm tra điều kiện hủy
            // Chỉ cho phép hủy khi trạng thái là "Đang xử lý" (hoặc trạng thái khởi tạo của bạn)
            if (donHang != null && donHang.TRANG_THAI == "Đang xử lý")
            {
                // 3. Cập nhật trạng thái
                donHang.TRANG_THAI = "Đã hủy";

                // (Tùy chọn) Nếu bạn có quản lý kho, nhớ cộng lại số lượng sản phẩm vào kho tại đây

                db.SaveChanges();
                TempData["ThongBao"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["Loi"] = "Không thể hủy đơn hàng này (Đã giao hoặc đang vận chuyển).";
            }

            // 4. Quay lại trang danh sách
            return RedirectToAction("Index");
        }
    }
}