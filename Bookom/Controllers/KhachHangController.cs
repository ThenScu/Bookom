using Bookom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Bookom.Controllers
{
    public class KhachHangController : Controller
    {
        // GET: KhachHang
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();
        // GET: Chỉnh sửa thông tin
        public ActionResult UserInfo()
        {
            if (Session["UserEmail"] == null)
                return RedirectToAction("Login", "Login");

            var email = Session["UserEmail"].ToString();
            var user = data.ACCOUNTs.FirstOrDefault(u => u.EMAIL == email);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }
        public ActionResult Edit()
        {
            if (Session["UserEmail"] == null)
                return RedirectToAction("Login", "Login");

            var email = Session["UserEmail"].ToString();
            var user = data.ACCOUNTs.FirstOrDefault(u => u.EMAIL == email);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        public ActionResult Edit(ACCOUNT model)
        {
            var user = data.ACCOUNTs.FirstOrDefault(u => u.EMAIL == model.EMAIL);
            if (user == null)
                return HttpNotFound();

            // Cập nhật thông tin
            user.NAME = model.NAME;
            user.SDT = model.SDT;
            user.DIACHI = model.DIACHI;

            data.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("UserInfo");
        }

        public ActionResult LichSuDonHang()
        {
            // Kiểm tra đăng nhập
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Lấy thông tin tài khoản đang đăng nhập
            // Giả sử Session["User"] chứa object Account, hoặc bạn query lại từ UserName
            // Cách an toàn nhất là query lại từ Session["UserName"]
            string email = Session["UserName"].ToString();
            var account = data.ACCOUNTs.FirstOrDefault(s => s.EMAIL == email || s.NAME == email); // Sửa logic này tùy theo lúc login bạn lưu cái gì

            if (account == null) return RedirectToAction("Login", "Login");

            // Từ Account -> Tìm Khách hàng
            // (Vì bảng HOADON lưu MAKH chứ không lưu MAACCOUNT trực tiếp trong sơ đồ cũ của bạn)
            var kh = data.KHACHHANGs.FirstOrDefault(k => k.MAACCOUNT == account.MAACCOUNT);

            if (kh == null)
            {
                // Trường hợp tài khoản Admin hoặc chưa có thông tin khách hàng
                ViewBag.ThongBao = "Bạn chưa có lịch sử mua hàng.";
                return View(new List<HOADON>());
            }

            // Lấy danh sách hóa đơn của khách hàng này (Sắp xếp ngày mới nhất lên đầu)
            var listDonHang = data.HOADONs.Where(n => n.MAKH == kh.MAKH).OrderByDescending(n => n.NGAYLAP).ToList();

            return View(listDonHang);
        }

        // 2. Xem chi tiết cụ thể 1 đơn hàng (Bấm vào xem nó mua sách gì)
        public ActionResult ChiTietDonHang(int id)
        {
            if (Session["UserName"] == null) return RedirectToAction("Login", "Login");

            // Tìm đơn hàng theo Mã Hóa Đơn (MAHD)
            var donhang = data.HOADONs.FirstOrDefault(n => n.MAHD == id);

            if (donhang == null) return HttpNotFound();

            // Lấy danh sách chi tiết (Sách gì, bao nhiêu cuốn)
            // Code này dùng tính năng Lazy Loading của Entity Framework (CT_HOADON có sẵn trong HOADON)
            return View(donhang);
        }
    }
}