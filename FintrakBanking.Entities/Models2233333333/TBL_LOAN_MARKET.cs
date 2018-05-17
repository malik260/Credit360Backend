namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_MARKET")]
    public partial class TBL_LOAN_MARKET
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_MARKET()
        {
            TBL_LOAN_APPLICATION_DETL_TRA = new HashSet<TBL_LOAN_APPLICATION_DETL_TRA>();
        }

        [Key]
        public int MARKETID { get; set; }

        [Required]
        [StringLength(300)]
        public string MARKETNAME { get; set; }

        public int COMPANYID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [Required]
        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        public int CITYID { get; set; }

        [Required]
        [StringLength(500)]
        public string ADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_TRA> TBL_LOAN_APPLICATION_DETL_TRA { get; set; }
    }
}
