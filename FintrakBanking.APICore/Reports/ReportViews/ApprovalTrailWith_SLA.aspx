<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApprovalTrailWith_SLA.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.ApprovalTrailWith_SLA" %>
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
           <LocalReport ReportPath="Reports\Report\ApprovalTrailWithSLA.rdlc" >
                <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="WorkFlowSLA" />
                    </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
     
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="TrackWorkFlow" TypeName="FintrakBanking.ReportObjects.WorkFlowDesign">
            <SelectParameters>
                <asp:ControlParameter ControlID="operationId" DefaultValue="" Name="operationId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="companyId" DefaultValue="" Name="companyId" PropertyName="Text" Type="Int32" />
                <asp:ControlParameter ControlID="targetId" DefaultValue="" Name="targetId" PropertyName="Text" Type="Int32" />
          
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
        <asp:Label Visible="false" ID="operationId" runat="server" ></asp:Label>
        <asp:Label Visible="false" ID="targetId" runat="server" ></asp:Label>
        <asp:Label Visible="false" ID="companyId" runat="server" ></asp:Label>
         <asp:Label Visible="false" ID="staffId" runat="server" ></asp:Label>
    </form>
</body>
</html>
