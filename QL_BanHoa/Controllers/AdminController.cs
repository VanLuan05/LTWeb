using Newtonsoft.Json;
using QL_BanHoa.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
namespace QL_BanHoa.Controllers
{
    public class AdminController : Controller
    {

        QL_BanHoaEntities db = new QL_BanHoaEntities();
        string baseAddress = "https://localhost:44305/api/";

        public ActionResult Index()
        {
           
            DashboardViewModel model = new DashboardViewModel();

            // 1. Thống kê Tổng đơn hàng
            model.SoLuongDonHang = db.DonHangs.Count();

            // 2. Thống kê Tổng doanh thu (Nếu null thì trả về 0)
            model.TongDoanhThu = db.DonHangs.Sum(n => n.TONGTIEN) ?? 0;

            // 3. Thống kê Tổng sản phẩm
            model.SoLuongSanPham = db.SanPhams.Count();

            // 4. Thống kê Tổng khách hàng (Chỉ đếm vai trò Khách hàng)
            model.SoLuongKhachHang = db.NguoiDungs.Count(n => n.VAITRO == "Khách hàng");

            // 5. Lấy 5 đơn hàng mới nhất
            model.DonHangMoiNhat = db.DonHangs.OrderByDescending(n => n.NGAYDAT).Take(5).ToList();


            // 2. XỬ LÝ DỮ LIỆU BIỂU ĐỒ (Theo 12 tháng của năm hiện tại)
            var namHienTai = DateTime.Now.Year;

            model.ChartLabels = new List<string>();
            model.ChartDoanhThu = new List<decimal>();
            model.ChartLoiNhuan = new List<decimal>();

            for (int i = 1; i <= 12; i++)
            {
                model.ChartLabels.Add("Tháng " + i);

                // Tính tổng tiền của tháng i trong năm nay
                // Thêm .Value vào để lấy giá trị ngày tháng thực
                decimal tongTienThang = db.DonHangs
                    .Where(n => n.NGAYDAT.HasValue && n.NGAYDAT.Value.Month == i && n.NGAYDAT.Value.Year == namHienTai)
                    .Sum(n => n.TONGTIEN) ?? 0;

                model.ChartDoanhThu.Add(tongTienThang);

                // Giả sử Lợi nhuận là 30% doanh thu (Bạn có thể sửa logic này sau)
                model.ChartLoiNhuan.Add(tongTienThang * 0.3m);
            }

            return View(model);
        }
        public ActionResult FlowerManager(int? trang, string tuKhoa, int? maDanhMuc)
        {
            // 1. Cấu hình phân trang
            int kichThuocTrang = 20; // Số lượng bản ghi trên 1 trang
            int trangHienTai = (trang ?? 1);

            // 2. Lấy nguồn dữ liệu (Kèm bảng DanhMuc)
            var truyVan = db.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            // 3. Xử lý TÌM KIẾM theo tên (nếu có từ khóa)
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                truyVan = truyVan.Where(s => s.TENSP.Contains(tuKhoa));
            }

            // 4. Xử lý LỌC theo danh mục (nếu có chọn danh mục)
            if (maDanhMuc.HasValue)
            {
                truyVan = truyVan.Where(s => s.MADM == maDanhMuc);
            }

            // 5. Sắp xếp (Bắt buộc trước khi phân trang)
            truyVan = truyVan.OrderByDescending(x => x.MASP);

            // 6. Tính toán toán học cho phân trang
            int tongSoBanGhi = truyVan.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / kichThuocTrang);

            // Kiểm tra nếu trang hiện tại vượt quá tổng số trang
            if (trangHienTai > tongSoTrang && tongSoTrang > 0)
            {
                trangHienTai = tongSoTrang;
            }

            // Lấy dữ liệu cho trang hiện tại
            var danhSachKetQua = truyVan.Skip((trangHienTai - 1) * kichThuocTrang)
                                        .Take(kichThuocTrang)
                                        .ToList();

            // 7. Gửi dữ liệu qua View
            ViewBag.TongSoBanGhi = tongSoBanGhi;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TrangHienTai = trangHienTai;
            ViewBag.KichThuocTrang = kichThuocTrang;

            // Giữ lại giá trị lọc để hiển thị lại trên Form
            ViewBag.TuKhoaHienTai = tuKhoa;
            ViewBag.DanhMucHienTai = maDanhMuc;

            // Lấy danh sách danh mục để đổ vào Dropdown
            ViewBag.DanhSachDanhMuc = db.DanhMucs.ToList();

