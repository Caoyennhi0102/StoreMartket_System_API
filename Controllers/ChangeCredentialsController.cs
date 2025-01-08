using StoreMartket_System_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace StoreMartket_System_API.Controllers
{
    public class ChangeCredentialsController : Controller
    {
        // GET: ChangeCredentials
        [HttpGet]
        public ActionResult ChangeCredentials()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeCredentials(string newUserName, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newUserName) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new { success = false, message = "Không được để trống thông tin." });
            }
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction("ChangeCredentials");
            }

            var userID = Session["UserID"] as int?;
            if (userID == null)
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }
            using (var DB = new SqlConnectionServer())
            {
                var user = DB.Users.Find(userID);
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                user.UserName = newUserName;

                var salt = GenerateSalt();
                user.PasswordSalt = Convert.ToBase64String(salt);
                user.PasswordHash = HashPassword(newPassword, salt);

                user.DNLanDau = false;
                DB.SaveChanges();
            }
            Session["UserId"] = null;
            return RedirectToAction("Login", "Login");
        }
        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }
        private string HashPassword(string password, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
