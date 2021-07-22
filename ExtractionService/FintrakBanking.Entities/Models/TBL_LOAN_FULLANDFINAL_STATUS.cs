
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_FULLANDFINAL_STATUS")]
    public partial class TBL_LOAN_FULLANDFINAL_STATUS
    {
        [Key]
        public short FULLANDFINALSTATUSID { get; set; }

        public string FULLANDFINALSTATUSNAME { get; set; }

    }
}
