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
    
    public partial class HDLaoDong
    {
        public string MaHopDong { get; set; }
        public int MaNhanVien { get; set; }
        public int LuongCoBanTB { get; set; }
        public string LoaiHDLD { get; set; }
        public System.DateTime NgayApDung { get; set; }
        public Nullable<System.DateTime> NgayHetHL { get; set; }
        public string TrangThai { get; set; }
    
        public virtual BacLuong BacLuong { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}