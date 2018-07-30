using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanRepaymentSchedule : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                tearmLoanId.Text = Request.QueryString["tearmLoanId"];
                companyId.Text = Request.QueryString["companyId"];
                staffId.Text = Request.QueryString["staffId"];
                
            }
        }
    }
}