using FintrakBanking.Common.Enum;
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

        public DateTime? startDateTime { get; set; }

        public DateTime? endDateTime { get; set; }
        
        public int eodOperationLogId { get; set; }

        public int eodOperationId { get; set; }

        public string eodOperation { get; set; }

        public DateTime? eodDate { get; set; }

        public int? eodStatusId { get; set; }

        public string eodStatus { get; set; }

        public new int? companyId { get; set; }

        public string errorInformation { get; set; }

    }
}


