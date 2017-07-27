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

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCollateralRepository : ICollateralCustomerRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IProductRepository product;
        public CustomerCollateralRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IProductRepository _product)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            product = _product;
        }


        #region Collateral Customer 
        public IEnumerable<CollateralViewModel> GetCollateralCustomer(int customerId, int companyId)
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
                c.collateralMiscellaneous = GetCollateralMiscellaneousByCollateralCustomerId(c.collateralCustomerId);
               // c.collateralCustomerPolicy = GetCollateralCustomerPolicyByCollateralCustomerId(c.collateralCustomerId);
            }

            return collateral;
        }

        public async Task<bool> AddCollateralCustomer(CollateralViewModel entity)
        {

            var collateral = new tbl_Collateral_Customer
            {
                CompanyId = entity.companyId,
                CollateralTypeId = entity.collateralTypeId,
                CollateralCode = entity.collateralCode,
                CollateralValue = entity.collateralValue,
                CollateralValueDate = entity.collateralValueDate,
                CustomerId = entity.customerId,
                Quantity = entity.quantity,
                ReleaseDate = entity.releaseDate,
                ReleaseCollateral = entity.releaseCollateral,
                ChargeTypeId = entity.chargeTypeId,
                CurrencyId = (short)entity.currencyId,
                LimitContribution = entity.limitContribution,
                GraceDays = entity.graceDays,
                SeniorityOfClaimId = entity.seniorityOfClaimId,
                LendableMargin = entity.lendableMargin,
                StartDate = entity.startDate,
                EndDate = entity.endDate,
                RevisionDate = entity.revisionDate,
                RequireFieldInvestigation = entity.requireFieldInvestigation,
                RequireValuation = entity.requireValuation,
                RequireCheck = entity.requireCheck,
                AllowShare = entity.allowShare,
                RevaluationDate = entity.revaluationDate,
                LastValuationDate = entity.lastValuationDate,
                ValuationSource = entity.valuationSource,
                ValuationAmount = entity.valuationAmount,
                //HairCut = (double)entity.haircut,
                CamRefNumber = entity.camRefNumber,
                DateTimeCreated = genSetup.GetApplicaionDate().Date,
                CreatedBy = entity.createdBy,
                ApprovalStatus = entity.approvalStatus,
                DateActedOn = entity.dateActedOn,
                ActedOnBy = entity.actedOnBy,
                tbl_Collateral_Property = AddCollateralProperty((CollateralTypeEnum)entity.collateralTypeId, entity.collateralProperty),
                tbl_Collateral_Deposit = AddCollateralDeposit((CollateralTypeEnum)entity.collateralTypeId, entity.collateralDeposit),
                tbl_Collateral_Machine_Detail = AddCollateralMachineDetail((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMachineDetail),
                tbl_Collateral_Marketable_Security = AddCollateralMarketableSecurity((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMarketableSecurity),
                tbl_Collateral_InsurancePolicy = AddCollateralInsurancePolicy((CollateralTypeEnum)entity.collateralTypeId, entity.collateralInsurancePolicy),
                tbl_Collateral_PreciousMetal = AddCollateralPreciousMetal((CollateralTypeEnum)entity.collateralTypeId, entity.collateralPreciousMetal),
                tbl_Collateral_Gaurantee = AddCollateralGaurantee((CollateralTypeEnum)entity.collateralTypeId, entity.collateralGaurantee),
                tbl_Collateral_Vehicle = AddCollateralVehicle((CollateralTypeEnum)entity.collateralTypeId, entity.collateralVehicle),
                tbl_Collateral_Miscellaneous = AddCollateralMiscellaneous((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMiscellaneous),
                //tbl_Collateral_Customer_Policy = AddCollateralCustomerPolicy(entity.collateralTypeId, entity.collateralCustomerPolicy)
            };

            context.tbl_Collateral_Customer.Add(collateral);
            return await context.SaveChangesAsync() != 0;

        }

        public async Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user)
        {
            var collateral = context.tbl_Collateral_Customer.Find(colleralCustomerId);
            collateral.Deleted = true;
            collateral.DeletedBy = user.staffId;
            collateral.DateTimeDeleted = genSetup.GetApplicaionDate();

            return await context.SaveChangesAsync() != 0;
        }

        private List<CollateralViewModel> CollateralCustomer(int customerId, int companyId)
        {
            tbl_Collateral_Type_Sub sub = new tbl_Collateral_Type_Sub();
            return (from c in context.tbl_Collateral_Customer
                    join t in context.tbl_Collateral_Type on c.CollateralTypeId equals t.CollateralTypeId
                    where c.Deleted == false && c.CompanyId == companyId && c.CustomerId == customerId
                    select new CollateralViewModel
                    {
                        collateralTypeId = c.CollateralTypeId,
                        collateralCustomerId = c.CollateralCustomerId,
                        collateralTypeName = t.CollateralTypeName,
                        collateralCode = c.CollateralCode,
                        collateralValue = c.CollateralValue,
                        collateralValueDate = c.CollateralValueDate,
                        customerId = c.CustomerId,
                        quantity = c.Quantity,
                        releaseDate = c.ReleaseDate,
                        releaseCollateral = c.ReleaseCollateral,
                        chargeTypeId = c.ChargeTypeId,
                        currencyId = c.CurrencyId,
                        limitContribution = c.LimitContribution,
                        graceDays = c.GraceDays,
                        seniorityOfClaimId = c.SeniorityOfClaimId,
                        lendableMargin = c.LendableMargin,
                        //haircut = c.HairCut,
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
                        camRefNumber = c.CamRefNumber,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = c.CreatedBy,
                    }).ToList();
        }

        private List<CollateralViewModel> GetCollateralCustomerByCustomerId(int customerId, int companyId)
        {
            return CollateralCustomer(customerId, companyId);
        }

        public async Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralViewModel entity)
        {
            var collateral = context.tbl_Collateral_Customer.Find(collateralCustomerId);
            collateral.CollateralTypeId = entity.collateralTypeId;
            collateral.CollateralValue = entity.collateralValue;
            collateral.CollateralValueDate = entity.collateralValueDate;
            collateral.CustomerId = entity.customerId;
            collateral.Quantity = entity.quantity;
            collateral.ReleaseDate = entity.releaseDate;
            collateral.ReleaseCollateral = entity.releaseCollateral;
            collateral.DateTimeUpdated = genSetup.GetApplicaionDate().Date;
            collateral.LastUpdatedBy = entity.lastUpdatedBy;

            //TblCollateralMachineDetail collateralMachineDetail = collateral.TblCollateralMachineDetail.FirstOrDefault();

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Property)
            {
                var collateralProperty = context.tbl_Collateral_Property.Find(entity.collateralProperty.collateralPropertyId);

                collateralProperty.PropertyType = entity.collateralProperty.propertyType;
                collateralProperty.CityId = entity.collateralProperty.cityId;
                collateralProperty.CountryId = entity.collateralProperty.countryId;
                collateralProperty.PropertyAddress = entity.collateralProperty.propertyAddress;
                collateralProperty.ConstructionDate = entity.collateralProperty.constructionDate;
                collateralProperty.PurchaseDate = entity.collateralProperty.purchaseDate;
                collateralProperty.ZoneClassification = entity.collateralProperty.zoneClassification;
                collateralProperty.PropertyValueBaseTypeId = entity.collateralProperty.propertyValueBaseTypeId;
                collateralProperty.OtherLendersChargeAmount = entity.collateralProperty.otherLendersChargeAmount;
                collateralProperty.Remark = entity.collateralProperty.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities)
            {
                var collateralMarketableSecurity = context.tbl_Collateral_Marketable_Security.Find(entity.collateralMarketableSecurity.collateralMarketableSecurityId);

                collateralMarketableSecurity.SecurityType = entity.collateralMarketableSecurity.securityType;
                collateralMarketableSecurity.SecurityCode = entity.collateralMarketableSecurity.securityCode;
                collateralMarketableSecurity.Description = entity.collateralMarketableSecurity.description;
                collateralMarketableSecurity.IssuerName = entity.collateralMarketableSecurity.issuerName;
                collateralMarketableSecurity.IssuerReferenceNumber = entity.collateralMarketableSecurity.issuerReferenceNumber;
                collateralMarketableSecurity.UnitValue = entity.collateralMarketableSecurity.unitValue;
                collateralMarketableSecurity.NumberOfUnits = entity.collateralMarketableSecurity.numberOfUnits;
                collateralMarketableSecurity.Remark = entity.collateralMarketableSecurity.remark;

            }
            if (entity.collateralTypeId == (int)CollateralTypeEnum.TermDeposit)
            {
                var collateralDeposit = context.tbl_Collateral_Deposit.Find(entity.collateralDeposit.collateralDepositId);

                collateralDeposit.AccountType = entity.collateralDeposit.accountType;
                collateralDeposit.AccountNumber = entity.collateralDeposit.accountNumber;
                collateralDeposit.AccountBalance = (decimal)entity.collateralDeposit.accountBalance;
                collateralDeposit.Contribution = entity.collateralDeposit.contribution;
                collateralDeposit.Remark = entity.collateralDeposit.remark;
            }

            //if (entity.collateralTypeId == (int)CollateralTypeEnum.CASA)
            //{
            //    var collateralCasa = context.tbl_Collateral_Casa.Find(entity.collateralCasa.collateralCasaId);

            //    collateralCasa.AccountType = entity.collateralCasa.accountType;
            //    collateralCasa.AccountNumber = entity.collateralCasa.accountNumber;
            //    collateralCasa.AccountBalance = (decimal)entity.collateralCasa.accountBalance;
            //    collateralCasa.Contribution = entity.collateralCasa.contribution;
            //    collateralCasa.MaturityDate = entity.collateralCasa.maturityDate;
            //    collateralCasa.Remark = entity.collateralCasa.remark;

            //}

            if (entity.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
            {
                var collateralMachineDetail = context.tbl_Collateral_Machine_Detail.Find(entity.collateralMachineDetail.collateralMachineDetailId);
                    
                collateralMachineDetail.MachineName = entity.collateralMachineDetail.machineName;
                collateralMachineDetail.Manufacturer = entity.collateralMachineDetail.manufacturer;
                collateralMachineDetail.ManufacturedYear = entity.collateralMachineDetail.manufacturedYear;
                collateralMachineDetail.PurchasedYear = entity.collateralMachineDetail.purchasedYear;
                collateralMachineDetail.MachineValueBaseTypeId = entity.collateralMachineDetail.machineValueBaseTypeId;
                collateralMachineDetail.MachineryLocation = entity.collateralMachineDetail.machineryCondition;
                collateralMachineDetail.ReplacementValue = entity.collateralMachineDetail.replacementValue;
                collateralMachineDetail.ThirdPartyChargeAmount = entity.collateralMachineDetail.thirdPartyChargeAmount;
                collateralMachineDetail.MachineryCondition = entity.collateralMachineDetail.machineryCondition;
                collateralMachineDetail.IntendedUse = entity.collateralMachineDetail.intendedUse;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.PreciousMetal)
            {
                var collateralPreciousMetal = context.tbl_Collateral_PreciousMetal.Find(entity.collateralPreciousMetal.collateralPreciousMetalId);

                collateralPreciousMetal.CollateralCustomerId = entity.collateralPreciousMetal.collateralCustomerId;
                collateralPreciousMetal.PreciousMetal = entity.collateralPreciousMetal.preciousMetal;
                collateralPreciousMetal.MetalType = entity.collateralPreciousMetal.metalType;
                collateralPreciousMetal.WeightInGrammes = entity.collateralPreciousMetal.weightInGrammes;
                collateralPreciousMetal.ValuationAmount = entity.collateralPreciousMetal.valuationAmount;
                collateralPreciousMetal.UnitRate = entity.collateralPreciousMetal.unitRate;
                collateralPreciousMetal.PreciousMetalForm = entity.collateralPreciousMetal.preciousMetalForm;
                collateralPreciousMetal.Notes = entity.collateralPreciousMetal.notes;
                collateralPreciousMetal.Remark = entity.collateralPreciousMetal.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.InsurancePolicy)
            {
                tbl_Collateral_InsurancePolicy collateralInsurancePolicy = collateral.tbl_Collateral_InsurancePolicy.Where(x => x.CollateralInsurancePolicyId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralInsurancePolicy.PolicyNumber = entity.collateralInsurancePolicy.policyNumber;
                collateralInsurancePolicy.InsuranceAmount = entity.collateralInsurancePolicy.insuranceAmount;
                collateralInsurancePolicy.StartDate = entity.collateralInsurancePolicy.startDate;
                collateralInsurancePolicy.PremiumAmount = entity.collateralInsurancePolicy.premiumAmount;
                collateralInsurancePolicy.AssignmentDate = entity.collateralInsurancePolicy.assignmentDate;
                collateralInsurancePolicy.InsurerAddress = entity.collateralInsurancePolicy.insurerAddress;
                collateralInsurancePolicy.InsurerDetails = entity.collateralInsurancePolicy.insurerDetails;
                collateralInsurancePolicy.RenewalFrequencyTypeId = entity.collateralInsurancePolicy.renewalFrequencyTypeId;
                collateralInsurancePolicy.NextRenewalDate = entity.collateralInsurancePolicy.nextRenewalDate;
                collateralInsurancePolicy.Remark = entity.collateralInsurancePolicy.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Gaurantee)
            {
                tbl_Collateral_Gaurantee collateralGaurantee = collateral.tbl_Collateral_Gaurantee.Where(x => x.CollateralGauranteeId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralGaurantee.GuaranteeType = entity.collateralGaurantee.guaranteeType;
                collateralGaurantee.GuaranteeAmount = entity.collateralGaurantee.guaranteeAmount;
                //collateralGaurantee.GuarantorCifnumber = entity.collateralGaurantee.guarantorCifnumber;
                collateralGaurantee.GuarantorName = entity.collateralGaurantee.guarantorName;
                collateralGaurantee.GuarantorAddress = entity.collateralGaurantee.guarantorAddress;
                collateralGaurantee.AgreementDate = entity.collateralGaurantee.agreementDate;
                collateralGaurantee.ContinuingGuarantee = entity.collateralGaurantee.continuingGuarantee;
                collateralGaurantee.GuarantorOwnExposure = entity.collateralGaurantee.guarantorOwnExposure;
                collateralGaurantee.TotalGuaranteeAmount = entity.collateralGaurantee.totalGuaranteeAmount;
                collateralGaurantee.Revokeable = entity.collateralGaurantee.revokeable;
                collateralGaurantee.RevokeDate = entity.collateralGaurantee.revokeDate;
                collateralGaurantee.Rating = entity.collateralGaurantee.rating;
                collateralGaurantee.Remark = entity.collateralGaurantee.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Vehicle)
            {
                tbl_Collateral_Vehicle collateralVehicle = collateral.tbl_Collateral_Vehicle.Where(x => x.CollateralVehicleId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralVehicle.VehicleType = entity.collateralVehicle.vehicleType;
                collateralVehicle.NewOrUsed = entity.collateralVehicle.newOrUsed;
                collateralVehicle.Make = entity.collateralVehicle.make;
                collateralVehicle.Model = entity.collateralVehicle.model;
                collateralVehicle.Year = entity.collateralVehicle.year;
                collateralVehicle.RegistrationNumber = entity.collateralVehicle.registrationNumber;
                collateralVehicle.ChasisNumber = entity.collateralVehicle.chasisNumber;
                collateralVehicle.EngineNumber = entity.collateralVehicle.engineNumber;
                collateralVehicle.Owner = entity.collateralVehicle.owner;
                collateralVehicle.RegAuthority = entity.collateralVehicle.regAuthority;
                collateralVehicle.ResaleValue = entity.collateralVehicle.resaleValue;
                collateralVehicle.ValuationDate = entity.collateralVehicle.valuationDate;
                collateralVehicle.ValuationAmount = entity.collateralVehicle.valuationAmount;
                collateralVehicle.InvoiceValue = entity.collateralVehicle.invoiceValue;
                collateralVehicle.Remark = entity.collateralVehicle.remark;
            }

            if (entity.collateralTypeId == (int)CollateralTypeEnum.Miscellaneous)
            {
                tbl_Collateral_Miscellaneous collateralMiscellaneous = collateral.tbl_Collateral_Miscellaneous.Where(x => x.CollateralMiscellaneousId == entity.collateralTypeId)
                    .FirstOrDefault();

                collateralMiscellaneous.CollateralDescription = entity.collateralMiscellaneous.collateralDescription;
                collateralMiscellaneous.Units = entity.collateralMiscellaneous.units;
                collateralMiscellaneous.UnitValue = entity.collateralMiscellaneous.unitValue;
                collateralMiscellaneous.Remark = entity.collateralMiscellaneous.remark;
            }

            //if (entity.collateralCustomerPolicy != null)
            //{
            //    tbl_Collateral_Customer_Policy collateralCustomerPolicy = collateral.tbl_Collateral_Customer_Policy.Where(x => x.PolicyId == entity.collateralCustomerPolicy.policyId)
            //        .FirstOrDefault();

            //    collateralCustomerPolicy.InsuranceCompany = entity.collateralCustomerPolicy.insuranceCompany;
            //    collateralCustomerPolicy.PolicyNumber = entity.collateralCustomerPolicy.policyNumber;
            //    collateralCustomerPolicy.StartDate = entity.collateralCustomerPolicy.startDate;
            //    collateralCustomerPolicy.EndDate = entity.collateralCustomerPolicy.endDate;
            //}

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
                StaffId = (int)entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Update collateral with code: { entity.collateralCode} value { entity.collateralValue}",
                //Ipaddress = entity.userIPAddress,
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
        #endregion Collateral Customer

        #region Property
        private ICollection<tbl_Collateral_Property> AddCollateralProperty(CollateralTypeEnum collateralType, CollateralPropertyViewModel entity)
        {
            ICollection<tbl_Collateral_Property> collateral;

            if (collateralType != CollateralTypeEnum.Property)
                return null;

            collateral = new List<tbl_Collateral_Property>();

            collateral.Add(new tbl_Collateral_Property
            {
                PropertyType = entity.propertyType,
                //CollateralSubTypeId = (short)entity.collateralSubTypeId,
                CityId = entity.cityId,
                CountryId = entity.countryId,
                PropertyAddress = entity.propertyAddress,
                ConstructionDate = entity.constructionDate,
                PurchaseDate = entity.purchaseDate,
                ZoneClassification = entity.zoneClassification,
                PropertyValueBaseTypeId = entity.propertyValueBaseTypeId,
                LastValuationDate = entity.lastValuationDate,
                ValuationSource = entity.valuationSource,
                ValuationAmount = entity.valuationAmount,
                OtherLendersChargeAmount = entity.otherLendersChargeAmount,
                Remark = entity.remark,

            });

            return collateral;
        }

        private CollateralPropertyViewModel CollateralProperty(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Property
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where c.Deleted == false && m.CollateralCustomerId == collateralCustomerId
                    select new CollateralPropertyViewModel
                    {
                        collateralPropertyId = m.CollateralPropertyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        propertyType = m.PropertyType,
                        //collateralSubTypeId = (short)m.CollateralSubTypeId,
                        cityId = m.CityId,
                        countryId = m.CountryId,
                        propertyAddress = m.PropertyAddress,
                        constructionDate = m.ConstructionDate,
                        purchaseDate = m.PurchaseDate,
                        zoneClassification = m.ZoneClassification,
                        propertyValueBaseTypeId = m.PropertyValueBaseTypeId,
                        lastValuationDate = m.LastValuationDate,
                        valuationSource = m.ValuationSource,
                        valuationAmount = m.ValuationAmount,
                        otherLendersChargeAmount = m.OtherLendersChargeAmount,
                        remark = m.Remark,
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
                AccountType = entity.accountType,
                //CollateralSubTypeId = (short)entity.collateralSubTypeId,
                AccountNumber = entity.accountNumber,
                AccountBalance = (decimal)entity.accountBalance,
                Contribution = entity.contribution,
                Remark = entity.remark,
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
                        accountType = m.AccountType,
                        //collateralSubTypeId = (short)m.CollateralSubTypeId,
                        accountNumber = m.AccountNumber,
                        accountBalance = m.AccountBalance,
                        contribution = m.Contribution,
                        remark = m.Remark

                    }).FirstOrDefault();
        }

        private CollateralDepositViewModel GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralDeposit(CollateralCustomerId);
        }
        #endregion Deposit

        #region End od CASA
        //private ICollection<tbl_Collateral_Casa> AddCollateralCasa(CollateralTypeEnum collateralType, CollateralCasaViewModel entity)
        //{
        //    ICollection<tbl_Collateral_Casa> collateral;

        //    if (collateralType != CollateralTypeEnum.CASA)
        //        return null;

        //    collateral = new List<tbl_Collateral_Casa>();

        //    collateral.Add(new tbl_Collateral_Casa
        //    {
        //        AccountType = entity.accountType,
        //        CollateralSubTypeId = (short)entity.collateralSubTypeId,
        //        AccountNumber = entity.accountNumber,
        //        AccountBalance = (decimal)entity.accountBalance,
        //        Contribution = entity.contribution,
        //        MaturityDate = entity.maturityDate,
        //        Remark = entity.remark,
        //    });

        //    return collateral;
        //}

        //private CollateralCasaViewModel CollateralCasa(int collateralCustomerId)
        //{
        //    return (from m in context.tbl_Collateral_Casa
        //            join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
        //            where m.CollateralCustomerId == collateralCustomerId
        //            select new CollateralCasaViewModel
        //            {
        //                collateralCasaId = m.CollateralCasaId,
        //                collateralCustomerId = m.CollateralCustomerId,
        //                accountType = m.AccountType,
        //                collateralSubTypeId = (short)m.CollateralSubTypeId,
        //                accountNumber = m.AccountNumber,
        //                accountBalance = m.AccountBalance,
        //                contribution = m.Contribution,
        //                maturityDate = m.MaturityDate,
        //                remark = m.Remark

        //            }).FirstOrDefault();
        //}

        //private CollateralCasaViewModel GetCollateralCasaByCollateralCustomerId(int CollateralCustomerId)
        //{
        //    return CollateralCasa(CollateralCustomerId);
        //}
        #endregion End of CASA

        #region Machine Detail
        private ICollection<tbl_Collateral_Machine_Detail> AddCollateralMachineDetail(CollateralTypeEnum collateralType, CollateralMachineDetailViewModel entity)
        {
            ICollection<tbl_Collateral_Machine_Detail> collateral;

            if (collateralType != CollateralTypeEnum.PlantAndMachinery)
                return null;

            collateral = new List<tbl_Collateral_Machine_Detail>();

            collateral.Add(new tbl_Collateral_Machine_Detail
            {
                //CollateralSubTypeId = (short)entity.collateralSubTypeId,
                MachineName = entity.machineName,
                Manufacturer = entity.manufacturer,
                ManufacturedYear = entity.manufacturedYear,
                PurchasedYear = entity.purchasedYear,
                MachineValueBaseTypeId = entity.machineValueBaseTypeId,
                MachineryLocation = entity.machineryCondition,
                ReplacementValue = entity.replacementValue,
                ThirdPartyChargeAmount = entity.thirdPartyChargeAmount,
                MachineryCondition = entity.machineryCondition,
                IntendedUse = entity.intendedUse

            });

            return collateral;
        }

        private CollateralMachineDetailViewModel CollateralMachineDetail(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_Machine_Detail
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralMachineDetailViewModel
                    {
                        collateralMachineDetailId = m.CollateralMachineDetailId,
                        collateralCustomerId = m.CollateralCustomerId,
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        machineName = m.MachineName,
                        manufacturer = m.Manufacturer,
                        manufacturedYear = m.ManufacturedYear,
                        purchasedYear = m.PurchasedYear,
                        machineValueBaseTypeId = m.MachineValueBaseTypeId,
                        machineryLocation = m.MachineryCondition,
                        replacementValue = m.ReplacementValue,
                        thirdPartyChargeAmount = m.ThirdPartyChargeAmount,
                        machineryCondition = m.MachineryCondition,
                        intendedUse = m.IntendedUse

                    }).FirstOrDefault();
        }

        private CollateralMachineDetailViewModel GetCollateralMachineDetailByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMachineDetail(collateralCustomerId);
        }
        #endregion Machine Detail

        #region Marketable Security
        private ICollection<tbl_Collateral_Marketable_Security> AddCollateralMarketableSecurity(CollateralTypeEnum collateralType, CollateralMarketableSecurityViewModel entity)
        {
            ICollection<tbl_Collateral_Marketable_Security> collateral;

            if (collateralType != CollateralTypeEnum.MarketableSecurities)
                return null;

            collateral = new List<tbl_Collateral_Marketable_Security>();

            collateral.Add(new tbl_Collateral_Marketable_Security
            {
                CollateralCustomerId = entity.collateralCustomerId,
                SecurityType = entity.securityType,
               // CollateralSubTypeId = (short)entity.collateralSubTypeId,
                SecurityCode = entity.securityCode,
                Description = entity.description,
                IssuerName = entity.issuerName,
                IssuerReferenceNumber = entity.issuerReferenceNumber,
                UnitValue = entity.unitValue,
                NumberOfUnits = entity.numberOfUnits,
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
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        securityCode = m.SecurityCode,
                        description = m.Description,
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
                CollateralCustomerId = entity.collateralCustomerId,
                PreciousMetal = entity.preciousMetal,
                MetalType = entity.metalType,
               // CollateralSubTypeId = (short)entity.collateralSubTypeId,
                WeightInGrammes = entity.weightInGrammes,
                ValuationAmount = entity.valuationAmount,
                UnitRate = entity.unitRate,
                PreciousMetalForm = entity.preciousMetalForm,
                Notes = entity.notes,
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
                        preciousMetal = m.PreciousMetal,
                        metalType = m.MetalType,
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        weightInGrammes = m.WeightInGrammes,
                        valuationAmount = m.ValuationAmount,
                        unitRate = m.UnitRate,
                        preciousMetalForm = m.PreciousMetalForm,
                        notes = m.Notes,
                        remark = m.Remark

                    }).FirstOrDefault();
        }

        private CollateralPreciousMetalViewModel GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralPreciousMetal(collateralCustomerId);
        }
        #endregion Precious Metal

        #region Insurance Policy
        private ICollection<tbl_Collateral_InsurancePolicy> AddCollateralInsurancePolicy(CollateralTypeEnum collateralType, CollateralInsurancePolicyViewModel entity)
        {
            ICollection<tbl_Collateral_InsurancePolicy> collateral;

            if (collateralType != CollateralTypeEnum.InsurancePolicy)
                return null;

            collateral = new List<tbl_Collateral_InsurancePolicy>();

            collateral.Add(new tbl_Collateral_InsurancePolicy
            {
                CollateralInsurancePolicyId = entity.collateralInsurancePolicyId,
                CollateralCustomerId = entity.collateralCustomerId,
                //CollateralSubTypeId = (short)entity.collateralSubTypeId,
                PolicyNumber = entity.policyNumber,
                InsuranceAmount = entity.insuranceAmount,
                StartDate = entity.startDate,
                PremiumAmount = entity.premiumAmount,
                AssignmentDate = entity.assignmentDate,
                InsurerAddress = entity.insurerAddress,
                InsurerDetails = entity.insurerDetails,
                RenewalFrequencyTypeId = entity.renewalFrequencyTypeId,
                NextRenewalDate = entity.nextRenewalDate,
                Remark = entity.remark

            });

            return collateral;
        }

        private CollateralInsurancePolicyViewModel CollateralInsurancePolicy(int collateralCustomerId)
        {
            return (from m in context.tbl_Collateral_InsurancePolicy
                    join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
                    where m.CollateralCustomerId == collateralCustomerId
                    select new CollateralInsurancePolicyViewModel
                    {
                        collateralInsurancePolicyId = m.CollateralInsurancePolicyId,
                        collateralCustomerId = m.CollateralCustomerId,
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        policyNumber = m.PolicyNumber,
                        insuranceAmount = m.InsuranceAmount,
                        startDate = m.StartDate,
                        premiumAmount = m.PremiumAmount,
                        assignmentDate = m.AssignmentDate,
                        insurerAddress = m.InsurerAddress,
                        insurerDetails = m.InsurerDetails,
                        renewalFrequencyTypeId = m.RenewalFrequencyTypeId,
                        nextRenewalDate = m.NextRenewalDate,
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
                CollateralGauranteeId = entity.collateralGauranteeId,
                CollateralCustomerId = entity.collateralCustomerId,
                GuaranteeType = entity.guaranteeType,
                //CollateralSubTypeId = (short)entity.collateralSubTypeId,
                GuaranteeAmount = entity.guaranteeAmount,
                //GuarantorCifnumber = entity.guarantorCifnumber,
                GuarantorName = entity.guarantorName,
                GuarantorAddress = entity.guarantorAddress,
                AgreementDate = entity.agreementDate,
                ContinuingGuarantee = entity.continuingGuarantee,
                GuarantorOwnExposure = entity.guarantorOwnExposure,
                TotalGuaranteeAmount = entity.totalGuaranteeAmount,
                Revokeable = entity.revokeable,
                RevokeDate = entity.revokeDate,
                Rating = entity.rating,
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
                        guaranteeType = m.GuaranteeType,
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        guaranteeAmount = m.GuaranteeAmount,
                        //guarantorCifnumber = m.GuarantorCifnumber,
                        guarantorName = m.GuarantorName,
                        guarantorAddress = m.GuarantorAddress,
                        agreementDate = m.AgreementDate,
                        continuingGuarantee = m.ContinuingGuarantee,
                        guarantorOwnExposure = m.GuarantorOwnExposure,
                        totalGuaranteeAmount = m.TotalGuaranteeAmount,
                        revokeable = m.Revokeable,
                        revokeDate = m.RevokeDate,
                        rating = m.Rating,
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
                CollateralVehicleId = entity.collateralVehicleId,
                CollateralCustomerId = entity.collateralCustomerId,
                VehicleType = entity.vehicleType,
               // CollateralSubTypeId = (short)entity.collateralSubTypeId,
                NewOrUsed = entity.newOrUsed,
                Make = entity.make,
                Model = entity.model,
                Year = entity.year,
                RegistrationNumber = entity.registrationNumber,
                ChasisNumber = entity.chasisNumber,
                EngineNumber = entity.engineNumber,
                Owner = entity.owner,
                RegAuthority = entity.regAuthority,
                ResaleValue = entity.resaleValue,
                ValuationDate = entity.valuationDate,
                ValuationAmount = entity.valuationAmount,
                InvoiceValue = entity.invoiceValue,
                Remark = entity.remark,

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
                        //collateralSubTypeId = m.CollateralSubTypeId,
                        newOrUsed = m.NewOrUsed,
                        make = m.Make,
                        model = m.Model,
                        year = m.Year,
                        registrationNumber = m.RegistrationNumber,
                        chasisNumber = m.ChasisNumber,
                        engineNumber = m.EngineNumber,
                        owner = m.Owner,
                        regAuthority = m.RegAuthority,
                        resaleValue = m.ResaleValue,
                        valuationDate = m.ValuationDate,
                        valuationAmount = m.ValuationAmount,
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
                CollateralMiscellaneousId = entity.collateralMiscellaneousId,
                CollateralCustomerId = entity.collateralCustomerId,
                CollateralDescription = entity.collateralDescription,
                Units = entity.units,
                UnitValue = entity.unitValue,
                Remark = entity.remark
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
                        collateralDescription = m.CollateralDescription,
                        units = m.Units,
                        unitValue = m.UnitValue,
                        remark = m.Remark,
                        collateralMiscellaneousNotes = GetCollateralMiscellaneousNotesByMiscellaneousId(m.CollateralMiscellaneousId)

                    }).FirstOrDefault();
        }

        private CollateralMiscellaneousViewModel GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId)
        {
            return Miscellaneous(collateralCustomerId);
        }
        #endregion Miscellaneous

        #region Miscellaneous Notes
        private Task<bool> AddCollateralMiscNotes(collateralMiscellaneousNotesViewModel entity)
        {
            return null;
        }

        public Task<bool> DeleteCollateralMiscellaneousNotes(int miscNoteId, UserInfo user)
        {
            return null;
        }

        public Task<bool> UpdateCollateralMiscellaneousNotes(int miscNoteId, collateralMiscellaneousNotesViewModel entity)
        {
            return null;
        }

        private List<collateralMiscellaneousNotesViewModel> MiscellaneousNotes(int miscellaneousId)
        {
            return (from m in context.tbl_Collateral_Miscellaneous_Notes
                    join c in context.tbl_Collateral_Miscellaneous on m.MiscellaneousId equals c.CollateralMiscellaneousId
                    where m.MiscellaneousId == miscellaneousId
                    select new collateralMiscellaneousNotesViewModel
                    {
                        miscellaneousNoteId = m.MiscellaneousNoteId,
                        miscellaneousId = m.MiscellaneousId,
                        columnName = m.ColumnName,
                        columnValue = m.ColumnValue,
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                        createdBy = m.CreatedBy
                    }).ToList();
        }

        private List<collateralMiscellaneousNotesViewModel> GetCollateralMiscellaneousNotesByMiscellaneousId(int collateralMiscellaneousId)
        {
            return MiscellaneousNotes(collateralMiscellaneousId);
        }
        #endregion Miscellaneous Notes

        #region Collateral Customer Policy
        //private ICollection<tbl_Collateral_Customer_Policy> AddCollateralCustomerPolicy(int collateralTypeId, CollateralCustomerPolicyViewModel entity)
        //{
        //    var type = context.tbl_Collateral_Type.Where(x => x.CollateralTypeId == collateralTypeId).FirstOrDefault();
        //    ICollection<tbl_Collateral_Customer_Policy> customerPolicy;

        //    if (!type.RequireInsurancePolicy)
        //        return null;
        //    if (entity == null)
        //        throw new InvalidOperationException("This collateral type requires insurance policy which was not submitted");

        //    customerPolicy = new List<tbl_Collateral_Customer_Policy>();

        //    customerPolicy.Add(new tbl_Collateral_Customer_Policy
        //    {
        //        PolicyNumber = entity.policyNumber,
        //        EndDate = entity.endDate,
        //        StartDate = entity.startDate,
        //        InsuranceCompany = entity.insuranceCompany,

        //    });

        //    return customerPolicy;
        //}

        //private CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(int collateralCustomerId)
        //{
        //    return (from m in context.tbl_Collateral_Customer_Policy
        //            where m.CollateralCustomerId == collateralCustomerId
        //            select new CollateralCustomerPolicyViewModel
        //            {
        //                policyId = m.PolicyId,
        //                collateralCustomerId = m.CollateralCustomerId,
        //                insuranceCompany = m.InsuranceCompany,
        //                policyNumber = m.PolicyNumber,
        //                startDate = m.StartDate,
        //                endDate = m.EndDate
        //            }).FirstOrDefault();
        //}
        #endregion End of Collateral Customer Policy

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
                        dateTimeCreated = genSetup.GetApplicaionDate().Date,
                    });
        }
        #endregion Seniority Of Claims

        #region Listing Functions
        public IEnumerable<CollateralTypeViewModel> GetCollateralType()
        {
            return (from m in context.tbl_Collateral_Type
                    select new CollateralTypeViewModel
                    {
                        collateralTypeId = m.CollateralTypeId,
                        collateralTypeName = m.CollateralTypeName,
                        //chargeGlaccountId = m.ChargeGlaccount,
                         //requireInsurancePolicy = m.RequireInsurancePolicy,
                        details = m.Details
                    });
        }

        public IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType()
        {
            return (from m in context.tbl_Collateral_ValueBase_Type
                    select new CollateralValueBaseTypeViewModel
                    {
                        collateralValueBaseTypeId = m.CollateralValueBaseTypeId,
                        collateralTypeId = m.CollateralTypeId,
                        valueBaseTypeName = m.ValueBaseTypeName
                    });
        }

        //public IEnumerable<CollateralValuersViewModel> GetCollateralValuers(int companyId)
        //{
        //    return (from m in context.tbl_Collateral_Valuers
        //            where m.CompanyId == companyId
        //            select new CollateralValuersViewModel
        //            {
        //                collateralValuerId = m.CollateralValuerId,
        //                cityId = m.CityId,
        //                name = m.Name,
        //                valuerLicenceNumber = m.ValuerLicenceNumber,
        //            });
        //}

        public IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType()
        {
            return (from m in context.tbl_Collateral_ValuerType
                    select new CollateralValuerTypeViewModel
                    {
                        collateralValuerTypeId = m.CollateralValuerTypeId,
                        valuerTypeName = m.ValuerTypeName
                    });
        }

        private IEnumerable<CollateralSubTypeViewModel> CollateralSubType()
        {
            return (from m in context.tbl_Collateral_Type_Sub
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.CollateralSubTypeId,
                        collateralTypeId = m.CollateralTypeId,
                        collateralSubTypeName = m.CollateralSubTypeName,
                        haircut = m.Haircut,
                        revaluationDuration = m.RevaluationDuration
                    }).ToList();
        }

        public CollateralSubTypeViewModel GetCollateralSubTypeById(short collateralSubTypeId)
        {
            return CollateralSubType().Where(x => x.collateralSubTypeId == collateralSubTypeId).FirstOrDefault();
        }

        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            return CollateralSubType().Where(x => x.collateralTypeId == collateralTypeId);
        }
        #endregion End of Listing Functions


    }
}