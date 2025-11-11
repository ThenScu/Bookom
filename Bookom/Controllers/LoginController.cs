using System;
using System.Linq;
using System.Web.Mvc;
using Bookom.Models;

namespace Bookom.Controllers
{
    public class LoginController : Controller
    {
        // Kết nối database
        QL_NHASACHEntities3 data = new QL_NHASACHEntities3();

        // -------------------- LOGIN ----------------
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            // Tìm tài khoản có email & password trùng
            var user = data.ACCOUNTs.FirstOrDefault(u => u.EMAIL == email && u.PASSWORD == password);

            if (user != null)
            {
                // Lưu thông tin đăng nhập vào Session
                Session["UserName"] = user.NAME;
                Session["UserEmail"] = user.EMAIL;
                Session["UserType"] = user.TYPEACCOUNT;

                // Điều hướng theo loại tài khoản
                if (user.TYPEACCOUNT == "Admin")
                    return RedirectToAction("Index", "Admin");
                else if (user.TYPEACCOUNT == "NhanVien")
                    return RedirectToAction("Index", "NhanVien");
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                // Sai tài khoản hoặc mật khẩu
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
            // Kiểm tra trống
            if (string.IsNullOrEmpty(acc.EMAIL) || string.IsNullOrEmpty(acc.PASSWORD) || string.IsNullOrEmpty(acc.NAME))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View(acc);
            }

            // Kiểm tra email trùng
            var existing = data.ACCOUNTs.FirstOrDefault(a => a.EMAIL == acc.EMAIL);
            if (existing != null)
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View(acc);
            }

            // Thêm tài khoản mới
            acc.TYPEACCOUNT = "KhachHang";
            data.ACCOUNTs.Add(acc);
            data.SaveChanges();

            ViewBag.Success = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        // --------------------- LOGOUT ----------------
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
