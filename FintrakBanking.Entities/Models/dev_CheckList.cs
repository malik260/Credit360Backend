namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class dev_CheckList
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string Checklist { get; set; }
    }
}
