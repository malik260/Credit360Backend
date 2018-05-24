namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_CHILDREN")]
    public partial class TBL_CUSTOMER_CHILDREN
    {
        [Key]
        public int CUSTOMERCHILDRENID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string CHILDNAME { get; set; }

        [Column(TypeName = "date")]
        public DateTime CHILDDATEOFBIRTH { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
