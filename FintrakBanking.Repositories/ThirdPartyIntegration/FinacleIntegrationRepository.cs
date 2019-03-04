using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.CR_ACCT.ToLower()== searchItem || x.DR_ACCT.ToLower() == searchItem || x.BATCH_ID.ToLower() == searchItem 
                                                    || x.FLOW_TYPE.ToLower() == searchItem || searchItem.StartsWith( x.NARRATION.ToLower())
                                                    || x.TRAN_ID.ToLower() == searchItem || x.TRAN_TYPE.ToLower() == searchItem || searchItem ==null || searchItem=="")
                    select new BatchPostingViewModel
                    {
                        sid = x.SID,
                        batchId = x.BATCH_ID,
                        batchRefId = x.BATCH_REF_ID,
                        trancType = x.TRAN_TYPE,
                        flowType = x.FLOW_TYPE,
                        amt = x.AMT,
                        drAccount = x.DR_ACCT,
                        crAccount = x.CR_ACCT,
                        currencyCode = x.REF_CRNCY_CODE,
                        rateCode = x.RATE_CODE,
                        rate = x.RATE,
                        naration = x.NARRATION,
                        status = x.STATUS,
                        tranactionId = x.TRAN_ID,
                        postedFlag = x.PSTD_FLG,
                        postedDate = x.PSTD_DATE,
                        rcreDate = x.RCRE_DATE,
                        postedUserId = x.PSTD_USR_ID,
                        failedFlag = x.FAIL_FLG,
                        deleteFlag = x.DEL_FLG,
                        failureReasonCode = x.FAILURE_REASON_CODE,
                        failureReason = x.FAILURE_REASON,
                        amountCollected = x.AMT_COLLECTED,
                        lienAmount = x.LIEN_AMT,
                        lienFlg = x.LIEN_FLG,
                        TodFlg = x.TOD_FLG,
                        valueDateNumber = x.VALUE_DATE_NUM,
                        loanAccount = x.LOAN_ACCT,
                        fintrakFlag = x.FINTRAK_FLG,
                        bankId = x.BANK_ID


                    }).ToList();
        }

        public List<BatchPostingViewModel> GetBatchPostingMain(DateTime startDate, DateTime endDate, string searchItem)
        {
            if (searchItem != null)
                searchItem = searchItem.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_MAIN
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && (x.BATCH_ID.ToLower()==searchItem || searchItem == null || searchItem == "")
                    select new BatchPostingViewModel
                    {
                        sid =x.SID,
                        batchId = x.BATCH_ID,
                        trancType  = x.TRAN_TYPE,
                        rcreDate = x.RCRE_DATE,
                        rcreUser = x.RCRE_USER,
                       totalAmount = x. TOTAL_AMT,
                       recCount =x.REC_COUNT,
                       status = x.STATUS,
                       postedDate = x.PSTD_DATE,
                       postedUserId = x.PSTD_USR_ID,
                       deleteFlag = x.DEL_FLG,
                       isSelected = x.IS_SELECTED,
                       bankId = x.BANK_ID,
                       totalAmountCollected = x.TOTAL_AMT_COLLECTED,
                       postedFlag = x.PSTD_FLG
                        
                    }).ToList();
        }

        public List<BatchPostingViewModel> GetBatchPostingDetailSearch(DateTime startDate, DateTime endDate, string status)
        {
            if (status != null)
                status = status.ToLower();

            return (from x in _stgCon.FINTRAK_TRAN_PROC_DETAILS
                    where DbFunctions.TruncateTime(x.RCRE_DATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(x.RCRE_DATE) <= DbFunctions.TruncateTime(endDate)
                                                    && x.STATUS.ToLower() == status
                    select new BatchPostingViewModel
                    {
                        sid = x.SID,
                        batchId = x.BATCH_ID,
                        batchRefId = x.BATCH_REF_ID,
                        trancType = x.TRAN_TYPE,
                        flowType = x.FLOW_TYPE,
                        amt = x.AMT,
                        drAccount = x.DR_ACCT,
                        crAccount = x.CR_ACCT,
                        currencyCode = x.REF_CRNCY_CODE,
                        rateCode = x.RATE_CODE,
                        rate = x.RATE,
                        naration = x.NARRATION,
                        status = x.STATUS,
                        tranactionId = x.TRAN_ID,
                        postedFlag = x.PSTD_FLG,
                        postedDate = x.PSTD_DATE,
                        rcreDate = x.RCRE_DATE,
                        postedUserId = x.PSTD_USR_ID,
                        failedFlag = x.FAIL_FLG,
                        deleteFlag = x.DEL_FLG,
                        failureReasonCode = x.FAILURE_REASON_CODE,
                        failureReason = x.FAILURE_REASON,
                        amountCollected = x.AMT_COLLECTED,
                        lienAmount = x.LIEN_AMT,
                        lienFlg = x.LIEN_FLG,
                        TodFlg = x.TOD_FLG,
                        valueDateNumber = x.VALUE_DATE_NUM,
                        loanAccount = x.LOAN_ACCT,
                        fintrakFlag = x.FINTRAK_FLG,
                        bankId = x.BANK_ID


                    }).ToList();
        }
        
        #endregion

    }
}
