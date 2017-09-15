using FintrakBanking.Interfaces.Credit;
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
using FintrakBanking.Interfaces.media;
using FintrakBanking.Interfaces.Setups.Credit;

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCollateralRepository : ICustomerCollateralRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IProductRepository product;
        private IMediaRepository media;
        private ICollateralTypeRepository collateralType;

        public CustomerCollateralRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail, IProductRepository _product,
            IMediaRepository _media,
            ICollateralTypeRepository _collateralType
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.product = _product;
            this.media = _media;
            this.collateralType = _collateralType;
        }

        #region New 

        // ADD

        public async Task<bool> AddCollateral(CollateralViewModel entity)
        {
            int collateralId = AddCollateralMainForm(entity);

            if (collateralId > 0)
            {
                switch (entity.collateralTypeId)
                {
                    case (int)CollateralTypeEnum.TermDeposit: AddDepositCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.PlantAndMachinery: AddEquipmentCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Miscellaneous: AddMiscellaneousCollateral(collateralId, entity); break;

                    default: break;
                }

                if (entity.hasInsurance) { AddItemInsurancePolicy(collateralId, entity); }

                bool saved = await context.SaveChangesAsync() != 0;

                if (saved) { return true; } // audit here

                DeleteCollateral(collateralId);
            }

            return false;
        }

        // UPDATE

        public async Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId)
        {
            UpdateCollateralMainForm(entity, collateralId);

            switch (entity.collateralTypeId)
            {
                case (int)CollateralTypeEnum.TermDeposit: UpdateDepositCollateral(entity); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: UpdateEquipmentCollateral(entity); break;
                case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(entity); break;

                default: break;
            }

            if (entity.hasInsurance) { UpdateItemInsurancePolicy(entity); }

            bool saved = await context.SaveChangesAsync() != 0;

            if (saved) { return true; } // audit here

            return false;
        }

        // MAIN collateral

        private int AddCollateralMainForm(CollateralViewModel model)
        {
            if (context.tbl_Collateral_Customer.Where(x => x.CollateralCode == model.collateralCode).Any() == true)
            {
                throw new Exception("The specified Collateral Code is already used in the system!");
            }

            var collateral = context.tbl_Collateral_Customer.Add(new tbl_Collateral_Customer
            {
                CollateralTypeId = model.collateralTypeId,
                CollateralSubTypeId = model.collateralSubTypeId,
                CollateralCode = model.collateralCode,
                CompanyId = model.companyId,
                AllowSharing = model.allowSharing,
                IsLocationBased = model.isLocationBased,
                ValuationCycle = model.valuationCycle,
                HairCut = model.haircut,
                CurrencyId = model.currencyId,
                CustomerId = model.customerId,
                CamRefNumber = model.camRefNumber,
                CreatedBy = model.createdBy,
                DateTimeCreated = genSetup.GetApplicationDate()
            });

            if (context.SaveChanges() == 1) // may not be needed
            {
                return collateral.CollateralCustomerId;
            }

            return 0;
        }

        private void UpdateCollateralMainForm(CollateralViewModel model, int collateralId)
        {
            var collateral = context.tbl_Collateral_Customer.Find(collateralId);
            collateral.CollateralTypeId = model.collateralTypeId;
            collateral.CollateralSubTypeId = model.collateralSubTypeId;
            collateral.CollateralCode = model.collateralCode;
            collateral.AllowSharing = model.allowSharing;
            collateral.IsLocationBased = model.isLocationBased;
            collateral.ValuationCycle = model.valuationCycle;
            collateral.HairCut = model.haircut;
            collateral.CurrencyId = model.currencyId;
            collateral.CamRefNumber = model.camRefNumber;
            collateral.LastUpdatedBy = model.lastUpdatedBy;
            collateral.DateTimeUpdated = genSetup.GetApplicationDate();
        }

        private void DeleteCollateral(int collateralId)
        {
            var collateral = context.tbl_Collateral_Customer.Find(collateralId);
            collateral.Deleted = true; // audit here
            context.SaveChanges();
        }


        // EQUIPMENT collateral

        private void AddEquipmentCollateral(int collateralId, CollateralViewModel entity)
        {
            context.tbl_Collateral_Plant_And_Equipment.Add(new tbl_Collateral_Plant_And_Equipment
            {
                CollateralCustomerId = collateralId,
                MachineName = entity.machineName,
                Description = entity.description,
                MachineNumber = entity.machineNumber,
                ManufacturerName = entity.manufacturerName,
                YearOfManufacture = entity.yearOfManufacture,
                YearOfPurchase = entity.yearOfPurchase,
                ValueBaseTypeId = (short)entity.valueBaseTypeId,
                MachineCondition = entity.machineCondition,
                MachineryLocation = entity.machineryLocation,
                ReplacementValue = entity.replacementValue,
                EquipmentSize = entity.equipmentSize,
                IntendedUse = entity.intendedUse,
            });
        }

        private void UpdateEquipmentCollateral(CollateralViewModel model)
        {
            var collateral = context.tbl_Collateral_Plant_And_Equipment
                .Where(x => x.CollateralCustomerId == model.collateralId)
                .FirstOrDefault();

            collateral.MachineName = model.machineName;
            collateral.Description = model.description;
            collateral.MachineNumber = model.machineNumber;
            collateral.ManufacturerName = model.manufacturerName;
            collateral.YearOfManufacture = model.yearOfManufacture;
            collateral.YearOfPurchase = model.yearOfPurchase;
            collateral.ValueBaseTypeId = (short)model.valueBaseTypeId;
            collateral.MachineCondition = model.machineCondition;
            collateral.MachineryLocation = model.machineryLocation;
            collateral.ReplacementValue = model.replacementValue;
            collateral.EquipmentSize = model.equipmentSize;
            collateral.IntendedUse = model.intendedUse;
        }

        // FIX DEPOSIT collateral

        private void AddDepositCollateral(int collateralId, CollateralViewModel entity)
        {
            context.tbl_Collateral_Deposit.Add(new tbl_Collateral_Deposit
            {
                CollateralCustomerId = collateralId,
                DealReferenceNumber = entity.dealReferenceNumber,
                AccountNumber = entity.accountNumber,
                ExistingLienAmount = entity.existingLienAmount,
                LienAmount = entity.lienAmount,
                AvailableBalance = entity.availableBalance,
                SecurityValue = entity.securityValue,
                MaturityDate = entity.maturityDate,
                MaturityAmount = entity.maturityAmount,
                Remark = entity.remark,
            });
        }

        private void UpdateDepositCollateral(CollateralViewModel entity)
        {
            var collateral = context.tbl_Collateral_Deposit
                .Where(x => x.CollateralCustomerId == entity.collateralId)
                .FirstOrDefault();

            collateral.DealReferenceNumber = entity.dealReferenceNumber;
            collateral.AccountNumber = entity.accountNumber;
            collateral.ExistingLienAmount = entity.existingLienAmount;
            collateral.LienAmount = entity.lienAmount;
            collateral.AvailableBalance = entity.availableBalance;
            collateral.SecurityValue = entity.securityValue;
            collateral.MaturityDate = entity.maturityDate;
            collateral.MaturityAmount = entity.maturityAmount;
            collateral.Remark = entity.remark;
        }

        // MISCELALEOUS

        private void AddMiscellaneousCollateral(int collateralId, CollateralViewModel entity)
        {
            throw new NotImplementedException();
        }

        private void UpdateMiscellaneousCollateral(CollateralViewModel entity)
        {
            throw new NotImplementedException();
        }

        // ITEM INSURANCE

        private void AddItemInsurancePolicy(int collateralId, CollateralViewModel entity)
        {
            context.tbl_Collateral_Item_Policy.Add(new tbl_Collateral_Item_Policy
            {
                CollateralCustomerId = collateralId,
                PolicyReferenceNumber = entity.referenceNumber,
                InsuranceCompanyName = entity.insuranceCompany,
                CoverageAmount = entity.coverageAmount,
                StartDate = entity.startDate,
                EndDate = entity.expiryDate,
            });
        }

        private void UpdateItemInsurancePolicy(CollateralViewModel entity)
        {
            var collateral = context.tbl_Collateral_Item_Policy
                .Where(x => x.CollateralCustomerId == entity.collateralId)
                .FirstOrDefault();

            collateral.PolicyReferenceNumber = entity.referenceNumber;
            collateral.InsuranceCompanyName = entity.insuranceCompany;
            collateral.CoverageAmount = entity.coverageAmount;
            collateral.StartDate = entity.startDate;
            collateral.EndDate = entity.expiryDate;
        }

        // GET MAIN INFO

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int companyId)
        {
            var collateral = context.tbl_Collateral_Customer.Where(x => x.Deleted == false
                && x.CompanyId == companyId
                && x.CustomerId == customerId
            )
            .Select(x => new CollateralViewModel
            {
                collateralId = x.CollateralCustomerId,
                collateralTypeId = x.CollateralTypeId,
                collateralSubTypeId = x.CollateralSubTypeId,
                customerId = x.CustomerId,
                currencyId = x.CurrencyId,
                collateralCode = x.CollateralCode,
                camRefNumber = x.CamRefNumber,
                allowSharing = x.AllowSharing,
                isLocationBased = x.IsLocationBased,
                valuationCycle = x.ValuationCycle,
                haircut = x.HairCut,
                approvalStatus = x.ApprovalStatus,
            }).ToList();

            return collateral;
        }

        // GET TYPE SPICIFIC & INSURANCE DETAILS

        public CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            var data = new CollateralViewModel();
            switch (typeId)
            {
                case (int)CollateralTypeEnum.TermDeposit: data = GetCollateralDeposit(collateralId); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: data = GetCollateralMachinery(collateralId); break;

                default:
                    break;
            }

            return data;
        }

        private CollateralViewModel GetCollateralMachinery(int collateralId)
        {
            var specifics = context.tbl_Collateral_Plant_And_Equipment.FirstOrDefault(x => x.CollateralCustomerId == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.CollateralCustomerId,
                machineName = specifics.MachineName,
                description = specifics.Description,
                machineNumber = specifics.MachineNumber,
                manufacturerName = specifics.ManufacturerName,
                yearOfManufacture = specifics.YearOfManufacture,
                yearOfPurchase = specifics.YearOfPurchase,
                valueBaseTypeId = specifics.ValueBaseTypeId,
                machineCondition = specifics.MachineCondition,
                machineryLocation = specifics.MachineryLocation,
                replacementValue = specifics.ReplacementValue,
                equipmentSize = specifics.EquipmentSize,
                intendedUse = specifics.IntendedUse,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        private CollateralViewModel GetCollateralDeposit(int collateralId)
        {
            var specifics = context.tbl_Collateral_Deposit.FirstOrDefault(x => x.CollateralCustomerId == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.CollateralCustomerId,
                collateralDepositId = specifics.CollateralDepositId,
                dealReferenceNumber = specifics.DealReferenceNumber,
                accountNumber = specifics.AccountNumber,
                existingLienAmount = specifics.ExistingLienAmount,
                lienAmount = specifics.LienAmount,
                availableBalance = specifics.AvailableBalance,
                securityValue = specifics.SecurityValue,
                maturityDate = specifics.MaturityDate,
                maturityAmount = specifics.MaturityAmount,
                remark = specifics.Remark,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        private CollateralViewModel GetCollateralInsurancePolicy(CollateralViewModel details)
        {
            var insurance = context.tbl_Collateral_Item_Policy.FirstOrDefault(x => x.CollateralCustomerId == details.collateralId);
            if (insurance != null)
            {
                details.referenceNumber = insurance.PolicyReferenceNumber;
                details.insuranceCompany = insurance.InsuranceCompanyName;
                details.coverageAmount = insurance.CoverageAmount;
                details.startDate = insurance.StartDate;
                details.expiryDate = insurance.EndDate;
            }
            return details;
        }

        #endregion New 

        /*  
         *  db changes
         *  
         *  tbl_Collateral_Customer
         *      Add
         *          int CollateralSubTypeId
         *          
         *   tbl_Collateral_Deposit
         *      Remove
         *          CompanyId
         *          CollateralSubTypeId
         *          AccountType
         *      Change
         *          ExistingLienAmount datatype to (bit)
         */

        #region Collateral Customer 

        public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomer(int customerId, int companyId)
        {
            var collateral = GetCollateralCustomerByCustomerId(customerId, companyId).Where(x => x.deleted == false);

            foreach (var c in collateral)
            {
                c.collateralProperty = GetCollateralPropertyByCollateralCustomerId(c.collateralCustomerId);
                c.collateralMachineDetail = GetCollateralMachineDetailByCollateralCustomerId(c.collateralCustomerId);
                c.collateralMarketableSecurity = GetCollateralMarketableSecurityByCollateralCustomerId(c.collateralCustomerId);
                c.collateralPreciousMetal = GetCollateralPreciousMetalByCollateralCustomerId(c.collateralCustomerId);
                c.collateralInsurancePolicy = GetCollateralInsurancePolicyByCollateralCustomerId(c.collateralCustomerId);
                c.collateralGaurantee = GetCollateralGauranteeByCollateralCustomerId(c.collateralCustomerId);
                c.collateralVehicle = GetCollateralVehicleByCollateralCustomerId(c.collateralCustomerId);
                //c.collateralMiscellaneous = GetCollateralMiscellaneousByCollateralCustomerId(c.collateralCustomerId);
                c.collateralCustomerPolicy = GetCollateralCustomerPolicyByCollateralCustomerId(c.collateralCustomerId);
            }

            return collateral;
        }

        public async Task<bool> AddCollateralCustomer(CollateralCustomerViewModel entity)
        {
            var collateral = new tbl_Collateral_Customer
            {
                CompanyId = entity.companyId,
                CollateralTypeId = entity.collateralTypeId,
                CollateralCode = entity.collateralCode,
                CurrencyId = entity.currencyId,
                AllowSharing = entity.allowSharing,
                IsLocationBased = entity.isLocationBased,
                ValuationCycle = entity.valuationCycle,
                HairCut = entity.hairCut,
                CustomerId = entity.customerId,

                ApprovalStatus = entity.approvalStatus,
                DateActedOn = entity.dateActedOn,
                ActedOnBy = entity.actedOnBy,
                CamRefNumber = entity.camRefNumber,
                DateTimeCreated = genSetup.GetApplicationDate().Date,
                CreatedBy = entity.createdBy,
                tbl_Collateral_Immovable_Property = AddCollateralProperty((CollateralTypeEnum)entity.collateralTypeId, entity.collateralProperty),
                tbl_Collateral_Deposit = AddCollateralDeposit((CollateralTypeEnum)entity.collateralTypeId, entity.collateralDeposit),
                tbl_Collateral_Plant_And_Equipment = AddCollateralMachineDetail((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMachineDetail),
                tbl_Collateral_Marketable_Security = AddCollateralMarketableSecurity((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMarketableSecurity),
                tbl_Collateral_Policy = AddCollateralInsurancePolicy((CollateralTypeEnum)entity.collateralTypeId, entity.collateralInsurancePolicy),
                tbl_Collateral_PreciousMetal = AddCollateralPreciousMetal((CollateralTypeEnum)entity.collateralTypeId, entity.collateralPreciousMetal),
                tbl_Collateral_Gaurantee = AddCollateralGaurantee((CollateralTypeEnum)entity.collateralTypeId, entity.collateralGaurantee),
                tbl_Collateral_Vehicle = AddCollateralVehicle((CollateralTypeEnum)entity.collateralTypeId, entity.collateralVehicle),
                tbl_Collateral_Miscellaneous = AddCollateralMiscellaneous((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMiscellaneous),
            };

            context.tbl_Collateral_Customer.Add(collateral);
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user)
        {
            var collateral = context.tbl_Collateral_Customer.Find(colleralCustomerId);
            collateral.Deleted = true;
            collateral.DeletedBy = user.staffId;
            collateral.DateTimeDeleted = genSetup.GetApplicationDate();

            return await context.SaveChangesAsync() != 0;
        }

        private List<CollateralCustomerViewModel> CollateralCustomer(int customerId, int companyId)
        {
            tbl_Collateral_Type_Sub sub = new tbl_Collateral_Type_Sub();
            return (from c in context.tbl_Collateral_Customer
                    join t in context.tbl_Collateral_Type on c.CollateralTypeId equals t.CollateralTypeId
                    where c.Deleted == false && c.CompanyId == companyId && c.CustomerId == customerId
                    select new CollateralCustomerViewModel
                    {
                        collateralTypeId = c.CollateralTypeId,
                        collateralType = c.tbl_Collateral_Type.CollateralTypeName,
                        collateralCustomerId = c.CollateralCustomerId,
                        collateralCode = c.CollateralCode,
                        currencyId = c.CurrencyId,
                        currency = c.tbl_Currency.CurrencyName,
                        allowSharing = c.AllowSharing,
                        isLocationBased = c.IsLocationBased,
                        valuationCycle = c.ValuationCycle,
                        hairCut = c.HairCut,
                        customerId = c.CustomerId,
                        customerName = c.tbl_Customer.LastName + " " + c.tbl_Customer.FirstName,
                        approvalStatus = c.ApprovalStatus,
                        dateActedOn = c.DateActedOn,
                        actedOnBy = c.ActedOnBy,
                        camRefNumber = c.CamRefNumber,
                        dateTimeCreated = c.DateTimeCreated,
                        createdBy = c.CreatedBy,
                    }).ToList();
        }

        public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomerByCustomerId(int customerId, int companyId)
        {
            return CollateralCustomer(customerId, companyId);
        }

        public async Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralCustomerViewModel entity)
        {
            var collateral = context.tbl_Collateral_Customer.Find(collateralCustomerId);
            collateral.CollateralCode = entity.collateralCode;
            collateral.CurrencyId = entity.currencyId;
            collateral.AllowSharing = entity.allowSharing;
            collateral.IsLocationBased = entity.isLocationBased;
            collateral.ValuationCycle = entity.valuationCycle;
            collateral.HairCut = entity.hairCut;
            collateral.CustomerId = entity.customerId;
            collateral.ApprovalStatus = entity.approvalStatus;
            collateral.DateActedOn = entity.dateActedOn;
            collateral.ActedOnBy = entity.actedOnBy;
            collateral.CamRefNumber = entity.camRefNumber;
            collateral.DateTimeUpdated = entity.dateTimeCreated;
            collateral.LastUpdatedBy = entity.lastUpdatedBy;

            //TblCollateralMachineDetail collateralMachineDetail = collateral.TblCollateralMachineDetail.FirstOrDefault();

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Property)
            {
                var collateralProperty = context.tbl_Collateral_Immovable_Property.Find(entity.collateralProperty.collateralPropertyId);

                collateralProperty.PropertyName = entity.collateralProperty.propertyName;
                collateralProperty.CityId = entity.collateralProperty.cityId;
                collateralProperty.CountryId = entity.collateralProperty.countryId;
                collateralProperty.PropertyAddress = entity.collateralProperty.propertyAddress;
                collateralProperty.ConstructionDate = entity.collateralProperty.constructionDate;
                collateralProperty.DateOfAcquisition = entity.collateralProperty.dateOfAcquisition;
                collateralProperty.LastValuationDate = entity.collateralProperty.lastValuationDate;
                collateralProperty.ValuerId = entity.collateralProperty.valuerId;
                collateralProperty.ValuerReferenceNumber = entity.collateralProperty.valuerReferenceNumber;
                collateralProperty.OpenMarketValue = entity.collateralProperty.openMarketValue;
                collateralProperty.CollateralValue = entity.collateralProperty.collateralValue;
                collateralProperty.ForcedSaleValue = entity.collateralProperty.forcedSaleValue;
                collateralProperty.StampToCover = entity.collateralProperty.stampToCover;
                collateralProperty.ValuationSource = entity.collateralProperty.valuationSource;
                collateralProperty.OriginalValue = entity.collateralProperty.originalValue;
                collateralProperty.AvailableValue = entity.collateralProperty.availableValue;
                collateralProperty.SecurityValue = entity.collateralProperty.securityValue;
                collateralProperty.CollateralUsableAmount = entity.collateralProperty.collateralUsableAmount;
                collateralProperty.PropertyValueBaseTypeId = entity.collateralProperty.propertyValueBaseTypeId;
                collateralProperty.Remark = entity.collateralProperty.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities)
            {
                var collateralMarketableSecurity = context.tbl_Collateral_Marketable_Security.Find(entity.collateralMarketableSecurity.collateralMarketableSecurityId);

                collateralMarketableSecurity.SecurityType = entity.collateralMarketableSecurity.securityType;
                collateralMarketableSecurity.DealReferenceNumber = entity.collateralMarketableSecurity.dealReferenceNumber;
                collateralMarketableSecurity.EffectiveDate = entity.collateralMarketableSecurity.effectiveDate;
                collateralMarketableSecurity.MaturityDate = entity.collateralMarketableSecurity.maturityDate;
                collateralMarketableSecurity.DealAmount = entity.collateralMarketableSecurity.dealAmount;
                collateralMarketableSecurity.SecurityValue = entity.collateralMarketableSecurity.securityValue;
                collateralMarketableSecurity.LienUsableAmount = entity.collateralMarketableSecurity.lienUsableAmount;
                collateralMarketableSecurity.Rating = entity.collateralMarketableSecurity.rating;
                collateralMarketableSecurity.PercentageInterest = entity.collateralMarketableSecurity.percentageInterest;
                collateralMarketableSecurity.InterestPaymentFrequency = entity.collateralMarketableSecurity.interestPaymentFrequency;
                collateralMarketableSecurity.IssuerName = entity.collateralMarketableSecurity.issuerName;
                collateralMarketableSecurity.IssuerReferenceNumber = entity.collateralMarketableSecurity.issuerReferenceNumber;
                collateralMarketableSecurity.UnitValue = entity.collateralMarketableSecurity.unitValue;
                collateralMarketableSecurity.NumberOfUnits = entity.collateralMarketableSecurity.numberOfUnits;
                collateralMarketableSecurity.Remark = entity.collateralMarketableSecurity.remark;
            }
            if (entity.collateralTypeId == (int)CollateralTypeEnum.TermDeposit)
            {
                var collateralDeposit = context.tbl_Collateral_Deposit.Find(entity.collateralDeposit.collateralDepositId);

                collateralDeposit.AccountNumber = entity.collateralDeposit.accountNumber;
                collateralDeposit.DealReferenceNumber = entity.collateralDeposit.dealReferenceNumber;
                collateralDeposit.ExistingLienAmount = entity.collateralDeposit.existingLienAmount;
                collateralDeposit.LienAmount = entity.collateralDeposit.lienAmount;
                collateralDeposit.AvailableBalance = entity.collateralDeposit.availableBalance;
                collateralDeposit.SecurityValue = entity.collateralDeposit.securityValue;
                collateralDeposit.MaturityDate = entity.collateralDeposit.maturityDate;
                collateralDeposit.MaturityAmount = entity.collateralDeposit.maturityAmount;
                collateralDeposit.Remark = entity.collateralDeposit.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.CASA)
            {
                var collateralCasa = context.tbl_Collateral_Casa.Find(entity.collateralCasa.collateralCasaId);

                collateralCasa.AccountNumber = entity.collateralCasa.accountNumber;
                collateralCasa.IsOwnedByCustomer = entity.collateralCasa.isOwnedByCustomer;
                collateralCasa.AvailableBalance = entity.collateralCasa.availableBalance;
                collateralCasa.ExistingLienAmount = entity.collateralCasa.existingLienAmount;
                collateralCasa.LienAmount = entity.collateralCasa.lienAmount;
                collateralCasa.SecurityValue = entity.collateralCasa.securityValue;
                collateralCasa.Remark = entity.collateralCasa.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
            {
                var collateralMachineDetail = context.tbl_Collateral_Plant_And_Equipment.Find(entity.collateralMachineDetail.collateralMachineDetailId);

                collateralMachineDetail.MachineName = entity.collateralMachineDetail.machineName;
                collateralMachineDetail.Description = entity.collateralMachineDetail.description;
                collateralMachineDetail.MachineNumber = entity.collateralMachineDetail.machineNumber;
                collateralMachineDetail.ManufacturerName = entity.collateralMachineDetail.manufacturerName;
                collateralMachineDetail.YearOfManufacture = entity.collateralMachineDetail.yearOfManufacture;
                collateralMachineDetail.YearOfPurchase = entity.collateralMachineDetail.yearOfManufacture;
                collateralMachineDetail.ValueBaseTypeId = entity.collateralMachineDetail.valueBaseTypeId;
                collateralMachineDetail.MachineCondition = entity.collateralMachineDetail.machineCondition;
                collateralMachineDetail.MachineryLocation = entity.collateralMachineDetail.machineryLocation;
                collateralMachineDetail.EquipmentSize = entity.collateralMachineDetail.equipmentSize;
                collateralMachineDetail.ReplacementValue = entity.collateralMachineDetail.replacementValue;
                collateralMachineDetail.IntendedUse = entity.collateralMachineDetail.intendedUse;

            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.PreciousMetal)
            {
                var collateralPreciousMetal = context.tbl_Collateral_PreciousMetal.Find(entity.collateralPreciousMetal.collateralPreciousMetalId);

                collateralPreciousMetal.CollateralCustomerId = entity.collateralPreciousMetal.collateralCustomerId;
                collateralPreciousMetal.IsOwnedByCustomer = entity.collateralPreciousMetal.isOwnedByCustomer;
                collateralPreciousMetal.PreciousMetalName = entity.collateralPreciousMetal.preciousMetalName;
                collateralPreciousMetal.WeightInGrammes = entity.collateralPreciousMetal.weightInGrammes;
                collateralPreciousMetal.ValuationAmount = entity.collateralPreciousMetal.valuationAmount;
                collateralPreciousMetal.UnitRate = entity.collateralPreciousMetal.unitRate;
                collateralPreciousMetal.PreciousMetalForm = entity.collateralPreciousMetal.preciousMetalForm;
                collateralPreciousMetal.Remark = entity.collateralPreciousMetal.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.InsurancePolicy)
            {
                tbl_Collateral_Policy collateralInsurancePolicy = collateral.tbl_Collateral_Policy.Where(x => x.CollateralInsurancePolicyId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralInsurancePolicy.PremiumAmount = entity.collateralInsurancePolicy.premiumAmount;
                collateralInsurancePolicy.IsOwnedByCustomer = entity.collateralInsurancePolicy.isOwnedByCustomer;
                collateralInsurancePolicy.InsurancePolicyNumber = entity.collateralInsurancePolicy.insurancePolicyNumber;
                collateralInsurancePolicy.PolicyAmount = entity.collateralInsurancePolicy.policyAmount;
                collateralInsurancePolicy.InsuranceCompanyName = entity.collateralInsurancePolicy.insuranceCompanyName;
                collateralInsurancePolicy.PolicyStartDate = entity.collateralInsurancePolicy.policyStartDate;
                collateralInsurancePolicy.AssignDate = entity.collateralInsurancePolicy.assignDate;
                collateralInsurancePolicy.PolicyRenewalDate = entity.collateralInsurancePolicy.policyRenewalDate;
                collateralInsurancePolicy.InsurerAddress = entity.collateralInsurancePolicy.insurerAddress;
                collateralInsurancePolicy.InsurerDetails = entity.collateralInsurancePolicy.insurerDetails;
                collateralInsurancePolicy.RenewalFrequencyTypeId = entity.collateralInsurancePolicy.renewalFrequencyTypeId;
                collateralInsurancePolicy.Remark = entity.collateralInsurancePolicy.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Gaurantee)
            {
                tbl_Collateral_Gaurantee collateralGaurantee = collateral.tbl_Collateral_Gaurantee.Where(x => x.CollateralGauranteeId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralGaurantee.IsOwnedByCustomer = entity.collateralGaurantee.isOwnedByCustomer;
                collateralGaurantee.InstitutionName = entity.collateralGaurantee.institutionName;
                collateralGaurantee.GuarantorReferenceNumber = entity.collateralGaurantee.guarantorReferenceNumber;
                collateralGaurantee.GuaranteeValue = entity.collateralGaurantee.guaranteeValue;
                collateralGaurantee.StartDate = entity.collateralGaurantee.startDate;
                collateralGaurantee.EndDate = entity.collateralGaurantee.endDate;
                collateralGaurantee.GuarantorAddress = entity.collateralGaurantee.guarantorAddress;
                collateralGaurantee.Remark = entity.collateralGaurantee.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Vehicle)
            {
                tbl_Collateral_Vehicle collateralVehicle = collateral.tbl_Collateral_Vehicle.Where(x => x.CollateralVehicleId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralVehicle.VehicleType = entity.collateralVehicle.vehicleType;
                collateralVehicle.VehicleStatus = entity.collateralVehicle.vehicleStatus;
                collateralVehicle.VehicleMake = entity.collateralVehicle.vehicleMake;
                collateralVehicle.ModelName = entity.collateralVehicle.modelName;
                collateralVehicle.ManufacturedDate = entity.collateralVehicle.manufacturedDate;
                collateralVehicle.SerialNumber = entity.collateralVehicle.serialNumber;
                collateralVehicle.NameOfOwner = entity.collateralVehicle.nameOfOwner;
                collateralVehicle.RegistrationCompany = entity.collateralVehicle.registrationCompany;
                collateralVehicle.LastValuationAmount = entity.collateralVehicle.lastValuationAmount;
                collateralVehicle.RegistrationNumber = entity.collateralVehicle.registrationNumber;
                collateralVehicle.ChasisNumber = entity.collateralVehicle.chasisNumber;
                collateralVehicle.EngineNumber = entity.collateralVehicle.engineNumber;
                collateralVehicle.ResaleValue = entity.collateralVehicle.resaleValue;
                collateralVehicle.ValuationDate = entity.collateralVehicle.valuationDate;
                collateralVehicle.InvoiceValue = entity.collateralVehicle.invoiceValue;
                collateralVehicle.Remark = entity.collateralVehicle.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Miscellaneous)
            {
                tbl_Collateral_Miscellaneous collateralMiscellaneous = collateral.tbl_Collateral_Miscellaneous.Where(x => x.CollateralMiscellaneousId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralMiscellaneous.IsOwnedByCustomer = entity.collateralMiscellaneous.isOwnedByCustomer;
                collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
                collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
                collateralMiscellaneous.Note = entity.collateralMiscellaneous.note;
                //if (entity.collateralMiscellaneous.collateralMiscellaneousNotes != null)
                //{
                //    tbl_Collateral_Miscellaneous_Notes collateralMiscellaneousNote = context.tbl_Collateral_Miscellaneous_Notes.Where(x => x.MiscellaneousId == entity.collateralMiscellaneous.collateralMiscellaneousId)
                //    .FirstOrDefault();

                //    collateralMiscellaneousNote.ColumnName = entity.collateralMiscellaneous.collateralMiscellaneousNotes.;
                //    collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
                //    collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
                //    collateralMiscellaneous.Note = entity.collateralMiscellaneous.note;
                //}
            }

            //if (entity.collateralCustomerPolicy != null)
            //{
            //    tbl_Collateral_Item_Policy collateralCustomerPolicy = collateral.tbl_Collateral_Customer_Policy.Where(x => x.PolicyId == entity.collateralCustomerPolicy.policyId)
            //        .FirstOrDefault();

            //    collateralCustomerPolicy.PolicyReferenceNumber = entity.collateralCustomerPolicy.policyReferenceNumber;
            //    collateralCustomerPolicy.InsuranceCompanyName = entity.collateralCustomerPolicy.insuranceCompanyName;
            //    collateralCustomerPolicy.StartDate = entity.collateralCustomerPolicy.startDate;
            //    collateralCustomerPolicy.EndDate = entity.collateralCustomerPolicy.endDate;
            //}

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
                StaffId = (int)entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Update collateral with code: { entity.collateralCode} of { entity.valuationCycle} valuation cycle",
                //Ipaddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
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
        #endregion Collateral Customer

        #region Property
        private ICollection<tbl_Collateral_Immovable_Property> AddCollateralProperty(CollateralTypeEnum collateralType, CollateralPropertyViewModel entity)
        {
            ICollection<tbl_Collateral_Immovable_Property> collateral;

            if (collateralType != CollateralTypeEnum.Property)
                return null;

            collateral = new List<tbl_Collateral_Immovable_Property>();

            collateral.Add(new tbl_Collateral_Immovable_Property
            {
                //CollateralPropertyId = entity.collateralPropertyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                PropertyName = entity.propertyName,
                CityId = entity.cityId,
                CountryId = entity.countryId,
                PropertyAddress = entity.propertyAddress,
                ConstructionDate = entity.constructionDate,
                PropertyValueBaseTypeId = entity.propertyValueBaseTypeId,
                DateOfAcquisition = entity.dateOfAcquisition,
                LastValuationDate = entity.lastValuationDate,
                ValuerId = entity.valuerId,
                ValuerReferenceNumber = entity.valuerReferenceNumber,
                OpenMarketValue = entity.openMarketValue,
                CollateralValue = entity.collateralValue,
                ForcedSaleValue = entity.forcedSaleValue,
                StampToCover = entity.stampToCover,
                ValuationSource = entity.valuationSource,
                OriginalValue = entity.originalValue,
                AvailableValue = entity.availableValue,
                SecurityValue = entity.securityValue,
                CollateralUsableAmount = entity.collateralUsableAmount,
                Remark = entity.remark
            });

            return collateral;
        }

        private CollateralPropertyViewModel CollateralProperty(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Immovable_Property
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where c.Deleted == false && m.CollateralCustomerId == collateralCustomerId
                    select new CollateralPropertyViewModel
                    {
                        collateralPropertyId = m.CollateralPropertyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        propertyName = m.PropertyName,
                        cityId = m.CityId,
                        countryId = m.CountryId,
                        propertyAddress = m.PropertyAddress,
                        constructionDate = m.ConstructionDate,
                        propertyValueBaseTypeId = m.PropertyValueBaseTypeId,
                        dateOfAcquisition = m.DateOfAcquisition,
                        lastValuationDate = m.LastValuationDate,
                        valuerId = m.ValuerId,
                        valuerReferenceNumber = m.ValuerReferenceNumber,
                        openMarketValue = m.OpenMarketValue,
                        collateralValue = m.CollateralValue,
                        forcedSaleValue = m.ForcedSaleValue,
                        stampToCover = m.StampToCover,
                        valuationSource = m.ValuationSource,
                        originalValue = m.OriginalValue,
                        availableValue = m.AvailableValue,
                        securityValue = m.SecurityValue,
                        collateralUsableAmount = m.CollateralUsableAmount,
                        remark = m.Remark
                    }).FirstOrDefault();
        }

        private CollateralPropertyViewModel GetCollateralPropertyByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralProperty(CollateralCustomerId);
        }
        #endregion Property

        #region Deposit
        private ICollection<tbl_Collateral_Deposit> AddCollateralDeposit(CollateralTypeEnum collateralType, CollateralDepositViewModel entity)
        {
            ICollection<tbl_Collateral_Deposit> collateral;

            if (collateralType != CollateralTypeEnum.TermDeposit)
                return null;

            collateral = new List<tbl_Collateral_Deposit>();

            collateral.Add(new tbl_Collateral_Deposit
            {
                //CollateralDepositId = entity.collateralDepositId,
                //CollateralCustomerId = entity.collateralCustomerId,
                DealReferenceNumber = entity.dealReferenceNumber,
                AccountNumber = entity.accountNumber,
                ExistingLienAmount = entity.existingLienAmount,
                LienAmount = entity.lienAmount,
                AvailableBalance = entity.availableBalance,
                SecurityValue = entity.securityValue,
                MaturityDate = entity.maturityDate,
                MaturityAmount = entity.maturityAmount,
                Remark = entity.remark
            });

            return collateral;
        }

        private CollateralDepositViewModel CollateralDeposit(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Deposit
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralDepositViewModel
                    {
                        collateralDepositId = m.CollateralDepositId,
                        collateralCustomerId = m.CollateralCustomerId,
                        dealReferenceNumber = m.DealReferenceNumber,
                        accountNumber = m.AccountNumber,
                        existingLienAmount = m.ExistingLienAmount,
                        lienAmount = m.LienAmount,
                        availableBalance = m.AvailableBalance,
                        securityValue = m.SecurityValue,
                        maturityDate = m.MaturityDate,
                        maturityAmount = m.MaturityAmount,
                        remark = m.Remark

                    }).FirstOrDefault();
        }

        private CollateralDepositViewModel GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralDeposit(CollateralCustomerId);
        }
        #endregion Deposit

        #region End od CASA
        private ICollection<tbl_Collateral_Casa> AddCollateralCasa(CollateralTypeEnum collateralType, CollateralCasaViewModel entity)
        {
            ICollection<tbl_Collateral_Casa> collateral;

            if (collateralType != CollateralTypeEnum.CASA)
                return null;

            collateral = new List<tbl_Collateral_Casa>();

            collateral.Add(new tbl_Collateral_Casa
            {
                AccountNumber = entity.accountNumber,
                IsOwnedByCustomer = entity.isOwnedByCustomer,
                AvailableBalance = entity.availableBalance,
                ExistingLienAmount = entity.existingLienAmount,
                LienAmount = entity.lienAmount,
                SecurityValue = entity.securityValue,
                Remark = entity.remark
            });

            return collateral;
        }

        private CollateralCasaViewModel CollateralCasa(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Casa
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralCasaViewModel
                    {
                        collateralCasaId = m.CollateralCasaId,
                        collateralCustomerId = m.CollateralCustomerId,
                        accountNumber = m.AccountNumber,
                        isOwnedByCustomer = m.IsOwnedByCustomer,
                        availableBalance = m.AvailableBalance,
                        existingLienAmount = m.ExistingLienAmount,
                        lienAmount = m.LienAmount,
                        securityValue = m.SecurityValue,
                        remark = m.Remark

                    }).FirstOrDefault();
        }

        private CollateralCasaViewModel GetCollateralCasaByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralCasa(CollateralCustomerId);
        }
        #endregion End of CASA

        #region Plants and Equipment
        private ICollection<tbl_Collateral_Plant_And_Equipment> AddCollateralMachineDetail(CollateralTypeEnum collateralType, CollateralPlantsAndEquipmentViewModel entity)
        {
            ICollection<tbl_Collateral_Plant_And_Equipment> collateral;

            if (collateralType != CollateralTypeEnum.PlantAndMachinery)
                return null;

            collateral = new List<tbl_Collateral_Plant_And_Equipment>();

            collateral.Add(new tbl_Collateral_Plant_And_Equipment
            {
                MachineName = entity.machineName,
                Description = entity.description,
                MachineNumber = entity.machineNumber,
                ManufacturerName = entity.manufacturerName,
                YearOfManufacture = entity.yearOfManufacture,
                YearOfPurchase = entity.yearOfManufacture,
                ValueBaseTypeId = entity.valueBaseTypeId,
                MachineCondition = entity.machineCondition,
                MachineryLocation = entity.machineryLocation,
                ReplacementValue = entity.replacementValue,
                EquipmentSize = entity.equipmentSize,
                IntendedUse = entity.intendedUse
            });

            return collateral;
        }

        private CollateralPlantsAndEquipmentViewModel CollateralMachineDetail(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Plant_And_Equipment
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralPlantsAndEquipmentViewModel
                    {
                        collateralMachineDetailId = m.CollateralMachineDetailId,
                        collateralCustomerId = m.CollateralCustomerId,
                        machineName = m.MachineName,
                        description = m.Description,
                        machineNumber = m.MachineNumber,
                        manufacturerName = m.ManufacturerName,
                        yearOfManufacture = m.YearOfManufacture,
                        yearOfPurchase = m.YearOfPurchase,
                        valueBaseTypeId = m.ValueBaseTypeId,
                        machineCondition = m.MachineCondition,
                        machineryLocation = m.MachineryLocation,
                        replacementValue = m.ReplacementValue,
                        equipmentSize = m.EquipmentSize,
                        intendedUse = m.IntendedUse
                    }).FirstOrDefault();
        }

        private CollateralPlantsAndEquipmentViewModel GetCollateralMachineDetailByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMachineDetail(collateralCustomerId);
        }
        #endregion Plants and Equipment

        #region Marketable Security
        private ICollection<tbl_Collateral_Marketable_Security> AddCollateralMarketableSecurity(CollateralTypeEnum collateralType, CollateralMarketableSecurityViewModel entity)
        {
            ICollection<tbl_Collateral_Marketable_Security> collateral;

            if (collateralType != CollateralTypeEnum.MarketableSecurities)
                return null;

            collateral = new List<tbl_Collateral_Marketable_Security>();

            collateral.Add(new tbl_Collateral_Marketable_Security
            {
                SecurityType = entity.securityType,
                DealReferenceNumber = entity.dealReferenceNumber,
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,
                DealAmount = entity.dealAmount,
                SecurityValue = entity.securityValue,
                LienUsableAmount = entity.lienUsableAmount,
                IssuerName = entity.issuerName,
                IssuerReferenceNumber = entity.issuerReferenceNumber,
                UnitValue = entity.unitValue,
                NumberOfUnits = entity.numberOfUnits,
                Rating = entity.rating,
                PercentageInterest = entity.percentageInterest,
                InterestPaymentFrequency = entity.interestPaymentFrequency,
                Remark = entity.remark

            });

            return collateral;
        }

        private CollateralMarketableSecurityViewModel CollateralMarketableSecurity(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Marketable_Security
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralMarketableSecurityViewModel
                    {
                        collateralMarketableSecurityId = m.CollateralMarketableSecurityId,
                        collateralCustomerId = m.CollateralCustomerId,
                        securityType = m.SecurityType,
                        dealReferenceNumber = m.DealReferenceNumber,
                        effectiveDate = m.EffectiveDate,
                        maturityDate = m.MaturityDate,
                        dealAmount = m.DealAmount,
                        securityValue = m.SecurityValue,
                        lienUsableAmount = m.LienUsableAmount,
                        rating = m.Rating,
                        percentageInterest = m.PercentageInterest,
                        interestPaymentFrequency = m.InterestPaymentFrequency,
                        issuerName = m.IssuerName,
                        issuerReferenceNumber = m.IssuerReferenceNumber,
                        unitValue = m.UnitValue,
                        numberOfUnits = m.NumberOfUnits,
                        remark = m.Remark



                    }).FirstOrDefault();
        }

        private CollateralMarketableSecurityViewModel GetCollateralMarketableSecurityByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMarketableSecurity(collateralCustomerId);
        }

        #endregion Marketable Security

        #region Precious Metal
        private ICollection<tbl_Collateral_PreciousMetal> AddCollateralPreciousMetal(CollateralTypeEnum collateralType, CollateralPreciousMetalViewModel entity)
        {
            ICollection<tbl_Collateral_PreciousMetal> collateral;

            if (collateralType != CollateralTypeEnum.PreciousMetal)
                return null;

            collateral = new List<tbl_Collateral_PreciousMetal>();

            collateral.Add(new tbl_Collateral_PreciousMetal
            {
                IsOwnedByCustomer = entity.isOwnedByCustomer,
                PreciousMetalName = entity.preciousMetalName,
                WeightInGrammes = entity.weightInGrammes,
                ValuationAmount = entity.valuationAmount,
                UnitRate = entity.unitRate,
                PreciousMetalForm = entity.preciousMetalForm,
                Remark = entity.remark

            });

            return collateral;
        }

        private CollateralPreciousMetalViewModel CollateralPreciousMetal(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_PreciousMetal
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralPreciousMetalViewModel
                    {
                        collateralPreciousMetalId = m.CollateralPreciousMetalId,
                        collateralCustomerId = m.CollateralCustomerId,
                        isOwnedByCustomer = m.IsOwnedByCustomer,
                        preciousMetalName = m.PreciousMetalName,
                        weightInGrammes = m.WeightInGrammes,
                        valuationAmount = m.ValuationAmount,
                        unitRate = m.UnitRate,
                        preciousMetalForm = m.PreciousMetalForm,
                        remark = m.Remark

                    }).FirstOrDefault();
        }

        private CollateralPreciousMetalViewModel GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralPreciousMetal(collateralCustomerId);
        }
        #endregion Precious Metal

        #region Insurance Policy
        private ICollection<tbl_Collateral_Policy> AddCollateralInsurancePolicy(CollateralTypeEnum collateralType, CollateralInsurancePolicyViewModel entity)
        {
            ICollection<tbl_Collateral_Policy> collateral;

            if (collateralType != CollateralTypeEnum.InsurancePolicy)
                return null;

            collateral = new List<tbl_Collateral_Policy>();

            collateral.Add(new tbl_Collateral_Policy
            {
                //CollateralInsurancePolicyId = entity.collateralInsurancePolicyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                IsOwnedByCustomer = entity.isOwnedByCustomer,
                InsurancePolicyNumber = entity.insurancePolicyNumber,
                PremiumAmount = entity.premiumAmount,
                PolicyAmount = entity.policyAmount,
                InsuranceCompanyName = entity.insuranceCompanyName,
                InsurerAddress = entity.insurerAddress,
                PolicyStartDate = entity.policyStartDate,
                AssignDate = entity.assignDate,
                RenewalFrequencyTypeId = entity.renewalFrequencyTypeId,
                InsurerDetails = entity.insurerDetails,
                PolicyRenewalDate = entity.policyRenewalDate,
                Remark = entity.remark

            });

            return collateral;
        }

        private CollateralInsurancePolicyViewModel CollateralInsurancePolicy(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Policy
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralInsurancePolicyViewModel
                    {
                        collateralInsurancePolicyId = m.CollateralInsurancePolicyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        isOwnedByCustomer = m.IsOwnedByCustomer,
                        insurancePolicyNumber = m.InsurancePolicyNumber,
                        premiumAmount = m.PremiumAmount,
                        policyAmount = m.PolicyAmount,
                        insuranceCompanyName = m.InsuranceCompanyName,
                        insurerAddress = m.InsurerAddress,
                        policyStartDate = m.PolicyStartDate,
                        assignDate = m.AssignDate,
                        renewalFrequencyTypeId = m.RenewalFrequencyTypeId,
                        insurerDetails = m.InsurerDetails,
                        policyRenewalDate = m.PolicyRenewalDate,
                        remark = m.Remark
                    }).FirstOrDefault();
        }

        private CollateralInsurancePolicyViewModel GetCollateralInsurancePolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralInsurancePolicy(collateralCustomerId);
        }
        #endregion Insurance Policy

        #region Gaurantee
        private ICollection<tbl_Collateral_Gaurantee> AddCollateralGaurantee(CollateralTypeEnum collateralType, CollateralGauranteeViewModel entity)
        {
            ICollection<tbl_Collateral_Gaurantee> collateral;

            if (collateralType != CollateralTypeEnum.Gaurantee)
                return null;

            collateral = new List<tbl_Collateral_Gaurantee>();

            collateral.Add(new tbl_Collateral_Gaurantee
            {
                //CollateralGauranteeId = entity.collateralGauranteeId,
                //CollateralCustomerId = entity.collateralCustomerId,
                IsOwnedByCustomer = entity.isOwnedByCustomer,
                InstitutionName = entity.institutionName,
                GuarantorAddress = entity.guarantorAddress,
                GuarantorReferenceNumber = entity.guarantorReferenceNumber,
                GuaranteeValue = entity.guaranteeValue,
                StartDate = entity.startDate,
                EndDate = entity.endDate,
                Remark = entity.remark

            });

            return collateral;
        }

        private CollateralGauranteeViewModel CollateralGaurantee(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Gaurantee
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralGauranteeViewModel
                    {
                        collateralGauranteeId = m.CollateralGauranteeId,
                        collateralCustomerId = m.CollateralCustomerId,
                        isOwnedByCustomer = m.IsOwnedByCustomer,
                        institutionName = m.InstitutionName,
                        guarantorAddress = m.GuarantorAddress,
                        guarantorReferenceNumber = m.GuarantorReferenceNumber,
                        guaranteeValue = m.GuaranteeValue,
                        startDate = m.StartDate,
                        endDate = m.EndDate,
                        remark = m.Remark
                    }).FirstOrDefault();
        }

        private CollateralGauranteeViewModel GetCollateralGauranteeByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralGaurantee(collateralCustomerId);
        }
        #endregion Gaurantee

        #region Vehicle
        private ICollection<tbl_Collateral_Vehicle> AddCollateralVehicle(CollateralTypeEnum collateralType, CollateralVehicleViewModel entity)
        {
            ICollection<tbl_Collateral_Vehicle> collateral;

            if (collateralType != CollateralTypeEnum.Vehicle)
                return null;

            collateral = new List<tbl_Collateral_Vehicle>();

            collateral.Add(new tbl_Collateral_Vehicle
            {
                VehicleType = entity.vehicleType,
                VehicleStatus = entity.vehicleStatus,
                VehicleMake = entity.vehicleMake,
                ModelName = entity.modelName,
                ManufacturedDate = entity.manufacturedDate,
                RegistrationNumber = entity.registrationNumber,
                SerialNumber = entity.serialNumber,
                ChasisNumber = entity.chasisNumber,
                EngineNumber = entity.engineNumber,
                NameOfOwner = entity.nameOfOwner,
                RegistrationCompany = entity.registrationCompany,
                ResaleValue = entity.resaleValue,
                ValuationDate = entity.valuationDate,
                LastValuationAmount = entity.lastValuationAmount,
                InvoiceValue = entity.invoiceValue,
                Remark = entity.remark
            });

            return collateral;
        }

        private CollateralVehicleViewModel CollateralVehicle(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Vehicle
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralVehicleViewModel
                    {
                        collateralVehicleId = m.CollateralVehicleId,
                        collateralCustomerId = m.CollateralCustomerId,
                        vehicleType = m.VehicleType,
                        vehicleStatus = m.VehicleStatus,
                        vehicleMake = m.VehicleMake,
                        modelName = m.ModelName,
                        manufacturedDate = m.ManufacturedDate,
                        registrationNumber = m.RegistrationNumber,
                        serialNumber = m.SerialNumber,
                        chasisNumber = m.ChasisNumber,
                        engineNumber = m.EngineNumber,
                        nameOfOwner = m.NameOfOwner,
                        registrationCompany = m.RegistrationCompany,
                        resaleValue = m.ResaleValue,
                        valuationDate = m.ValuationDate,
                        lastValuationAmount = m.LastValuationAmount,
                        invoiceValue = m.InvoiceValue,
                        remark = m.Remark
                    }).FirstOrDefault();
        }

        private CollateralVehicleViewModel GetCollateralVehicleByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralVehicle(collateralCustomerId);
        }
        #endregion Vehicle

        #region Miscellaneous
        private ICollection<tbl_Collateral_Miscellaneous> AddCollateralMiscellaneous(CollateralTypeEnum collateralType, CollateralMiscellaneousViewModel entity)
        {
            ICollection<tbl_Collateral_Miscellaneous> collateral;

            if (collateralType != CollateralTypeEnum.Miscellaneous)
                return null;

            collateral = new List<tbl_Collateral_Miscellaneous>();

            collateral.Add(new tbl_Collateral_Miscellaneous
            {
                //CollateralMiscellaneousId = entity.collateralMiscellaneousId,
                //CollateralCustomerId = entity.collateralCustomerId,
                IsOwnedByCustomer = entity.isOwnedByCustomer,
                NameOfSecurity = entity.nameOfSecurity,
                SecurityValue = entity.securityValue,
                Note = entity.note,
                tbl_Collateral_Miscellaneous_Notes = AddCollateralMiscNotes(entity.collateralMiscellaneousNotes)
            });

            return collateral;
        }

        private CollateralMiscellaneousViewModel Miscellaneous(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Miscellaneous
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralMiscellaneousViewModel
                    {
                        collateralMiscellaneousId = m.CollateralMiscellaneousId,
                        collateralCustomerId = m.CollateralCustomerId,
                        isOwnedByCustomer = m.IsOwnedByCustomer,
                        nameOfSecurity = m.NameOfSecurity,
                        securityValue = m.SecurityValue,
                        note = m.Note,
                        collateralMiscellaneousNotes = GetCollateralMiscellaneousNotesByMiscellaneousId(m.CollateralMiscellaneousId)

                    }).FirstOrDefault();
        }

        private CollateralMiscellaneousViewModel GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId)
        {
            return Miscellaneous(collateralCustomerId);
        }
        #endregion Miscellaneous

        #region Miscellaneous Notes
        private ICollection<tbl_Collateral_Miscellaneous_Notes> AddCollateralMiscNotes(List<CollateralMiscellaneousNotesViewModel> entity)
        {
            ICollection<tbl_Collateral_Miscellaneous_Notes> collateral;
            collateral = new List<tbl_Collateral_Miscellaneous_Notes>();
            foreach (var note in entity)
            {
                collateral.Add(new tbl_Collateral_Miscellaneous_Notes
                {
                    MiscellaneousNoteId = note.miscellaneousNoteId,
                    MiscellaneousId = note.miscellaneousNoteId,
                    ColumnName = note.columnName,
                    ColumnValue = note.columnValue
                });
            }

            return collateral;
        }

        public Task<bool> DeleteCollateralMiscellaneousNotes(int miscNoteId, UserInfo user)
        {
            return null;
        }

        public Task<bool> UpdateCollateralMiscellaneousNotes(int miscNoteId, CollateralMiscellaneousNotesViewModel entity)
        {
            return null;
        }

        private List<CollateralMiscellaneousNotesViewModel> MiscellaneousNotes(int miscellaneousId)
        {
            return (from m in context.tbl_Collateral_Miscellaneous_Notes
                    join c in context.tbl_Collateral_Miscellaneous on m.MiscellaneousId equals c.CollateralMiscellaneousId
                    where m.MiscellaneousId == miscellaneousId
                    select new CollateralMiscellaneousNotesViewModel
                    {
                        miscellaneousNoteId = m.MiscellaneousNoteId,
                        miscellaneousId = m.MiscellaneousId,
                        columnName = m.ColumnName,
                        columnValue = m.ColumnValue,
                    }).ToList();
        }

        private List<CollateralMiscellaneousNotesViewModel> GetCollateralMiscellaneousNotesByMiscellaneousId(int collateralMiscellaneousId)
        {
            return MiscellaneousNotes(collateralMiscellaneousId);
        }
        #endregion Miscellaneous Notes

        #region Collateral Customer Policy
        private ICollection<tbl_Collateral_Item_Policy> AddCollateralCustomerPolicy(int collateralTypeId, CollateralCustomerPolicyViewModel entity)
        {
            var type = context.tbl_Collateral_Type.Where(x => x.CollateralTypeId == collateralTypeId).FirstOrDefault();
            ICollection<tbl_Collateral_Item_Policy> customerPolicy;

            if (!type.RequireInsurancePolicy)
                return null;
            if (entity == null)
                throw new InvalidOperationException("This collateral type requires insurance policy which was not submitted");

            customerPolicy = new List<tbl_Collateral_Item_Policy>();

            customerPolicy.Add(new tbl_Collateral_Item_Policy
            {
                //PolicyId = entity.policyId,
                // CollateralCustomerId = entity.collateralCustomerId,
                PolicyReferenceNumber = entity.policyReferenceNumber,
                InsuranceCompanyName = entity.insuranceCompanyName,
                StartDate = entity.startDate,
                EndDate = entity.endDate

            });

            return customerPolicy;
        }

        private CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Item_Policy
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralCustomerPolicyViewModel
                    {
                        policyId = m.PolicyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        policyReferenceNumber = m.PolicyReferenceNumber,
                        insuranceCompanyName = m.InsuranceCompanyName,
                        startDate = m.StartDate,
                        endDate = m.EndDate
                    }).FirstOrDefault();
        }
        #endregion End of Collateral Customer Policy

        //#region Collateral Documents
        //private ICollection<tbl_Collateral_Documents> AddCollateralDocument(CollateralTypeEnum collateralType, CollateralDocumentViewModel entity)
        //{
        //    ICollection<tbl_Collateral_Documents> collateral;

        //    //if (collateralType != CollateralTypeEnum.MarketableSecurities)
        //    //    return null;

        //    collateral = new List<tbl_Collateral_Documents>();

        //    collateral.Add(new tbl_Collateral_Documents
        //    {
        //        DocumentId = entity.documentId,
        //        CollateralCustomerId = entity.collateralCustomerId,
        //        DocumentCategory = entity.documentCategory,
        //        DocumentRef = entity.documentRef,
        //        DocumentCode = entity.documentCode,
        //        DocumentType = entity.documentType,
        //        IsMandatory = entity.isMandatory,
        //        Remark = entity.remark,
        //        //CreatedBy = entity
        //        // DateTimeCreated = entity
        //    });

        //    return collateral;
        //}

        //private CollateralDocumentViewModel CollateralDocument(int collateralCustomerId)
        //{
        //    return (from m in context.tbl_Collateral_Documents
        //            join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
        //            where m.CollateralCustomerId == collateralCustomerId
        //            select new CollateralDocumentViewModel
        //            {
        //                documentId = m.DocumentId,
        //                collateralCustomerId = m.CollateralCustomerId,
        //                documentCategory = m.DocumentCategory,
        //                documentRef = m.DocumentRef,
        //                documentCode = m.DocumentCode,
        //                documentType = m.DocumentType,
        //                isMandatory = m.IsMandatory,
        //                remark = m.Remark
        //            }).FirstOrDefault();
        //}

        //private CollateralDocumentViewModel GetCollateralDocumentByCollateralCustomerId(int collateralCustomerId)
        //{
        //    return CollateralDocument(collateralCustomerId);
        //}

        //#endregion End of Collateral Documents

        #region Seniority Of Claims
        //This CRUD function should be moved to setup in the collateralType repository and the get function will depend on the its setup Get function 
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
                        dateTimeCreated = genSetup.GetApplicationDate(),
                    });
        }
        #endregion Seniority Of Claims

        #region Listing Functions
        public IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType()
        {
            return (from m in context.tbl_Collateral_Valuebase_Type
                    select new CollateralValueBaseTypeViewModel
                    {
                        collateralValueBaseTypeId = m.CollateralValueBaseTypeId,
                        collateralTypeId = m.CollateralTypeId,
                        valueBaseTypeName = m.ValueBaseTypeName
                    });
        }

        public IEnumerable<CollateralValuersViewModel> GetCollateralValuer(int companyId)
        {
            return (from m in context.tbl_Collateral_Valuer
                    where m.CompanyId == companyId
                    select new CollateralValuersViewModel
                    {
                        collateralValuerId = m.CollateralValuerId,
                        cityId = m.CityId,
                        name = m.Name,
                        valuerLicenceNumber = m.ValuerLicenceNumber,
                    });
        }

        public IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType()
        {
            return (from m in context.tbl_Collateral_Valuer_Type
                    select new CollateralValuerTypeViewModel
                    {
                        collateralValuerTypeId = m.CollateralValuerTypeId,
                        valuerTypeName = m.ValuerTypeName
                    });
        }

        public IEnumerable<CollateralTypeViewModel> GetCollateralType()
        {
            return this.collateralType.GetCollateralTypes();
        }

        #endregion End of Listing Functions
    }
}
