using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class AtcLodgmentDetailViewModel : GeneralEntity
    {
        public int atcLodgmentDetailId { get; set; }

        public string detail { get; set; }

        public decimal value { get; set; }

        public int uploadId { get; set; }
        public int atcLodgmentId { get; set; }
    }
}