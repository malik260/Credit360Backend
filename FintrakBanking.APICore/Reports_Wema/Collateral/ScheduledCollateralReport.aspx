<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ScheduledCollateralReport.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Collateral.ScheduledCollateralReport" %>

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
                <LocalReport ReportPath="Reports\Collateral\ScheduledCollateral.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="odsCollPropRev" Name="CollateralPropertyDetails" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
        </div>
        <asp:ObjectDataSource ID="odsCollPropRev" runat="server" SelectMethod="ScheduledCollateralData" TypeName="FintrakBanking.ReportObjects.ReportingObjects.ScheduledCollateralObject">
            <SelectParameters>
                <asp:QueryStringParameter Name="startDate" QueryStringField="startDate" Type="DateTime" />
                <asp:QueryStringParameter Name="endDate" QueryStringField="startDate" Type="DateTime" />
                <asp:QueryStringParameter Name="companyId" QueryStringField="companyId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </form>
</body>
</html>
