namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_COLLATERAL_ITEM_POLI")]
    public partial class TBL_TEMP_COLLATERAL_ITEM_POLI
    {
        [Key]
        public int TEMPPOLICYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        //[StringLength(50)]
        public string POLICYREFERENCENUMBER { get; set; }

        [Required]
        public int INSURANCECOMPANYID { get; set; }

        public int INSURANCETYPEID { get; set; }

        //[Column(TypeName = "money")]
        public decimal SUMINSURED { get; set; }

        public decimal? PREMIUMAMOUNT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        public int CREATEDBY { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public bool ISPOLICYAPPROVAL { get; set; }

        public string DESCRIPTION { get; set; }

        public int? PREMIUMPERCENT { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public string PREVIOUSINSURANCE { get; set; }
        public string COLLATERALDETAILS { get; set; }
    }
}
