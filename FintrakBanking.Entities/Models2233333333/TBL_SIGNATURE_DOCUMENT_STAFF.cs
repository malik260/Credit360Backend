namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_SIGNATURE_DOCUMENT_STAFF")]
    public partial class TBL_SIGNATURE_DOCUMENT_STAFF
    {
        [Key]
        public int DOCUMENTSTAFFID { get; set; }

        public short DOCUMENTTYPEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE1 { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE2 { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE3 { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE4 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_SIGNATURE_DOCUMENT_TYPE TBL_SIGNATURE_DOCUMENT_TYPE { get; set; }
    }
}
