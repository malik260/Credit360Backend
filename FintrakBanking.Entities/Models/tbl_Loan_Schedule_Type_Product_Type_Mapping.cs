namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Schedule_Type_Product_Type_Mapping")]
    public partial class tbl_Loan_Schedule_Type_Product_Type_Mapping
    {
        [Key]
        public int ScheduleProductTypeId { get; set; }

        public short ScheduleTypeId { get; set; }

        public short ProductTypeId { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Loan_Schedule_Type tbl_Loan_Schedule_Type { get; set; }
    }
}
