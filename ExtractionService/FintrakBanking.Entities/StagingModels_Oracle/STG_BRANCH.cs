namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKSTAGING.STG_BRANCH")]
    public partial class STG_BRANCH
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        [Key]
        [StringLength(255)]
        public string BRANCHCODE { get; set; }

        [StringLength(255)]
        public string BRANCHNAME { get; set; }

        [StringLength(255)]
        public string COMPANYCODE { get; set; }

        [StringLength(255)]
        public string ADDRESSLINE1 { get; set; }

        [StringLength(255)]
        public string ADDRESSLINE2 { get; set; }

        [StringLength(255)]
        public string COMMENTS { get; set; }

        [StringLength(255)]
        public string STATECODE { get; set; }

        [StringLength(255)]
        public string CITYCODE { get; set; }

        [StringLength(255)]
        public string STATENAME { get; set; }

        [StringLength(255)]
        public string CITYNAME { get; set; }
    }
}
