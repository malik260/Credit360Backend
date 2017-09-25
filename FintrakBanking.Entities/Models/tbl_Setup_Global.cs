namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Setup_Global")]
    public partial class tbl_Setup_Global
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short GlobalSetupId { get; set; }

        public double AuthorisedOverdraft_InterestRate { get; set; }

        public double DefaultPastDue_InterestRate { get; set; }

        public int RepaymentGracePeriod { get; set; }
    }
}
