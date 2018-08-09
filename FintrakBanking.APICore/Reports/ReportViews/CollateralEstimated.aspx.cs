using FintrakBanking.Common.Extensions;
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
    public partial class CollateralEstimated : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {

                    //    collateralCode.Text = Request.QueryString["startDate"];
                    //    acctNumber.Text = Request.QueryString["endDate"];
                    //    companyId.Text = Request.QueryString["companyId"];

                    //    ReportParameter cCode = new ReportParameter("collateralCode", collateralCode.Text);
                    //    ReportParameter aNumber = new ReportParameter("acctNumber", acctNumber.Text);

                    //    ReportViewer.LocalReport.SetParameters(new ReportParameter[] { cCode, aNumber });
                    //    ReportViewer.LocalReport.Refresh();
                    //}
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

                    if (dateDifference.Seconds > 10)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }

                    if (!IsPostBack)
                    {
                        companyId.Text = Request.QueryString["companyId"]; //"1",

                        collateralCode.Text = Request.QueryString["collateralCode"]; //"252"; 

                        ReportViewer.LocalReport.Refresh();

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