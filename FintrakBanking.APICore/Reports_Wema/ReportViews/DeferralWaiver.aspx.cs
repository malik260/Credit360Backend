using FintrakBanking.Common.Extensions;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FintrakBanking.ReportObjects;
using FintrakBanking.ReportObjects.ReportingObjects;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class DeferralWaiver : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string operationId = Request.QueryString["operationId"];
                    string targetId = Request.QueryString["targetId"];
                    string staffId = Request.QueryString["staffId"];
                    string loanApplicationDetailId = Request.QueryString["loanApplicationDetailId"];
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

                    DEFERRALWAIVER deferralWaiver = new DEFERRALWAIVER();

                    var data = deferralWaiver.GetDeferralWaiver(Int32.Parse(staffId), Int32.Parse(operationId), Int32.Parse(loanApplicationDetailId));
                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "DeferralWaiver";
                    ReportViewer.LocalReport.DataSources.Add(reportDataSource);

                    var conditions = deferralWaiver.GetChecklistAwaitingApproval(Int32.Parse(staffId), Int32.Parse(operationId), Int32.Parse(loanApplicationDetailId));
                    ReportDataSource reportDataSourceCon = new ReportDataSource();
                    reportDataSourceCon.Value = conditions;
                    reportDataSourceCon.Name = "conditions";
                    ReportViewer.LocalReport.DataSources.Add(reportDataSourceCon);

                    var approvals = deferralWaiver.GetDeferralnAprroval(Int32.Parse(operationId), Int32.Parse(targetId));
                    ReportDataSource reportDataSourceApp = new ReportDataSource();
                    reportDataSourceApp.Value = approvals;
                    reportDataSourceApp.Name = "approvals";
                    ReportViewer.LocalReport.DataSources.Add(reportDataSourceApp);

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, true);
                    }

                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/DeferralWaiver.rdlc");
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