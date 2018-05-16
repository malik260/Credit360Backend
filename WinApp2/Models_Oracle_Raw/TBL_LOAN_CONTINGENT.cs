namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_CONTINGENT")]
    public partial class TBL_LOAN_CONTINGENT
    {
        public TBL_LOAN_CONTINGENT()
        {
            TBL_LOAN_CONTINGENT_USAGE = new HashSet<TBL_LOAN_CONTINGENT_USAGE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONTINGENTLOANID { get; set; }

        public int CUSTOMERID { get; set; }

        public int PRODUCTID { get; set; }

        public int LOANSYSTEMTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int BRANCHID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int ISTENORED { get; set; }

        public int ISBANKFORMAT { get; set; }

        public int CURRENCYID { get; set; }

        public decimal EXCHANGERATE { get; set; }

        [Required]
        [StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string RELATED_LOAN_REFERENCE_NUMBER { get; set; }

        public int SUBSECTORID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        [StringLength(50)]
        public string MISCODE { get; set; }

        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public decimal CONTINGENTAMOUNT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? APPROVEDBY { get; set; }

        [StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public int LOANSTATUSID { get; set; }

        public int ISDISBURSED { get; set; }

        [StringLength(50)]
        public string DISBURSEDBY { get; set; }

        [StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        public int? OPERATIONID { get; set; }

        public int DISCHARGELETTER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DISBURSEDATE { get; set; }

        public DateTime? BOOKINGDATE { get; set; }

        public DateTime? MATURITYDATE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }

        public virtual TBL_LOAN_STATUS TBL_LOAN_STATUS { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual ICollection<TBL_LOAN_CONTINGENT_USAGE> TBL_LOAN_CONTINGENT_USAGE { get; set; }
    }
}
