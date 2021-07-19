namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_FEE")]
    public partial class TBL_LOAN_FEE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_FEE()
        {
            TBL_LOAN_FEE_SCHEDULE = new HashSet<TBL_LOAN_FEE_SCHEDULE>();
        }

        [Key]
        public int LOANCHARGEFEEID { get; set; }
        public int? CASAACCOUNTID { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int CHARGEFEEID { get; set; }

        //[StringLength(3000)]
        public string DESCRIPTION { get; set; }
        public bool ISMANUAL { get; set; }

        public bool ISPOSTED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEERATEVALUE { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEEDEPENDENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal FEEAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal EARNEDFEEAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal TAXAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal EARNEDTAXAMOUNT { get; set; }

        public bool ISINTEGRALFEE { get; set; }

        public string FEESOURCEMODULE { get; set; }

        public bool ISRECURRING { get; set; }

        public short RECURRINGPAYMENTDAY { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }
        public int? LOANREVIEWOPERATIONID { get; set; }

        //public int? SOURCELOANID { get; set; }
        //public short? SOURCELOANSYSTEMTYPEID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? SOURCELOANID { get; set; }
        public short? SOURCELOANSYSTEMTYPEID { get; set; }
        public bool? ISDEFERRED { get; set; }
        public DateTime? EFFECTIVEDATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FEE_SCHEDULE> TBL_LOAN_FEE_SCHEDULE { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}
