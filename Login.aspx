<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Saved.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h3>Log In</h3>
    <br />
    <br />

    <table class='standard' style="border:1px solid black;">
        <tr>
            <td cellspacing="5px">Username: </td><td>
                <asp:TextBox width="400px" ID="txtUserName" readonly runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td>2FA Pin: </td><td>
                <asp:TextBox ID="txtPin" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td colspan="3">
                <asp:Button ID="btnLogin" runat="server" Text="Log In" OnClick="btnLogin_Click" />
                &nbsp;
                <!--
                <asp:Button ID="btnRegister" runat="server" Text="Register" OnClick="btnRegister_Click" />
                &nbsp;
                <asp:Button ID="BtnResetPassword" runat="server" Text="Reset Password" OnClick="btnResetPassword_Click" />
                -->
            </td>

            <td></td>
        </tr>

    </table>
  
  
</asp:Content>
