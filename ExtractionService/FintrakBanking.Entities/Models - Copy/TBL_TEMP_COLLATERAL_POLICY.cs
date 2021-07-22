namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_POLICY")]
    public partial class TBL_TEMP_COLLATERAL_POLICY
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALINSURPOLICYID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ISOWNEDBYCUSTOMER { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string INSURANCEPOLICYNUMBER { get; set; }

        [StringLength(100)]
        public string INSURANCETYPE { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal PREMIUMAMOUNT { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal POLICYAMOUNT { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(10)]
        public string INSURANCECOMPANYNAME { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(300)]
        public string INSURERADDRESS { get; set; }

        [Key]
        [Column(Order = 8)]
        public DateTime POLICYSTARTDATE { get; set; }

        [Key]
        [Column(Order = 9)]
        public DateTime ASSIGNDATE { get; set; }

        public int? RENEWALFREQUENCYTYPEID { get; set; }

        [StringLength(500)]
        public string INSURERDETAILS { get; set; }

        [Key]
        [Column(Order = 10)]
        public DateTime POLICYRENEWALDATE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }
    }
}
