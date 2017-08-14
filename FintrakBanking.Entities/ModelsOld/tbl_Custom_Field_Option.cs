namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Custom_Field_Option")]
    public partial class tbl_Custom_Field_Option
    {
        [Key]
        public int CustomFieldOptionsId { get; set; }

        public int CustomFieldId { get; set; }

        [StringLength(50)]
        public string OptionsKey { get; set; }

        [StringLength(50)]
        public string OptionsValue { get; set; }
    }
}
