using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("Tbl_CustomerUUSDocument")]

    public class TblCustomerUUSDocument
    {
        public int Id { get; set; }
        public long PmbId { get; set; }
        public string Item { get; set; } 
        public string Nhfno { get; set; }
        public string Images { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public byte[] Filedata { get; set; } 
        public int ItemId { get; set; }
    }
}
