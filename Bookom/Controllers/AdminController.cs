using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bookom.Models;
using System.Data.Entity; 

namespace Bookom.Controllers
{
    public class AdminController : Controller
    {
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();

        // 1. DASHBOARD
        public ActionResult Index()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            ViewBag.SoLuongSach = data.SACHes.Count();
            ViewBag.SoLuongDonHang = data.HOADONs.Count();
            ViewBag.DonChuaDuyet = data.HOADONs.Count(n => n.TINHTRANG == 0);

            return View();
        }

        // 2. QUẢN LÝ ĐƠN HÀNG
        public ActionResult QuanLyDonHang()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            var dsDonHang = data.HOADONs.OrderByDescending(n => n.NGAYLAP).ToList();
            return View(dsDonHang);
        }

        // 3. DUYỆT ĐƠN
        // 3. XÁC NHẬN ĐƠN HÀNG (Duyệt đơn)
        public ActionResult DuyetDon(int id)
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            // Lấy đơn hàng và TẢI LUÔN danh sách Chi Tiết đơn hàng (CT_HOADON) kèm theo
            // Đảm bảo bạn đã thêm 'using System.Data.Entity;' ở đầu file nhé!
            var donhang = data.HOADONs
                .Include(n => n.CT_HOADON)
                .FirstOrDefault(n => n.MAHD == id);


            // [ĐÃ SỬA LỖI Ở ĐÂY] Kiểm tra đơn hàng tồn tại VÀ trạng thái là 0 hoặc NULL
            // Vì TINHTRANG có thể là NULL cho đơn hàng mới.
            if (donhang != null && (donhang.TINHTRANG == 0 || donhang.TINHTRANG == null))
            {
                // 1. CHUYỂN TRẠNG THÁI sang 1 (Đã duyệt)
                donhang.TINHTRANG = 1;

                // 2. TRỪ KHO SÁCH
                foreach (var chiTiet in donhang.CT_HOADON)
                {
                    var sach = data.SACHes.FirstOrDefault(s => s.MASACH == chiTiet.MASACH);

                    if (sach != null)
                    {
                        // Giảm số lượng trong kho
                        sach.SOLUONG -= chiTiet.SOLUONG ?? 0;

                        // Đảm bảo số lượng không âm 
                        if (sach.SOLUONG < 0)
                        {
                            sach.SOLUONG = 0;
                        }
                    }
                }

                // 3. LƯU TẤT CẢ THAY ĐỔI (Trạng thái đơn + Số lượng sách)
                data.SaveChanges();
            }

            // Sau khi duyệt xong, chuyển về trang Quản lý đơn hàng
            return RedirectToAction("QuanLyDonHang");
        }

        // ---------------------------------------------------------
        // PHẦN QUẢN LÝ SÁCH
        // ---------------------------------------------------------

        // 4. XEM DANH SÁCH SÁCH
        public ActionResult QuanLySach()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            // BƯỚC 2: Thêm .Include để lấy tên NXB và Tên Thể Loại
            var sach = data.SACHes.Include("NHASANXUAT").Include("THELOAI").OrderByDescending(s => s.MASACH).ToList();

            return View(sach);
        }

        // 5. XÓA SÁCH
        public ActionResult XoaSach(int id)
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            var sach = data.SACHes.FirstOrDefault(s => s.MASACH == id);
            if (sach != null)
            {
                data.SACHes.Remove(sach);
                data.SaveChanges();
            }
            return RedirectToAction("QuanLySach");
        }

        // 6. THÊM SÁCH MỚI
        [HttpGet]
        public ActionResult ThemSach()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList(), "MANSX", "TENNSX");
            ViewBag.MaTL = new SelectList(data.THELOAIs.ToList(), "MATL", "TENTL");

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemSach(SACH sach, HttpPostedFileBase fileUpload)
        {
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList(), "MANSX", "TENNSX");
            ViewBag.MaTL = new SelectList(data.THELOAIs.ToList(), "MATL", "TENTL");

            if (fileUpload != null)
            {
                // ... (Code xử lý ảnh giữ nguyên)
                var fileName = System.IO.Path.GetFileName(fileUpload.FileName);
                var path = System.IO.Path.Combine(Server.MapPath("~/Content/img"), fileName);
                fileUpload.SaveAs(path);
                sach.ANHBIA = fileName;
            }
            else
            {
                sach.ANHBIA = "logo.png";
            }

            data.SACHes.Add(sach);
            try
            {
                data.SaveChanges();
            }
            catch (Exception ex)
            {
                // [QUAN TRỌNG]: Nếu lỗi, hãy báo lỗi và return View(sach) để form giữ lại dữ liệu
                ViewBag.Error = "Lỗi khi lưu Database: " + ex.InnerException?.InnerException?.Message;
                return View(sach);
            }


            // Bước 3: Xong thì quay về danh sách
            return RedirectToAction("QuanLySach");
        }

        // 7. SỬA SÁCH 
        [HttpGet]
        public ActionResult SuaSach(int id)
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
                return RedirectToAction("Login", "Login");

            // 1. Lấy cuốn sách cần sửa
            SACH sach = data.SACHes.SingleOrDefault(n => n.MASACH == id);
            if (sach == null) return HttpNotFound();

            // 2. Đưa dữ liệu vào DropdownList (và chọn sẵn giá trị cũ)
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList(), "MANSX", "TENNSX", sach.MANSX);
            ViewBag.MaTL = new SelectList(data.THELOAIs.ToList(), "MATL", "TENTL", sach.MATL);

            return View(sach);
        }

        // 8. SỬA SÁCH (LƯU DỮ LIỆU)
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaSach(SACH sach, HttpPostedFileBase fileUpload)
        {
            // B1: Đảm bảo Dropdownlist vẫn có dữ liệu (phòng khi lỗi)
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList(), "MANSX", "TENNSX", sach.MANSX);
            ViewBag.MaTL = new SelectList(data.THELOAIs.ToList(), "MATL", "TENTL", sach.MATL);

            // B2: Tìm sách cũ trong DB (để cập nhật trạng thái Entity)
            var sachDB = data.SACHes.FirstOrDefault(s => s.MASACH == sach.MASACH);

            if (sachDB != null)
            {
                // B3: Cập nhật các trường thông tin cơ bản
                sachDB.TENSACH = sach.TENSACH;
                sachDB.GIA = sach.GIA;
                sachDB.SOLUONG = sach.SOLUONG;
                sachDB.TACGIA = sach.TACGIA;
                sachDB.MANSX = sach.MANSX;
                sachDB.MATL = sach.MATL;

                // B4: Xử lý ảnh (Nếu có chọn ảnh mới thì cập nhật, không thì giữ nguyên ảnh cũ)
                if (fileUpload != null)
                {
                    var fileName = System.IO.Path.GetFileName(fileUpload.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/img"), fileName);
                    fileUpload.SaveAs(path);
                    sachDB.ANHBIA = fileName; // Gán tên ảnh mới
                }

                try
                {
                    data.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi khi cập nhật Database: " + ex.InnerException?.InnerException?.Message;
                    return View(sach);
                }
            }
            return RedirectToAction("QuanLySach");
        }
    }
}