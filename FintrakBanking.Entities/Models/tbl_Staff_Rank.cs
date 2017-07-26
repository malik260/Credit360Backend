namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Staff_Rank")]
    public partial class tbl_Staff_Rank
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Staff_Rank()
        {
            tbl_Staff = new HashSet<tbl_Staff>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        public int RankId { get; set; }

        [StringLength(50)]
        public string RankCode { get; set; }

        [Required]
        [StringLength(50)]
        public string RankName { get; set; }

        public int CompanyId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Staff> tbl_Staff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