            return View(danhSachKetQua);
        }

        public ActionResult CategoryManager(int? trang, string tuKhoa, int? maDanhMucCha)
        {
            List<DanhMuc> danhSachHienThi = new List<DanhMuc>();
            int trangHienTai = trang ?? 1;
            int kichThuocTrang = 20;

            // Khởi tạo HttpClient để gọi API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Gọi API GetAll để lấy danh sách danh mục cha
                var resAll = client.GetAsync("DanhMucAPI/GetAllDanhMucs").Result;
                if (resAll.IsSuccessStatusCode)
                {
                    var jsonAll = resAll.Content.ReadAsStringAsync().Result;
                    var allList = JsonConvert.DeserializeObject<List<DanhMuc>>(jsonAll);
                    // Lưu vào ViewBag để View hiển thị dropdown
                    ViewBag.DanhSachDanhMuc = allList.Where(x => x.MADM_CHA == null).ToList();
                }

                // Chuẩn bị tham số gửi đi (trang và từ khóa)
                // URL sẽ là: api/DanhMucAPI/GetDanhMucs?trang=1&tuKhoa=hoa
                string apiPath = $"DanhMucAPI/GetDanhMucs?trang={trangHienTai}&tuKhoa={tuKhoa}";

                if (maDanhMucCha.HasValue)
                {
                    apiPath += $"&maDanhMucCha={maDanhMucCha}";
                }

                try
                {
                    // Gọi API (GET) -> Chờ kết quả (.Result)
                    var response = client.GetAsync(apiPath).Result;

                    // Nếu thành công (Code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Đọc nội dung JSON trả về
                        var jsonString = response.Content.ReadAsStringAsync().Result;

                        // Chuyển JSON thành đối tượng C#
                        dynamic dataApi = JsonConvert.DeserializeObject(jsonString);

                        // Lấy danh sách danh mục từ biến "Data" của API
                        danhSachHienThi = dataApi.Data.ToObject<List<DanhMuc>>();

                        // Lấy thông tin phân trang
                        ViewBag.TongSoBanGhi = (int)dataApi.Total;
                        ViewBag.TongSoTrang = (int)dataApi.TotalPages;
                        ViewBag.TrangHienTai = (int)dataApi.CurrentPage;
                    }
                }
                catch (Exception)
                {
                    // Nếu lỗi kết nối API thì danh sách sẽ rỗng
                    ModelState.AddModelError("", "Không thể kết nối đến API.");
                }
            }

            // Các ViewBag hỗ trợ hiển thị
            ViewBag.KichThuocTrang = kichThuocTrang;
            ViewBag.TuKhoaHienTai = tuKhoa;

            ViewBag.MaDanhMucChaHienTai = maDanhMucCha;
            // Dictionary để hiển thị tên danh mục cha
            ViewBag.TraCuuDanhMuc = db.DanhMucs.ToDictionary(x => x.MADM, x => x.TENDM);

            return View(danhSachHienThi);
        }

        public ActionResult MoneyMoney()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AddProduct()
        {
            ViewBag.DanhSachDanhMuc = db.DanhMucs.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult AddProduct(SanPham sanPham, HttpPostedFileBase anhSanPham)
        {
            if (anhSanPham != null && anhSanPham.ContentLength > 0)
            {
                string tenFile = Path.GetFileName(anhSanPham.FileName);
                string duongDan = Path.Combine(Server.MapPath("~/Content/hinhanh/"), tenFile);

                anhSanPham.SaveAs(duongDan);
                sanPham.URL_ANH = tenFile;
            }
            else
            {
                // Nếu người dùng không upload ảnh thì để trống
                sanPham.URL_ANH = "";
            }

            sanPham.NGAYTHEM = DateTime.Now;
            sanPham.LUOTMUA = 0;

            if (ModelState.IsValid)
            {
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("FlowerManager");
            }

            ViewBag.DanhSachDanhMuc = db.DanhMucs.ToList();
            return View(sanPham);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Tìm sản phẩm trong CSDL theo ID
            var sanPham = db.SanPhams.Find(id);

            // Nếu không tìm thấy thì báo lỗi 404
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Load danh sách danh mục để hiển thị lên Dropdown
            ViewBag.DanhSachDanhMuc = db.DanhMucs.ToList();

            return View(sanPham);
        }

        [HttpPost]
        public ActionResult EditProduct(SanPham sanPham, HttpPostedFileBase anhSanPham)
        {
            if (ModelState.IsValid)
            {
                // Lấy sản phẩm gốc từ CSDL ra để cập nhật
                var sanPhamTrongDB = db.SanPhams.FirstOrDefault(s => s.MASP == sanPham.MASP);

                if (sanPhamTrongDB != null)
                {
                    // Cập nhật các thông tin cơ bản
                    sanPhamTrongDB.TENSP = sanPham.TENSP;
                    sanPhamTrongDB.GIA = sanPham.GIA;
                    sanPhamTrongDB.SOLUONGTON = sanPham.SOLUONGTON;
                    sanPhamTrongDB.DONVITINH = sanPham.DONVITINH;
                    sanPhamTrongDB.TRANGTHAI = sanPham.TRANGTHAI;
                    sanPhamTrongDB.MOTA = sanPham.MOTA;
                    sanPhamTrongDB.MADM = sanPham.MADM;

                    // Chỉ cập nhật ảnh nếu người dùng chọn ảnh mới
                    if (anhSanPham != null && anhSanPham.ContentLength > 0)
                    {
                        string tenFile = Path.GetFileName(anhSanPham.FileName);
                        string thuMucLuu = Server.MapPath("~/Content/hinhanh/");

                        if (!Directory.Exists(thuMucLuu))
                        {
                            Directory.CreateDirectory(thuMucLuu);
                        }

                        string duongDan = Path.Combine(thuMucLuu, tenFile);
                        anhSanPham.SaveAs(duongDan);

                        // Cập nhật tên ảnh mới vào CSDL
                        sanPhamTrongDB.URL_ANH = tenFile;
                    }

                    db.SaveChanges();
                    return RedirectToAction("FlowerManager");
                }
            }

            // Nếu lỗi thì load lại danh mục và trả về View
            ViewBag.DanhSachDanhMuc = db.DanhMucs.ToList();
            return View(sanPham);
        }

        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            // Tìm sản phẩm cần xóa
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                try
                {
                    // Xóa ảnh cũ trong thư mục nếu có
                    if (!string.IsNullOrEmpty(sanPham.URL_ANH))
                    {
                        string duongDanAnh = Path.Combine(Server.MapPath("~/Content/hinhanh/"), sanPham.URL_ANH);
                        if (System.IO.File.Exists(duongDanAnh))
                        {
                            System.IO.File.Delete(duongDanAnh);
                        }
                    }

                    db.SanPhams.Remove(sanPham);
                    db.SaveChanges();

                    TempData["ThongBao"] = "Xóa sản phẩm #" + id + " thành công!";
                }
                catch (Exception)
                {
                    // Trường hợp lỗi (ví dụ: sản phẩm đã có trong đơn hàng, không xóa được)
                    TempData["Loi"] = "Không thể xóa sản phẩm này vì đã có dữ liệu liên quan (đơn hàng, giỏ hàng...).";
                }
            }
            else
            {
                TempData["Loi"] = "Không tìm thấy sản phẩm cần xóa.";
            }

            return RedirectToAction("FlowerManager");
        }

        // Xóa nhiều sản phẩm cùng lúc
        [HttpPost]
        public ActionResult DeleteMultipleProducts(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Loi"] = "Bạn chưa chọn sản phẩm nào để xóa.";
                return RedirectToAction("FlowerManager");
            }

            var danhSachDaXoa = new List<int>(); // Chứa các ID xóa thành công
            var danhSachLoi = new List<int>();   // Chứa các ID bị lỗi do khóa ngoại

            foreach (var id in ids)
            {
                var sanPham = db.SanPhams.Find(id);
                if (sanPham != null)
                {
                    try
                    {
                        // 1. Xóa ảnh (nếu có)
                        if (!string.IsNullOrEmpty(sanPham.URL_ANH))
                        {
                            string duongDanAnh = Path.Combine(Server.MapPath("~/Content/hinhanh/"), sanPham.URL_ANH);
                            if (System.IO.File.Exists(duongDanAnh))
                            {
                                System.IO.File.Delete(duongDanAnh);
                            }
                        }

                        // Xóa dưới database
                        db.SanPhams.Remove(sanPham);
                        db.SaveChanges(); // Lưu ngay để bắt lỗi từng sản phẩm

                        danhSachDaXoa.Add(id);
                    }
                    catch (Exception)
                    {
                        // Ghi nhận thất bại do khóa ngoại,...
                        danhSachLoi.Add(id);
                    }
                }
            }

            // thông báo
            if (danhSachDaXoa.Count > 0)
            {
                TempData["ThongBao"] = $"Đã xóa thành công {danhSachDaXoa.Count} sản phẩm (ID: {string.Join(", ", danhSachDaXoa.Select(id => "#" + id))}).";
            }

            if (danhSachLoi.Count > 0)
            {
                TempData["Loi"] = $"Không thể xóa {danhSachLoi.Count} sản phẩm do có dữ liệu liên quan (ID: {string.Join(", ", danhSachLoi)}).";
            }

            return RedirectToAction("FlowerManager");
        }

        [HttpGet]
        public ActionResult AddCategory()
        {
            List<DanhMuc> danhSachDanhMuc = new List<DanhMuc>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Gọi API lấy toàn bộ danh mục để đổ vào dropdown Danh mục cha
                var response = client.GetAsync("DanhMucAPI/GetAllDanhMucs").Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync().Result;
                    danhSachDanhMuc = JsonConvert.DeserializeObject<List<DanhMuc>>(jsonString);
                }
            }

            ViewBag.DanhSachDanhMuc = danhSachDanhMuc;
            return View();
        }
        [HttpPost]
        public ActionResult AddCategory(DanhMuc danhMuc)
        {
            // Kiểm tra sơ bộ
            if (string.IsNullOrEmpty(danhMuc.TENDM))
            {
                ModelState.AddModelError("", "Tên danh mục không được để trống.");
            }

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseAddress);

                    // Gửi dữ liệu object DanhMuc lên API
                    // PostAsJsonAsync tự động chuyển object thành JSON gửi đi
                    var response = client.PostAsJsonAsync("DanhMucAPI/ThemDanhMuc", danhMuc).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["ThongBao"] = "Thêm danh mục thành công!";
                        return RedirectToAction("CategoryManager");
                    }
                    else
                    {
                        // Đọc lỗi từ API trả về
                        var jsonError = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError("", "Lỗi API: " + jsonError);
                    }
                }
            }

            // Nếu thất bại: Gọi lại API lấy danh sách cha để đổ lại vào dropdown vì ViewBag bị mất khi reload
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                var response = client.GetAsync("DanhMucAPI/GetAllDanhMucs").Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync().Result;
                    ViewBag.DanhSachDanhMuc = JsonConvert.DeserializeObject<List<DanhMuc>>(jsonString);
                }
            }

            return View(danhMuc);
        }

        [HttpGet]
        public ActionResult EditCategory(int id)
        {
            var danhMuc = db.DanhMucs.Find(id);
            if (danhMuc == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách để chọn cha, nhưng phải loại trừ chính nó
            ViewBag.DanhSachDanhMuc = db.DanhMucs.Where(x => x.MADM != id).ToList();

            return View(danhMuc);
        }

        [HttpPost]
        public ActionResult EditCategory(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                var dmTrongDB = db.DanhMucs.Find(danhMuc.MADM);
                if (dmTrongDB != null)
                {
                    dmTrongDB.TENDM = danhMuc.TENDM;
                    dmTrongDB.MADM_CHA = danhMuc.MADM_CHA;
                    dmTrongDB.TRANGTHAI = danhMuc.TRANGTHAI;

                    db.SaveChanges();
                    TempData["ThongBao"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction("CategoryManager");
                }
            }

            // Nếu lỗi, load lại danh sách cha, trừ chính nó
            ViewBag.DanhSachDanhMuc = db.DanhMucs.Where(x => x.MADM != danhMuc.MADM).ToList();
            return View(danhMuc);
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Gọi API DELETE: api/DanhMucAPI/XoaDanhMuc/5
                var response = client.DeleteAsync($"DanhMucAPI/XoaDanhMuc/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    // thành công
                    TempData["ThongBao"] = $"Xóa danh mục #{id} thành công!";
                }
                else
                {
                    // thất bại
                    var json = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        dynamic error = JsonConvert.DeserializeObject(json);
                        TempData["Loi"] = $"Không thể xóa danh mục #{id}. Lý do: {error.Message}";
                    }
                    catch
                    {
                        TempData["Loi"] = $"Không thể xóa danh mục #{id} (Lỗi kết nối API).";
                    }
                }
            }
            return RedirectToAction("CategoryManager");
        }


        //Xóa nhiều danh mục cùng lúc
        [HttpPost]
        public ActionResult DeleteMultipleCategories(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Loi"] = "Bạn chưa chọn danh mục nào để xóa.";
                return RedirectToAction("CategoryManager");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Gọi API POST: api/DanhMucAPI/XoaNhieuDanhMuc
                var response = client.PostAsJsonAsync("DanhMucAPI/XoaNhieuDanhMuc", ids).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(json);

                    int deletedCount = (int)result.DeletedCount;
                    int errorCount = (int)result.ErrorCount;

                    // Lấy danh sách ID xóa thành công và thất bại
                    List<int> listDeleted = result.DeletedIds.ToObject<List<int>>();
                    List<int> listError = result.ErrorIds.ToObject<List<int>>();

                    string msg = "";

                    // Xử lý thông báo thành công
                    if (deletedCount > 0)
                    {
                        string strDeletedIds = string.Join(", ", listDeleted.Select(i => "#" + i));
                        msg = $"Đã xóa thành công {deletedCount} danh mục ({strDeletedIds}).";
                    }
                    else
                    {
                        msg = "Xóa không thành công.";
                    }

                    // Xử lý thông báo lỗi (nếu có)
                    if (errorCount > 0)
                    {
                        string strErrorIds = string.Join(", ", listError.Select(i => "#" + i));
                        // Thêm dấu chấm ngắt câu nếu phía trước có nội dung
                        if (deletedCount > 0) msg += " ";

                        msg += $"Không thể xóa {errorCount} mục ({strErrorIds}) do có danh mục con hoặc chứa sản phẩm.";
                    }

                    // Xử lý màu sắc
                    if (errorCount > 0 && deletedCount == 0)
                    {
                        TempData["Loi"] = msg;
                    }
                    else
                    {
                        // Các trường hợp còn lại (thành công hết hoặc thành công 1 phần) thì xanh
                        TempData["ThongBao"] = msg;
                    }
                }
                else
                {
                    TempData["Loi"] = "Lỗi kết nối API khi xóa nhiều.";
                }
            }
            return RedirectToAction("CategoryManager");
        }
        // 1. HIỂN THỊ DANH SÁCH ĐƠN HÀNG
        public ActionResult DonHang()
        {
            // Lấy danh sách đơn hàng sắp xếp theo ngày đặt mới nhất
            var listDH = db.DonHangs.OrderByDescending(n => n.NGAYDAT).ToList();
            return View(listDH);
        }

        // 2. XEM CHI TIẾT ĐƠN HÀNG
        public ActionResult ChiTietDonHang(int id)
        {
            // Tìm đơn hàng theo mã
            var dh = db.DonHangs.Find(id);
            if (dh == null)
            {
                return RedirectToAction("DonHang");
            }
            return View(dh);
        }

        // 3. XỬ LÝ CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        [HttpPost]
        public ActionResult CapNhatTrangThai(int id, string trangThai)
        {
            var dh = db.DonHangs.Find(id);
            if (dh != null)
            {
                dh.TRANG_THAI = trangThai; // Gán trạng thái mới
                db.SaveChanges(); // Lưu vào CSDL
            }
            // Load lại trang chi tiết để thấy thay đổi
            return RedirectToAction("ChiTietDonHang", new { id = id });
        }
        // Action để hiển thị trang in hóa đơn
        public ActionResult InHoaDon(int id)
        {
            var dh = db.DonHangs.Find(id);
            if (dh == null) return HttpNotFound();
            return View(dh);
        }
        public ActionResult XuatHoaDonPDF(int id)
        { 
            var dh = db.DonHangs.Find(id);
            if (dh == null) return HttpNotFound();

            // Thay vì return View, ta return ViewAsPdf
            return new ViewAsPdf("InHoaDon", dh)
            {
                FileName = "HoaDon_" + id + ".pdf", // Tên file khi tải về
                PageSize = Rotativa.Options.Size.A4, // Khổ giấy A4
                PageMargins = new Rotativa.Options.Margins(20, 20, 20, 20), // Canh lề
                                                                         
            };
        }
        public ActionResult TaiKhoan()
        {
            var users = db.NguoiDungs
                  .OrderBy(u => u.VAITRO) // Sắp xếp theo vai trò
                  .ToList();

            // Truyền danh sách người dùng sang View
            return View(users);

        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut(); 
            Session.Clear(); 
            return RedirectToAction("Index", "Home");
        }

        // Quản lý tin nhắn liên hệ
        public ActionResult QLLienHe()
        {
            // Kiểm tra quyền admin nếu cần
            // if (Session["VaiTro"] != "Quản trị") return RedirectToAction("Index", "Login");

            var listLienHe = db.LienHes.OrderByDescending(n => n.NGAYGUI).ToList();
            return View(listLienHe);
        }

        // Xóa tin nhắn (nếu cần)
        public ActionResult XoaLienHe(int id)
        {
            var lh = db.LienHes.Find(id);
            if (lh != null)
            {
                db.LienHes.Remove(lh);
                db.SaveChanges();
            }
            return RedirectToAction("QLLienHe");
        }


        // Lấy thông báo cho Admin 
        [HttpGet]
        public ActionResult GetNotifications()
        {
            var listThongBao = new List<ThongBaoModel>();

            // 1. ĐƠN HÀNG MỚI (Lấy 5 đơn gần nhất trạng thái 'Đang xử lý')
            var donHangMoi = db.DonHangs
                .Where(n => n.TRANG_THAI == "Đang xử lý")
                .OrderByDescending(n => n.NGAYDAT)
                .Take(5)
                .ToList();

            foreach (var item in donHangMoi)
            {
                listThongBao.Add(new ThongBaoModel
                {
                    TieuDe = "Đơn hàng mới #" + item.MADH,
                    NoiDung = "Khách đặt mua " + (item.ChiTietDonHangs.Count) + " sản phẩm.", // Hoặc hiện tên khách nếu có
                    ThoiGian = item.NGAYDAT ?? DateTime.Now,
                    Loai = "order-new",
                    LinkLienKet = "/Admin/ChiTietDonHang/" + item.MADH
                });
            }

            // 2. KHÁCH HỦY ĐƠN (Lấy 5 đơn gần nhất trạng thái 'Đã hủy')
            
            var donHuy = db.DonHangs
                .Where(n => n.TRANG_THAI == "Đã hủy")
                .OrderByDescending(n => n.NGAYDAT)
                .Take(5)
                .ToList();

            foreach (var item in donHuy)
            {
                listThongBao.Add(new ThongBaoModel
                {
                    TieuDe = "Đơn hàng đã hủy #" + item.MADH,
                    NoiDung = "Đơn hàng đã bị hủy.",
                    ThoiGian = item.NGAYDAT ?? DateTime.Now, // Thực tế nên là ngày hủy
                    Loai = "order-cancel",
                    LinkLienKet = "/Admin/ChiTietDonHang/" + item.MADH
                });
            }

            // 3. LIÊN HỆ MỚI (Lấy 5 liên hệ gần nhất)
            
            var lienHeMoi = db.LienHes.OrderByDescending(n => n.NGAYGUI).Take(5).ToList();
            foreach (var item in lienHeMoi)
            {
                listThongBao.Add(new ThongBaoModel
                {
                    TieuDe = "Liên hệ từ " + item.HOTEN, 
                    NoiDung = item.NOIDUNG.Length > 30 ? item.NOIDUNG.Substring(0, 30) + "..." : item.NOIDUNG,
                    ThoiGian = item.NGAYGUI ?? DateTime.Now,
                    Loai = "contact",
                    LinkLienKet = "/Admin/QLLienHe"
                });
            }
            

            // 4. NGƯỜI DÙNG MỚI 
            
            var userMoi = db.NguoiDungs.OrderByDescending(n => n.MAND).Take(5).ToList(); 
            foreach (var item in userMoi)
            {
                listThongBao.Add(new ThongBaoModel
                {
                    TieuDe = "Thành viên mới",
                    NoiDung = item.HOTEN + " vừa đăng ký.",
                    ThoiGian = DateTime.Now, 
                    Loai = "user",
                    LinkLienKet = "/Admin/TaiKhoan"
                });
            }
            

            // Sắp xếp trộn lẫn tất cả theo thời gian giảm dần và lấy 10 tin mới nhất
            var result = listThongBao.OrderByDescending(n => n.ThoiGian).Take(10).ToList();

            // Trả về JSON kèm số lượng tin chưa đọc (ở đây mình đếm đơn đang xử lý làm số lượng badge)
            int soLuongCanXuLy = db.DonHangs.Count(n => n.TRANG_THAI == "Đang xử lý");

            return Json(new
            {
                Count = soLuongCanXuLy,
                List = result.Select(x => new {
                    x.TieuDe,
                    x.NoiDung,
                    ThoiGian = x.ThoiGian.ToString("dd/MM HH:mm"), // Format ngày giờ cho đẹp
                    x.Loai,
                    x.LinkLienKet
                })
            }, JsonRequestBehavior.AllowGet);
        }
    }
}