using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Helper;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels; 
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IGeneralSetupRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GeneralSetupRepository : IGeneralSetupRepository
    {
        private FinTrakBankingContext context;
        ICultureHelper cultureHelper;
        public GeneralSetupRepository(FinTrakBankingContext _context,
                                      ICultureHelper _cultureHelper)
        {
            this.context = _context;
            this.cultureHelper = _cultureHelper;
        }

        public DateTime GetApplicaionDate()
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
                        lookupName = data.CurrencyCode + " -- " + data.CurrencyName
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
                        lookupId = data.DealClassificationId,
                        lookupName = data.Classification
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDayCount()
        {
            return (from data in context.tbl_Day_Count
                    select new LookupViewModel()
                    {
                        lookupId = data.DayCountId,
                        lookupName = data.DayCountName
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
                        lookupId = short.Parse(data.OperationId.ToString()),
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
                        lookupId = short.Parse(data.OperationId.ToString()),
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

    }
}