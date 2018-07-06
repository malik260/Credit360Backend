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
            StartDate.Text = Request.QueryString["StartDate"];
            EndDate.Text = Request.QueryString["EndDate"];
            PostedByStaffId.Text = Request.QueryString["PostedByStaffId"];
            glAccountId.Text = Request.QueryString["glAccountId"];
            branchId.Text = Request.QueryString["branchId"];
            companyId.Text = Request.QueryString["companyId"];
        }
    }
}