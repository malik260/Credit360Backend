<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OutPutTemplate.aspx.cs" Inherits="FintrakBanking.APICore.Reports.ReportViews.OutPutTemplate" %>

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
        
        <LocalReport ReportPath="Reports\Report\OutputDocument.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odGetFee" Name="Fee " />
                    <rsweb:ReportDataSource DataSourceId="odGetMonthActivitySignature" Name="MonthActivitySign" />
                    <rsweb:ReportDataSource DataSourceId="odGetApplicationApproval" Name="Approval" />
                    <rsweb:ReportDataSource DataSourceId="odGetChecklist" Name="Checklist" />
                    <rsweb:ReportDataSource DataSourceId="odGetCollateral" Name="Collateral" />
                    <rsweb:ReportDataSource DataSourceId="odGetConcurrences" Name="Concurrences" />
                    <rsweb:ReportDataSource DataSourceId="odGetCustomerFacilities" Name="CustomerFacilities" />
                        <rsweb:ReportDataSource DataSourceId="odGetCustomerInformation" Name="CustomerInformation" />
                        <rsweb:ReportDataSource DataSourceId="odGetMonthsActivity" Name="MonthsActivity" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
     
     
        </div>
        <asp:ObjectDataSource ID="odGetFee" runat="server" SelectMethod="GetFee" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
             <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetMonthActivitySignature" runat="server" SelectMethod="MonthActivitySignature" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetApplicationApproval" runat="server" SelectMethod="GetApplicationApproval" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetChecklist" runat="server" SelectMethod="GetChecklist" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetCollateral" runat="server" SelectMethod="GetCollateral" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetConcurrences" runat="server" SelectMethod="GetConcurrences" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" OldValuesParameterFormatString="original_{0}" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetMonthsActivity" runat="server" SelectMethod="GetMonthsActivity" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetCustomerFacilities" runat="server" SelectMethod="GetCustomerFacilities" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
        <asp:ObjectDataSource ID="odGetCustomerInformation" runat="server" SelectMethod="GetCustomerInformation" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo" OldValuesParameterFormatString="original_{0}" >
            <SelectParameters>
                 <asp:QueryStringParameter Name="loanApplicationId" QueryStringField="loanApplicationId" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </form>


</body>
</html>
