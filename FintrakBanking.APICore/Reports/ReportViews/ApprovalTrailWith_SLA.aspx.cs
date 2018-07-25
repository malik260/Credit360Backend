using FintrakBanking.ReportObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class ApprovalTrailWith_SLA : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

               int operationId = Int32.Parse( Request.QueryString["operationId"]);
               int targetId = Int32.Parse(Request.QueryString["loanApplicationId"]);
               int companyId = Int32.Parse(Request.QueryString["companyId"]);
              int  staffId = Int32.Parse(Request.QueryString["staffId"]);

                WorkFlowDesign workFlow = new WorkFlowDesign();
             var data =   workFlow.TrackWorkFlow(operationId, companyId, targetId, staffId);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "WorkFlowSLA";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/ApprovalTrailWithSLA.rdlc");
                this.ReportViewer.LocalReport.Refresh();

            }
        }
    }
}