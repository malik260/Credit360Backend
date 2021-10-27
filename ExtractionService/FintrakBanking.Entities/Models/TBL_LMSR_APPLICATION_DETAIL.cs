namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LMSR_APPLICATION_DETAIL")]
    public partial class TBL_LMSR_APPLICATION_DETAIL
    {
        public TBL_LMSR_APPLICATION_DETAIL()
        {
            // 4 ITEMS
        }

        [Key]
        public int LOANREVIEWAPPLICATIONID { get; set; }

        public int LOANAPPLICATIONID { get; set; }
        public int LOANID { get; set; }
        public short? CURRENCYID { get; set; }
        public short PRODUCTID { get; set; }
        public int OPERATIONID { get; set; }
        //public short APPROVEDLINESTATUSID { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public int CUSTOMERID { get; set; }

        // public short STATUSID { get; set; }
        public bool DELETED { get; set; }

        //[StringLength(2000)]
        public string REVIEWDETAILS { get; set; } 
        public short REVIEWSTAGEID { get; set; }

        //[StringLength(2000)]
        public string REPAYMENTTERMS { get; set; }

        public int REPAYMENTSCHEDULEID { get; set; }

        ////[StringLength(2000)]
        //public string REPAYMENTSCHEDULE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int PROPOSEDTENOR { get; set; }

        public double PROPOSEDINTERESTRATE { get; set; } 
        
        public decimal PROPOSEDAMOUNT { get; set; }

        public int APPROVEDTENOR { get; set; }

        public double APPROVEDINTERESTRATE { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        public bool OPERATIONPERFORMED { get; set; }

        //[StringLength(2000)]
        public string MANAGEMENTPOSITION { get; set; }

        public decimal? CUSTOMERPROPOSEDAMOUNT { get; set; }

        //[Required]
        ////[StringLength(200)]
        //public string LOANREFERENCENUMBER { get; set; }

        // public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        //public virtual TBL_CASA TBL_CASA { get; set; }

        // public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        // public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        //public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual TBL_LMSR_APPLICATION TBL_LMSR_APPLICATION { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }

        public virtual ICollection<TBL_LMSR_APPLICATION_COLLATRL2> TBL_LMSR_APPLICATION_COLLATRL2 { get; set; }

        public virtual ICollection<TBL_LMSR_APPLICATION_COVENANT> TBL_LMSR_APPLICATION_COVENANT { get; set; }

        public virtual ICollection<TBL_LMSR_APPLICATN_DETL_MTRIG> TBL_LMSR_APPLICATN_DETL_MTRIG { get; set; }

        public virtual ICollection<TBL_LMSR_TRANSACTION_DYNAMICS> TBL_LMSR_TRANSACTION_DYNAMICS { get; set; }

        public virtual ICollection<TBL_LMSR_CONDITION_PRECEDENT> TBL_LMSR_CONDITION_PRECEDENT { get; set; }
    }
}