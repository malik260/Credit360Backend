using FintrakBanking.Common.Extensions;
using FintrakBanking.ReportObjects.ReportingObjects;
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
    public partial class AllContingentsReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                    int reportType = int.Parse(Request.QueryString["reportAllType"]);
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

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, false);
                    }

                    LimitsMonitoringReportsObjects sla = new LimitsMonitoringReportsObjects();
                    if (reportType == 1)
                    {
                        var data = sla.ContingentRebookingReport(startDate, endDate);
                        this.ReportViewer.LocalReport.DataSources.Clear();
                        ReportDataSource reportDataSource = new ReportDataSource();
                        reportDataSource.Value = data;
                        reportDataSource.Name = "Rebooking";

                        this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Rebooking.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                    }
                    if (reportType == 2)
                    {
                        var data = sla.ContingentAmortizationReport(startDate, endDate);
                        this.ReportViewer.LocalReport.DataSources.Clear();
                        ReportDataSource reportDataSource = new ReportDataSource();
                        reportDataSource.Value = data;
                        reportDataSource.Name = "Amortization";

                        this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Amortization.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                    }
                    if (reportType == 3)
                    {
                        var data = sla.ContingentDischargeReport(startDate, endDate);
                        this.ReportViewer.LocalReport.DataSources.Clear();
                        ReportDataSource reportDataSource = new ReportDataSource();
                        reportDataSource.Value = data;
                        reportDataSource.Name = "Discharge";

                        this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Discharge.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                    }

                    
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