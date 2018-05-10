namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLTN_CREDIT_BUREAU")]
    public partial class TBL_LOAN_APPLTN_CREDIT_BUREAU
    {
        [Key]
        public int APPLICATIONCREDITBUREAUID { get; set; }

        public int CUSTOMERCREDITBUREAUID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_CUSTOMER_CREDIT_BUREAU TBL_CUSTOMER_CREDIT_BUREAU { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}
