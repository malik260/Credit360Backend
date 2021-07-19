namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_STAFF
    {
        [Required]
        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [StringLength(10)]
        public string STAFFCODE { get; set; }

        [Required]
        [StringLength(20)]
        public string STAFFNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        [StringLength(20)]
        public string PHONE { get; set; }

        [StringLength(200)]
        public string EMAIL { get; set; }

        [StringLength(500)]
        public string ADDRESS1 { get; set; }

        [StringLength(500)]
        public string ADRESS2 { get; set; }

        [StringLength(10)]
        public string GENDER { get; set; }

        [Required]
        [StringLength(10)]
        public string BRANCHCODE { get; set; }

        public double DEPARTMENTCODE { get; set; }

        [StringLength(10)]
        public string SUPERVISORSTAFFCODE { get; set; }

        public double? RANKCODE { get; set; }

        [StringLength(20)]
        public string STATUS { get; set; }

        [StringLength(500)]
        public string TEAMSTRUCTURE { get; set; }

        [StringLength(20)]
        public string ACTIVE { get; set; }
    }
}
