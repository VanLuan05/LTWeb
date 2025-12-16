using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using QL_BanHoa;

namespace QL_BanHoa.ApiControllers
{
    public class DanhMucAPIController : ApiController
    {
        private QL_BanHoaEntities db = new QL_BanHoaEntities();

        // API hiển thị danh sách danh mục
        // GET: api/DanhMucAPI
        [HttpGet]
        [Route("api/DanhMucAPI/GetDanhMucs")]
        public IHttpActionResult GetDanhMucs(int trang = 1, string tuKhoa = "", int? maDanhMucCha = null)
        {
            int kichThuocTrang = 20;

            // Lấy nguồn dữ liệu
            var truyVan = db.DanhMucs.AsQueryable();
            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                truyVan = truyVan.Where(dm => dm.TENDM.Contains(tuKhoa));
            }
            //Xử lý lọc theo cha
            if (maDanhMucCha.HasValue)
            {
                truyVan = truyVan.Where(dm => dm.MADM_CHA == maDanhMucCha);
            }
            //Sắp xếp
            truyVan = truyVan.OrderBy(x => x.MADM);

            // Tính toán phân trang
            int tongSoBanGhi = truyVan.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / kichThuocTrang);

            // Xử lý trang hiện tại
            if (trang < 1) trang = 1;
            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;

            // Lấy dữ liệu và chuyển đổi sang đối tượng mới
            var danhSach = truyVan.Skip((trang - 1) * kichThuocTrang).Take(kichThuocTrang).ToList().Select(x => new
            {
                x.MADM,
                x.TENDM,
                x.MADM_CHA,
                TenDanhMucCha = x.MADM_CHA != null ? db.DanhMucs.Find(x.MADM_CHA)?.TENDM : null,
                x.TRANGTHAI
            }).ToList();

