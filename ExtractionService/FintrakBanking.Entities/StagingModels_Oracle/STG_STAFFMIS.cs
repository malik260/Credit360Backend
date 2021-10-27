namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKSTAGING.STG_STAFFMIS")]
    public partial class STG_STAFFMIS
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        [Required]
        [StringLength(500)]
        public string USERNAME { get; set; }

        [Key]
        [StringLength(500)]
        public string STAFFCODE { get; set; }

        [StringLength(500)]
        public string MISCODE { get; set; }

        [StringLength(500)]
        public string MISNAME { get; set; }

        [StringLength(500)]
        public string MISTYPE { get; set; }

        [StringLength(500)]
        public string FIRSTNAME { get; set; }

        [StringLength(500)]
        public string LASTNAME { get; set; }

        [StringLength(500)]
        public string MIDDLENAME { get; set; }

        [StringLength(4000)]
        public string PHONE { get; set; }

        [Required]
        [StringLength(720)]
        public string EMAIL { get; set; }

        [StringLength(4000)]
        public string ADDRESS { get; set; }

        [StringLength(500)]
        public string GENDER { get; set; }

        [StringLength(400)]
        public string BRANCHCODE { get; set; }

        [StringLength(500)]
        public string DEPARTMENTCODE { get; set; }

        [StringLength(700)]
        public string SUPERVISORSTAFFCODE { get; set; }

        [StringLength(500)]
        public string RANKCODE { get; set; }

        [StringLength(500)]
        public string STATUS { get; set; }

        [StringLength(500)]
        public string TEAMSTRUCTURE { get; set; }

        public bool? ACTIVE { get; set; }
    }
}
