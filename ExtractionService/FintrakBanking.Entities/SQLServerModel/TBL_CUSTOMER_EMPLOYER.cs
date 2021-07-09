namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_EMPLOYER")]
    public partial class TBL_CUSTOMER_EMPLOYER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_EMPLOYER()
        {
            TBL_CUSTOMER_EMPLOYMENTHISTORY = new HashSet<TBL_CUSTOMER_EMPLOYMENTHISTORY>();
        }

        [Key]
        public int EMPLOYERID { get; set; }

        [Required]
        //[StringLength(300)]
        public string EMPLOYER_NAME { get; set; }

        public short EMPLOYER_SUB_TYPEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(500)]
        public string ADDRESS { get; set; }

        public int CITYID { get; set; }

        //[StringLength(50)]
        public string PHONENUMBER { get; set; }

        //[StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER_EMPLOYER_TYPE_SUB TBL_CUSTOMER_EMPLOYER_TYPE_SUB { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_EMPLOYMENTHISTORY> TBL_CUSTOMER_EMPLOYMENTHISTORY { get; set; }
    }
}
