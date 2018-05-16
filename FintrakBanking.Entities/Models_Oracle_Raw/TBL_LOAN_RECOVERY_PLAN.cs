namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_RECOVERY_PLAN")]
    public partial class TBL_LOAN_RECOVERY_PLAN
    {
        public TBL_LOAN_RECOVERY_PLAN()
        {
            TBL_LOAN_RECOVERY_PLAN_PAYMNT = new HashSet<TBL_LOAN_RECOVERY_PLAN_PAYMNT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RECOVERYPLANID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int? AGENTID { get; set; }

        public decimal? AMOUNTOWED { get; set; }

        public decimal WRITEOFFAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual ICollection<TBL_LOAN_RECOVERY_PLAN_PAYMNT> TBL_LOAN_RECOVERY_PLAN_PAYMNT { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }
    }
}
