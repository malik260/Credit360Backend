using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common.Extensions;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.Credit;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
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
    public partial class CFLOfferLetter : System.Web.UI.Page
    {        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string applicationRefNumber = Request.QueryString["applicationRefNumber"];
                    string RequestId = Request.QueryString["RequestId"];
                    string WorkflowStage = Request.QueryString["WorkflowStage"];
                    string ReasonForRejection = Request.QueryString["ReasonForRejection"];
                    string ActionByName = Request.QueryString["ActionByName"];
                    string StatusCode = Request.QueryString["StatusCode"];

                    GenerateOutPutDocument(StatusCode, applicationRefNumber, RequestId, WorkflowStage, ReasonForRejection, ActionByName);

                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.Message);
                    this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.offerLetterReport.LocalReport.Refresh();
                    return;
                }
            }
        }

        void GenerateOutPutDocument(string StatusCode, string applicationRefNumber, string RequestId, string WorkflowStage, string ReasonForRejection,
                                    string ActionByName)
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
            dsGetDynamics.Name = "OfferLetterTransactionDynamics";

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

            foreach (var x in loanApplicationDetail)
            {
                switch (x.productClassId)
                {
                    case (int)ProductClassEnum.ImportFinanceFacilities:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_ImportFinance.rdlc");
                        break;
                    case (int)ProductClassEnum.EmergingBusiness:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_LeaseFacility.rdlc");
                        break;
                    case (int)ProductClassEnum.MHSS:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_MHSS.rdlc");
                        break;

                    default:
                        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter.rdlc");
                        break;
                }
            }

            this.offerLetterReport.LocalReport.ReportPath = reportLink;
            if (WorkflowStage == "14")
            {
                Byte[] mybytes = this.offerLetterReport.LocalReport.Render("PDF");
                StringBuilder builder = new StringBuilder();
                FinTrakBankingContext context = new FinTrakBankingContext();
                TransactionPosting transaction = new TransactionPosting(context);

                //for (int i = 0; i < mybytes.Length; i++)
                //{
                //    //builder.Append(mybytes[i].ToString("x2"));
                //    builder.Append(System.Convert.ToBase64String(mybytes));
                //}

                builder.Append(Convert.ToBase64String(mybytes));

                string cflReport = builder.ToString();
                OfferLetterResponse offerLetters = new OfferLetterResponse();
                offerLetters.StatusCode = StatusCode;
                offerLetters.RequestId = RequestId;
                offerLetters.WorkflowStage = WorkflowStage;
                offerLetters.Attachment.FileLink = cflReport;
                offerLetters.Attachment.FileType = "pdf";
                offerLetters.ReasonForRejection = ReasonForRejection;
                offerLetters.ActionByName = ActionByName;
                transaction.ApiOfferLetterPosting(offerLetters, applicationRefNumber);
            }
            this.offerLetterReport.LocalReport.Refresh();
        }

    }
}