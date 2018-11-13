using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Credit
{
    public class FXAccountCreationRepository : IFXAccountCreationRepository
    {
        private FinTrakBankingStagingContext context;
        private FinTrakBankingContext bankingContext;
        private IIntegrationWithFinacle finacle;
        private IAuditTrailRepository auditTrail;
        bool USE_THIRD_PARTY_INTEGRATION = false;

        public short AuditType { get; private set; }

        public FXAccountCreationRepository(FinTrakBankingStagingContext _context,
            FinTrakBankingContext _bankingContext,
            IIntegrationWithFinacle _finacle,
            IAuditTrailRepository _auditTrail)
        {
            bankingContext = _bankingContext;
            context = _context;
            finacle = _finacle;
            auditTrail = _auditTrail;
            var globalSetting = bankingContext.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode1()
        {
            var freecode1 = (from a in context.STG_FREECODE1
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE1,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode4()
        {
            var freecode1 = (from a in context.STG_FREECODE4
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE4,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode5()
        {
            var freecode1 = (from a in context.STG_FREECODE5
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE5,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode6()
        {
            var freecode1 = (from a in context.STG_FREECODE6
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE6,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode7()
        {
            var freecode1 = (from a in context.STG_FREECODE7
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE7,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode8()
        {
            var freecode1 = (from a in context.STG_FREECODE8
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE8,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode9()
        {
            var freecode1 = (from a in context.STG_FREECODE9
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE9,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllFreeCode10()
        {
            var freecode1 = (from a in context.STG_FREECODE10
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE10,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllModeOfAdvance()
        {
            var freecode1 = (from a in context.STG_MODE_OF_ADV
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.MODE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllNatureOfAdvance()
        {
            var freecode1 = (from a in context.STG_NAT_OF_ADV
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.NATURE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllOccupationCode()
        {
            var freecode1 = (from a in context.STG_OCCUPATION_CODE
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.OCCUPATION_CODE,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllPurposeOfAdvance()
        {
            var freecode1 = (from a in context.STG_PURPOSE_OF_ADV
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.PURPOSE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 //  del_Flg = a.DEL_FLG,
                                 //  bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllAdvanceType()
        {
            var freecode1 = (from a in context.STG_ADVANCE_TYPE
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.ADVANCE_TYPE,
                                 ref_Desc = a.REF_DESC,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllBorrowerCategory()
        {
            var freecode1 = (from a in context.STG_BORROWER_CAT
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.BORROWER_CATEGORY,
                                 ref_Desc = a.REF_DESC,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllSubSector()
        {
            var freecode1 = (from a in context.STG_SUB_SECTOR
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.SUB_SECTOR,
                                 ref_Desc = a.REF_DESC,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllSector()
        {
            var freecode1 = (from a in context.STG_SECTOR_CODE
                             where a.DEL_FLG == "N"
                             orderby a.REF_DESC
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.SECTOR_CODE,
                                 ref_Desc = a.REF_DESC,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllCurrencyCode()
        {
            var freecode1 = (from a in context.STG_CURRENCY_TBL
                             where a.ENTITY_CRE_FLAG == "Y"
                             orderby a.CURRENCY_NAME
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.CURRENCY,
                                 ref_Desc = a.CURRENCY_NAME,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllSchemeCode()
        {
            var freecode1 = (from a in context.STG_SCHEMECODE_TBL
                             orderby a.SCHEME_CODE
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.SCHEME_CODE,
                                 ref_Desc = a.SCHEME_CODE + " - " + a.SCHEME_DESCRIPTION,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllSolId()
        {
            var freecode1 = (from a in context.STG_SOL_TBL
                             orderby a.BRANCH_NAME
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.SOL_ID,
                                 ref_Desc = a.BRANCH_NAME,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllGLSubHead()
        {
            var freecode1 = (from a in context.STG_GL_SUBHEAD_TBL
                             orderby a.GL_SUB_HEAD_CODE
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.GL_SUB_HEAD_CODE,
                                 ref_Desc = a.SCHM_CODE + " - " + a.GL_SUB_HEAD_CODE,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FXAccountCreationViewModel> GetAllGLSubHead(string schemeCode)
        {
            var freecode1 = (from a in context.STG_GL_SUBHEAD_TBL
                             where a.SCHM_CODE.ToLower().Trim() == schemeCode.ToLower().Trim()
                             orderby a.GL_SUB_HEAD_CODE
                             select new FXAccountCreationViewModel
                             {
                                 fx_Code = a.GL_SUB_HEAD_CODE,
                                 ref_Desc = a.SCHM_CODE + " - " + a.GL_SUB_HEAD_CODE,
                                 // del_Flg = a.DEL_FLG,
                                 // bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }



        public FXAccountCreationListViewModel GetListOfFSCode()
        {
            var list = new FXAccountCreationListViewModel();
            list.freeCode1 = GetAllFreeCode1();
            list.freeCode4 = GetAllFreeCode4();
            list.freeCode5 = GetAllFreeCode5();
            list.freeCode6 = GetAllFreeCode6();
            list.freeCode7 = GetAllFreeCode7();
            list.freeCode8 = GetAllFreeCode8();
            list.freeCode9 = GetAllFreeCode9();
            list.freeCode10 = GetAllFreeCode10();

            list.modeOfAdvance = GetAllModeOfAdvance();
            list.natureOfAdvance = GetAllNatureOfAdvance();
            list.purposeOfAdvance = GetAllPurposeOfAdvance();
            list.advanceType = GetAllAdvanceType();

            list.sectorCode = GetAllSector();
            list.sub_sector = GetAllSubSector();

            list.borrowerCategoryCode = GetAllBorrowerCategory();
            list.occupationCode = GetAllOccupationCode();

            list.currencyCode = GetAllCurrencyCode();
            list.schemeCode = GetAllSchemeCode();
            list.sol_Ids = GetAllSolId();
            list.glSubHeadCode = GetAllGLSubHead();
            return list;
        }
        public string ForeignCurrencyAccountCreation(CreateAccountViewModel entity, UserInfo user)
        {
            try
            {
                string accountNumber = null;
                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var customerInfo = bankingContext.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == entity.customerCode).Select(a => a).FirstOrDefault();
                    var branchCode = bankingContext.TBL_BRANCH.Where(x => x.BRANCHID == user.BranchId).Select(k => k.BRANCHCODE).FirstOrDefault();
                    entity.solId = branchCode;

                    var accountInfo = finacle.CreateForeignAccount(entity);
                    if (accountInfo != null)
                    {
                        var currencyId = bankingContext.TBL_CURRENCY.Where(x => x.CURRENCYCODE.ToUpper() == entity.currencyCode.ToUpper()).Select(x => x.CURRENCYID).FirstOrDefault();

                        accountNumber = accountInfo.accountNumber;
                        var customerExist = (from a in bankingContext.TBL_CASA where a.PRODUCTACCOUNTNUMBER == accountInfo.accountNumber select a).Any();
                        if (!customerExist)
                        {
                            TBL_CASA addCustomerAcct = new TBL_CASA();
                            addCustomerAcct.CUSTOMERID = customerInfo.CUSTOMERID;
                            addCustomerAcct.AVAILABLEBALANCE = 0;
                            addCustomerAcct.LEDGERBALANCE = 0;
                            addCustomerAcct.PRODUCTACCOUNTNAME = "FX Revolving Interest CAP Account";
                            addCustomerAcct.PRODUCTACCOUNTNUMBER = accountInfo.accountNumber;
                            addCustomerAcct.PRODUCTID = (short)DefaultProductEnum.CASA; ;
                            addCustomerAcct.COMPANYID = user.companyId;
                            addCustomerAcct.BRANCHID = (short)user.BranchId;
                            addCustomerAcct.CURRENCYID = currencyId;
                            addCustomerAcct.ISCURRENTACCOUNT = true;
                            addCustomerAcct.ACCOUNTSTATUSID = (short)CASAAccountStatusEnum.Active;
                            addCustomerAcct.LIENAMOUNT = 0;
                            addCustomerAcct.HASLIEN = false;
                            addCustomerAcct.POSTNOSTATUSID = 1;
                            addCustomerAcct.DELETED = false;
                            bankingContext.TBL_CASA.Add(addCustomerAcct);
                        }
                    }
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ForeignAccountCreation,
                        STAFFID = user.staffId,
                        BRANCHID = (short)user.BranchId,
                        DETAIL = $"Added New Foreign Loan Account for {accountInfo.customerName} with account number {accountInfo.accountNumber} ",
                        IPADDRESS = user.userIPAddress,
                        URL = user.applicationUrl,
                        APPLICATIONDATE = DateTime.Now,
                        SYSTEMDATETIME = DateTime.Now
                    };
                    auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------
                }
                var output = bankingContext.SaveChanges() > 0;
                if (output == true)
                {
                    return accountNumber;
                }
                return null;
            }
            catch (TwoFactorAuthenticationException et)
            {
                 throw new TwoFactorAuthenticationException(et.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
