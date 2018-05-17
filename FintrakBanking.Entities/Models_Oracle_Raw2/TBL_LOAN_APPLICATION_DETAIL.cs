namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_DETAIL")]
    public partial class TBL_LOAN_APPLICATION_DETAIL
    {
        public TBL_LOAN_APPLICATION_DETAIL()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_COLLATRL2 = new HashSet<TBL_LOAN_APPLICATION_COLLATRL2>();
            TBL_LOAN_APPLICATION_COVENANT = new HashSet<TBL_LOAN_APPLICATION_COVENANT>();
            TBL_LOAN_APPLICATION_DETL_TRA = new HashSet<TBL_LOAN_APPLICATION_DETL_TRA>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION_DETL_BG = new HashSet<TBL_LOAN_APPLICATION_DETL_BG>();
            TBL_LOAN_APPLICATION_DETL_INV = new HashSet<TBL_LOAN_APPLICATION_DETL_INV>();
            TBL_LOAN_APPLICATION_DETL_EDU = new HashSet<TBL_LOAN_APPLICATION_DETL_EDU>();
            TBL_LOAN_APPLICATION_DETL_FEE = new HashSet<TBL_LOAN_APPLICATION_DETL_FEE>();
            TBL_LOAN_APPLICATION_DETL_LOG = new HashSet<TBL_LOAN_APPLICATION_DETL_LOG>();
            TBL_LOAN_APPLICATN_DETL_MTRIG = new HashSet<TBL_LOAN_APPLICATN_DETL_MTRIG>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BOOKING_REQUEST = new HashSet<TBL_LOAN_BOOKING_REQUEST>();
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_RATE_FEE_CONCESSION = new HashSet<TBL_LOAN_RATE_FEE_CONCESSION>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_TRANSACTION_DYNAMICS = new HashSet<TBL_LOAN_TRANSACTION_DYNAMICS>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANAPPLICATIONDETAILID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public int PROPOSEDPRODUCTID { get; set; }

        public int PROPOSEDTENOR { get; set; }

        public decimal PROPOSEDINTERESTRATE { get; set; }

        public decimal PROPOSEDAMOUNT { get; set; }

        public int APPROVEDPRODUCTID { get; set; }

        public int APPROVEDTENOR { get; set; }

        public decimal APPROVEDINTERESTRATE { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        public int CURRENCYID { get; set; }

        public decimal EXCHANGERATE { get; set; }

        public int SUBSECTORID { get; set; }

        public int STATUSID { get; set; }

        [Required]
        [StringLength(500)]
        public string LOANPURPOSE { get; set; }

        public decimal? EQUITYAMOUNT { get; set; }

        public int? EQUITYCASAACCOUNTID { get; set; }

        public int CONSESSIONAPPROVALSTATUSID { get; set; }

        [StringLength(3000)]
        public string CONSESSIONREASON { get; set; }

        public int HASDONECHECKLIST { get; set; }

        public int ISPOLITICALLYEXPOSED { get; set; }

        [StringLength(2000)]
        public string REPAYMENTTERMS { get; set; }

        [StringLength(2000)]
        public string REPAYMENTSCHEDULE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? EXPIRYDATE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_COLLATRL2> TBL_LOAN_APPLICATION_COLLATRL2 { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_TRA> TBL_LOAN_APPLICATION_DETL_TRA { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_INV> TBL_LOAN_APPLICATION_DETL_INV { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_EDU> TBL_LOAN_APPLICATION_DETL_EDU { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT1 { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_FEE> TBL_LOAN_APPLICATION_DETL_FEE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_LOG> TBL_LOAN_APPLICATION_DETL_LOG { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATN_DETL_MTRIG> TBL_LOAN_APPLICATN_DETL_MTRIG { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }

        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }

        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        public virtual ICollection<TBL_LOAN_RATE_FEE_CONCESSION> TBL_LOAN_RATE_FEE_CONCESSION { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_TRANSACTION_DYNAMICS> TBL_LOAN_TRANSACTION_DYNAMICS { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }
    }
}
