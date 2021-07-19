namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_POLICY")]
    public partial class TBL_TEMP_COLLATERAL_POLICY
    {
        [Key]
        public int TEMPCOLLATERALINSURPOLICYID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public bool ISOWNEDBYCUSTOMER { get; set; }

        [Required]
        [StringLength(50)]
        public string INSURANCEPOLICYNUMBER { get; set; }

        [StringLength(100)]
        public string INSURANCETYPE { get; set; }

        [Column(TypeName = "money")]
        public decimal PREMIUMAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal POLICYAMOUNT { get; set; }

        [Required]
        [StringLength(10)]
        public string INSURANCECOMPANYNAME { get; set; }

    

        [Required]
        [StringLength(300)]
        public string INSURERADDRESS { get; set; }

        public DateTime POLICYSTARTDATE { get; set; }

        public DateTime ASSIGNDATE { get; set; }

        public short? RENEWALFREQUENCYTYPEID { get; set; }

        [StringLength(500)]
        public string INSURERDETAILS { get; set; }

        public DateTime POLICYRENEWALDATE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }
    }
}
