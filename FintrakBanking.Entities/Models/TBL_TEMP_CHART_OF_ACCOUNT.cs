namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CHART_OF_ACCOUNT")]
    public partial class TBL_TEMP_CHART_OF_ACCOUNT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_TEMP_CHART_OF_ACCOUNT()
        {
            TBL_TEMP_CHART_OF_ACCOUNT_CUR = new HashSet<TBL_TEMP_CHART_OF_ACCOUNT_CUR>();
        }

        [Key]
        public int GLACCOUNTID { get; set; }

        public int ACCOUNTTYPEID { get; set; }

        [Required]
        [StringLength(20)]
        public string ACCOUNTCODE { get; set; }

        [Required]
        [StringLength(500)]
        public string ACCOUNTNAME { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public bool SYSTEMUSE { get; set; }

        public int? ACCOUNTSTATUSID { get; set; }

        public short? GLCLASSID { get; set; }

        public bool BRANCHSPECIFIC { get; set; }

        [StringLength(20)]
        public string OLDACCOUNTID { get; set; }

        public short FSCAPTIONID { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_ACCOUNT_TYPE TBL_ACCOUNT_TYPE { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT_CLASS TBL_CHART_OF_ACCOUNT_CLASS { get; set; }

        public virtual TBL_FINANCIAL_STATEMENT_CAPTN TBL_FINANCIAL_STATEMENT_CAPTN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHART_OF_ACCOUNT_CUR> TBL_TEMP_CHART_OF_ACCOUNT_CUR { get; set; }
    }
}
