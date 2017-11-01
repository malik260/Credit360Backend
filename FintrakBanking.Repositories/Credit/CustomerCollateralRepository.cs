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
using System.Data.Entity;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;

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
        private IWorkflow workflow;

        public CustomerCollateralRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail, IProductRepository _product,
            IMediaRepository _media,
            ICollateralTypeRepository _collateralType,
            IWorkflow workflow
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.product = _product;
            this.media = _media;
            this.collateralType = _collateralType;
            this.workflow = workflow;
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
                    case (int)CollateralTypeEnum.Gaurantee: AddGuaranteeCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.CASA: AddCasaCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Property: AddImmovablePropertyCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.MarketableSecurities: AddMarketableSecuritiesCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.InsurancePolicy: AddPolicyCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.PreciousMetal: AddPreciousMetalCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Stock: AddStockCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Vehicle: AddVehicleCollateral(collateralId, entity); break;

                    default: break;
                }

                if (entity.hasInsurance) { AddItemInsurancePolicy(collateralId, entity); }

                bool saved;
                try
                {
                    saved = await context.SaveChangesAsync() != 0;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.InnerException.ToString());
                }
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
                case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(entity); break;
                case (int)CollateralTypeEnum.CASA: UpdateCasaCollateral(entity); break;
                case (int)CollateralTypeEnum.Property: UpdateImmovablePropertyCollateral(entity); break;
                case (int)CollateralTypeEnum.MarketableSecurities: UpdateMarketableSecuritiesCollateral(entity); break;
                case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(entity); break;
                case (int)CollateralTypeEnum.PreciousMetal: UpdatePreciousMetalCollateral(entity); break;
                case (int)CollateralTypeEnum.Stock: UpdateStockCollateral(entity); break;
                case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(entity); break;

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
            if (context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == model.collateralCode).Any() == true)
            {
                throw new Exception("The specified Collateral Code is already used in the system!");
            }

            var collateral = context.TBL_COLLATERAL_CUSTOMER.Add(new TBL_COLLATERAL_CUSTOMER
            {
                COLLATERALTYPEID = model.collateralTypeId,
                COLLATERALSUBTYPEID = model.collateralSubTypeId,
                COLLATERALCODE = model.collateralCode,
                COLLATERALVALUE = model.collateralValue,
                COMPANYID = model.companyId,
                ALLOWSHARING = model.allowSharing,
                ISLOCATIONBASED = model.isLocationBased,
                VALUATIONCYCLE = model.valuationCycle,
                HAIRCUT = model.haircut,
                CURRENCYID = model.currencyId,
                CUSTOMERID = model.customerId,
                CAMREFNUMBER = model.camRefNumber,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate()
            });

            return collateral.COLLATERALCUSTOMERID;

            //if (context.SaveChanges() == 1) // may not be needed
            //{
            //    return collateral.CollateralCustomerId;
            //}

            //return 0;
        }

        private void UpdateCollateralMainForm(CollateralViewModel model, int collateralId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId);
            collateral.COLLATERALTYPEID = model.collateralTypeId;
            collateral.COLLATERALSUBTYPEID = model.collateralSubTypeId;
            collateral.COLLATERALCODE = model.collateralCode;
            collateral.COLLATERALVALUE = model.collateralValue;
            collateral.ALLOWSHARING = model.allowSharing;
            collateral.ISLOCATIONBASED = model.isLocationBased;
            collateral.VALUATIONCYCLE = model.valuationCycle;
            collateral.HAIRCUT = model.haircut;
            collateral.CURRENCYID = model.currencyId;
            collateral.CAMREFNUMBER = model.camRefNumber;
            collateral.LASTUPDATEDBY = model.lastUpdatedBy;
            collateral.DATETIMEUPDATED = genSetup.GetApplicationDate();
        }

        private void DeleteCollateral(int collateralId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId);
            collateral.DELETED = true; // audit here
            context.SaveChanges();
        }


        // EQUIPMENT collateral

        private void AddEquipmentCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_PLANT_AND_EQUIPMENT.Add(new TBL_COLLATERAL_PLANT_AND_EQUIPMENT
            {
                COLLATERALCUSTOMERID = collateralId,
                MACHINENAME = entity.machineName,
                DESCRIPTION = entity.description,
                MACHINENUMBER = entity.machineNumber,
                MANUFACTURERNAME = entity.manufacturerName,
                YEAROFMANUFACTURE = entity.yearOfManufacture,
                YEAROFPURCHASE = entity.yearOfPurchase,
                VALUEBASETYPEID = (short)entity.valueBaseTypeId,
                MACHINECONDITION = entity.machineCondition,
                MACHINERYLOCATION = entity.machineryLocation,
                REPLACEMENTVALUE = entity.replacementValue,
                EQUIPMENTSIZE = entity.equipmentSize,
                INTENDEDUSE = entity.intendedUse,
            });
        }

        private void UpdateEquipmentCollateral(CollateralViewModel model)
        {
            var collateral = context.TBL_COLLATERAL_PLANT_AND_EQUIPMENT
                .Where(x => x.COLLATERALCUSTOMERID == model.collateralId)
                .FirstOrDefault();

            collateral.MACHINENAME = model.machineName;
            collateral.DESCRIPTION = model.description;
            collateral.MACHINENUMBER = model.machineNumber;
            collateral.MANUFACTURERNAME = model.manufacturerName;
            collateral.YEAROFMANUFACTURE = model.yearOfManufacture;
            collateral.YEAROFPURCHASE = model.yearOfPurchase;
            collateral.VALUEBASETYPEID = (short)model.valueBaseTypeId;
            collateral.MACHINECONDITION = model.machineCondition;
            collateral.MACHINERYLOCATION = model.machineryLocation;
            collateral.REPLACEMENTVALUE = model.replacementValue;
            collateral.EQUIPMENTSIZE = model.equipmentSize;
            collateral.INTENDEDUSE = model.intendedUse;
        }

        // FIX DEPOSIT collateral

        private void AddDepositCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_DEPOSIT.Add(new TBL_COLLATERAL_DEPOSIT
            {
                COLLATERALCUSTOMERID = collateralId,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                ACCOUNTNUMBER = entity.accountNumber,
                EXISTINGLIENAMOUNT = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                AVAILABLEBALANCE = entity.availableBalance,
                SECURITYVALUE = entity.securityValue,
                MATURITYDATE = entity.maturityDate,
                MATURITYAMOUNT = entity.maturityAmount,
                REMARK = entity.remark,
            });
        }

        private void UpdateDepositCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_DEPOSIT
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.DEALREFERENCENUMBER = entity.dealReferenceNumber;
            collateral.ACCOUNTNUMBER = entity.accountNumber;
            collateral.EXISTINGLIENAMOUNT = entity.existingLienAmount;
            collateral.LIENAMOUNT = entity.lienAmount;
            collateral.AVAILABLEBALANCE = entity.availableBalance;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.MATURITYDATE = entity.maturityDate;
            collateral.MATURITYAMOUNT = entity.maturityAmount;
            collateral.REMARK = entity.remark;
        }

        // MISCELALEOUS

        private void AddMiscellaneousCollateral(int collateralId, CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_MISCELLANEOUS.Add(new TBL_COLLATERAL_MISCELLANEOUS
            {
                COLLATERALCUSTOMERID = collateralId,
                NAMEOFSECURITY = entity.securityName,
                SECURITYVALUE = entity.securityValue,
            });

            //if (context.SaveChanges() > 0) // EF will take care of this
            AddMiscellaneousNotes(entity, collateral.COLLATERALMISCELLANEOUSID);
        }

        private void AddMiscellaneousNotes(CollateralViewModel entity, int miscellaneousId)
        {
            if (entity.notes != null)
            {
                foreach (var note in entity.notes)
                {
                    context.TBL_COLLATERAL_MISCELLANEOUS_NOTES.Add(new TBL_COLLATERAL_MISCELLANEOUS_NOTES
                    {
                        MISCELLANEOUSID = miscellaneousId,
                        COLUMNNAME = note.labelName,
                        COLUMNVALUE = note.labelValue,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now
                    });
                }
                //context.SaveChanges();
            }
        }

        private void UpdateMiscellaneousCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_MISCELLANEOUS
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.NAMEOFSECURITY = entity.securityName;
            collateral.SECURITYVALUE = entity.securityValue;

            UpdateMiscellaneousNotes(entity, collateral.COLLATERALMISCELLANEOUSID);
        }

        private void UpdateMiscellaneousNotes(CollateralViewModel entity, int miscellaneousId)
        {
            var notes = context.TBL_COLLATERAL_MISCELLANEOUS_NOTES.Where(x => x.MISCELLANEOUSID == miscellaneousId);
            foreach (var note in notes)
            {
                note.COLUMNVALUE = entity.notes.FirstOrDefault(x => x.labelName == note.COLUMNNAME).labelValue;
            }
        }

        // ITEM INSURANCE

        private void AddItemInsurancePolicy(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
            {
                COLLATERALCUSTOMERID = collateralId,
                POLICYREFERENCENUMBER = entity.referenceNumber,
                INSURANCECOMPANYNAME = entity.insuranceCompany,
                SUMINSURED = entity.sumInsured,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = (DateTime)entity.expiryDate,
            });
        }

        private void UpdateItemInsurancePolicy(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_ITEM_POLICY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.POLICYREFERENCENUMBER = entity.referenceNumber;
            collateral.INSURANCECOMPANYNAME = entity.insuranceCompany;
            collateral.SUMINSURED = entity.sumInsured;
            collateral.STARTDATE = (DateTime)entity.startDate;
            collateral.ENDDATE = (DateTime)entity.expiryDate;
        }

        // GET MAIN INFO

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int companyId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false
                && x.COMPANYID == companyId
                && x.CUSTOMERID == customerId
            )
            .Select(x => new CollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralTypeId = x.COLLATERALTYPEID,
                collateralSubTypeId = x.COLLATERALSUBTYPEID,
                customerId = x.CUSTOMERID,
                currencyId = x.CURRENCYID,
                currency = x.TBL_CURRENCY.CURRENCYNAME,
                collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                collateralCode = x.COLLATERALCODE,
                collateralValue = x.COLLATERALVALUE,
                camRefNumber = x.CAMREFNUMBER,
                allowSharing = x.ALLOWSHARING,
                isLocationBased = x.ISLOCATIONBASED,
                valuationCycle = x.VALUATIONCYCLE,
                haircut = x.HAIRCUT,
                approvalStatus = x.APPROVALSTATUS,
            })
            .OrderByDescending(x => x.collateralId)
            .ToList();

            return collateral;
        }

        public IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId)
        {
            return GetCustomerCollateral(companyId).Where(x=>x.collateralTypeId == collateralTypeId && (x.customerId==customerId || x.customerId == thirdpartyCustomerId));
            
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false
                && x.COMPANYID == companyId
            )
            .Select(x => new CollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralTypeId = x.COLLATERALTYPEID,
                collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                collateralSubTypeId = x.COLLATERALSUBTYPEID,
                customerId = x.CUSTOMERID,
                currencyId = x.CURRENCYID,
                currency = x.TBL_CURRENCY.CURRENCYNAME,
                currencyCode = x.TBL_CURRENCY.CURRENCYCODE,
                collateralCode = x.COLLATERALCODE,
                collateralValue = x.COLLATERALVALUE,
                camRefNumber = x.CAMREFNUMBER,
                allowSharing = x.ALLOWSHARING,
                isLocationBased = x.ISLOCATIONBASED,
                valuationCycle = x.VALUATIONCYCLE,
                haircut = x.HAIRCUT,
                approvalStatus = x.APPROVALSTATUS,
                //collateralValue = x.CollateralValue

            })
            .OrderByDescending(x => x.collateralId)
            
            .ToList();

            
            
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
                case (int)CollateralTypeEnum.Miscellaneous: data = GetCollateralMiscellaneous(collateralId); break;
                case (int)CollateralTypeEnum.Gaurantee: data = GetCollateralGuarantee(collateralId); break;
                case (int)CollateralTypeEnum.CASA: data = GetCollateralCasa(collateralId); break;
                case (int)CollateralTypeEnum.Property: data = GetCollateralImmovableProperty(collateralId); break;
                case (int)CollateralTypeEnum.MarketableSecurities: data = GetCollateralMarketableSecurities(collateralId); break;
                case (int)CollateralTypeEnum.InsurancePolicy: data = GetCollateralPolicy(collateralId); break;
                case (int)CollateralTypeEnum.PreciousMetal: data = GetCollateralPreciousMetal(collateralId); break;
                case (int)CollateralTypeEnum.Stock: data = GetCollateralStock(collateralId); break;
                case (int)CollateralTypeEnum.Vehicle: data = GetCollateralVehicle(collateralId); break;

                default:
                    break;
            }

            return data;
        }

        private CollateralViewModel GetCollateralMiscellaneous(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_MISCELLANEOUS.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                detailId = specifics.COLLATERALMISCELLANEOUSID,
                securityName = specifics.NAMEOFSECURITY,
                securityValue = specifics.SECURITYVALUE,
            };
            details = GetMiscellaneousNotes(details);
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        private CollateralViewModel GetMiscellaneousNotes(CollateralViewModel details)
        {
            var notes = context.TBL_COLLATERAL_MISCELLANEOUS_NOTES.Where(x => x.MISCELLANEOUSID == details.detailId);
            var list = new List<MiscellaneousNote>();
            foreach (var note in notes)
            {
                list.Add(new MiscellaneousNote
                {
                    labelName = note.COLUMNNAME,
                    labelValue = note.COLUMNVALUE,
                    controlName = note.COLUMNNAME,
                });
            }
            details.notes = list;
            return details;
        }

        private CollateralViewModel GetCollateralMachinery(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_PLANT_AND_EQUIPMENT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                machineName = specifics.MACHINENAME,
                description = specifics.DESCRIPTION,
                machineNumber = specifics.MACHINENUMBER,
                manufacturerName = specifics.MANUFACTURERNAME,
                yearOfManufacture = specifics.YEAROFMANUFACTURE,
                yearOfPurchase = specifics.YEAROFPURCHASE,
                valueBaseTypeId = specifics.VALUEBASETYPEID,
                machineCondition = specifics.MACHINECONDITION,
                machineryLocation = specifics.MACHINERYLOCATION,
                replacementValue = specifics.REPLACEMENTVALUE,
                equipmentSize = specifics.EQUIPMENTSIZE,
                intendedUse = specifics.INTENDEDUSE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        private CollateralViewModel GetCollateralDeposit(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralDepositId = specifics.COLLATERALDEPOSITID,
                dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                accountNumber = specifics.ACCOUNTNUMBER,
                existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                availableBalance = specifics.AVAILABLEBALANCE,
                securityValue = specifics.SECURITYVALUE,
                maturityDate = specifics.MATURITYDATE,
                maturityAmount = specifics.MATURITYAMOUNT,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        private CollateralViewModel GetCollateralInsurancePolicy(CollateralViewModel details)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == details.collateralId);
            if (insurance != null)
            {
                details.referenceNumber = insurance.POLICYREFERENCENUMBER;
                details.insuranceCompany = insurance.INSURANCECOMPANYNAME;
                details.sumInsured = insurance.SUMINSURED;
                details.startDate = insurance.STARTDATE;
                details.expiryDate = insurance.ENDDATE;
            }
            return details;
        }

        // stock collateral

        private void AddStockCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_STOCK.Add(new TBL_COLLATERAL_STOCK
            {
                COLLATERALCUSTOMERID = collateralId,
                COMPANYNAME = entity.companyName,
                SHAREQUANTITY = entity.shareQuantity,
                MARKETPRICE = entity.marketPrice,
                AMOUNT = entity.amount,
                SHARESSECURITYVALUE = entity.sharesSecurityValue,
                SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse,
            });
        }

        private void UpdateStockCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_STOCK
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.COMPANYNAME = entity.companyName;
            collateral.SHAREQUANTITY = entity.shareQuantity;
            collateral.MARKETPRICE = entity.marketPrice;
            collateral.AMOUNT = entity.amount;
            collateral.SHARESSECURITYVALUE = entity.sharesSecurityValue;
            collateral.SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse;
        }

        private CollateralViewModel GetCollateralStock(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_STOCK.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralStockId = specifics.COLLATERALSTOCKID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                companyName = specifics.COMPANYNAME,
                shareQuantity = specifics.SHAREQUANTITY,
                marketPrice = specifics.MARKETPRICE,
                amount = specifics.AMOUNT,
                sharesSecurityValue = specifics.SHARESSECURITYVALUE,
                shareValueAmountToUse = specifics.SHAREVALUEAMOUNTTOUSE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }


        // vehicle collateral

        private void AddVehicleCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_VEHICLE.Add(new TBL_COLLATERAL_VEHICLE
            {
                COLLATERALCUSTOMERID = collateralId,
                VEHICLETYPE = entity.vehicleType,
                VEHICLESTATUS = entity.vehicleStatus,
                VEHICLEMAKE = entity.vehicleMake,
                MODELNAME = entity.modelName,
                MANUFACTUREDDATE = entity.manufacturedDate,
                REGISTRATIONNUMBER = entity.registrationNumber,
                SERIALNUMBER = entity.serialNumber,
                CHASISNUMBER = entity.chasisNumber,
                ENGINENUMBER = entity.engineNumber,
                NAMEOFOWNER = entity.nameOfOwner,
                REGISTRATIONCOMPANY = entity.registrationCompany,
                RESALEVALUE = entity.resaleValue,
                VALUATIONDATE = entity.valuationDate,
                LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                INVOICEVALUE = entity.invoiceValue,
                REMARK = entity.remark,
            });
        }

        private void UpdateVehicleCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_VEHICLE
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.VEHICLETYPE = entity.vehicleType;
            collateral.VEHICLESTATUS = entity.vehicleStatus;
            collateral.VEHICLEMAKE = entity.vehicleMake;
            collateral.MODELNAME = entity.modelName;
            collateral.MANUFACTUREDDATE = entity.manufacturedDate;
            collateral.REGISTRATIONNUMBER = entity.registrationNumber;
            collateral.SERIALNUMBER = entity.serialNumber;
            collateral.CHASISNUMBER = entity.chasisNumber;
            collateral.ENGINENUMBER = entity.engineNumber;
            collateral.NAMEOFOWNER = entity.nameOfOwner;
            collateral.REGISTRATIONCOMPANY = entity.registrationCompany;
            collateral.RESALEVALUE = entity.resaleValue;
            collateral.VALUATIONDATE = entity.valuationDate;
            collateral.LASTVALUATIONAMOUNT = entity.lastValuationAmount;
            collateral.INVOICEVALUE = entity.invoiceValue;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralVehicle(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_VEHICLE.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralVehicleId = specifics.COLLATERALVEHICLEID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                vehicleType = specifics.VEHICLETYPE,
                vehicleStatus = specifics.VEHICLESTATUS,
                vehicleMake = specifics.VEHICLEMAKE,
                modelName = specifics.MODELNAME,
                manufacturedDate = specifics.MANUFACTUREDDATE,
                registrationNumber = specifics.REGISTRATIONNUMBER,
                serialNumber = specifics.SERIALNUMBER,
                chasisNumber = specifics.CHASISNUMBER,
                engineNumber = specifics.ENGINENUMBER,
                nameOfOwner = specifics.NAMEOFOWNER,
                registrationCompany = specifics.REGISTRATIONCOMPANY,
                resaleValue = specifics.RESALEVALUE,
                valuationDate = specifics.VALUATIONDATE,
                lastValuationAmount = specifics.LASTVALUATIONAMOUNT,
                invoiceValue = specifics.INVOICEVALUE,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // preciousMetal collateral

        private void AddPreciousMetalCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_PRECIOUSMETAL.Add(new TBL_COLLATERAL_PRECIOUSMETAL
            {
                COLLATERALCUSTOMERID = collateralId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                PRECIOUSMETALNAME = entity.preciousMetalName,
                WEIGHTINGRAMMES = entity.weightInGrammes,
                VALUATIONAMOUNT = entity.valuationAmount,
                UNITRATE = entity.unitRate,
                PRECIOUSMETALFORM = entity.preciousMetalForm,
                REMARK = entity.remark,
            });
        }

        private void UpdatePreciousMetalCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_PRECIOUSMETAL
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.PRECIOUSMETALNAME = entity.preciousMetalName;
            collateral.WEIGHTINGRAMMES = entity.weightInGrammes;
            collateral.VALUATIONAMOUNT = entity.valuationAmount;
            collateral.UNITRATE = entity.unitRate;
            collateral.PRECIOUSMETALFORM = entity.preciousMetalForm;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralPreciousMetal(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_PRECIOUSMETAL.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralPreciousMetalId = specifics.COLLATERALPRECIOUSMETALID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                preciousMetalName = specifics.PRECIOUSMETALNAME,
                weightInGrammes = specifics.WEIGHTINGRAMMES,
                valuationAmount = specifics.VALUATIONAMOUNT,
                unitRate = specifics.UNITRATE,
                preciousMetalForm = specifics.PRECIOUSMETALFORM,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // casa collateral

        private void AddCasaCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_CASA.Add(new TBL_COLLATERAL_CASA
            {
                COLLATERALCUSTOMERID = collateralId,
                ACCOUNTNUMBER = entity.accountNumber,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                AVAILABLEBALANCE = entity.availableBalance,
                EXISTINGLIENAMOUNT = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                SECURITYVALUE = entity.securityValue,
                REMARK = entity.remark,
            });
        }

        private void UpdateCasaCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_CASA
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.ACCOUNTNUMBER = entity.accountNumber;
            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.AVAILABLEBALANCE = entity.availableBalance;
            collateral.EXISTINGLIENAMOUNT = entity.existingLienAmount;
            collateral.LIENAMOUNT = entity.lienAmount;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralCasa(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                accountNumber = specifics.ACCOUNTNUMBER,
                isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                availableBalance = specifics.AVAILABLEBALANCE,
                existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // guarantee collateral

        private void AddGuaranteeCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_GAURANTEE.Add(new TBL_COLLATERAL_GAURANTEE
            {
                COLLATERALCUSTOMERID = collateralId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                INSTITUTIONNAME = entity.institutionName,
                GUARANTORADDRESS = entity.guarantorAddress,
                GUARANTORREFERENCENUMBER = entity.guarantorReferenceNumber,
                GUARANTEEVALUE = entity.guaranteeValue,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = entity.endDate,
                REMARK = entity.remark,
            });
        }

        private void UpdateGuaranteeCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_GAURANTEE
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.INSTITUTIONNAME = entity.institutionName;
            collateral.GUARANTORADDRESS = entity.guarantorAddress;
            collateral.GUARANTORREFERENCENUMBER = entity.guarantorReferenceNumber;
            collateral.GUARANTEEVALUE = entity.guaranteeValue;
            collateral.STARTDATE = (DateTime)entity.startDate;
            collateral.ENDDATE = entity.endDate;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralGuarantee(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_GAURANTEE.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralGauranteeId = specifics.COLLATERALGAURANTEEID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                isOwnedByCustomer = (bool)specifics.ISOWNEDBYCUSTOMER,
                institutionName = specifics.INSTITUTIONNAME,
                guarantorAddress = specifics.GUARANTORADDRESS,
                guarantorReferenceNumber = specifics.GUARANTORREFERENCENUMBER,
                guaranteeValue = specifics.GUARANTEEVALUE,
                startDate = specifics.STARTDATE,
                endDate = specifics.ENDDATE,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // immovableProperty collateral

        private void AddImmovablePropertyCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_IMMOVABLE_PROPERTY.Add(new TBL_COLLATERAL_IMMOVABLE_PROPERTY
            {
                COLLATERALCUSTOMERID = collateralId,
                PROPERTYNAME = entity.propertyName,
                CITYID = entity.cityId,
                COUNTRYID = entity.countryId,
                CONSTRUCTIONDATE = entity.constructionDate,
                PROPERTYADDRESS = entity.propertyAddress,
                DATEOFACQUISITION = entity.dateOfAcquisition,
                LASTVALUATIONDATE = entity.lastValuationDate,
                VALUERID = entity.valuerId,
                VALUERREFERENCENUMBER = entity.valuerReferenceNumber,
                PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId,
                OPENMARKETVALUE = entity.openMarketValue,
                COLLATERALVALUE = (decimal)entity.collateralValue,
                FORCEDSALEVALUE = entity.forcedSaleValue,
                STAMPTOCOVER = entity.stampToCover,
                VALUATIONSOURCE = entity.valuationSource,
                ORIGINALVALUE = entity.originalValue,
                AVAILABLEVALUE = entity.availableValue,
                SECURITYVALUE = entity.securityValue,
                COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount,
                REMARK = entity.remark,
                NEARESTLANDMARK = entity.nearestLandMark,
                NEARESTBUSSTOP = entity.nearestBusStop,
                LONGITUDE = entity.longitude,
                LATITUDE = entity.latitude,
            });
        }

        private void UpdateImmovablePropertyCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_IMMOVABLE_PROPERTY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.PROPERTYNAME = entity.propertyName;
            collateral.CITYID = entity.cityId;
            collateral.COUNTRYID = entity.countryId;
            collateral.CONSTRUCTIONDATE = entity.constructionDate;
            collateral.PROPERTYADDRESS = entity.propertyAddress;
            collateral.DATEOFACQUISITION = entity.dateOfAcquisition;
            collateral.LASTVALUATIONDATE = entity.lastValuationDate;
            collateral.VALUERID = entity.valuerId;
            collateral.VALUERREFERENCENUMBER = entity.valuerReferenceNumber;
            collateral.PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId;
            collateral.OPENMARKETVALUE = entity.openMarketValue;
            collateral.COLLATERALVALUE = (decimal)entity.collateralValue;
            collateral.FORCEDSALEVALUE = entity.forcedSaleValue;
            collateral.STAMPTOCOVER = entity.stampToCover;
            collateral.VALUATIONSOURCE = entity.valuationSource;
            collateral.ORIGINALVALUE = entity.originalValue;
            collateral.AVAILABLEVALUE = entity.availableValue;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount;
            collateral.REMARK = entity.remark;
            collateral.NEARESTLANDMARK = entity.nearestLandMark;
            collateral.NEARESTBUSSTOP = entity.nearestBusStop;
            collateral.LONGITUDE = entity.longitude;
            collateral.LATITUDE = entity.latitude;
        }

        private CollateralViewModel GetCollateralImmovableProperty(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_IMMOVABLE_PROPERTY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralPropertyId = specifics.COLLATERALPROPERTYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                propertyName = specifics.PROPERTYNAME,
                cityId = specifics.CITYID,
                countryId = specifics.COUNTRYID,
                constructionDate = specifics.CONSTRUCTIONDATE,
                propertyAddress = specifics.PROPERTYADDRESS,
                dateOfAcquisition = specifics.DATEOFACQUISITION,
                lastValuationDate = specifics.LASTVALUATIONDATE,
                valuerId = specifics.VALUERID,
                valuerReferenceNumber = specifics.VALUERREFERENCENUMBER,
                propertyValueBaseTypeId = specifics.PROPERTYVALUEBASETYPEID,
                openMarketValue = specifics.OPENMARKETVALUE,
                collateralValue = specifics.COLLATERALVALUE,
                forcedSaleValue = specifics.FORCEDSALEVALUE,
                stampToCover = specifics.STAMPTOCOVER,
                valuationSource = specifics.VALUATIONSOURCE,
                originalValue = specifics.ORIGINALVALUE,
                availableValue = specifics.AVAILABLEVALUE,
                securityValue = (decimal)specifics.SECURITYVALUE,
                collateralUsableAmount = specifics.COLLATERALUSABLEAMOUNT,
                remark = specifics.REMARK,
                nearestLandMark = specifics.NEARESTLANDMARK,
                nearestBusStop = specifics.NEARESTBUSSTOP,
                longitude = specifics.LONGITUDE,
                latitude = specifics.LATITUDE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // marketableSecurities collateral

        private void AddMarketableSecuritiesCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_MARKETABLE_SECURITY.Add(new TBL_COLLATERAL_MARKETABLE_SECURITY
            {
                COLLATERALCUSTOMERID = collateralId,
                SECURITYTYPE = entity.securityType,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                EFFECTIVEDATE = entity.effectiveDate,
                MATURITYDATE = entity.maturityDate,
                DEALAMOUNT = entity.dealAmount,
                SECURITYVALUE = entity.securityValue,
                LIENUSABLEAMOUNT = entity.lienUsableAmount,
                ISSUERNAME = entity.issuerName,
                ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                UNITVALUE = entity.unitValue,
                NUMBEROFUNITS = entity.numberOfUnits,
                RATING = entity.rating,
                PERCENTAGEINTEREST = entity.percentageInterest,
                INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                REMARK = entity.remark,
            });
        }

        private void UpdateMarketableSecuritiesCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_MARKETABLE_SECURITY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.SECURITYTYPE = entity.securityType;
            collateral.DEALREFERENCENUMBER = entity.dealReferenceNumber;
            collateral.EFFECTIVEDATE = entity.effectiveDate;
            collateral.MATURITYDATE = entity.maturityDate;
            collateral.DEALAMOUNT = entity.dealAmount;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.LIENUSABLEAMOUNT = entity.lienUsableAmount;
            collateral.ISSUERNAME = entity.issuerName;
            collateral.ISSUERREFERENCENUMBER = entity.issuerReferenceNumber;
            collateral.UNITVALUE = entity.unitValue;
            collateral.NUMBEROFUNITS = entity.numberOfUnits;
            collateral.RATING = entity.rating;
            collateral.PERCENTAGEINTEREST = entity.percentageInterest;
            collateral.INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralMarketableSecurities(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_MARKETABLE_SECURITY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralMarketableSecurityId = specifics.COLLATERALMARKETABLESECURITYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                securityType = specifics.SECURITYTYPE,
                dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                effectiveDate = specifics.EFFECTIVEDATE,
                maturityDate = specifics.MATURITYDATE,
                dealAmount = specifics.DEALAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                lienUsableAmount = specifics.LIENUSABLEAMOUNT,
                issuerName = specifics.ISSUERNAME,
                issuerReferenceNumber = specifics.ISSUERREFERENCENUMBER,
                unitValue = specifics.UNITVALUE,
                numberOfUnits = specifics.NUMBEROFUNITS,
                rating = specifics.RATING,
                percentageInterest = specifics.PERCENTAGEINTEREST,
                interestPaymentFrequency = specifics.INTERESTPAYMENTFREQUENCY,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }


        // policy collateral

        private void AddPolicyCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_POLICY.Add(new TBL_COLLATERAL_POLICY
            {
                COLLATERALCUSTOMERID = collateralId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                PREMIUMAMOUNT = entity.premiumAmount,
                POLICYAMOUNT = entity.policyAmount,
                INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                INSURERADDRESS = entity.insurerAddress,
                POLICYSTARTDATE = entity.policyStartDate,
                ASSIGNDATE = entity.assignDate,
                RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                INSURERDETAILS = entity.insurerDetails,
                POLICYRENEWALDATE = entity.policyRenewalDate,
                REMARK = entity.remark,
            });
        }

        private void UpdatePolicyCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_POLICY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber;
            collateral.PREMIUMAMOUNT = entity.premiumAmount;
            collateral.POLICYAMOUNT = entity.policyAmount;
            collateral.INSURANCECOMPANYNAME = entity.insuranceCompanyName;
            collateral.INSURERADDRESS = entity.insurerAddress;
            collateral.POLICYSTARTDATE = entity.policyStartDate;
            collateral.ASSIGNDATE = entity.assignDate;
            collateral.RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId;
            collateral.INSURERDETAILS = entity.insurerDetails;
            collateral.POLICYRENEWALDATE = entity.policyRenewalDate;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralPolicy(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralInsurancePolicyId = specifics.COLLATERALINSURANCEPOLICYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                insurancePolicyNumber = specifics.INSURANCEPOLICYNUMBER,
                premiumAmount = specifics.PREMIUMAMOUNT,
                policyAmount = specifics.POLICYAMOUNT,
                insuranceCompanyName = specifics.INSURANCECOMPANYNAME,
                insurerAddress = specifics.INSURERADDRESS,
                policyStartDate = specifics.POLICYSTARTDATE,
                assignDate = specifics.ASSIGNDATE,
                renewalFrequencyTypeId = specifics.RENEWALFREQUENCYTYPEID,
                insurerDetails = specifics.INSURERDETAILS,
                policyRenewalDate = specifics.POLICYRENEWALDATE,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        #endregion New 

        public IEnumerable<ActiveCustomerCollateralViewModel> GetActiveCustomerCollateral(int customerId) // REFACTOR PROJECTION
        {
            // tbl_Customer --> tbl_Collateral_Customer --> tbl_Loan_Application --> tbl_Loan_Collateral_Mapping

            var collaterals = context.TBL_CUSTOMER//.Where(x => x.CustomerId == customerId)
                .Join(context.TBL_COLLATERAL_CUSTOMER, c => c.CUSTOMERID, o => o.CUSTOMERID, (c, o) => new { Customer = c, Collateral = o })
                .Join(context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == customerId), cc => cc.Collateral.CUSTOMERID, a => a.CUSTOMERID, (cc, a) => new { CustomerCollateral = cc, Application = a })
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING, ca => ca.Application.LOANAPPLICATIONID, m => m.LOANAPPLICATIONID, (ca, m) => new { CollateralApplication = ca, Mapping = m })
                .Select(x => new ActiveCustomerCollateralViewModel
                {
                    customerId = x.CollateralApplication.Application.CUSTOMERID,
                    collateralCustomerId = x.Mapping.COLLATERALCUSTOMERID,
                    //currencyId = x.CollateralApplication.Application.CurrencyId,
                    //productId = x.CollateralApplication.Application.ProductId,
                    loanTypeId = x.CollateralApplication.Application.LOANTYPEID,
                    loanCollateralMappingId = x.Mapping.LOANCOLLATERALMAPPINGID,
                    //loanId = x.Mapping.LoanId,
                    loanApplicationId = x.Mapping.LOANAPPLICATIONID,
                    isReleased = x.Mapping.ISRELEASED,
                    releaseApprovalStatusId = (short)x.Mapping.RELEASEAPPROVALSTATUSID,
                    //productTypeId = x.Mapping.ProductTypeId,
                    customerCode = x.CollateralApplication.CustomerCollateral.Customer.CUSTOMERCODE,
                    firstName = x.CollateralApplication.CustomerCollateral.Customer.FIRSTNAME,
                    middleName = x.CollateralApplication.CustomerCollateral.Customer.MIDDLENAME,
                    lastName = x.CollateralApplication.CustomerCollateral.Customer.LASTNAME,
                    collateralCode = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                    collateralValue = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                    allowSharing = x.Mapping.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                    isLocationBased = x.Mapping.TBL_COLLATERAL_CUSTOMER.ISLOCATIONBASED,
                    valuationCycle = x.Mapping.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                    hairCut = x.Mapping.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                    collateralTypeId = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                    applicationReferenceNumber = x.CollateralApplication.Application.APPLICATIONREFERENCENUMBER,
                    applicationDate = x.CollateralApplication.Application.APPLICATIONDATE,
                    //principalAmount = x.CollateralApplication.Application.PrincipalAmount,
                    interestRate = x.CollateralApplication.Application.INTERESTRATE,
                    //exchangeRate = x.CollateralApplication.Application.ExchangeRate,
                    //tenor = x.CollateralApplication.Application.Tenor,
                    loanInformation = x.CollateralApplication.Application.LOANINFORMATION,
                })
                .Where(x => x.isReleased == false)
                .Distinct();

            return collaterals;
        }

        public IEnumerable<ActiveCustomerCollateralViewModel> GetLoanCollateral(int loanId) 
        {
            var collaterals = context.TBL_CUSTOMER
                .Join(context.TBL_COLLATERAL_CUSTOMER, c => c.CUSTOMERID, o => o.CUSTOMERID, (c, o) => new { Customer = c, Collateral = o })
                .Join(context.TBL_LOAN_APPLICATION, cc => cc.Collateral.CUSTOMERID, a => a.CUSTOMERID, (cc, a) => new { CustomerCollateral = cc, Application = a })
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING, ca => ca.Application.LOANAPPLICATIONID, m => m.LOANAPPLICATIONID, (ca, m) => new { CollateralApplication = ca, Mapping = m })
                .Select(x => new ActiveCustomerCollateralViewModel
                {
                    customerId = x.CollateralApplication.Application.CUSTOMERID,
                    collateralCustomerId = x.Mapping.COLLATERALCUSTOMERID,
                    loanTypeId = x.CollateralApplication.Application.LOANTYPEID,
                    loanCollateralMappingId = x.Mapping.LOANCOLLATERALMAPPINGID,
                    loanApplicationId = x.Mapping.LOANAPPLICATIONID,
                    isReleased = x.Mapping.ISRELEASED,
                    releaseApprovalStatusId = (short)x.Mapping.RELEASEAPPROVALSTATUSID,
                    customerCode = x.CollateralApplication.CustomerCollateral.Customer.CUSTOMERCODE,
                    firstName = x.CollateralApplication.CustomerCollateral.Customer.FIRSTNAME,
                    middleName = x.CollateralApplication.CustomerCollateral.Customer.MIDDLENAME,
                    lastName = x.CollateralApplication.CustomerCollateral.Customer.LASTNAME,
                    collateralCode = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                    collateralValue = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                    allowSharing = x.Mapping.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                    isLocationBased = x.Mapping.TBL_COLLATERAL_CUSTOMER.ISLOCATIONBASED,
                    valuationCycle = x.Mapping.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                    hairCut = x.Mapping.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                    collateralTypeId = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                    applicationReferenceNumber = x.CollateralApplication.Application.APPLICATIONREFERENCENUMBER,
                    applicationDate = x.CollateralApplication.Application.APPLICATIONDATE,
                    interestRate = x.CollateralApplication.Application.INTERESTRATE,
                    loanInformation = x.CollateralApplication.Application.LOANINFORMATION,
                })
                .Where(x => x.isReleased == false)
                .Distinct();

            return collaterals;
        }

        public bool ReleaseCollateral(int collateralMappingId, int staffId, GeneralEntity model)
        {
            var mapping = context.TBL_LOAN_COLLATERAL_MAPPING.Find(collateralMappingId);
            context.Entry(mapping).State = EntityState.Modified;
            mapping.RELEASEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Release Action '{ mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------
            
            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralMappingId;
            workflow.Comment = "Request for collateral release";
            workflow.OperationId = (int)OperationsEnum.CollateralRelease;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }

        public bool ApproveCollateralRelease(ApprovalViewModel entity, int staffId, GeneralEntity model)
        {
            var mapping = context.TBL_LOAN_COLLATERAL_MAPPING.Find(entity.targetId);
            context.Entry(mapping).State = EntityState.Modified;
            mapping.RELEASEAPPROVALSTATUSID = (short)entity.approvalStatusId;
            mapping.ISRELEASED = entity.approvalStatusId == (int)ApprovalStatusEnum.Approved ? true : false;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseApproval,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Release Approval '{ mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (short)entity.approvalStatusId;
            workflow.TargetId = entity.targetId;
            workflow.Comment = entity.comment;
            workflow.OperationId = (int)OperationsEnum.CollateralRelease;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }

        public IEnumerable<ActiveCustomerCollateralViewModel> GetPendingCustomerCollateralRelease()
        {
            return context.TBL_CUSTOMER//.Where(x => x.CustomerId == customerId)
                .Join(context.TBL_COLLATERAL_CUSTOMER, c => c.CUSTOMERID, o => o.CUSTOMERID, (c, o) => new { Customer = c, Collateral = o })
                .Join(context.TBL_LOAN_APPLICATION, cc => cc.Collateral.CUSTOMERID, a => a.CUSTOMERID, (cc, a) => new { CustomerCollateral = cc, Application = a })
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING, ca => ca.Application.LOANAPPLICATIONID, m => m.LOANAPPLICATIONID, (ca, m) => new { CollateralApplication = ca, Mapping = m })
                .Select(x => new ActiveCustomerCollateralViewModel
                {
                    customerId = x.CollateralApplication.Application.CUSTOMERID,
                    collateralCustomerId = x.Mapping.COLLATERALCUSTOMERID,
                    loanTypeId = x.CollateralApplication.Application.LOANTYPEID,
                    loanCollateralMappingId = x.Mapping.LOANCOLLATERALMAPPINGID,
                    loanApplicationId = x.Mapping.LOANAPPLICATIONID,
                    isReleased = x.Mapping.ISRELEASED,
                    releaseApprovalStatusId = (short)x.Mapping.RELEASEAPPROVALSTATUSID,
                    customerCode = x.CollateralApplication.CustomerCollateral.Customer.CUSTOMERCODE,
                    firstName = x.CollateralApplication.CustomerCollateral.Customer.FIRSTNAME,
                    middleName = x.CollateralApplication.CustomerCollateral.Customer.MIDDLENAME,
                    lastName = x.CollateralApplication.CustomerCollateral.Customer.LASTNAME,
                    collateralCode = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                    collateralValue = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                    allowSharing = x.Mapping.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                    isLocationBased = x.Mapping.TBL_COLLATERAL_CUSTOMER.ISLOCATIONBASED,
                    valuationCycle = x.Mapping.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                    hairCut = x.Mapping.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                    collateralTypeId = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                    applicationReferenceNumber = x.CollateralApplication.Application.APPLICATIONREFERENCENUMBER,
                    applicationDate = x.CollateralApplication.Application.APPLICATIONDATE,
                    interestRate = x.CollateralApplication.Application.INTERESTRATE,
                    loanInformation = x.CollateralApplication.Application.LOANINFORMATION,
                })
                .Where(x => x.isReleased == false && x.releaseApprovalStatusId == (int)ApprovalStatusEnum.Processing)
                .Distinct();
        }

        public IQueryable<CollateralSearchViewModel> SearchCollateral(string searchQuery, int companyId)
        {
            IQueryable<CollateralSearchViewModel> result = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                result =
                    context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.COMPANYID == companyId)
                    .Select(o => new CollateralSearchViewModel
                    {
                        collateralId = o.COLLATERALCUSTOMERID,
                        customerId = o.CUSTOMERID,
                        collateralTypeId = o.COLLATERALSUBTYPEID,
                        collateralTypeName = o.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        customerCode = o.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = o.TBL_CUSTOMER.FIRSTNAME + " " + o.TBL_CUSTOMER.MIDDLENAME + " " + o.TBL_CUSTOMER.LASTNAME,
                        currencyId = o.CURRENCYID,
                        currencyCode = o.TBL_CURRENCY.CURRENCYCODE,
                        collateralCode = o.COLLATERALCODE,
                        allowSharing = o.ALLOWSHARING,
                        isLocationBased = o.ISLOCATIONBASED,
                        valuationCycle = o.VALUATIONCYCLE,
                        haircut = o.HAIRCUT,
                    })
                    .Where(x =>
                       x.collateralCode.ToLower().Contains(searchQuery)
                    || x.collateralTypeName.ToLower().Contains(searchQuery)
                    || x.customerCode.ToLower().Contains(searchQuery)
                    || x.currencyCode.Contains(searchQuery)
                    || x.customerName.Contains(searchQuery)
                    )
                    .Take(12);
            }

            return result;
        }


        public bool AssignCollateral(ActiveCustomerCollateralViewModel model)
        {
            var assignment = new TBL_LOAN_COLLATERAL_MAPPING
            {
                LOANAPPLICATIONID = model.loanApplicationId,
                COLLATERALCUSTOMERID = model.collateralCustomerId,
                RELEASEAPPROVALSTATUSID = 0
            };

            context.TBL_LOAN_COLLATERAL_MAPPING.Add(assignment);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralAssignmentAction,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Assignment :: LoanApplicationId:'{ assignment.LOANAPPLICATIONID }' CollateralCustomerId:'{ assignment.COLLATERALCUSTOMERID }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;
        }




        #region Collateral Customer 

        //public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomer(int customerId, int companyId)
        //{
        //    var collateral = GetCollateralCustomerByCustomerId(customerId, companyId).Where(x => x.deleted == false);

        //    return collateral;
        //}

        //public async Task<bool> AddCollateralCustomer(CollateralCustomerViewModel entity)
        //{
        //    var collateral = new tbl_Collateral_Customer
        //    {
        //        CompanyId = entity.companyId,
        //        CollateralTypeId = entity.collateralTypeId,
        //        CollateralCode = entity.collateralCode,
        //        CurrencyId = entity.currencyId,
        //        AllowSharing = entity.allowSharing,
        //        IsLocationBased = entity.isLocationBased,
        //        ValuationCycle = entity.valuationCycle,
        //        HairCut = entity.hairCut,
        //        CustomerId = entity.customerId,

        //        ApprovalStatus = entity.approvalStatus,
        //        DateActedOn = entity.dateActedOn,
        //        ActedOnBy = entity.actedOnBy,
        //        CamRefNumber = entity.camRefNumber,
        //        DateTimeCreated = genSetup.GetApplicationDate().Date,
        //        CreatedBy = entity.createdBy,
        //        tbl_Collateral_Immovable_Property = AddCollateralProperty((CollateralTypeEnum)entity.collateralTypeId, entity.collateralProperty),
        //        tbl_Collateral_Deposit = AddCollateralDeposit((CollateralTypeEnum)entity.collateralTypeId, entity.collateralDeposit),
        //        tbl_Collateral_Plant_And_Equipment = AddCollateralMachineDetail((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMachineDetail),
        //        tbl_Collateral_Marketable_Security = AddCollateralMarketableSecurity((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMarketableSecurity),
        //        tbl_Collateral_Policy = AddCollateralInsurancePolicy((CollateralTypeEnum)entity.collateralTypeId, entity.collateralInsurancePolicy),
        //        tbl_Collateral_PreciousMetal = AddCollateralPreciousMetal((CollateralTypeEnum)entity.collateralTypeId, entity.collateralPreciousMetal),
        //        tbl_Collateral_Gaurantee = AddCollateralGaurantee((CollateralTypeEnum)entity.collateralTypeId, entity.collateralGaurantee),
        //        tbl_Collateral_Vehicle = AddCollateralVehicle((CollateralTypeEnum)entity.collateralTypeId, entity.collateralVehicle),
        //        tbl_Collateral_Miscellaneous = AddCollateralMiscellaneous((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMiscellaneous),
        //    };

        //    context.tbl_Collateral_Customer.Add(collateral);
        //    return await context.SaveChangesAsync() != 0;
        //}

        public async Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(colleralCustomerId);
            collateral.DELETED = true;
            collateral.DELETEDBY = user.staffId;
            collateral.DATETIMEDELETED = genSetup.GetApplicationDate();

            return await context.SaveChangesAsync() != 0;
        }

        //private List<CollateralCustomerViewModel> CollateralCustomer(int customerId, int companyId)
        //{
        //    tbl_Collateral_Type_Sub sub = new tbl_Collateral_Type_Sub();
        //    return (from c in context.tbl_Collateral_Customer
        //            join t in context.tbl_Collateral_Type on c.CollateralTypeId equals t.CollateralTypeId
        //            where c.Deleted == false && c.CompanyId == companyId && c.CustomerId == customerId
        //            select new CollateralCustomerViewModel
        //            {
        //                collateralTypeId = c.CollateralTypeId,
        //                collateralType = c.tbl_Collateral_Type.CollateralTypeName,
        //                collateralCustomerId = c.CollateralCustomerId,
        //                collateralCode = c.CollateralCode,
        //                currencyId = c.CurrencyId,
        //                currency = c.tbl_Currency.CurrencyName,
        //                allowSharing = c.AllowSharing,
        //                isLocationBased = c.IsLocationBased,
        //                valuationCycle = c.ValuationCycle,
        //                hairCut = c.HairCut,
        //                customerId = c.CustomerId,
        //                customerName = c.tbl_Customer.LastName + " " + c.tbl_Customer.FirstName,
        //                approvalStatus = c.ApprovalStatus,
        //                dateActedOn = c.DateActedOn,
        //                actedOnBy = c.ActedOnBy,
        //                camRefNumber = c.CamRefNumber,
        //                dateTimeCreated = c.DateTimeCreated,
        //                createdBy = c.CreatedBy,
        //            })
        //            .OrderByDescending(x => x.collateralCustomerId)
        //            .ToList();
        //}

        //public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomerByCustomerId(int customerId, int companyId)
        //{
        //    return CollateralCustomer(customerId, companyId);
        //}

        //public async Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralCustomerViewModel entity)
        //{
        //    var collateral = context.tbl_Collateral_Customer.Find(collateralCustomerId);
        //    collateral.CollateralCode = entity.collateralCode;
        //    collateral.CurrencyId = entity.currencyId;
        //    collateral.AllowSharing = entity.allowSharing;
        //    collateral.IsLocationBased = entity.isLocationBased;
        //    collateral.ValuationCycle = entity.valuationCycle;
        //    collateral.HairCut = entity.hairCut;
        //    collateral.CustomerId = entity.customerId;
        //    collateral.ApprovalStatus = entity.approvalStatus;
        //    collateral.DateActedOn = entity.dateActedOn;
        //    collateral.ActedOnBy = entity.actedOnBy;
        //    collateral.CamRefNumber = entity.camRefNumber;
        //    collateral.DateTimeUpdated = entity.dateTimeCreated;
        //    collateral.LastUpdatedBy = entity.lastUpdatedBy;

        //    //TblCollateralMachineDetail collateralMachineDetail = collateral.TblCollateralMachineDetail.FirstOrDefault();

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Property)
        //    {
        //        var collateralProperty = context.tbl_Collateral_Immovable_Property.Find(entity.collateralProperty.collateralPropertyId);

        //        collateralProperty.PropertyName = entity.collateralProperty.propertyName;
        //        collateralProperty.CityId = entity.collateralProperty.cityId;
        //        collateralProperty.CountryId = entity.collateralProperty.countryId;
        //        collateralProperty.PropertyAddress = entity.collateralProperty.propertyAddress;
        //        collateralProperty.ConstructionDate = entity.collateralProperty.constructionDate;
        //        collateralProperty.DateOfAcquisition = entity.collateralProperty.dateOfAcquisition;
        //        collateralProperty.LastValuationDate = entity.collateralProperty.lastValuationDate;
        //        collateralProperty.ValuerId = entity.collateralProperty.valuerId;
        //        collateralProperty.ValuerReferenceNumber = entity.collateralProperty.valuerReferenceNumber;
        //        collateralProperty.OpenMarketValue = entity.collateralProperty.openMarketValue;
        //        collateralProperty.CollateralValue = entity.collateralProperty.collateralValue;
        //        collateralProperty.ForcedSaleValue = entity.collateralProperty.forcedSaleValue;
        //        collateralProperty.StampToCover = entity.collateralProperty.stampToCover;
        //        collateralProperty.ValuationSource = entity.collateralProperty.valuationSource;
        //        collateralProperty.OriginalValue = entity.collateralProperty.originalValue;
        //        collateralProperty.AvailableValue = entity.collateralProperty.availableValue;
        //        collateralProperty.SecurityValue = entity.collateralProperty.securityValue;
        //        collateralProperty.CollateralUsableAmount = entity.collateralProperty.collateralUsableAmount;
        //        collateralProperty.PropertyValueBaseTypeId = entity.collateralProperty.propertyValueBaseTypeId;
        //        collateralProperty.Remark = entity.collateralProperty.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities)
        //    {
        //        var collateralMarketableSecurity = context.tbl_Collateral_Marketable_Security.Find(entity.collateralMarketableSecurity.collateralMarketableSecurityId);

        //        collateralMarketableSecurity.SecurityType = entity.collateralMarketableSecurity.securityType;
        //        collateralMarketableSecurity.DealReferenceNumber = entity.collateralMarketableSecurity.dealReferenceNumber;
        //        collateralMarketableSecurity.EffectiveDate = entity.collateralMarketableSecurity.effectiveDate;
        //        collateralMarketableSecurity.MaturityDate = entity.collateralMarketableSecurity.maturityDate;
        //        collateralMarketableSecurity.DealAmount = entity.collateralMarketableSecurity.dealAmount;
        //        collateralMarketableSecurity.SecurityValue = entity.collateralMarketableSecurity.securityValue;
        //        collateralMarketableSecurity.LienUsableAmount = entity.collateralMarketableSecurity.lienUsableAmount;
        //        collateralMarketableSecurity.Rating = entity.collateralMarketableSecurity.rating;
        //        collateralMarketableSecurity.PercentageInterest = entity.collateralMarketableSecurity.percentageInterest;
        //        collateralMarketableSecurity.InterestPaymentFrequency = entity.collateralMarketableSecurity.interestPaymentFrequency;
        //        collateralMarketableSecurity.IssuerName = entity.collateralMarketableSecurity.issuerName;
        //        collateralMarketableSecurity.IssuerReferenceNumber = entity.collateralMarketableSecurity.issuerReferenceNumber;
        //        collateralMarketableSecurity.UnitValue = entity.collateralMarketableSecurity.unitValue;
        //        collateralMarketableSecurity.NumberOfUnits = entity.collateralMarketableSecurity.numberOfUnits;
        //        collateralMarketableSecurity.Remark = entity.collateralMarketableSecurity.remark;
        //    }
        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.TermDeposit)
        //    {
        //        var collateralDeposit = context.tbl_Collateral_Deposit.Find(entity.collateralDeposit.collateralDepositId);

        //        collateralDeposit.AccountNumber = entity.collateralDeposit.accountNumber;
        //        collateralDeposit.DealReferenceNumber = entity.collateralDeposit.dealReferenceNumber;
        //        //collateralDeposit.ExistingLienAmount = entity.collateralDeposit.existingLienAmount;
        //        collateralDeposit.LienAmount = entity.collateralDeposit.lienAmount;
        //        collateralDeposit.AvailableBalance = entity.collateralDeposit.availableBalance;
        //        collateralDeposit.SecurityValue = entity.collateralDeposit.securityValue;
        //        collateralDeposit.MaturityDate = entity.collateralDeposit.maturityDate;
        //        collateralDeposit.MaturityAmount = entity.collateralDeposit.maturityAmount;
        //        collateralDeposit.Remark = entity.collateralDeposit.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.CASA)
        //    {
        //        var collateralCasa = context.tbl_Collateral_Casa.Find(entity.collateralCasa.collateralCasaId);

        //        collateralCasa.AccountNumber = entity.collateralCasa.accountNumber;
        //        collateralCasa.IsOwnedByCustomer = entity.collateralCasa.isOwnedByCustomer;
        //        collateralCasa.AvailableBalance = entity.collateralCasa.availableBalance;
        //        collateralCasa.ExistingLienAmount = entity.collateralCasa.existingLienAmount;
        //        collateralCasa.LienAmount = entity.collateralCasa.lienAmount;
        //        collateralCasa.SecurityValue = entity.collateralCasa.securityValue;
        //        collateralCasa.Remark = entity.collateralCasa.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
        //    {
        //        var collateralMachineDetail = context.tbl_Collateral_Plant_And_Equipment.Find(entity.collateralMachineDetail.collateralMachineDetailId);

        //        collateralMachineDetail.MachineName = entity.collateralMachineDetail.machineName;
        //        collateralMachineDetail.Description = entity.collateralMachineDetail.description;
        //        collateralMachineDetail.MachineNumber = entity.collateralMachineDetail.machineNumber;
        //        collateralMachineDetail.ManufacturerName = entity.collateralMachineDetail.manufacturerName;
        //        collateralMachineDetail.YearOfManufacture = entity.collateralMachineDetail.yearOfManufacture;
        //        collateralMachineDetail.YearOfPurchase = entity.collateralMachineDetail.yearOfManufacture;
        //        collateralMachineDetail.ValueBaseTypeId = entity.collateralMachineDetail.valueBaseTypeId;
        //        collateralMachineDetail.MachineCondition = entity.collateralMachineDetail.machineCondition;
        //        collateralMachineDetail.MachineryLocation = entity.collateralMachineDetail.machineryLocation;
        //        collateralMachineDetail.EquipmentSize = entity.collateralMachineDetail.equipmentSize;
        //        collateralMachineDetail.ReplacementValue = entity.collateralMachineDetail.replacementValue;
        //        collateralMachineDetail.IntendedUse = entity.collateralMachineDetail.intendedUse;

        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.PreciousMetal)
        //    {
        //        var collateralPreciousMetal = context.tbl_Collateral_PreciousMetal.Find(entity.collateralPreciousMetal.collateralPreciousMetalId);

        //        collateralPreciousMetal.CollateralCustomerId = entity.collateralPreciousMetal.collateralCustomerId;
        //        collateralPreciousMetal.IsOwnedByCustomer = entity.collateralPreciousMetal.isOwnedByCustomer;
        //        collateralPreciousMetal.PreciousMetalName = entity.collateralPreciousMetal.preciousMetalName;
        //        collateralPreciousMetal.WeightInGrammes = entity.collateralPreciousMetal.weightInGrammes;
        //        collateralPreciousMetal.ValuationAmount = entity.collateralPreciousMetal.valuationAmount;
        //        collateralPreciousMetal.UnitRate = entity.collateralPreciousMetal.unitRate;
        //        collateralPreciousMetal.PreciousMetalForm = entity.collateralPreciousMetal.preciousMetalForm;
        //        collateralPreciousMetal.Remark = entity.collateralPreciousMetal.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.InsurancePolicy)
        //    {
        //        tbl_Collateral_Policy collateralInsurancePolicy = collateral.tbl_Collateral_Policy.Where(x => x.CollateralInsurancePolicyId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralInsurancePolicy.PremiumAmount = entity.collateralInsurancePolicy.premiumAmount;
        //        collateralInsurancePolicy.IsOwnedByCustomer = entity.collateralInsurancePolicy.isOwnedByCustomer;
        //        collateralInsurancePolicy.InsurancePolicyNumber = entity.collateralInsurancePolicy.insurancePolicyNumber;
        //        collateralInsurancePolicy.PolicyAmount = entity.collateralInsurancePolicy.policyAmount;
        //        collateralInsurancePolicy.InsuranceCompanyName = entity.collateralInsurancePolicy.insuranceCompanyName;
        //        collateralInsurancePolicy.PolicyStartDate = entity.collateralInsurancePolicy.policyStartDate;
        //        collateralInsurancePolicy.AssignDate = entity.collateralInsurancePolicy.assignDate;
        //        collateralInsurancePolicy.PolicyRenewalDate = entity.collateralInsurancePolicy.policyRenewalDate;
        //        collateralInsurancePolicy.InsurerAddress = entity.collateralInsurancePolicy.insurerAddress;
        //        collateralInsurancePolicy.InsurerDetails = entity.collateralInsurancePolicy.insurerDetails;
        //        collateralInsurancePolicy.RenewalFrequencyTypeId = entity.collateralInsurancePolicy.renewalFrequencyTypeId;
        //        collateralInsurancePolicy.Remark = entity.collateralInsurancePolicy.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Gaurantee)
        //    {
        //        tbl_Collateral_Gaurantee collateralGaurantee = collateral.tbl_Collateral_Gaurantee.Where(x => x.CollateralGauranteeId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralGaurantee.IsOwnedByCustomer = entity.collateralGaurantee.isOwnedByCustomer;
        //        collateralGaurantee.InstitutionName = entity.collateralGaurantee.institutionName;
        //        collateralGaurantee.GuarantorReferenceNumber = entity.collateralGaurantee.guarantorReferenceNumber;
        //        collateralGaurantee.GuaranteeValue = entity.collateralGaurantee.guaranteeValue;
        //        collateralGaurantee.StartDate = entity.collateralGaurantee.startDate;
        //        collateralGaurantee.EndDate = entity.collateralGaurantee.endDate;
        //        collateralGaurantee.GuarantorAddress = entity.collateralGaurantee.guarantorAddress;
        //        collateralGaurantee.Remark = entity.collateralGaurantee.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Vehicle)
        //    {
        //        tbl_Collateral_Vehicle collateralVehicle = collateral.tbl_Collateral_Vehicle.Where(x => x.CollateralVehicleId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralVehicle.VehicleType = entity.collateralVehicle.vehicleType;
        //        collateralVehicle.VehicleStatus = entity.collateralVehicle.vehicleStatus;
        //        collateralVehicle.VehicleMake = entity.collateralVehicle.vehicleMake;
        //        collateralVehicle.ModelName = entity.collateralVehicle.modelName;
        //        collateralVehicle.ManufacturedDate = entity.collateralVehicle.manufacturedDate;
        //        collateralVehicle.SerialNumber = entity.collateralVehicle.serialNumber;
        //        collateralVehicle.NameOfOwner = entity.collateralVehicle.nameOfOwner;
        //        collateralVehicle.RegistrationCompany = entity.collateralVehicle.registrationCompany;
        //        collateralVehicle.LastValuationAmount = entity.collateralVehicle.lastValuationAmount;
        //        collateralVehicle.RegistrationNumber = entity.collateralVehicle.registrationNumber;
        //        collateralVehicle.ChasisNumber = entity.collateralVehicle.chasisNumber;
        //        collateralVehicle.EngineNumber = entity.collateralVehicle.engineNumber;
        //        collateralVehicle.ResaleValue = entity.collateralVehicle.resaleValue;
        //        collateralVehicle.ValuationDate = entity.collateralVehicle.valuationDate;
        //        collateralVehicle.InvoiceValue = entity.collateralVehicle.invoiceValue;
        //        collateralVehicle.Remark = entity.collateralVehicle.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Miscellaneous)
        //    {
        //        tbl_Collateral_Miscellaneous collateralMiscellaneous = collateral.tbl_Collateral_Miscellaneous.Where(x => x.CollateralMiscellaneousId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
        //        collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
        //        //if (entity.collateralMiscellaneous.collateralMiscellaneousNotes != null)
        //        //{
        //        //    tbl_Collateral_Miscellaneous_Notes collateralMiscellaneousNote = context.tbl_Collateral_Miscellaneous_Notes.Where(x => x.MiscellaneousId == entity.collateralMiscellaneous.collateralMiscellaneousId)
        //        //    .FirstOrDefault();

        //        //    collateralMiscellaneousNote.ColumnName = entity.collateralMiscellaneous.collateralMiscellaneousNotes.;
        //        //    collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
        //        //    collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
        //        //    collateralMiscellaneous.Note = entity.collateralMiscellaneous.note;
        //        //}
        //    }

        //    //if (entity.collateralCustomerPolicy != null)
        //    //{
        //    //    tbl_Collateral_Item_Policy collateralCustomerPolicy = collateral.tbl_Collateral_Customer_Policy.Where(x => x.PolicyId == entity.collateralCustomerPolicy.policyId)
        //    //        .FirstOrDefault();

        //    //    collateralCustomerPolicy.PolicyReferenceNumber = entity.collateralCustomerPolicy.policyReferenceNumber;
        //    //    collateralCustomerPolicy.InsuranceCompanyName = entity.collateralCustomerPolicy.insuranceCompanyName;
        //    //    collateralCustomerPolicy.StartDate = entity.collateralCustomerPolicy.startDate;
        //    //    collateralCustomerPolicy.EndDate = entity.collateralCustomerPolicy.endDate;
        //    //}

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
        //        StaffId = (int)entity.lastUpdatedBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Update collateral with code: { entity.collateralCode} of { entity.valuationCycle} valuation cycle",
        //        //Ipaddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);


        //    return await context.SaveChangesAsync() != 0;
        //}

        public bool IsCollateralDocExists(string docName)
        {
            return false;
            //return context.TblCollateralCustomer.Any(c => string.Equals(c.DocumentNo, docName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion Collateral Customer

        #region Property
        private ICollection<TBL_COLLATERAL_IMMOVABLE_PROPERTY> AddCollateralProperty(CollateralTypeEnum collateralType, CollateralPropertyViewModel entity)
        {
            ICollection<TBL_COLLATERAL_IMMOVABLE_PROPERTY> collateral;

            if (collateralType != CollateralTypeEnum.Property)
                return null;

            collateral = new List<TBL_COLLATERAL_IMMOVABLE_PROPERTY>();

            collateral.Add(new TBL_COLLATERAL_IMMOVABLE_PROPERTY
            {
                //CollateralPropertyId = entity.collateralPropertyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                PROPERTYNAME = entity.propertyName,
                CITYID = entity.cityId,
                COUNTRYID = entity.countryId,
                PROPERTYADDRESS = entity.propertyAddress,
                CONSTRUCTIONDATE = entity.constructionDate,
                PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId,
                DATEOFACQUISITION = entity.dateOfAcquisition,
                LASTVALUATIONDATE = entity.lastValuationDate,
                VALUERID = entity.valuerId,
                VALUERREFERENCENUMBER = entity.valuerReferenceNumber,
                OPENMARKETVALUE = entity.openMarketValue,
                COLLATERALVALUE = entity.collateralValue,
                FORCEDSALEVALUE = entity.forcedSaleValue,
                STAMPTOCOVER = entity.stampToCover,
                VALUATIONSOURCE = entity.valuationSource,
                ORIGINALVALUE = entity.originalValue,
                AVAILABLEVALUE = entity.availableValue,
                SECURITYVALUE = entity.securityValue,
                COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount,
                REMARK = entity.remark
            });

            return collateral;
        }

        private CollateralPropertyViewModel CollateralProperty(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_IMMOVABLE_PROPERTY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where c.DELETED == false && m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPropertyViewModel
                    {
                        collateralPropertyId = m.COLLATERALPROPERTYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        propertyName = m.PROPERTYNAME,
                        cityId = m.CITYID,
                        countryId = m.COUNTRYID,
                        propertyAddress = m.PROPERTYADDRESS,
                        constructionDate = m.CONSTRUCTIONDATE,
                        propertyValueBaseTypeId = m.PROPERTYVALUEBASETYPEID,
                        dateOfAcquisition = m.DATEOFACQUISITION,
                        lastValuationDate = m.LASTVALUATIONDATE,
                        valuerId = m.VALUERID,
                        valuerReferenceNumber = m.VALUERREFERENCENUMBER,
                        openMarketValue = m.OPENMARKETVALUE,
                        collateralValue = m.COLLATERALVALUE,
                        forcedSaleValue = m.FORCEDSALEVALUE,
                        stampToCover = m.STAMPTOCOVER,
                        valuationSource = m.VALUATIONSOURCE,
                        originalValue = m.ORIGINALVALUE,
                        availableValue = m.AVAILABLEVALUE,
                        collateralUsableAmount = m.COLLATERALUSABLEAMOUNT,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        private CollateralPropertyViewModel GetCollateralPropertyByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralProperty(CollateralCustomerId);
        }
        #endregion Property

        #region Deposit
        private ICollection<TBL_COLLATERAL_DEPOSIT> AddCollateralDeposit(CollateralTypeEnum collateralType, CollateralDepositViewModel entity)
        {
            ICollection<TBL_COLLATERAL_DEPOSIT> collateral;

            if (collateralType != CollateralTypeEnum.TermDeposit)
                return null;

            collateral = new List<TBL_COLLATERAL_DEPOSIT>();

            collateral.Add(new TBL_COLLATERAL_DEPOSIT
            {
                //CollateralDepositId = entity.collateralDepositId,
                //CollateralCustomerId = entity.collateralCustomerId,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                ACCOUNTNUMBER = entity.accountNumber,
                //ExistingLienAmount = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                AVAILABLEBALANCE = entity.availableBalance,
                SECURITYVALUE = entity.securityValue,
                MATURITYDATE = entity.maturityDate,
                MATURITYAMOUNT = entity.maturityAmount,
                REMARK = entity.remark
            });

            return collateral;
        }

        private CollateralDepositViewModel CollateralDeposit(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_DEPOSIT
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralDepositViewModel
                    {
                        collateralDepositId = m.COLLATERALDEPOSITID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        dealReferenceNumber = m.DEALREFERENCENUMBER,
                        accountNumber = m.ACCOUNTNUMBER,
                        //existingLienAmount = m.ExistingLienAmount,
                        lienAmount = m.LIENAMOUNT,
                        availableBalance = m.AVAILABLEBALANCE,
                        securityValue = m.SECURITYVALUE,
                        maturityDate = m.MATURITYDATE,
                        maturityAmount = m.MATURITYAMOUNT,
                        remark = m.REMARK

                    }).FirstOrDefault();
        }

        private CollateralDepositViewModel GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralDeposit(CollateralCustomerId);
        }
        #endregion Deposit

        #region End od CASA
        private ICollection<TBL_COLLATERAL_CASA> AddCollateralCasa(CollateralTypeEnum collateralType, CollateralCasaViewModel entity)
        {
            ICollection<TBL_COLLATERAL_CASA> collateral;

            if (collateralType != CollateralTypeEnum.CASA)
                return null;

            collateral = new List<TBL_COLLATERAL_CASA>();

            collateral.Add(new TBL_COLLATERAL_CASA
            {
                ACCOUNTNUMBER = entity.accountNumber,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                AVAILABLEBALANCE = entity.availableBalance,
                EXISTINGLIENAMOUNT = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                SECURITYVALUE = entity.securityValue,
                REMARK = entity.remark
            });

            return collateral;
        }

        private CollateralCasaViewModel CollateralCasa(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_CASA
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralCasaViewModel
                    {
                        collateralCasaId = m.COLLATERALCASAID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        accountNumber = m.ACCOUNTNUMBER,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        availableBalance = m.AVAILABLEBALANCE,
                        existingLienAmount = m.EXISTINGLIENAMOUNT,
                        lienAmount = m.LIENAMOUNT,
                        securityValue = m.SECURITYVALUE,
                        remark = m.REMARK

                    }).FirstOrDefault();
        }

        private CollateralCasaViewModel GetCollateralCasaByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralCasa(CollateralCustomerId);
        }
        #endregion End of CASA

        #region Plants and Equipment
        private ICollection<TBL_COLLATERAL_PLANT_AND_EQUIPMENT> AddCollateralMachineDetail(CollateralTypeEnum collateralType, CollateralPlantsAndEquipmentViewModel entity)
        {
            ICollection<TBL_COLLATERAL_PLANT_AND_EQUIPMENT> collateral;

            if (collateralType != CollateralTypeEnum.PlantAndMachinery)
                return null;

            collateral = new List<TBL_COLLATERAL_PLANT_AND_EQUIPMENT>();

            collateral.Add(new TBL_COLLATERAL_PLANT_AND_EQUIPMENT
            {
                MACHINENAME = entity.machineName,
                DESCRIPTION = entity.description,
                MACHINENUMBER = entity.machineNumber,
                MANUFACTURERNAME = entity.manufacturerName,
                YEAROFMANUFACTURE = entity.yearOfManufacture,
                YEAROFPURCHASE = entity.yearOfManufacture,
                VALUEBASETYPEID = entity.valueBaseTypeId,
                MACHINECONDITION = entity.machineCondition,
                MACHINERYLOCATION = entity.machineryLocation,
                REPLACEMENTVALUE = entity.replacementValue,
                EQUIPMENTSIZE = entity.equipmentSize,
                INTENDEDUSE = entity.intendedUse
            });

            return collateral;
        }

        private CollateralPlantsAndEquipmentViewModel CollateralMachineDetail(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_PLANT_AND_EQUIPMENT
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPlantsAndEquipmentViewModel
                    {
                        collateralMachineDetailId = m.COLLATERALMACHINEDETAILID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        machineName = m.MACHINENAME,
                        description = m.DESCRIPTION,
                        machineNumber = m.MACHINENUMBER,
                        manufacturerName = m.MANUFACTURERNAME,
                        yearOfManufacture = m.YEAROFMANUFACTURE,
                        yearOfPurchase = m.YEAROFPURCHASE,
                        valueBaseTypeId = m.VALUEBASETYPEID,
                        machineCondition = m.MACHINECONDITION,
                        machineryLocation = m.MACHINERYLOCATION,
                        replacementValue = m.REPLACEMENTVALUE,
                        equipmentSize = m.EQUIPMENTSIZE,
                        intendedUse = m.INTENDEDUSE
                    }).FirstOrDefault();
        }

        private CollateralPlantsAndEquipmentViewModel GetCollateralMachineDetailByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMachineDetail(collateralCustomerId);
        }
        #endregion Plants and Equipment

        #region Marketable Security
        private ICollection<TBL_COLLATERAL_MARKETABLE_SECURITY> AddCollateralMarketableSecurity(CollateralTypeEnum collateralType, CollateralMarketableSecurityViewModel entity)
        {
            ICollection<TBL_COLLATERAL_MARKETABLE_SECURITY> collateral;

            if (collateralType != CollateralTypeEnum.MarketableSecurities)
                return null;

            collateral = new List<TBL_COLLATERAL_MARKETABLE_SECURITY>();

            collateral.Add(new TBL_COLLATERAL_MARKETABLE_SECURITY
            {
                SECURITYTYPE = entity.securityType,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                EFFECTIVEDATE = entity.effectiveDate,
                MATURITYDATE = entity.maturityDate,
                DEALAMOUNT = entity.dealAmount,
                SECURITYVALUE = entity.securityValue,
                LIENUSABLEAMOUNT = entity.lienUsableAmount,
                ISSUERNAME = entity.issuerName,
                ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                UNITVALUE = entity.unitValue,
                NUMBEROFUNITS = entity.numberOfUnits,
                RATING = entity.rating,
                PERCENTAGEINTEREST = entity.percentageInterest,
                INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralMarketableSecurityViewModel CollateralMarketableSecurity(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_MARKETABLE_SECURITY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralMarketableSecurityViewModel
                    {
                        collateralMarketableSecurityId = m.COLLATERALMARKETABLESECURITYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        securityType = m.SECURITYTYPE,
                        dealReferenceNumber = m.DEALREFERENCENUMBER,
                        effectiveDate = m.EFFECTIVEDATE,
                        maturityDate = m.MATURITYDATE,
                        dealAmount = m.DEALAMOUNT,
                        securityValue = m.SECURITYVALUE,
                        lienUsableAmount = m.LIENUSABLEAMOUNT,
                        rating = m.RATING,
                        percentageInterest = m.PERCENTAGEINTEREST,
                        interestPaymentFrequency = m.INTERESTPAYMENTFREQUENCY,
                        issuerName = m.ISSUERNAME,
                        issuerReferenceNumber = m.ISSUERREFERENCENUMBER,
                        unitValue = m.UNITVALUE,
                        numberOfUnits = m.NUMBEROFUNITS,
                        remark = m.REMARK



                    }).FirstOrDefault();
        }

        private CollateralMarketableSecurityViewModel GetCollateralMarketableSecurityByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMarketableSecurity(collateralCustomerId);
        }

        #endregion Marketable Security

        #region Precious Metal
        private ICollection<TBL_COLLATERAL_PRECIOUSMETAL> AddCollateralPreciousMetal(CollateralTypeEnum collateralType, CollateralPreciousMetalViewModel entity)
        {
            ICollection<TBL_COLLATERAL_PRECIOUSMETAL> collateral;

            if (collateralType != CollateralTypeEnum.PreciousMetal)
                return null;

            collateral = new List<TBL_COLLATERAL_PRECIOUSMETAL>();

            collateral.Add(new TBL_COLLATERAL_PRECIOUSMETAL
            {
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                PRECIOUSMETALNAME = entity.preciousMetalName,
                WEIGHTINGRAMMES = entity.weightInGrammes,
                VALUATIONAMOUNT = entity.valuationAmount,
                UNITRATE = entity.unitRate,
                PRECIOUSMETALFORM = entity.preciousMetalForm,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralPreciousMetalViewModel CollateralPreciousMetal(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_PRECIOUSMETAL
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPreciousMetalViewModel
                    {
                        collateralPreciousMetalId = m.COLLATERALPRECIOUSMETALID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        preciousMetalName = m.PRECIOUSMETALNAME,
                        weightInGrammes = m.WEIGHTINGRAMMES,
                        valuationAmount = m.VALUATIONAMOUNT,
                        unitRate = m.UNITRATE,
                        preciousMetalForm = m.PRECIOUSMETALFORM,
                        remark = m.REMARK

                    }).FirstOrDefault();
        }

        private CollateralPreciousMetalViewModel GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralPreciousMetal(collateralCustomerId);
        }
        #endregion Precious Metal

        #region Insurance Policy
        private ICollection<TBL_COLLATERAL_POLICY> AddCollateralInsurancePolicy(CollateralTypeEnum collateralType, CollateralInsurancePolicyViewModel entity)
        {
            ICollection<TBL_COLLATERAL_POLICY> collateral;

            if (collateralType != CollateralTypeEnum.InsurancePolicy)
                return null;

            collateral = new List<TBL_COLLATERAL_POLICY>();

            collateral.Add(new TBL_COLLATERAL_POLICY
            {
                //CollateralInsurancePolicyId = entity.collateralInsurancePolicyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                PREMIUMAMOUNT = entity.premiumAmount,
                POLICYAMOUNT = entity.policyAmount,
                INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                INSURERADDRESS = entity.insurerAddress,
                POLICYSTARTDATE = entity.policyStartDate,
                ASSIGNDATE = entity.assignDate,
                RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                INSURERDETAILS = entity.insurerDetails,
                POLICYRENEWALDATE = entity.policyRenewalDate,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralInsurancePolicyViewModel CollateralInsurancePolicy(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_POLICY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralInsurancePolicyViewModel
                    {
                        collateralInsurancePolicyId = m.COLLATERALINSURANCEPOLICYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        insurancePolicyNumber = m.INSURANCEPOLICYNUMBER,
                        premiumAmount = m.PREMIUMAMOUNT,
                        policyAmount = m.POLICYAMOUNT,
                        insuranceCompanyName = m.INSURANCECOMPANYNAME,
                        insurerAddress = m.INSURERADDRESS,
                        policyStartDate = m.POLICYSTARTDATE,
                        assignDate = m.ASSIGNDATE,
                        renewalFrequencyTypeId = m.RENEWALFREQUENCYTYPEID,
                        insurerDetails = m.INSURERDETAILS,
                        policyRenewalDate = m.POLICYRENEWALDATE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        private CollateralInsurancePolicyViewModel GetCollateralInsurancePolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralInsurancePolicy(collateralCustomerId);
        }
        #endregion Insurance Policy


        private CollateralGauranteeViewModel CollateralGaurantee(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_GAURANTEE
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralGauranteeViewModel
                    {
                        collateralGauranteeId = m.COLLATERALGAURANTEEID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        institutionName = m.INSTITUTIONNAME,
                        guarantorAddress = m.GUARANTORADDRESS,
                        guarantorReferenceNumber = m.GUARANTORREFERENCENUMBER,
                        guaranteeValue = m.GUARANTEEVALUE,
                        startDate = m.STARTDATE,
                        endDate = m.ENDDATE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        #region Vehicle
        private ICollection<TBL_COLLATERAL_VEHICLE> AddCollateralVehicle(CollateralTypeEnum collateralType, CollateralVehicleViewModel entity)
        {
            ICollection<TBL_COLLATERAL_VEHICLE> collateral;

            if (collateralType != CollateralTypeEnum.Vehicle)
                return null;

            collateral = new List<TBL_COLLATERAL_VEHICLE>();

            collateral.Add(new TBL_COLLATERAL_VEHICLE
            {
                VEHICLETYPE = entity.vehicleType,
                VEHICLESTATUS = entity.vehicleStatus,
                VEHICLEMAKE = entity.vehicleMake,
                MODELNAME = entity.modelName,
                MANUFACTUREDDATE = entity.manufacturedDate,
                REGISTRATIONNUMBER = entity.registrationNumber,
                SERIALNUMBER = entity.serialNumber,
                CHASISNUMBER = entity.chasisNumber,
                ENGINENUMBER = entity.engineNumber,
                NAMEOFOWNER = entity.nameOfOwner,
                REGISTRATIONCOMPANY = entity.registrationCompany,
                RESALEVALUE = entity.resaleValue,
                VALUATIONDATE = entity.valuationDate,
                LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                INVOICEVALUE = entity.invoiceValue,
                REMARK = entity.remark
            });

            return collateral;
        }

        private CollateralVehicleViewModel CollateralVehicle(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_VEHICLE
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralVehicleViewModel
                    {
                        collateralVehicleId = m.COLLATERALVEHICLEID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        vehicleType = m.VEHICLETYPE,
                        vehicleStatus = m.VEHICLESTATUS,
                        vehicleMake = m.VEHICLEMAKE,
                        modelName = m.MODELNAME,
                        manufacturedDate = m.MANUFACTUREDDATE,
                        registrationNumber = m.REGISTRATIONNUMBER,
                        serialNumber = m.SERIALNUMBER,
                        chasisNumber = m.CHASISNUMBER,
                        engineNumber = m.ENGINENUMBER,
                        nameOfOwner = m.NAMEOFOWNER,
                        registrationCompany = m.REGISTRATIONCOMPANY,
                        resaleValue = m.RESALEVALUE,
                        valuationDate = m.VALUATIONDATE,
                        lastValuationAmount = m.LASTVALUATIONAMOUNT,
                        invoiceValue = m.INVOICEVALUE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        private CollateralVehicleViewModel GetCollateralVehicleByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralVehicle(collateralCustomerId);
        }
        #endregion Vehicle

        #region Miscellaneous
        private ICollection<TBL_COLLATERAL_MISCELLANEOUS> AddCollateralMiscellaneous(CollateralTypeEnum collateralType, CollateralMiscellaneousViewModel entity)
        {
            ICollection<TBL_COLLATERAL_MISCELLANEOUS> collateral;

            if (collateralType != CollateralTypeEnum.Miscellaneous)
                return null;

            collateral = new List<TBL_COLLATERAL_MISCELLANEOUS>();

            collateral.Add(new TBL_COLLATERAL_MISCELLANEOUS
            {
                //CollateralMiscellaneousId = entity.collateralMiscellaneousId,
                //CollateralCustomerId = entity.collateralCustomerId,
                NAMEOFSECURITY = entity.nameOfSecurity,
                SECURITYVALUE = entity.securityValue,
                TBL_COLLATERAL_MISCELLANEOUS_NOTES = AddCollateralMiscNotes(entity.collateralMiscellaneousNotes)
            });

            return collateral;
        }

        private CollateralMiscellaneousViewModel Miscellaneous(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_MISCELLANEOUS
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralMiscellaneousViewModel
                    {
                        collateralMiscellaneousId = m.COLLATERALMISCELLANEOUSID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        nameOfSecurity = m.NAMEOFSECURITY,
                        securityValue = m.SECURITYVALUE,
                        collateralMiscellaneousNotes = GetCollateralMiscellaneousNotesByMiscellaneousId(m.COLLATERALMISCELLANEOUSID)

                    }).FirstOrDefault();
        }

        private CollateralMiscellaneousViewModel GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId)
        {
            return Miscellaneous(collateralCustomerId);
        }
        #endregion Miscellaneous

        #region Miscellaneous Notes
        private ICollection<TBL_COLLATERAL_MISCELLANEOUS_NOTES> AddCollateralMiscNotes(List<CollateralMiscellaneousNotesViewModel> entity)
        {
            ICollection<TBL_COLLATERAL_MISCELLANEOUS_NOTES> collateral;
            collateral = new List<TBL_COLLATERAL_MISCELLANEOUS_NOTES>();
            foreach (var note in entity)
            {
                collateral.Add(new TBL_COLLATERAL_MISCELLANEOUS_NOTES
                {
                    MISCELLANEOUSNOTEID = note.miscellaneousNoteId,
                    MISCELLANEOUSID = note.miscellaneousNoteId,
                    COLUMNNAME = note.columnName,
                    COLUMNVALUE = note.columnValue
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
            return (from m in context.TBL_COLLATERAL_MISCELLANEOUS_NOTES
                    join c in context.TBL_COLLATERAL_MISCELLANEOUS on m.MISCELLANEOUSID equals c.COLLATERALMISCELLANEOUSID
                    where m.MISCELLANEOUSID == miscellaneousId
                    select new CollateralMiscellaneousNotesViewModel
                    {
                        miscellaneousNoteId = m.MISCELLANEOUSNOTEID,
                        miscellaneousId = m.MISCELLANEOUSID,
                        columnName = m.COLUMNNAME,
                        columnValue = m.COLUMNVALUE,
                    }).ToList();
        }

        private List<CollateralMiscellaneousNotesViewModel> GetCollateralMiscellaneousNotesByMiscellaneousId(int collateralMiscellaneousId)
        {
            return MiscellaneousNotes(collateralMiscellaneousId);
        }
        #endregion Miscellaneous Notes

        #region Collateral Customer Policy
        private ICollection<TBL_COLLATERAL_ITEM_POLICY> AddCollateralCustomerPolicy(int collateralTypeId, CollateralCustomerPolicyViewModel entity)
        {
            var type = context.TBL_COLLATERAL_TYPE.Where(x => x.COLLATERALTYPEID == collateralTypeId).FirstOrDefault();
            ICollection<TBL_COLLATERAL_ITEM_POLICY> customerPolicy;

            if (!type.REQUIREINSURANCEPOLICY)
                return null;
            if (entity == null)
                throw new InvalidOperationException("This collateral type requires insurance policy which was not submitted");

            customerPolicy = new List<TBL_COLLATERAL_ITEM_POLICY>();

            customerPolicy.Add(new TBL_COLLATERAL_ITEM_POLICY
            {
                //PolicyId = entity.policyId,
                // CollateralCustomerId = entity.collateralCustomerId,
                POLICYREFERENCENUMBER = entity.policyReferenceNumber,
                INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                STARTDATE = entity.startDate,
                ENDDATE = entity.endDate

            });

            return customerPolicy;
        }

        private CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_ITEM_POLICY
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralCustomerPolicyViewModel
                    {
                        policyId = m.POLICYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        policyReferenceNumber = m.POLICYREFERENCENUMBER,
                        insuranceCompanyName = m.INSURANCECOMPANYNAME,
                        startDate = m.STARTDATE,
                        endDate = m.ENDDATE
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

        public async Task<bool> AddCollateralValuer(CollateralValuersViewModel entity)
        {
            var valuer = new TBL_COLLATERAL_VALUER
            {
                CITYID = entity.cityId,
                NAME = entity.name,
                VALUERLICENCENUMBER = entity.valuerLicenceNumber,
                VALUERTYPEID = entity.valuerTypeId,
                COUNTRYID = entity.countryId,
                EMAILADDRESS = entity.emailAddress,
                PHONENUMBER = entity.phoneNumber,
                ADDRESS = entity.address,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false
            };
            context.TBL_COLLATERAL_VALUER.Add(valuer);

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_Collateral_Valuer with Id: {entity.collateralValuerId} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }

        public async Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id)
        {
            var valuer = context.TBL_COLLATERAL_VALUER.Find(id);

            if (valuer != null)
            {
                valuer.CITYID = entity.cityId;
                valuer.NAME = entity.name;
                valuer.VALUERLICENCENUMBER = entity.valuerLicenceNumber;
                valuer.VALUERTYPEID = entity.valuerTypeId;
                valuer.COUNTRYID = entity.countryId;
                valuer.EMAILADDRESS = entity.emailAddress;
                valuer.PHONENUMBER = entity.phoneNumber;
                valuer.ADDRESS = entity.address;
                valuer.COMPANYID = entity.companyId;
            };

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Collateral_Valuer with Id: {entity.collateralValuerId} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }

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
            return (from m in context.TBL_COLLATERAL_SENIORITYOFCLAIMS
                    select new CollateralSeniorityOfClaimsViewModel
                    {
                        seniorityOfClaimId = m.COLLATERALSENIORITYOFCLAIMID,
                        seniorityOfClaims = m.SENIORITYOFCLAIMS,
                        description = m.DESCRIPTION,
                        dateTimeCreated = genSetup.GetApplicationDate(),
                    });
        }
        #endregion Seniority Of Claims

        #region Listing Functions
        public IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType()
        {
            return (from m in context.TBL_COLLATERAL_VALUEBASE_TYPE
                    select new CollateralValueBaseTypeViewModel
                    {
                        collateralValueBaseTypeId = m.COLLATERALVALUEBASETYPEID,
                        collateralTypeId = m.COLLATERALTYPEID,
                        valueBaseTypeName = m.VALUEBASETYPENAME
                    });
        }

        public IEnumerable<CollateralValuersViewModel> GetCollateralValuer(int companyId)
        {
            return (from m in context.TBL_COLLATERAL_VALUER
                    where m.COMPANYID == companyId
                    select new CollateralValuersViewModel
                    {
                        collateralValuerId = m.COLLATERALVALUERID,
                        cityId = m.CITYID,
                        name = m.NAME,
                        valuerLicenceNumber = m.VALUERLICENCENUMBER,
                        valuerTypeId = m.VALUERTYPEID,
                        countryId = m.COUNTRYID,
                        //accountNumber = m.nu,
                        //valuerBVN = m.,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,

                    });
        }

        public IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType()
        {
            return (from m in context.TBL_COLLATERAL_VALUER_TYPE
                    select new CollateralValuerTypeViewModel
                    {
                        valuerTypeId = m.COLLATERALVALUERTYPEID,
                        valuerTypeName = m.VALUERTYPENAME
                    });
        }

        public IEnumerable<CollateralTypeViewModel> GetCollateralType()
        {
            return this.collateralType.GetCollateralTypes();
        }

        #endregion End of Listing Functions
    }
}
