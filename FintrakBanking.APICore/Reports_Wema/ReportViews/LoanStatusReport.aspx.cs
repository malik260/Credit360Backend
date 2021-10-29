using FintrakBanking.Common.Extensions;
using FintrakBanking.ReportObjects;
using FintrakBanking.ReportObjects.ReportingObjects;
using FintrakBanking.ViewModels.Reports;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanStatusReport : System.Web.UI.Page
    {
        LoanReportObjects Repo = new LoanReportObjects();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                    string  ReportType = Request.QueryString["ReportType"];
                    int companyId = Int32.Parse(Request.QueryString["companyId"]);
                    short branchId = short.Parse(Request.QueryString["branchId"]);
                    string loanRefNo = Request.QueryString["loanRefNo"];
                    int productClassId = Int32.Parse(Request.QueryString["productClassId"]);
                    int? staffId = Int32.Parse(Request.QueryString["staffId"]);
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    //DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    //var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    //if (inputHashValue != incomingDateHash)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    //var currentDate = DateTime.Now;

                    //var dateDifference = currentDate - incomingDate;

                    //if (dateDifference.Seconds > 30)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}
                   
                   
                    
                    string rdlcUrl="",rdlcSourceName = "";
                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();


                    if (ReportType == "DisbursedLoansReport")
                    {
                     var data1=  GetDisbursedLoans(ref rdlcUrl,ref rdlcSourceName,startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }
                    else if (ReportType == "TerminatedLoansReport")
                    {

                        var data1 = GetTerminatedLoans(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }

                    else if (ReportType == "InitiatedLoansReport") {
                        var data1 = GetInitiatedLoans(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }
                    else if (ReportType == "MaturedLoansReport") {
                        var data1 = GetMaturedLoans(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }
                    
                    else if (ReportType == "ApprovedLoansReport")
                    {
                        var data1 = GetApprovedLoans(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }
                    else if (ReportType == "CancelledLoansReport")
                    {
                        var data1 = GetCancelledLoans(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }
                    
                    else if (ReportType == "CustomerListReport")
                    {
                        var data1 = GetListOfCustomers(ref rdlcUrl, ref rdlcSourceName, startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                        reportDataSource.Value = data1;
                    }

                    // reportDataSource.Value = Disburseddata;
                    // rdlcUrl = Server.MapPath("~/Reports/Report/InitiatedLoans.rdlc");
                    //   rdlcSourceName = "InitiatedLoans";

                    //var data = Repo.GetInitiatedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);
                    //reportDataSource.Value = data;
                    reportDataSource.Name = rdlcSourceName;

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, false);
                    }
                    ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());
                    ReportParameter eDate = new ReportParameter("endDate", endDate.ToString());

                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    
                    this.ReportViewer.LocalReport.ReportPath = rdlcUrl;
                    //ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                    this.ReportViewer.LocalReport.Refresh();


                }
                catch (Exception ex)
                {
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
                    return;
                }
            }
              IEnumerable<MarturedLoansViewModel> GetMaturedLoans(ref string rdlcUrl, ref string rdlcSourceName,DateTime startDate, DateTime endDate,int companyId, string loanRefNo, short  branchId, int  productClassId,int? staffId)
            {
                rdlcSourceName = "MaturedLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/MaturedLoans.rdlc");
                
               return this.Repo.GetMaturedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
            IEnumerable<ApprovedLoansViewModel> GetApprovedLoans(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "ApprovedLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/ApprovedLoans.rdlc");

                return this.Repo.GetApprovedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
            IEnumerable<ApprovedLoansViewModel> GetCancelledLoans(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "CancelledLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/CancelledLoans.rdlc");

                return this.Repo.GetCancelledLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
            
            IEnumerable<InitiatedLoansViewModel> GetInitiatedLoans(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "InitiatedLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/InitiatedLoans.rdlc");

                return this.Repo.GetInitiatedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
            IEnumerable<TerminatedLoansViewModel> GetTerminatedLoans(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "TerminatedLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/TerminatedLoans.rdlc");

                return this.Repo.GetTerminatedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
            IEnumerable<CustomerViewModel> GetListOfCustomers(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "ListOfCustomers";
                rdlcUrl = Server.MapPath("~/Reports/Report/ListOfCustomers.rdlc");

                return this.Repo.GetListOfCustomers(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
           
            IEnumerable<DisburstLoanViewModel> GetDisbursedLoans(ref string rdlcUrl, ref string rdlcSourceName, DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short branchId, int productClassId, int? staffId)
            {
                rdlcSourceName = "DisbursedLoans";
                rdlcUrl = Server.MapPath("~/Reports/Report/DisbursedLoans.rdlc");

                return this.Repo.GetDisbursedLoans(startDate, endDate, companyId, loanRefNo, branchId, productClassId, staffId);

            }
        }
    }
}