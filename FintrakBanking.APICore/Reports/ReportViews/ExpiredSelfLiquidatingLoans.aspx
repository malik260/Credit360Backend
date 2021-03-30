<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpiredSelfLiquidatingLoans.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.ExpiredSelfLiquidatingLoans" %>

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
        <rsweb:ReportViewer KeepSessionAlive="false" ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1300px">
            <LocalReport ReportPath="Reports\Report\ExpiredSelfLiquidatingLoans.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsSelfLiqLoanExp" Name="SelfLiqLoanDetails" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsSelfLiqLoanExp" runat="server" SelectMethod="SelfLiquidatingLoanExpiry" TypeName="FintrakBanking.ReportObjects.Credit.LoanMonitoring">
            <SelectParameters>
                <asp:QueryStringParameter Name="companyId" QueryStringField="companyId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
