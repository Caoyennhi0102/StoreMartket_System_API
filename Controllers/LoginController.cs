using StoreMartket_System_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace StoreMartket_System_API.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlConnectionServer _sqlConnectionDatabase;
        // GET: Login
        public LoginController()
        {
            _sqlConnectionDatabase = new SqlConnectionServer();
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("Login");
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không được để trống." });
            }
            var user = _sqlConnectionDatabase.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            if (user.Locked == false)
            {
                return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau." });
            }
            int failedAttempts = Session["FailedLoginAttempts"] != null ? (int)Session["FailedLoginAttempts"] : 0;
            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                failedAttempts++;
                Session["FailedLoginAttempts"] = failedAttempts;  // Lưu số lần đăng nhập sai vào session
                if (failedAttempts >= 3)
                {
                    user.Locked = false;
                    _sqlConnectionDatabase.SaveChanges();
                    return Json(new { success = false, message = "Mật khẩu không đúng. Bạn đã thử sai 3 lần. Tài khoản của bạn đã bị khóa." });

                }
                return Json(new { success = false, message = "Mật khẩu không đúng. Bạn đã thử sai " + failedAttempts + " lần." });
            }
            Session["FailedLoginAttempts"] = 0;
            string getClientIp = GetClientIp();
            user.DiaChiIP = getClientIp;
            user.Locked = false;
            user.TGDangNhap = DateTime.Now;
            _sqlConnectionDatabase.SaveChanges();

            Session["HoTen"] = user.NhanVien.HoTen;
            FormsAuthentication.SetAuthCookie(username, false);
            if (user.DNLanDau == true)
            {
                Session["UserId"] = user.UserID;
                return RedirectToAction("ChangeCredentials", "ChangeCredentials");
            }
            var userRole = GetUserRoles(username);
            if (userRole == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (userRole == "Manager")
            {
                return RedirectToAction("Dashboard", "Manager");
            }
            else
            {
                return RedirectToAction("Dashboard", "User");
            }




        }
        // Phương thức xác thực mật khẩu
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash, string storedPasswordSalt)
        {
            var saltBytes = Convert.FromBase64String(storedPasswordSalt);
            var hashBytes = Convert.FromBase64String(storedPasswordHash);

            using (var hmac = new HMACSHA256(saltBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));

                return computedHash.SequenceEqual(hashBytes);
            }
        }
        private string GetClientIp()
        {
            var ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.UserHostAddress;
            }
            return ipAddress;
        }
        public User GetUserByUserName(string userName)
        {
            return _sqlConnectionDatabase.Users.FirstOrDefault(u => u.UserName == userName);
        }
        public string GetUserRoles(string username)
        {
            var user = GetUserByUserName(username);
            return user != null ? user.Roles : string.Empty;
        }
    }
}