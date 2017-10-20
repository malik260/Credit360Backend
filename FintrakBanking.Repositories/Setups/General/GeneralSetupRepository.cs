using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    public class GeneralSetupRepository : IGeneralSetupRepository
    {
        private FinTrakBankingContext context;

        public GeneralSetupRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public DateTime GetApplicationDate()
        {
            return this.context.tbl_FinanceCurrentDate.FirstOrDefault().CurrentDate;
        }

        public DateTime CalculateMaturityDate(DateTime effectiveDate, TenorModeEnum tenorModeId, int tenor)
        {
            DateTime output = DateTime.Now;

            if (tenorModeId == TenorModeEnum.Days)
                output = effectiveDate.AddDays(tenor);
            else if (tenorModeId == TenorModeEnum.Months)
                output = effectiveDate.AddMonths(tenor);
            else if (tenorModeId == TenorModeEnum.Years)
                output = effectiveDate.AddYears(tenor);

            return output;
        }

        public IEnumerable<LookupViewModel> GetAllTenorMode()
        {
            return (from data in context.tbl_Tenor_Mode
                    select new LookupViewModel()
                    {
                        lookupId = data.TenorModeId,
                        lookupName = data.TenorModeName
                    });
        }

        public int GetLoanApplicationRef()
        {
            return CommonHelpers.GetLoanReferanceNumber();
        }

        /// <summary>
        /// Returns All tbl_Product group
        /// </summary>
        /// <returns>IEnumerable of ProductGroup</returns>
        public IEnumerable<LookupViewModel> GetAllCurrency()
        {
            return (from data in context.tbl_Currency
                    select new LookupViewModel()
                    {
                        lookupId = data.CurrencyId,
                        lookupName = data.CurrencyCode + " -- " + data.CurrencyName,
                        lookupTypeName = data.CurrencyCode
                    });
        }

        public IEnumerable<LookupViewModel> GetSector()
        {
            return (from data in context.tbl_Sector
                    select new LookupViewModel()
                    {
                        lookupId = data.SectorId,
                        lookupName = data.Name
                    });
        }

        public IEnumerable<LookupViewModel> GetSubsector()
        {
            return (from data in context.tbl_Sub_Sector
                    select new LookupViewModel()
                    {
                        lookupId = data.SubSectorId,
                        lookupName = data.Name
                    });
        }

        public IEnumerable<LookupViewModel> GetAllCustomerType()
        {
            return (from data in context.tbl_Customer_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.CustomerTypeId,
                        lookupName = data.Name
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealClassificationType()
        {
            return (from data in context.tbl_Deal_Classification
                    select new LookupViewModel()
                    {
                        lookupId = data.DealClassificationID,
                        lookupName = data.Classification
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDayCount()
        {
            return (from data in context.tbl_Day_Count_Convention
                    select new LookupViewModel()
                    {
                        lookupId = data.DayCountConventionId,
                        lookupName = data.DayCountConventionName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFeeAmortisationType()
        {
            return (from data in context.tbl_Fee_Amortisation_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.FeeAmortisationTypeId,
                        lookupName = data.FeeAmortisationTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealTypes()
        {
            return (from data in context.tbl_Deal_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.DealTypeId,
                        lookupName = data.DealTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFSTypes()
        {
            return (from data in context.tbl_Financial_Statement_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.FSTypeId,
                        lookupName = data.FSTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFrequencyTypes()
        {
            return (from data in context.tbl_Frequency_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.FrequencyTypeId,
                        lookupName = data.Mode
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperationTypes()
        {
            return (from data in context.tbl_Operations_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.OperationTypeId,
                        lookupName = data.OperationTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperations()
        {
            return (from data in context.tbl_Operations
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OperationId,
                        lookupName = data.OperationName,
                        lookupTypeId = data.OperationTypeId,
                        lookupTypeName = data.tbl_Operations_Type.OperationTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetOperations(short operationTypeId)
        {
            return (from data in context.tbl_Operations
                    where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OperationId,
                        lookupName = data.OperationName,
                        lookupTypeId = data.OperationTypeId,
                        lookupTypeName = data.tbl_Operations_Type.OperationTypeName
                    });
        }

        /// <summary>
        /// Adds new product group
        /// </summary>
        /// <param name="group"></param>
        /// <returns>true/false</returns>

        //public async Task<bool> SaveProductGroup(ProductGroupViewModel group)
        //{
        //    var productGroup = new TblProductGroup()
        //    {
        //        ProductGroupName = group.productGroupName,
        //        ProductGroupCode = group.productGroupCode
        //    };

        //    this.context.TblProductGroup.Add(productGroup);

        //    var response = await this.context.SaveChangesAsync();
        //    return response != 0;
        //}

        //   public IEnumerable<LoanCovenantDetailViewModel>  Get

        public IEnumerable<SectorViewModel> GetAllSectors()
        {
            var data = (from cs in context.tbl_Sector
                        select new SectorViewModel()
                        {
                            sectorId = cs.SectorId,
                            sectorName = cs.Name,
                            sectorCode = cs.Code,
                        });

            return data;
        }

        public IEnumerable<SectorViewModel> GetAllSubSectors()
        {
            var data = (from cs in context.tbl_Sub_Sector
                        select new SectorViewModel()
                        {
                            subSectorId = cs.SubSectorId,
                            sectorId = cs.tbl_Sector.SectorId,
                            sectorName = cs.Name,
                            sectorCode = cs.Code,
                        }).Distinct();

            return data;
        }

        public IEnumerable<SectorViewModel> GetSectorsBySubSectorId(short ssId)
        {
            var data = (from s in context.tbl_Sub_Sector
                        where s.SubSectorId == ssId
                        select new SectorViewModel()
                        {
                            subSectorId = s.SubSectorId,
                            sectorId = s.tbl_Sector.SectorId,
                            sectorName = s.Name,
                            sectorCode = s.Code
                        });

            return data;
        }
    }
}