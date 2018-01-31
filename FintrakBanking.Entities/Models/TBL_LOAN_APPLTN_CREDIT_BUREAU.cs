namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLTN_CREDIT_BUREAU")]
    public partial class TBL_LOAN_APPLTN_CREDIT_BUREAU
    {
        [Key]
        public int APPLICATIONCREDITBUREAUID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public short CREDITBUREAUID { get; set; }

        [Column(TypeName = "money")]
        public decimal CHARGEAMOUNT { get; set; }

        public bool ISCOMPLETED { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATECOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CREDIT_BUREAU TBL_CREDIT_BUREAU { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}
