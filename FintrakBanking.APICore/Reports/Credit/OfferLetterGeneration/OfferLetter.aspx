<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OfferLetter.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration.OfferLetter" %>

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
        <rsweb:ReportViewer ID="offerLetterReport" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="685px" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="751px">
            <LocalReport ReportPath="Reports\Credit\OfferLetterGeneration\OfferLetter.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetter" Name="OfferLetterDetails" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterDetails" Name="OfferLetterLoanDetail" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterConditionPrecedent" Name="OfferLetterConditionPrecident" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterConditionSubsequent" Name="OfferLetterConditionSubsequent" />
                    <rsweb:ReportDataSource DataSourceId="odsOfferLetterFee" Name="OfferLetterFee" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    
        <asp:ObjectDataSource ID="odsOfferLetter" runat="server" SelectMethod="GenerateOfferLetter" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
        <asp:ObjectDataSource ID="odsOfferLetterDetails" runat="server" SelectMethod="GetLoanApplicationDetail" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <asp:ObjectDataSource ID="odsOfferLetterConditionPrecedent" runat="server" SelectMethod="GetLoanApplicationConditionPrecident" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
       <asp:ObjectDataSource ID="odsOfferLetterConditionSubsequent" runat="server" SelectMethod="GetLoanApplicationConditionSubsequent" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>

         <asp:ObjectDataSource ID="odsOfferLetterFee" runat="server" SelectMethod="GetLoanApplicationFee" TypeName="FintrakBanking.ReportObjects.Credit.OfferLetterInfo">
            <SelectParameters>
                <asp:QueryStringParameter Name="applicationRefNumber" QueryStringField="applicationRefNumber" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
