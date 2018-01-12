<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GetStakeholdersOnExpirationOfFTP.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.GetStakeholdersOnExpirationOfFTP" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
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

            <rsweb:ReportViewer ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="800px" WaitMessageFont-Names="Verdana"
                WaitMessageFont-Size="14pt" Width="100%" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
                <LocalReport ReportPath="Reports\Report\StakeholderWithExpiredFTP.rdlc">

                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="Stakeholder" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>

            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetStakeHolderOnExperationOfFTP" TypeName="FintrakBanking.ReportObjects.LoanReportObjects" OldValuesParameterFormatString="original_{0}">
                <SelectParameters>
                    <asp:ControlParameter ControlID="branchId" Name="branchId" PropertyName="Text" Type="Int16" />
                    <asp:ControlParameter ControlID="customerName" Name="customerName" PropertyName="Text" Type="String" />
                    <asp:ControlParameter ControlID="startDate" Name="maturityDate" PropertyName="Text" Type="DateTime" />
                </SelectParameters>
            </asp:ObjectDataSource>
        </div>
        <asp:Label ID="customerName" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="companyId" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="branchId" runat="server" Visible="false"></asp:Label>\
        <asp:Label ID="startDate" runat="server" Visible="false"></asp:Label>
    </form>
</body>
</html>
