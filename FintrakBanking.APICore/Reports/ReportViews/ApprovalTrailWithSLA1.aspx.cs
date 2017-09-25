using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
	public partial class ApprovalTrailWithSLA1 : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            if (!IsPostBack)
            {
                //ReportParameter operationId = new ReportParameter("operationId", Request.QueryString["operationId"]);
                //ReportParameter targetId = new ReportParameter("targetId", Request.QueryString["loanApplicationId"]);
                //ReportParameter companyId = new ReportParameter("companyId", Request.QueryString["companyId"]);
                //approvalTrailWithSLA.LocalReport.SetParameters(new ReportParameter[] { operationId,targetId, companyId });
                //approvalTrailWithSLA.LocalReport.Refresh();

            }

        }
	}
}