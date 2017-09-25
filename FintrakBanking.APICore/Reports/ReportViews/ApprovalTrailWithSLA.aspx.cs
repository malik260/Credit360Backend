using FintrakBanking.Entities.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit
{
    public partial class ApprovalTrailWithSLA : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                operationId.Text = Request.QueryString["operationId"];
                targetId .Text = Request.QueryString["loanApplicationId"];
                companyId.Text = Request.QueryString["companyId"];

                //ReportViewer.LocalReport.DataSources.Clear();
                //// ReportViewer.ProcessingMode = ProcessingMode.Local;
                //ReportViewer.Attributes.Add("style", "margin - bottom: 30px;");
                //ReportViewer.ShowParameterPrompts = false;
                //ReportParameter[] reportParameterList = new ReportParameter[3];
                //ObjectDataSource1.TypeName = "FintrakBanking.ReportObjects.TrackWorkFlow";
                //ObjectDataSource1.SelectParameters.Clear();

                //ReportViewer.LocalReport.DataSources.Clear();
                //reportParameterList[0] = new ReportParameter("operationId", Request.QueryString["operationId"]);
                //reportParameterList[1] = new ReportParameter("targetId", Request.QueryString["loanApplicationId"]);
                //reportParameterList[2] = new ReportParameter("companyId", Request.QueryString["companyId"]);

                //ObjectDataSource1.SelectParameters.Add("operationId", Request.QueryString["operationId"]);
                //ObjectDataSource1.SelectParameters.Add("targetId",    Request.QueryString["loanApplicationId"]);
                //ObjectDataSource1.SelectParameters.Add("companyId",    Request.QueryString["companyId"]);

                //ObjectDataSource1.SelectMethod = "TrackWorkFlow";

                //var rptDataSource = new ReportDataSource();

                //rptDataSource.DataSourceId = "ObjectDataSource1";


                //ReportViewer.LocalReport.DataSources.Add(rptDataSource);
                //ReportViewer.LocalReport.SetParameters(reportParameterList);
                //ReportViewer.DataBind();
                ReportViewer.LocalReport.Refresh();

                //ReportParameter operationId = new ReportParameter("operationId", Request.QueryString["operationId"]);
                //ReportParameter targetId = new ReportParameter("targetId", Request.QueryString["loanApplicationId"]);
                //ReportParameter companyId = new ReportParameter("companyId", Request.QueryString["companyId"]);
                //offerLetter.LocalReport.SetParameters(new ReportParameter[] { operationId, targetId, companyId });
                // offerLetter.LocalReport.Refresh();
            }
        }
    }
}