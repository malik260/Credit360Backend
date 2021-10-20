<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FORM3800B_LMS.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration.FORM3800B_LMS" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>


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
            <rsweb:ReportViewer ID="offerLetterReport" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="1000px">
                <LocalReport ReportPath="Reports\Credit\OfferLetterGeneration\FORM3800B_LMS.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="odsfacility" Name="Facility" />
                        <rsweb:ReportDataSource DataSourceId="odsSecurityDetails" Name="SecurityDetails" />
                        <rsweb:ReportDataSource DataSourceId="odsCustomerDetails" Name="CustomerDetails" />
                        <rsweb:ReportDataSource DataSourceId="odsFeeDetails" Name="FeeDetails" />

                        <rsweb:ReportDataSource DataSourceId="odsInteralConditionsPrecedent" Name="InternalConditionPrecedent" />
                        <rsweb:ReportDataSource DataSourceId="odsTransactionDynamics" Name="TransactionDynamics" />
                         <rsweb:ReportDataSource DataSourceId="odsInternalConditionSubsquent" Name="InternalConditionSubsequent" />
                        <rsweb:ReportDataSource DataSourceId="odsComments" Name="Comments" />
                        <rsweb:ReportDataSource DataSourceId="odsMonitoringTrggers" Name="MonitoringTrggers" />

                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>

            <asp:ObjectDataSource ID="odsfacility" runat="server" SelectMethod="Lmsr_LoanDetail" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>

            <asp:ObjectDataSource ID="odsSecurityDetails" runat="server" SelectMethod="Lmsr_Collateral" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
            <asp:ObjectDataSource ID="odsCustomerDetails" runat="server" SelectMethod="Lmsr_Customer" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
            <asp:ObjectDataSource ID="odsFeeDetails" runat="server" SelectMethod="GetLoanApplicationFee" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>

         <asp:ObjectDataSource ID="odsInteralConditionsPrecedent" runat="server" SelectMethod="Lmsr_ConditionPrecedents" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
            <asp:ObjectDataSource ID="odsTransactionDynamics" runat="server" SelectMethod="Lmsr_ConditionDynamics" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>


              <asp:ObjectDataSource ID="odsInternalConditionSubsquent" runat="server" SelectMethod="Lmsr_ConditionSubsequents" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
          <%-- <asp:ObjectDataSource ID="odsExternalConditionSubsquent" runat="server" SelectMethod="Lmsr_Internal_ConditionSubsequents" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>--%>
            <asp:ObjectDataSource ID="odsComments" runat="server" SelectMethod="Lmsr_LoanComments" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
            <asp:ObjectDataSource ID="odsMonitoringTrggers" runat="server" SelectMethod="Lmsr_loanMonitoringTriggers" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfoLMSR">
                <SelectParameters>
                    <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
                </SelectParameters>
            </asp:ObjectDataSource>
            
        </div>
    </form>
    <asp:Label ID="applicationRefNumber" runat="server" Visible="false"></asp:Label>
</body>
</html>
