using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using Microsoft.Reporting.WebForms;
using System;

namespace FintrakBanking.APICore.Reports.Credit
{
    public partial class TestReport : System.Web.UI.Page
    {
        private ApiControllerBase authCtrl = new ApiControllerBase();

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!IsPostBack)
            //{
            //    //ReportParameter date = new ReportParameter("branchName", Request.QueryString["branchName"]);
            //    //rvBranchReport.LocalReport.SetParameters(new ReportParameter[] { date });
            //    rvBranchReport.LocalReport.Refresh();

            //}
        }

    }

}