namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_AUTHORISED_SIGNATORY")]
    public partial class TBL_AUTHORISED_SIGNATORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_AUTHORISED_SIGNATORY()
        {
            TBL_OPERATION_SIGNATORY = new HashSet<TBL_OPERATION_SIGNATORY>();
        }

        [Key]
        public int SIGNATORYID { get; set; }

        public string SIGNATORYNAME { get; set; }
        public string SIGNATORYTITLE { get; set; }
        public string SIGNATORYINITIALS { get; set; }
        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public ICollection<TBL_OPERATION_SIGNATORY> TBL_OPERATION_SIGNATORY { get; set; }
    }
}
