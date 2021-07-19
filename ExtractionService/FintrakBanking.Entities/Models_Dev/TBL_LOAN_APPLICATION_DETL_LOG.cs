namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_DETL_LOG")]
    public partial class TBL_LOAN_APPLICATION_DETL_LOG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOAN_APPLICATION_DETAIL_LOGID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public short APPROVEDPRODUCTID { get; set; }

        public int APPROVEDTENOR { get; set; }

        public double APPROVEDINTERESTRATE { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        public double EXCHANGERATE { get; set; }

        public short STATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
