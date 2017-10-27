namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Risk_Rating")]
    public partial class tbl_Customer_Risk_Rating
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_Risk_Rating()
        {
            tbl_Customer = new HashSet<tbl_Customer>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RiskRatingId { get; set; }

        [Required]
        [StringLength(200)]
        public string RiskRating { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public bool IsInvestmentGrade { get; set; }

        public double MaximumShareHolderFundPercentage { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }
    }
}
