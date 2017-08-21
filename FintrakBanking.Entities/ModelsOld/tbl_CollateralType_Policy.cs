namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_CollateralType_Policy")]
    public partial class tbl_CollateralType_Policy
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_CollateralType_Policy()
        {
            tbl_Collateral_Property = new HashSet<tbl_Collateral_Property>();
        }

        [Key]
        public short CollateralType_PolicyId { get; set; }

        [StringLength(50)]
        public string PolicyNumber { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(250)]
        public string InsuranceCompany { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Property> tbl_Collateral_Property { get; set; }
    }
}
