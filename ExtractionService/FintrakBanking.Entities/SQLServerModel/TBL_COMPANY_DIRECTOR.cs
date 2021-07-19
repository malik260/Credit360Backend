namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_COMPANY_DIRECTOR")]
    public partial class TBL_COMPANY_DIRECTOR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COMPANY_DIRECTOR()
        {
            TBL_CUSTOMER_RELATED_PARTY = new HashSet<TBL_CUSTOMER_RELATED_PARTY>();
        }

        [Key]
        public int COMPANYDIRECTORID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(50)]
        public string TITLE { get; set; }

        [Required]
        //[StringLength(200)]
        public string FIRSTNAME { get; set; }

        [Required]
        //[StringLength(200)]
        public string MIDDLENAME { get; set; }

        [Required]
        //[StringLength(200)]
        public string LASTNAME { get; set; }

        //[StringLength(10)]
        public string GENDER { get; set; }

        //[StringLength(50)]
        public string BVN { get; set; }

        //[StringLength(500)]
        public string ADDRESS { get; set; }

        //[StringLength(50)]
        public string EMAIL { get; set; }

        //[StringLength(150)]
        public string PHONENUMBER { get; set; }

        public bool ISACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_RELATED_PARTY> TBL_CUSTOMER_RELATED_PARTY { get; set; }
    }
}
