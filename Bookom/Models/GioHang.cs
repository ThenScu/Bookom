using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bookom.Models
{
    public class GioHang
    {
        // Khai báo db để lấy thông tin sách
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();

        public int iMaSach { get; set; }
        public string sTenSach { get; set; }
        public string sAnhBia { get; set; }
        public Double dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public Double dThanhTien
        {
            get { return iSoLuong * dDonGia; }
        }

        // Hàm tạo cho giỏ hàng (Lấy thông tin từ Database ném vào đây)
        public GioHang(int MaSach)
        {
            iMaSach = MaSach;
            SACH sach = data.SACHes.Single(n => n.MASACH == iMaSach);
            sTenSach = sach.TENSACH;
            sAnhBia = sach.ANHBIA;
            dDonGia = double.Parse(sach.GIA.ToString());
            iSoLuong = 1;
        }
    }
}