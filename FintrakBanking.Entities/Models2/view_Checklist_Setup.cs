namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class view_Checklist_Setup
    {
        public int? APPROVALLEVELID { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(150)]
        public string LEVELNAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CHECKLIST_TYPEID { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public string CHECKLIST_TYPE_NAME { get; set; }
    }
}
