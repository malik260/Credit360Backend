<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CollateralPropertyRevaluation.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.CollateralPropertyRevaluation" %>

 <%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

 <%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

 <!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="width: 795px">
    
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <rsweb:ReportViewer ID="collPropRv" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="751px">
            <LocalReport ReportPath="Reports\Credit\Monitoring\CollateralPropertyRevaluation.rdlc">
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsCollPropRv" runat="server" SelectMethod="CollateralPropertyRevaluation" TypeName="FintrakBanking.ReportObjects.Credit.LoanMonitoring"></asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
