using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICollateralCustomerRepository
    {
        #region Collateral
        Task<bool> AddCollateral(CollateralViewModel entity);
        #endregion Collateral

        #region Collateral tbl_Customer
        //Task<bool> AddCollateralCustomer(CollateralCustomer entity);
        //Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user);
        //Task<bool> UpdateCollateralCustomer(int colleralCustomerId, CollateralCustomer entity);
        IEnumerable<CollateralCustomer> GetCollateralCustomerByParam(CustomerControllerRequestViewModel entity);
        IEnumerable<CollateralCustomer> GetCollateralCustomerByCustomerId(int customerId);

        bool IsCollateralDocExists(string docName);
        #endregion Collateral tbl_Customer

        #region Property
        //Task<bool> AddCollateralProperty(CollateralDeposit entity);
        //Task<bool> DeleteCollateralProperty(int propertyTypeId, UserInfo user);
        //Task<bool> UpdateCollateralProperty(int propertyTypeId, CollateralDeposit entity);
        IEnumerable<CollateralProperty> GetCollateralPropertyById(int propertyTypeId, int companyId);
        IEnumerable<CollateralProperty> GetCollateralPropertyByCollateralCustomerId(int CollateralCustomerId, int companyId);
        #endregion Property

        #region Deposit
        //Task<bool> AddCollateralDeposit(CollateralDeposit entity);
        //Task<bool> DeleteCollateralDeposit(int termDepositTranAccId, UserInfo user);
        //Task<bool> UpdateCollateralDeposit(int termDepositTranAccId, CollateralDeposit entity);
        IEnumerable<CollateralDeposit> GetCollateralDepositById(int termDepositTranAccId, int companyId);
        IEnumerable<CollateralDeposit> GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId, int companyId);
        #endregion Deposit

        #region Machine Detail
        //Task<bool> AddCollateralMachineDetail(CollateralMachineDetail entity);
        //Task<bool> DeleteCollateralMachineDetail(int machineDetailId, UserInfo user);
        //Task<bool> UpdateCollateralMachineDetail(int machineDetailId, CollateralMachineDetail entity);
        IEnumerable<CollateralMachineDetail> GetCollateralMachineDetailById(int machineDetailId, int companyId);
        IEnumerable<CollateralMachineDetail> GetCollateralMachineByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Machine Detail

        #region Marketable Security
        //Task<bool> AddCollateralMarketableSecurity(CollateralMarketableSecurity entity);
        //Task<bool> DeleteCollateralMarketableSecurity(int securityTypeId, UserInfo user);
        //Task<bool> UpdateCollateralMarketableSecurity(int securityTypeId, CollateralMarketableSecurity entity);
        IEnumerable<CollateralMarketableSecurity> GetCollateralMarketableSecurityById(int securityTypeId, int companyId);
        IEnumerable<CollateralMarketableSecurity> GetCollateralMarketableSecurityByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Marketable Security

        #region Precious Metal
        //Task<bool> AddCollateralPreciousMetal(CollateralPreciousMetal entity);
        //Task<bool> DeleteCollateralPreciousMetal(int preciousMetalId, UserInfo user);
        //Task<bool> UpdateCollateralPreciousMetal(int preciousMetalId, CollateralPreciousMetal entity);
        IEnumerable<CollateralPreciousMetal> GetCollateralPreciousMetalById(int preciousMetalId, int companyId);
        IEnumerable<CollateralPreciousMetal> GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Precious Metal

        #region Insurance Policy
        //Task<bool> AddCollateralInsurancePolicy(CollateralInsurancePolicy entity);
        //Task<bool> DeleteCollateralInsurancePolicy(int insurancePolicyId, UserInfo user);
        //Task<bool> UpdateCollateralInsurancePolicy(int insurancePolicyId, CollateralInsurancePolicy entity);
        IEnumerable<CollateralInsurancePolicy> GetCollateralInsurancePolicyById(int insurancePolicyId, int companyId);
        IEnumerable<CollateralInsurancePolicy> GetCollateralInsurancePolicyByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Insurance Policy

        #region Gaurantee
        //Task<bool> AddCollateralGaurantee(CollateralGaurantee entity);
        //Task<bool> DeleteCollateralGaurantee(int gauranteeId, UserInfo user);
        //Task<bool> UpdateCollateralGaurantee(int gauranteeId, CollateralGaurantee entity);
        IEnumerable<CollateralGaurantee> GetCollateralGauranteeById(int gauranteeId, int companyId);
        IEnumerable<CollateralGaurantee> GetCollateralGauranteeByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Gaurantee

        #region Vehicle
        //Task<bool> AddCollateralVehicle(CollateralVehicle entity);
        //Task<bool> DeleteCollateralVehicle(int vehicleTypeId, UserInfo user);
        //Task<bool> UpdateCollateralVehicle(int vehicleTypeId, CollateralVehicle entity);
        IEnumerable<CollateralVehicle> GetCollateralVehicleById(int vehicleTypeId, int companyId);
        IEnumerable<CollateralVehicle> GetCollateralVehicleByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Vehicle

        #region Miscellaneous
        Task<bool> AddCollateralMiscellaneous(CollateralMiscellaneous entity);
        Task<bool> DeleteCollateralMiscellaneous(int miscellaneousId, UserInfo user);
        Task<bool> UpdateCollateralMiscellaneous(int miscellaneousId, CollateralMiscellaneous entity);
        IEnumerable<CollateralMiscellaneous> GetCollateralMiscellaneousById(int miscellaneousId, int companyId);
        IEnumerable<CollateralMiscellaneous> GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId, int companyId);
        #endregion Miscellaneous

        #region Miscellaneous Notes
        Task<bool> AddCollateralMiscNotes(CollateralMiscNotes entity);
        Task<bool> DeleteCollateralMiscNotes(int miscNoteId, UserInfo user);
        Task<bool> UpdateCollateralMiscNotes(int miscNoteId, CollateralMiscNotes entity);
        IEnumerable<CollateralMiscNotes> GetCollateralMiscNotesById(int miscNoteId);
        IEnumerable<CollateralMiscNotes> GetCollateralMiscNotesByMiscellaneousId(int miscellaneousId);
        #endregion Miscellaneous Notes

        #region Seniority Of Claims
        Task<bool> AddCollateralSeniorityOfClaims(CollateralSeniorityOfClaimsViewModel entity);
        Task<bool> DeleteCollateralSeniorityOfClaims(int seniorityOfClaimId, UserInfo user);
        Task<bool> UpdateCollateralSeniorityOfClaims(int seniorityOfClaimId, CollateralSeniorityOfClaimsViewModel entity);
        IEnumerable<CollateralSeniorityOfClaimsViewModel> GetCollateralSeniorityOfClaims();
        #endregion Seniority Of Claims
    }
}
