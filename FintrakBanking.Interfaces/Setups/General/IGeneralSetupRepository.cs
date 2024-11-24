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

        DateTime GetApplicationEODLastRefreshedDate();

        int GetLoanApplicationRef(); 

        IEnumerable<LookupViewModel> GetAllFrequencyTypes();
        //Task<bool>  SaveProductGroup(ProductGroupViewModel group);
        DateTime GetApplicationDate();

        IEnumerable<LookupViewModel> GetAllOperationTypes();

        IEnumerable<LookupViewModel> GetAllOperations();

        IEnumerable<LookupViewModel> GetOperations(short operationTypeId);

        IEnumerable<SectorViewModel> GetAllSectors();
        IEnumerable<GlobalSectorViewModel> GetAllGlobalSectors();
        bool UpdateGlobalSector(GlobalSectorViewModel model, int id);

        IEnumerable<SectorViewModel> GetSectorsBySubSectorId(short ssId);
        List<SectorViewModel> GetSubSectorsBySector(int sectorId);

        IEnumerable<SectorViewModel> GetAllSubSectors();

        bool UpdateSector(SectorViewModel model, short id);
        bool DeleteSector(int Id, UserInfo user);
        bool AddSector(SectorViewModel model);
        
        IEnumerable<int> GetStaffApprovalLevelIds(int staffId, int operationId);
        IEnumerable<int> GetStaffApprovalLevelIdsWithoutRelief(int staffId, int operationId);
        List<int> GetRouteLevels(int operationId, int depth);

        IEnumerable<LookupViewModel> GetRegionByType(int regionTypeId);

        IEnumerable<ProfileBusinessUnitViewModel> GetProfileBusinessUnits();
        List<int> GetStaffRlieved(int staffId);

      
    }
}