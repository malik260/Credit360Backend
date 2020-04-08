<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OfferLetterLMSR.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration.OfferLetterLMSR" %>
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
        <rsweb:ReportViewer ID="offerLetterReport" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="751px">
            <LocalReport ReportPath="Reports\Credit\OfferLetterGeneration\OfferLetterLMSR.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="OfferLetterDetails" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterDetails" Name="OfferLetterLoanDetail" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterConditionPrecedent" Name="OfferLetterConditionPrecident" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterConditionSubsequent" Name="OfferLetterConditionSubsequent" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterFee" Name="OfferLetterFee" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterSignatory" Name="OfferLetterSignatory" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterCollateral" Name="OfferLetterCollateral" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterBorrowerDetail" Name="OfferLetterBorrowerDetail" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterTransactionDynamics" Name="OfferLetterConditionDynamics" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterTransactionDynamics" Name="OfferLetterTransactionDynamics" />

                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>

        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GenerateOfferLetter" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
        <asp:ObjectDataSource ID="odsOfferLetter" runat="server" SelectMethod="GenerateOfferLetter" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
        <asp:ObjectDataSource ID="odsOfferLetterDetails" runat="server" SelectMethod="GetLoanApplicationDetail" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <asp:ObjectDataSource ID="odsOfferLetterConditionPrecedent" runat="server" SelectMethod="GetLoanApplicationConditionPrecident" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
       <asp:ObjectDataSource ID="odsOfferLetterConditionSubsequent" runat="server" SelectMethod="GetLoanApplicationConditionSubsequent" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>

         <asp:ObjectDataSource ID="odsOfferLetterFee" runat="server" SelectMethod="GetLoanApplicationFee" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>

                 <asp:ObjectDataSource ID="odsOfferLetterSignatory" runat="server" SelectMethod="GetLoanApplicationSignatory" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
 
                 <asp:ObjectDataSource ID="odsOfferLetterCollateral" runat="server" SelectMethod="GetLoanCollateral" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
           <asp:ObjectDataSource ID="odsOfferLetterBorrowerDetail" runat="server" SelectMethod="GenerateOfferLetter" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <asp:ObjectDataSource ID="odsOfferLetterTransactionDynamics" runat="server" SelectMethod="Lmsr_ConditionDynamics" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR" OldValuesParameterFormatString="original_{0}">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
