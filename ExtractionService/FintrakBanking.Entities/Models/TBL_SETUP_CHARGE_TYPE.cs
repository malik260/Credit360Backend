namespace FintrakBanking.Entities.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_SETUP_CHARGE_TYPE")]
    public partial class TBL_SETUP_CHARGE_TYPE
    {
        [Key]
        public short CHARGETYPEID { get; set; }


        //[StringLength(100)]
        public string CHARGETYPENAME { get; set; }

    }
}
