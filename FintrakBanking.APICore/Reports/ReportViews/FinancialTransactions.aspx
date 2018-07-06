<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FinancialTransactions.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.FinancialTransactions" %>
 
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

              <rsweb:ReportViewer ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="1000px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" BackColor=""
                  ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" 
                  LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor=""
                  PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" 
                  SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" 
                  ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
                  <localreport reportpath="Reports\Report\FinanceTransactions.rdlc">
                      <datasources>
                          <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="FinanceTransactions" />
                      </datasources>
                  </localreport>
           
        </rsweb:ReportViewer>
             <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="FinanceTransaction" TypeName="FintrakBanking.ReportObjects.ReportingObjects.FinanceRepotObject">
                 <SelectParameters>
                     <asp:ControlParameter ControlID="StartDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                     <asp:ControlParameter ControlID="EndDate" Name="endDate" PropertyName="Text" Type="DateTime" />
                     <asp:ControlParameter ControlID="branchId" Name="branchId" PropertyName="Text" Type="Int32" />
                     <asp:ControlParameter ControlID="glAccountId" Name="glAccountId" PropertyName="Text" Type="Int32" />
                     <asp:ControlParameter ControlID="PostedByStaffId" Name="PostedByStaffId" PropertyName="Text" Type="Int32" />
                     <asp:ControlParameter ControlID="companyId" Name="companyId" PropertyName="Text" Type="Int32" />
                 </SelectParameters>
             </asp:ObjectDataSource>
        </div>
        <asp:Label ID="StartDate" runat="server" Visible="false"  ></asp:Label>
        <asp:Label ID="EndDate" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="PostedByStaffId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="glAccountId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="branchId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="companyId" runat="server"  Visible="false" ></asp:Label>
    </form>
</body>
</html>
