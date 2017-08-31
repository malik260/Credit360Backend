namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_CompanyInfomation")]
    public partial class tbl_Customer_CompanyInfomation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyInfomationId { get; set; }

        public int CustomerId { get; set; }

        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(200)]
        public string CompanyWebsite { get; set; }

        [StringLength(200)]
        public string CompanyEmail { get; set; }

        [StringLength(200)]
        public string RegisteredOffice { get; set; }

        [StringLength(200)]
        public string AnnualTurnOver { get; set; }

        [StringLength(200)]
        public string CorporateBusinessCategory { get; set; }

        [StringLength(200)]
        public string CreditRating { get; set; }

        [StringLength(200)]
        public string PreviousCreditRating { get; set; }

        public int? PaidUpCapital { get; set; }

        public int? AuthorisedCapital { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

    }
}
