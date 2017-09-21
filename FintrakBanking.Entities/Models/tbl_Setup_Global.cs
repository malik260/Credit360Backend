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

        public int CompanyId { get; set; }

        public double UnauthorisedOverdraft_InterestRate { get; set; }

        public double PastDueInDefault_InterestRate { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}
