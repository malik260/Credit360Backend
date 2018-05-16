namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_ITEM_POLICY")]
    public partial class TBL_COLLATERAL_ITEM_POLICY
    {
        [Key]
        public int POLICYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string POLICYREFERENCENUMBER { get; set; }

        [Required]
        [StringLength(250)]
        public string INSURANCECOMPANYNAME { get; set; }

        [StringLength(100)]
        public string INSURANCETYPE { get; set; }

        //[Column(TypeName = "money")]
        public decimal SUMINSURED { get; set; }

        //[Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        public bool HASEXPIRED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
