using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common.Extensions;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.Credit;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration
{
    public partial class OfferLetter : System.Web.UI.Page
    {
        private string API_KEY = "RlRDMzYwOnRlc3RTZWNyZXQ=";
        private string API_URL = "http://10.1.7.116:8989/";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string applicationRefNumber = Request.QueryString["applicationRefNumber"];

                    GenerateOutPutDocument(applicationRefNumber);


                }
                catch(Exception ex)
                {
                    //throw new Exception(ex.Message);
                    this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.offerLetterReport.LocalReport.Refresh();
                    return;
                }
            }
        }

        void GenerateOutPutDocument(string applicationRefNumber)
        {

            OfferLetterInfo offerLetter = new OfferLetterInfo();

            var offerLetterDetails = offerLetter.GenerateOfferLetter(applicationRefNumber);
            this.offerLetterReport.LocalReport.DataSources.Clear();
            ReportDataSource dsOfferLetterDetails = new ReportDataSource();
            dsOfferLetterDetails.Value = offerLetterDetails;
            dsOfferLetterDetails.Name = "OfferLetterDetails";

            var loanApplicationDetail = offerLetter.GetLoanApplicationDetail(applicationRefNumber);
            ReportDataSource dsLoanApplicationDetail = new ReportDataSource();
            dsLoanApplicationDetail.Value = loanApplicationDetail;
            dsLoanApplicationDetail.Name = "OfferLetterLoanDetail";

            var conditionPrecident = offerLetter.GetLoanApplicationConditionPrecident(applicationRefNumber);
            ReportDataSource dsConditionPrecident = new ReportDataSource();
            dsConditionPrecident.Value = conditionPrecident;
            dsConditionPrecident.Name = "OfferLetterConditionPrecident";

            var conditionSubsequent = offerLetter.GetLoanApplicationConditionSubsequent(applicationRefNumber);
            ReportDataSource dsConditionSubsequent = new ReportDataSource();
            dsConditionSubsequent.Value = conditionSubsequent;
            dsConditionSubsequent.Name = "OfferLetterConditionSubsequent";

            var fee = offerLetter.GetLoanApplicationFee(applicationRefNumber);
            ReportDataSource dsFee = new ReportDataSource();
            dsFee.Value = fee;
            dsFee.Name = "OfferLetterFee";

            var signatory = offerLetter.GetLoanApplicationSignatory(applicationRefNumber);
            ReportDataSource dsSignatory = new ReportDataSource();
            dsSignatory.Value = signatory;
            dsSignatory.Name = "OfferLetterSignatory";

            var collateral = offerLetter.GetLoanCollateral(applicationRefNumber);
            ReportDataSource dsCollateral = new ReportDataSource();
            dsCollateral.Value = collateral;
            dsCollateral.Name = "OfferLetterCollateral";

            var generateOfferLetter = offerLetter.GenerateOfferLetter(applicationRefNumber);
            ReportDataSource dsGenerateOfferLetter = new ReportDataSource();
            dsGenerateOfferLetter.Value = generateOfferLetter;
            dsGenerateOfferLetter.Name = "OfferLetterBorrowerDetail";

            var getLeaseFacility = offerLetter.GetLeaseFacility(applicationRefNumber);
            ReportDataSource dsGetLeaseFacility = new ReportDataSource();
            dsGetLeaseFacility.Value = getLeaseFacility;
            dsGetLeaseFacility.Name = "LeaseFacility";

            var getDynamics = offerLetter.Los_ConditionDynamics(applicationRefNumber);
            ReportDataSource dsGetDynamics = new ReportDataSource();
            dsGetDynamics.Value = getDynamics;
            dsGetDynamics.Name = "TrasactionDynamics";

            offerLetterReport.LocalReport.DataSources.Add(dsOfferLetterDetails);
            offerLetterReport.LocalReport.DataSources.Add(dsLoanApplicationDetail);
            offerLetterReport.LocalReport.DataSources.Add(dsConditionPrecident);
            offerLetterReport.LocalReport.DataSources.Add(dsConditionSubsequent);
            offerLetterReport.LocalReport.DataSources.Add(dsCollateral);
            offerLetterReport.LocalReport.DataSources.Add(dsFee);
            offerLetterReport.LocalReport.DataSources.Add(dsSignatory);
            offerLetterReport.LocalReport.DataSources.Add(dsCollateral);
            offerLetterReport.LocalReport.DataSources.Add(dsGenerateOfferLetter);
            offerLetterReport.LocalReport.DataSources.Add(dsGetLeaseFacility);
            offerLetterReport.LocalReport.DataSources.Add(dsGetDynamics);

            var reportLink = string.Empty;

            foreach(var x in loanApplicationDetail)
            {
                switch (x.productClassId)
                {
                    case (int)ProductClassEnum.ImportFinanceFacilities:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_ImportFinance.rdlc");
                        break;
                    case (int)ProductClassEnum.EmergingBusiness:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_LeaseFacility.rdlc");
                        break;

                    default: reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter.rdlc");
                        break;
                }
            }

            this.offerLetterReport.LocalReport.ReportPath = reportLink;
            Byte[] mybytes = this.offerLetterReport.LocalReport.Render("PDF");
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < mybytes.Length; i++)
            {
                builder.Append(mybytes[i].ToString("x2"));
            }
            string cflReport = builder.ToString();
            OfferLetterResponse offerLetters = new OfferLetterResponse();
            offerLetters.StatusCode = "00";
            offerLetters.RequestId = "3664925";
            offerLetters.WorkflowStage = "11";
            offerLetters.FileLink = cflReport;
            offerLetters.FileType = "pdf";

            ApiOfferLetterPosting(offerLetters, applicationRefNumber);
            this.offerLetterReport.LocalReport.Refresh();
        }

        public async Task<ResponseMessage> ApiOfferLetterPosting(OfferLetterResponse model, string refNumber)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            OfferLetterResponse responseApi = new OfferLetterResponse();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            string apiUrl = "api/CallBack/notify-status-change";
            try
            {
                var token = new AuthenticationHeaderValue("Basic", API_KEY);
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;


                responseDateTime = DateTime.Now;


                if (response.IsSuccessStatusCode)
                {

                    responseApi = await response.Content.ReadAsAsync<OfferLetterResponse>();

                    var res = new OfferLetterResponse
                    {
                        StatusCode = responseApi.StatusCode,
                        RequestId = responseApi.RequestId,
                        WorkflowStage = responseApi.WorkflowStage,

                    };
                    responseMsg = new ResponseMessage
                    {
                        APIOffetResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();

                responseMsg.responseMessage = responseJson;
                //handler.Dispose();
                //client.Dispose();

                return responseMsg;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;
                //if (responseJson == string.Empty) responseJson = innerExceptionMessage;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = apiUrl,
                    LOGTYPEID = 14,
                    REFERENCENUMBER = refNumber,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
            }

        }


    }
}