namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_APPROVAL_SETUP")]
    public partial class TBL_APPROVAL_SETUP
    {

        [Key]
        public int APPROVALSETUPID { get; set; }
        public bool USEROUNDROBIN { get; set; }
        public bool ISRETAILONLYROUNDROBIN { get; set; }
    }
}
