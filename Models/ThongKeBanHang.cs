//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StoreMartket_System_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ThongKeBanHang
    {
        public int MaThongKe { get; set; }
        public int MaNhanVien { get; set; }
        public System.DateTime NgayThongKe { get; set; }
        public decimal TongTien { get; set; }
        public int SoLuongBanHang { get; set; }
        public string MaHangHoa { get; set; }
    
        public virtual HangHoa HangHoa { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}
