namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Content_PlaceHolder")]
    public partial class tbl_Content_PlaceHolder
    {
        [Key]
        public int ContentPlaceHolderId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContentPlaceHolder { get; set; }
    }
}
