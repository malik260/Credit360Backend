namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Call_Limit")]
    public partial class tbl_Call_Limit
    {
        [Key]
        public int CallLimitId { get; set; }

        public int JobTitleId { get; set; }

        [Column(TypeName = "money")]
        public decimal CallLimit { get; set; }

        public short FrequencyId { get; set; }

        public int CallLimitTypeId { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type { get; set; }

        public virtual tbl_Call_Limit_Type tbl_Call_Limit_Type { get; set; }
    }
}
