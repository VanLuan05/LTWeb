using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BanHoa.Models
{
    public class TrangChu
    {
        public List<SanPham> BoHoaReHomNay { get; set; }
        public List<SanPham> BoHoaTuoiHomNay { get; set; }
        public List<SanPham> GardenStyle { get; set; }
        public List<SanPham> HoaTulipSangTrong { get; set; }
        public List<SanPham> HoaTuoiUaChuong { get; set; }
        public List<SanPham> HopHoaMicaXinh { get; set; }
    }
}