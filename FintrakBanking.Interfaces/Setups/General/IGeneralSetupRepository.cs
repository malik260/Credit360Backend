using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using System;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IGeneralSetupRepository
    {
        DateTime CalculateMaturityDate(DateTime effectiveDate, TenorModeEnum tenorModeId, int tenor);

        IEnumerable<LookupViewModel> GetAllTenorMode();

        IEnumerable<LookupViewModel> GetAllCurrency();

        IEnumerable<LookupViewModel> GetAllCustomerType();

        IEnumerable<LookupViewModel> GetAllDealClassificationType();

        IEnumerable<LookupViewModel> GetAllDayCount();

        IEnumerable<LookupViewModel> GetAllFeeAmortisationType();

        IEnumerable<LookupViewModel> GetAllDealTypes();

        IEnumerable<LookupViewModel> GetAllFSTypes();

        IEnumerable<LookupViewModel> GetAllFrequencyTypes();
        //Task<bool>  SaveProductGroup(ProductGroupViewModel group);
        DateTime GetApplicaionDate();

        IEnumerable<LookupViewModel> GetAllOperationTypes();

        IEnumerable<LookupViewModel> GetAllOperations();

        IEnumerable<LookupViewModel> GetOperations(short operationTypeId);
    }
}