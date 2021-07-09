namespace FintrakBanking.Entities.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TBL_LINE_FACILITY_STATUS")]
    public partial class TBL_LINE_FACILITY_STATUS
    {

        [Key]
        public short LINESTATUSID { get; set; }

        [Required]
        public string STATUSNAME { get; set; }

        public bool ISACTIVE { get; set; }


    }
}


