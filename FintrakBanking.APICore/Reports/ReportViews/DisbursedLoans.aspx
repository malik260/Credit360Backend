<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisbursedLoans.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.DisbursedLoans" %>

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

        <rsweb:ReportViewer ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="800px" WaitMessageFont-Names="Verdana" 
            WaitMessageFont-Size="14pt" Width="100%" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
            <LocalReport ReportPath="Reports\Report\LoanDisbursement.rdlc" >
                
                <datasources>
                          <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="DisbursedLoan" />
                      </datasources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetDisburstLoans" TypeName="FintrakBanking.ReportObjects.LoanReportObjects" OldValuesParameterFormatString="original_{0}">
            <SelectParameters> 
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="companyId" DefaultValue="" Name="companyId" PropertyName="Text" Type="Int32" />
          
                <asp:ControlParameter ControlID="loanRefNo" Name="loanRefNo" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="branchId" Name="branchId" PropertyName="Text" Type="Int16" />
          
                <asp:ControlParameter ControlID="productClassId" Name="productClassId" PropertyName="Text" Type="Int32" />
          
                <asp:ControlParameter ControlID="staffId" Name="staffId" PropertyName="Text" Type="Int32" />
          
            </SelectParameters>
        </asp:ObjectDataSource>
        </div> 
        <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="companyId" runat="server"  Visible="false" ></asp:Label>
                <asp:Label ID="loanRefNo" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="branchId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="productClassId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="staffId" runat="server"  Visible="false" ></asp:Label>

    </form>
</body>
</html>
