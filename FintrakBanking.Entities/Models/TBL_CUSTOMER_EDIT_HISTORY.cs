namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_EDIT_HISTORY")]
    public partial class TBL_CUSTOMER_EDIT_HISTORY
    {
        [Key]
        public int CUSTOMEREDITHISTORYID { get; set; }

        public int? CUSTOMERID { get; set; }

        [StringLength(100)]
        public string CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
