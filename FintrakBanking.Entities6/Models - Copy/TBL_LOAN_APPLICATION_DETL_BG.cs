namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_DETL_BG")]
    public partial class TBL_LOAN_APPLICATION_DETL_BG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BONDID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int PRINCIPALID { get; set; }

        public decimal AMOUNT { get; set; }

        public int CURRENCYID { get; set; }

        public int ISTENORED { get; set; }

        public int ISBANKFORMAT { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        [StringLength(800)]
        public string APPROVAL_COMMENT { get; set; }

        [StringLength(50)]
        public string REFERENCENO { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int? APPROVEDBY { get; set; }

        public DateTime? APPROVEDDATETIME { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? CONTRACT_ENDDATE { get; set; }

        public DateTime? CONTRACT_STARTDATE { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRINCIPAL TBL_LOAN_PRINCIPAL { get; set; }
    }
}
