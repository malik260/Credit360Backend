<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CovenantsApproachingDueDate.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.Monitoring.CovenantsApproachingDueDate" %>

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
        <rsweb:ReportViewer ID="covDueDateRv" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1100px">
            <LocalReport ReportPath="Reports\Report\CovenantsApproachingDueDate.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsCovDueDate" Name="CovenantDetails" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsCovDueDate" runat="server" SelectMethod="CovenantsApproachingDueDate" TypeName="FintrakBanking.ReportObjects.ReportingObjects.LimitsMonitoringReportsObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
                 <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>

    </form>
</body>
</html>
