using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralTypeViewModel : GenaralEntity
    {
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        public string details { get; set; }
    }

    public class CollateralTypeSubViewModel : GenaralEntity
    {
        public short collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public double haircut { get; set; }
        public int revaluationDuration { get; set; }
    }

    public class CollateralSeniorityOfClaimsViewModel : GenaralEntity
    {
        public short seniorityOfClaimId { get; set; }
        public string seniorityOfClaims { get; set; }
        public string description { get; set; }
    }

}
