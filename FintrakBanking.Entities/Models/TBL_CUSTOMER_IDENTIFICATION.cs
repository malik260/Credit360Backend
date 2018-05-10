namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_IDENTIFICATION")]
    public partial class TBL_CUSTOMER_IDENTIFICATION
    {
        [Key]
        public int IDENTIFICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(25)]
        public string IDENTIFICATIONNO { get; set; }

        public int? IDENTIFICATIONMODEID { get; set; }

        [StringLength(200)]
        public string ISSUEPLACE { get; set; }

        [StringLength(200)]
        public string ISSUEAUTHORITY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_IDENTI_MODE_TYPE TBL_CUSTOMER_IDENTI_MODE_TYPE { get; set; }
    }
}
