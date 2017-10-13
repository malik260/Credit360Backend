using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Finance
{
    public class EndOfDayViewModel : GeneralEntity
    {
        //public DateTime date { get; set; }
    }

    public class FinanceEndofdayViewModel : GeneralEntity
    {
        public int endOfDayId { get; set; }

        public DateTime date { get; set; }

        public DateTime startDateTime { get; set; }

        public DateTime? endDateTime { get; set; }


    }
}
