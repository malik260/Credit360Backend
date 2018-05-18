using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI;
using System.Net.Http;
using System.Linq;

namespace FinTrakBanking.ThirdPartyIntegration.Credit
{
    public   class IntegrationWithCWGAPI : IIntegrationWithCWGAPI
    {
        private FinTrakBankingContext context;
        private TransactionPosting transaction;
        private CustomerDetails customer;
        public IntegrationWithCWGAPI(FinTrakBankingContext context, TransactionPosting transaction, CustomerDetails customer)
        {
            this.context = context;
            this.transaction = transaction;
            this.customer = customer;
        }

        public ResponseMessageViewModel OverDraftExtend(OverDraftExtendViewModel model)
        {


            ResponseMessage result = null;
            if(LogOverDraftExtend(model))
            Task.Run(async () => result = await transaction.APIOverDraftExtend(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogOverDraftExtend(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + " " + result.Message.ReasonPhrase);
            }

        }

        public ResponseMessageViewModel OverDraftNormal(OverDraftNormalViewModel model)
        {
            ResponseMessage result = null;
           if( LogOverDraftNormal(model))
            Task.Run(async () => result = await transaction.APIOverDraftNormal(model)).GetAwaiter().GetResult();
          
            if (result.Message.IsSuccessStatusCode)
            {
               if( result.APIResponse.webRequestStatus.Replace(":","") == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogOverDraftNormal(model);
                    return result.APIResponse;
                }
            }
            else
            {                 
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
            
           
        }

        public ResponseMessageViewModel OverDraftTopUp(OverDraftTopUpAndRenewViewModel model)
        {
            ResponseMessage result = null;

            model.apiUrl = @"api/OverDraft/TopUp";

            if (LogOverDraftTopUpAndRenew(model))
            Task.Run(async () => result = await transaction.APIOverDraftTopUp(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogOverDraftTopUpAndRenew(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel OverDraftRenew(OverDraftTopUpAndRenewViewModel model)
        {
            ResponseMessage result = null;

            model.apiUrl = @"api/OverDraft/Renew ";

            if (LogOverDraftTopUpAndRenew(model))
                Task.Run(async () => result = await transaction.APIOverDraftTopUp(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogOverDraftTopUpAndRenew(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model)
        {
            ResponseMessage result = null;
            model.APIUrl = @"api/TemporaryOverDraft/Normal";


            Task.Run(async () => result = await transaction.APITemporaryOverDraftNormal(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogTemporaryOverDraft(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model)
        {
            model.APIUrl = @"api/TemporaryOverDraft/Running";
            ResponseMessage result = null;

            if (LogTemporaryOverDraft(model))  
                Task.Run(async () => result = await transaction.APITemporaryOverDraftRunning(model)).GetAwaiter().GetResult();            
           
          

                if (result.Message.IsSuccessStatusCode)
                {
                    if (result.APIResponse.webRequestStatus == "FAILURE")
                    {
                        throw new Exception(result.APIResponse.webRequestStatus);
                    }
                    else
                    {
                    LogTemporaryOverDraft(model);
                    return  result.APIResponse;
                    }
                }
                else
                {
                    throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
                }
            //}
            //else
            //{
            //    throw new Exception("Logging Finaco transaction failed, operation is truncated.");
            //}
            
        }

        public ResponseMessageViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model)
        {
            model.APIUrl = @"api/TemporaryOverDraft/Single";
            ResponseMessage result = null;
           if(LogTemporaryOverDraft(model))
            Task.Run(async () => result = await transaction.APITemporaryOverDraftSingle(model)).GetAwaiter().GetResult();            

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    LogTemporaryOverDraft(model);
                    return result.APIResponse;
                }
            }
            else
            {

                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }
               
        public bool GetExposePersonStatus(string customerCode)
        {
            bool result = false;
            string  data = string.Empty;

            Task.Run(async () => data = await customer.CheckExposePerson(customerCode)).GetAwaiter().GetResult();

            if(data == "" || data == "NO-MATCH" || data == "NOT AVAIABLE")
                result = false ;
            if (data == "Match" )
                result =  true;

           return result;

        }

        public  BVNCustomerDetailsViewModel BVNCustomerDetails(string customerCode)
        {        
            BVNCustomerDetailsViewModel data =  null;
            Task.Run(async () => data = await customer.BVNCustomerDetails(customerCode)).GetAwaiter().GetResult();
            if(data != null)
                return data;
            throw new Exception("Not Found ");

        }

        public GLAccountDetailsViewModel ValidateGLNumber(string glNumber)
        {
            GLAccountDetailsViewModel result = null;
            Task.Run(async () => result = await transaction.APIOfficeAccountGetGeneralLedgerAccountRecord(glNumber)).GetAwaiter().GetResult();
            if (result.response.ReasonPhrase == "OK")
            {
                return result;
            }
            return result;
        }
        
        public TDAccountRecordViewModel ValidateTDAccountNumber(string teamDepositAccountNumber)
        {
            TDAccountRecordViewModel result = null;
            Task.Run(async () => result = await transaction.APIOfficeAccountGetTermDepositAccountRecord(teamDepositAccountNumber)).GetAwaiter().GetResult();
            if (result.response.ReasonPhrase == "OK")
            {
                return result;
            }
            return result;
        }


        #region  private
        private bool LogOverDraftExtend(OverDraftExtendViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTEXTEND.Find(model.overdraftExtendId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_OVERDRAFTEXTEND
                {
                    ACCOUNTNUMBER = model.accountNumber,
                    APIURL = @"api/OverDraft/Extend",
                    DATETIMECREATED = DateTime.Now,
                    EXPIRYDATE = model.expiryDate,
                    SANCTIONLIMIT = model.sanctionLimit,
                    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber
                };
                context.TBL_CUSTOM_OVERDRAFTEXTEND.Add(data);
                result = context.SaveChanges() > 0;
                model.overdraftExtendId = data.OVERDRAFTEXTENDID;
           
            }
            return result;
        }

        private bool LogOverDraftNormal(OverDraftNormalViewModel model )
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTNORMAL.Find(model.overdraftNormalId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_OVERDRAFTNORMAL
                {
                    ACCOUNTNUMBER = model.accountNumber,
                    APIURL = @"api/OverDraft/Normal",
                    DATETIMECREATED = DateTime.Now,
                    EXPIRYDATE = model.expiryDate,
                    SANCTIONLIMIT = model.sanctionLimit,
                    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber,
                    APPLICATIONDATE = model.applicationDate,
                    DOCUMENTDATE = model.documentDate,
                    REVIEWEDDATE = model.reviewedDate,
                    SANCTIONAUTHORIZER = model.sanctionAuthorizer,
                    SANCTIONDATE = model.sanctionDate,
                    SANCTIONLEVEL = model.sanctionLevel,

                };
                context.TBL_CUSTOM_OVERDRAFTNORMAL.Add(data);
                result = context.SaveChanges() > 0;
                model.overdraftNormalId = data.OVERDRAFTNORMALID;
            }
            return result;
        }

        private bool LogOverDraftTopUpAndRenew(OverDraftTopUpAndRenewViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTEXTEND.Find(model.overdraftExtendId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_OVERDRAFTEXTEND
                {

                    ACCOUNTNUMBER = model.accountNumber,
                    APIURL = model.apiUrl,
                    DATETIMECREATED = DateTime.Now,
                    EXPIRYDATE = model.expiryDate,
                    SANCTIONLIMIT = model.sanctionLimit,
                    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber
                };
                context.TBL_CUSTOM_OVERDRAFTEXTEND.Add(data);
                result = context.SaveChanges() > 0;
                model.overdraftExtendId = data.OVERDRAFTEXTENDID;
            }
            return result;

        }

        private bool LogTemporaryOverDraft(TemporaryOverDraftViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_TEMPORARYOVERDRAFT.Find(model.TemporaryOverDraftId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_TEMPORARYOVERDRAFT
                {
                    APIURL = model.APIUrl,
                    DATETIMECREATED = DateTime.Now,
                    TEMPORARYOVERDRAFTAMOUNT = model.TemporaryOverDraftAmount,
                    TEMPORARYOVERDRAFTDATE = model.TemporaryOverDraftDate,
                    TEMPORARYOVERDRAFTFLAG = model.TemporaryOverDraftFlag,
                    TEMPORARYOVERDRAFTNARATION = model.TemporaryOverDraftNaration
                };
                context.TBL_CUSTOM_TEMPORARYOVERDRAFT.Add(data);
                result = context.SaveChanges() > 0;
                model.TemporaryOverDraftId = data.TEMPORARYOVERDRAFTID;
            }
            return result;
        }

     
        #endregion

    }
}
