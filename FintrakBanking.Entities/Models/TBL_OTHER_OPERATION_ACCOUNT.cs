namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_OTHER_OPERATION_ACCOUNT")]
    public partial class TBL_OTHER_OPERATION_ACCOUNT
    {

        [Key]
        public int OTHEROPERATIONACCOUNTID { get; set; }

        [Required]
        public int OTHEROPERATIONID { get; set; }

        [Required]
        public int GLACCOUNTID { get; set; }

        public int? GLACCOUNTID2 { get; set; }

        [Required]
        public int COMPANYID { get; set; }

    }
}
