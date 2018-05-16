namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVOLVING")]
    public partial class TBL_LOAN_REVOLVING
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_REVOLVING()
        {
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
        }

        [Key]
        public int REVOLVINGLOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public short BRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public short? REVOLVINGTYPEID { get; set; }

        public double EXCHANGERATE { get; set; }

        [Required]
        [StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string RELATED_LOAN_REFERENCE_NUMBER { get; set; }

        public short SUBSECTORID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        [StringLength(50)]
        public string MISCODE { get; set; }

        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public double INTERESTRATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime MATURITYDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime BOOKINGDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal OVERDRAFTLIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal PASTDUEPRINCIPAL { get; set; }

        [Column(TypeName = "money")]
        public decimal PASTDUEINTEREST { get; set; }

        [Column(TypeName = "money")]
        public decimal INTERESTONPASTDUEPRINCIPAL { get; set; }

        [Column(TypeName = "money")]
        public decimal INTERESTONPASTDUEINTEREST { get; set; }

        [Column(TypeName = "money")]
        public decimal PENALCHARGEAMOUNT { get; set; }

        public bool ISTEMPORARYOVERDRAFT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? APPROVEDBY { get; set; }

        [StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public short LOANSTATUSID { get; set; }

        public bool ISDISBURSED { get; set; }

        [StringLength(50)]
        public string DISBURSEDBY { get; set; }

        [StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DISBURSEDATE { get; set; }

        public int? OPERATIONID { get; set; }

        public bool DISCHARGELETTER { get; set; }

        public bool SUSPENDINTEREST { get; set; }

        public short DAYCOUNTCONVENTIONID { get; set; }

        public int INT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int EXT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int USER_PRUDENTIAL_GUIDE_STATUSID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NPLDATE { get; set; }

        [StringLength(50)]
        public string SERIALNUMBER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE1 { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual TBL_LOAN_REVOLVING_TYPE TBL_LOAN_REVOLVING_TYPE { get; set; }

        public virtual TBL_LOAN_STATUS TBL_LOAN_STATUS { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}
