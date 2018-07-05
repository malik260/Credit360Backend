<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomeFacilityRepayment.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.CustomeFacilityRepayment" %>
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
            <LocalReport ReportPath="Reports\Report\CustomFacilityRepayment.rdlc" >
                
                <datasources>
                          <rsweb:ReportDataSource DataSourceId="ObjectDataSourceCustomFacilityRepayment" Name="facilityRepayment" />
                      <rsweb:ReportDataSource DataSourceId="odsLogo" Name="logo" />
                      </datasources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSourceCustomFacilityRepayment" runat="server" SelectMethod="CustomeFacilityRepayment" TypeName="FintrakBanking.ReportObjects.ReportingObjects.FinanceRepotObject">
            <SelectParameters> 
                <asp:ControlParameter ControlID="endDate" Name="endDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="startDate" Name="startDate" PropertyName="Text" Type="DateTime" />
                <asp:ControlParameter ControlID="companyId" Name="companyId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="valueCode" Name="valueCode" PropertyName="Text" Type="String" />
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
        <asp:Label ID="valueCode" runat="server"  Visible="false" ></asp:Label>
     
        
    </form>
</body>
</html>
