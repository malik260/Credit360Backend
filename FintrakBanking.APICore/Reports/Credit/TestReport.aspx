<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestReport.aspx.cs" Inherits="FintrakBanking.APICore.Reports.Credit.TestReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

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
            <rsweb:ReportViewer ID="rvBranchReport" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="651px">
                <LocalReport ReportPath="Reports\Credit\TestReport.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="Branch" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
        </div>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetBranch" TypeName="FintrakBanking.ReportObjects.BranchInfo">
            <SelectParameters>
                <asp:QueryStringParameter DefaultValue="0" Name="id" QueryStringField="id" Type="Int16" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </form>
</body>
</html>
