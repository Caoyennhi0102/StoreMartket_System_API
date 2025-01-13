using Microsoft.Win32;
using StoreMartket_System_API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;

namespace StoreMartket.Controllers
{
    // Đánh dấu phân quyền đăng nhập Admin
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {


        private readonly SqlConnectionServer _sqlConnectionserver;

        public AdminController()
        {
            _sqlConnectionserver = new SqlConnectionServer();
        }

        /*
        protected override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
        {
            if (Session["UserName"] == null)
            {
                actionExecutingContext.Result = RedirectToAction("Login", "Login");
            }
            base.OnActionExecuting(actionExecutingContext);
        }*/
        public ActionResult Dashboard()
        {
            bool IsSessionActive = (Session["UserName"] != null);
            ViewBag.IsSessionActive = IsSessionActive;
            return View();
        }
        public ActionResult LogOut(User user)
        {
            FormsAuthentication.SignOut();
            user.TGDangXuat = DateTime.Now;
            _sqlConnectionserver.SaveChanges();
            Session.Clear();
            return RedirectToAction("Login", "Admin");
        }
        private int GenerateCuaHangId()
        {
            // Lấy mã cửa hàng cao nhất hiện tại
            var maxID = _sqlConnectionserver.CuaHangs.Select(CH => CH.MaCuaHang).DefaultIfEmpty(0).Max();

            // Tăng mã lên 1
            return maxID + 1;
        }

        public string GetFormattedCuaHangId()
        {
            int NewStoreID = GenerateCuaHangId();
            return "CH" + NewStoreID.ToString("D4");
        }

        // Phương thức kiểm tra địa  chỉ 
        public bool CheckAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                // Địa chỉ không được để trống
                return false;
            }
            if (address.Length < 10)
            {
                // Địa chỉ quá ngắn
                return false;
            }
            string allowedCharsPattern = @"^[a-zA-Z0-9\s,.-]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(address, allowedCharsPattern))
            {
                // Địa chỉ chứa ký tự không hợp lệ
                return false;
            }
            return true;

        }
        // Phương thức kiểm tra địa chỉ Email 
        public bool CheckEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
            {
                return false;
            }
            return true;
        }
        public bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }
            if (phoneNumber.Length < 10)
            {
                return false;
            }
            string pattern = @"^\d{10}$";
            return Regex.IsMatch(pattern, phoneNumber);
        }

        public bool CheckTaxCode(string taxCode)
        {
            if (string.IsNullOrEmpty(taxCode))
            {
                return false;
            }
            string pattern = @"^\d{10}$|^\d{14}$";
            return Regex.IsMatch(taxCode, pattern);
        }
        public string GenerateStoreEmail(string tenCH, int maCH)
        {
            string emailPrefix = tenCH.Trim().ToLower().Replace(" ", ".");
            string email = $"{emailPrefix}.{maCH}@company.com";
            return email;
        }
        [HttpGet]
        public JsonResult GetStoreList()
        {
            var stores = _sqlConnectionserver.CuaHangs.ToList();
            return Json(stores, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin
        [HttpGet]
        public ActionResult AddStore()
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddStore(string tenCH, string diaChi, string dienThoai, string email, string mst, int chTruong)
        {
            // Kiểm tra tên cửa hàng có để trống hay không 
            if (string.IsNullOrEmpty(tenCH))
            {
                return Json(new { success = false, message = "Tên cửa hàng không được để trống" });

            }
            if (!CheckAddress(diaChi))
            {
                return Json(new { success = false, message = "Địa chỉ không hợp lệ. Vui lòng kiểm tra lại." });
            }
            if (!CheckAddress(email))
            {
                return Json(new { success = false, message = "Địa chỉ email không hợp lệ. Vui lòng kiểm tra lại." });
            }
            if (!IsPhoneNumberValid(dienThoai))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ.Vui lòng kiểm tra lại." });
            }

            if (!CheckAddress(mst))
            {
                return Json(new { success = false, message = "Mã số thuế không hợp lệ .Vui lòng kiểm tra lại." });
            }
            var CHTruong = _sqlConnectionserver.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == chTruong);
            if (CHTruong == null)
            {
                return Json(new { success = false, message = "Mã nhân viên trưởng cửa hàng không hợp lệ." });
            }

            int maCH = GenerateCuaHangId();
            int soluongNhanVien = _sqlConnectionserver.NhanViens.Count(nv => nv.MaCuaHang == maCH);
            // Truyền dữ liệu qua ViewData
            ViewData["SLNV"] = soluongNhanVien;
            string generatedEmail = GenerateStoreEmail(tenCH, maCH);
            var newStore = new CuaHang
            {
                MaCuaHang = maCH,
                TenCH = tenCH,
                DiaChi = diaChi,
                DienThoai = dienThoai,
                Email = generatedEmail,
                MST = mst,
                CHTruong = chTruong,
                SLNV = soluongNhanVien
            };
            _sqlConnectionserver.CuaHangs.Add(newStore);
            _sqlConnectionserver.SaveChanges();

            return Json(new { success = true, message = "Thêm cửa hàng thành công", maCH = maCH });
        }
        [HttpGet]
        public ActionResult UpdateStore()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SearchStore(string maCH)
        {
            try
            {
                if (string.IsNullOrEmpty(maCH) || !int.TryParse(maCH, out var storeId))
                {
                    return Json(new { success = false, message = "Mã cửa hàng không hợp lệ." });
                }


                var store = _sqlConnectionserver.CuaHangs.FirstOrDefault(CH => CH.MaCuaHang == storeId);
                if (store == null)
                {
                    return Json(new { success = false, message = "Cửa hàng không tồn tại" });
                }
                // Trả về thông tin cửa hàng dưới dạng JSON
                return Json(new
                {
                    success = true,
                    store = new
                    {
                        store.MaCuaHang,
                        store.TenCH,
                        store.DiaChi,
                        store.DienThoai,
                        store.Email,
                        store.MST,
                        store.CHTruong,
                        store.SLNV
                    }
                });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình cập nhật. Vui lòng liên hệ với quản trị viên.{ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult UpdateStore(int? maCH, string tenCH, string diaChi, string dienThoai, string email, string mst, int? chTruong)
        {
            try
            {
                if (maCH == null)
                {
                    Console.WriteLine("Giá trị maCH: " + maCH);
                    return Json(new { success = false, message = "Mã cửa hàng không hợp lệ." });
                }

                if (_sqlConnectionserver == null || _sqlConnectionserver.CuaHangs == null)
                {
                    return Json(new { success = false, message = "Không thể truy cập dữ liệu cửa hàng. Vui lòng kiểm tra lại." });
                }
                var store = _sqlConnectionserver.CuaHangs.FirstOrDefault(CH => CH.MaCuaHang == maCH);
                if (store == null)
                {
                    return Json(new { success = false, message = "Cửa hàng không tồn tại" });
                }
                if (string.IsNullOrEmpty(tenCH))
                {
                    return Json(new { success = false, message = "Tên cửa hàng không được để trống" });

                }
                if (!CheckAddress(diaChi))
                {
                    return Json(new { success = false, message = "Địa chỉ không hợp lệ. Vui lòng kiểm tra lại." });
                }
                if (!CheckAddress(email))
                {
                    return Json(new { success = false, message = "Địa chỉ email không hợp lệ. Vui lòng kiểm tra lại." });
                }
                if (!IsPhoneNumberValid(dienThoai))
                {
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ.Vui lòng kiểm tra lại." });
                }

                if (!CheckAddress(mst))
                {
                    return Json(new { success = false, message = "Mã số thuế không hợp lệ .Vui lòng kiểm tra lại." });
                }

                if (chTruong == null || !_sqlConnectionserver.NhanViens.Any(nv => nv.MaNhanVien == chTruong.Value))
                {
                    return Json(new { success = false, message = "Mã nhân viên trưởng cửa hàng không hợp lệ." });
                }
                int soluongNhanVien = _sqlConnectionserver.NhanViens.Count(nv => nv.MaCuaHang == maCH);
                // Truyền dữ liệu qua ViewData
                ViewData["SLNV"] = soluongNhanVien;
                store.TenCH = tenCH;
                store.DiaChi = diaChi;
                store.DienThoai = dienThoai;
                store.Email = email;
                store.MST = mst;
                store.CHTruong = (int)chTruong;
                store.SLNV = soluongNhanVien;
                _sqlConnectionserver.SaveChanges();

                return Json(new { success = true, message = "Cập nhật cửa hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình cật nhật{ex.Message}" });
            }


        }

        [HttpGet]
        public ActionResult DeleteStore()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DeleteStore(int? maCH)
        {
            try
            {
                if (maCH == null)
                {
                    return Json(new { success = false, message = "Mã cửa hàng không hợp lệ." });
                }
                if (_sqlConnectionserver == null || _sqlConnectionserver.CuaHangs == null)
                {
                    return Json(new { success = false, message = "Không thể truy cập dữ liệu cửa hàng. Vui lòng kiểm tra lại." });
                }
                var store = _sqlConnectionserver.CuaHangs.Find(maCH);
                if (store != null)
                {
                    var deleteStore = _sqlConnectionserver.BoPhans.Where(u => u.MaCuaHang == maCH);


                    if (deleteStore.Any())
                    {

                        _sqlConnectionserver.BoPhans.RemoveRange(deleteStore);
                    }
                    var deleteStoreNV = _sqlConnectionserver.NhanViens.Where(u => u.MaCuaHang == maCH);
                    if (deleteStoreNV.Any())
                    {
                        _sqlConnectionserver.NhanViens.RemoveRange(deleteStoreNV);
                    }

                    _sqlConnectionserver.CuaHangs.Remove(store);
                    _sqlConnectionserver.SaveChanges();


                }
                return Json(new { success = false, message = "Cửa hàng không tồn tại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình xóa {ex.Message}" });
            }
        }

        public ActionResult GetStore()
        {
            try
            {
                var getStore = _sqlConnectionserver.CuaHangs.Select(Ch => new { Ch.MaCuaHang, Ch.TenCH }).ToList();
                return Json(getStore, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình gọi danh sách cửa hàng{ex.Message}" });
            }
        }
        public string CreateCodeDepartments(string TenBP)
        {
            if (string.IsNullOrEmpty(TenBP))
            {
                return "Tên bộ phận không được để trống.";

            }
            string[] from = TenBP.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string Firstcharacter = string.Concat(from.Select(t => t.ToString().ToUpper()));

            return $"BP_{Firstcharacter}";

        }
        [HttpGet]
        public ActionResult CreateDepartments()
        {
            var store = _sqlConnectionserver.CuaHangs.Select(ch => new { ch.MaCuaHang, ch.TenCH }).ToList();
            ViewBag.Store = new SelectList(store, "MaCuaHang", "TenCH");
            return View();
        }
        [HttpPost]
        public ActionResult CreateDepartments(string TenBP, int? maCH)
        {
            try
            {
                if (string.IsNullOrEmpty(TenBP) || maCH == null)
                {
                    return Json(new { success = false, message = "Tên bộ phận và mã cửa hàng không được để trống." });

                }
                string MaBP = CreateCodeDepartments(TenBP);
                var department = new BoPhan
                {
                    MaBoPhan = MaBP,
                    TenBP = TenBP,
                    MaCuaHang = (int)maCH,
                };
                _sqlConnectionserver.BoPhans.Add(department);
                _sqlConnectionserver.SaveChanges();
                return Json(new { success = true, message = "Thêm bộ phận thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình thêm bộ phận{ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult GetDepartments()
        {
            try
            {
                var departments = _sqlConnectionserver.BoPhans.Select(
                    bp => new
                    {
                        bp.MaBoPhan,
                        bp.TenBP,
                        bp.MaCuaHang
                    })
                    .ToList();
                return Json(departments, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi tải danh sách bộ phận: {ex.Message}" });
            }
        }
        [HttpPost]
        public ActionResult SearchDepartmentsByID(string MaBP)
        {


            if (string.IsNullOrEmpty(MaBP))
            {
                return Json(new { success = false, message = "Mã bộ phận không được để trống." });
            }
            var department = _sqlConnectionserver.BoPhans.Where(BP => BP.MaBoPhan == MaBP)
                .Select(BP => new
                {
                    BP.MaBoPhan,
                    BP.TenBP,
                    BP.MaCuaHang,
                    TenCuaHang = BP.CuaHang.TenCH
                })
                .FirstOrDefault();
            if (department == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bộ phận với mã đã cho." });
            }
            return Json(new { success = true, data = department });
        }


        [HttpGet]
        public ActionResult UpdateDepartments()
        {
            var stores = _sqlConnectionserver.CuaHangs
            .Select(ch => new { ch.MaCuaHang, ch.TenCH })
            .ToList();
            ViewBag.Store = new SelectList(stores, "MaCuaHang", "TenCH");
            return View();
        }
        [HttpPost]
        public ActionResult UpdateDepartments(string maBoPhan, string tenBoPhan, int? maCuaHang)
        {
            try
            {
                if (string.IsNullOrEmpty(maBoPhan) || string.IsNullOrEmpty(tenBoPhan) || maCuaHang == null)
                {
                    return Json(new { success = false, message = "Thông tin bộ phận không được để trống." });
                }
                var department = _sqlConnectionserver.BoPhans.FirstOrDefault(bp => bp.MaBoPhan == maBoPhan);

                if (department == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bộ phận để cập nhật." });
                }
                department.TenBP = tenBoPhan;
                department.MaCuaHang = (int)maCuaHang;
                _sqlConnectionserver.SaveChanges();
                return Json(new { success = true, message = "Cập nhật bộ phận thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult DeleteDepartments()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DeleteDepartments(string maBP)
        {
            try
            {
                if (string.IsNullOrEmpty(maBP))
                {
                    return Json(new { success = false, message = "Mã bộ phận không được để trống." });

                }
                var bophans = _sqlConnectionserver.BoPhans.Find(maBP);
                if (bophans != null)
                {
                    var deleteBPNV = _sqlConnectionserver.NhanViens.Where(u => u.MaBoPhan == maBP);
                    if (deleteBPNV.Any())
                    {
                        _sqlConnectionserver.NhanViens.RemoveRange(deleteBPNV);

                    }
                    var deleteBPCV = _sqlConnectionserver.ChucVus.Where(u => u.MaBoPhan == maBP);
                    if (deleteBPNV.Any())
                    {
                        _sqlConnectionserver.ChucVus.RemoveRange(deleteBPCV);
                    }
                    _sqlConnectionserver.BoPhans.Remove(bophans);
                    _sqlConnectionserver.SaveChanges();
                    return Json(new { success = true, message = "Xóa Bộ phận không thành công" });


                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy bộ phận với mã đã cho." });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = $"Có lỗi xảy ra trong quá trình xóa bộ phận {ex.Message}" });
            }
        }
        public string CreatePositionCode(string TenCV)
        {
            // Tách chuỗi thành các từ, loại bỏ khoảng trắng thừa
            var words = TenCV.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Lấy ký tự đầu của mỗi từ và giữ nguyên chữ hoa/thường
            string MaCV = string.Concat(words.Select(w => w.Substring(0, 1)));

            // Thêm tiền tố "CV_"
            return "CV_" + MaCV;

        }
        [HttpGet]
        public JsonResult GetPositionByDepartments(string maBP)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Mã bộ phận nhận được: {maBP}");
                var chucvu = _sqlConnectionserver.ChucVus.Where(cv => cv.MaBoPhan == maBP)
                    .Select(cv => new { cv.MaChucVu, cv.TenChucVu }).ToList();
                System.Diagnostics.Debug.WriteLine($"Số lượng chức vụ: {chucvu.Count}");
                return Json(chucvu, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult AddPosition()
        {
            var listCV = _sqlConnectionserver.ChucVus.Include(c => c.BoPhan).ToList();
            ViewBag.DSCV = listCV;
            var bophans = _sqlConnectionserver.BoPhans.ToList();
            ViewBag.DSBoPhan = new SelectList(listCV, "MaBoPhan", "TenBoPhan");
            return View();

        }
        [HttpPost]
        public ActionResult AddPosition(string TenCV, string maBP)
        {
            try
            {
                if (string.IsNullOrEmpty(TenCV) || string.IsNullOrEmpty(maBP))
                {
                    return Json(new { success = false, message = "Tên chức vụ và bộ phận không được để trống." });
                }
                var MaCV = CreatePositionCode(TenCV);
                var bophans = _sqlConnectionserver.BoPhans.FirstOrDefault(BP => BP.MaBoPhan == maBP);
                if (bophans == null)
                {
                    return Json(new { success = false, message = "Mã bộ phận không tồn tại" });
                }
                var newChucVu = new ChucVu
                {
                    MaChucVu = MaCV,
                    TenChucVu = TenCV,
                    MaBoPhan = maBP,

                };
                _sqlConnectionserver.ChucVus.Add(newChucVu);
                _sqlConnectionserver.SaveChanges();
                return Json(new
                {
                    success = true,
                    MaChucVu = MaCV,
                    TenChucVu = TenCV,
                    TenBoPhan = bophans.TenBP

                });


            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình thêm chức vụ.", error = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult SearchPosition(string maCV)
        {
            try
            {
                if (string.IsNullOrEmpty(maCV))
                {
                    return Json(new { success = false, message = "Mã chức vụ không được để trống." });
                }

                var chucvu = _sqlConnectionserver.ChucVus.FirstOrDefault(cv => cv.MaChucVu == maCV);
                if (chucvu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chức vụ." });
                }
                return Json(new
                {
                    success = true,
                    MaChucVu = chucvu.MaChucVu,
                    TenCV = chucvu.TenChucVu,
                    MaBP = chucvu.MaBoPhan,
                    tenBoPhan = chucvu.BoPhan?.TenBP


                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình tìm kiếm mã chức vụ{ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult UpdatePosition()
        {
            var listCV = _sqlConnectionserver.ChucVus.Include(c => c.BoPhan).ToList();
            ViewBag.DSCV = listCV;
            var bophans = _sqlConnectionserver.BoPhans.ToList();
            ViewBag.DSBoPhan = new SelectList(listCV, "MaBoPhan", "TenBoPhan");
            return View();
        }
        [HttpPost]
        public ActionResult UpdatePosition(ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                var existingChucVu = _sqlConnectionserver.ChucVus.FirstOrDefault(cv => cv.MaChucVu == chucVu.MaChucVu);
                if (existingChucVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chức vụ cần cập nhật." });
                }
                existingChucVu.TenChucVu = chucVu.TenChucVu;
                existingChucVu.MaBoPhan = chucVu.MaBoPhan;
                _sqlConnectionserver.SaveChanges();

                return Json(new { success = true, message = "Cập nhật chức vụ thành công." });

            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });


        }
        [HttpGet]
        public ActionResult DeletePosition()
        {
            var listCV = _sqlConnectionserver.ChucVus.Include(c => c.BoPhan).ToList();
            ViewBag.DSCV = listCV;
            var bophans = _sqlConnectionserver.BoPhans.ToList();
            ViewBag.DSBoPhan = new SelectList(listCV, "MaBoPhan", "TenBoPhan");
            return View();
        }
        [HttpPost]
        public ActionResult DeletePosition(string MaCV)
        {
            try
            {
                if (string.IsNullOrEmpty(MaCV))
                {
                    return Json(new { success = false, message = "Mã chức vụ không được để trống." });
                }
                var deleteCV = _sqlConnectionserver.ChucVus.Find(MaCV);
                if (deleteCV != null)
                {
                    var deletePositionNV = _sqlConnectionserver.NhanViens.Where(u => u.MaChucVu == MaCV);
                    if (deletePositionNV.Any())
                    {
                        _sqlConnectionserver.NhanViens.RemoveRange(deletePositionNV);
                    }
                    _sqlConnectionserver.ChucVus.Remove(deleteCV);
                    _sqlConnectionserver.SaveChanges();
                    return Json(new { success = true, message = "Xóa chức vụ thành công" });
                }
                return Json(new { success = false, message = "Mã chức vụ không tồn tại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình xóa chức vụ{ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult AddUser()
        {
            
            return View();
        }
        public ActionResult AddUser(int? MaNV, string role)
        {
            try
            {
                if (MaNV == null || MaNV <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã nhân viên hợp lệ" });
                }
                var nhanvien = _sqlConnectionserver.NhanViens.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if (nhanvien == null)
                {
                    return Json(new { success = false, message = "Mã nhân viên không tồn tại." });
                }
                var existingUser = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Nhân viên đã có tài khoản." });
                }
                var validRoles = new List<string> { "Admin", "Manager", "User" };
                if (!validRoles.Contains(role))
                {
                    return Json(new { success = false, message = "Vai trò không hợp lệ." });
                }
                string username = "User_" + MaNV + "_" + GenerateRandomString(5);
                string password = GenerateRandomString(10);
                var passwordHashAndSalt = HashPassword(password);

                var newUser = new User
                {
                    MaNhanVien = nhanvien.MaNhanVien,
                    UserName = username,
                    PasswordSalt = passwordHashAndSalt.Salt,
                    PasswordHash = passwordHashAndSalt.HashedPassword,
                    TGDangNhap = null,
                    TGDangXuat = null,
                    Locked = false,
                    DiaChiIP = null,
                    DNLanDau = true,
                    Roles = role,
                    TrangThai = "Chờ Duyệt",
                    TrangThaiDuyetQL = "Chờ Duyệt"

                };
                _sqlConnectionserver.Users.Add(newUser);
                _sqlConnectionserver.SaveChanges();
                var managerEmail = _sqlConnectionserver.NhanViens.Where(
                    nv => nv.MaChucVu == _sqlConnectionserver.ChucVus.Where(cv => cv.TenChucVu == "Quản lý cửa hàng")
                    .Select(cv => cv.MaChucVu)
                    .FirstOrDefault() && nv.MaBoPhan == nhanvien.MaBoPhan)
                    .Select(nv => nv.Email).FirstOrDefault();
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return Json(new { success = true, message = "User đã được tạo nhưng không tìm thấy Quản lý cửa hàng để gửi email phê duyệt." });
                }
                var emailService = new EmailService();
                string subject = "Phê duyệt User mới";
                string body = $"Xin chào Quản lý,<br/>" +
              $"Nhân viên {nhanvien.HoTen} (Mã NV: {MaNV}) đã được Admin tạo tài khoản.<br/>" +
              $"Vui lòng phê duyệt tài khoản tại đường dẫn sau: " +
              $"<a href='https://yourapp.com/User/Approve?UserId={newUser.UserID}&Action=Approve'>Phê duyệt</a> | " +
              $"<a href='https://yourapp.com/User/Approve?UserId={newUser.UserID}&Action=Reject'>Từ chối</a>";
                emailService.SendEmail(managerEmail, subject, body);

                return Json(new { success = true, message = " User đã được tạo và đang chờ phê duyệt." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
        [HttpGet]
        public JsonResult GetAllUserList()
        {
            try
            {
                var users = _sqlConnectionserver.Users.Select(u => new
                {
                    u.UserID,
                    u.MaNhanVien,
                    u.Roles,
                    UserName = new string('*', u.UserName.Length), // Hiển thị dấu hoa thị theo độ dài UserName
                    PasswordHash = new string('*', u.PasswordHash.Length), // Hiển thị dấu hoa thị cho Password
                    u.TrangThai,
                    u.MaNVQL,
                    u.TrangThaiDuyetQL,
                    
                }).ToList();
                if (users.Count == 0)
                {
                    return Json(new { success = false, message = "Không có người dùng nào." });
                }
                return Json(users);

            }catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
        [HttpGet]
        public JsonResult SearchUserByMaNV(int MaNV)
        {
            var user = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
            if (user != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        UserID = user.UserID,
                        MaNhanVien = user.MaNhanVien,
                        Roles = user.Roles,
                        TrangThai = user.TrangThai,
                        TrangThaiDuyetQL = user.TrangThaiDuyetQL
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Không tìm thấy User." }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult UpdateUser()
        {
            var users = _sqlConnectionserver.Users.ToList();
           
            return View(users);
        }
        [HttpPost]
        public ActionResult UpdateUser(User userUpdate, int? MaNV)
        {
            try
            {
                if (MaNV == null || MaNV <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã nhân viên hợp lệ" });
                }
                var user = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if (user == null)
                {
                    return Json(new { success = false, message = "Mã nhân viên không tồn tại." });
                }
                
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User để cập nhật." });
                }
                var nhanvien = _sqlConnectionserver.NhanViens.FirstOrDefault(u => u.MaNhanVien == MaNV);
                user.MaNhanVien = userUpdate.MaNhanVien;
                user.Roles = userUpdate.Roles;
                user.TrangThai = userUpdate.TrangThai = "Chờ Duyệt";
                user.TrangThaiDuyetQL = userUpdate.TrangThaiDuyetQL = "Chờ Duyệt";
                _sqlConnectionserver.SaveChanges();

                var managerEmail = _sqlConnectionserver.NhanViens
          .Where(nv =>
              nv.MaChucVu == _sqlConnectionserver.ChucVus
                  .Where(cv => cv.TenChucVu == "Quản lý cửa hàng")
                  .Select(cv => cv.MaChucVu)
                  .FirstOrDefault() &&
              nv.MaBoPhan == nhanvien.MaBoPhan)
          .Select(nv => nv.Email)
          .FirstOrDefault();
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return Json(new { success = true, message = "User đã được tạo nhưng không tìm thấy Quản lý cửa hàng để gửi email phê duyệt." });
                }
                var emailService = new EmailService();
                string subject = "Phê duyệt User mới";
                string body = $@"
        Xin chào Quản lý,<br/><br/>
        Nhân viên {user.MaNhanVien} (UserID: {user.UserID}) đã được cập nhật thông tin.<br/>
        Vui lòng phê duyệt hoặc từ chối tài khoản tại đường dẫn sau:<br/>
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Approve'>Phê duyệt</a> | 
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Reject'>Từ chối</a>";

                emailService.SendEmail(managerEmail, subject, body);
                return Json(new { success = true, message = "User đã được cập nhật và email phê duyệt đã được gửi đến quản lý." });
            }
            catch(Exception ex)
            {
                return Json(new { success = true, message = $"User đã được cập nhật nhưng không thể gửi email phê duyệt. Chi tiết lỗi: {ex.Message}" });
            }

           


        }
        
        [HttpGet]
        public ActionResult DeleteUser()
        {
            var users = _sqlConnectionserver.Users.ToList();

            return View(users);
        }
        [HttpPost]
        public ActionResult DeleteUser(int userId,int? MaNV)
        {
            try
            {
                if (MaNV == null || MaNV < 0)
                {
                    return Json(new { success = false, message = "Không được để trống hoặc mã nhân phải lớn hơn 0" });

                }
                var userDelete = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if (userDelete == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User tương ứng với mã nhân viên cung cấp " });

                }
                
                
                var nhanvien = _sqlConnectionserver.NhanViens.FirstOrDefault(u => u.MaNhanVien == MaNV);
                _sqlConnectionserver.Users.Remove(userDelete);
                _sqlConnectionserver.SaveChanges();
                var managerEmail = _sqlConnectionserver.NhanViens
          .Where(nv =>
              nv.MaChucVu == _sqlConnectionserver.ChucVus
                  .Where(cv => cv.TenChucVu == "Quản lý cửa hàng")
                  .Select(cv => cv.MaChucVu)
                  .FirstOrDefault() &&
              nv.MaBoPhan == nhanvien.MaBoPhan)
          .Select(nv => nv.Email)
          .FirstOrDefault();
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return Json(new { success = true, message = "User đã được tạo nhưng không tìm thấy Quản lý cửa hàng để gửi email phê duyệt." });
                }
                var emailService = new EmailService();
                string subject = "Phê duyệt User mới";
                string body = $@"
        Xin chào Quản lý,<br/><br/>
        Nhân viên {userDelete.MaNhanVien} (UserID: {userDelete.UserID}) đã được xóa .<br/>
        Vui lòng phê duyệt hoặc từ chối tài khoản tại đường dẫn sau:<br/>
        <a href='https://yourapp.com/User/Approve?UserId={userDelete.UserID}&Action=Approve'>Phê duyệt</a> | 
        <a href='https://yourapp.com/User/Approve?UserId={userDelete.UserID}&Action=Reject'>Từ chối</a>";

                emailService.SendEmail(managerEmail, subject, body);
                return Json(new { success = true, message = "User đã được xóa khỏi hệ thống và email phê duyệt đã được gửi đến quản lý." });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình xóa User{ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult LockUser()
        {
            var users = _sqlConnectionserver.Users.ToList();
            return View(users);
        }
        [HttpPost]
        public ActionResult LockUser(int? MaNV)
        {
            try
            {
                if(MaNV == null || MaNV < 0)
                {
                    return Json(new { success = false, message = "Không được để trống hoặc mã nhân phải lớn hơn 0" });
                }
                var user = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if(user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User tương ứng với mã nhân viên cung cấp " });

                }
                if(user.Locked == false)
                {
                    return Json(new { success = false, message = "User đã bị khóa trước đó" });
                }
                user.TrangThai = "Lock";
                user.Locked = true;
                user.NgayCapNhat = DateTime.Now;
                _sqlConnectionserver.SaveChanges();
                var nhanvien = _sqlConnectionserver.NhanViens.FirstOrDefault(u => u.MaNhanVien == MaNV);
                _sqlConnectionserver.Users.Remove(user);
                _sqlConnectionserver.SaveChanges();
                var managerEmail = _sqlConnectionserver.NhanViens
          .Where(nv =>
              nv.MaChucVu == _sqlConnectionserver.ChucVus
                  .Where(cv => cv.TenChucVu == "Quản lý cửa hàng")
                  .Select(cv => cv.MaChucVu)
                  .FirstOrDefault() &&
              nv.MaBoPhan == nhanvien.MaBoPhan)
          .Select(nv => nv.Email)
          .FirstOrDefault();
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return Json(new { success = true, message = "User đã được tạo nhưng không tìm thấy Quản lý cửa hàng để gửi email phê duyệt." });
                }
                var emailService = new EmailService();
                string subject = "Phê duyệt User mới";
                string body = $@"
        Xin chào Quản lý,<br/><br/>
        Nhân viên {user.MaNhanVien} (UserID: {user.UserID}) đã được khóa  .<br/>
        Vui lòng phê duyệt hoặc từ chối tài khoản tại đường dẫn sau:<br/>
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Approve'>Phê duyệt</a> | 
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Reject'>Từ chối</a>";

                emailService.SendEmail(managerEmail, subject, body);
                return Json(new { success = true, message = "User đã được khóa  và email phê duyệt đã được gửi đến quản lý." });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra trong quá trình khóa User Lỗi:{ex.Message}" });
            }
        }
        [HttpGet]
        public ActionResult UnlockUser()
        {
            var users = _sqlConnectionserver.Users.ToList();

            return View(users);
        }
        [HttpPost]
        public  ActionResult UnlockUser(int? MaNV)
        {
            try
            {
                if(MaNV ==  null ||  MaNV < 0)
                {
                    return Json(new { success = false, message = " Không được để trống hoặc mã nhân viên phải lớn hơn 0 " });

                }
                var user = _sqlConnectionserver.Users.FirstOrDefault(u => u.MaNhanVien == MaNV);
                if(user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User tương ứng với mã nhân viên cung cấp " });

                }
                if(user.Locked == false)
                {
                    return Json(new { success = false, message = "User không bị khóa vẫn hoạt động bình thường" });
                }
                user.Locked = false;
                user.TrangThai = "Active";
                user.NgayCapNhat = DateTime.Now;
                _sqlConnectionserver.SaveChanges();
                var nhanvien = _sqlConnectionserver.NhanViens.FirstOrDefault(u => u.MaNhanVien == MaNV);
                _sqlConnectionserver.Users.Remove(user);
                _sqlConnectionserver.SaveChanges();
                var managerEmail = _sqlConnectionserver.NhanViens
          .Where(nv =>
              nv.MaChucVu == _sqlConnectionserver.ChucVus
                  .Where(cv => cv.TenChucVu == "Quản lý cửa hàng")
                  .Select(cv => cv.MaChucVu)
                  .FirstOrDefault() &&
              nv.MaBoPhan == nhanvien.MaBoPhan)
          .Select(nv => nv.Email)
          .FirstOrDefault();
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return Json(new { success = true, message = "User đã được tạo nhưng không tìm thấy Quản lý cửa hàng để gửi email phê duyệt." });
                }
                var emailService = new EmailService();
                string subject = "Phê duyệt User mới";
                string body = $@"
        Xin chào Quản lý,<br/><br/>
        Nhân viên {user.MaNhanVien} (UserID: {user.UserID}) (Ngày cập nhật:{user.NgayCapNhat}) đã được mở khóa.<br/>
        Vui lòng phê duyệt hoặc từ chối tài khoản tại đường dẫn sau:<br/>
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Approve'>Phê duyệt</a> | 
        <a href='https://yourapp.com/User/Approve?UserId={user.UserID}&Action=Reject'>Từ chối</a>";

                emailService.SendEmail(managerEmail, subject, body);
                return Json(new { success = true, message = "User đã được mở khóa  và email phê duyệt đã được gửi đến quản lý." });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi: {ex.Message} xảy ra trong quá trình khóa  User" });
            }
        }
        public string CreateCodePermissions(string tenQuyen)
        {
            if (string.IsNullOrWhiteSpace(tenQuyen))
            {
                return "Tên quyền không được để trống.";
            }
            var words = tenQuyen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var codePermissions = string.Concat(words.Select(w => w[0])).ToUpper();
            return codePermissions;
        }
        [HttpGet]
        public JsonResult GetPermissions()
        {
            try
            {
                var ListPermissions = _sqlConnectionserver.Quyens.Select(q => new
                {
                    q.MaQuyen,
                    q.TenQuyen
                }).ToList();
                return Json(ListPermissions, JsonRequestBehavior.AllowGet);
            }catch(Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult CreatePermissions()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreatePermissions(string tenQuyen)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenQuyen))
                {
                    return Json(new { success = false, message = "Tên quyền không được để trống!" });
                }
                string maQuyen = CreateCodePermissions(tenQuyen);
                if (_sqlConnectionserver.Quyens.Any(q => q.MaQuyen == maQuyen))
                {
                    return Json(new { success = false, message = "Mã quyền đã tồn tại!" });
                }
                var quyen = new Quyen
                {
                    MaQuyen = maQuyen,
                    TenQuyen = tenQuyen
                };
                _sqlConnectionserver.Quyens.Add(quyen);
                _sqlConnectionserver.SaveChanges();
                return Json(new { success = true, message = "Tạo quyền thành công!" });
            }catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi:{ex.Message} xảy ra trong quá trình tạo quyền" });
            }

        }
        [HttpPost]
        public ActionResult SearchCodePermissions(string maQuyen)
        {
            try
            {
                if (string.IsNullOrEmpty(maQuyen))
                {
                    return Json(new { success = false, message = "Mã quyền không được để trống." });
                }

                if (_sqlConnectionserver == null)
                {
                    return Json(new { success = false, message = "Kết nối cơ sở dữ liệu không khả dụng." });
                }

                var quyen = _sqlConnectionserver.Quyens.FirstOrDefault(u => u.MaQuyen == maQuyen);

                if (quyen != null)
                {
                    return Json(new { success = true, data = quyen });
                }

                return Json(new { success = false, message = "Không tìm thấy quyền!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public ActionResult UpdatePermissions()
        {
           
            return View();
        }
        [HttpPost]
        public  ActionResult UpdatePermissions(string maQuyen, string tenQuyen)
        {
            try
            {
                if (string.IsNullOrEmpty(maQuyen))
                {
                    return Json(new { success = false, message = "Mã quyền không được để trống" });
                }
                var quyen = _sqlConnectionserver.Quyens.FirstOrDefault(u => u.MaQuyen == maQuyen);
                if (quyen == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy quyền với mã đã cung cấp" });
                }
                if (string.IsNullOrEmpty(tenQuyen))
                {
                    quyen.TenQuyen = tenQuyen;
                    _sqlConnectionserver.SaveChanges();
                    return Json(new { success = true, message = "Cập nhật quyền thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Tên quyền không được để trống" });
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = $"Có lỗi:{ex.Message} trong quá trình cập nhật quyền" });
            }
            

        }
        [HttpGet]
        public ActionResult DeletePermissions()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DeletePermissions(string maQuyen)
        {
            if (string.IsNullOrEmpty(maQuyen))
            {
                return Json(new { success = false, message = "Mã quyền không được để trống" });
            }
            var quyen = _sqlConnectionserver.Quyens.Find(maQuyen);
            if(quyen != null)
            {
                var maquyen = _sqlConnectionserver.Users.Where(u => u.MaQuyen == maQuyen);
                if (maquyen.Any())
                {
                    _sqlConnectionserver.Users.RemoveRange(maquyen);
                }
                _sqlConnectionserver.Quyens.Remove(quyen);
                _sqlConnectionserver.SaveChanges();
                return Json(new { success = true, message = "Xóa quyền thành công" });
            }
            return Json(new { success = false, message = "Mã quyền không tồn tại" });
        }
        public ActionResult Approve(int userID, string Action, string OperationType)
        {
            try
            {
                var user = _sqlConnectionserver.Users.FirstOrDefault(u => u.UserID == userID);
                if(user == null)
                {
                    return Json(new { success = false, message = "User không tồn tại." });
                }
                if (OperationType == "Add")
                {
                    if (Action == "Approve")
                    {
                        user.TrangThaiDuyetQL = "Approve";
                        user.DNLanDau = true;
                        _sqlConnectionserver.SaveChanges();

                        var emailService = new EmailService();
                        string subject = "User của bạn đã được phê duyệt";
                        string body = $"Xin chào {user.UserName}{user.PasswordHash},<br/>User của bạn đã được phê duyệt. Vui lòng đăng nhập để đổi mật khẩu để sử dụng.";
                        emailService.SendEmail(user.NhanVien.Email, subject, body);

                        return Json(new { success = true, message = "Tài khoản đã được phê duyệt." });
                    }
                    else if (Action == "Reject")
                    {
                        user.TrangThaiDuyetQL = "Reject";
                        _sqlConnectionserver.SaveChanges();

                        return Json(new { success = true, message = "Tài khoản đã bị từ chối." });

                    }
                }
                // Logic xử lý cho cập nhật user
                if (OperationType == "Update")
                {
                    if (Action == "Approve")
                    {
                        user.TrangThaiDuyetQL = "Approve";
                        _sqlConnectionserver.SaveChanges();

                        var emailService = new EmailService();
                        string subject = "User của bạn đã được cập nhật và phê duyệt";
                        string body = $"Xin chào {user.UserName},<br/>Thông tin tài khoản của bạn đã được cập nhật và phê duyệt. Vui lòng đăng nhập để sử dụng.";
                        emailService.SendEmail(user.NhanVien.Email, subject, body);

                        return Json(new { success = true, message = "Cập nhật tài khoản đã được phê duyệt." });
                    }
                    else if (Action == "Reject")
                    {
                        user.TrangThaiDuyetQL = "Reject";
                        _sqlConnectionserver.SaveChanges();

                        return Json(new { success = true, message = "Cập nhật tài khoản đã bị từ chối." });
                    }
                }
                // Logic xử lý cho xóa user
                if (OperationType == "Delete")
                {
                    if (Action == "Approve")
                    {
                        _sqlConnectionserver.Users.Remove(user);
                        _sqlConnectionserver.SaveChanges();

                        var emailService = new EmailService();
                        string subject = "User của bạn đã bị xóa";
                        string body = $"Xin chào {user.UserName},<br/>Tài khoản của bạn đã bị xóa khỏi hệ thống. Nếu có vấn đề gì, vui lòng liên hệ với bộ phận quản lý.";
                        emailService.SendEmail(user.NhanVien.Email, subject, body);

                        return Json(new { success = true, message = "Tài khoản đã bị xóa." });
                    }
                    else if (Action == "Reject")
                    {
                        return Json(new { success = true, message = "Xóa tài khoản đã bị từ chối." });
                    }
                }
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        

        // Hàm tạo chuỗi ngẫu nhiên
        private string GenerateRandomString(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=";
            StringBuilder randomString = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                randomString.Append(validChars[random.Next(validChars.Length)]);
            }
            return randomString.ToString();
        }
        // Hàm băm mật khẩu với Salt
        private (string HashedPassword, string Salt) HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = GenerateRandomString(16); // Tạo salt ngẫu nhiên
                hmac.Key = Encoding.UTF8.GetBytes(salt);
                var hashedPassword = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return (hashedPassword, salt);
            }
        }


    }
}