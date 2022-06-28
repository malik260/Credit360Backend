<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CorporateCustomerCreation.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.CorporateCustomerCreation" %>
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
        </div>
        <rsweb:ReportViewer KeepSessionAlive="false" ID="ReportViewer" runat="server" Height="800px" Width="100%">
        </rsweb:ReportViewer>
    </form>
</body>
</html>

