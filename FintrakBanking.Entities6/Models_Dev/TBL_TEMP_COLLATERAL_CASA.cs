namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_CASA")]
    public partial class TBL_TEMP_COLLATERAL_CASA
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCASAID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        public int ISOWNEDBYCUSTOMER { get; set; }

        public decimal AVAILABLEBALANCE { get; set; }

        public decimal EXISTINGLIENAMOUNT { get; set; }

        public decimal LIENAMOUNT { get; set; }

        public decimal SECURITYVALUE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }
    }
}
