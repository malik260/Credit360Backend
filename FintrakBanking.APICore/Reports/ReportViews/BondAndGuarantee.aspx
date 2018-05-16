<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BondAndGuarantee.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.BondAndGuarantee" %>
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
        <rsweb:ReportViewer ID="expOverDraftLoansRv" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1100px">
            <LocalReport ReportPath="Reports\Report\BondAndGuarantee.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsExpODraftLoans" Name="DataSet1" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsExpODraftLoans" runat="server" SelectMethod="BondAndGuarantee" TypeName="FintrakBanking.ReportObjects.ReportingObjects.LimitsMonitoringReportsObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
                                <asp:ControlParameter ControlID="status" Name="status" PropertyName="Text" Type="Int32" />

            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
    </form>
</body>
            <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>
    <asp:Label ID="status" runat="server" Visible="false" ></asp:Label>

</html>
