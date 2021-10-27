namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_RELEASE")]
    public partial class TBL_COLLATERAL_RELEASE
    {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        //public TBL_COLLATERAL_RELEASE()
        //{
        //    TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
        //    TBL_COLLATERAL_RELEASE_TYPE = new HashSet<TBL_COLLATERAL_RELEASE_TYPE>();
        //    TBL_COLLATERAL_RELEASE_STATUS = new HashSet<TBL_COLLATERAL_RELEASE_STATUS>();


        //}

        [Key]
        public int COLLATERALRELEASEID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public int COLLATERALRELEASETYPEID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public string DESCRIPTION { get; set; }


        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public bool? JOBREQUESTSENT { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_COLLATERAL_RELEASE_TYPE> TBL_COLLATERAL_RELEASE_TYPE { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_COLLATERAL_RELEASE_STATUS> TBL_COLLATERAL_RELEASE_STATUS { get; set; }

    }
}
