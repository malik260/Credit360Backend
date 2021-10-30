<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RiskAssetTeam.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.RiskAssetTeam" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
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
            <rsweb:ReportViewer ID="ReportViewer" runat="server" Width="100%" Height="100%" ShowParameterPrompts="false" SizeToReportContent="true" ShowPageNavigationControls="true" KeepSessionAlive="true">
            </rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
