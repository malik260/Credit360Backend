using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ReportObjects.Credit;
using FintrakBanking.ReportObjects.ReportingObjects;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.Reporting.WebForms;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class OutPutTemplate : System.Web.UI.Page
    {   

        Warning[] warnings;
        string[] streamIds;
        string contentType;
        string encoding;
        string extension;

        protected void Page_Load(object sender, EventArgs e)
         {
            if (!IsPostBack)
            {

                //var queryString = Request.QueryString["loanApplicationId"];

                //if (!String.IsNullOrEmpty(queryString))
                //{
                //    int loanApplicationId = Convert.ToInt32(queryString);
                //    GenerateOutPutDocument(loanApplicationId);
                //}
            }
        }

        void GenerateOutPutDocument(int loanApplicationId) {

            OutputDocument outputDocument = new OutputDocument();
                 
            var fee =  outputDocument.GetFee(loanApplicationId);
            this.ReportViewer.LocalReport.DataSources.Clear();
            ReportDataSource dsFee = new ReportDataSource();
            dsFee.Value = fee;
            dsFee.Name = "Fee";

            var monthActivitySignature = outputDocument.GetMonthsActivity(loanApplicationId);
            ReportDataSource dsMonthsActivity = new ReportDataSource();
            dsMonthsActivity.Value = monthActivitySignature;
            dsMonthsActivity.Name = "MonthActivitySign";

            var approval = outputDocument.GetApplicationApproval(loanApplicationId);
            ReportDataSource dsApproval = new ReportDataSource();
            dsApproval.Value = approval;
            dsApproval.Name = "Approval";

            var checklist = outputDocument.GetChecklist(loanApplicationId);
            ReportDataSource dsChecklist = new ReportDataSource();
            dsChecklist.Value = checklist;
            dsChecklist.Name = "Checklist";

            var collateral = outputDocument.GetCollateral(loanApplicationId);
            ReportDataSource dsCollateral = new ReportDataSource();
            dsCollateral.Value = collateral;
            dsCollateral.Name = "Collateral";

            var concurrences = outputDocument.GetConcurrences(loanApplicationId);
            ReportDataSource dsConcurrences = new ReportDataSource();
            dsConcurrences.Value = concurrences;
            dsConcurrences.Name = "Concurrences";

            var facilities = outputDocument.GetCustomerFacilities(loanApplicationId);
            ReportDataSource dsFacilities = new ReportDataSource();
            dsFacilities.Value = facilities;
            dsFacilities.Name = "CustomerFacilities";

            var information = outputDocument.GetCustomerInformation(loanApplicationId);
            ReportDataSource dsInformation = new ReportDataSource();
            dsInformation.Value = information;
            dsInformation.Name = "CustomerInformation";

            var activitty = outputDocument.GetMonthsActivity(loanApplicationId);
            ReportDataSource dsActivity = new ReportDataSource();
            dsActivity.Value = activitty;
            dsActivity.Name = "MonthsActivity";

            var summary = outputDocument.GetSummary(loanApplicationId);
            ReportDataSource dssummary = new ReportDataSource();
            dssummary.Value = summary;
            dssummary.Name = "Summary";

            var currentRequest = outputDocument.GetCurrentRequest(loanApplicationId);
            ReportDataSource dsCurrentRequest = new ReportDataSource();
            dsCurrentRequest.Value = currentRequest;
            dsCurrentRequest.Name = "CurrentRequest";

            ReportViewer.LocalReport.DataSources.Add(dsFee);
            ReportViewer.LocalReport.DataSources.Add(dsMonthsActivity);
            ReportViewer.LocalReport.DataSources.Add(dsApproval);
            ReportViewer.LocalReport.DataSources.Add(dsChecklist);
            ReportViewer.LocalReport.DataSources.Add(dsCollateral);
            ReportViewer.LocalReport.DataSources.Add(dsConcurrences);
            ReportViewer.LocalReport.DataSources.Add(dsFacilities);
            ReportViewer.LocalReport.DataSources.Add(dsInformation);
            ReportViewer.LocalReport.DataSources.Add(dsActivity);
            ReportViewer.LocalReport.DataSources.Add(dsCurrentRequest);
            ReportViewer.LocalReport.DataSources.Add(dssummary);

            this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/OutputDocument.rdlc");

            //Export the RDLC Report to Byte Array.
            byte[] bytes = ReportViewer.LocalReport.Render("WORD", null, out contentType, out encoding, out extension, out streamIds, out warnings);

            // #########   CODE TO DOWNLOAD FILE
            //Response.Clear();
            //Response.Buffer = true;
            //Response.Charset = "";
            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //Response.ContentType = contentType;
            //Response.AppendHeader("Content-Disposition", "attachment; filename=RDLC." + extension);
            //Response.BinaryWrite(bytes);
            //Response.Flush();
            //Response.End();

            var model = new DocumentUploadViewModel
            {
                targetId = loanApplicationId,
                operationId = facilities != null ? facilities.FirstOrDefault().operationId : 0,
                customerId = facilities != null ? facilities.FirstOrDefault().customerId : 0,
                overwrite = true,
                fileExtension = extension,
                customerCode = facilities != null ? facilities.FirstOrDefault().customerCode : "",
                fileName = facilities != null ? facilities.FirstOrDefault().applicationReferenceNumber : "",
                createdBy = facilities != null ? facilities.FirstOrDefault().createdBy : 0,
                documentCode = facilities != null ? "OP/" + facilities.FirstOrDefault().applicationReferenceNumber : "",
                documentTitle = facilities != null ? "OutPut_Document_" + facilities.FirstOrDefault().applicationReferenceNumber : "",
                targetReferenceNumber = facilities != null ? facilities.FirstOrDefault().targetReferenceNumber : "",
                documentTypeId = (int)DocumentTypeEnum.LoanApplicationApprovalOuputDocument
            };
            outputDocument.AddDocumentUpload(model, bytes);
        }
     }
}