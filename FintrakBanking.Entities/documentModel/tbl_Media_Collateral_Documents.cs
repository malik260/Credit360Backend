namespace FintrakBanking.Entities.documentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Media_Collateral_Documents
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(400)]
        public string FileName { get; set; }

        [Required]
        [StringLength(10)]
        public string FileExtension { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        public DateTime SystemDateTime { get; set; }
    }
}
