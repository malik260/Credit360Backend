using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrPerformanceEvaluationViewModel : GeneralEntity
    {
        public int psrPerformanceEvaluationId { get; set; }

        public decimal grossAmount { get; set; }

        public decimal amountReceived { get; set; }

        public decimal progressPayment { get; set; }

        public int certifiedValueWorkDone { get; set; }

        public int managementUnitValueWorkDone { get; set; }

        public int consultantValueWorkDone { get; set; }

        public decimal costVariation { get; set; }

        public int timeVariation { get; set; }

    }
}