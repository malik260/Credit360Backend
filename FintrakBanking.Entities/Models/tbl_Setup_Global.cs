namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_SETUP_GLOBAL")]
    public partial class TBL_SETUP_GLOBAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short GLOBALSETUPID { get; set; }

        public int COMPANYID { get; set; }

        public double UNAUTHORISEDOVERDRAFT_INTERESTRATE { get; set; }

        public double PASTDUEINDEFAULT_INTERESTRATE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
