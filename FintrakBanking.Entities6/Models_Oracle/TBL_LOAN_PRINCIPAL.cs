namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_PRINCIPAL")]
    public partial class TBL_LOAN_PRINCIPAL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_PRINCIPAL()
        {
            TBL_LOAN_APPLICATION_DETL_BG = new HashSet<TBL_LOAN_APPLICATION_DETL_BG>();
            TBL_LOAN_APPLICATION_DETL_INV = new HashSet<TBL_LOAN_APPLICATION_DETL_INV>();
        }

        [Key]
        public int PRINCIPALID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRINCIPALSREGNUMBER { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public int COMPANYID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_INV> TBL_LOAN_APPLICATION_DETL_INV { get; set; }
    }
}
