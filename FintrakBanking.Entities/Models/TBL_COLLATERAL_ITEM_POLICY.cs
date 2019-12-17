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

        //[Required]
        ////[StringLength(50)]
        public string POLICYREFERENCENUMBER { get; set; }

        //[Required]
        public int INSURANCECOMPANYID { get; set; }

        public int INSURANCETYPEID { get; set; }
        public int? POLICYSTATEID { get; set; }
        public bool DIFFERPOLICY { get; set; }

        //public string INSURANCETYPE { get; set; }

        //[Column(TypeName = "money")]
        public decimal SUMINSURED { get; set; }

        public decimal? PREMIUMAMOUNT { get; set; }

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

        public String DESCRIPTION { get; set; }

        public int? PREMIUMPERCENT { get; set; }

        public short? APPROVALSTATUSID { get; set; }

        //public  string POLICYSTATE { get; set; }
        public string COMPANYADDRESS { get; set; }
        //public int?  PREVIOUSINSURANCEID { get; set; }


        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