            // Trả về kết quả dạng json gồm data và thông tin phân trang
            return Ok(new
            {
                Data = danhSach,
                Total = tongSoBanGhi,
                TotalPages = tongSoTrang,
                CurrentPage = trang
            });
        }

        // GET: api/DanhMucAPI/5
        [HttpGet]
        [Route("api/DanhMucAPI/GetDanhMuc/{id}")]
        [ResponseType(typeof(DanhMuc))]
        public IHttpActionResult GetDanhMuc(int id)
        {
            DanhMuc danhMuc = db.DanhMucs.Find(id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            var ketQua = new
            {
                danhMuc.MADM,
                danhMuc.TENDM,
                danhMuc.MADM_CHA,
                danhMuc.TRANGTHAI
            };

            return Ok(ketQua);
        }

        // API Lấy toàn bộ danh mục, dùng cho dropdown
        // URL: /api/DanhMucAPI/GetAllDanhMucs
        [HttpGet]
        [Route("api/DanhMucAPI/GetAllDanhMucs")]
        public IHttpActionResult GetAllDanhMucs()
        {
            // Chỉ lấy ID và Tên để dữ liệu nhẹ, response nhanh
            // SỬA: Lấy thêm MADM_CHA để phục vụ việc lọc danh mục gốc bên Controller
            var list = db.DanhMucs.Select(x => new { x.MADM, x.TENDM, x.MADM_CHA }).ToList();
            return Ok(list);
        }

        // API Thêm danh mục mới
        // URL: /api/DanhMucAPI/ThemDanhMuc
        [HttpPost]
        [Route("api/DanhMucAPI/ThemDanhMuc")]
        public IHttpActionResult ThemDanhMuc(DanhMuc danhMuc)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate thủ công nếu cần (ví dụ tên rỗng)
            if (string.IsNullOrEmpty(danhMuc.TENDM))
            {
                return BadRequest("Tên danh mục không được để trống.");
            }

            // Gán giá trị mặc định
            if (string.IsNullOrEmpty(danhMuc.TRANGTHAI))
            {
                danhMuc.TRANGTHAI = "Hiển thị";
            }

            try
            {
                db.DanhMucs.Add(danhMuc);
                db.SaveChanges();

                // Trả về thông báo thành công
                return Ok(new { Success = true, Message = "Thêm danh mục thành công!", NewId = danhMuc.MADM });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // API Cập nhật danh mục
        // URL: /api/DanhMucAPI/CapNhatDanhMuc/5
        [HttpPut]
        [Route("api/DanhMucAPI/CapNhatDanhMuc/{id}")]
        public IHttpActionResult CapNhatDanhMuc(int id, DanhMuc danhMuc)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id != danhMuc.MADM) return BadRequest("ID không khớp");

            // Đánh dấu đối tượng là đã sửa đổi
            db.Entry(danhMuc).State = System.Data.Entity.EntityState.Modified;

            try
            {
                db.SaveChanges();
                return Ok(new { Success = true, Message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // API Xóa 1 danh mục có kiểm tra ràng buộc
        // URL: /api/DanhMucAPI/XoaDanhMuc/5
        [HttpDelete]
        [Route("api/DanhMucAPI/XoaDanhMuc/{id}")]
        public IHttpActionResult XoaDanhMuc(int id)
        {
            var danhMuc = db.DanhMucs.Find(id);
            if (danhMuc == null) return NotFound();

            // KIỂM TRA RÀNG BUỘC (Logic giống MVC)
            bool coSanPham = db.SanPhams.Any(x => x.MADM == id);
            bool coDanhMucCon = db.DanhMucs.Any(x => x.MADM_CHA == id);

            if (coSanPham)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = $"Không thể xóa! Danh mục #{id} đang chứa sản phẩm." });
            }
            if (coDanhMucCon)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = $"Không thể xóa! Danh mục #{id} là cha của danh mục khác." });
            }

            try
            {
                db.DanhMucs.Remove(danhMuc);
                db.SaveChanges();
                return Ok(new { Success = true, Message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // API Xóa nhiều danh mục
        // URL: /api/DanhMucAPI/XoaNhieuDanhMuc
        [HttpPost]
        [Route("api/DanhMucAPI/XoaNhieuDanhMuc")]
        public IHttpActionResult XoaNhieuDanhMuc(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return BadRequest("Chưa chọn danh mục nào.");

            var danhSachDaXoa = new List<int>();
            var danhSachLoi = new List<int>();

            foreach (var id in ids)
            {
                var danhMuc = db.DanhMucs.Find(id);
                if (danhMuc != null)
                {
                    bool coSanPham = db.SanPhams.Any(x => x.MADM == id);
                    bool coDanhMucCon = db.DanhMucs.Any(x => x.MADM_CHA == id);

                    if (coSanPham || coDanhMucCon)
                    {
                        danhSachLoi.Add(id);
                    }
                    else
                    {
                        try
                        {
                            db.DanhMucs.Remove(danhMuc);
                            db.SaveChanges();
                            danhSachDaXoa.Add(id);
                        }
                        catch
                        {
                            danhSachLoi.Add(id);
                        }
                    }
                }
            }

            // Trả về kết quả tổng hợp
            return Ok(new
            {
                Success = true,
                DeletedCount = danhSachDaXoa.Count,
                ErrorCount = danhSachLoi.Count,
                DeletedIds = danhSachDaXoa,
                ErrorIds = danhSachLoi
            });
        }

        //// PUT: api/DanhMucAPI/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutDanhMuc(int id, DanhMuc danhMuc)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != danhMuc.MADM)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(danhMuc).State = System.Data.Entity.EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!DanhMucExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/DanhMucAPI
        //[ResponseType(typeof(DanhMuc))]
        //public IHttpActionResult PostDanhMuc(DanhMuc danhMuc)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.DanhMucs.Add(danhMuc);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = danhMuc.MADM }, danhMuc);
        //}

        //// DELETE: api/DanhMucAPI/5
        //[ResponseType(typeof(DanhMuc))]
        //public IHttpActionResult DeleteDanhMuc(int id)
        //{
        //    DanhMuc danhMuc = db.DanhMucs.Find(id);
        //    if (danhMuc == null)
        //    {
        //        return NotFound();
        //    }

        //    db.DanhMucs.Remove(danhMuc);
        //    db.SaveChanges();

        //    return Ok(danhMuc);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool DanhMucExists(int id)
        //{
        //    return db.DanhMucs.Count(e => e.MADM == id) > 0;
        //}
    }
}