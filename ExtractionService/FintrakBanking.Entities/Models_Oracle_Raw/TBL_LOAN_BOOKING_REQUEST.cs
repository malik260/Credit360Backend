namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_BOOKING_REQUEST")]
    public partial class TBL_LOAN_BOOKING_REQUEST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOAN_BOOKING_REQUESTID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public decimal AMOUNT_REQUESTED { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
