namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_VALUER")]
    public partial class TBL_COLLATERAL_VALUER
    {
        [Key]
        public short COLLATERALVALUERID { get; set; }

        [Required]
        //[StringLength(50)]
        public string VALUERLICENCENUMBER { get; set; }

        [Required]
        //[StringLength(100)]
        public string NAME { get; set; }

        public short? VALUERTYPEID { get; set; }

        public int? COMPANYID { get; set; }

        public short? CITYID { get; set; }

        public short? COUNTRYID { get; set; }

        //[StringLength(50)]
        public string EMAILADDRESS { get; set; }

        //[StringLength(50)]
        public string PHONENUMBER { get; set; }

        //[StringLength(500)]
        public string ADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COLLATERAL_VALUER_TYPE TBL_COLLATERAL_VALUER_TYPE { get; set; }
    }
}
