using Bookom.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Bookom.Controllers
{
    public class MoMoController : Controller
    {
        QL_NHASACHEntities6 data = new QL_NHASACHEntities6();

        public ActionResult Payment()
        {
            // 1. Fix lỗi TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // 2. DÙNG CẶP KEY TEST MỚI (Ổn định hơn)
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMOBKUN20180529";
            string accessKey = "klm05TvNBzhg7h7j";
            string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";

            // 3. Thông tin đơn hàng
            string orderInfo = "Thanh toan Bookom"; // Không dấu, không ký tự lạ

            // Sửa lại Port của bạn (44392)
            string redirectUrl = "https://localhost:44392/MoMo/ConfirmPaymentClient";
            string ipnUrl = "https://localhost:44392/MoMo/SavePayment";
            string requestType = "captureWallet";

            string orderId = "Bookom" + DateTime.Now.Ticks.ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // 4. Tính tổng tiền (Ép kiểu long)
            double tongTien = 0;
            if (Session["GioHang"] != null)
            {
                var lstGioHang = Session["GioHang"] as System.Collections.Generic.List<GioHang>;
                foreach (var item in lstGioHang) tongTien += item.dThanhTien;
            }
            string amount = ((long)(tongTien > 0 ? tongTien : 10000)).ToString();

            // 5. TẠO CHỮ KÝ (SIGNATURE) - ĐÚNG CHUẨN MOMO
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            string signature = ComputeHmacSha256(rawHash, secretKey);

            // 6. TẠO JSON
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Bookom" },
                { "storeId", "BookomStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "vi" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }
            };

            // 7. GỬI REQUEST
            string responseFromMomo = SendPaymentRequest(endpoint, message.ToString());

            // 8. XỬ LÝ KẾT QUẢ
            JObject jmessage = JObject.Parse(responseFromMomo);

            if (jmessage.GetValue("payUrl") != null)
            {
                return Redirect(jmessage.GetValue("payUrl").ToString());
            }
            else
            {
                ViewBag.Error = "Lỗi MoMo: " + jmessage.GetValue("message") + " (" + jmessage.GetValue("localMessage") + ")";
                ViewBag.TongTien = tongTien; // Trả lại tiền để View không lỗi
                return View("~/Views/GioHang/GioHang.cshtml", Session["GioHang"]);
            }
        }

        public ActionResult ConfirmPaymentClient()
        {
            // Code xử lý kết quả giữ nguyên như cũ
            string errorCode = Request.QueryString["errorCode"];
            string orderId = Request.QueryString["orderId"];
            if (errorCode == "0")
            {
                Session["GioHang"] = null;
                ViewBag.Mess = "Thanh toán thành công!";
                return RedirectToAction("XacNhanDonHang", "GioHang");
            }
            else
            {
                ViewBag.Mess = "Thanh toán thất bại (Mã lỗi: " + errorCode + ")";
                return RedirectToAction("GioHang", "GioHang");
            }
        }

        // --- HÀM HASH MỚI (An toàn hơn) ---
        public string ComputeHmacSha256(string message, string secretKey)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmac = new HMACSHA256(keyByte))
            {
                byte[] hashMessage = hmac.ComputeHash(messageBytes);
                var sb = new StringBuilder();
                foreach (var b in hashMessage)
                {
                    sb.Append(b.ToString("x2")); // Chuyển sang Hex thường
                }
                return sb.ToString();
            }
        }

        // --- HÀM GỬI REQUEST (Giữ nguyên vì đã chuẩn) ---
        private string SendPaymentRequest(string endpoint, string postJsonString)
        {
            try
            {
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(endpoint);
                var postData = Encoding.UTF8.GetBytes(postJsonString);
                httpWReq.ProtocolVersion = HttpVersion.Version11;
                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/json; charset=UTF-8";
                httpWReq.ContentLength = postData.Length;

                using (var stream = httpWReq.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var errorReader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        return errorReader.ReadToEnd();
                    }
                }
                return "{\"message\":\"" + e.Message + "\"}";
            }
        }
    }
}