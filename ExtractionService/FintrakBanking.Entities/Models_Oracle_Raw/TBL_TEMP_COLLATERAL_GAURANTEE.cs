namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_GAURANTEE")]
    public partial class TBL_TEMP_COLLATERAL_GAURANTEE
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALGAURANTEEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [StringLength(100)]
        public string INSTITUTIONNAME { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(500)]
        public string GUARANTORADDRESS { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal GUARANTEEVALUE { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string TAXNUMBER { get; set; }

        [StringLength(50)]
        public string BVN { get; set; }

        [StringLength(50)]
        public string RCNUMBER { get; set; }

        [StringLength(50)]
        public string PHONENUMBER1 { get; set; }

        [StringLength(50)]
        public string PHONENUMBER2 { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [StringLength(50)]
        public string RELATIONSHIP { get; set; }

        [StringLength(50)]
        public string RELATIONSHIPDURATION { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
