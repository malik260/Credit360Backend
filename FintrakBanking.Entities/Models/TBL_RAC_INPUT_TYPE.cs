namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_INPUT_TYPE")]
    public partial class TBL_RAC_INPUT_TYPE
    {
        public TBL_RAC_INPUT_TYPE()
        {
            TBL_RAC_DEFINITION = new HashSet<TBL_RAC_DEFINITION>();
        }

        [Key]
        public int RACINPUTTYPEID { get; set; }

        [Required]
        //[StringLength(10)]
        public string INPUTTYPENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string INPUTTAG { get; set; }
        

        public virtual ICollection<TBL_RAC_DEFINITION> TBL_RAC_DEFINITION { get; set; }

    }
}
 