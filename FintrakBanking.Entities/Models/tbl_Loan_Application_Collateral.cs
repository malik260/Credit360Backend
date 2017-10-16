namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Application_Collateral")]
    public partial class tbl_Loan_Application_Collateral
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Application_Collateral()
        {
            tbl_Loan_Application_Collateral_RefNo = new HashSet<tbl_Loan_Application_Collateral_RefNo>();
        }

        [Key]
        public int CustomerCollateralId { get; set; }

        [StringLength(50)]
        public string CertificateOfOwnership { get; set; }

        public int? CityId { get; set; }

        public int CollateralTypeId { get; set; }

        [StringLength(50)]
        public string DocumentTitle { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [StringLength(50)]
        public string LocationAddress { get; set; }

        [StringLength(250)]
        public string NearestBusStop { get; set; }

        [StringLength(250)]
        public string NearestLandmark { get; set; }

        [Required]
        [StringLength(500)]
        public string OtherInformations { get; set; }

        public int LoanApplicationId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public DateTime SystemDateTime { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application_Collateral_RefNo> tbl_Loan_Application_Collateral_RefNo { get; set; }
    }
}
