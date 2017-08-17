namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_MIS_Info")]
    public partial class tbl_MIS_Info
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_MIS_Info()
        {
            tbl_MIS_Info1 = new HashSet<tbl_MIS_Info>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        public int MISInfoId { get; set; }

        [Required]
        [StringLength(50)]
        public string MISCode { get; set; }

        [Required]
        [StringLength(50)]
        public string MISName { get; set; }

        public short? MISTypeId { get; set; }

        public int? CompanyId { get; set; }

        public int? ParentMISInfoId { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_MIS_Type tbl_MIS_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_MIS_Info> tbl_MIS_Info1 { get; set; }

        public virtual tbl_MIS_Info tbl_MIS_Info2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
