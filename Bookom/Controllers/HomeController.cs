using Bookom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bookom.Controllers
{
    public class HomeController : Controller
    {
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();
        public ActionResult Index()
        {
            // Lấy 5 cuốn sách mới nhất đưa ra trang chủ (Ví dụ)
            var sachMoi = data.SACHes.OrderByDescending(s => s.MASACH).Take(5).ToList();
            return View(sachMoi);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Giới thiệu về nhà sách Bookom.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Trang liên hệ.";
            return View();
        }

        // Chức năng hiển thị sản phẩm theo danh mục
        public ActionResult SanPham(int? maTL, int? maNSX)
        {
            // 1. Danh sách thể loại
            ViewBag.TheLoai = data.THELOAIs.ToList();

            // 2. Lấy toàn bộ sách (Chưa lọc)
            var ds = data.SACHes.AsQueryable();
            // AsQueryable(): Tạo ra một câu lệnh "chờ".
            // Nghĩa là: Nó chưa chạy xuống Database lấy dữ liệu ngay lập tức.
            // Tác dụng: Để mình có thể gắn thêm các điều kiện .Where() (lọc) ở bên dưới.
            // Khi nào chốt xong hết điều kiện (gặp lệnh .ToList()) thì nó mới chạy 1 lần duy nhất để lấy đúng dữ liệu cần thiết.

            // 3. Lọc theo THỂ LOẠI (Nếu URL có ?maTL=...)
            if (maTL != null)
            {
                ds = ds.Where(s => s.MATL == maTL);
            }

            // 4. Lọc theo NHÀ XUẤT BẢN
            if (maNSX != null)
            {
                ds = ds.Where(s => s.MANSX == maNSX);
            }

            // 5. Trả về kết quả cuối cùng
            return View(ds.ToList());
        }

        public ActionResult ChiTiet(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var sach = data.SACHes.Find(id);
            if (sach == null)
            {
                return HttpNotFound(); 
            }

            return View(sach);
        }
        public ActionResult TimKiem(string tuKhoa)
        {
            ViewBag.TheLoai = data.THELOAIs.ToList();

            // Tìm sách theo tên (gần đúng)
            var sach = data.SACHes.Where(s => s.TENSACH.Contains(tuKhoa)).ToList();

            return View("SanPham", sach);
        }
    }
}