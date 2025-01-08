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
    
    public partial class Cong
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cong()
        {
            this.Luongs = new HashSet<Luong>();
            this.QLCaNVs = new HashSet<QLCaNV>();
            this.QLCaNVs1 = new HashSet<QLCaNV>();
            this.QLCaNVs2 = new HashSet<QLCaNV>();
            this.QLCaNVs3 = new HashSet<QLCaNV>();
            this.QLCaNVs4 = new HashSet<QLCaNV>();
            this.QLCaNVs5 = new HashSet<QLCaNV>();
            this.QLCaNVs6 = new HashSet<QLCaNV>();
        }
    
        public int MaCong { get; set; }
        public int MaNhanVien { get; set; }
        public System.DateTime NgayLamViec { get; set; }
        public System.TimeSpan ThoiGianBD { get; set; }
        public System.TimeSpan ThoiGianKT { get; set; }
        public decimal SoGioTT { get; set; }
        public string LoaiCa { get; set; }
        public string TrangThai { get; set; }
    
        public virtual NhanVien NhanVien { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Luong> Luongs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs5 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QLCaNV> QLCaNVs6 { get; set; }
    }
}
