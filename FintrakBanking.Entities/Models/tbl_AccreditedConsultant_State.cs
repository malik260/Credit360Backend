namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_AccreditedConsultant_State")]
    public partial class tbl_AccreditedConsultant_State
    {
        [Key]
        public int AccreditedConsultantStateCoveredID { get; set; }

        public int StateId { get; set; }

        public int AccreditedConsultantId { get; set; }

        public virtual tbl_AccreditedConsultant tbl_AccreditedConsultant { get; set; }
    }
}
