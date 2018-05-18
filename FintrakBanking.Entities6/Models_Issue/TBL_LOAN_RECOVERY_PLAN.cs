namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_RECOVERY_PLAN")]
    public partial class TBL_LOAN_RECOVERY_PLAN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_RECOVERY_PLAN()
        {
            TBL_LOAN_RECOVERY_PLAN_PAYMNT = new HashSet<TBL_LOAN_RECOVERY_PLAN_PAYMNT>();
        }

        [Key]
        public int RECOVERYPLANID { get; set; }

        public int LOANID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int? AGENTID { get; set; }

        //[Column(TypeName = "money")]
        public decimal? AMOUNTOWED { get; set; }

        //[Column(TypeName = "money")]
        public decimal WRITEOFFAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_RECOVERY_PLAN_PAYMNT> TBL_LOAN_RECOVERY_PLAN_PAYMNT { get; set; }
    }
}
