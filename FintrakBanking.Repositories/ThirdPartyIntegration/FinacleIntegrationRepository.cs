using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.ThirdPartyIntegration
{
    public class FinacleIntegrationRepository : IFinacleIntegrationRepository
    {
        private FinTrakBankingStagingContext _stgCon;
        public FinacleIntegrationRepository(FinTrakBankingStagingContext stgCon)
        {
            _stgCon = stgCon;
        }
        #region Batch Posting Report
        public List<BatchPostingViewModel> GetBatchPostingDetail(DateTime startDate, DateTime endDate, string searchItem)
        {
            if (searchItem!=null)
                searchItem = searchItem.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_DETAILS
                    where DbFunctions.TruncateTime(x.PSTD_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.PSTD_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.CR_ACCT.ToLower()== searchItem || x.DR_ACCT.ToLower() == searchItem || x.BATCH_ID.ToLower() == searchItem 
                                                    || x.FLOW_TYPE.ToLower() == searchItem || searchItem.StartsWith( x.NARRATION.ToLower())
                                                    || x.TRAN_ID.ToLower() == searchItem || x.TRAN_TYPE.ToLower() == searchItem || searchItem ==null || searchItem=="")
                    select new BatchPostingViewModel
                    {
                        amount = x.AMT,
                        amountCollected = x.AMT_COLLECTED,
                            bankId = x.BANK_ID,
                            batchId = x.BATCH_ID,
                            batchRefId = x.BATCH_REF_ID,
                            crAccount = x.CR_ACCT,
                            creditDate = x.RCRE_DATE,
                            currencyCode = x.REF_CRNCY_CODE,
                            deleteFlag = x.DEL_FLG,
                            drAccount = x.DR_ACCT,
                            failedFlag = x.FAIL_FLG,
                            failureReasonCode = x.FAILURE_REASON_CODE,
                            fintrakFlag = x.FINTRAK_FLG,
                            flowType = x.FLOW_TYPE,
                            lienAmount = x.LIEN_AMT,
                            lienFlg = x.LIEN_FLG,
                            loanAccount = x.LOAN_ACCT,
                            naration = x.NARRATION,
                        postedDate = x.PSTD_DATE,
                        postedFlag = x.PSTD_FLG,
                        postedUserId = x.PSTD_USR_ID,
                        rate = x.RATE,
                            sid = x.SID,
                        status = x.STATUS,
                        TodFlg = x.TOD_FLG,
                        tranactionId = x.TRAN_ID,
                        trancType = x.TRAN_TYPE,
                        valueDateNumber = x.VALUE_DATE_NUM,


                    }).ToList();
        }

        public List<BatchPostingViewModel> GetBatchPostingMain(DateTime startDate, DateTime endDate, string searchItem)
        {
            if (searchItem != null)
                searchItem = searchItem.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_MAIN
                    where DbFunctions.TruncateTime(x.PSTD_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.PSTD_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.BATCH_ID.ToLower()==searchItem || searchItem == null || searchItem == "")
                    select new BatchPostingViewModel
                    {
                        totalAmount = x.TOTAL_AMT,
                        totalAmountCollected = x.TOTAL_AMT_COLLECTED,
                        bankId = x.BANK_ID,
                        batchId = x.BATCH_ID,
                        deleteFlag = x.DEL_FLG,
                        postedFlag = x.PSTD_FLG,
                        postedUserId = x.PSTD_USR_ID,
                        sid = x.SID,
                        status = x.STATUS,
                        trancType = x.TRAN_TYPE,
                        creditDate = x.RCRE_DATE,
                        rcreUser = x.RCRE_USER,
                        recCount = x.REC_COUNT,
                        postedDate = x.PSTD_DATE,
                        isSelected = x.IS_SELECTED

                    }).ToList();
        }
        #endregion

    }
}
