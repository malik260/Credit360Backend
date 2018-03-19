namespace FintrakBaking.BranchUpdateService.Staging_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_BRANCH
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BRANCHCODE { get; set; }

        [StringLength(500)]
        public string ADDRESSLINE1 { get; set; }

        [StringLength(500)]
        public string ADDRESSLINE2 { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string STATECODE { get; set; }

        [StringLength(10)]
        public string SHORTNAME { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string CITYCODE { get; set; }

        [StringLength(50)]
        public string STATENAME { get; set; }

        [StringLength(100)]
        public string CITYNAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public string BRANCHNAME { get; set; }
    }
}
