using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BanHoa.Models
{
    public class DashboardViewModel
    {
        public int SoLuongDonHang { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int SoLuongSanPham { get; set; }
        public int SoLuongKhachHang { get; set; }

        // Danh sách 5 đơn hàng mới nhất
        public List<DonHang> DonHangMoiNhat { get; set; }

        // --- PHẦN NÀY CHO BIỂU ĐỒ ---
        public List<decimal> ChartDoanhThu { get; set; } // Dữ liệu cột doanh thu
        public List<decimal> ChartLoiNhuan { get; set; } // Dữ liệu cột lợi nhuận
        public List<string> ChartLabels { get; set; }    // Tên tháng (T1, T2...)
    }
}