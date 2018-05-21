namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKINGDOCUMENTS.TBL_DOC_COLLATERAL_VISITATION")]
    public partial class TBL_DOC_COLLATERAL_VISITATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DOCUMENTID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public int COLLATERALVISITATIONID { get; set; }

        [Required]
        [StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        [StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int CREATEDBY { get; set; }
    }
}
