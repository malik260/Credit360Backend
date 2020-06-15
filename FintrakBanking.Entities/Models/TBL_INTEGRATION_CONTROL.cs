namespace FintrakBanking.Entities.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TBL_INTEGRATION_CONTROL")]
    public partial class TBL_INTEGRATION_CONTROL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short INTEGRATIONCONTROLID { get; set; }

        public bool USE_THIRPARTY_POSTING { get; set; }

        public bool USE_THIRDPARTY_LIEN { get; set; }

        public bool USE_THIRDPARTY_LIEN_RELEASE { get; set; }

        public bool USE_THIRDPARTY_EXCHANGERATE { get; set; }

    }
}
