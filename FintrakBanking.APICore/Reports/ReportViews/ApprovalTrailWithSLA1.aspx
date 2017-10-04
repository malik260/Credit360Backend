<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApprovalTrailWithSLA1.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.ApprovalTrailWithSLA" %>


 <%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
 <%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
            </asp:ScriptManager>
            <rsweb:ReportViewer ID="approvalTrailWithSLA" runat="server" Font-Names="Verdana" Font-Size="8pt"  WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt"
                  BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" 
                InternalBorderStyle="Solid"   InternalBorderWidth="1px" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px"  
               Width="1500px" Height="700px" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px"
                ToolBarItemPressedHoverBackColor="153, 187, 226">
                <LocalReport ReportPath="Reports\Report\ApprovalTrailWithSLA.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="DataSet1" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>


        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="TrackWorkFlow" TypeName="FintrakBanking.ReportObjects.WorkFlowDesign">
            <SelectParameters>
                <asp:QueryStringParameter DefaultValue="6" Name="operationId" QueryStringField="operationId" Type="Int32" />
                <asp:QueryStringParameter DefaultValue="1" Name="companyId" QueryStringField="companyId" Type="Int32" />
                <asp:QueryStringParameter DefaultValue="1" Name="targetId" QueryStringField="targetId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    

        </div>
    </form>
</body>
</html>
