<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SeftLiquidatingLoans.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.SeftLiquidatingLoans" %>
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
        <rsweb:ReportViewer ID="nplLoanRv" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1100px">
            <LocalReport ReportPath="Reports\Report\SelfLiquidatingLoan.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsSelfLiquidatingLoans" Name="LiquidatingLoan" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsSelfLiquidatingLoans" runat="server" SelectMethod="SelfLiquidatingLoan" TypeName="FintrakBanking.ReportObjects.ReportingObjects.LimitsMonitoringReportsObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
        <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>
         <asp:Label ID="classification" runat="server" Visible="false" ></asp:Label>
    </form>
</body>

</html>
