namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_MODIFICATION")]
    public partial class TBL_CUSTOMER_MODIFICATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERMODIFICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public int TARGETID { get; set; }

        public short MODIFICATIONTYPEID { get; set; }

        public bool APPROVALCOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_MODIFICATN_TYPE TBL_CUSTOMER_MODIFICATN_TYPE { get; set; }
    }
}
