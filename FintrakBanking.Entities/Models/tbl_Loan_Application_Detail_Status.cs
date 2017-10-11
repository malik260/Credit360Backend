namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Application_Detail_Status")]
    public partial class tbl_Loan_Application_Detail_Status
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short StatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; }
    }
}
