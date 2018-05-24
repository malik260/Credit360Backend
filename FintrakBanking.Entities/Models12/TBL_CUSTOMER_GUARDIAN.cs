namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_GUARDIAN")]
    public partial class TBL_CUSTOMER_GUARDIAN
    {
        [Key]
        public int GUARDIANID { get; set; }

        [StringLength(100)]
        public string GUARDIANNAME { get; set; }

        [StringLength(20)]
        public string GUARDIANPHONE { get; set; }

        [StringLength(200)]
        public string GUARDIANADDRESS { get; set; }

        public int CUSTOMERID { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
