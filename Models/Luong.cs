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
    
    public partial class Luong
    {
        public int MaLuong { get; set; }
        public int MaCong { get; set; }
        public int MaKPI { get; set; }
        public int MaNhanVien { get; set; }
        public System.DateTime Thang { get; set; }
        public int SoNgayCongTrongThang { get; set; }
        public decimal SoGioLamViecTrongThang { get; set; }
        public decimal PhuCap { get; set; }
        public decimal BHXH { get; set; }
        public decimal BHYT { get; set; }
        public decimal BHTT { get; set; }
        public decimal KhauTru { get; set; }
        public int LuongCoBanTB { get; set; }
        public decimal TongLuongTN { get; set; }
        public System.DateTime NgayChotLuong { get; set; }
    
        public virtual BacLuong BacLuong { get; set; }
        public virtual Cong Cong { get; set; }
        public virtual KPI KPI { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}