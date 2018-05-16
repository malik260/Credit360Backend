<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CollateralPropertyRevaluation.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.CollateralPropertyRevaluation" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

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
            <rsweb:ReportViewer ID="collPropRv" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1100px" Height="608px">
                <LocalReport ReportPath="Reports\Report\CollateralPropertyRevaluation.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="odsCollPropRev" Name="CollateralPropertyDetails" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
        </div>
          <asp:Label ID="value" runat="server" Visible="false" ></asp:Label>
        <asp:ObjectDataSource ID="odsCollPropRev" runat="server" SelectMethod="CollateralPropertyRevaluation" TypeName="FintrakBanking.ReportObjects.ReportingObjects.LimitsMonitoringReportsObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </form>
</body>
     <asp:Label ID="startDate" runat="server" Visible ="false"  ></asp:Label>
     <asp:Label ID="endDate" runat="server" Visible ="false"  ></asp:Label>
</html>
