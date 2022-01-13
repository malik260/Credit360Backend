<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoanDocumentDeferalsForMCC.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.LoanDocumentDeferalsForMCC" %>

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

        <rsweb:ReportViewer KeepSessionAlive="false" ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="800px" WaitMessageFont-Names="Verdana" 
            WaitMessageFont-Size="14pt" Width="100%" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
            <LocalReport ReportPath="Reports\Report\LoanDocumentDeferralForMCC.rdlc" >
                
                <datasources>
                          <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="DeferralMCC" />
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="DeferralMCCExp" />
                     <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="WaiverMCC" />
                      </datasources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="LoanDeferralMCCCur" TypeName="FintrakBanking.ReportObjects.LoanReportObjects">
            <SelectParameters> 
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="companyId" DefaultValue="" Name="companyId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="branchCode" Name="branchCode" PropertyName="Text" Type="String" />
          
            </SelectParameters>
        </asp:ObjectDataSource>
               <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="LoanDeferralMCCExp" TypeName="FintrakBanking.ReportObjects.LoanReportObjects">
            <SelectParameters> 
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="companyId" DefaultValue="" Name="companyId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="branchCode" Name="branchCode" PropertyName="Text" Type="String" />
          
            </SelectParameters>
        </asp:ObjectDataSource>
             <asp:ObjectDataSource ID="ObjectDataSource3" runat="server" SelectMethod="LoanDocumentWaivedForMCC" TypeName="FintrakBanking.ReportObjects.LoanReportObjects">
            <SelectParameters> 
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="companyId" DefaultValue="" Name="companyId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="branchCode" Name="branchCode" PropertyName="Text" Type="String" />
          
            </SelectParameters>
        </asp:ObjectDataSource>
        </div>

         <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="companyId" runat="server"  Visible="false" ></asp:Label>
         <asp:Label ID="branchCode" runat="server"  Visible="false" ></asp:Label>
        
    </form>
</body>
</html>
