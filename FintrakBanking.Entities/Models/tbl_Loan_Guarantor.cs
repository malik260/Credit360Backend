namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_GUARANTOR")]
    public partial class TBL_LOAN_GUARANTOR
    {
        [Key]
        public short LOANGUARANTORID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        [StringLength(50)]
        public string BVN { get; set; }

        [StringLength(250)]
        public string FIRSTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(100)]
        public string RELATIONSHIP { get; set; }

        public int? RELATIONSHIPDURATION { get; set; }

        [StringLength(50)]
        public string PHONENUMBER1 { get; set; }

        [StringLength(50)]
        public string PHONENUMBER2 { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}
