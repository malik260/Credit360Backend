using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APPROVAL_GRID_LEVEL")]
    public class TBL_APPROVAL_GRID_LEVEL
    {
        [Key]
        public int APPROVALGRIDLEVELID { get; set; }
        public int APPROVALLEVELID { get; set; }
        public bool ISACTIVE { get; set; }
    }
}