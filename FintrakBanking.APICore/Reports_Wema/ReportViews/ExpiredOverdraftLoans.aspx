<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpiredOverdraftLoans.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.ExpiredOverdraftLoans" %>

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
        <rsweb:ReportViewer KeepSessionAlive="false" ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1100px">
            <LocalReport ReportPath="Reports\Report\ExpiredOverdraftLoans.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsExpODraftLoans" Name="OverdraftLoanDetails" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsExpODraftLoans" runat="server" SelectMethod="ExpiredOverDraftLoans" TypeName="FintrakBanking.ReportObjects.Credit.LoanMonitoring">
            <SelectParameters>
                <asp:QueryStringParameter Name="companyId" QueryStringField="companyId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
