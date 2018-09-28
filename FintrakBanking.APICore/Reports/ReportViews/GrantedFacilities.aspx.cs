
using FintrakBanking.Common.Extensions;
using FintrakBanking.ReportObjects.ReportingObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class PostedFinancialTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    int? branchId = 0;
                    int? PostedByStaffId = 0;
                    string imagePath = new Uri(Server.MapPath("~/Content/icons/firstbank-logo.jpg")).AbsoluteUri;
                    DateTime StartDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    DateTime EndDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                    int glAccountId = int.Parse(Request.QueryString["glAccountId"]);
                    int companyId = int.Parse(Request.QueryString["companyId"]);
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    if (inputHashValue != incomingDateHash)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }

                    var currentDate = DateTime.Now;

                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 30)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }
                    var staffId = Request.QueryString["PostedByStaffId"];
                    if (staffId != null && staffId != "")
                        PostedByStaffId = int.Parse(Request.QueryString["PostedByStaffId"]);
                    string branch = Request.QueryString["branchId"];
                    if (branch != null && branch != "")
                        branchId = Int32.Parse(Request.QueryString["branchId"]);

                    FinanceRepotObject sla = new FinanceRepotObject();
                    var data = sla.FinanceTransaction(StartDate, EndDate, companyId, branchId, glAccountId, PostedByStaffId);

                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "FinanceTransactions";

                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/FinanceTransactions.rdlc");

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, false);
                    }

                    ReportParameter sDate = new ReportParameter("StartDate", StartDate.ToString());
                    ReportParameter eDate = new ReportParameter("EndDate", EndDate.ToString());
                    ReportParameter logoPath = new ReportParameter("logo", imagePath);

                    this.ReportViewer.LocalReport.EnableExternalImages = true;
                    ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate, logoPath });
                    ReportViewer.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
                    return;
                }
            }
        }
    }
}