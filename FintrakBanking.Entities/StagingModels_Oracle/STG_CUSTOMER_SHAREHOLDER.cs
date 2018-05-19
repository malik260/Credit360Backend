namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKSTAGING.STG_CUSTOMER_SHAREHOLDER")]
    public partial class STG_CUSTOMER_SHAREHOLDER
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        [StringLength(255)]
        public string FIRSTNAME { get; set; }

        [StringLength(255)]
        public string LASTNAME { get; set; }

        [StringLength(255)]
        public string BVN { get; set; }

        [StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        [StringLength(255)]
        public string TYPE { get; set; }
    }
}
