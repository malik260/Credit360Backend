<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisburstedLoan.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.DisburstedLoan" %>

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

            
        <rsweb:ReportViewer ID="ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="485px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" 
            Width="100%" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
           <LocalReport ReportPath="Reports\Report\DisburstedLoans.rdlc" >
                <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="DataSet1" />
                    </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetDisburstLoans" TypeName="FintrakBanking.ReportObjects.LoanReportObjects">
            <SelectParameters>
                <asp:ControlParameter ControlID="hdf_startDate" Name="startDate" PropertyName="Value" Type="DateTime" />
                <asp:ControlParameter ControlID="hdf_endDdate" Name="endDdate" PropertyName="Value" Type="DateTime" />
                <asp:ControlParameter ControlID="hdf_companyId" DefaultValue="" Name="companyId" PropertyName="Value" Type="Int32" />
          
            </SelectParameters>
        </asp:ObjectDataSource>

    
        </div>
        <asp:HiddenField ID="hdf_startDate" runat="server" />
        <asp:HiddenField ID="hdf_endDdate" runat="server" />
        <asp:HiddenField ID="hdf_companyId" runat="server" />
    </form>
 
</body>
</html>
