namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_CREDIT_BUREAU")]
    public partial class TBL_CUSTOMER_CREDIT_BUREAU
    {
        public TBL_CUSTOMER_CREDIT_BUREAU()
        {
            TBL_LOAN_APPLTN_CREDIT_BUREAU = new HashSet<TBL_LOAN_APPLTN_CREDIT_BUREAU>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERCREDITBUREAUID { get; set; }

        public int CUSTOMERID { get; set; }

        public int CREDITBUREAUID { get; set; }

        public int? COMPANYDIRECTORID { get; set; }

        public decimal CHARGEAMOUNT { get; set; }

        public int ISREPORTOKAY { get; set; }

        public int USEDINTEGRATION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? DATECOMPLETED { get; set; }

        public virtual TBL_CREDIT_BUREAU TBL_CREDIT_BUREAU { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_LOAN_APPLTN_CREDIT_BUREAU> TBL_LOAN_APPLTN_CREDIT_BUREAU { get; set; }
    }
}
