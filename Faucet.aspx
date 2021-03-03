<%@ Page Title="Faucet" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Faucet.aspx.cs" Inherits="Saved.Faucet" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>The BIBLEPAY Faucet</h3>

    <hr />

    
    <table>
    <tr><td>    BBP Receive Address:&nbsp;<td>    <asp:TextBox ID="txtAddress" width="500px" runat="server" ></asp:TextBox></tr>

    <tr><td>    Your E-Mail Address:      <td>    <asp:TextBox ID="txtEmail" width="500px" runat="server" ></asp:TextBox></tr>
    </table>
    <br />
    <br />

    <font color="red">By Clicking I AGREE, I agree to receive BIBLEPAY e-mail updates, and tweets about BIBLEPAY.</font>
    <br />
    <asp:Button ID="btnGetMoney" runat="server" Text="I Agree to Receive BIBLEPAY E-Mails / Please send me some BIBLEPAY" OnClick="btnGetMoney_Click" />

    <br />

</asp:Content>
