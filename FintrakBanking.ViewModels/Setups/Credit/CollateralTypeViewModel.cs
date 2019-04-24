using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralTypeViewModel : GeneralEntity
    {
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        public string details { get; set; }
        public bool requireInsurancePolicy { get; set; }
        public bool requireVisitation { get; set; }
        public int? chargeGLAccountId { get; set; }
        public int position { get; set; }
        public int collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public string collateralClassificationName { get; set; }
        public int? collateralClassificationId { get; set; }
    }

    public class CollateralTypeSubViewModel : GeneralEntity
    {
        public short collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public double haircut { get; set; }
        public int revaluationDuration { get; set; }
    }

    public class CollateralSeniorityOfClaimsViewModel : GeneralEntity
    {
        public short seniorityOfClaimId { get; set; }
        public string seniorityOfClaims { get; set; }
        public string description { get; set; }
    }
    public class CollateralDocumentTypeViewModel : GeneralEntity
    {
        public short documentTypeId { get; set; }
        public string documentType { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }

    }
}
