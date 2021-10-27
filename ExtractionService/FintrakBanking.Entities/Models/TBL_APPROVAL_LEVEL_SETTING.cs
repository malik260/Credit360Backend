
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_APPROVAL_LEVEL_SETTING")]
    public partial class TBL_APPROVAL_LEVEL_SETTING
    {
        [Key]
        public int APPROVALLEVELSETTINGID { get; set; }
        [Required]
        public int APPROVALLEVELID { get; set; }
        [Required]
        //[StringLength(150)]
        public string SETTINGCODE { get; set; }
        [Required]
        //[StringLength(150)]
        public string SETTINGDESCRIPTION { get; set; }
    }
}
