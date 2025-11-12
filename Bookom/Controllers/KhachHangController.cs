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
        QL_NHASACHEntities3 data = new QL_NHASACHEntities3();
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

    }
}