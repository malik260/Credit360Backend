using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
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

        IEnumerable<LookupViewModel> GetSector();

        IEnumerable<LookupViewModel> GetSubsector();

        int GetLoanApplicationRef(); 

        IEnumerable<LookupViewModel> GetAllFrequencyTypes();
        //Task<bool>  SaveProductGroup(ProductGroupViewModel group);
        DateTime GetApplicationDate();

        IEnumerable<LookupViewModel> GetAllOperationTypes();

        IEnumerable<LookupViewModel> GetAllOperations();

        IEnumerable<LookupViewModel> GetOperations(short operationTypeId);

        IEnumerable<SectorViewModel> GetAllSectors();

        IEnumerable<SectorViewModel> GetSectorsBySubSectorId(short ssId);

        IEnumerable<SectorViewModel> GetAllSubSectors();

        bool Updatesector(SectorViewModel model, short id);

        IEnumerable<int> GetStaffApprovalLevelIds(int staffId, int operationId);
        //IEnumerable<int> GetRelievedStaffApprovalLevelIds(int staffId, int operationId);
    }
}