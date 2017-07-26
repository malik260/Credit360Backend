using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ICollateralCustomerRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CollateralCustomerRepository : ICollateralCustomerRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IProductRepository product;
        public CollateralCustomerRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IProductRepository _product)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            product = _product;
        }


        #region Main Collateral Function Call
        public async Task<bool> AddCollateral(CollateralViewModel entity)
        {
            var response = false;
            var CollateralCustomerResponse = AddCollateralCustomer(entity.collateralCustomer);
            if (CollateralCustomerResponse.Result)
            {
                switch (entity.collateralCustomer.collateralTypeId)
                {
                    case (int)CollateralTypeEnum.MarketableSecurities:
                        response = await AddCollateralMarketableSecurity(entity.collateralMarketableSecurity);
                        break;

                    //case (int)CollateralTypeEnum.Property:
                    //    response = await AddCollateralProperty(entity.collateralProperty);
                    //    break;

                    case (int)CollateralTypeEnum.TermDeposit:
                        response = await AddCollateralDeposit(entity.collateralDeposit);
                        break;

                    case (int)CollateralTypeEnum.Vehicle:
                        response = await AddCollateralVehicle(entity.collateralVehicle);
                        break;

                    case (int)CollateralTypeEnum.PreciousMetal:
                        response = await AddCollateralPreciousMetal(entity.collateralPreciousMetal);
                        break;

                    case (int)CollateralTypeEnum.PlantAndMachinery:
                        response = await AddCollateralMachineDetail(entity.collateralMachineDetail);
                        break;

                    case (int)CollateralTypeEnum.Policy:
                        response = await AddCollateralInsurancePolicy(entity.collateralInsurancePolicy);
                        break;
                    case (int)CollateralTypeEnum.Gaurantee:
                        response = await AddCollateralGaurantee(entity.collateralGaurantee);
                        break;

                    //default:
                    //    return false;
                        
                }
                return response;
            }
            else
            {
                return false;
            }
                
        }
        #endregion Main Collateral Function Call

        #region Collateral tbl_Customer

        private async Task<bool> AddCollateralCustomer(CollateralCustomer entity)
        {

            var collateral = new tbl_Collateral_Customer
            {
                CollateralTypeId = entity.collateralTypeId,
                CollateralCode = entity.collateralCode,
                CollateralValue = entity.collateralValue,
                CollateralValueDate = entity.collateralValueDate,
                CustomerId = entity.customerId,
                Quantity = entity.quantity,
                ReleaseDate = entity.releaseDate,
                ReleaseCollateral = entity.releaseCollateral,
                ChargeTypeId = entity.chargeTypeId,
                CurrencyId =(short) entity.currencyId,
                LimitContribution = entity.limitContribution,
                GraceDays = entity.graceDays,
                SeniorityOfClaimId = entity.seniorityOfClaimId,
                LendableMargin = entity.lendableMargin,
                StartDate = entity.startDate,
                EndDate = entity.endDate,
                RevisionDate  = entity.revisionDate,
                RequireFieldInvestigation = entity.requireFieldInvestigation,
                RequireValuation = entity.requireValuation,
                RequireCheck = entity.requireCheck,
                AllowShare = entity.allowShare,
                RevaluationDate = entity.revaluationDate,
                LastValuationDate = entity.lastValuationDate,
                ValuationSource  = entity.valuationSource,
                ValuationAmount = entity.valuationAmount,
                CamRefNumber = entity.camrefNo,
                DateTimeCreated = genSetup.GetApplicaionDate().Date,
                CreatedBy = entity.createdBy,
                ApprovalStatus = entity.approvalStatus,
                DateActedOn = entity.dateActedOn,
                ActedOnBy = entity.actedOnBy

            };

            context.tbl_Collateral_Customer.Add(collateral);


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added collateral with code: { entity.collateralCode} at the value of '{entity.collateralValue}'",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        private async Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user)
        {
            var collateral = context.tbl_Collateral_Customer.Find(colleralCustomerId);
            collateral.Deleted = true;
            collateral.DeletedBy = (int)user.staffId;
            collateral.DateTimeDeleted = genSetup.GetApplicaionDate();
            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted collateral with code: {collateral.CollateralCode} valued at { collateral.CollateralValue}",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);


            //end of Audit section -----------------------
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<CollateralCustomer> CollateralCustomer(int customerId)
        {
            return (from c in context.tbl_Collateral_Customer
                    join t in context.tbl_Collateral_Type on c.CollateralTypeId equals t.CollateralTypeId
                    where c.Deleted == false && c.CustomerId == customerId
                    select new CollateralCustomer
                    {
                        collateralTypeId = c.CollateralTypeId,
                        collateralTypeName = t.CollateralTypeName,
                        collateralCode = c.CollateralCode,
                        collateralValue = c.CollateralValue,
                        collateralValueDate = c.CollateralValueDate,
                        customerId = c.CustomerId,
                        quantity = c.Quantity,
                        releaseDate = c.ReleaseDate,
                        releaseCollateral = c.ReleaseCollateral,
                        chargeTypeId =(short) c.ChargeTypeId,
                        currencyId = c.CurrencyId,
                        limitContribution = c.LimitContribution,
                        graceDays = c.GraceDays,
                        seniorityOfClaimId = c.SeniorityOfClaimId,
                        lendableMargin = c.LendableMargin,
                        startDate = c.StartDate,
                        endDate = c.EndDate,
                        revisionDate = c.RevisionDate,
                        requireFieldInvestigation = c.RequireFieldInvestigation,
                        requireValuation = c.RequireValuation,
                        requireCheck = c.RequireCheck,
                        allowShare = c.AllowShare,
                        revaluationDate = c.RevaluationDate,
                        lastValuationDate = c.LastValuationDate,
                        valuationSource = c.ValuationSource,
                        valuationAmount = c.ValuationAmount,
                        camrefNo = c.CamRefNumber,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = c.CreatedBy

                    });
        }

        public IEnumerable<CollateralCustomer> GetCollateralCustomerByParam(CustomerControllerRequestViewModel entity)
        {
            var collateral = CollateralCustomer(entity.customerId);
            return collateral.Where(c => c.collateralTypeId == entity.collateralTypeId);
        }

        public IEnumerable<CollateralCustomer> GetCollateralCustomerByCustomerId(int customerId)
        {
            return CollateralCustomer(customerId);
        }

        private async Task<bool> UpdateCollateralCustomer(int colleralCustomerId, CollateralCustomer entity)
        {
            var collateral = context.tbl_Collateral_Customer.Find(colleralCustomerId);
            collateral.CollateralTypeId = entity.collateralTypeId;
            collateral.CollateralValue = entity.collateralValue;
            collateral.CollateralValueDate = entity.collateralValueDate;
            collateral.CustomerId = entity.customerId;
            collateral.Quantity = entity.quantity;
            collateral.ReleaseDate = entity.releaseDate;
            collateral.ReleaseCollateral = entity.releaseCollateral;
            collateral.DateTimeUpdated = genSetup.GetApplicaionDate().Date;
            collateral.LastUpdatedBy = entity.lastUpdatedBy;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
                StaffId = entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Update collateral with code: { entity.collateralCode} value { entity.collateralValue}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);


            return await context.SaveChangesAsync() != 0;
        }

        public bool IsCollateralDocExists(string docName)
        {
            return false;
            //return context.TblCollateralCustomer.Any(c => string.Equals(c.DocumentNo, docName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion Collateral tbl_Customer

        #region Property
        private async Task<bool> AddCollateralProperty(CollateralProperty entity)
        {
            var collateral = new tbl_Collateral_Property
            {
                CollateralPropertyId = entity.propertyTypeId,
                CollateralCustomerId = entity.collateralCustomerId,
                PropertyType = entity.propertyType,
                CityId = entity.cityId,
                CountryId = entity.countryId,
                PropertyAddress = entity.propertyAddress,
                ConstructionDate = entity.constructionDate,
                PurchaseDate = entity.purchaseDate,
                ZoneClassification = entity.zoneClassification,
                PropertyValueBaseTypeId = entity.propertyValueBaseTypeId,
            
                Haircut = entity.haircut,
                LastValuationDate = entity.lastValuationDate,
                ValuationSource = entity.valuationSource,
                ValuationAmount = entity.valuationAmount,
                OtherLendersChargeAmount = entity.otherLendersChargeAmount,
                Remark = entity.remark,
                DateTimeCreated = genSetup.GetApplicaionDate().Date,
                CreatedBy = entity.createdBy
            };

            context.tbl_Collateral_Property.Add(collateral);


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added collateral property '{ entity.propertyType}'", // for customer with code '{entity.accountNo}' $ AccountBalance '{entity.accountBalance}'",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        private Task<bool> DeleteCollateralProperty(int propertyTypeId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralProperty(int propertyTypeId, CollateralProperty entity)
        {
            return null;
        }

        private IEnumerable<CollateralProperty> CollateralProperty(int companyId)
        {
            return (from m in context.tbl_Collateral_Property
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralProperty
                    {
                        propertyTypeId = m.CollateralPropertyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        propertyType = m.PropertyType,
                        cityId = m.CityId,
                        countryId = m.CountryId,
                        propertyAddress = m.PropertyAddress,
                        constructionDate = m.ConstructionDate,
                        purchaseDate = m.PurchaseDate,
                        zoneClassification = m.ZoneClassification, 
                        haircut = m.Haircut,
                        lastValuationDate = m.LastValuationDate,
                        valuationSource = m.ValuationSource,
                        valuationAmount = m.ValuationAmount,
                        otherLendersChargeAmount = m.OtherLendersChargeAmount,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralProperty> GetCollateralPropertyById(int propertyTypeId, int companyId)
        {
            return CollateralProperty(companyId).Where(x => x.propertyTypeId == propertyTypeId).ToList();
        }

        public IEnumerable<CollateralProperty> GetCollateralPropertyByCollateralCustomerId(int CollateralCustomerId, int companyId)
        {
            return CollateralProperty(companyId).Where(x => x.collateralCustomerId == CollateralCustomerId).ToList();
        }
        #endregion Property

        #region Deposit
        private async Task<bool> AddCollateralDeposit(CollateralDeposit entity)
        {
            var collateral = new tbl_Collateral_Deposit
            {
                CollateralDepositId = entity.termDepositTranAccId,
                CollateralCustomerId = entity.collateralCustomerId,
                AccountType = entity.accountType,
                AccountNumber = entity.accountNo,
                AccountBalance = entity.accountBalance,
                Contribution = entity.contribution,
                MaturityDate = entity.maturityDate,
                Remark = entity.remark,
                DateTimeCreated = genSetup.GetApplicaionDate().Date,
                CreatedBy = entity.createdBy

            };

            context.tbl_Collateral_Deposit.Add(collateral);


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added collateral deposit with: AccountType '{ entity.accountType}',  AccountNo '{entity.accountNo}' $ AccountBalance '{entity.accountBalance}'",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        private Task<bool> DeleteCollateralDeposit(int termDepositTranAccId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralDeposit(int termDepositTranAccId, CollateralDeposit entity)
        {
            return null;
        }

        private IEnumerable<CollateralDeposit> CollateralDeposit(int companyId)
        {
            return (from m in context.tbl_Collateral_Deposit
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralDeposit
                    {
                        termDepositTranAccId = m.CollateralDepositId,
                        collateralCustomerId = m.CollateralCustomerId,
                        accountType = m.AccountType,
                        accountNo  = m.AccountNumber,
                        accountBalance = m.AccountBalance,
                        contribution = m.Contribution,
                        maturityDate = m.MaturityDate,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy

                    });
        }

        public IEnumerable<CollateralDeposit> GetCollateralDepositById(int termDepositTranAccId, int companyId)
        {
            return CollateralDeposit(companyId).Where(x => x.termDepositTranAccId == termDepositTranAccId).ToList();
        }

        public IEnumerable<CollateralDeposit> GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId, int companyId)
        {
            return CollateralDeposit(companyId).Where(x => x.collateralCustomerId == CollateralCustomerId).ToList();
        }
        #endregion Deposit

        #region Machine Detail
        private async Task<bool> AddCollateralMachineDetail(CollateralMachineDetail entity)
        {
            var collateral = new tbl_Collateral_Machine_Detail
            {
                CollateralMachineDetailId = entity.machineDetailId,
                CollateralCustomerId = entity.collateralCustomerId,
                MachineName = entity.machineDetails,
                Manufacturer = entity.manufacturer,
                ManufacturedYear = entity.manufacturedYear,
                PurchasedYear = entity.purchasedYear,
                MachineValueBaseTypeId = entity.machineValueBaseId,
                MachineryLocation = entity.machineryCondition,
                ReplacementValue = entity.replacementValue,
                ThirdPartyChargeAmount = entity.thirdPartyChargeAmount,
                MachineryCondition = entity.machineryCondition,
                IntendedUse = entity.intendedUse,
                DateTimeCreated = genSetup.GetApplicaionDate().Date,
                CreatedBy = entity.createdBy

            };

            context.tbl_Collateral_Machine_Detail.Add(collateral);


            // Audit Section ---------------------------
            var customer = context.tbl_Customer.Where(x => x.CustomerId == context.tbl_Collateral_Customer.Where(c => c.CollateralCustomerId == entity.collateralCustomerId).SingleOrDefault().CustomerId).SingleOrDefault();
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added collateral MachineDetail for: customer with customer code'{customer.CustomerCode }',  assessed Value '{entity.assessedValue}' & location '{entity.machineryLocation}'",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        private Task<bool> DeleteCollateralMachineDetail(int machineDetailId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralMachineDetail(int machineDetailId, CollateralMachineDetail entity)
        {
            return null;
        }

        private IEnumerable<CollateralMachineDetail> CollateralMachineDetail(int companyId)
        {
            return (from m in context.tbl_Collateral_Machine_Detail
                    join c in context.tbl_Collateral_Customer  on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralMachineDetail
                    {
                         machineDetailId = m.CollateralMachineDetailId,
                         collateralCustomerId = m.CollateralCustomerId,
                         machineDetails = m.MachineName,
                         manufacturer = m.Manufacturer,
                         manufacturedYear = m.ManufacturedYear,
                         purchasedYear = m.PurchasedYear,
                     
                         machineryLocation  = m.MachineryCondition,
                         replacementValue = m.ReplacementValue,
                         thirdPartyChargeAmount = m.ThirdPartyChargeAmount,
                         machineryCondition  = m.MachineryCondition,
                         intendedUse  = m.IntendedUse,
                         dateTimeCreated = genSetup.GetApplicaionDate().Date,
                         createdBy = m.CreatedBy

                    });
        }

        public IEnumerable<CollateralMachineDetail> GetCollateralMachineDetailById(int machineDetailId, int companyId)
        {
            return CollateralMachineDetail(companyId).Where(x => x.machineDetailId == machineDetailId).ToList();
        }

        public IEnumerable<CollateralMachineDetail> GetCollateralMachineByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralMachineDetail(companyId).Where(x=>x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Machine Detail

        #region Marketable Security
        private Task<bool> AddCollateralMarketableSecurity(CollateralMarketableSecurity entity)
        {
            return null;
        }

        private Task<bool> DeleteCollateralMarketableSecurity(int securityTypeId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralMarketableSecurity(int securityTypeId, CollateralMarketableSecurity entity)
        {
            return null;
        }

        private IEnumerable<CollateralMarketableSecurity> CollateralMarketableSecurity(int companyId)
        {
            return (from m in context.tbl_Collateral_Marketable_Security
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralMarketableSecurity
                    {
                        securityTypeId = m.CollateralMarketableSecurityId,
                        collateralCustomerId = m.CollateralCustomerId,
                        securityType = m.SecurityType,
                        securityCode = m.SecurityCode,
                        description = m.Description,
                        issuerName = m.IssuerName,
                        issuerRefNo = m.IssuerReferenceNumber,
                        unitValue = m.UnitValue,
                        noofUnits = m.NumberOfUnits,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy

                    });
        }

        public IEnumerable<CollateralMarketableSecurity> GetCollateralMarketableSecurityById(int securityTypeId, int companyId)
        {
            return CollateralMarketableSecurity(companyId).Where(x => x.securityTypeId == securityTypeId).ToList();
        }

        public IEnumerable<CollateralMarketableSecurity> GetCollateralMarketableSecurityByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralMarketableSecurity(companyId).Where(x => x.collateralCustomerId  == collateralCustomerId).ToList();
        }

        #endregion Marketable Security

        #region Precious Metal
        private Task<bool> AddCollateralPreciousMetal(CollateralPreciousMetal entity)
        {
            return null;
        }

        private Task<bool> DeleteCollateralPreciousMetal(int preciousMetalId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralPreciousMetal(int preciousMetalId, CollateralPreciousMetal entity)
        {
            return null;
        }

        private IEnumerable<CollateralPreciousMetal> CollateralPreciousMetal(int companyId)
        {
            return (from m in context.tbl_Collateral_PreciousMetal
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralPreciousMetal
                    {
                        preciousMetalId = m.CollateralPreciousMetalId,
                        collateralCustomerId = m.CollateralPreciousMetalId,
                        preciousMetal = m.PreciousMetal,
                        metalType = m.MetalType,
                        weighInGms = m.WeightInGrammes,
                        valuationAmount = m.ValuationAmount,
                        unitRate = m.UnitRate,
                        preciousMetalForm = m.PreciousMetalForm,
                        notes = m.Notes,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy

                    });
        }

        public IEnumerable<CollateralPreciousMetal> GetCollateralPreciousMetalById(int preciousMetalId, int companyId)
        {
            return CollateralPreciousMetal(companyId).Where(x => x.preciousMetalId == preciousMetalId).ToList();
        }
        public IEnumerable<CollateralPreciousMetal> GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralPreciousMetal(companyId).Where(x => x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Precious Metal

        #region Insurance Policy
        private Task<bool> AddCollateralInsurancePolicy(CollateralInsurancePolicy entity)
        {
            return null;
        }

        private Task<bool> DeleteCollateralInsurancePolicy(int insurancePolicyId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralInsurancePolicy(int insurancePolicyId, CollateralInsurancePolicy entity)
        {
            return null;
        }

        private IEnumerable<CollateralInsurancePolicy> CollateralInsurancePolicy(int companyId)
        {
            return (from m in context.tbl_Collateral_InsurancePolicy
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralInsurancePolicy
                    {
                        insurancePolicyId = m.CollateralInsurancePolicyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        policyNo = m.PolicyNumber,
                        insuranceAmount = m.InsuranceAmount,
                        startDate = m.StartDate,
                        premiumAmount = m.PremiumAmount,
                        assignmentDate = m.AssignmentDate,
                        insurerAddress = m.InsurerAddress,
                        insurerDetails = m.InsurerDetails,
                        renewalFrequencyId = m.RenewalFrequencyTypeId,
                        nextRenewalDate = m.NextRenewalDate,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralInsurancePolicy> GetCollateralInsurancePolicyById(int insurancePolicyId, int companyId)
        {
            return CollateralInsurancePolicy(companyId).Where(x => x.insurancePolicyId == insurancePolicyId).ToList();
        }
        public IEnumerable<CollateralInsurancePolicy> GetCollateralInsurancePolicyByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralInsurancePolicy(companyId).Where(x => x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Insurance Policy

        #region Gaurantee
        private Task<bool> AddCollateralGaurantee(CollateralGaurantee entity)
        {
            return null;
        }

        private Task<bool> DeleteCollateralGaurantee(int gauranteeId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralGaurantee(int gauranteeId, CollateralGaurantee entity)
        {
            return null;
        }

        private IEnumerable<CollateralGaurantee> CollateralGaurantee(int companyId)
        {
            return (from m in context.tbl_Collateral_Gaurantee
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralGaurantee
                    {
                        gauranteeId = m.CollateralGauranteeId,
                        collateralCustomerId = m.CollateralCustomerId,
                        guaranteeType = m.GuaranteeType,
                        guaranteeAmount = m.GuaranteeAmount,
                        guarantorCifno = m.GuarantorCIFNumber,
                        guarantorName = m.GuarantorName,
                        guarantorAddress = m.GuarantorAddress,
                        agreementDate = m.AgreementDate,
                        continuingGuarantee = m.ContinuingGuarantee,
                        guarantorOwnExposure = m.GuarantorOwnExposure,
                        totalGuaranteeAmount = m.TotalGuaranteeAmount,
                        revokeable = m.Revokeable,
                        revokeDate = m.RevokeDate,
                        rating = m.Rating,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralGaurantee> GetCollateralGauranteeById(int gauranteeId, int companyId)
        {
            return CollateralGaurantee(companyId).Where(x => x.gauranteeId == gauranteeId).ToList();
        }

        public IEnumerable<CollateralGaurantee> GetCollateralGauranteeByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralGaurantee(companyId).Where(x => x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Gaurantee

        #region Vehicle
        private Task<bool> AddCollateralVehicle(CollateralVehicle entity)
        {
            return null;
        }

        private Task<bool> DeleteCollateralVehicle(int vehicleTypeId, UserInfo user)
        {
            return null;
        }

        private Task<bool> UpdateCollateralVehicle(int vehicleTypeId, CollateralVehicle entity)
        {
            return null;
        }

        private IEnumerable<CollateralVehicle> CollateralVehicle(int companyId)
        {
            return (from m in context.tbl_Collateral_Vehicle
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralVehicle
                    {
                        vehicleTypeId = m.CollateralVehicleId ,
                        collateralCustomerId = m.CollateralCustomerId,  
                        vehicleType = m.VehicleType,
                        newOrused = m.NewOrUsed,
                        make = m.Make,
                        model = m.Model,
                        year = m.Year,
                        regnNo = m.RegistrationNumber,
                        chasisNo = m.ChasisNumber,
                        engineNo = m.EngineNumber,
                        owner = m.Owner,
                        regAuthority = m.RegAuthority,
                        resaleValue  = m.ResaleValue,
                        valuationDate = m.ValuationDate,
                        valuationAmount = m.ValuationAmount,
                        invoiceValue = m.InvoiceValue,
                        remark = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralVehicle> GetCollateralVehicleById(int vehicleTypeId, int companyId)
        {
            return CollateralVehicle(companyId).Where(x => x.vehicleTypeId == vehicleTypeId).ToList();
        }

        public IEnumerable<CollateralVehicle> GetCollateralVehicleByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return CollateralVehicle(companyId).Where(x => x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Vehicle

        #region Miscellaneous
        public Task<bool> AddCollateralMiscellaneous(CollateralMiscellaneous entity)
        {
            return null;
        }

        public Task<bool> DeleteCollateralMiscellaneous(int miscellaneousId, UserInfo user)
        {
            return null;
        }

        public Task<bool> UpdateCollateralMiscellaneous(int miscellaneousId, CollateralMiscellaneous entity)
        {
            return null;
        }

        private IEnumerable<CollateralMiscellaneous> Miscellaneous(int companyId)
        {
            return (from m in context.tbl_Collateral_Miscellaneous
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralMiscellaneous
                    {
                        miscellaneousId = m.CollateralMiscellaneousId,
                        collateralCustomerId = m.CollateralCustomerId,
                        collateralDesc = m.CollateralDescription,
                        units = m.Units,
                        unitValue = m.UnitValue,
                        remarks  = m.Remark,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralMiscellaneous> GetCollateralMiscellaneousById(int miscellaneousId, int companyId)
        {
            return Miscellaneous(companyId).Where(x => x.miscellaneousId == miscellaneousId).ToList();
        }

        public IEnumerable<CollateralMiscellaneous> GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId, int companyId)
        {
            return Miscellaneous(companyId).Where(x => x.collateralCustomerId == collateralCustomerId).ToList();
        }
        #endregion Miscellaneous

        #region Miscellaneous Notes
        public Task<bool> AddCollateralMiscNotes(CollateralMiscNotes entity)
        {
            return null;
        }

        public Task<bool> DeleteCollateralMiscNotes(int miscNoteId, UserInfo user)
        {
            return null;
        }

        public Task<bool> UpdateCollateralMiscNotes(int miscNoteId, CollateralMiscNotes entity)
        {
            return null;
        }

        private IEnumerable<CollateralMiscNotes> MiscNotes()
        {
            return (from m in context.tbl_Collateral_Miscellaneous_Notes
                    join c in context.tbl_Collateral_Miscellaneous on m.MiscellaneousId equals c.CollateralMiscellaneousId
                    where m.Deleted == false && c.Deleted == false
                    select new CollateralMiscNotes
                    {
                        miscNoteId = m.MiscellaneousNoteId,
                        miscellaneousId = m.MiscellaneousId,
                        columnName = m.ColumnName,
                        columnValue  = m.ColumnValue,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    });
        }

        public IEnumerable<CollateralMiscNotes> GetCollateralMiscNotesById(int miscNoteId)
        {
            return MiscNotes().Where(x => x.miscNoteId == miscNoteId).ToList();
        }

        public IEnumerable<CollateralMiscNotes> GetCollateralMiscNotesByMiscellaneousId(int miscellaneousId)
        {
            return MiscNotes().Where(x => x.miscellaneousId == miscellaneousId).ToList();
        }
        #endregion Miscellaneous Notes

        #region Seniority Of Claims
        public Task<bool> AddCollateralSeniorityOfClaims(CollateralSeniorityOfClaimsViewModel entity)
        {
            return null;
        }
        public Task<bool> DeleteCollateralSeniorityOfClaims(int seniorityOfClaimId, UserInfo user)
        {
            return null;
        }
        public Task<bool> UpdateCollateralSeniorityOfClaims(int seniorityOfClaimId, CollateralSeniorityOfClaimsViewModel entity)
        {
            return null;
        }

        public IEnumerable<CollateralSeniorityOfClaimsViewModel> GetCollateralSeniorityOfClaims()
        {
            return (from m in context.tbl_Collateral_SeniorityOfClaims
                    select new CollateralSeniorityOfClaimsViewModel
                    {
                        seniorityOfClaimId = m.CollateralSeniorityOfClaimId,
                        seniorityOfClaims = m.SeniorityOfClaims,
                        description = m.Description,
                       // dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        //createdBy = m.CreatedBy
                    });
        }

        #endregion Seniority Of Claims
    }
}

