namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_ORGANOGRAM")]
    public partial class TBL_STAFF_ORGANOGRAM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int STAFFID { get; set; }

        [Key]
        [StringLength(50)]
        public string STAFFCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        [StringLength(50)]
        public string RANK { get; set; }

        [StringLength(50)]
        public string JOBTITLE { get; set; }

        public short? STAFFSTATUSID { get; set; }

        [StringLength(50)]
        public string PARENTSTAFFCODE { get; set; }

        public int? COMPANYID { get; set; }

        public DateTime? DATETIMEREFRESHED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
