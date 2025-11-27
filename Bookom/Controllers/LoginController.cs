using System;
using System.Linq;
using System.Web.Mvc;
using Bookom.Models;

namespace Bookom.Controllers
{
    public class LoginController : Controller
    {
        // Lưu ý: Kiểm tra lại tên 'QL_NHASACHEntities...' xem đúng với máy bạn chưa nhé
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();

        // -------------------- LOGIN ----------------
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            // Tìm tài khoản
            var user = data.ACCOUNTs.FirstOrDefault(u => u.EMAIL == email && u.PASSWORD == password);

            if (user != null)
            {
                // [SỬA LẠI QUAN TRỌNG]: 
                // Session["UserName"] phải lưu EMAIL (là cái duy nhất) để các trang khác dùng để tìm kiếm
                Session["UserName"] = user.EMAIL;

                // Session["Name"] dùng để hiển thị "Xin chào..."
                Session["Name"] = user.NAME;

                // Phân quyền
                if (user.TYPEACCOUNT == "Admin")
                    return RedirectToAction("Index", "Admin"); // Chỉnh lại Controller Admin nếu cần
                else if (user.TYPEACCOUNT == "NhanVien")
                    return RedirectToAction("Index", "NhanVien"); // Chỉnh lại Controller NV nếu cần
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng!";
                return View();
            }
        }

        // --------------------- REGISTER ----------------
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(ACCOUNT acc)
        {
            // 1. Kiểm tra dữ liệu nhập
            if (string.IsNullOrEmpty(acc.EMAIL) || string.IsNullOrEmpty(acc.PASSWORD) || string.IsNullOrEmpty(acc.NAME))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View(acc);
            }

            // 2. Kiểm tra email đã tồn tại chưa
            var existing = data.ACCOUNTs.FirstOrDefault(a => a.EMAIL == acc.EMAIL);
            if (existing != null)
            {
                ViewBag.Error = "Email này đã được sử dụng, vui lòng chọn email khác!";
                return View(acc);
            }

            // 3. Kiểm tra số điện thoại trùng (Nếu DB có ràng buộc Unique SDT)
            var checkSDT = data.ACCOUNTs.FirstOrDefault(a => a.SDT == acc.SDT);
            if (checkSDT != null)
            {
                ViewBag.Error = "Số điện thoại này đã được đăng ký!";
                return View(acc);
            }

            try
            {
                // [BƯỚC 1]: TẠO TÀI KHOẢN (ACCOUNT)
                acc.TYPEACCOUNT = "KhachHang";
                data.ACCOUNTs.Add(acc);
                data.SaveChanges(); // Lưu ngay để sinh ra MAACCOUNT

                // [BƯỚC 2 - QUAN TRỌNG]: TẠO LUÔN THÔNG TIN KHÁCH HÀNG (KHACHHANG)
                // Nếu thiếu bước này, lúc mua hàng sẽ bị lỗi Null
                KHACHHANG kh = new KHACHHANG();
                kh.TENKH = acc.NAME;
                kh.EMAIL = acc.EMAIL;
                kh.SDT = acc.SDT;
                kh.DIACHI = acc.DIACHI;
                kh.MAACCOUNT = acc.MAACCOUNT; // Liên kết 2 bảng với nhau

                data.KHACHHANGs.Add(kh);
                data.SaveChanges();

                ViewBag.Success = "Đăng ký thành công! Mời bạn đăng nhập.";
                return View(); // Trả về View để hiện thông báo thành công
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(acc);
            }
        }

        // --------------------- LOGOUT ----------------
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa sạch session
            return RedirectToAction("Login");
        }
    }
}