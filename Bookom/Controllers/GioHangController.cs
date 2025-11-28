using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bookom.Models; 

namespace Bookom.Controllers
{
    public class GioHangController : Controller
    {
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();

        // 1. Lấy giỏ hàng từ Session
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                // Nếu chưa có giỏ hàng thì tạo mới
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        // 2. Thêm sản phẩm vào giỏ
        public ActionResult ThemGioHang(int maSach, string strURL)
        {
            // Lấy giỏ hàng hiện tại
            List<GioHang> lstGioHang = LayGioHang();

            // Kiểm tra sách này đã có trong giỏ chưa
            GioHang sanpham = lstGioHang.Find(n => n.iMaSach == maSach);

            if (sanpham == null)
            {
                // Chưa có thì tạo mới và thêm vào list
                sanpham = new GioHang(maSach);
                lstGioHang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                // Có rồi thì tăng số lượng
                sanpham.iSoLuong++;
                return Redirect(strURL);
            }
        }

        // 3. Tính tổng số lượng và tổng tiền
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSoLuong;
        }

        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return dTongTien;
        }

        // 4. Hiển thị trang Giỏ hàng
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }

        // 5. Xóa giỏ hàng
        public ActionResult XoaGioHang(int iMaSP)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanpham = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSP);
            if (sanpham != null)
            {
                lstGioHang.Remove(sanpham);
                if (lstGioHang.Count == 0)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("GioHang");
        }

        // 6. Cập nhật giỏ hàng
        public ActionResult CapNhatGioHang(int iMaSP, FormCollection f)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanpham = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSP);
            if (sanpham != null)
            {
                // Lấy số lượng từ form nhập liệu
                sanpham.iSoLuong = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }

        // 7. Hiển thị form Đặt Hàng
        [HttpGet]
        public ActionResult DatHang()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["UserName"] == null || Session["UserName"].ToString() == "")
            {
                return RedirectToAction("Login", "Login");
            }

            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 2. Lấy thông tin khách hàng để điền sẵn vào form
            var email = Session["UserName"].ToString();
            var account = data.ACCOUNTs.FirstOrDefault(a => a.EMAIL == email);
            var kh = data.KHACHHANGs.FirstOrDefault(k => k.MAACCOUNT == account.MAACCOUNT);

            // Gửi thông tin sang View qua ViewBag
            if (kh != null)
            {
                ViewBag.TenKH = kh.TENKH;
                ViewBag.SDT = kh.SDT;
                ViewBag.DiaChi = kh.DIACHI;
            }
            else
            {
                // Trường hợp chưa có thông tin thì để trống
                ViewBag.TenKH = "";
                ViewBag.SDT = "";
                ViewBag.DiaChi = "";
            }

            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }

        // 8. Xử lý Đặt Hàng (Lưu vào SQL)
        // [HttpPost] Đặt hàng: LƯU VÀO SQL
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            // 1. Kiểm tra giỏ hàng
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang == null || lstGioHang.Count == 0)
                return RedirectToAction("GioHang");

            // 2. Lấy thông tin khách hàng (Logic cũ của bạn)
            var emailUser = Session["UserName"].ToString();
            var account = data.ACCOUNTs.FirstOrDefault(a => a.EMAIL == emailUser);
            var khachHang = data.KHACHHANGs.FirstOrDefault(k => k.MAACCOUNT == account.MAACCOUNT);

            if (khachHang == null) return RedirectToAction("Login", "Login");

            // 3. TẠO VÀ LƯU HÓA ĐƠN
            HOADON ddh = new HOADON();
            ddh.MAKH = khachHang.MAKH;
            ddh.NGAYLAP = DateTime.Now;
            ddh.TINHTRANG = 0; // 0: Chờ duyệt

            data.HOADONs.Add(ddh);
            data.SaveChanges(); // Lưu xong để lấy được MAHD

            // 4. LƯU CHI TIẾT HÓA ĐƠN
            foreach (var item in lstGioHang)
            {
                CT_HOADON cthd = new CT_HOADON();
                cthd.MAHD = ddh.MAHD;
                cthd.MASACH = item.iMaSach;
                cthd.SOLUONG = item.iSoLuong;
                cthd.DONGIA = (decimal)item.dDonGia;

                data.CT_HOADON.Add(cthd);
            }
            data.SaveChanges();

            // =========================================================================
            // KIỂM TRA NÚT BẤM ĐỂ CHUYỂN HƯỚNG
            // =========================================================================

            string paymentMethod = collection["paymentMethod"]; // Lấy giá trị từ nút bấm (TienMat hoặc MoMo)

            if (paymentMethod == "MoMo")
            {
                // Nếu chọn MoMo: KHÔNG xóa giỏ hàng vội, chuyển sang trang thanh toán MoMo
                return RedirectToAction("Payment", "MoMo");
            }
            else
            {
                // Nếu chọn Tiền Mặt: Xóa giỏ hàng và hiện thông báo thành công luôn
                Session["GioHang"] = null;
                return RedirectToAction("XacNhanDonHang", "GioHang");
            }
        }

        public ActionResult XacNhanDonHang()
        {
            return View();
        }
    }
}