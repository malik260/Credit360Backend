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
    public partial class InterestIncome : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string startDateString = Request.QueryString["startDate"];
                    DateTime.TryParse(startDateString, out DateTime startDate);
                    //DateTime startDate = DateTime.ParseExact(startDateString, "ddMMyyyyHHmmss", null);

                    string endDateString = Request.QueryString["endDate"];
                    DateTime.TryParse(endDateString, out DateTime endDate);
                    //DateTime endDate = DateTime.ParseExact(endDateString, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    HashHelper hash = new HashHelper();
                    string inputHashValue = Request.QueryString["hashValue"];
                    var incomingDateHash = hash.HashString(startDateString).Replace("-", "");

                    //if (inputHashValue != incomingDateHash)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, true);
                    }

                    RSR psr = new RSR();
                    var interestIncome = psr.GetInterestIncome(startDate, endDate);
                    ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = interestIncome;
                    reportDataSource.Name = "InterestIncome";
                    ReportViewer.LocalReport.DataSources.Add(reportDataSource);                    

                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/InterestIncome.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
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