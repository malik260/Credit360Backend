<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SLAReport.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.SLAReport" %>

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
            <LocalReport ReportPath="Reports\Report\SLAMonitoringReport.rdlc" >
                
                <datasources>
                      <rsweb:ReportDataSource DataSourceId="ObjectDataSourceSLA" Name="SLATracker" />
                      <rsweb:ReportDataSource DataSourceId="odsLogo" Name="logo" />
                      </datasources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSourceSLA" runat="server" SelectMethod="SLAMonitoring" TypeName="FintrakBanking.ReportObjects.ReportingObjects.LimitsMonitoringReportsObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="approvalStatus" Name="approvalStatus" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="operationId" Name="operationId" PropertyName="Text" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
            <asp:ObjectDataSource ID="odsLogo" runat="server" SelectMethod="GetCompanyLogoArray" TypeName="FintrakBanking.ReportObjects.ReportingObjects.CompanyLogo">
                <SelectParameters>
                    <asp:Parameter DefaultValue="1" Name="companyId" Type="Int32" />
                </SelectParameters>
             </asp:ObjectDataSource>
        </div>

         <asp:Label ID="endDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="startDate" runat="server" Visible="false" ></asp:Label>
        <asp:Label ID="companyId" runat="server"  Visible="false" ></asp:Label>
        <asp:Label ID="approvalStatus" runat="server"  Visible="false" ></asp:Label>
      <asp:Label ID="operationId" runat="server"  Visible="false" ></asp:Label>
        
    </form>
</body></html>
