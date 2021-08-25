namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_SIGNATURE_DOCUMENT_TYPE")]
    public partial class TBL_SIGNATURE_DOCUMENT_TYPE
    {
        public TBL_SIGNATURE_DOCUMENT_TYPE()
        {
            TBL_SIGNATURE_DOCUMENT_STAFF = new HashSet<TBL_SIGNATURE_DOCUMENT_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DOCUMENTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string DOCUMENTTYPENAME { get; set; }

        public virtual ICollection<TBL_SIGNATURE_DOCUMENT_STAFF> TBL_SIGNATURE_DOCUMENT_STAFF { get; set; }
    }
}
