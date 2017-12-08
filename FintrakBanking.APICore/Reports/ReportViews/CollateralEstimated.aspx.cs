using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
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
            //if (!IsPostBack)
            //{
            //    collateralCode.Text = Request.QueryString["startDate"];
            //    acctNumber.Text = Request.QueryString["endDate"];
            //    companyId.Text = Request.QueryString["companyId"];

            //    ReportParameter cCode = new ReportParameter("collateralCode", collateralCode.Text);
            //    ReportParameter aNumber = new ReportParameter("acctNumber", acctNumber.Text);

            //    ReportViewer.LocalReport.SetParameters(new ReportParameter[] { cCode, aNumber });
            //    ReportViewer.LocalReport.Refresh();
            //}

            if (!IsPostBack)
            {
                companyId.Text = Request.QueryString["companyId"]; //"1",

                collateralCode.Text = Request.QueryString["collateralCode"]; //"252"; 

                ReportViewer.LocalReport.Refresh();

            }
        }
    }
}