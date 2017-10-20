<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SelfLiquidatingLoanExpiry.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.SelfLiquidatingLoanExpiry" %>

 <%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

 <%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

 <!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="width: 796px; height: 644px">
    
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="SelfLiquidatingLoanExpiry" TypeName="FintrakBanking.ReportObjects.Credit.LoanMonitoring"></asp:ObjectDataSource>
        <rsweb:ReportViewer ID="selfLiqLoanExpRv" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="751px">
            <LocalReport ReportPath="Reports\Credit\Monitoring\SelfLiquidatingLoanExpiry.rdlc">
            </LocalReport>
        </rsweb:ReportViewer>
    
    </div>
    </form>
</body>
</html>
