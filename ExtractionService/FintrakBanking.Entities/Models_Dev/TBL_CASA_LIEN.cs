namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CASA_LIEN")]
    public partial class TBL_CASA_LIEN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LIENID { get; set; }

        [Required]
        [StringLength(50)]
        public string SOURCEREFERENCENUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTACCOUNTNUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string LIENREFERENCENUMBER { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public decimal LIENAMOUNT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public short LIENTYPEID { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int CREATEDBY { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA_LIEN_TYPE TBL_CASA_LIEN_TYPE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
