using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Helper;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
            return this.context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;            
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
            return (from data in context.TBL_TENOR_MODE
                    select new LookupViewModel()
                    {
                        lookupId = data.TENORMODEID,
                        lookupName = data.TENORMODENAME
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
            return (from data in context.TBL_CURRENCY
                    where data.INUSE == true
                    select new LookupViewModel()
                    {
                        lookupId = data.CURRENCYID,
                        lookupName = data.CURRENCYCODE + " -- " + data.CURRENCYNAME,
                         lookupTypeName = data.CURRENCYCODE
                    });
        }

        public IEnumerable<LookupViewModel> GetSector() {
            return (from   data in context.TBL_SECTOR
                    select new LookupViewModel()
                    {
                        lookupId = data.SECTORID ,
                        lookupName = data.NAME  
                    });
        }
        public IEnumerable<LookupViewModel> GetSubsector( )
        {
            return (from data in context.TBL_SUB_SECTOR 
                    select new LookupViewModel()
                    {
                        lookupId = data.SUBSECTORID,
                        lookupName = data.NAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllCustomerType()
        {
            return (from data in context.TBL_CUSTOMER_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.CUSTOMERTYPEID,
                        lookupName = data.NAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealClassificationType()
        {
            return (from data in context.TBL_DEAL_CLASSIFICATION
                    select new LookupViewModel()
                    {
                        lookupId = data.DEALCLASSIFICATIONID,
                        lookupName = data.CLASSIFICATION
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDayCount()
        {
            return (from data in context.TBL_DAY_COUNT_CONVENTION
                    select new LookupViewModel()
                    {
                        lookupId = data.DAYCOUNTCONVENTIONID,
                        lookupName = data.DAYCOUNTCONVENTIONNAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFeeAmortisationType()
        {
            return (from data in context.TBL_FEE_AMORTISATION_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FEEAMORTISATIONTYPEID,
                        lookupName = data.FEEAMORTISATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealTypes()
        {
            return (from data in context.TBL_DEAL_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.DEALTYPEID,
                        lookupName = data.DEALTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFSTypes()
        {
            return (from data in context.TBL_FINANCIAL_STATEMENT_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FSTYPEID,
                        lookupName = data.FSTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFrequencyTypes()
        {
            return (from data in context.TBL_FREQUENCY_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FREQUENCYTYPEID,
                        lookupName = data.MODE
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperationTypes()
        {
            return (from data in context.TBL_OPERATIONS_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.OPERATIONTYPEID,
                        lookupName = data.OPERATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperations()
        {
            return (from data in context.TBL_OPERATIONS                    
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OPERATIONID,
                        lookupName = data.OPERATIONNAME,
                        lookupTypeId = data.OPERATIONTYPEID,
                        lookupTypeName = data.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetOperations(short operationTypeId)
        {
            return (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OPERATIONID,
                        lookupName = data.OPERATIONNAME,
                        lookupTypeId = data.OPERATIONTYPEID,
                        lookupTypeName = data.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
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
            var data = (from cs in context.TBL_SECTOR
                        select new SectorViewModel()
                        {
                            sectorId = cs.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                        });

            return data;
        }

        public IEnumerable<SectorViewModel> GetAllSubSectors()
        {
            var data = (from cs in context.TBL_SUB_SECTOR
                        select new SectorViewModel()
                        {
                            subSectorId = cs.SUBSECTORID,
                            sectorId = cs.TBL_SECTOR.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                        }).Distinct();

            return data;
        }

        public IEnumerable<SectorViewModel> GetSectorsBySubSectorId(short ssId)
        {
            var data = (from s in context.TBL_SUB_SECTOR
                        where s.SUBSECTORID == ssId
                        select new SectorViewModel()
                        {
                            subSectorId = s.SUBSECTORID,
                            sectorId = s.TBL_SECTOR.SECTORID,
                            sectorName = s.NAME,
                            sectorCode = s.CODE
                        });

            return data;
        }

    }
}