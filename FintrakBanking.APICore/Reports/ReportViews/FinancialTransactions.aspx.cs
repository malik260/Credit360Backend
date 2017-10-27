using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class FinancialTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            StartDate.Text = Request.QueryString[""];
            EndDate.Text = Request.QueryString[""];
            PostedByStaffId.Text = Request.QueryString[""];
        }
    }
}