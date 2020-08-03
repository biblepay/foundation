<%@ Page Title="Deposit" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Deposit.aspx.cs" Inherits="Saved.Deposit" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>Deposit</h3>

    <hr />

    
    <table>
    <tr><td>    Deposit Address:&nbsp;<td>    <asp:TextBox ID="txtDepositAddress" width="500px" readonly runat="server" ></asp:TextBox></tr>
    <tr><td>    Balance:          <td>    <asp:TextBox ID="txtBalance" width="200px" readonly runat="server" ></asp:TextBox></tr>
    </table>
    <asp:Button ID="btnDepositReport" runat="server" Text="Run Deposit Report" OnClick="btnDepositReport_Click" />

    <br />
    <hr />

    <h3>Withdrawal</h3>
    <br />
    Note:  To make a withdrawal, paste your biblepay address in the Withdrawal Address box below, type in the amount and click Withdraw BBP.

    <hr />

    <br />
    <table>
    <tr><td>Withdrawal Address: &nbsp;<td>    <asp:TextBox ID="txtWithdrawalAddress" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td>Withdrawal Amount: &nbsp;<td>    <asp:TextBox ID="txtAmount" width="200px" runat="server" ></asp:TextBox></tr>
    </table>
    <asp:Button ID="btnWithdraw" runat="server" Text="Withdraw BBP" OnClick="btnWithdraw_Click" />

</asp:Content>
