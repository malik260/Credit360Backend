using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrCommentViewModel : GeneralEntity
    {
        public int psrCommentId { get; set; }

        public string comment { get; set; }

    }
}