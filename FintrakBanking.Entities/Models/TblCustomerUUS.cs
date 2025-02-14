using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("Tbl_CustomerUUS")]
    public class TblCustomerUUS
    {
        public int Id { get; set; }
        public long PmbId { get; set; }
        public string EmployeeNhfNumber { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public DateTime? DeferDate { get; set; }
        public int Option { get; set; }
        public int ItemId { get; set; }
        public string ReviewalComment { get; set; }
        public string ApprovalComment { get; set; }
    }
}
