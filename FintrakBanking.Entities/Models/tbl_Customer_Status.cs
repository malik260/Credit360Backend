namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Status")]
    public partial class tbl_Customer_Status
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CustomerStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerStatusName { get; set; }
    }
}
